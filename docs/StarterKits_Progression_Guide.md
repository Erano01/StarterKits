```txt
StarterKits - Progression ve Kit Icerigi Rehberi

Hedef Formul
1) XML (modlet) ile yap
2) XML yetmezse GameAPI events
3) Gerekirse Harmony patch
4) En son script/transpiler

Not
- 1 oyuncu 1 kez secim kilidi su anda kodda var ama devre disi (EnableOneTimeSelectionLock=false).
- Test surecinde bu dogru yaklasim: once icerik ve progression, en son kilit.

====================================================
A) Crafting Skill magazine / crafting skill artirma
====================================================

1) Dolayli (vanilla mantigi): skill magazine odulu ver
- Vanilla quest odullerinde skill magazine loot grubu kullaniliyor.
- Ornek pattern: reward type="LootItem" id="groupChallengeRewardSkillMagazinesAll" value="1"

Ornek (quests.xml patch):
<append xpath="/quests/quest[@id='quest_your_starter_kit']/rewards">
    <reward type="LootItem" id="groupChallengeRewardSkillMagazinesAll" value="3" ischosen="true"/>
</append>

Bu yontem oyuncuya magazine verdirir; oyuncu okuyunca ilgili crafting skill progression artar.

2) Dogrudan crafting progression seviyesi set et
- progression.xml icindeki crafting adlari progression_name olarak kullanilir.
- Ornek crafting progression adlari:
  craftingArmor, craftingMedical, craftingFood, craftingSeeds,
  craftingRifles, craftingMachineGuns, craftingExplosives,
  craftingWorkstations, craftingVehicles, craftingSalvageTools, craftingBows

Ornek (triggered_effect):
<triggered_effect trigger="onSelfBuffStart" action="SetProgressionLevel" progression_name="craftingArmor" level="11"/>
<triggered_effect trigger="onSelfBuffStart" action="SetProgressionLevel" progression_name="craftingFood" level="100"/>

3) Kademeli artis
<triggered_effect trigger="onSelfBuffStart" action="AddProgressionLevel" progression_name="craftingRifles" levels="5"/>

Not: progression_name degerleri mutlaka progression.xml'deki gercek adla birebir ayni olmali.

====================================================
B) Perk book sayisi artirma (book collection)
====================================================

1) Perk book item/loot ver
- Vanilla odul patterni:
  <reward type="LootItem" id="perkBooks" value="1" ischosen="true"/>

Ornek:
<append xpath="/quests/quest[@id='quest_your_starter_kit']/rewards">
    <reward type="LootItem" id="perkBooks" value="5" ischosen="true"/>
</append>

2) Belirli book progression set et (direkt)
- Vanilla items.xml'de cok kullanilan pattern:
  action="SetProgressionLevel" progression_name="perkXxxSomething" level="-1"
- level="-1": max seviyeye set eder.

Ornek:
<triggered_effect trigger="onSelfBuffStart" action="SetProgressionLevel" progression_name="perkNightStalkerComplete" level="-1"/>
<triggered_effect trigger="onSelfBuffStart" action="SetProgressionLevel" progression_name="perkLuckyLooterComplete" level="-1"/>

3) Book group adlari (progression.xml)
- skillArtOfMining
- skillLuckyLooter
- skillNightStalker
- skillTechJunkie
- skillUrbanCombat
- skillSpearHunter
- skillSniper

====================================================
C) Perk nasil artirilir
====================================================

1) Direkt set et
<triggered_effect trigger="onSelfBuffStart" action="SetProgressionLevel" progression_name="perkDeadEye" level="5"/>
<triggered_effect trigger="onSelfBuffStart" action="SetProgressionLevel" progression_name="perkMiner69r" level="5"/>

2) Kademeli ekle
<triggered_effect trigger="onSelfBuffStart" action="AddProgressionLevel" progression_name="perkPummelPete" levels="1"/>

3) Skill point odulu ver (oyuncu kendisi perk bassin)
<reward type="SkillPoints" value="5" ischosen="true"/>

4) Perk isimlerini nereden alacagim?
- progression.xml icindeki <perk name="..."> degeri progression_name olarak kullanilir.
- Ornekler:
  perkDeadEye, perkDemolitionsExpert, perkJavelinMaster,
  perkBoomstick, perkPummelPete, perkSkullCrusher,
  perkPackMule, perkTreasureHunter, perkSalvageOperations

====================================================
D) Oyuncuya armor veya silah verme
====================================================

1) XML ile loot group odulu ver (en guvenli, vanilla pattern)
- Vanilla ornek:
  <reward type="LootItem" id="groupQuestWeapons" value="1" ischosen="true" isfixed="true"/>
  <reward type="LootItem" id="groupQuestTools" value="2" ischosen="true"/>

Starter kit icin ornek:
<append xpath="/quests/quest[@id='quest_your_starter_kit']/rewards">
    <reward type="LootItem" id="groupQuestWeapons" value="1" ischosen="true" isfixed="true"/>
    <reward type="LootItem" id="groupQuestTools" value="1" ischosen="true"/>
</append>

2) Kendi loot group'unu tanimla (armor + weapon net kontrol)
- loot.xml icinde custom group ac:
<append xpath="/lootcontainers">
    <lootgroup name="groupStarterKitSoldier" count="all">
        <item name="gunAK47" count="1" quality="4,6"/>
        <item name="ammo762mmBulletBall" count="120"/>
        <item name="apparelJacketMilitary" count="1" quality="4,6"/>
        <item name="apparelPantsMilitary" count="1" quality="4,6"/>
        <item name="apparelBootsMilitary" count="1" quality="4,6"/>
        <item name="apparelGlasses" count="1" quality="2,4"/>
    </lootgroup>
</append>

- Sonra quest/reward tarafinda bunu ver:
<append xpath="/quests/quest[@id='quest_your_starter_kit']/rewards">
    <reward type="LootItem" id="groupStarterKitSoldier" value="1" ischosen="true" isfixed="true"/>
</append>

Not: item adlarini kendi game surumundeki items.xml'den birebir dogrula.

====================================================
E) Server-side guvenlik notlari (simdilik hedef disi ama hazir)
====================================================

- UI tarafi sadece secim niyeti gosterir, asil odul uygulamasi server-side olmalidir.
- Coklu tik/acik bulma riskine karsi:
  1) Confirm isleminde server tarafinda tekrar kontrol
  2) Islem atomik flag + odul verme + log
  3) Sonra tek-sefer kilidini aktif et (su an bilerek kapali)

====================================================
F) Uygulama sirasi onerisi
====================================================

1) Ilk adim: sadece XML ile 1-2 kit deneme
   - perk set
   - crafting skill set
   - perkBooks + skill magazine odulu
   - weapon/armor loot group
2) Test gecince bu mapping'i tum kitlere yay
3) En son one-time selection lock'u aktif et

Bu dosya, mevcut StarterKits gelistirme asamasinda "one-time lock sonra" prensibine gore hazirlandi.
```