using System;
using System.Collections.Generic;
namespace Capybara67
{
    public enum ItemSlot { Head, Body, Scene, Trail, Emotion, Skin }
    public enum UnlockRule { Free, Rounds, Hits, Mission, Combo, PerfectCombo, PerfectHits, Collection }
    public sealed class CapyItem
    {
        public string id,name,color; public ItemSlot slot; public UnlockRule rule; public int target,rarity,art;
        public string Requirement => rule==UnlockRule.Free?"Доступно всем":rule==UnlockRule.Rounds?"Раундов: "+target:rule==UnlockRule.Hits?"Ловель всего: "+target:rule==UnlockRule.Mission?"Поручений: "+target:rule==UnlockRule.Combo?"Серия: "+target:rule==UnlockRule.PerfectCombo?"Идеальных подряд: "+target:rule==UnlockRule.PerfectHits?"Идеальных всего: "+target:"Предметов: "+target;
        public string Rarity => rarity==0?"ОБЫЧНЫЙ":rarity==1?"РЕДКИЙ":rarity==2?"ЭПИЧЕСКИЙ":"ЛЕГЕНДАРНЫЙ";
    }
    public static class CapyCatalog
    {
        // Stable IDs are deliberately independent from translated names and artwork.
        const string Data=@"panama|Панама|0|0|91DDC0|1|1|0
cap|Кепка «Лови!»|0|1|E88C77|2|30|0
crown|Листовая корона|0|2|A0CF82|4|14|1
duck|Уточка-пассажир|0|3|FFE08A|2|60|0
detective|Шляпа следопыта|0|4|CBB38C|3|5|0
phones|Листовой бит|0|5|9F9DE6|5|4|1
ushanka|Тёплые уши|0|6|C2CADF|2|100|0
wreath|Венок лотосов|0|7|EAAAC8|2|80|0
helmet|Мастер сада|0|8|F6BF65|3|8|0
chef|Поварской колпак|0|9|F5EDDC|2|120|0
boat|Бумажный кораблик|0|10|C6DEEE|1|2|0
glasses|Невозмутимость|0|11|EAD08F|4|14|1
scarf|Мандариновый шарф|1|0|EB9D76|2|25|0
tie|Важный день|1|1|DA849D|2|50|0
apron|Садовый повар|1|2|DCE3B7|3|3|0
cape|Листовой герой|1|3|7FC0A2|4|14|1
sweater|Три волны|1|4|B6A9DC|2|140|0
shirt67|Майка 6–7|1|5|F0C770|4|14|1
hoodie|Лунный сад|1|6|8795BC|2|180|0
raincoat|Дождевик «Капля»|1|7|F2D06F|3|10|0
armor|Картонные доспехи|1|8|C7A07B|5|5|1
orangeShirt|Футболка «Долька»|1|9|9FD2C0|2|40|0
moon|Лунный пруд|2|0|30384D|0|0|0
sunset|Персиковый закат|2|1|76506B|2|100|0
jungle|Тропический сад|2|2|294A45|3|6|0
space|Орбитальный огород|2|3|302A57|5|6|2
room|Комната коллекционера|2|4|554966|7|8|0
kitchen|Кухня с мандаринами|2|5|635763|2|160|0
snow|Снежный двор|2|6|56758C|4|14|1
beach|Тихий пляж|2|7|486D80|3|12|0
rainbow|Радужная лента|3|0|A4E3CD|4|7|0
stars|Звёздная пыль|3|1|F8DA94|5|4|1
hearts|Сердечки дружбы|3|2|F0A6BD|2|200|0
pixels|Пиксельные искры|3|3|8EDAD8|4|14|1
fire|Янтарный огонёк|3|4|F8AA68|5|6|2
digits|Цифры 6–7|3|5|C6B0EF|4|21|2
laugh|Тихий смешок|4|0|F1C0AC|3|1|0
wow|Ого!|4|1|EFD597|6|20|0
yawn|Сонный зевок|4|2|C6B4E1|2|70|0
joy|Получилось!|4|3|AFDFC8|4|14|1
brow|Скептическая бровь|4|4|D0B8A3|5|4|1
smile|Солнечная улыбка|4|5|F2D477|5|7|2
classic|Мандаринка|5|0|C98A52|0|0|0
sport|Пружинка|5|1|B96E4E|2|100|0
sleepy|Тихоня|5|2|B5A29A|2|250|0
neon|Люми|5|3|695787|4|14|1
pixel|Кубик|5|4|CFA66A|5|5|1
sunny|Лучик|5|5|E5BD5D|5|7|2
cosmic|Орбита|5|6|AAA9C8|4|21|2
fluffy|Плюшка|5|7|EADBC0|2|400|0
robot|Бип|5|8|91ADBD|4|14|1
gold|Золотой час|5|9|D9A83E|5|14|3";
        public static readonly CapyItem[] Items=Build();
        static CapyItem[] Build(){var list=new List<CapyItem>();foreach(var row in Data.Split('\n')){var a=row.Trim().Split('|');list.Add(new CapyItem{id=a[0],name=a[1],slot=(ItemSlot)int.Parse(a[2]),art=int.Parse(a[3]),color=a[4],rule=(UnlockRule)int.Parse(a[5]),target=int.Parse(a[6]),rarity=int.Parse(a[7])});}return list.ToArray();}
        public static CapyItem Find(string id){foreach(var i in Items)if(i.id==id)return i;return null;}
    }
    [Serializable] public sealed class CapyProgress
    {
        public int version=2,rounds,hits,perfects,missions,bestCombo,bestPerfect,silhouette;
        public bool goldenCrown,reducedEffects;
        public string dailyDate="";
        public List<string> owned=new List<string>();
        public string[] equipped={"","","moon","","","classic"};
        public void Repair()
        {
            if(owned==null)owned=new List<string>();
            if(equipped==null || equipped.Length!=6)equipped=new[]{"","","moon","","","classic"};
            Grant("moon");Grant("classic");
            for(int s=0;s<6;s++){var i=CapyCatalog.Find(equipped[s]);if(i==null || (int)i.slot!=s || !Has(i.id))equipped[s]=s==2?"moon":s==5?"classic":"";}
            silhouette=Math.Max(0,Math.Min(2,silhouette));
        }
        public bool Has(string id)=>owned.Contains(id);
        public bool Grant(string id){if(CapyCatalog.Find(id)==null || Has(id))return false;owned.Add(id);return true;}
        public bool Equip(string id){var i=CapyCatalog.Find(id);if(i==null || !Has(id))return false;equipped[(int)i.slot]=id;return true;}
        public void Migrate(int pearls,int outfit)
        {
            Repair();if(pearls>=10 || outfit==1)Grant("panama");
            if(pearls>=25 || outfit==2){Grant("glasses");Grant("scarf");}
            if(pearls>=50 || outfit==3){Grant("crown");goldenCrown=true;}
            if(outfit==1)Equip("panama");if(outfit==2){Equip("glasses");Equip("scarf");}if(outfit==3)Equip("crown");
        }
        public int Value(CapyItem i)=>i.rule==UnlockRule.Free?0:i.rule==UnlockRule.Rounds?rounds:i.rule==UnlockRule.Hits?hits:i.rule==UnlockRule.Mission?missions:i.rule==UnlockRule.Combo?bestCombo:i.rule==UnlockRule.PerfectCombo?bestPerfect:i.rule==UnlockRule.PerfectHits?perfects:CosmeticCount;
        public int CosmeticCount { get {int n=0;foreach(var id in owned){var i=CapyCatalog.Find(id);if(i!=null && i.slot!=ItemSlot.Skin)n++;}return n;} }
        public List<string> Unlock()
        {
            var added=new List<string>();bool again;
            do {again=false;foreach(var i in CapyCatalog.Items)
                if(!Has(i.id) && (Value(i)>=i.target || i.id=="glasses" && bestPerfect>=4) && (i.id!="robot" || hits>=300))
                {Grant(i.id);added.Add(i.id);again=true;}
            }while(again);return added;
        }
        public List<string> Complete(RoundModel r)
        {rounds++;hits+=r.Hits;perfects+=r.PerfectHits;bestCombo=Math.Max(bestCombo,r.BestCombo);bestPerfect=Math.Max(bestPerfect,r.BestPerfectCombo);if(r.MissionDone)missions++;return Unlock();}
        public string ClaimDaily(DateTime utc)
        {
            string date=utc.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture);
            if(string.CompareOrdinal(date,dailyDate)<=0)return null;
            foreach(var i in CapyCatalog.Items)if(i.rarity==0 && i.slot!=ItemSlot.Skin && i.rule!=UnlockRule.Free && i.id!="panama" && i.id!="boat" && !Has(i.id))
            {Grant(i.id);dailyDate=date;Unlock();return i.id;}
            return null;
        }
    }
}
