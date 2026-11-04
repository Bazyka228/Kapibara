$ErrorActionPreference='Stop'
$projectRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$artRoot=Join-Path $projectRoot 'Assets/Capybara67/Resources/Outfits'
$catalogPath=Join-Path $artRoot 'Catalog.json'
$catalog=Get-Content -LiteralPath $catalogPath -Raw | ConvertFrom-Json
# Measured on the final generated PNGs; decorative duck eyes and jewels are excluded.
$landmarks=@{
 'none'=@(@(372,344,24,-20,510,333,18,-16),@(801,190,22,25,935,163,20,23),@(1338,123,22,25),@(563,646,24,-20),@(1101,605,24,-20))
 'cap'=@(@(373,345,24,-20,516,333,18,-16),@(803,191,22,25,936,163,20,23),@(1338,132,22,25),@(584,648,24,-20),@(1129,621,26,-20))
 'crown'=@(@(358,357,24,-20,498,351,18,-16),@(784,196,22,25,916,170,20,23),@(1328,141,22,25),@(565,650,24,-20),@(1101,605,24,-20))
 'panama'=@(@(371,343,24,-20,512,331,18,-16),@(802,191,22,25,934,164,20,23),@(1342,134,22,25),@(585,647,24,-20),@(1111,618,24,-20))
 'duck'=@(@(385,348,24,-20,519,337,18,-16),@(781,205,22,25,907,180,20,23),@(1319,157,22,25),@(579,661,24,-20),@(1091,627,24,-20))
}
$frames=@(@(12,216,244),@(43,172,243),@(40,190,243),@(37,196,241),@(48,190,241))
$reports=@()
foreach($head in @('none','cap','crown','panama','duck')){
 $name="$head-bandana"
 $measurement=& (Join-Path $PSScriptRoot 'Measure-Atlas.ps1') -Path (Join-Path $artRoot "$name.png") -ExpectedCount 5 -Eyes
 $bounds=($measurement[0]|ConvertFrom-Json).bounds
 $stages=@()
 for($n=0;$n -lt 5;$n++){
  $b=$bounds[$n];$left=[Math]::Max(0,$b[0]-2);$top=[Math]::Max(0,$b[1]-2)
  $width=[Math]::Min(1536,$b[0]+$b[2]+2)-$left;$height=[Math]::Min(1024,$b[1]+$b[3]+2)-$top
  $f=$frames[$n];$targetHeight=$height*$f[1]/$width
  $eyes=@();$e=$landmarks[$head][$n]
  for($i=0;$i -lt $e.Count;$i+=4){$eyes+=@{x=$e[$i];y=$e[$i+1];z=$e[$i+2];w=$e[$i+3]}}
  $stages+=@{crop=@{x=$left;y=$top;width=$width;height=$height};frame=@{x=$f[0];y=[Math]::Round($f[2]-$targetHeight,3);width=$f[1];height=[Math]::Round($targetHeight,3)};eyes=$eyes}
 }
 $keyHead=if($head -eq 'none'){''}else{$head}
 $catalog.outfits=@($catalog.outfits|Where-Object {!($_.head -eq $keyHead -and $_.clothes -eq 'bandana' -and $_.skin -eq 'classic' -and $_.form -eq -1)})
 $catalog.outfits+=@{head=$keyHead;clothes='bandana';skin='classic';form=-1;resource="Outfits/$name";sourceWidth=1536;sourceHeight=1024;stages=$stages}
 $reports+=@{name=$name;alpha=($measurement[0]|ConvertFrom-Json);features=($measurement[1]|ConvertFrom-Json);eyes=$landmarks[$head]}
}
$catalog|ConvertTo-Json -Depth 12|Set-Content -LiteralPath $catalogPath -Encoding utf8
$reports|ConvertTo-Json -Depth 10|Set-Content -LiteralPath (Join-Path $PSScriptRoot 'landmarks-bandana-20261019.json') -Encoding utf8
& (Join-Path $PSScriptRoot 'Update-ProductionManifest.ps1')
