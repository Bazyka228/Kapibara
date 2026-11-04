$ErrorActionPreference='Stop'
$projectRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$source=Get-Content -LiteralPath (Join-Path $projectRoot 'Assets/Capybara67/Idle/IdleCatalog.cs') -Raw
$data=[regex]::Match($source,'const string Data=@"(.*?)";', [System.Text.RegularExpressions.RegexOptions]::Singleline).Groups[1].Value
if(!$data){throw 'Cannot read the wardrobe catalog'}
$items=@($data -split "`n" | ForEach-Object { $a=$_.Trim() -split '\|'; [pscustomobject]@{id=$a[0];slot=[int]$a[2]} })
$hats=@('')+@($items|Where-Object slot -eq 0|ForEach-Object id)
$clothes=@('')+@($items|Where-Object slot -eq 1|ForEach-Object id)
$catalog=Get-Content -LiteralPath (Join-Path $projectRoot 'Assets/Capybara67/Resources/Outfits/Catalog.json') -Raw|ConvertFrom-Json
$ready=@{}
foreach($entry in $catalog.outfits){
 if($entry.skin -ne 'classic' -or $entry.form -ne -1){continue}
 if($entry.stages.Count -ne 5){throw "Incomplete stages: $($entry.resource)"}
 if(!(Test-Path -LiteralPath (Join-Path $projectRoot "Assets/Capybara67/Resources/$($entry.resource).png"))){throw "Missing atlas: $($entry.resource)"}
 $key="$($entry.head)|$($entry.clothes)"
 if($ready.ContainsKey($key)){throw "Duplicate pair: $key"}
 $ready[$key]=$entry
}
$rows=@(foreach($hat in $hats){foreach($clothing in $clothes){
 $key="$hat|$clothing";$entry=$ready[$key]
 [pscustomobject]@{head=$hat;clothes=$clothing;skin='classic';form=-1;stages=5;status=$(if($entry){'integrated'}else{'planned'});resource=$(if($entry){$entry.resource}else{''})}
}})
$rows|Export-Csv -LiteralPath (Join-Path $PSScriptRoot 'production.csv') -Encoding utf8 -NoTypeInformation
[pscustomobject]@{combinations=$rows.Count;portraits=$rows.Count*5;integratedAtlases=$ready.Count;integratedPortraits=$ready.Count*5;remainingAtlases=$rows.Count-$ready.Count}|ConvertTo-Json

