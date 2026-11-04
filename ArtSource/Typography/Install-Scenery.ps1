$ErrorActionPreference='Stop'
$source=Join-Path $PSScriptRoot '../../Assets/Capybara67'
$target='C:/Users/fedor/Second Project'
$backup=Join-Path $target ('Backups/SceneryTypography-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
$files=@('Idle/CapyIdlePortraits.cs','Idle/CapyIdleStages.cs','Idle/CapyIdleArt.cs','Idle/CapyIdleScenery.cs','Idle/CapyIdleGame.cs','Editor/CapyIdleValidation.cs')
foreach($f in $files){$to=Join-Path $target ('Assets/Capybara67/'+$f);if(Test-Path $to){$saved=Join-Path $backup $f;New-Item -ItemType Directory -Force (Split-Path $saved)|Out-Null;Copy-Item -LiteralPath $to -Destination $saved};Copy-Item -LiteralPath (Join-Path $source $f) -Destination $to -Force}
Copy-Item -Path (Join-Path $source 'Resources/Fonts/Nunito-*'),(Join-Path $source 'Resources/Fonts/OFL-Nunito.txt') -Destination (Join-Path $target 'Assets/Capybara67/Resources/Fonts') -Force
$art=Join-Path $target 'ArtSource/Typography';New-Item -ItemType Directory -Force $art|Out-Null;Copy-Item -Path (Join-Path $PSScriptRoot '*') -Destination $art -Force
"Installed eight scenes, bare stages and Nunito. Backup: $backup"
