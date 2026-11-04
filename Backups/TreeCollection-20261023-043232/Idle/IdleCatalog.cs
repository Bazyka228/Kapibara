using System.Collections.Generic;
namespace Capybara67.Idle {
 public sealed class IdleItem { public string id,name,color; public int slot,art,stage,stars; public double price; public int RewardDay=>id=="firefly_garden"?3:id=="set_dawn"?7:0; public bool IsSet=>CapyCompleteSets.IsSet(id); public double bonus=>id=="moon"||id=="classic"?0:slot<3?.02:0; }
 public static class IdleCatalog {
 const string Data=@"panama|Панама|0|0|91DDC0|2|20|0
cap|Кепка|0|1|E88C77|2|45|0
crown|Корона|0|2|A0CF82|3|280|0
duck|Уточка|0|3|FFE08A|3|95|0
detective|Сафари|0|4|CBB38C|3|120|0
phones|Наушники|0|5|9F9DE6|4|580|0
ushanka|Ушанка|0|6|C2CADF|4|170|0
wreath|Венок|0|7|EAAAC8|4|195|0
helmet|Строитель|0|8|F6BF65|4|220|0
chef|Шеф-повар|0|9|F5EDDC|5|245|0
boat|Кораблик|0|10|C6DEEE|5|270|0
glasses|Очки|0|11|EAD08F|6|1180|0
scarf|Тёплый шарф|1|0|EB9D76|2|20|0
tie|Бабочка|1|1|DA849D|2|45|0
apron|Фартук|1|2|DCE3B7|2|70|0
cape|Плащ героя|1|3|7FC0A2|4|380|0
sweater|Свитер|1|4|B6A9DC|3|120|0
shirt67|Футболка 6–7|1|5|F0C770|4|580|0
hoodie|Рюкзак|1|6|8795BC|4|170|0
raincoat|Дождевик|1|7|F2D06F|4|195|0
armor|Пояс мастера|1|8|C7A07B|5|880|0
orangeShirt|Мандарин|1|9|9FD2C0|5|245|0
moon|Сад у пруда|2|0|9BBE77|1|0|0
sunset|Терраса|2|1|DEB083|2|90|0
jungle|Джунгли|2|2|77A15C|2|140|0
space|Звёздный сад|2|3|A7B9C9|5|1330|4
room|Веранда|2|4|BCA67A|3|240|0
kitchen|Чайный дом|2|5|C19D76|3|290|0
snow|Зимний лес|2|6|A9CBBF|5|1360|0
beach|Пляж|2|7|8EC8B0|4|390|0
firefly_garden|Сад светлячков|2|8|7EB5A0|1|0|0
rainbow|Пузырьки|3|0|A4E3CD|2|40|0
stars|Звёздочки|3|1|F8DA94|3|360|0
hearts|Сердечки|3|2|F0A6BD|2|140|0
pixels|Пиксели|3|3|8EDAD8|4|760|0
fire|Светлячки|3|4|F8AA68|5|1680|4
digits|Цифры 6–7|3|5|C6B0EF|5|2030|4
laugh|Тихий смешок|4|0|F1C0AC|2|40|0
petals|Лепестки сакуры|3|6|EBA5BF|2|180|0
butterflies|Бабочки|3|7|A7B8E2|3|300|0
dew|Росинки|3|8|8ECFD7|3|350|0
leaves|Листопад|3|9|B2C97C|3|380|0
music|Мелодия сада|3|10|D3AFD8|4|650|0
moons|Лунные искры|3|11|E4D6A1|5|900|0
wow|Ого!|4|1|EFD597|2|90|0
yawn|Сонный зевок|4|2|C6B4E1|2|140|0
joy|Получилось!|4|3|AFDFC8|4|760|0
brow|Скептик|4|4|D0B8A3|4|960|0
smile|Улыбка|4|5|F2D477|5|2030|4
classic|Мандаринка|5|0|DDA779|1|0|0
love|Обнимашки|4|6|E9ABBC|2|160|0
wink|Подмигни|4|7|EBC889|2|190|0
curious|Интерес|4|8|AFCBB5|3|260|0
proud|Гордость|4|9|E7C16D|3|300|0
peace|Дзен|4|10|B9C9DB|4|450|0
inspired|Озарение|4|11|D6B4D9|4|500|0
sport|Пружинка|5|1|B96E4E|2|90|0
sleepy|Тихоня|5|2|B5A29A|2|140|0
neon|Люми|5|3|695787|4|760|0
pixel|Кубик|5|4|CFA66A|4|960|0
sunny|Лучик|5|5|E5BD5D|5|2030|4
cosmic|Орбита|5|6|AAA9C8|6|2380|4
fluffy|Плюшка|5|7|EADBC0|4|390|0
robot|Бип|5|8|91ADBD|5|1760|0
gold|Золотой час|5|9|D9A83E|8|4900|6
beret|Берет художника|0|12|B76E80|2|80|0
beanie|Вязаная шапка|0|13|8BA685|2|100|0
bunnyears|Заячьи ушки|0|14|F0D8C9|3|180|0
froghat|Лягушонок|0|15|8EAF62|3|210|0
daisyhat|Ромашковая шляпа|0|16|D8B873|3|250|0
wizardhat|Звездочёт|0|17|657696|5|980|0
aviator|Авиатор|0|18|AF8060|4|470|0
sailor|Морячок|0|19|E9E3CD|4|410|0
pirate|Капитан удачи|0|20|84644F|5|1050|0
hibiscus|Гибискус|0|21|DE929A|3|230|0
orangehat|Мандариновая шапка|0|22|E9AD63|4|450|0
partyhat|Праздничный колпак|0|23|EAB99B|3|280|0
leaftiara|Лесная тиара|0|24|719C69|6|1600|0
bandana|Бандана в горошек|1|10|D88F7D|2|85|0
poncho|Уютное пончо|1|11|A4B38A|3|220|0
lifevest|Спасательный жилет|1|12|E9A15F|4|460|0
satchel|Сумка-листочек|1|13|B38C62|3|270|0
camera|Фотоаппарат|1|14|81A5B5|4|560|0
leafcape|Лесная накидка|1|15|7C9F65|5|940|0
starcape|Звёздная мантия|1|16|65758F|6|1800|0
overalls|Комбинезон|1|17|819DB3|4|490|0
waistcoat|Нарядный жилет|1|18|A8717D|5|840|0
medal|Медаль дружбы|1|19|E4BE70|5|900|0
flowerlei|Ромашковые бусы|1|20|E8D9AC|3|260|0
picnicpack|Корзинка для пикника|1|21|CAA779|6|1500|0";
 public static readonly IdleItem[] Items=Build();
 static IdleItem[] Build(){var list=new List<IdleItem>();foreach(var row in Data.Split('\n')){var a=row.Trim().Split('|');list.Add(new IdleItem{id=a[0],name=a[1],slot=int.Parse(a[2]),art=int.Parse(a[3]),color=a[4],stage=IdleModel.StageFromLegacy(int.Parse(a[5])),price=int.Parse(a[6]),stars=int.Parse(a[7])});}foreach(var set in CapyCompleteSets.All)list.Add(new IdleItem{id=set.id,name=set.name,slot=0,art=-1,color=set.color,stage=set.stage,price=set.price});return list.ToArray();}
 public static IdleItem Find(string id){foreach(var i in Items)if(i.id==id)return i;return null;}
 }
}





