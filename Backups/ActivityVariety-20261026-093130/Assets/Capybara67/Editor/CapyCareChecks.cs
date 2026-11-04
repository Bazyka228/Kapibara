using System;
using Capybara67.Idle;

namespace Capybara67.Editor {
 // Pure model checks also run outside Unity; never access PlayerPrefs.
 public static class CapyCareChecks {
  static int checks;
  static void Check(bool ok,string name){checks++;if(!ok)throw new Exception("Care system: "+name);}
  static bool Near(double a,double b)=>Math.Abs(a-b)<.00001;
  static IdleModel Fresh(double value=75){var m=new IdleModel();m.S.fullness=m.S.comfort=m.S.mood=value;return m;}
  public static int Run(){
   checks=0;
   CheckFirstSession();
   CheckTreeProtection();
   var old=new IdleSave{coins=5000,growth=6,dailyCount=7,starter=true,bed=2,stars=17,feeds=6,activeSeconds=900};
   var m=new IdleModel(old);
   Check(m.S.careVersion==1&&m.S.fullness==75&&m.S.mood==75&&m.S.comfort==75,"legacy needs initialized once");
   Check(m.S.coins==5000&&m.S.growth==6&&m.S.dailyCount==7&&m.S.stars==17&&m.S.feeds==6&&m.S.activeSeconds==900,"legacy progression unchanged");
   m.S.fullness=12;m.Repair();Check(m.S.fullness==12,"repair never refills existing needs");
   m.S.mood=double.NaN;m.S.comfort=double.PositiveInfinity;m.S.fullness=120;m.S.lastPet=double.NaN;m.Repair();
   Check(m.S.mood==0&&m.S.comfort==0&&m.S.fullness==100&&m.S.lastPet==-100,"damaged values sanitized");
   m=Fresh(0);m.S.bed=2;double baseRate=m.BasePassive,baseTap=m.BaseCareValue;
   m.Tick(60);Check(Near(m.S.coins,baseRate*60),"empty needs preserve baseline passive");
   double funds=m.S.coins;Check(m.Care()&&Near(m.S.coins-funds,baseTap),"empty needs preserve click");
   Check(m.S.fullness==0&&m.S.comfort==0&&m.S.mood==0,"needs floor");
   m=Fresh();m.S.bed=2;m.Tick(60);
   Check(Near(m.S.fullness,74.5)&&Near(m.S.comfort,74.7)&&Near(m.S.mood,74.6),"slow active decay");
   Check(Near(m.S.coins,m.BasePassive*60*1.05),"happy passive bonus");
   Check(Near(m.CareValue,m.BaseCareValue*1.1),"happy click bonus");
   var small=Fresh(72);var large=Fresh(72);small.S.bed=large.S.bed=4;
   for(int n=0;n<1200;n++)small.Tick(.5);large.Tick(600);
   Check(Near(small.S.coins,large.S.coins)&&Near(small.S.fullness,large.S.fullness),"income independent of frame partition across threshold");
   m=Fresh(20);
   double growth=m.S.growth;int feeds=m.S.feeds,pity=m.S.pity;
   Check(m.CareAction(0)&&m.S.fullness==45,"free leaf");
   Check(m.S.growth==growth&&m.S.feeds==feeds&&m.S.pity==pity&&m.S.coins==0,"leaf no growth money or evolution");
   Check(!m.CareAction(0),"animation action lock");
   m.Tick(2);Check(m.CareAction(1),"free wash");
   Check(m.CareMultiplier==1.2&&m.S.comfort>59,"wash restores and boosts useful care");
   double until=m.S.careBuffUntil;m.Tick(3);m.CareAction(1);m.Tick(3);m.CareAction(1);
   Check(m.S.careBuffUntil<=m.S.activeSeconds+30,"boost never stacks duration");
   m.Tick(40);Check(m.CareMultiplier<=1.1,"boost expires");
   m=Fresh(100);m.CareAction(1);Check(m.CareMultiplier==1.1,"full wash does not grant temporary boost");
   m.Tick(3);Check(m.CareAction(3),"pet");Check(!m.CareAction(3),"pet cannot spam");
   m.Tick(2);Check(!m.CareAction(3),"pet has separate ten second interval");
   m.Tick(8);Check(m.CareAction(3),"pet returns after interval");
   m=Fresh(60);m.CareAction(2);Check(m.S.comfort==58&&m.S.mood==90,"play restores mood small mess");
   m.Tick(3);double comfort=m.S.comfort;m.CareAction(2);Check(m.S.comfort==comfort,"play dirt capped per minute");
   m=Fresh(60);m.CareAction(4);Check(m.S.comfort==75&&m.S.mood==65,"brush alternative");
   m=Fresh(60);m.CareAction(5);Check(m.S.resting,"voluntary rest");m.Tick(30);
   Check(Near(m.S.comfort,69.85)&&Near(m.S.mood,69.8)&&m.S.restRemaining==0,"rest grants gradual bounded recovery");
   double after=m.S.mood;m.Tick(30);Check(m.S.mood<after,"rest cannot grant permanent refill");
   m.Care();Check(!m.S.resting,"tap wakes immediately");
   m=Fresh(0);m.S.coins=10;string form;
   Check(m.Feed(0,out form)&&m.S.fullness==30&&m.S.mood==5&&m.S.growth==2&&m.S.feeds==1,"paid feeding integrates unchanged growth");
   Check(!m.Feed(0,out form)&&m.S.fullness==30,"failed feeding has no care side effects");
   m=Fresh(0);Check(m.Starter()&&m.Stage==2&&m.S.fullness==30,"starter integrates once");
   Check(!m.Starter()&&m.S.fullness==30,"starter not duplicated");
   double utc=1900000000;
   foreach(double absence in new[]{60.0,3600,14400,86400,172800,31536000}){
    m=Fresh(0);m.S.bed=2;m.S.lastUtc=utc;
    double award=m.Arrive(utc+absence);
    Check(Near(award,Math.Min(absence,14400)*m.BasePassive),"baseline offline cap "+absence);
    Check(m.S.fullness==(absence>=3600?50:0),"offline floor "+absence);
    Check(m.ReturnGift==(absence>86400),"gift threshold "+absence);
    double balance=m.S.coins;m.Arrive(utc+absence);
    Check(m.S.coins==balance&&!m.ReturnGift,"duplicate arrival no extra reward "+absence);
    m.Arrive(utc);Check(m.S.coins==balance,"rollback no award "+absence);
   }
   m=Fresh(80);m.S.bed=1;m.S.lastUtc=utc;double expected=14400*m.BasePassive*1.05;
   Check(Near(m.Arrive(utc+86400),expected)&&m.S.fullness==70,"offline bonus uses departure state");
   Check(!m.Happy,"restoration does not affect already computed offline bonus");
   m=Fresh(20);m.S.lastUtc=utc;m.CareAction(1);m.Arrive(utc+100);Check(m.S.careBuffUntil==0&&!m.S.resting,"offline clears short active buffs");
   m=Fresh();m.Tick(double.NaN);m.Tick(double.PositiveInfinity);m.Tick(-1);Check(m.S.activeSeconds==0&&m.S.fullness==75,"invalid tick no changes");
   m=Fresh();m.Arrive(utc);Check(m.S.coins==0&&!m.ReturnGift&&m.S.fullness==75,"first visit no imaginary absence");
   m=Fresh();Check(!m.CareAction(-1)&&!m.CareAction(6)&&m.S.fullness==75,"invalid actions no changes");
   Check(IdleModel.Foods.Length==12,"twelve ordinary dishes");
   for(int n=0;n<IdleModel.Foods.Length;n++){
    var food=IdleModel.Foods[n];m=Fresh(0);m.S.growth=IdleModel.Growth[food.stage-1];m.S.coins=food.price;m.S.premiumFood=2;
    double before=m.S.growth;
    Check(m.Feed(n,out form)&&m.S.coins==0&&Near(m.S.growth-before,food.growth),"food cost and growth "+n);
    Check(m.S.fullness==food.fullness&&m.S.premiumFood==2,"food satiety distinct from premium stock "+n);
    m=Fresh();m.S.coins=food.price-1;m.S.growth=150000;
    Check(!m.Feed(n,out form)&&m.S.coins==food.price-1&&m.S.fullness==75,"unaffordable meal unchanged "+n);
    if(food.stage>1){m=Fresh();m.S.coins=1e6;Check(!m.Feed(n,out form)&&m.S.coins==1e6,"stage lock "+n);}
   }
   m=Fresh(0);m.S.growth=360;m.S.premiumFood=1;m.S.coins=987;
   Check(m.Feed(IdleModel.PremiumMeal,out form)&&m.S.premiumFood==0&&m.S.coins==987&&m.S.growth==600&&m.S.fullness==60,"premium stock uses independent index");
   m.Tick(9);Check(!m.Feed(IdleModel.PremiumMeal,out form),"premium empty stock");
   Check(m.MealGrowth(99)==0&&!m.Feed(99,out form)&&m.MealFullness(99)==0,"invalid dish rejected");
   for(int stage=1;stage<=5;stage++){
    m=Fresh();m.S.growth=IdleModel.Growth[stage-1];
    for(int day=1;day<=12;day++){m.RefreshDay("2030-01-"+day.ToString("00"));Check(m.FeaturedFood>=0&&IdleModel.Foods[m.FeaturedFood].stage<=stage,"featured food accessible "+stage+"/"+day);}
   }
   m=Fresh();m.S.growth=150000;m.S.forms.Add("ninja");m.S.form="ninja";
   Check(Near(m.MealGrowth(4),IdleModel.Foods[4].growth*1.08)&&Near(m.MealGrowth(5),IdleModel.Foods[5].growth),"fruit form bonus respects new ingredients");
   // Tap progression is permanent, optional and independent from care needs.
   m=Fresh();Check(m.S.tapPower==0&&m.S.autoAdCount==0&&m.S.autoRemaining==0,"old profile has no invented upgrades or ads");
   for(int kind=0;kind<3;kind++){
    m=Fresh();m.S.coins=1e9;m.S.growth=150000;
    for(int level=0;level<IdleModel.TapUpgradeCaps[kind];level++){
     double price=m.TapPrice(kind),beforeCoins=m.S.coins,beforeTap=m.CareValue;
     Check(m.UpgradeTap(kind)&&m.TapLevel(kind)==level+1&&Near(beforeCoins-m.S.coins,price),"upgrade exactly one level/charge "+kind+"/"+level);
     Check(kind==2||m.CareValue>beforeTap,"upgrade raises tap "+kind+"/"+level);
    }
    double maxCoins=m.S.coins;Check(!m.UpgradeTap(kind)&&m.S.coins==maxCoins,"max upgrade cannot charge "+kind);
    m=Fresh();m.S.growth=150000;m.S.coins=m.TapPrice(kind)-1;Check(!m.UpgradeTap(kind),"poor upgrade rejected "+kind);
    if(kind>0){m=Fresh();m.S.coins=1e9;Check(!m.UpgradeTap(kind),"upgrade age gate "+kind);}
   }
   m=Fresh();Check(!m.UpgradeTap(-1)&&!m.UpgradeTap(3),"invalid upgrade rejected");
   m.S.tapBonus=2;double normal=m.CareValue;
   for(int n=1;n<=10;n++){m.Tick(.5);Check(m.Care(),"combo tap "+n);Check(Near(m.LastTapEarned,m.CareValue*(n==10?2:1)),"every tenth bonus "+n);}
   int combo=m.S.tapCombo;Check(!m.Care()&&m.S.tapCombo==combo,"blocked click never advances combo");
   double now=1900000000; m=Fresh();
   for(int n=1;n<=5;n++){
    int serial=m.BeginAutoAd(now);Check(serial>0&&m.BeginAutoAd(now)==0,"one pending ad "+n);
    Check(!m.CompleteAutoAd(serial+1,now),"wrong reward token rejected "+n);
    Check(m.CompleteAutoAd(serial,now),"confirmed ad counts "+n);
    Check(!m.CompleteAutoAd(serial,now),"duplicate ad reward ignored "+n);
    m.CancelAutoAd();Check(m.BeginAutoAd(now)==0,"ad cooldown or active auto blocks new ad "+n);now+=121;
    Check(m.S.autoAdCount==(n==5?0:n)&&m.S.autoRemaining==(n==5?1200:0),"only fifth ad starts timer "+n);
   }
   m.S.fullness=m.S.comfort=m.S.mood=0;double value=m.CareValue;int beforeCares=m.S.cares,beforeCombo=m.S.tapCombo;
   m.Tick(.4);Check(m.S.coins==0&&Near(m.S.autoRemaining,1199.6),"auto waits for whole second");m.Tick(.6);Check(Near(m.S.coins,value),"auto pays one tap per second");
   m.Tick(1199);Check(Near(m.S.coins,value*1200)&&m.S.autoRemaining==0,"auto has exactly twenty minutes of payouts");
   Check(m.S.cares==beforeCares&&m.S.tapCombo==beforeCombo&&m.S.feeds==0,"auto never farms care quests combo or feeding");
   double finished=m.S.coins;m.Tick(40);Check(m.S.coins==finished,"expired auto stops paying");
   var a=Fresh(72);var b=Fresh(72);a.S.autoRemaining=b.S.autoRemaining=1200;a.S.tapPower=b.S.tapPower=3;a.S.bed=b.S.bed=2;
   a.S.careBuffUntil=b.S.careBuffUntil=30;for(int n=0;n<3600;n++)a.Tick(1.0/3);b.Tick(1200);
   Check(Math.Abs(a.S.coins-b.S.coins)<.0001&&Math.Abs(a.S.autoRemaining-b.S.autoRemaining)<.0001,"auto independent of frame partition and care thresholds");
   m=Fresh();int cancelled=m.BeginAutoAd(now);m.CancelAutoAd();Check(!m.CompleteAutoAd(cancelled,now)&&m.S.autoAdCount==0,"cancel and error never award");
   int lost=m.BeginAutoAd(now);m=new IdleModel(m.S);Check(!m.CompleteAutoAd(lost,now),"reload discards unfinished ad request");
   m.S.autoRemaining=300;m.S.autoPhase=.3;m.S.lastUtc=now;m.Arrive(now+86400);Check(m.S.autoRemaining==300&&Near(m.S.autoPhase,.3),"offline preserves remaining auto time without extra auto earnings");
   m.S.autoRemaining=double.NaN;m.S.autoPhase=double.PositiveInfinity;m.S.tapPower=-1;m.S.tapMultiplier=500;m.S.autoAdCount=99;m.Repair();
   Check(m.S.autoRemaining==0&&m.S.autoPhase==0&&m.S.tapPower==0&&m.S.tapMultiplier==10&&m.S.autoAdCount==4,"invalid upgrade and auto save sanitized");
   CheckEconomy();CheckSurprises();return checks;
  }
  static void CheckFirstSession(){
   var m=new IdleModel();Check(m.FirstSessionStep==0,"new profile starts with greeting");
   m.Care();Check(m.FirstSessionStep==1,"one tap opens free bowl guidance");
   m.Starter();Check(m.FirstSessionStep==2,"starter advances to passive income");
   Check(!m.BuyBed()&&m.FirstSessionStep==2,"failed purchase does not advance guidance");
   m.S.coins=100;m.UpgradeTap(0);Check(m.FirstSessionStep==2,"out of order upgrade retains missing bed step");
   m.BuyBed();Check(m.FirstSessionStep==4,"earlier tap upgrade is not requested again");
   m.StartActivity(1);m.StepActivity(10);Check(m.FirstSessionStep==4,"leaving an unfinished run does not finish onboarding");
   m.StepActivity(15);Check(m.S.activityCompleted&&m.FirstSessionStep==5,"completed activity finishes onboarding");
   m.StartActivity(0);Check(m.FirstSessionStep==5,"replay cannot regress onboarding");
   double coins=m.S.coins,growth=m.S.growth;int stars=m.S.stars;uint rng=m.S.randomState;
   for(int i=0;i<20;i++){int step=m.FirstSessionStep;m.Repair();}
   Check(m.S.coins==coins&&m.S.growth==growth&&m.S.stars==stars&&m.S.randomState==rng,"guidance never pays or rolls rewards");
   var save=new IdleSave{starter=true,growth=360,bed=3,tapMultiplier=1,activitySerial=1,activity=new IdleActivity{complete=true}};
   m=new IdleModel(save);Check(m.FirstSessionStep==5&&m.S.activityCompleted,"legacy completed activity is recognized");
   save=new IdleSave{starter=true,growth=360,bed=3,tapBonus=1,activitySerial=2,activity=new IdleActivity{complete=false}};
   m=new IdleModel(save);Check(m.FirstSessionStep==5&&m.S.activityCompleted,"legacy replay preserves earlier completion");
   save=new IdleSave{starter=true,growth=6,bed=1,activitySerial=1,activity=new IdleActivity{complete=false}};
   m=new IdleModel(save);Check(m.FirstSessionStep==3&&!m.S.activityCompleted,"legacy unfinished first run is not credited");
  }
  static void CheckEconomy(){
   var old=new IdleSave{growth=151,coins=5000,stars=17,starter=true,bed=2};
   var m=new IdleModel(old);
   Check(m.Stage==2&&m.S.growth==151&&m.S.coins==5000&&m.S.stars==17&&m.S.bed==2,"balance update preserves existing profile without automatic payout");
   Check(m.PendingGrowthGifts==1&&m.GrowthGiftCoins==25&&m.NextGrowthGift==183,"old profile eligible for finite gift");
   Check(m.ClaimGrowthGifts()&&m.S.coins==5025&&m.S.stars==18,"gift pays exact earned amount");
   m=new IdleModel(m.S);Check(!m.ClaimGrowthGifts()&&m.S.coins==5025&&m.S.stars==18,"gift receipt survives reload");
   for(int i=0;i<9;i++){
    m=Fresh();m.S.growth=IdleModel.GrowthGiftAt(i)-.01;m.ClaimGrowthGifts();
    Check(m.PendingGrowthGifts==0,"no early gift "+i);
    double coins=m.S.coins;int stars=m.S.stars;
    m.S.growth=IdleModel.GrowthGiftAt(i);Check(m.PendingGrowthGifts==1&&m.ClaimGrowthGifts(),"exact boundary gift "+i);
    Check(m.S.coins-coins==(i<3?25:i<6?100:400)&&m.S.stars-stars==1,"boundary reward "+i);
    Check(!m.ClaimGrowthGifts(),"repeated claim rejected "+i);
   }
   m=Fresh();m.S.growth=150000;
   Check(m.PendingGrowthGifts==9&&m.GrowthGiftCoins==1575&&m.NextGrowthGift==0,"skipped milestones accumulate once");
   Check(m.ClaimGrowthGifts()&&m.S.stars==9&&m.S.coins==1575&&m.S.growth==150000,"all gifts leave growth intact");
   m.Repair();Check(!m.ClaimGrowthGifts()&&m.S.growthGiftMask==511,"repair preserves finite receipts");
   for(int level=0;level<8;level++){
    m=Fresh(0);m.S.bed=level;m.S.growth=IdleModel.Growth[m.BedStage-1];m.S.coins=m.BedPrice;
    double price=m.BedPrice,rate=m.Passive,gain=m.BedIncomeGain;
    Check(gain>0&&price/gain<=7200,"marginal bed payback at most two hours "+level);
    Check(m.BuyBed()&&m.S.coins==0&&Near(m.Passive-rate,gain),"bed preview equals actual benefit "+level);
    m=Fresh();m.S.bed=level;m.S.growth=IdleModel.Growth[m.BedStage-2];m.S.coins=1e8;
    Check(!m.BuyBed()&&m.S.bed==level&&m.S.coins==1e8,"bed stage gate "+level);
   }
   m=Fresh();m.S.bed=8;m.S.coins=1e8;Check(m.BedIncomeGain==0&&!m.BuyBed(),"maximum bed cannot charge");
   // Deterministic no-bonus purchase paths, not claims about real-player duration.
   foreach(int stage in new[]{2,3,4}){
    m=Fresh();m.S.growth=IdleModel.Growth[stage-1];m.S.coins=1e6;int food=stage==2?4:stage==3?3:10,count=0;string form;
    foreach(var f in IdleModel.Forms)m.S.forms.Add(f.id);
    while(m.Stage==stage&&count<100){m.S.activeSeconds+=8;Check(m.Feed(food,out form),"progression path feeding "+stage);count++;}
    Check(count==(stage==2?8:stage==3?14:12)&&m.Stage==stage+1,"bounded no-bonus meals to next stage "+stage);
   }
  }
  static void CheckTreeProtection(){
   var m=new IdleModel(new IdleSave{starter=true,growth=150000,coins=100000});m.RefreshSurprises("2030-01-01");
   Check(m.SurprisePaidAvailable==10&&m.SurpriseBlock(10)==GardenBlock.None,"budget shows actual affordable batch");
   m.S.surpriseRun=3;Check(m.SurprisePaidAvailable==7&&m.SurpriseBlock(10)==GardenBlock.Series,"series limits explain unavailable batch");
   m.S.surpriseRun=0;m.S.surprisePaid=10;Check(m.SurprisePaidAvailable==0&&m.SurpriseBlock(1)==GardenBlock.Daily,"daily limit reason");
   m.S.surprisePaid=0;m.S.surpriseBudget=100;Check(m.SurpriseBlock(1)==GardenBlock.Budget,"frozen budget reason");
   m.S.surpriseBudget=10000;m.S.coins=599;Check(m.SurpriseBlock(1)==GardenBlock.Reserve&&m.SurprisePaidAvailable==0,"reserve protected");
   m.S.coins=600;Check(m.SurprisePaidAvailable==1&&m.CanOpenSurprise(1),"exact price plus reserve accepted");
   m.S.surpriseTickets=1;m.S.surpriseBudget=0;m.S.coins=0;m.S.surprisePaid=10;
   Check(m.CanOpenSurprise(1,true)&&!m.CanOpenSurprise(10,true),"ticket bypasses paid limits but not invalid count");
   m.S.surpriseBreakUntil=120;Check(m.SurpriseBlock(1,true)==GardenBlock.Pause,"ticket respects active break");
   m=new IdleModel(new IdleSave{starter=true,growth=150000,coins=1000000});m.RefreshSurprises("2030-01-01");
   for(int round=0;round<80;round++){
    // A ten-gift batch updates the collection after each gift, not after the whole batch.
    var before=new System.Collections.Generic.HashSet<string>(m.S.owned);m.S.surprisePaid=0;m.S.surpriseSpent=0;m.S.surpriseRun=0;m.S.surpriseBreakUntil=0;
    var batch=m.OpenSurprises(10);Check(batch!=null,"protected batch opens");
    foreach(var r in batch)if(!string.IsNullOrEmpty(r.item)){
     bool missing=m.SurprisePool(r.tier).Exists(i=>!before.Contains(i.id));
     Check(!missing||!before.Contains(r.item),"no duplicate while rarity has missing item");
     Check(r.duplicate==before.Contains(r.item),"receipt duplicate matches collection at draw");before.Add(r.item);
    }
   }
   foreach(int stage in new[]{2,3,4,5})foreach(int pity in new[]{0,9}){
    m=new IdleModel(new IdleSave{starter=true,growth=IdleModel.Growth[stage-1],surprisePity=pity});
    for(int tier=0;tier<4;tier++){
     var pool=m.SurprisePool(tier);Check(pool.Count>0,"every reachable tier has content");
     m.Grant(pool[0].id);double sum=0;foreach(var i in pool)sum+=m.SurpriseItemChance(i.id);
     Check(Near(sum,m.SurpriseItemGroupChance(tier)),"displayed item chances sum to group chance");
     Check(m.SurpriseItemChance(pool[0].id)==0,"owned item excluded until group collected");
     foreach(var i in pool)m.Grant(i.id);sum=0;foreach(var i in pool)sum+=m.SurpriseItemChance(i.id);
     Check(Near(sum,m.SurpriseItemGroupChance(tier)),"collected group retains total probability");
    }
   }
   var legacy=new IdleSave{starter=true,growth=360,coins=5000,stars=19,surprisePity=7,surprisePaid=2,surpriseBudget=1000,surpriseSpent=600,surpriseRun=3,surpriseTickets=4};
   legacy.surpriseResults.Add(new GardenReward{item="set_tree_sun",duplicate=true,stars=11,tier=3,name="Saved result"});
   m=new IdleModel(legacy);Check(m.S.coins==5000&&m.S.stars==19&&m.S.surprisePity==7&&m.S.surpriseTickets==4&&m.S.surpriseSpent==600&&m.S.surpriseResults[0].duplicate,"protection does not rewrite old rewards or balances");
  }
  static void CheckSurprises(){
   var m=new IdleModel(new IdleSave{coins=10000,growth=6});const string day="2030-01-01";
   m.RefreshSurprises(day);Check(m.S.surpriseBudget==2000&&m.SurprisePrice==200,"daily budget and stage price");
   m.S.coins+=5000;m.RefreshSurprises(day);Check(m.S.surpriseBudget==2000,"income does not inflate same-day budget");
   m.RefreshSurprises("2029-12-31");Check(m.S.surpriseDate==day,"clock rollback cannot reset spending");
   Check(m.ClaimSurpriseTicket(day)&&!m.ClaimSurpriseTicket(day)&&m.S.surpriseTickets==1,"daily ticket once");
   double coins=m.S.coins;Check(m.OpenSurprises(1,true)!=null&&m.S.coins>=coins&&m.S.surprisePaid==0&&m.S.surpriseSpent==0,"free opening does not spend coins or daily budget");
   Check(m.OpenSurprises(10)==null&&m.S.coins>=coins,"batch cannot bypass ten consecutive openings");
   m.S.surpriseRun=0;var batch=m.OpenSurprises(10);Check(batch!=null&&batch.Count==10&&m.S.surprisePaid==10&&m.S.surpriseSpent==2000,"batch is one upfront transaction");
   Check(batch.Exists(r=>r.tier>=2),"ten outcomes include guaranteed epic or legendary");
   Check(m.SurpriseWait>0&&m.OpenSurprises(1)==null,"ten opens enforce calm break");m.Tick(120);
   Check(m.SurpriseWait==0&&m.OpenSurprises(1)==null,"daily paid cap remains after break");
   m.RefreshSurprises("2030-01-02");Check(m.S.surprisePaid==0&&m.S.surpriseSpent==0&&m.S.surpriseBudget==Math.Floor(m.S.coins*.2),"next day resets spend without loss");
   Check(IdleModel.SurpriseTier(0,0)==0&&IdleModel.SurpriseTier(.6,0)==1&&IdleModel.SurpriseTier(.85,0)==2&&IdleModel.SurpriseTier(.97,0)==3,"published probability boundaries");
   Check(IdleModel.SurpriseTier(0,9)==3&&IdleModel.SurpriseTier(.03,9)==2&&IdleModel.SurpriseTier(.99,9)==2,"guarantee keeps three percent legendary");
   foreach(int stage in new[]{1,2,3,4,5}){m.S.growth=IdleModel.Growth[stage-1];foreach(int tier in new[]{0,1,2,3})foreach(var i in m.SurprisePool(tier))Check(i.RewardDay==0&&i.stage<=stage&&i.TreeOnly&&i.TreeTier==tier&&CapyCompleteSets.Visible(i),"pool excludes locked/calendar/obsolete items "+i.id);}
   m=new IdleModel(new IdleSave{growth=360,coins=10000,surpriseTickets=30});foreach(var i in IdleCatalog.Items)m.Grant(i.id);foreach(var f in IdleModel.Forms)m.S.forms.Add(f.id);
   m.S.surprisePity=9;int before=m.S.stars;var reward=m.OpenSurprises(1,true)[0];
   Check(reward.tier>=2&&reward.duplicate&&m.S.stars-before==reward.stars&&reward.stars>=6,"duplicate converts to useful stars and preserves receipt");
   Check(m.S.surprisePity==0&&m.S.surpriseResults[0]==reward,"success resets pity and receipt survives navigation");
   m=new IdleModel(new IdleSave{growth=6,stars=50});Check(m.CraftSurprise("sport")&&m.S.stars==30&&m.S.owned.Contains("sport"),"craft selected exact skin");
   Check(!m.CraftSurprise("sport")&&!m.CraftSurprise("set_dawn")&&!m.CraftSurprise("gold")&&m.S.stars==30,"craft rejects repeat calendar and locked stage");
   before=m.S.stars;Check(m.ChooseSeed(day,0)!=null&&m.S.stars>before&&m.ChooseSeed(day,1)==null,"three seeds grant once without wager");
   m.S.coins=299;m.RefreshSurprises(day);Check(!m.CanOpenSurprise(1)&&m.S.coins==299,"small wallet cannot be depleted");
   m.S.cares=100;Check(m.ClaimQuest()&&m.S.surpriseTickets==1,"quest grants extra ticket once");
   const double now=1893456000;int request=m.BeginSurpriseAd(now,day);
   Check(request>0&&!m.CompleteAutoAd(request,now)&&m.CompleteSurpriseAd(request,now,day),"surprise ad cannot count toward auto series");
   Check(!m.CompleteSurpriseAd(request,now,day)&&m.S.surpriseTickets==2&&m.S.autoAdCount==0,"surprise ad reward idempotent");m.CancelAutoAd();
   Check(m.BeginAutoAd(now)==0&&m.BeginSurpriseAd(now+120,day)==0,"shared cooldown and one daily video ticket");
   m.S.surpriseAds=false;Check(m.BeginSurpriseAd(now+86400,"2030-01-02")==0,"disabled video gifts never start");
   var counts=new int[6];var tiers=new int[4];m.S.growth=6;
   foreach(var item in IdleCatalog.Items)if(item.TreeOnly){counts[item.slot]++;tiers[item.TreeTier]++;Check(!m.Purchase(item.id)&&!m.CraftSurprise(item.id),"tree item cannot be purchased or crafted "+item.id);}
   Check(counts[0]==6&&counts[2]==6&&counts[3]==6&&counts[4]==6,"exactly six rewards per requested category");
   Check(tiers[0]==4&&tiers[1]==8&&tiers[2]==8&&tiers[3]==4,"all four categories span all rarities");
   var legacy=new IdleModel(new IdleSave{growth=6,coins=5000,stars=2,forms=new System.Collections.Generic.List<string>{"cosmos","chill"},form="cosmos"});
   Check(legacy.S.form==""&&legacy.S.forms.Count==1&&legacy.S.forms[0]=="chill"&&legacy.S.stars==12&&legacy.S.coins==5000,"cosmos retires with compensation and keeps other progress");legacy.Repair();Check(legacy.S.stars==12,"cosmos compensation is idempotent");
   for(int n=0;n<800;n++){m.S.surpriseTickets=1;m.S.surpriseRun=0;m.S.surpriseBreakUntil=0;var r=m.OpenSurprises(1,true)[0];Check(string.IsNullOrEmpty(r.form)&&(string.IsNullOrEmpty(r.item)||IdleCatalog.Find(r.item).TreeOnly),"only exclusive items drop "+n);}
   m.S.surprisePity=100;m.S.surpriseBudget=double.NaN;m.S.surpriseSpent=-9;m.S.surpriseBuffUntil=double.PositiveInfinity;m.Repair();
   Check(m.S.surprisePity==9&&m.S.surpriseBudget==0&&m.S.surpriseSpent==0&&m.S.surpriseBuffUntil==0,"damaged surprise save repaired");
  }
 }
}
