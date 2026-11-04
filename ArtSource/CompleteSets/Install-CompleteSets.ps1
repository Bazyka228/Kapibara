param([string]$Target='C:\Users\fedor\Second Project')
$ErrorActionPreference='Stop'
$source=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$targetRoot=(Resolve-Path -LiteralPath $Target).Path
if(!(Test-Path -LiteralPath (Join-Path $targetRoot 'ProjectSettings/ProjectVersion.txt'))){throw 'Not a Unity project'}
$catalog=Get-Content (Join-Path $source 'Assets/Capybara67/Resources/CompleteSets/Catalog.json') -Raw|ConvertFrom-Json
if($catalog.outfits.Count -ne 36){throw 'Expected all 36 validated atlases'}
foreach($entry in $catalog.outfits){if($entry.stages.Count -ne 5 -or !(Test-Path (Join-Path $source ('Assets/Capybara67/Resources/'+$entry.resource+'.png')))){throw 'Incomplete production asset'}}
$files=@('Idle/CapyCompleteSets.cs','Idle/CapyIdleCare.cs','Idle/CapyIdleAvatar.cs','Idle/IdleHold.cs','Editor/CapyCareChecks.cs','Idle/CapyIdleCosmetics.cs','Idle/CapyPortraitMotion.cs','Idle/CapyIdleLife.cs','Idle/CapyIdleScenery.cs','Idle/CapyIdleStages.cs','Idle/CapyAccessoryReview.cs','Idle/IdleCatalog.cs','Idle/IdleModel.cs','Idle/CapyBakedOutfits.cs','Idle/CapyIdlePortraits.cs','Idle/CapyIdleTheme.cs','Idle/CapyIdleGame.cs','Editor/CapyIdleValidation.cs','Resources/Shaders/CapyEyeBlink.shader')
$backup=Join-Path $targetRoot ('Backups/CompleteSets-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
$files+='Idle/CapyTweenDriver.cs'
$files+='Idle/CapyIdleWelcome.cs'
$files+='Idle/CapyCareGesture.cs'
$files+='Idle/CapyIdleGestures.cs'
$files+='Idle/IdleTapProgress.cs'
$files+='Idle/CapyIdleTap.cs'
$files+='Idle/IdleSurprises.cs'
$files+='Idle/CapyIdleSurprises.cs'
New-Item -ItemType Directory -Force $backup|Out-Null
foreach($relative in $files){
 $destination=Join-Path $targetRoot ('Assets/Capybara67/'+$relative)
 if(Test-Path -LiteralPath $destination){$saved=Join-Path $backup $relative;New-Item -ItemType Directory -Force (Split-Path $saved)|Out-Null;Copy-Item -LiteralPath $destination -Destination $saved}
}
$assets=Join-Path $targetRoot 'Assets/Capybara67/Resources/CompleteSets'
if(Test-Path -LiteralPath $assets){Copy-Item -LiteralPath $assets -Destination (Join-Path $backup 'CompleteSets') -Recurse}
New-Item -ItemType Directory -Force $assets|Out-Null
Copy-Item -Path (Join-Path $source 'Assets/Capybara67/Resources/CompleteSets/*') -Destination $assets -Force -Recurse
foreach($relative in $files){Copy-Item -LiteralPath (Join-Path $source ('Assets/Capybara67/'+$relative)) -Destination (Join-Path $targetRoot ('Assets/Capybara67/'+$relative)) -Force}
$dotweenTarget=Join-Path $targetRoot 'Assets/Plugins/Demigiant'
$rewardTarget=Join-Path $targetRoot 'Assets/PluginYourGames/Modules'
New-Item -ItemType Directory -Force $rewardTarget|Out-Null
if(Test-Path -LiteralPath (Join-Path $rewardTarget 'RewardedAdv')){Copy-Item -LiteralPath (Join-Path $rewardTarget 'RewardedAdv') -Destination (Join-Path $backup 'RewardedAdv') -Recurse}
Copy-Item -LiteralPath (Join-Path $source 'Assets/PluginYourGames/Modules/RewardedAdv') -Destination $rewardTarget -Recurse -Force
Copy-Item -LiteralPath (Join-Path $source 'Assets/PluginYourGames/Modules/RewardedAdv.meta') -Destination $rewardTarget -Force
New-Item -ItemType Directory -Force $dotweenTarget|Out-Null
if(Test-Path -LiteralPath (Join-Path $dotweenTarget 'DOTween')){Copy-Item -LiteralPath (Join-Path $dotweenTarget 'DOTween') -Destination (Join-Path $backup 'DOTween') -Recurse}
Copy-Item -LiteralPath (Join-Path $source 'Assets/Plugins/Demigiant/DOTween') -Destination $dotweenTarget -Recurse -Force
$resources=Join-Path $targetRoot 'Assets/Resources'
New-Item -ItemType Directory -Force $resources|Out-Null
if(Test-Path -LiteralPath (Join-Path $resources 'DOTweenSettings.asset')){Copy-Item -LiteralPath (Join-Path $resources 'DOTweenSettings.asset') -Destination $backup}
Copy-Item -Path (Join-Path $source 'Assets/Resources/DOTweenSettings.asset*') -Destination $resources -Force
$art=Join-Path $targetRoot 'ArtSource/CompleteSets';New-Item -ItemType Directory -Force $art|Out-Null
Copy-Item -Path (Join-Path $PSScriptRoot '*') -Destination $art -Force
foreach($relative in $files){
 if((Get-FileHash (Join-Path $source ('Assets/Capybara67/'+$relative))).Hash -ne (Get-FileHash (Join-Path $targetRoot ('Assets/Capybara67/'+$relative))).Hash){throw "Copy mismatch: $relative"}
}
"Installed 36 complete sets / 180 portraits. Backup: $backup"


