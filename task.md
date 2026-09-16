# Úkol – doručení teleportem 

## Popis problému 

V rámci optimalizace a snahy o co nejrychlejší doručení je potřeba rychle a efektivně plánovat přepravu zásilek do AlzaBoxů, aby zákazník obdržel zboží co nejdříve. 

Zásilky do boxů rozvážejí malé dodávky, které jezdí po okruhu ze skladu centrálního pro danou oblast. Každá zásilka má definovanou hmotnost v kilogramech, objem v m³ (konkrétní rozměry pro zjednodušení neřešíme) a výnosnost v Kč. 

Pro jeden sklad máme k dispozici 120 dodávek, které jezdí po okruhu dvakrát denně. Do každé dodávky se vejde 7 m³ zásilek a maximální povolená hmotnost nákladu je 5,5 tuny. 

S výjimkou úterý a čtvrtku jezdíme s maximálním využitím kapacity. To znamená, že se během jedné cesty po okruhu nedostane na všechny balíčky, které lze zákazníkům odeslat. Proto chceme při každém okruhu, který vozidla absolvují, dosáhnout co nejvyšší výnosnosti. 

Jelikož je celý logistický proces Alzy poměrně komplikovaný, je pro výpočet rozřazení balíčků do dodávek k dispozici poměrně krátké časové okno. Program tedy nemůže výpočet provádět příliš dlouho. Pro jedno plánování uvažujme řádově statisíce balíčků. 

## Doplňující informace 

K řešení tohoto problému můžete použít libovolné nástroje včetně AI, libovolné algoritmy dostupné na internetu a veškeré veřejně dostupné zdroje. 

Pohybujeme se v byznysovém světě, takže zadání je záměrně obecnější a ponechává prostor pro vlastní interpretaci. Řešení nemusí být maximálně optimální, musí však daný problém vhodným způsobem řešit – mělo by být dostatečně rychlé a přinášet uspokojivou výnosnost. 

Hodnotí se především přístup k řešení problému a způsob uvažování, čitelnost kódu, rychlost řešení a profitabilita zvoleného postupu. 

Pokud vás napadne více způsobů, jak tento problém řešit, implementujte ten, který považujete za nejvhodnější. Ostatní řešení můžete stručně popsat v doprovodném textu. 

Kód pište v jazyce C#. Algoritmy nemusí ošetřovat vstupy a výstupy ani být extrémně robustní. Problém můžete libovolně zjednodušit, pokud to v reálném světě dává smysl. Můžete například předpokládat, že všechny balíčky mají kladnou výnosnost. 

Interní 

## Výstup 

Výstupem by měl být kód řešící daný problém a textový popis vysvětlující, jak jste problém pochopili, jak jste se jej rozhodli řešit a jaká omezení či předpoklady jste stanovili. 

Stačí stručný popis; případná podrobnější diskuse proběhne během pohovoru. 


# Reseni
## Myslenky
- core problému je samotný rozdělování balíku do dodávek
    - neřeší se zdroj inputu (balíků) => produkcne by to mohlo fungovat pres async komunikaci (MQ, napr. ASB), nebo sync (RESTove pres API)
        - Dejme tomu ze je sluzba, ktera handluje objednavky, na successu objednavky by se publishnula do ASB a existovala by jedna ze sluzeb, ktera by si ukladala v persistence potrebne informace k rozplanovani (Samotnej payload message nemusi mit AsyncAPI definici presne jen pro tento use-case, teoreticky v alza domene je vice sluzeb co s objednavkou neco dela a potrebuje i dalsi informace, nez jen cenu/vynosnost, objem a kg).
        - dale nastava problem => objednavka nemusi byt v "cilove destinaci" mysleno tak, ze balik musi byt nejprve prevezen do skladu odkud to teprv bude putovat do alzaboxu. To muze byt reseno nejakym dalsim domain eventem, muze byt nejaka dalsi sluzba co planuje rozvoz mezi skladama, realny case si dokazu predstavit ze se rozvoz naplanuje => provede => v cilovem skladu se treba naskenuje balik, ktery by trigroval nejakou dalsi message
        - planovani tedy pracuje jen s baliky, ktere jsou fyzicky ve skladu (naskenovane) => pred kazdym okruhem se udela snapshot a ten se predhodi planovaci
            - baliky co dorazi behem vypoctu se neresi, pojedou dalsim okruhem
    - Performance zalezi taky na tom, kdy bezi rozdelovani
        - kontiualne (tady si dokazu predstavit problem, ze prioritu maji ty s vetsi vynosnosti/marzi, priklad - uz je plno, ale najednou prijde dalsi objednavka ktera ma vlastne vetsi prioritu) 
        - cronjob - v .NETu, background service, ktera je v kubernetu definovana jako cronjob resource a spousti se v urcity cas
## Reseni v DDD svete
- Objednavky se tvori postupne, teoreticky existuje sluzba, ktera objednavky vytvari a publishuje danou message:
napr. topic `alza-orders.created`
    