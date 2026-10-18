[CmdletBinding()]
param(
    [string]$Configuration = 'Debug',
    [string]$MSBuildPath = '',
    [string]$SvcUtilPath = 'C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\SvcUtil.exe'
)
$ErrorActionPreference = 'Stop'
if (-not $MSBuildPath) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    $MSBuildPath = & $vswhere -latest -products Microsoft.VisualStudio.Product.Community Microsoft.VisualStudio.Product.Professional Microsoft.VisualStudio.Product.Enterprise Microsoft.VisualStudio.Product.BuildTools -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
}
if (-not $MSBuildPath -or -not (Test-Path -LiteralPath $MSBuildPath)) { throw 'MSBuild was not found. Pass -MSBuildPath.' }
if (-not (Test-Path -LiteralPath $SvcUtilPath)) { throw 'SvcUtil was not found. Install the .NET Framework SDK or pass -SvcUtilPath.' }
$contractsDirectory = Join-Path (Split-Path $PSScriptRoot -Parent) 'DominoAllFives.Contracts'
$contractsProject = Join-Path $contractsDirectory 'DominoAllFives.Contracts.csproj'
& $MSBuildPath $contractsProject /t:Build "/p:Configuration=$Configuration" /v:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw 'Building the server contracts failed.' }
$contractsAssembly = Join-Path $contractsDirectory "bin\$Configuration\DominoAllFives.Contracts.dll"
$metadataDirectory = Join-Path $PSScriptRoot 'Metadata'
New-Item -ItemType Directory -Force -Path $metadataDirectory | Out-Null
# Export only service contracts and their reachable data types; no live MEX endpoint is needed.
& $SvcUtilPath $contractsAssembly /t:metadata "/directory:$metadataDirectory" /nologo
if ($LASTEXITCODE -ne 0) { throw 'Exporting WCF metadata failed.' }
$metadataFiles = Get-ChildItem -LiteralPath $metadataDirectory -File | Where-Object { $_.Extension -in '.wsdl', '.xsd' } | Select-Object -ExpandProperty FullName
# Deliberately omit /reference: the client owns generated transport types and never reuses server types.
& $SvcUtilPath @metadataFiles /language:C# '/namespace:*,DominoAllFives.Client.Proxy' /serializer:DataContractSerializer /noConfig "/out:$(Join-Path $PSScriptRoot 'Reference.cs')" /nologo
if ($LASTEXITCODE -ne 0) { throw 'Generating the client proxy failed.' }
