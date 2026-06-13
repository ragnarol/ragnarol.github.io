# Fading Suns RPG - Asset Downloader (Windows PowerShell)
# Run from the repo root:
#   .\game\tools\download_assets.ps1

$GameRoot = Split-Path -Parent $PSScriptRoot
$TmpDir   = Join-Path $PSScriptRoot "_tmp"

$Destinations = @{
    tiles_city       = Join-Path $GameRoot "Assets\Sprites\Tiles\City"
    tiles_palace     = Join-Path $GameRoot "Assets\Sprites\Tiles\Palace"
    tiles_spaceport  = Join-Path $GameRoot "Assets\Sprites\Tiles\Spaceport"
    tiles_wilderness = Join-Path $GameRoot "Assets\Sprites\Tiles\Wilderness"
    tiles_generic    = Join-Path $GameRoot "Assets\Sprites\Tiles\Generic"
    characters       = Join-Path $GameRoot "Assets\Sprites\Characters"
    ui               = Join-Path $GameRoot "Assets\Sprites\UI"
    music            = Join-Path $GameRoot "Assets\Audio\Music"
    sfx              = Join-Path $GameRoot "Assets\Audio\SFX"
}

foreach ($d in $Destinations.Values) {
    New-Item -ItemType Directory -Force -Path $d | Out-Null
}
New-Item -ItemType Directory -Force -Path $TmpDir | Out-Null

$Assets = @(
    @{ Name="Kenney-IsometricMiniatureDungeon"; Dest="tiles_palace";
       Url="https://kenney.nl/media/pages/assets/isometric-miniature-dungeon/0a5d1b7982-1699889325/kenney_isometric-miniature-dungeon.zip" },
    @{ Name="Kenney-IsometricBlocks";           Dest="tiles_generic";
       Url="https://kenney.nl/media/pages/assets/isometric-blocks/6f48e56d77-1699889294/kenney_isometric-blocks.zip" },
    @{ Name="Kenney-IsometricLandscape";        Dest="tiles_wilderness";
       Url="https://kenney.nl/media/pages/assets/isometric-landscape/dbbd4ab9c6-1699889313/kenney_isometric-landscape.zip" },
    @{ Name="Kenney-IsometricTilesBuildings";   Dest="tiles_city";
       Url="https://kenney.nl/media/pages/assets/isometric-tiles-buildings/bcdb879649-1721997218/kenney_isometric-tiles-buildings.zip" },
    @{ Name="Kenney-IsometricMiniaturePrototype"; Dest="tiles_spaceport";
       Url="https://kenney.nl/media/pages/assets/isometric-miniature-prototype/ee1b78acaf-1699889319/kenney_isometric-miniature-prototype.zip" },
    @{ Name="Kenney-UIPackRPGExpansion";        Dest="ui";
       Url="https://kenney.nl/media/pages/assets/ui-pack-rpg-expansion/b1e8bea25c-1699889358/kenney_ui-pack-rpg-expansion.zip" },
    @{ Name="Kenney-TinyDungeon";               Dest="characters";
       Url="https://kenney.nl/media/pages/assets/tiny-dungeon/a6c1da6e95-1699889355/kenney_tiny-dungeon.zip" },
    @{ Name="Kenney-RPGAudio";                  Dest="sfx";
       Url="https://kenney.nl/media/pages/assets/rpg-audio/d9c5b5a5d2-1699889345/kenney_rpg-audio.zip" },
    @{ Name="Kenney-InterfaceSounds";           Dest="sfx";
       Url="https://kenney.nl/media/pages/assets/interface-sounds/81b30baf4a-1699889307/kenney_interface-sounds.zip" },
    @{ Name="Kenney-SciFiSounds";               Dest="sfx";
       Url="https://kenney.nl/media/pages/assets/sci-fi-sounds/7c1b5ceedd-1699889348/kenney_sci-fi-sounds.zip" }
)

$ImageExt = @(".png", ".jpg")
$AudioExt = @(".ogg", ".wav", ".mp3", ".aiff")
$AllExt   = $ImageExt + $AudioExt

$Failed    = @()
$Succeeded = @()

$Sep = "-" * 60
Write-Host $Sep
Write-Host "Fading Suns RPG - Asset Downloader (PowerShell)"
Write-Host $Sep

foreach ($Asset in $Assets) {
    $ZipPath = Join-Path $TmpDir ($Asset.Name + ".zip")
    $DestDir = $Destinations[$Asset.Dest]

    Write-Host ""
    Write-Host ("[CC0] " + $Asset.Name)
    Write-Host ("  -> " + $DestDir.Replace($GameRoot + "\", ""))

    # Download
    if (-not (Test-Path $ZipPath)) {
        try {
            Write-Host "  Downloading..."
            $wc = New-Object System.Net.WebClient
            $wc.Headers.Add("User-Agent", "FadingSunsAssetDownloader/1.0")
            $wc.DownloadFile($Asset.Url, $ZipPath)
            Write-Host "  Downloaded OK"
        }
        catch {
            Write-Host ("  FAILED: " + $_)
            Write-Host "  -> Visit https://kenney.nl to download manually."
            $Failed += $Asset.Name
            continue
        }
    }
    else {
        Write-Host "  (cached)"
    }

    # Extract image/audio files only, flattened into destination folder
    try {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $zip = [System.IO.Compression.ZipFile]::OpenRead($ZipPath)
        foreach ($entry in $zip.Entries) {
            $ext = [System.IO.Path]::GetExtension($entry.Name).ToLower()
            if ($AllExt -notcontains $ext) { continue }
            if ($entry.Name -eq "") { continue }
            $target = Join-Path $DestDir $entry.Name
            if (Test-Path $target) { continue }
            [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $target, $false)
        }
        $zip.Dispose()
        Write-Host "  Extracted OK"
        $Succeeded += $Asset.Name
    }
    catch {
        Write-Host ("  Extract error: " + $_)
        $Failed += $Asset.Name
    }
}

Write-Host ""
Write-Host $Sep
Write-Host ("Done. " + $Succeeded.Count + " succeeded, " + $Failed.Count + " failed.")

if ($Failed.Count -gt 0) {
    Write-Host ("Failed: " + ($Failed -join ", "))
    Write-Host "Download these manually from: https://kenney.nl/assets"
}
