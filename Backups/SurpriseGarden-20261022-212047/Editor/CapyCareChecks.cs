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
   Check(m.Feed(IdleModel.PremiumMeal,out form)&&m.S.premiumFood==0&&m.S.coins==987&&m.S.growth==420&&m.S.fullness==60,"premium stock uses independent index");
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
   return checks;
  }
 }
}
