# Packages the mod for release into ./releases/:
#   releases/PortugueseColonists/                 ready to copy into RimWorld\Mods\
#   releases/PortugueseColonists-<version>.zip    same folder, zipped (version from About.xml)
#
# Steps: build the DLL -> check all XML parses -> copy the mod files (+ Brazilian copy of the
# Portuguese translation) -> zip.
#
# Usage:  powershell -ExecutionPolicy Bypass -File Source\package.ps1 [-GamePath "C:\Games\RimWorld"] [-SkipBuild]

param(
    [string]$GamePath = "C:\Games\RimWorld",
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"
$modRoot = Split-Path -Parent $PSScriptRoot
$modName = "PortugueseColonists"
$releaseDir = Join-Path $modRoot "releases"
$outDir = Join-Path $releaseDir $modName
# Everything the game needs. Source/ and releases/ are deliberately left out.
$include = "About", "Assemblies", "Defs", "Languages", "Names"

# 1. Build the DLL
if ($SkipBuild) {
    Write-Host "Skipping build, using the existing DLL."
} else {
    & (Join-Path $PSScriptRoot "build.ps1") -GamePath $GamePath
}
if (-not (Test-Path (Join-Path $modRoot "Assemblies\$modName.dll"))) { throw "Assemblies\$modName.dll is missing. Run without -SkipBuild." }

# 2. Check every XML file parses, so a typo is caught here instead of in game
$xmlFiles = Get-ChildItem ($include | ForEach-Object { Join-Path $modRoot $_ }) -Recurse -Filter *.xml
foreach ($file in $xmlFiles) {
    try {
        [xml](Get-Content -Raw -Encoding UTF8 $file.FullName) | Out-Null
    } catch {
        throw "Invalid XML in $($file.FullName.Substring($modRoot.Length + 1)): $($_.Exception.Message)"
    }
}
Write-Host "Checked $($xmlFiles.Count) XML files."

# 3. Copy the mod files into a clean release folder
if (Test-Path $outDir) { Remove-Item $outDir -Recurse -Force }
New-Item -ItemType Directory -Force $outDir | Out-Null
foreach ($folder in $include) {
    Copy-Item (Join-Path $modRoot $folder) -Destination $outDir -Recurse
}

# Brazilian Portuguese players get a copy of the European Portuguese translation (better than English).
# Only the European folder exists in the project, so there is a single translation to maintain.
# Folder names are built from char codes because PowerShell 5.1 misreads accents in script files.
$e = [char]0x00EA
$ptLang = Join-Path $outDir "Languages\Portuguese (Portugu${e}s)"
$brLang = Join-Path $outDir "Languages\PortugueseBrazilian (Portugu${e}s Brasileiro)"
if (Test-Path $ptLang) {
    if (Test-Path $brLang) { Remove-Item $brLang -Recurse -Force }
    Copy-Item $ptLang -Destination $brLang -Recurse
    Write-Host "Copied the Portuguese translation to Brazilian Portuguese."
}

# 4. Zip it, with the mod folder at the root of the zip
$version = ([xml](Get-Content -Raw -Encoding UTF8 (Join-Path $modRoot "About\About.xml"))).ModMetaData.modVersion
$zipPath = Join-Path $releaseDir "$modName-$version.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
$zip = [IO.Compression.ZipFile]::Open($zipPath, [IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($file in Get-ChildItem $outDir -Recurse -File) {
        # Forward slashes, so the zip also extracts correctly outside Windows
        $entryName = "$modName/" + $file.FullName.Substring($outDir.Length + 1).Replace('\', '/')
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $file.FullName, $entryName, [IO.Compression.CompressionLevel]::Optimal) | Out-Null
    }
} finally {
    $zip.Dispose()
}

Write-Host ""
Write-Host "Done:"
Write-Host "  Folder: releases\$modName\   (copy into $GamePath\Mods\)"
Write-Host "  Zip:    releases\$modName-$version.zip"
