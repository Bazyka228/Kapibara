param([switch]$RequireAll,[switch]$ReviewOnly)
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$resource=Join-Path $project 'Assets/Capybara67/Resources/CompleteSets'
$manifest=Get-Content (Join-Path $PSScriptRoot 'manifest.json') -Raw | ConvertFrom-Json
$entries=@();$report=@()
foreach($set in $manifest) {
 try {
 $path=Join-Path $resource ($set.slug+'.png')
 if(!(Test-Path -LiteralPath $path)){if($RequireAll){throw "Missing set $($set.id)"};continue}
 $lines=@(& (Join-Path $PSScriptRoot '../Outfits/Measure-Atlas.ps1') -Path $path -ExpectedCount 5 -Eyes)
 $bounds=($lines[0]|ConvertFrom-Json).bounds;$features=($lines[1]|ConvertFrom-Json).features
 $bmp=[Drawing.Bitmap]::new($path);$sw=$bmp.Width;$sh=$bmp.Height;$bmp.Dispose()
 $stages=@();$eyeCounts=@()
 for($n=0;$n -lt 5;$n++) {
  $b=$bounds[$n];$pad=if($set.slug -in @('explorer','detective','orchard','dragon')){0}else{2}
  $x=[Math]::Max(0,$b[0]-$pad);$y=[Math]::Max(0,$b[1]-$pad)
  $w=[Math]::Min($sw-$x,$b[2]+2*$pad);$h=[Math]::Min($sh-$y,$b[3]+2*$pad)
  $expected=if($n -eq 0 -or ($set.slug -eq 'gardener' -and $n -ge 3) -or ($set.slug -in @('coffee','festival','sun','dawn') -and $n -eq 3)){0}elseif($n -eq 1){2}else{1}
  $eyes=@()
  if($expected -gt 0) {
   # Read-only detection of compact, filled eye dots in the upper face region.
   $dots=@($features|Where-Object { $_[0] -gt ($x+$w*.38) -and $_[0] -lt ($x+$w*.99) -and $_[1] -gt ($y+$h*.12) -and $_[1] -lt ($y+$h*.49) -and $_[2] -ge 15 -and $_[3] -ge 20 -and $_[4]/($_[2]*$_[3]) -gt .62 })
   # Frog-hat button eyes are decoration, never animate them.
   if($set.slug -eq 'frog' -and $n -ge 2){$eyeX=@(0,0,1336,599,1146)[$n];$dots=@($dots|Where-Object { [Math]::Abs($_[0]-$eyeX) -lt 3 })}
   if($set.slug -eq 'dragon'){
    if($n -eq 1){$dots=@($dots|Where-Object { $_[1] -gt 180 })}
    else{$eyeX=@(0,0,1333,601,1141)[$n];$dots=@($dots|Where-Object { [Math]::Abs($_[0]-$eyeX) -lt 3 })}
   }
   if($dots.Count -ne $expected){throw "Eye landmark review needed: $($set.slug) age $($n+1): expected $expected, found $($dots.Count): $($dots|ConvertTo-Json -Compress)"}
   foreach($d in $dots){$eyes+=@{x=$d[0];y=$d[1];z=[Math]::Round($d[2]/1.6+3,2);w=[Math]::Round($d[3]/1.6+3,2)}}
  }
  $fw=@(216,172,184,198,214)[$n];$fh=$h*$fw/$w
  if($fh -gt 230){$fw*=230/$fh;$fh=230}
  $stages+=@{crop=@{x=$x;y=$y;width=$w;height=$h};frame=@{x=[Math]::Round((240-$fw)/2,3);y=[Math]::Round(244-$fh,3);width=[Math]::Round($fw,3);height=[Math]::Round($fh,3)};eyes=@($eyes)}
  $eyeCounts+=$eyes.Count
 }
 for($i=0;$i -lt 5;$i++){for($j=$i+1;$j -lt 5;$j++){
  $a=$stages[$i].crop;$b=$stages[$j].crop
  if($a.x -lt $b.x+$b.width -and $a.x+$a.width -gt $b.x -and $a.y -lt $b.y+$b.height -and $a.y+$a.height -gt $b.y){throw "Overlapping rectangular crops: $($set.slug) $i $j"}
 }}
 $entries+=@{head=$set.id;clothes='';skin='classic';form=-1;resource='CompleteSets/'+$set.slug;sourceWidth=$sw;sourceHeight=$sh;stages=$stages}
 $report+=@{id=$set.id;ages=5;animatedEyes=$eyeCounts;bounds=$bounds}
 $meta=$path+'.meta'
 if(!(Test-Path -LiteralPath $meta)){
  $template=Get-Content (Join-Path $project 'Assets/Capybara67/Resources/Outfits/Scarf.png.meta') -Raw
  $template=$template -replace 'guid: [a-f0-9]+',('guid: '+[Guid]::NewGuid().ToString('N'))
  [IO.File]::WriteAllText($meta,$template)
 }
 } catch {if(!$ReviewOnly){throw};Write-Warning $_}
}
[IO.File]::WriteAllText((Join-Path $resource 'Catalog.json'),(@{outfits=$entries}|ConvertTo-Json -Depth 12))
[IO.File]::WriteAllText((Join-Path $PSScriptRoot 'atlas-checks.json'),($report|ConvertTo-Json -Depth 12))
Write-Output "Validated $($entries.Count) sets / $($entries.Count*5) portraits; disjoint crops and precise eye dots."
