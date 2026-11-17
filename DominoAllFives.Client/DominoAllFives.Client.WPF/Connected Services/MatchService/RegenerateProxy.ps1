param(
    [string]$SvcUtilPath,
    [string]$ContractsAssembly
)

$ErrorActionPreference = 'Stop'
if ($PSVersionTable.PSEdition -eq 'Core') {
    throw 'Run this script with Windows PowerShell (.NET Framework).'
}

Add-Type -AssemblyName System.ServiceModel, System.Runtime.Serialization, System.Web.Services
$metadataPath = Join-Path $PSScriptRoot 'Metadata'
if ($ContractsAssembly) {
    $assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path $ContractsAssembly).Path)
    $contract = [System.ServiceModel.Description.ContractDescription]::GetContract(
        $assembly.GetType('DominoAllFives.Contracts.Services.IMatchService', $true))
    $exporter = New-Object System.ServiceModel.Description.WsdlExporter
    $exporter.ExportContract($contract)
    if ($exporter.Errors | Where-Object { !$_.IsWarning }) {
        throw 'The current match contract could not be exported.'
    }
    $exporter.GeneratedWsdlDocuments[0].Write((Join-Path $metadataPath 'Match0.wsdl'))
    # Keep schema filenames stable by matching the namespace of each stored schema.
    foreach ($schema in $exporter.GeneratedXmlSchemas.Schemas()) {
        $target = $null
        foreach ($file in (Get-ChildItem -LiteralPath $metadataPath -Filter '*.xsd')) {
            [xml]$stored = Get-Content -LiteralPath $file.FullName -Raw
            if ($stored.schema.targetNamespace -eq $schema.TargetNamespace) {
                $target = $file.FullName
                break
            }
        }
        if (!$target) { throw ('No metadata filename for ' + $schema.TargetNamespace) }
        $writer = [Xml.XmlWriter]::Create($target, [Xml.XmlWriterSettings]@{ Indent = $true })
        try { $schema.Write($writer) } finally { $writer.Dispose() }
    }
}

$metadata = @(Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'Metadata') -File |
    Where-Object Extension -In '.wsdl','.xsd' | ForEach-Object FullName)
if ($metadata.Count -eq 0) {
    throw 'The exported match service metadata is missing.'
}

$output = Join-Path $PSScriptRoot 'Reference.cs'
if ($SvcUtilPath) {
    & $SvcUtilPath $metadata /language:C# '/namespace:*,MatchService' `
        '/collectionType:System.Collections.Generic.List`1' /noConfig /syncOnly ('/out:' + $output)
    if ($LASTEXITCODE -ne 0) { throw ('SvcUtil failed with exit code ' + $LASTEXITCODE) }
    return
}

# The framework code generator is available even when the SDK SvcUtil is absent.
$set = New-Object System.ServiceModel.Description.MetadataSet
foreach ($file in $metadata) {
    if ([IO.Path]::GetExtension($file) -eq '.wsdl') {
        $document = [System.Web.Services.Description.ServiceDescription]::Read($file)
        $section = [System.ServiceModel.Description.MetadataSection]::CreateFromServiceDescription(
            $document)
    }
    else {
        $reader = [Xml.XmlReader]::Create($file)
        try { $schema = [Xml.Schema.XmlSchema]::Read($reader, $null) }
        finally { $reader.Dispose() }
        $section = [System.ServiceModel.Description.MetadataSection]::CreateFromSchema($schema)
    }
    $set.MetadataSections.Add($section)
}
$unit = New-Object System.CodeDom.CodeCompileUnit
$provider = New-Object Microsoft.CSharp.CSharpCodeProvider
$options = New-Object System.Runtime.Serialization.ImportOptions
$options.CodeProvider = $provider
$options.Namespaces.Add('*', 'MatchService')
$options.ReferencedCollectionTypes.Add([System.Collections.Generic.List``1])
$dataImporter = New-Object System.Runtime.Serialization.XsdDataContractImporter($unit)
$dataImporter.Options = $options
$importer = New-Object System.ServiceModel.Description.WsdlImporter($set)
$importer.State.Add([System.Runtime.Serialization.XsdDataContractImporter], $dataImporter)
$generator = New-Object System.ServiceModel.Description.ServiceContractGenerator($unit)
$generator.NamespaceMappings.Add('*', 'MatchService')
$generator.Options = [System.ServiceModel.Description.ServiceContractGenerationOptions]'ClientClass, ChannelInterface'
foreach ($contract in $importer.ImportAllContracts()) {
    $null = $generator.GenerateServiceContractType($contract)
}
if (($importer.Errors + $generator.Errors) | Where-Object { !$_.IsWarning }) {
    throw 'The stored match metadata could not generate a proxy.'
}
$writer = [IO.StreamWriter]::new($output, $false, [Text.UTF8Encoding]::new($false))
$format = New-Object System.CodeDom.Compiler.CodeGeneratorOptions
$format.BracingStyle = 'C'
try { $provider.GenerateCodeFromCompileUnit($unit, $writer, $format) }
finally { $writer.Dispose(); $provider.Dispose() }
$source = [IO.File]::ReadAllText($output)
$source = [regex]::Replace($source, '(?m)[\t ]+(?=\r?$)', '')
[IO.File]::WriteAllText($output, $source, [Text.UTF8Encoding]::new($false))
