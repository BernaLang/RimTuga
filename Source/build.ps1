# Builds Assemblies/PortugueseColonists.dll using the C# compiler that ships with Windows.
# Only needed when PortugueseColonists.cs changes - adding names or figures does NOT require a rebuild.
#
# Usage:  powershell -ExecutionPolicy Bypass -File Source\build.ps1 [-GamePath "C:\Games\RimWorld"]

param(
    [string]$GamePath = "C:\Games\RimWorld"
)

$ErrorActionPreference = "Stop"
$modRoot = Split-Path -Parent $PSScriptRoot
$managed = Join-Path $GamePath "RimWorldWin64_Data\Managed"
$csc = Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$outDir = Join-Path $modRoot "Assemblies"

if (-not (Test-Path $managed)) { throw "RimWorld not found at '$GamePath'. Pass -GamePath." }
New-Item -ItemType Directory -Force $outDir | Out-Null

# Compile against the game's own runtime libraries (not Windows' .NET), so the DLL matches what RimWorld runs.
$refs = "mscorlib.dll", "System.dll", "System.Core.dll", "netstandard.dll", "Assembly-CSharp.dll", "UnityEngine.CoreModule.dll" |
    ForEach-Object { "/reference:" + (Join-Path $managed $_) }

& $csc /nologo /noconfig /nostdlib+ /target:library /optimize+ /warn:4 `
    "/out:$(Join-Path $outDir 'PortugueseColonists.dll')" `
    $refs `
    (Join-Path $PSScriptRoot "PortugueseColonists.cs")

if ($LASTEXITCODE -ne 0) { throw "Build failed." }
Write-Host "Built Assemblies\PortugueseColonists.dll"
