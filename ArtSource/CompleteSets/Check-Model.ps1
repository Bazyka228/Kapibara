$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$stub=Join-Path $project '../Verification/CompleteSets/UnityResourceStub.cs'
@'
namespace UnityEngine {
 public class TextAsset {public string text;public static implicit operator bool(TextAsset t)=>t!=null;}
 public static class Resources {public static T Load<T>(string path) where T:class {throw new System.Exception("Unexpected resource access: "+path);}}
 public static class JsonUtility {public static T FromJson<T>(string json){throw new System.Exception("Unexpected JSON access");}}
}
'@ | Set-Content -LiteralPath $stub -Encoding utf8
Add-Type -IgnoreWarnings -Path $stub,(Join-Path $project 'Assets/Capybara67/Idle/CapyCompleteSets.cs'),(Join-Path $project 'Assets/Capybara67/Idle/IdleCatalog.cs'),(Join-Path $project 'Assets/Capybara67/Idle/IdleModel.cs'),(Join-Path $project 'Assets/Capybara67/Idle/IdleTapProgress.cs'),(Join-Path $project 'Assets/Capybara67/Idle/IdleSurprises.cs'),(Join-Path $project 'Assets/Capybara67/Editor/CapyCareChecks.cs')
$json=Get-Content (Join-Path $project 'Assets/Capybara67/Resources/CompleteSets/Sets.json') -Raw|ConvertFrom-Json
$defs=[Capybara67.Idle.CapySetDefinition[]]@($json.sets|ForEach-Object {
 $d=[Capybara67.Idle.CapySetDefinition]::new();$d.id=$_.id;$d.slug=$_.slug;$d.name=$_.name;$d.color=$_.color;$d.legacy=[string[]]$_.legacy;$d.stage=$_.stage;$d.price=$_.price;$d
})
[Capybara67.Idle.CapyCompleteSets].GetField('definitions',[Reflection.BindingFlags]'NonPublic,Static').SetValue($null,$defs)
$script:checks=0
function Check([bool]$ok,[string]$label){if(!$ok){throw $label};$script:checks++}
$items=[Capybara67.Idle.IdleCatalog]::Items
Check ($defs.Count -eq 36) '36 set definitions'
Check (@($items|Where-Object {[Capybara67.Idle.CapyCompleteSets]::Visible($_)}).Count -eq 79) '79 visible shop entries'
foreach($item in $items|Where-Object {$_.slot -lt 2 -and !$_.IsSet}) {
 $save=[Capybara67.Idle.IdleSave]::new();$save.coins=5000;$save.growth=6;$save.owned.Add($item.id);$save.equipped[$item.slot]=$item.id
 $model=[Capybara67.Idle.IdleModel]::new($save)
 $mapped=@($defs|Where-Object {$item.id -in $_.legacy})
 Check ($mapped.Count -gt 0) ('mapping '+$item.id)
 foreach($set in $mapped){Check ($save.owned.Contains($set.id)) ('grant '+$set.id)}
 Check ($save.owned.Contains($item.id)) ('receipt '+$item.id)
 Check ([Capybara67.Idle.CapyCompleteSets]::IsSet($save.equipped[0]) -and $save.equipped[1] -eq '') ('complete equip '+$item.id)
 Check ($save.coins -eq 5000 -and $model.Stage -eq 2) ('progress '+$item.id)
 $before=$save|ConvertTo-Json -Depth 5 -Compress;$model.Repair()
 Check (($save|ConvertTo-Json -Depth 5 -Compress) -ceq $before) ('idempotent '+$item.id)
}
foreach($set in $defs){
 $model=[Capybara67.Idle.IdleModel]::new();$model.S.growth=150000;$model.S.coins=100000
 if($set.id -eq 'set_dawn'){Check (!$model.Purchase($set.id)) 'reward cannot be bought';$model.S.dailyCount=7;$model.GrantDailyMilestones()}
 Check ($model.Purchase($set.id)) ('purchase '+$set.id)
 Check ($model.S.coins -eq (100000-$set.price)) ('single charge '+$set.id)
 Check ($model.S.equipped[0] -eq $set.id -and $model.S.equipped[1] -eq '') ('equip '+$set.id)
 Check ($model.Purchase($set.id) -and $model.S.coins -eq (100000-$set.price)) ('free re-equip '+$set.id)
 Check ($model.Purchase('sport') -and $model.S.equipped[0] -eq $set.id) ('skin keeps whole portrait '+$set.id)
}
$careChecks=[Capybara67.Editor.CapyCareChecks]::Run()
"PASS: $script:checks wardrobe and $careChecks care model assertions. Real model/catalog/migration source; Unity resource loading stubbed; no PlayerPrefs or user profile access."


