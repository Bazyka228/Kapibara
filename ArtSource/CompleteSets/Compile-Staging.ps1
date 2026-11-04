$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$actual='C:\Users\fedor\Second Project'
$verification=Join-Path $project '../../Second Project/Verification/CompleteSets'
$verification=[IO.Path]::GetFullPath((Join-Path $project '../Verification/CompleteSets'))
New-Item -ItemType Directory -Force $verification | Out-Null
$editor='C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Data'
foreach($assembly in @('Assembly-CSharp','Assembly-CSharp-Editor')) {
 [xml]$xml=Get-Content (Join-Path $actual ($assembly+'.csproj'))
 $argsList=@('-nologo','-target:library','-langversion:9.0','-nostdlib+',('-out:"'+(Join-Path $verification ($assembly+'.dll'))+'"'))
 $defines=@($xml.Project.PropertyGroup.DefineConstants|Where-Object {$_})[0]
 $defines+=';RewardedAdv_yg'
 $argsList+=('-define:'+($defines -replace ';',','))
 $dotween=Join-Path $project 'Assets/Plugins/Demigiant/DOTween'
 $argsList+=('-r:"'+(Join-Path $dotween 'DOTween.dll')+'"')
 if($assembly -eq 'Assembly-CSharp-Editor'){$argsList+=('-r:"'+(Join-Path $verification 'Assembly-CSharp.dll')+'"')}
 foreach($ref in $xml.Project.ItemGroup.Reference) {
  if(!$ref.HintPath){continue}
  $path=[string]$ref.HintPath
  if(![IO.Path]::IsPathRooted($path)){$path=Join-Path $actual $path}
  if($assembly -eq 'Assembly-CSharp-Editor' -and [IO.Path]::GetFileName($path) -eq 'Assembly-CSharp.dll'){$path=Join-Path $verification 'Assembly-CSharp.dll'}
  $argsList+=('-r:"'+$path+'"')
 }
 foreach($compile in $xml.Project.ItemGroup.Compile) {
  if(!$compile.Include){continue}
  $path=Join-Path $project $compile.Include
  if(!(Test-Path -LiteralPath $path)){$path=Join-Path $actual $compile.Include}
  $argsList+=('"'+$path+'"')
 }
 if($assembly -eq 'Assembly-CSharp'){$argsList+=('"'+(Join-Path $project 'Assets/Capybara67/Idle/CapyCompleteSets.cs')+'"')}
 if($assembly -eq 'Assembly-CSharp'){$argsList+=('"'+(Join-Path $project 'Assets/Capybara67/Idle/CapyIdleScenery.cs')+'"')}
 if($assembly -eq 'Assembly-CSharp'){$argsList+=('"'+(Join-Path $project 'Assets/Capybara67/Idle/CapyIdleCosmetics.cs')+'"')}
 if($assembly -eq 'Assembly-CSharp'){$argsList+=('"'+(Join-Path $project 'Assets/Capybara67/Idle/CapyIdleCare.cs')+'"')}
 if($assembly -eq 'Assembly-CSharp'){
  foreach($tapFile in @('IdleTapProgress.cs','CapyIdleTap.cs','IdleSurprises.cs','CapyIdleSurprises.cs')){$argsList+=('"'+(Join-Path $project ('Assets/Capybara67/Idle/'+$tapFile))+'"')}
  $argsList+=('"'+(Join-Path $project 'Assets/Capybara67/Idle/CapyTweenDriver.cs')+'"')
  $argsList+=('"'+(Join-Path $project 'Assets/Capybara67/Idle/CapyIdleWelcome.cs')+'"')
  foreach($gestureFile in @('CapyCareGesture.cs','CapyIdleGestures.cs')){$argsList+=('"'+(Join-Path $project ('Assets/Capybara67/Idle/'+$gestureFile))+'"')}
  foreach($module in Get-ChildItem -LiteralPath (Join-Path $dotween 'Modules') -Filter '*.cs'){$argsList+=('"'+$module.FullName+'"')}
 }
 foreach($rewardFile in Get-ChildItem -LiteralPath (Join-Path $project 'Assets/PluginYourGames/Modules/RewardedAdv') -Filter '*.cs' -Recurse){
  $isEditor=$rewardFile.FullName -match '[\\/]Editor[\\/]'
  if(($assembly -eq 'Assembly-CSharp-Editor') -eq $isEditor){$argsList+=('"'+$rewardFile.FullName+'"')}
 }
 if($assembly -eq 'Assembly-CSharp-Editor'){$argsList+=('"'+(Join-Path $project 'Assets/Capybara67/Editor/CapyCareChecks.cs')+'"')}
 $rsp=Join-Path $verification ($assembly+'.rsp');[IO.File]::WriteAllLines($rsp,$argsList)
 & (Join-Path $editor 'NetCoreRuntime/dotnet.exe') (Join-Path $editor 'DotNetSdk/sdk/8.0.318/Roslyn/bincore/csc.dll') ('@'+$rsp) 2>&1 | Tee-Object -FilePath (Join-Path $verification ($assembly+'.log'))
 if($LASTEXITCODE -ne 0){throw "Compilation failed: $assembly"}
}
'Both staged Unity assemblies compiled successfully.'


