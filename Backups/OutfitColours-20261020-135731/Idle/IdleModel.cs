using System;
using System.Collections.Generic;
namespace Capybara67.Idle
{
    [Serializable] public sealed class IdleSave
    {
        public int wardrobeVersion;
        public int version=1,bed,feeds,cares,pity,dailyCount,quest,stars,premiumFood;
        public double coins,growth,activeSeconds,lastCare=-100,lastFeed=-100,lastRoll=-100,lastUtc,totalCoins;
        public uint randomState=2463534242;
        public bool starter,sound=true,calm,memeVisuals=true;
        public string dailyDate="",form="",eventDate="",visitorDate="";
        public List<string> owned=new List<string>(),forms=new List<string>();
        public string[] equipped={"","","moon","","","classic"};
    }
    public sealed class IdleFood
    {
        public string name,color;public double price,growth;public int stage;
        public IdleFood(string n,string c,double p,double g,int s){name=n;color=c;price=p;growth=g;stage=s;}
    }
    public sealed class IdleForm
    {
        public string id,name,color,bonus;public int art;
        public IdleForm(string i,string n,string c,string b,int a){id=i;name=n;color=c;bonus=b;art=a;}
    }
    public sealed class IdleModel
    {
        public readonly IdleSave S;
        public const int StageCount=5;
        // Growth is stored as points, not an age index. Existing saves need no destructive migration.
        public static readonly double[] Growth={0,6,360,14000,150000};
        public static readonly string[] Stages={"Новорождённый","Малыш","Любопытный","Молодой взрослый","Хранитель сада"};
        // Original atlas poses retained by the five-stage design: 1, 2, 3, 6, 9.
        public static int SourcePose(int stage)=>stage<=3?Math.Max(1,stage):stage==4?6:9;
        public static int StageFromLegacy(int stage)=>stage<=2?Math.Max(1,stage):stage<=5?3:stage<=8?4:5;
        public static readonly double[] BedPrices={10,100,700,3000,7000,14000,28000,56000};
        static readonly double[] Rates={0,.2,.35,.55,.85,1.3,2,3,4.5};
        static readonly double[] CareBase={1,1.15,1.3,1.75,2.2};
        static readonly double[] VisitorGifts={30,40,50,80,110};
        public static readonly IdleFood[] Foods={new IdleFood("Зелёная миска","77C6A5",10,2,1),new IdleFood("Хрустящий обед","FFAD62",50,12,2),new IdleFood("Мандариновый микс","FF9F1C",150,45,3),new IdleFood("Пикник","ED9BAD",800,250,3)};
        public static readonly IdleForm[] Forms={
            new IdleForm("sixseven","Шесть-Семыч","DDA779","+7% к пассиву",0),new IdleForm("chill","Капи на чиле","B6A79F","+10% офлайн",1),
            new IdleForm("grin","Улыбака","E5B28C","+8% за заботу",2),new IdleForm("pixelpuff","Пиксель-Пух","CBAA79","+5% роста от еды",3),
            new IdleForm("cosmos","Капи-Космос","A6A2D4","+6% к пассиву",4),new IdleForm("disco","Диско-Топот","DCA5C8","+6% к монетам",5),
            new IdleForm("ninja","Мандариндзюцу","BACA96","+8% роста от фруктов",6),new IdleForm("professor","Профессор Плюх","BFAE96","+10% за задачи",7)};
        public IdleModel(IdleSave save=null){S=save??new IdleSave();Repair();}
        static double Clean(double x,double max=1e12)=>double.IsNaN(x)||double.IsInfinity(x)?0:Math.Max(0,Math.Min(max,x));
        public void Repair(){
            S.coins=Clean(S.coins);S.growth=Clean(S.growth,1e9);S.activeSeconds=Clean(S.activeSeconds);S.lastUtc=Clean(S.lastUtc,253402300799);
            S.bed=Math.Max(0,Math.Min(8,S.bed));S.pity=Math.Max(0,Math.Min(66,S.pity));S.feeds=Math.Max(0,S.feeds);S.cares=Math.Max(0,S.cares);
            S.stars=Math.Max(0,S.stars);S.premiumFood=Math.Max(0,S.premiumFood);S.dailyCount=Math.Max(0,S.dailyCount);S.quest=Math.Max(0,S.quest);S.totalCoins=Clean(S.totalCoins);
            if(S.randomState==0)S.randomState=2463534242;
            if(S.owned==null)S.owned=new List<string>();if(S.forms==null)S.forms=new List<string>();
            S.owned.RemoveAll(id=>IdleCatalog.Find(id)==null);S.forms.RemoveAll(id=>Form(id)==null);
            S.owned=new List<string>(new HashSet<string>(S.owned));S.forms=new List<string>(new HashSet<string>(S.forms));
            Grant("moon");Grant("classic");
            if(S.equipped==null || S.equipped.Length!=6)S.equipped=new[]{"","","moon","","","classic"};
            for(int n=0;n<6;n++){var i=IdleCatalog.Find(S.equipped[n]);if(i==null || i.slot!=n || !S.owned.Contains(i.id))S.equipped[n]=n==2?"moon":n==5?"classic":"";}
            CapyCompleteSets.Migrate(S);GrantDailyMilestones();
            if(!S.forms.Contains(S.form??""))S.form="";
            if(double.IsNaN(S.lastCare)||double.IsInfinity(S.lastCare))S.lastCare=S.activeSeconds;
            if(double.IsNaN(S.lastFeed)||double.IsInfinity(S.lastFeed))S.lastFeed=S.activeSeconds;
            if(double.IsNaN(S.lastRoll)||double.IsInfinity(S.lastRoll))S.lastRoll=S.activeSeconds;
            S.lastCare=Math.Min(S.activeSeconds,S.lastCare);S.lastFeed=Math.Min(S.activeSeconds,S.lastFeed);S.lastRoll=Math.Min(S.activeSeconds,S.lastRoll);
            if(!ValidDate(S.eventDate,out _))S.eventDate="";if(!ValidDate(S.visitorDate,out _))S.visitorDate="";
        }
        public int Stage {get{int n=1;while(n<Growth.Length && S.growth>=Growth[n])n++;return n;}}
        public double StageFraction=>Stage==StageCount?1:(S.growth-Growth[Stage-1])/(Growth[Stage]-Growth[Stage-1]);
        public static IdleForm Form(string id){foreach(var f in Forms)if(f.id==id)return f;return null;}
        public int FormArt=>Form(S.form)?.art??-1;
        double OutfitBonus(int slot){var i=IdleCatalog.Find(S.equipped[slot==1&&CapyCompleteSets.IsSet(S.equipped[0])?0:slot]);return i==null?0:i.bonus;}
        public double CareValue=>(CareBase[Stage-1]+S.bed*.25)*(1+OutfitBonus(0)+OutfitBonus(2)+(FormArt==2?.08:FormArt==5?.06:0));
        public double Passive=>Rates[S.bed]*(1+OutfitBonus(1)+OutfitBonus(2)+(FormArt==0?.07:FormArt==4||FormArt==5?.06:0));
        public double BedPrice=>S.bed<8?BedPrices[S.bed]:0;
        public int BedStage=>StageFromLegacy(Math.Min(9,S.bed+2));
        public bool CanBed=>S.bed<8 && Stage>=BedStage && S.coins>=BedPrice;
        public double FeedWait=>Math.Max(0,8-(S.activeSeconds-S.lastFeed));
        public double RollWait=>Math.Max(0,60-(S.activeSeconds-S.lastRoll));
        public int PityLimit=>S.forms.Count==0?12:67;
        public void Tick(double dt){if(double.IsNaN(dt)||double.IsInfinity(dt)||dt<=0)return;S.activeSeconds+=dt;Earn(Passive*dt);}
        public bool Care(){if(S.activeSeconds-S.lastCare<.5-1e-8)return false;S.lastCare=S.activeSeconds;S.cares++;Earn(CareValue);return true;}
        void Earn(double value){S.coins=Math.Min(1e12,S.coins+value);S.totalCoins=Math.Min(1e12,S.totalCoins+value);}
        public bool BuyBed(){if(!CanBed)return false;S.coins-=BedPrice;S.bed++;return true;}
        public bool Starter(){if(S.starter)return false;S.starter=true;S.growth+=6;S.feeds++;S.lastFeed=S.activeSeconds;return true;}
        public double MealGrowth(int index){if(index<0||index>4)return 0;double growth=index==4?60:Foods[index].growth;
            return growth*(1+(FormArt==3?.05:FormArt==6&&index==2?.08:0)+(index==FeaturedFood?.1:0));}
        public bool Feed(int index,out string newForm){
            newForm=null;if(FeedWait>1e-8)return false;
            if(index==4){if(S.premiumFood<=0 || Stage<3)return false;S.premiumFood--;}
            else {if(index<0||index>=Foods.Length)return false;var food=Foods[index];if(Stage<food.stage||S.coins<food.price)return false;S.coins-=food.price;}
            S.growth+=MealGrowth(index);S.feeds++;S.lastFeed=S.activeSeconds;
            if(Stage>=3 && RollWait<=1e-8 && S.forms.Count<Forms.Length){
                S.lastRoll=S.activeSeconds;bool success=Next()<.02 || S.pity>=PityLimit-1;
                if(success){var left=new List<IdleForm>();foreach(var f in Forms)if(!S.forms.Contains(f.id))left.Add(f);
                    newForm=left[Math.Min(left.Count-1,(int)(Next()*left.Count))].id;S.forms.Add(newForm);S.form=newForm;S.pity=0;
                }else S.pity++;
            }return true;
        }
        double Next(){uint x=S.randomState;x^=x<<13;x^=x>>17;x^=x<<5;S.randomState=x;return x/4294967296.0;}
        public double Arrive(double utc){
            utc=Clean(utc,253402300799);if(utc<=S.lastUtc)return 0;
            double seconds=S.lastUtc<=0?0:Math.Min(14400,utc-S.lastUtc);S.lastUtc=utc;
            double award=seconds*Passive*(FormArt==1?1.1:1);Earn(award);return award;
        }
        public void Stamp(double utc){if(utc>S.lastUtc)S.lastUtc=utc;}
        public void Grant(string id){if(IdleCatalog.Find(id)!=null&&!S.owned.Contains(id))S.owned.Add(id);}
        public bool Purchase(string id){var i=IdleCatalog.Find(id);if(i==null)return false;
            if(!S.owned.Contains(id)){if(i.RewardDay>0 || Stage<i.stage || S.coins<i.price || S.stars<i.stars)return false;S.coins-=i.price;S.stars-=i.stars;Grant(id);}
            S.equipped[i.slot]=id;
            if(i.IsSet)S.equipped[1]="";
            else if(i.slot==5&&CapyCompleteSets.IsSet(S.equipped[0]))S.equipped[0]="";
            return true;
        }
        public bool SelectForm(string id){if(id!=""&&!S.forms.Contains(id))return false;S.form=id;return true;}
        static bool ValidDate(string date,out DateTime parsed)=>DateTime.TryParseExact(date,"yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out parsed);
        public bool RefreshDay(string date){if(!ValidDate(date,out _)||string.CompareOrdinal(date,S.eventDate)<=0)return false;S.eventDate=date;return true;}
        public int FeaturedFood=>ValidDate(S.eventDate,out var day)?(int)(day.Ticks/TimeSpan.TicksPerDay%4):-1;
        public int VisitorKind=>ValidDate(S.eventDate,out var day)?(int)(day.Ticks/TimeSpan.TicksPerDay%3):0;
        public string VisitorName=>new[]{"Лучик","Плюшка","Тихоня"}[VisitorKind];
        public bool CanVisit=>S.feeds>=3&&ValidDate(S.eventDate,out _)&&string.CompareOrdinal(S.visitorDate,S.eventDate)<0;
        public double VisitorReward=>VisitorGifts[Stage-1];
        public bool WelcomeVisitor(){if(!CanVisit)return false;S.visitorDate=S.eventDate;Earn(VisitorReward);return true;}
        public void GrantDailyMilestones(){if(S.dailyCount>=3)Grant("firefly_garden");if(S.dailyCount>=7)Grant("set_dawn");}
        public bool ClaimDaily(string utcDate){
            if(S.feeds==0 || !DateTime.TryParseExact(utcDate,"yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out var parsed) || string.CompareOrdinal(S.dailyDate,utcDate)>=0)return false;
            S.dailyDate=utcDate;int day=S.dailyCount++%7;Earn(new[]{25,50,100,150,250,400,600}[day]);if(day==6){S.premiumFood++;S.stars+=3;}GrantDailyMilestones();return true;
        }
        public int QuestType=>S.quest%5;
        public int QuestGoal=>QuestType==0?10+(S.quest/5)*100:QuestType==1?3+(S.quest/5)*15:QuestType==2?Math.Min(8,1+S.quest/5):QuestType==3?Math.Min(52,5+S.quest/5*4):StageFromLegacy(Math.Min(9,3+S.quest/5));
        public int QuestValue=>QuestType==0?S.cares:QuestType==1?S.feeds:QuestType==2?S.bed:QuestType==3?S.owned.Count:Stage;
        public bool QuestDone=>S.quest<40 && QuestValue>=QuestGoal;
        public string QuestName=>S.quest>=40?"Все поручения выполнены":new[]{"Погладь капибару","Накорми капибару","Улучши подстилку","Собери предметы","Достигни стадии"}[QuestType];
        public double QuestReward=>Math.Min(1000,20+S.quest*25)*(FormArt==7?1.1:1);
        public bool ClaimQuest(){if(!QuestDone)return false;Earn(QuestReward);S.stars++;S.quest++;return true;}
    }
}


