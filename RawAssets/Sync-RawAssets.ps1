param()

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$archiveRoot = $PSScriptRoot
$records = [System.Collections.Generic.List[object]]::new()

function Copy-ArchivedFile([System.IO.FileInfo]$File, [string]$RelativeDestination)
{
    $destination = Join-Path $archiveRoot $RelativeDestination
    New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
    $sourceHash = (Get-FileHash -LiteralPath $File.FullName -Algorithm SHA256).Hash
    if (Test-Path -LiteralPath $destination)
    {
        $destinationHash = (Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash
        if ($sourceHash -ne $destinationHash)
        {
            Copy-Item -LiteralPath $File.FullName -Destination $destination -Force
        }
    }
    else
    {
        Copy-Item -LiteralPath $File.FullName -Destination $destination
    }
    if ((Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash -ne $sourceHash)
    {
        throw "Archive verification failed: $RelativeDestination"
    }
    $records.Add([pscustomobject]@{
        Source = [System.IO.Path]::GetRelativePath($projectRoot, $File.FullName).Replace('\', '/')
        Archive = $RelativeDestination.Replace('\', '/')
        Bytes = $File.Length
        SHA256 = $sourceHash
    })
}

function Copy-AssetTree([string]$Source, [string]$Destination)
{
    $sourceRoot = Join-Path $projectRoot $Source
    if (!(Test-Path -LiteralPath $sourceRoot)) { return }
    foreach ($file in Get-ChildItem -LiteralPath $sourceRoot -File -Recurse | Sort-Object FullName)
    {
        if ($file.Name -eq '.gitkeep') { continue }
        $relative = [System.IO.Path]::GetRelativePath($sourceRoot, $file.FullName)
        Copy-ArchivedFile $file (Join-Path $Destination $relative)
    }
}

Copy-AssetTree 'Assets/_Game/Art/Sprites/Player' '01_Player'
Copy-AssetTree 'Assets/_Game/Art/Sprites/Enemies' '02_Enemies'
Copy-AssetTree 'Assets/_Game/Art/Sprites/Bosses' '03_Bosses'
foreach ($file in Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Assets/_Game/Art/Sprites/Bullets') -File)
{
    $group = if ($file.Name.StartsWith('bullet_player_')) { 'Player' } else { 'Enemies' }
    Copy-ArchivedFile $file (Join-Path "04_Bullets/$group" $file.Name)
}
Copy-AssetTree 'Assets/_Game/Art/Sprites/Skills' '05_Skills'
Copy-AssetTree 'Assets/_Game/Art/Sprites/Backgrounds' '06_Backgrounds'
Copy-AssetTree 'Assets/_Game/Art/Sprites/UI' '07_UI'
Copy-AssetTree 'Assets/_Game/Art/Sprites/_Placeholder' '08_Placeholders'
Copy-AssetTree 'Assets/_Game/Art/Materials' '09_Effects/Materials'
Copy-AssetTree 'Assets/_Game/Art/Shaders' '09_Effects/Shaders'
Copy-AssetTree 'Assets/_Game/Audio' '09_Effects/Audio'
foreach ($folder in @('Prefabs', 'Data', 'Scenes'))
{
    Copy-AssetTree "Assets/_Game/$folder" "10_Unity/$folder"
}
Copy-AssetTree 'Assets/Settings' '10_Unity/Settings'

$imageExtensions = @('.png', '.jpg', '.jpeg', '.webp', '.gif', '.bmp', '.tga', '.psd', '.svg', '.exr', '.hdr', '.tif', '.tiff')
foreach ($file in Get-ChildItem -LiteralPath (Join-Path $projectRoot 'output') -File -Recurse | Sort-Object FullName)
{
    $relative = [System.IO.Path]::GetRelativePath((Join-Path $projectRoot 'output'), $file.FullName)
    if ($imageExtensions -contains $file.Extension.ToLowerInvariant())
    {
        if ($file.Name -like '*_concept_*')
        {
            Copy-ArchivedFile $file (Join-Path '11_Concepts/Bosses' $file.Name)
        }
        else
        {
            Copy-ArchivedFile $file (Join-Path '12_Previews' $relative)
        }
    }
    else
    {
        Copy-ArchivedFile $file (Join-Path '13_Documentation/Production' $relative)
    }
}
Copy-AssetTree 'docs' '13_Documentation/GameSpecs'
Copy-ArchivedFile (Get-Item -LiteralPath (Join-Path $projectRoot 'Assets/_Game/SETUP.md')) '13_Documentation/SETUP.md'

# Kiểm tra mọi ảnh trong Assets đều đã được gom, kể cả khi thêm thư mục mới.
foreach ($file in Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Assets') -File -Recurse)
{
    if ($imageExtensions -notcontains $file.Extension.ToLowerInvariant()) { continue }
    $source = [System.IO.Path]::GetRelativePath($projectRoot, $file.FullName).Replace('\', '/')
    if ($source -notin $records.Source)
    {
        $relative = [System.IO.Path]::GetRelativePath((Join-Path $projectRoot 'Assets'), $file.FullName)
        Copy-ArchivedFile $file (Join-Path '10_Unity/OtherArt' $relative)
        if (Test-Path -LiteralPath ($file.FullName + '.meta'))
        {
            Copy-ArchivedFile (Get-Item -LiteralPath ($file.FullName + '.meta')) (Join-Path '10_Unity/OtherArt' ($relative + '.meta'))
        }
    }
}

$records | Sort-Object Archive | Export-Csv -LiteralPath (Join-Path $archiveRoot 'manifest.csv') -NoTypeInformation -Encoding utf8
$imageCount = @($records | Where-Object { $imageExtensions -contains [System.IO.Path]::GetExtension($_.Archive).ToLowerInvariant() }).Count
$summary = [pscustomobject]@{
    GeneratedAt = (Get-Date).ToString('o')
    Files = $records.Count
    Images = $imageCount
    Bytes = ($records | Measure-Object Bytes -Sum).Sum
    VerifiedSHA256 = $true
    Groups = @($records | Group-Object { ($_.Archive -split '/')[0] } | Sort-Object Name | Select-Object Name, Count)
}
$summary | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $archiveRoot 'summary.json') -Encoding utf8
$summary | Select-Object Files, Images, Bytes, VerifiedSHA256 | Format-List

if (Get-Command node -ErrorAction SilentlyContinue)
{
    & node (Join-Path $archiveRoot 'build-catalog.mjs')
    if ($LASTEXITCODE -ne 0) { throw 'Catalog generation failed' }
}
else
{
    Write-Warning 'Install Node.js and run node RawAssets/build-catalog.mjs to refresh the catalog.'
}
