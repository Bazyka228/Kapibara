using System;
using Capybara67.Idle;

namespace Capybara67.Editor {
 // Pure model checks also run outside Unity; never access PlayerPrefs.
 public static class CapyCareChecks {
  static int checks;
  static void Check(bool ok,string name){checks++;if(!ok)throw new Exception("Care system: "+name);}
  static bool Near(double a,double b)=>Math.Abs(a-b)<.00001;
  static IdleModel Fresh(double value=75){var m=new IdleModel();m.S.fullness=m.S.comfort=m.S.mood=value;return m;}
  static void CheckActivityVariety(){
   var legacy=new IdleActivity{seed=4,elapsed=9};
   for(int i=0;i<8;i++)Check(Near(IdleModel.FruitX(legacy,i),.12+((i*37+76)%77)/100.0),"legacy fruit path preserved");
   Check(Near(IdleModel.FruitFallTime(legacy),3.2),"legacy speed preserved");
   var m=new IdleModel(new IdleSave{starter=true,growth=6,activityRuns=null,activityBest=null});
   for(int kind=0;kind<2;kind++)for(int run=0;run<6;run++){
    Check(m.StartActivity(kind),"variety run starts");var a=m.S.activity;
    Check(a.variant==run%3&&a.patternVersion==2,"variants rotate separately per game");
    Check(m.StartActivity(kind)&&object.ReferenceEquals(a,m.S.activity),"resume keeps pattern identity");
    Check(!m.StartActivity(1-kind),"cannot replace unfinished variant");
    if(kind==0){
     Check(Near(IdleModel.FruitX(a,0),.5),"first fruit centered");
     for(int i=0;i<8;i++){
      double x=IdleModel.FruitX(a,i);Check(x>=.12&&x<=.89,"basket landing reachable");
      m.ActivityBasket(x);m.StepActivity(i*2.7+IdleModel.FruitFallTime(a)+.001-a.elapsed);
     }
    }else for(int i=0;i<6;i++){
     double x=IdleModel.SearchX(a,i),y=IdleModel.SearchY(a,i);
     Check(x>=0&&x+80<=432&&y>=0&&y+80<=371,"search target inside field above basket");
     for(int j=0;j<i;j++)Check(Math.Abs(x-IdleModel.SearchX(a,j))>=80||Math.Abs(y-IdleModel.SearchY(a,j))>=80,"search targets do not overlap");
     Check(m.ActivityFind(i)&&!m.ActivityFind(i),"find counts once");
    }
    m.StepActivity(25);int full=kind==0?8:6;
    Check(a.complete&&a.score==full&&m.S.activityBest[kind]==full,"perfect run updates record");
    Check(a.newBest==(run==0)&&m.S.activityRuns[kind]==run+1,"record celebration only on improvement");
    double paid=m.S.coins;m.StepActivity(25);m=new IdleModel(m.S);
    Check(m.S.coins==paid&&m.S.activityRuns[kind]==run+1,"completion and reload never repay or recount");
   }
   var old=new IdleSave{starter=true,growth=6,activity=new IdleActivity{kind=1,seed=3,elapsed=9,score=1,mask=4}};
   m=new IdleModel(old);Check(m.StartActivity(1)&&m.S.activity.patternVersion==0&&m.S.activity.mask==4&&m.S.activity.elapsed==9,"old pending search preserved");
   Check(m.S.activityRuns[0]==0&&m.S.activityRuns[1]==0,"old history is not fabricated");
  }
  static void CheckRemovedCurrency(){
   var save=new IdleSave{starter=true,growth=6,coins=5000,stars=579,totalCoins=9000,mandarins=new MandarinProgress{balance=12}};
   save.surpriseResults.Add(new GardenReward{stars=11,coins=50,duplicate=true,name="Звёздный подарок"});
   var m=new IdleModel(save);
   Check(m.S.coins==19475&&m.S.stars==0&&m.M.balance==12&&m.S.totalCoins==9000,"legacy currency converts without affecting premium wallet or earned counters");
   Check(m.S.surpriseResults[0].coins==325&&m.S.surpriseResults[0].stars==0&&m.S.surpriseResults[0].name=="Монетный подарок","old receipt converts for display without paying again");
   m.Repair();m=new IdleModel(m.S);Check(m.S.coins==19475&&m.S.surpriseResults[0].coins==325,"currency conversion survives repeated load without duplicate grants");
   m=new IdleModel(new IdleSave{coins=1e12-10,stars=int.MaxValue});Check(m.S.coins==1e12&&m.S.stars==0,"currency conversion respects coin cap without integer overflow");
   m=new IdleModel(new IdleSave{coins=5000,stars=-9});Check(m.S.coins==5000&&m.S.stars==0,"invalid legacy currency cannot remove coins");
   m=new IdleModel(new IdleSave{growth=150000,coins=10000});double price=IdleCatalog.Find("space").price;
   Check(m.Purchase("space")&&m.S.coins==10000-price&&m.S.stars==0,"former currency gated scenery now needs only coins");
   m=new IdleModel(new IdleSave{starter=true,growth=6,cares=100});double reward=m.QuestReward;
   Check(m.ClaimQuest()&&m.S.coins==reward&&m.S.stars==0&&m.S.surpriseTickets==1,"quest awards coins and ticket only");
  }
  static void CheckMandarins(){
   const string day="2026-10-26";var m=new IdleModel(new IdleSave{coins=5000,growth=6,starter=true,stars=7,mandarins=null});
   m.RefreshDay(day);Check(m.M.balance==0&&m.S.coins==5175&&m.S.stars==0&&m.Stage==2,"mandarin migration leaves old economy intact");
   Check(!m.ClaimMandarins(day),"daily currency requires activity after day baseline");
   for(int i=0;i<3;i++){Check(m.CareAction(0),"free care task action");m.Tick(3);}
   Check(m.MandarinTaskDone&&m.ClaimMandarins(day)&&m.M.balance==3,"daily free three granted");
   Check(!m.ClaimMandarins(day)&&!new IdleModel(m.S).ClaimMandarins(day),"daily receipt survives model reload");
   for(int n=1;n<5;n++){
    string date=new DateTime(2026,10,26).AddDays(n*2).ToString("yyyy-MM-dd");m.RefreshDay(date);
    Check(!m.MandarinTaskDone,"new day starts fresh activity counters");
    m.StartActivity(0);m.StepActivity(25);Check(m.ClaimMandarins(date),"activity alternative accepts participation");
   }
   Check(m.M.balance==20&&m.M.claims==5,"five claims with gaps award fifteen plus five");
   Check(!m.ClaimMandarins(day)&&!m.ClaimMandarins("bad"),"clock rollback and bad date cannot claim");
   m=new IdleModel(new IdleSave{growth=6,starter=true,coins=10000});m.RefreshDay(day);string form;
   for(int n=0;n<2;n++){m.Tick(9);m.Feed(0,out form);}Check(m.MandarinTaskDone,"feeding alternative completes daily task");
   m.M.balance=200;double value=m.CareValue;Check(m.BuyMandarin("tap_boost",day)&&Near(m.ManualCareValue,value*1.25)&&Near(m.CareValue,value),"tap boost changes manual earnings only");
   int balance=m.M.balance;Check(!m.BuyMandarin("tap_boost",day)&&m.M.balance==balance,"active boost cannot be bought twice");
   m.S.autoRemaining=2;m.S.autoPhase=0;double coins=m.S.coins;m.Tick(1);Check(Near(m.S.coins-coins,m.CareValue),"autotap earns unboosted care value");
   m.S.lastUtc=10000;double left=m.MandarinTapLeft;m.Arrive(11000);Check(Near(m.MandarinTapLeft,left),"offline does not consume active boost time");
   m.Tick(600);Check(Near(m.ManualCareValue,m.CareValue),"manual boost expires");
   Check(m.BuyMandarin("care_stock",day)&&m.M.stock==3,"care stock inventory purchase");
   m.S.fullness=m.S.mood=100;Check(!m.UseMandarinStock()&&m.M.stock==3,"full needs do not consume stock");
   m.S.fullness=5;m.S.mood=7;double growth=m.S.growth;int feeds=m.S.feeds;Check(m.UseMandarinStock()&&m.M.stock==2&&m.S.fullness==45&&m.S.mood==17,"stock restores exact bounded needs");
   Check(m.S.growth==growth&&m.S.feeds==feeds&&!m.UseMandarinStock(),"stock has no growth rolls and respects animation cooldown");
   m=new IdleModel(new IdleSave{starter=true,growth=14000,coins=1000000});m.RefreshDay(day);m.S.eventDate="";m.M.balance=100;
   foreach(var f in IdleModel.Forms)m.S.forms.Add(f.id);
   Check(m.BuyMandarin("growth_boost",day),"growth supplement purchased");
   for(int n=0;n<5;n++){m.Tick(9);Check(m.Feed(10,out form),"supplement feed accepted");}
   Check(Near(m.S.growth,14000+60000+13600)&&m.M.growthCharges==0&&Near(m.M.growthLeft,0),"growth bonus cap exact across five feedings");
   Check(!m.BuyMandarin("growth_boost",day),"growth one per day");
   m.S.growth=150000;Check(!m.BuyMandarin("growth_boost",day),"no useless final-stage growth sale");
   m=new IdleModel(new IdleSave{starter=true,growth=6,coins=1000});m.RefreshDay(day);m.M.balance=100;
   coins=m.S.coins;Check(m.BuyMandarin("coin_pouch",day)&&m.S.coins-coins==100,"coin pouch floor payout");
   Check(!m.BuyMandarin("coin_pouch",day),"coin pouch daily receipt");m.S.bed=8;Check(m.MandarinCoinAmount==5000,"coin pouch upper cap");
   m.RefreshSurprises(day);for(int n=0;n<3;n++)Check(m.BuyMandarin("tree_gift",day),"mandarin tree opening");
   balance=m.M.balance;Check(!m.BuyMandarin("tree_gift",day)&&m.M.balance==balance&&m.S.surprisePaid==3&&m.S.surpriseTotal==3,"tree daily cap and shared paid counter");
   m.M.treeBought=0;m.S.surprisePaid=9;Check(m.BuyMandarin("tree_gift",day)&&!m.BuyMandarin("tree_gift",day),"tree total paid cap cannot be bypassed");
   m.S.growth=150000;m.M.balance=1000;
   foreach(var item in IdleCatalog.Items){if(MandarinItemPriceForTest(item)==0)continue;
    if(m.S.owned.Contains(item.id))continue;balance=m.M.balance;
    if(balance<IdleModel.MandarinItemPrice(item))m.M.balance=1000;
    string worn=m.S.equipped[item.slot];Check(m.BuyMandarin("item:"+item.id,day)&&m.S.owned.Contains(item.id)&&m.S.equipped[item.slot]==worn,"cosmetic grant does not equip "+item.id);
    balance=m.M.balance;Check(!m.BuyMandarin("item:"+item.id,day)&&m.M.balance==balance,"cosmetic duplicate does not charge "+item.id);
   }
   Check(!m.BuyMandarin("item:set_dawn",day)&&!m.BuyMandarin("item:set_tree_sun",day)&&!m.BuyMandarin("unknown",day),"calendar tree exclusives and invalid IDs cannot be bought directly");
   m.M.balance=0;Check(!m.BuyMandarin("care_stock",day),"insufficient balance rejected");
   foreach(var pack in IdleModel.MandarinPacks)Check(pack.price>0&&pack.bonus<=pack.amount*.25&&pack.Total==pack.amount+pack.bonus,"package bonus arithmetic");
  }
  static int MandarinItemPriceForTest(IdleItem item)=>IdleModel.MandarinItemPrice(item);
  static void CheckCurrencyPayments(){
   Check(IdleModel.MandarinPacks.Length==6,"six proposed packs");
   var ids=new System.Collections.Generic.HashSet<string>();foreach(var pack in IdleModel.MandarinPacks)Check(ids.Add(pack.id)&&pack.Total>0,"unique draft pack");
   foreach(double cost in new[]{0d,25,250,999,1000,1001,2999,12000,999999999}){
    Check(IdleModel.MandarinCost(cost)*1000d-IdleModel.MandarinChange(cost)==cost,"conversion preserves exact cost");
    Check(IdleModel.MandarinChange(cost)>=0&&IdleModel.MandarinChange(cost)<1000,"bounded coin change");
   }
   var m=new IdleModel(new IdleSave{starter=true,growth=150000,coins=0});m.M.balance=1000;
   foreach(double bad in new[]{-1d,double.NaN,double.PositiveInfinity})Check(!m.CanPay(bad)&&!m.CanPay(bad,true),"invalid prices rejected");
   m.S.coins=1e12;Check(!m.CanPay(25,true),"change cannot overflow wallet");m.S.coins=0;
   double earned=m.S.totalCoins,price=m.BedPrice;Check(m.BuyBed(true)&&m.S.bed==1&&m.M.balance==1000-IdleModel.MandarinCost(price)&&Near(m.S.coins,IdleModel.MandarinChange(price)),"bed accepts mandarins with exact change");Check(m.S.totalCoins==earned,"change is not lifetime earned income");
   price=m.TapPrice(0);double coins=m.S.coins;int fruit=m.M.balance;Check(m.UpgradeTap(0,true)&&m.M.balance==fruit-IdleModel.MandarinCost(price)&&Near(m.S.coins,coins+IdleModel.MandarinChange(price)),"tap exact mandarin debit");
   m.S.bed=0;m.S.eventDate="";string form;
   for(int i=0;i<IdleModel.Foods.Length;i++){m.Tick(10);coins=m.S.coins;fruit=m.M.balance;price=IdleModel.Foods[i].price;Check(m.Feed(i,out form,true)&&m.M.balance==fruit-IdleModel.MandarinCost(price)&&Near(m.S.coins,coins+IdleModel.MandarinChange(price)),"all foods accept same rate "+i);coins=m.S.coins;fruit=m.M.balance;Check(!m.Feed(i,out form,true)&&m.M.balance==fruit&&m.S.coins==coins,"cooldown never charges "+i);}
   foreach(var item in IdleCatalog.Items){if(!CapyCompleteSets.Visible(item)||item.TreeOnly||item.RewardDay>0||m.S.owned.Contains(item.id))continue;
    m.M.balance=1000000;m.S.coins=0;Check(m.Purchase(item.id,true)&&m.M.balance==1000000-IdleModel.MandarinCost(item.price)&&Near(m.S.coins,IdleModel.MandarinChange(item.price)),"wardrobe uniform rate "+item.id);
    fruit=m.M.balance;coins=m.S.coins;Check(m.Purchase(item.id,true)&&m.M.balance==fruit&&m.S.coins==coins,"owned re-equip free "+item.id);
   }
   Check(!m.Purchase("set_tree_sun",true)&&!m.Purchase("set_dawn",true),"exclusive items stay exclusive");
   m=new IdleModel(new IdleSave{starter=true,growth=6,coins=0});m.M.balance=10;m.RefreshSurprises("2026-10-27");
   Check(m.OpenSurprises(1,false,true)!=null&&m.M.balance==9&&m.S.surprisePaid==1&&m.S.surpriseSpent==0,"mandarins use common tree daily cap without gold budget");
   m.S.surprisePaid=10;fruit=m.M.balance;var random=m.S.randomState;Check(m.OpenSurprises(1,false,true)==null&&m.M.balance==fruit&&m.S.randomState==random,"tree limit cannot spend or reroll");
   m.S.surprisePaid=0;m.M.balance=0;m.S.coins=0;Check(!m.BuyBed(true)&&!m.UpgradeTap(0,true)&&!m.Feed(0,out form,true)&&!m.Purchase("set_sun",true),"empty wallet cannot buy");
   m.M.balance=17;m.M.stock=2;var restored=new IdleModel(m.S);Check(restored.M.balance==17&&restored.M.stock==2,"repair preserves old currency and inventory");
  }
  static void CheckPaidCare(){
   int[] prices={5,20,15,5,10,10};
   for(int action=0;action<6;action++){
    var m=new IdleModel(new IdleSave{starter=true,growth=6,coins=100});m.S.fullness=m.S.comfort=m.S.mood=20;
    var expected=new IdleModel(new IdleSave{starter=true,growth=6});expected.S.fullness=expected.S.comfort=expected.S.mood=20;expected.CareAction(action);
    Check(IdleModel.CarePrice(action)==prices[action],"fixed care price "+action);
    uint rng=m.S.randomState;double growth=m.S.growth;int feeds=m.S.feeds;
    Check(m.PurchaseCare(action)&&m.S.coins==100-prices[action],"care exact gold debit "+action);
    Check(Near(m.S.fullness,expected.S.fullness)&&Near(m.S.comfort,expected.S.comfort)&&Near(m.S.mood,expected.S.mood)&&m.S.resting==expected.S.resting,"paid care keeps intended effects "+action);
    Check(m.S.growth==growth&&m.S.feeds==feeds&&m.S.randomState==rng,"care does not buy growth or rolls "+action);
    double coins=m.S.coins;Check(!m.PurchaseCare(action)&&m.S.coins==coins,"care cooldown cannot charge twice "+action);
    m=new IdleModel(new IdleSave{starter=true,growth=6,coins=prices[action]-1});m.S.fullness=m.S.comfort=m.S.mood=20;
    Check(!m.PurchaseCare(action)&&m.S.coins==prices[action]-1&&m.S.fullness==20&&m.S.comfort==20&&m.S.mood==20,"insufficient care funds leave all needs unchanged "+action);
    m.S.coins=0;m.M.balance=1;Check(m.PurchaseCare(action,true)&&m.M.balance==0&&m.S.coins==1000-prices[action],"care alternative currency gives exact change "+action);
   }
   var invalid=new IdleModel(new IdleSave{coins=100});Check(!invalid.PurchaseCare(-1)&&!invalid.PurchaseCare(6)&&invalid.S.coins==100,"invalid care IDs cannot charge");
   Check(invalid.PurchaseCare(5),"rest starts once");invalid.Tick(3);double balance=invalid.S.coins;Check(!invalid.PurchaseCare(5)&&invalid.S.coins==balance,"active rest cannot be bought again");
  }
  static void CheckSixActivities(){
   var quickSearch=new IdleModel(new IdleSave{starter=true,growth=6});quickSearch.StartActivity(1);quickSearch.StepActivity(2);for(int i=0;i<6;i++)quickSearch.ActivityFind(i);quickSearch.StepActivity(.2);Check(!quickSearch.S.activity.complete,"last found fruit keeps its short flight");quickSearch.StepActivity(.6);Check(quickSearch.S.activity.complete&&quickSearch.S.activity.elapsed<3,"finished search does not wait on empty field");
   Check(IdleModel.MusicNote(new IdleActivity{seed=int.MinValue},0)>=0,"malformed seed cannot index a negative note");
   var old=new IdleModel(new IdleSave{starter=true,growth=6,activityRuns=new[]{7,11},activityBest=new[]{8,5},activity=new IdleActivity{kind=1,elapsed=4,mask=2,score=1}});
   Check(old.S.activityRuns.Length==6&&old.S.activityRuns[0]==7&&old.S.activityRuns[1]==11&&old.S.activityBest[1]==5&&old.S.activity.mask==2,"six-game migration preserves history and pending run");
   Check(!old.StartActivity(-1)&&!old.StartActivity(6),"six-game range enforced");
   for(int kind=2;kind<6;kind++)for(int variant=0;variant<3;variant++){
    var m=new IdleModel(new IdleSave{starter=true,growth=6});m.S.activityRuns[kind]=variant;
    Check(m.StartActivity(kind),"new activity starts "+kind);var a=m.S.activity;Check(a.variant==variant,"new game variant rotates");
    Check(!m.StartActivity((kind+1)%6),"pending game cannot be overwritten");
    if(kind==2){
     Check(IdleModel.MemoryPreview(a)&&!m.ActivityCard(0),"memory preview is read only");m.StepActivity(2);
     int second=1;while(IdleModel.MemoryFace(a,second)==IdleModel.MemoryFace(a,0))second++;
     Check(m.ActivityCard(0)&&!m.ActivityCard(0)&&m.ActivityCard(second),"memory distinct selections");Check(a.score==0&&!m.ActivityCard(5),"mismatch reveal blocks rapid extra cards");m.StepActivity(1);
     for(int face=0;face<3;face++){for(int n=0;n<6;n++)if(IdleModel.MemoryFace(a,n)==face)Check(m.ActivityCard(n),"matching memory face");m.StepActivity(1);}
    }else if(kind==3){
     Check(!m.ActivityNote(0),"cannot input during demonstration");
     m.StepActivity(IdleModel.MusicLength(a)*.7+a.sequenceAt+.3);
     Check(!m.ActivityNote((IdleModel.MusicNote(a,0)+1)%4)&&a.score==0,"wrong note replays without penalty or reward");
     for(int round=0;round<3;round++){
      m.StepActivity(a.sequenceAt+IdleModel.MusicLength(a)*.7+.3-a.elapsed);
      int length=IdleModel.MusicLength(a);for(int n=0;n<length;n++)Check(m.ActivityNote(IdleModel.MusicNote(a,n)),"correct melody note");
     }m.StepActivity(.1);
    }else if(kind==4){
     Check(!m.ActivitySort((IdleModel.PicnicGroup(a)+1)%3)&&a.score==0,"wrong food category leaves score alone");
     for(int n=0;n<6;n++){Check(m.ActivitySort(IdleModel.PicnicGroup(a)),"correct sorting");m.StepActivity(.5);}
    }else{
     for(int n=0;n<5;n++)Check(m.ActivityPaint(n,n),"paint distinct petals");Check(!m.FinishPainting(),"incomplete flower cannot finish");
     Check(m.ActivityPaint(0,4)&&a.score==5,"repaint is creative not extra score");Check(!m.ActivityPaint(6,0)&&!m.ActivityPaint(0,5),"invalid painting inputs rejected");
     Check(m.ActivityPaint(5,2),"sixth petal");m.StepActivity(3);Check(m.FinishPainting()&&!m.FinishPainting(),"finish artwork once");
    }
    Check(a.complete&&a.score==IdleModel.ActivityGoal(kind)&&a.reward>0&&m.S.activityRuns[kind]==variant+1,"perfect completion pays and records "+kind);
    double coins=m.S.coins;m.StepActivity(100);m=new IdleModel(m.S);m.StepActivity(100);Check(m.S.coins==coins,"reloaded finished game never repays "+kind);
    Check(!m.ActivityCard(0)&&!m.ActivityNote(0)&&!m.ActivitySort(0)&&!m.ActivityPaint(0,0),"completed game ignores input");
    Check(m.StartActivity(kind),"replay starts after receipt");m.StepActivity(100);Check(m.S.activity.complete&&m.S.activity.score==0&&m.S.activity.reward>0,"timeout gives small participation reward "+kind);
   }
   var painter=new IdleModel(new IdleSave{starter=true,growth=6});painter.StartActivity(5);painter.ActivityPaint(3,4);painter.S.activity.selection=2;var restored=new IdleModel(painter.S);Check(restored.S.activity.colors==5<<(3*3)&&restored.S.activity.selection==2,"art colours and palette preserved");
  }
  static void CheckActivityPolish(){
   var decks=new System.Collections.Generic.HashSet<int>();
   for(int seed=0;seed<60;seed++){var m=new IdleModel(new IdleSave{starter=true,growth=6,activitySerial=seed});m.StartActivity(2);var a=m.S.activity;decks.Add(a.deck);int[] count=new int[3];for(int n=0;n<6;n++)count[IdleModel.MemoryFace(a,n)]++;Check(count[0]==2&&count[1]==2&&count[2]==2,"shuffle always contains three complete pairs");int deck=a.deck;new IdleModel(m.S);Check(a.deck==deck,"shuffle preserved on repair");}Check(decks.Count>12,"memory has many distinct layouts");
   var old=new IdleActivity{kind=2,patternVersion=1,seed=3,variant=1};Check(IdleModel.MemoryFace(old,0)==2&&!IdleModel.MemoryPreview(old),"old card layout and no preview preserved");
   var music=new IdleModel(new IdleSave{starter=true,growth=6});music.StartActivity(3);Check(!music.RepeatMusic(),"repeat disabled during demonstration");music.StepActivity(3);music.ActivityNote(IdleModel.MusicNote(music.S.activity,0));Check(music.RepeatMusic()&&music.S.activity.step==0&&music.S.activity.helps==1&&music.S.activity.score==0,"repeat restarts only current tune");Check(!music.RepeatMusic(),"cannot stack replay requests");
   var search=new IdleModel(new IdleSave{starter=true,growth=6});search.StartActivity(1);Check(search.HintActivity()&&!search.HintActivity(),"search hint has cooldown");int hint=IdleModel.SearchHint(search.S.activity);search.ActivityFind(hint);Check(IdleModel.SearchHint(search.S.activity)!=hint,"helper always points to unfound fruit");
   var picnic=new IdleModel(new IdleSave{starter=true,growth=6});picnic.StartActivity(4);Check(picnic.ActivitySort(IdleModel.PicnicGroup(picnic.S.activity)),"picnic serving accepted");Check(!picnic.ActivitySort(IdleModel.PicnicGroup(picnic.S.activity))&&picnic.S.activity.score==1,"flight cannot accept a second food");picnic.StepActivity(.5);Check(picnic.ActivitySort(IdleModel.PicnicGroup(picnic.S.activity)),"next serving after flight");
   var paint=new IdleModel(new IdleSave{starter=true,growth=6});paint.StartActivity(5);paint.S.activity.brush=2;paint.ActivityPaint(3,4);Check(((paint.S.activity.patterns>>6)&3)==2,"brush pattern stored per region");paint.S.activity.brush=1;paint.ActivityPaint(2,0);new IdleModel(paint.S);Check(((paint.S.activity.patterns>>6)&3)==2&&((paint.S.activity.patterns>>4)&3)==1&&paint.S.activity.score==2,"patterns survive alongside colours");
   var basket=new IdleModel(new IdleSave{starter=true,growth=6});basket.StartActivity(0);var b=basket.S.activity;for(int n=0;n<3;n++){basket.ActivityBasket(IdleModel.FruitX(b,n));basket.StepActivity(n*2.7+IdleModel.FruitFallTime(b)+.001-b.elapsed);}Check(b.combo==3&&b.bestCombo==3,"consecutive catches build a series");basket.ActivityBasket(0);basket.StepActivity(3*2.7+IdleModel.FruitFallTime(b)+.001-b.elapsed);Check(b.combo==0&&b.bestCombo==3&&b.score==3,"miss keeps best series and collected fruit");
  }
  public static int Run(){
   checks=0;
   CheckActivityPolish();
   CheckFirstSession();
   CheckTreeProtection();
   CheckActivityVariety();
   CheckMandarins();
   CheckCurrencyPayments();
   CheckPaidCare();
   CheckSixActivities();
   CheckRemovedCurrency();
   var old=new IdleSave{coins=5000,growth=6,dailyCount=7,starter=true,bed=2,stars=17,feeds=6,activeSeconds=900};
   var m=new IdleModel(old);
   Check(m.S.careVersion==1&&m.S.fullness==75&&m.S.mood==75&&m.S.comfort==75,"legacy needs initialized once");
   Check(m.S.coins==5425&&m.S.growth==6&&m.S.dailyCount==7&&m.S.stars==0&&m.S.feeds==6&&m.S.activeSeconds==900,"legacy progression unchanged");
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
   Check(m.Stage==2&&m.S.growth==151&&m.S.coins==5425&&m.S.stars==0&&m.S.bed==2,"balance update preserves existing profile without automatic payout");
   Check(m.PendingGrowthGifts==1&&m.GrowthGiftCoins==50&&m.NextGrowthGift==183,"old profile eligible for finite gift");
   Check(m.ClaimGrowthGifts()&&m.S.coins==5475&&m.S.stars==0,"gift pays exact earned amount");
   m=new IdleModel(m.S);Check(!m.ClaimGrowthGifts()&&m.S.coins==5475&&m.S.stars==0,"gift receipt survives reload");
   for(int i=0;i<9;i++){
    m=Fresh();m.S.growth=IdleModel.GrowthGiftAt(i)-.01;m.ClaimGrowthGifts();
    Check(m.PendingGrowthGifts==0,"no early gift "+i);
    double coins=m.S.coins;int stars=m.S.stars;
    m.S.growth=IdleModel.GrowthGiftAt(i);Check(m.PendingGrowthGifts==1&&m.ClaimGrowthGifts(),"exact boundary gift "+i);
    Check(m.S.coins-coins==(i<3?50:i<6?125:425)&&m.S.stars==0,"boundary reward "+i);
    Check(!m.ClaimGrowthGifts(),"repeated claim rejected "+i);
   }
   m=Fresh();m.S.growth=150000;
   Check(m.PendingGrowthGifts==9&&m.GrowthGiftCoins==1800&&m.NextGrowthGift==0,"skipped milestones accumulate once");
   Check(m.ClaimGrowthGifts()&&m.S.stars==0&&m.S.coins==1800&&m.S.growth==150000,"all gifts leave growth intact");
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
   m=new IdleModel(legacy);Check(m.S.coins==5475&&m.S.stars==0&&m.S.surprisePity==7&&m.S.surpriseTickets==4&&m.S.surpriseSpent==600&&m.S.surpriseResults[0].duplicate,"protection does not rewrite old rewards or balances");
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
   m.S.surprisePity=9;double before=m.S.coins;var reward=m.OpenSurprises(1,true)[0];
   Check(reward.tier>=2&&reward.duplicate&&m.S.coins-before==reward.coins&&reward.coins>=150&&m.S.stars==0,"duplicate converts to coins and preserves receipt");
   Check(m.S.surprisePity==0&&m.S.surpriseResults[0]==reward,"success resets pity and receipt survives navigation");
   m=new IdleModel(new IdleSave{growth=6,coins=1250});Check(m.Purchase("sport")&&m.S.coins==1250-IdleCatalog.Find("sport").price&&m.S.owned.Contains("sport"),"coin purchase grants selected skin");
   before=m.S.coins;Check(m.Purchase("sport")&&!m.Purchase("set_dawn")&&!m.Purchase("gold")&&m.S.coins==before,"equip repeat free and special purchases rejected");
   before=m.S.coins;Check(m.ChooseSeed(day,0)!=null&&m.S.coins>before&&m.S.stars==0&&m.ChooseSeed(day,1)==null,"three seeds grant once without wager");
   m.S.coins=299;m.RefreshSurprises(day);Check(!m.CanOpenSurprise(1)&&m.S.coins==299,"small wallet cannot be depleted");
   m.S.cares=100;Check(m.ClaimQuest()&&m.S.surpriseTickets==1,"quest grants extra ticket once");
   const double now=1893456000;int request=m.BeginSurpriseAd(now,day);
   Check(request>0&&!m.CompleteAutoAd(request,now)&&m.CompleteSurpriseAd(request,now,day),"surprise ad cannot count toward auto series");
   Check(!m.CompleteSurpriseAd(request,now,day)&&m.S.surpriseTickets==2&&m.S.autoAdCount==0,"surprise ad reward idempotent");m.CancelAutoAd();
   Check(m.BeginAutoAd(now)==0&&m.BeginSurpriseAd(now+120,day)==0,"shared cooldown and one daily video ticket");
   m.S.surpriseAds=false;Check(m.BeginSurpriseAd(now+86400,"2030-01-02")==0,"disabled video gifts never start");
   var counts=new int[6];var tiers=new int[4];m.S.growth=6;
   foreach(var item in IdleCatalog.Items)if(item.TreeOnly){counts[item.slot]++;tiers[item.TreeTier]++;Check(!m.Purchase(item.id)&&IdleModel.MandarinItemPrice(item)==0,"tree item cannot be purchased or crafted "+item.id);}
   Check(counts[0]==6&&counts[2]==6&&counts[3]==6&&counts[4]==6,"exactly six rewards per requested category");
   Check(tiers[0]==4&&tiers[1]==8&&tiers[2]==8&&tiers[3]==4,"all four categories span all rarities");
   var legacy=new IdleModel(new IdleSave{growth=6,coins=5000,stars=2,forms=new System.Collections.Generic.List<string>{"cosmos","chill"},form="cosmos"});
   Check(legacy.S.form==""&&legacy.S.forms.Count==1&&legacy.S.forms[0]=="chill"&&legacy.S.stars==0&&legacy.S.coins==5300,"cosmos retires with compensation and keeps other progress");legacy.Repair();Check(legacy.S.stars==0&&legacy.S.coins==5300,"cosmos compensation is idempotent");
   for(int n=0;n<800;n++){m.S.surpriseTickets=1;m.S.surpriseRun=0;m.S.surpriseBreakUntil=0;var r=m.OpenSurprises(1,true)[0];Check(string.IsNullOrEmpty(r.form)&&(string.IsNullOrEmpty(r.item)||IdleCatalog.Find(r.item).TreeOnly),"only exclusive items drop "+n);}
   m.S.surprisePity=100;m.S.surpriseBudget=double.NaN;m.S.surpriseSpent=-9;m.S.surpriseBuffUntil=double.PositiveInfinity;m.Repair();
   Check(m.S.surprisePity==9&&m.S.surpriseBudget==0&&m.S.surpriseSpent==0&&m.S.surpriseBuffUntil==0,"damaged surprise save repaired");
  }
 }
}
