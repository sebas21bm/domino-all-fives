param(
    [string]$SvcUtilPath = (Join-Path ${env:ProgramFiles(x86)} 'Microsoft SDKs/Windows/v10.0A/bin/NETFX 4.8 Tools/SvcUtil.exe')
)

$ErrorActionPreference = 'Stop'
if (!(Test-Path -LiteralPath $SvcUtilPath)) {
    throw 'Specify the path to the .NET Framework 4.8 SvcUtil.exe with -SvcUtilPath.'
}

$metadata = @(Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'Metadata') -File |
    Where-Object Extension -In '.wsdl','.xsd' | ForEach-Object FullName)
if ($metadata.Count -eq 0) {
    throw 'The exported match service metadata is missing.'
}

$output = Join-Path $PSScriptRoot 'Reference.cs'
& $SvcUtilPath $metadata /language:C# '/namespace:*,MatchService' `
    '/collectionType:System.Collections.Generic.List`1' /noConfig /syncOnly ('/out:' + $output)
if ($LASTEXITCODE -ne 0) {
    throw ('SvcUtil failed with exit code ' + $LASTEXITCODE)
}
