using UnityEngine;
using UnityEngine.UI;

namespace Capybara67
{
    public sealed partial class CapyGame
    {
        [SerializeField] GameObject wardrobe, resultPanel;
        [SerializeField] Button wardrobeClose, equipButton, resultAgain, resultClose;
        [SerializeField] Button[] outfitButtons;
        [SerializeField] SoftShape[] outfitCards;
        [SerializeField] Text wardrobeInfo, equipText, resultTitle, resultStats, resultReward;
        [SerializeField] Text[] outfitStatus;
        int previewOutfit;
        static readonly string[] OutfitNames={"Натуральный вайб","Садовник","Исследователь","Король лотосов"};
        static readonly int[] OutfitThresholds={0,10,25,50};

        public static bool OutfitUnlocked(int index,int earned) => index>=0 && index<OutfitThresholds.Length && earned>=OutfitThresholds[index];

        void BuildGardenPanels()
        {
            wardrobe=Rect(board,"Wardrobe overlay",0,0,480,850).gameObject;
            Shape(wardrobe.transform,"Dim",0,0,480,850,"171426EE",0).raycastTarget=true;
            Shape(wardrobe.transform,"Wardrobe card",20,164,440,563,"322C46",28);
            Label(wardrobe.transform,"Wardrobe title","ТВОЙ ВАЙБ",44,184,350,38,27,"F7EBDD",TextAnchor.MiddleLeft,true);
            Label(wardrobe.transform,"Wardrobe subtitle","Собирай жемчужины. Открывай образы.",44,227,390,27,14,"B9AACB");
            outfitCards=new SoftShape[4];outfitButtons=new Button[4];outfitStatus=new Text[4];
            for(int i=0;i<4;i++) {
                float x=44+i%2*202, y=277+i/2*145;
                outfitButtons[i]=Button(wardrobe.transform,"Outfit "+i,x,y,190,132,"453B58",out var unused,"",1);
                outfitCards[i]=outfitButtons[i].GetComponent<SoftShape>();
                var icon=Rect(outfitButtons[i].transform,"Outfit icon",55,13,80,62);
                DrawOutfitIcon(icon,i);
                Label(outfitButtons[i].transform,"Name",OutfitNames[i],5,79,180,23,13,"F7EBDD",TextAnchor.MiddleCenter,true);
                outfitStatus[i]=Label(outfitButtons[i].transform,"Status","",5,104,180,20,11,"C4B5CD",TextAnchor.MiddleCenter);
            }
            wardrobeInfo=Label(wardrobe.transform,"Wardrobe info","",44,581,392,37,14,"CFC0D9",TextAnchor.MiddleCenter);
            equipButton=Button(wardrobe.transform,"Equip",44,636,300,53,"A4F0CC",out equipText,"НАДЕТЬ",16);equipText.color=C("241D38");
            wardrobeClose=Button(wardrobe.transform,"Wardrobe close",357,636,79,53,"504460",out var back,"НАЗАД",12);
            wardrobe.SetActive(false);
            resultPanel=Rect(board,"Result overlay",0,0,480,850).gameObject;
            Shape(resultPanel.transform,"Result dim",0,0,480,850,"171426DB",0).raycastTarget=true;
            Shape(resultPanel.transform,"Result card",28,228,424,411,"342D49",28);
            Label(resultPanel.transform,"Round complete","РАУНД ЗАВЕРШЁН",56,248,368,23,12,"A4F0CC",TextAnchor.MiddleCenter);
            resultTitle=Label(resultPanel.transform,"Result title","",52,290,376,62,35,"F7EBDD",TextAnchor.MiddleCenter,true);
            resultStats=Label(resultPanel.transform,"Result stats","",64,375,352,83,17,"D2C2DC",TextAnchor.MiddleCenter);
            resultReward=Label(resultPanel.transform,"Result reward","",54,478,372,39,16,"A4F0CC",TextAnchor.MiddleCenter);
            resultAgain=Button(resultPanel.transform,"Retry",54,548,276,56,"A4F0CC",out var retry,"ЕЩЁ РАЗ   →",17);retry.color=C("241D38");
            resultClose=Button(resultPanel.transform,"Result close",341,548,84,56,"514461",out var close,"МЕНЮ",13);
            resultPanel.SetActive(false);
        }
        static void DrawOutfitIcon(Transform icon,int index)
        {
            Shape(icon,"Mini ear L",6,7,15,23,"C9976B",7);Shape(icon,"Mini ear R",58,7,15,23,"C9976B",7);
            Shape(icon,"Mini face",0,18,80,45,"D4A777",18);Shape(icon,"Mini muzzle",25,39,36,20,"ECC99A",8);
            Shape(icon,"Mini eye L",15,32,6,4,"49333B",2);Shape(icon,"Mini eye R",60,32,6,4,"49333B",2);
            if(index==1) {Shape(icon,"Hat",14,2,55,20,"A4F0CC",6);Shape(icon,"Brim",5,18,73,7,"75BA9D",3);Flower(icon,42,10,3);}
            if(index==2) {for(int i=0;i<2;i++){Shape(icon,"Rim",8+i*46,27,25,17,"EFD194",6);Shape(icon,"Glass",11+i*46,30,19,11,"486079",4);}Shape(icon,"Bridge",32,29,23,3,"EFD194",1);}
            if(index==3) {Shape(icon,"Crown",16,5,50,16,"EBC575",3);for(int i=0;i<3;i++)Shape(icon,"Crown tip",19+i*19,-2,7,17,"F6D68D",2);Shape(icon,"Gem",37,9,9,8,"A4F0CC",3);}
        }
        void InitGarden()
        {
            outfit=Mathf.Clamp(outfit,0,3);
            if(!OutfitUnlocked(outfit,pearls)) outfit=0;
            for(int i=0;i<outfitButtons.Length;i++){int slot=i;outfitButtons[i].onClick.AddListener(()=>{previewOutfit=slot;RefreshWardrobe();});}
            wardrobeClose.onClick.AddListener(()=>{wardrobe.SetActive(false);Refresh();});
            equipButton.onClick.AddListener(()=>{if(!OutfitUnlocked(previewOutfit,pearls))return;outfit=previewOutfit;Persist();RefreshWardrobe();Refresh();});
            resultAgain.onClick.AddListener(Action);
            resultClose.onClick.AddListener(()=>resultPanel.SetActive(false));
            BindPlatform();
        }
        void RefreshWardrobe()
        {
            for(int i=0;i<4;i++) {
                outfitCards[i].color=C(i==previewOutfit?"6B597D":"453B58");
                outfitStatus[i].text=outfit==i?"НАДЕТО":OutfitUnlocked(i,pearls)?"ОТКРЫТО":OutfitThresholds[i]+" ЖЕМЧУЖИН";
            }
            bool unlocked=OutfitUnlocked(previewOutfit,pearls);
            wardrobeInfo.text=unlocked?"Образ открыт навсегда. Жемчужины не тратятся.":"Ещё "+(OutfitThresholds[previewOutfit]-pearls)+" жемчужин до нового образа";
            equipButton.interactable=unlocked;equipText.text=outfit==previewOutfit?"УЖЕ НАДЕТО":unlocked?"НАДЕТЬ":"ПОКА ЗАКРЫТО";
        }
        void ShowResult(bool record,int reward)
        {
            resultTitle.text=(record?"НОВЫЙ РЕКОРД!\n":"")+round.Score+" ОЧКОВ";
            resultTitle.fontSize=record?27:35;
            int attempts=round.Hits+round.Misses;
            int accuracy=attempts>0?Mathf.RoundToInt(round.Hits*100f/attempts):0;
            resultStats.text="Поймано: "+round.Hits+"     Идеально: "+round.PerfectHits+"\nЛучшая серия: "+round.BestCombo+"     Точность: "+accuracy+"%";
            resultReward.text="+"+reward+" жемчужин   ·   Всего: "+pearls;
            resultPanel.SetActive(true);
        }
    }
}
