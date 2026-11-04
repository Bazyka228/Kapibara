. "$PSScriptRoot/Check-Model.ps1"
function NewActivityModel {
 $m=[Capybara67.Idle.IdleModel]::new();$m.S.growth=6;$m.S.starter=$true;return $m
}
$m=[Capybara67.Idle.IdleModel]::new()
Check (!$m.StartActivity(0)) 'activities gated before stage two'
$m=NewActivityModel
Check ($m.StartActivity(1)) 'search starts'
$id=$m.S.activity.serial
Check (!$m.StartActivity(0)) 'unfinished run cannot be replaced'
Check ($m.StartActivity(1) -and $m.S.activity.serial -eq $id) 'resume preserves run'
Check (!$m.ActivityFind(-1) -and !$m.ActivityFind(6)) 'invalid search targets rejected'
for($i=0;$i -lt 6;$i++){Check ($m.ActivityFind($i)) 'target accepted';Check (!$m.ActivityFind($i)) 'target cannot repeat'}
$m.StepActivity(.74)
Check (!$m.S.activity.complete -and $m.S.coins -eq 0) 'no premature award'
$m.StepActivity(.02)
Check ($m.S.activity.complete -and $m.S.activity.reward -gt 0) 'search completes'
$paid=$m.S.coins;$growth=$m.S.growth
$m.StepActivity(25);$m.ActivityFind(0)|Out-Null
Check ($m.S.coins -eq $paid -and $m.S.growth -eq $growth -and $m.S.cares -eq 0 -and $m.S.feeds -eq 0) 'no duplicate payout or core progression'
$loaded=[Capybara67.Idle.IdleModel]::new($m.S);$loaded.StepActivity(25)
Check ($loaded.S.coins -eq $paid) 'completed save does not repay'
Check ($m.StartActivity(0) -and $m.S.activity.serial -gt $id) 'one click replay new identity'
for($frame=0;$frame -lt 251;$frame++){
 $a=$m.S.activity
 for($i=0;$i -lt 8;$i++){if(($a.resolved -band (1 -shl $i)) -eq 0){$m.ActivityBasket([Capybara67.Idle.IdleModel]::FruitX($a,$i));break}}
 $m.StepActivity(.1)
}
Check ($m.S.activity.complete -and $m.S.activity.score -eq 8) 'basket perfect run'
$m=NewActivityModel;$m.StartActivity(0)|Out-Null;$m.ActivityBasket(1)
$m.StepActivity([double]::NaN);$m.StepActivity(-1)
Check ($m.S.activity.elapsed -eq 0) 'invalid time ignored'
for($i=0;$i -lt 250;$i++){$m.StepActivity(.1)}
$m.StepActivity(.01)
Check ($m.S.activity.complete -and $m.S.activity.reward -ge [Math]::Ceiling($m.S.activity.rate*25*.125)) 'minimum reward guaranteed'
$m=NewActivityModel;$m.StartActivity(1)|Out-Null;$m.StepActivity(9);$m.ActivityFind(2)|Out-Null
$loaded=[Capybara67.Idle.IdleModel]::new($m.S)
Check ($loaded.StartActivity(1) -and $loaded.S.activity.elapsed -eq 9 -and $loaded.S.activity.mask -eq 4) 'unfinished progress survives model reload'
"PASS: activities and existing model checks: $script:checks. No PlayerPrefs access."
