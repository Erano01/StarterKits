// Kendi contextini buraya yazabilirsin daha sonra kullanmak için, dökümantasyondan elde ettiğin bilgileri vesaire:

# v3.1 ModAPI Notes (via dnSpy-mcp)

Kaynak: `/home/erano/.local/share/Steam/steamapps/common/7 Days To Die/7DaysToDie_Data/Managed/Assembly-CSharp.dll`
Bu dosya v3.1-Staging için canlı referans. Yeni bir sınıfa ihtiyaç oldukça dnSpy-mcp ile decompile edip buraya ekleniyor — tahmin/hafızadan yazılmıyor.

## Root cause: v3.1'de mod tamamen kırılıyordu

`Src/StarterKitsModApi.cs` → `harmony.PatchAll(Assembly.GetExecutingAssembly())` assembly'deki TÜM `[HarmonyPatch]` class'larını tarar. `Harmony/Examples.cs` içindeki `AudioClientPlay`/`AudioClientPlay2` tutorial patch'leri `Audio.Client.Play(int,string,float)` hedefliyordu — bu overload v3.1'de yok. `AccessTools` hedefi bulamayınca `PatchAll` exception fırlatıyor, bu da `InitMod()`'u tamamen durduruyor (floor patch, kit selection store, join handler dahil hiçbir şey initialize olmuyor). Log kanıtı: `output_log_client__2026-08-13__19-55-21.txt` satır 151-169.

## Audio.Client (v3.1) — güncel imzalar

```csharp
Play(int playOnEntityId, string soundGroupName, float occlusion, float volumeScale)          // eskiden (int,string,float) idi, volumeScale eklendi
Play(Vector3 position, string soundGroupName, float occlusion, int entityId, float volumeScale) // eskiden (Vector3,string,float,int) idi, volumeScale eklendi
Stop(int stopOnEntityId, string soundGroupName)
Stop(Vector3 position, string soundGroupName)
```

## ProgressionValue (v3.1)

- `Level` (get/set, int) — Skill tipi progression class'larda **her zaman `ProgressionClass.MaxLevel` döner**, `level` field'ı ignore edilir. Bu v2.5/2.6'dan farklı olabilir, floor patch buna dikkat etmeli.
- `GetLevel()` **artık yok** — `StarterKitProgressionFloorPatch.cs` bunu hedefliyor (satır 34-39), `TryPatch` sessizce atlıyor (try/catch var, crash etmiyor ama floor bu yoldan uygulanmıyor).
- `CalculatedMaxLevel(EntityAlive)` → int, var, imza aynı.
- `CalculatedLevel(EntityAlive)` → int, var, imza aynı.
- `GetCalculatedLevel(EntityAlive)` → float, var, imza aynı. Frame-cache var (`calculatedFrame == Time.frameCount`), ama postfix her çağrıda çalışır çünkü Harmony orijinal metodu sarmalıyor.
- `IsLocked(EntityAlive)` → bool, var, imza aynı.

## ProgressionClass (v3.1)

- `static GetCalculatedMaxLevel(EntityAlive, ProgressionValue)` → int, var, imza aynı.
- `IsSkill`, `IsAttribute`, `MaxLevel`, `MinLevel`, `LevelRequirements` (private, `DictionaryList<int,LevelRequirement>`) mevcut.

## Proje temizliği — DONE (v3.1-Staging)

1. ~~`Harmony/Examples.cs`~~ — silindi. Crash kaynağıydı.
2. ~~`Src/XUiC_KitSelectionMenu.cs: TryAddBuffByName()`~~ — silindi (hiç çağrılmıyordu).
3. ~~`Src/XUiC_KitSelectionMenu.cs: SetEnabled()` + `TrySetBoolMember()`~~ — silindi (hiç çağrılmıyordu).
4. ~~`StarterKits.csproj` Content listesi~~ — düzeltildi: `Readme.txt`→`README.md`, `windows.xml` yorumdan çıkarıldı, `styles.xml` eklendi.
5. ~~`starterKitName` dead custom-var write~~ — silindi (`starterKitSelected` zaten tek gerçek lock flag'i).

## v3.1 API kırığı #2 — XUiView.IsDirty kaldırıldı (build sırasında bulundu)

`XUiView.IsDirty` artık public settable property değil (v2.5/2.6'da vardı). v3.1'de `isDirty` private field, dışarıdan sadece `SetDirty()` public metoduyla tetikleniyor.
Etki: `XUiC_KitSelectionMenu.cs` içindeki `SetVisible()` ve `TryApplyText()` derleme hatası veriyordu (`controller.ViewComponent.IsDirty = true;`).
Fix: `IsDirty = true` → `SetDirty()` çağrısına çevrildi (iki yer). `XUiView.IsVisible` property olarak hâlâ duruyor, değişmedi.

Build doğrulaması: `dotnet build StarterKits.csproj -v minimal` → 0 Warning, 0 Error (2026-08-14).

## v3.1 API kırığı #3 — XUi klasör yapısı ve <ruleset> kaldırılmış (asıl "mod çalışmıyor" sebebi)

Test: yeni dünya (`Pregen06k02/Test`) açıldı, ESC'ye basıldı, Starter Kit butonu yoktu. Log kanıtı:
- `GUIWindowManager.Open: Window "starterKitGroup" unknown!` — pencere hiç register olmamış.
- `[StarterKits] Starter Kit button not found in InGame menu.` — buton XML'i hiç uygulanmamış.

Kök sebep: v2.6'da vanilla `Data/Config/XUi/` tek klasördü. v3.1'de üçe bölünmüş:
- `Data/Config/XUi_Common/` (styles.xml, templates.xml — paylaşılan)
- `Data/Config/XUi_InGame/` (windows.xml, xui.xml, styles.xml, templates.xml — oyun içi HUD/menüler, ESC menüsü `ingameMenu` burada)
- `Data/Config/XUi_Menu/` (windows.xml, xui.xml, styles.xml, templates.xml — ana menü)

Mod'un `Config/XUi/windows.xml` ve `Config/XUi/xui.xml` dosyaları hiçbir vanilla dosyaya eşleşmiyordu (mod XML patch sistemi klasör bazlı gruplanıyor), yani patch'ler **hiç uygulanmadı**. Ayrıca `xui.xml` içinde `<append xpath="/xui/ruleset[@name='default']">` kullanıyorduk — v3.1'de `<ruleset>` elementi tamamen kaldırılmış (`grep -rl "ruleset" Data/Config/` → sıfır sonuç), `<xui>` artık `window_group`'ları doğrudan child olarak alıyor.

Fix (v3.1-Staging):
1. `Config/XUi/` → `Config/XUi_InGame/` olarak taşındı (`git mv`). ESC menüsü (`ingameMenu`) ve starter kit penceresi ikisi de oyun içi olduğu için doğru hedef bu.
2. `xui.xml`: `xpath="/xui/ruleset[@name='default']"` → `xpath="/xui"` olarak düzeltildi.
3. `windows.xml` içeriği değişmedi — doğrulandı: `/windows` root hâlâ geçerli, `//window[@name='ingameMenu']//grid[@name='buttons']` hâlâ eşleşiyor (satır 4432-4433, `XUi_InGame/windows.xml`).
4. `styles.xml` boş dosya, kullanılan renk token'ları (`[white]`, `[black]`, `[darkGrey]`, `[mediumGrey]`, `[selectedColor]`) hepsi `XUi_Common/styles.xml`'de sağlam, dokunmaya gerek yok.
5. `StarterKits.csproj` Content path'leri `Config\XUi_InGame\...` olarak güncellendi.

Test edildi, çalışıyor: crash yok, ESC menüsünde buton var, starterKitGroup açılıyor.

## v3.1 API kırığı #4 — `backgroundcolor` attribute'ü panel/rect'lerde artık render edilmiyor

Belirti: mod çalışıyor ama header/selection_panel/overview_panel arka planı siyahtan transparana dönmüş gibi görünüyordu.

Kök sebep: v3.1 assembly'sinde `attributeBackgroundColor` diye bir handler yok (`search_members` sıfır sonuç), vanilla `XUi_InGame/windows.xml`'de de `<panel>`/`<rect>` üzerinde `backgroundcolor="..."` kullanımı sıfır — sadece özel controller elemanlarında (`player_stats_entry` gibi) var. Vanilla artık solid arka planı explicit `<sprite sprite="menu_empty3px" color="[...]" type="sliced" fillcenter="false" />` child'ı ile yapıyor (örn. `backgroundMain` sprite'ları). Bizim `windows.xml`'de `backgroundcolor` attribute'üne SADECE güvenen elemanlar (sprite fallback'i olmayanlar) sessizce arka planını kaybetti.

Fix (v3.1-Staging, `Config/XUi_InGame/windows.xml`):
- `header`, `selection_panel`, `overview_panel` panellerine `sprite="menu_empty3px" color="[black]"` background sprite eklendi, dead `backgroundcolor="[black]"` attribute'ü kaldırıldı.
- 14 kit hücresi rect'i (`Scavenger`, `Huntsman`, ... `Burglar`) — bunların hiç sprite fallback'i yoktu, tamamen arka planı kaybetmişlerdi. Her birine `color="[darkGrey]"` background sprite eklendi.
- `overviewFrame` (`backgroundcolor="22,22,22,255"`) ve `imgContainer` (`backgroundcolor="36,36,36,255"`) — bunlar zaten kendi sprite'ları tarafından örtülüyordu (davranış hiç değişmemişti, redundant idi), dead attribute'ler temizlendi, görsel değişiklik yok.

**DÜZELTME #2 (aynı bug'ın devamı):** İlk fix'te `fillcenter="false"` kullanmıştım — bu YANLIŞ, hâlâ transparan görünüyordu (restart + yeni save ile de doğrulandı, xui reload sorunu değildi). dnSpy ile `XUiV_Sprite.updateData()` decompile edilince kanıtlandı:
```csharp
sprite.centerType = (fillCenter ? UIBasicSprite.AdvancedType.Sliced : UIBasicSprite.AdvancedType.Invisible);
```
`fillcenter="false"` = merkezi **Invisible** yapar (sadece ince border kalır, tam da bizim gördüğümüz "transparan" görüntü). Doğrusu `fillcenter="true"` (zaten `XUiV_ImageBased.SetDefaults()`'ta default değer). Eklediğim 17 background sprite'ının (3 panel + 14 kit hücresi) hepsinde `fillcenter="false"` → `fillcenter="true"` yapıldı. Önceden var olan diğer `fillcenter="false"` kullanımları (selOverlay border decoration, imgContainer border efekti gibi — kasıtlı hollow-frame amaçlı) dokunulmadı.

Ders: XUi attribute semantiğini varsayımla değil dnSpy ile doğrula — "vanilla'da böyle kullanılıyor" gözlemi tek başına yeterli değilmiş, attribute'ün gerçek davranışını C# implementasyonundan teyit etmek gerekiyor.

**DÜZELTME #3:** `[black]` = `0,0,0,255` (zaten max opak) ama görsel hâlâ "yeterince koyu değil" hissi veriyordu. Sebep: oyunun `EnumGamePrefs.OptionsBackgroundGlobalOpacity` ayarı `XUi.BackgroundGlobalOpacity`'yi besliyor, bu da `XUiV_ImageBased.opacityModColor()` içinde sprite alpha'sını çarpıyor (`globalOpacityModifier * GlobalOpacitySetting`). Oyuncunun UI background opacity ayarı %100 değilse, `color="[black]"` olsa bile sprite kısmen saydam render oluyor. Vanilla bazı zorunlu-opak arka planlarda (`backgroundMain`) bunu `globalopacitymod="0"` ile bypass ediyor — `opacityModColor()`'da `globalOpacityModifier == 0` ise orijinal renk hiç çarpılmadan (yani tam alpha ile) döndürülüyor. Aynısı 17 background sprite'ımıza da eklendi.

**DÜZELTME #4:** `globalopacitymod="0"` ile tam opak yapınca kullanıcıya göre "çok koyu" oldu, eski (slider'a bağlı, kısmen saydam) hali daha çok beğenildi — sadece "çok çok az" daha koyu isteniyor. Kesin bir hedef alpha değeri bilinmediği (oyuncunun opacity slider ayarı authoring-time'da bilinmiyor) için, base sprite orijinal haline (slider'dan etkilenen, `globalopacitymod` attribute'ü olmadan) döndürüldü, üstüne ikinci bir ince overlay sprite eklendi: aynı boyut/pozisyon, `color="0,0,0,40"` (düşük sabit alpha), `globalopacitymod="0"` (slider'dan bağımsız sabit katkı). Alpha-blend sonucu: eski görünüme çok yakın ama biraz daha koyu, slider değerinden bağımsız olarak öngörülebilir küçük bir artış. 17 sprite'ın hepsinde bu pattern (base + overlay) uygulandı.

## UI iyileştirme — Kit Stats satırlarına perk/skill ikonu eklendi

Önceden "Kit Stats" alanı düz metin ("- Salvage Operations 5/5" gibi) tek bir `lblKitBonuses` label'ıydı. `KitOverviewData.StatLines` artık `string[]` değil `StatEntry[]` (`Text` + `IconProgressionName`). `IconProgressionName`, o kit'in `KitRewards.ProgressionFloors` dictionary'sindeki gerçek progression key'i (örn. `perkSalvageOperations`) — 14 kit'in tüm StatLines'ı bu gerçek key'lerle elle eşleştirildi (bazı satırların (örn. Farmer'ın "Super Corn Crafting Magazine"ı, Ex-Soldier'ın "Commando Armor Fullset"ı) progression floor karşılığı yok, bunlar ikonsuz kalıyor — CustomVar/ItemReward kaynaklı, progression değil).

İkon çözümleme (`ResolveStatIconSprite`): `Progression.ProgressionClasses[name].Icon` → `progression.xml`'deki `icon="..."` attribute'ü (bazen virgüllü çoklu-tier liste, ilk değer alınıyor) → `ItemIconAtlas`'tan sprite adı olarak kullanılıyor (kit portrelerinde zaten kullandığımız atlas).

XML: `Config/XUi_InGame/windows.xml` — `lblKitBonuses` kaldırıldı, yerine sabit 10 satır (`statIcon0..9` + `statLabel0..9`, en kalabalık kit 10 satırlı) eklendi, her satır 16x16 icon + 15pt label. C#: `MaxStatRows=10`, `ApplyStatRows()` her satırı doldurur/gizler, kullanılmayan satırlar `SetVisible(false)`.

## UI iyileştirme — DÜZELTME: ikonlar görünmüyordu + liste butona taşıyordu

Kullanıcı test etti: ikonlar hiç görünmedi, Miner gibi 10 satırlı kitlerde liste "Select & Confirm" butonunun üstüne biniyordu.

**İkon sorunu — kök sebep:** `ProgressionClass.Icon` (`progression.xml`'deki `icon="ui_game_symbol_..."` değerleri) `ItemIconAtlas`'ta DEĞİL, ayrı bir atlas'ta yaşıyor. dnSpy ile takip ettim: `XUiC_SkillEntry.GetBindingValueInternal("groupicon")` → `currentSkill.ProgressionClass.Icon`'u döndürüyor, XML'de `<sprite name="groupIcon" sprite="{groupicon}" style="icon32px" .../>` — `style="icon32px"` (`XUi_Common/styles.xml`) sadece width/height/color/foregroundlayer tanımlıyor, **atlas attribute'ü hiçbir yerde yok**. Yani atlas, XML'den erişilemeyen bir Unity prefab default'undan geliyor (asset bundle'lar sıkıştırılmış, `strings` ile bile okunamadı). Ayrıca kitap-tipi (`<book>`) perk'lerin (Sniper Perk Book, Bar Brawling, vs.) zaten hiç `icon` attribute'ü yok — bu yüzden "Complete" suffix'li eşlemelerim zaten kısmen yanlıştı.

**Çözüm:** Mod'un kendi `UIAtlases/ItemIconAtlas/` klasörü zaten kanıtlanmış şekilde çalışıyor (kit portreleri buradan geliyor). `progression.xml` icon'ları yerine, `items.xml`'den TEK TEK doğrulanmış (`grep`'lenmiş, var olduğu teyit edilmiş) gerçek eşya ikonlarını kategori bazlı kullandım: `IconClub` (meleeWpnClubT3SteelClub), `IconSniper` (gunRifleT3SniperRifle), `IconBow`, `IconSpear`, `IconKnuckles`, `IconKnife`, `IconWrench`, `IconHelmet`, `IconPistol`, `IconMachineGun`, `IconRocket`, `IconAuger`, `IconMedkit`, `IconVehicle`. `StatEntry.IconProgressionName` → `IconSpriteName` olarak yeniden adlandırıldı (artık progression key değil, doğrudan sprite adı taşıyor). Belirsiz/uygun eşya ikonu olmayan satırlar (Parkour, Perception Mastery, Lucky Looter vb.) bilerek ikonsuz bırakıldı — yanlış/yanıltıcı ikon vermek yerine.

Ders: XUi'de bir attribute'ün XML'de nasıl kullanıldığını görmek yetmiyor — hangi ATLAS'tan geldiğini de zincirin sonuna kadar (style → styles.xml → hiç atlas yok) doğrulamak lazım, yoksa "çalışıyor gibi duruyor ama render olmuyor" tuzağına düşülüyor.

**Layout sorunu — kök sebep:** 10 satır × 21px + satır başlangıcı y=-415, overviewFrame'in overview_panel içindeki pos'u (-55) ile birleşince son satır mutlak y=-659'a düşüyordu; buton -625/-677 aralığındaydı — doğrudan çakışma.

**Çözüm:** Satır yüksekliği 21→18px, font 15→13, icon 16→14px, blok başlangıcı -415→-410; `btnConfirmKit` -625→-655'e taşındı. Yeni hesapla son satır mutlak y≈-627, buton üstü -655 — ~28px pay var.

## İkonlar hâlâ yanlış — kullanıcı gerçek vanilla sembol ikonlarını istiyor (item ikonu değil)

Kullanıcı `docs/menu-icons.png` ile Tab/Skills menüsündeki gerçek `ui_game_symbol_*` ikonlarını gösterdi ve "bunları oyun içinden bul, screenshot'tan alma" dedi. Item-icon (kılıç/tüfek vb.) çözümü onun istediği değil.

Loose dosya arandı: `Data/Web/webroot/static/media/` sadece 11 alakasız "animal tracking" ikonu içeriyor; `Data/ItemIcons/` 5176 dosya ama sıfır `ui_game_symbol_*`. Gerçek sembol ikonları `Data/Addressables/Standalone/textures_assets_textures/ui.bundle` içinde — sıkıştırılmış Unity AssetBundle, `grep`/`strings` ile okunamıyor, AssetStudio/AssetRipper gibi özel bir extraction aracı gerekiyor (elimde yok).

**Kullanıcıya iş bölümü teklif edildi ve kabul edildi:** Kullanıcı AssetStudioGUI ile `ui.bundle`'dan ikonları PNG olarak export edip `docs/extracted-icons/` altına koyacak, ben onları `UIAtlases/ItemIconAtlas/` altına taşıyıp `StatEntry` sabitlerini (`IconClub` vb. → gerçek `ui_game_symbol_*` isimleri) güncelleyeceğim.

**İhtiyaç duyulan 52 gerçek ikon adı** (progression.xml'den `grep` ile doğrulandı, kitap-tipi perk'ler için üst skill'in icon'u kullanıldı):
`ui_game_symbol_scrap, ui_game_symbol_perception_mastery, ui_game_symbol_wrench, ui_game_symbol_map_cursor, ui_game_symbol_animal_tracker, ui_game_symbol_paint_copy_block, ui_game_symbol_archery, ui_game_symbol_sneak_attack, ui_game_symbol_long_shot, ui_game_symbol_parkour, ui_game_symbol_hard_target, ui_game_symbol_run_and_gun, ui_game_symbol_armor_iron, ui_game_symbol_cardio, ui_game_symbol_boxer, ui_game_symbol_knunchuck, ui_game_symbol_character, ui_game_symbol_fortitude_mastery, ui_game_symbol_siphoning_strikes, ui_game_symbol_healing_factor, ui_game_symbol_bat, ui_game_symbol_tree, ui_game_symbol_crops, ui_game_symbol_fork, ui_game_symbol_light_armor2, ui_game_symbol_spear, ui_game_symbol_electric_generator, ui_game_symbol_4x4, ui_game_symbol_junk_turret, ui_game_symbol_electric_turret, ui_game_symbol_workbench, ui_game_symbol_service, ui_game_symbol_rifle, ui_game_symbol_medical, ui_game_symbol_talk, ui_game_symbol_sledge, ui_game_symbol_grand_slam, ui_game_symbol_strength_mastery, ui_game_symbol_pack_mule, ui_game_symbol_mining, ui_game_symbol_mother_load, ui_game_symbol_tool, ui_game_symbol_explosion, ui_game_symbol_stealth, ui_game_symbol_gunslinger, ui_game_symbol_agility_mastery, ui_game_symbol_pistol, ui_game_symbol_shopping_cart, ui_game_symbol_treasure, ui_game_symbol_adventure, ui_game_symbol_barter, ui_game_symbol_unlock, ui_game_symbol_knife, ui_game_symbol_deep_cuts`

**Yan bulgu (ayrı bug, henüz dokunulmadı):** `perkIronGut` (Huntsman ve Tyson kitlerinde `KitRewards.ProgressionFloors`'da kullanılıyor) `progression.xml`'de gerçekte YOK — bu floor hiçbir zaman gerçek bir progression'a uygulanmıyor, "Iron Gut" ödülü şu an sessizce hiçbir şey yapmıyor. Kullanıcıya ayrıca söylenmeli.

## Vanilla ikon avı — AssetRipper ile denendi, hâlâ bulunamadı

AssetRipper (linux-x64, github.com/AssetRipper/AssetRipper) `--headless --port` ile ayağa kaldırıldı, düz HTML form tabanlı olduğu için tamamen `curl` ile scriptlenebildi (`/LoadFile` POST, `/Collections/View` GET ile asset listesi). Kontrol edilen bundle'lar — hiçbirinde `ui_game_symbol_*` deseniyle eşleşme yok:
- `textures_assets_textures/ui.bundle` (33 texture, sadece flag/overlay/map_cursor gibi harita ikonları)
- `textures_assets_textures/hud.bundle` (22 texture)
- `textures_assets_textures/graphics.bundle` (8 texture)
- `automatic_assets_generic/itemicons.bundle`, `automatic_assets_other/items.bundle` (zaten bildiğimiz item ikonları)
- `7DaysToDie_Data/resources.resource` — bu bir "Resources" blob'u (ham stream), AssetRipper'da browse edilebilir asset listesi olarak açılmıyor.

Kontrol edilen tüm collection'larda class filter sadece `AssetBundle`/`Texture2D` gösteriyor — hiç `Sprite` veya `MonoBehaviour` class'ı yok. Bu, ikonların muhtemelen bir Unity SpriteAtlas (tek büyük texture + adlandırılmış alt-bölgeler) içinde gömülü olduğu ve henüz doğru bundle'ı bulamadığımız anlamına geliyor. Geriye ~200 bundle kaldı (çoğu `player_assets_entities`/`zombies_assets_entities` altında karakter/zombi modelleri — isim olarak UI ile alakasız, ama kesin değil).

**Durum:** AssetRipper server durduruldu (`pkill`). Kullanıcıya, tüm `Standalone` klasörünü tek seferde AssetStudioGUI'nin "Load Folder" + global arama kutusuyla taramasının (benim tek-tek bundle denemekten çok daha verimli) veya item-icon çözümünde kalmanın arasında seçim sunuldu.

## ÇÖZÜLDÜ — atlas adı "UIAtlas"

Discord'da (Guppy's) TFP çalışanı olduğu düşünülen biri atlas adının doğrudan `UIAtlas` olduğunu söyledi. Extraction'a hiç gerek yokmuş — `atlas="UIAtlas"` + `progression.xml`'deki gerçek `icon="ui_game_symbol_..."` değeri yeterli.

Uygulanan:
- `Config/XUi_InGame/windows.xml`: 10 `statIcon0-9` sprite'ının `atlas="ItemIconAtlas"` → `atlas="UIAtlas"`, ayrıca vanilla'nın kendi `groupIcon` kullanımıyla (`XUi_InGame/templates.xml:610`) tutarlı olsun diye `foregroundlayer="true" globalopacitymod="0"` eklendi (opacity slider'dan etkilenmesin).
- `Src/XUiC_KitSelectionMenu.cs`: item-icon bucket sabitleri (`IconClub` vb.) tamamen silindi. Her `StatEntry`'nin ikinci parametresi artık gerçek `ui_game_symbol_*` ismi — `progression.xml`'den grep ile tek tek doğrulanmış (bkz. yukarıdaki 52 ikonluk liste). `perkIronGut`, "Commando Armor Fullset", "Super Corn Crafting Magazine" gibi gerçek progression karşılığı olmayan satırlar ikonsuz kaldı (kasıtlı).

Build 0 Warning/0 Error. Test edildi — ikonlar çalışıyor (bkz. docs/image.png), `perkIronGut` beklendiği gibi ikonsuz (gerçek progression değil, ayrı bug).

## Layout revizyonu: 2 sütunlu stat grid

Kullanıcı geri bildirimi: font/icon çok küçük, panelin sağ tarafı boş kalıyor (tek sütun stacking), buton çok aşağıda/boşluk fazla.

Fix: `statIcon0-9`/`statLabel0-9` artık 2 sütun x 5 satır (index 0-4 sol sütun, index 5-9 sağ sütun — column-major, C# `ApplyStatRows` değişmedi). Icon 14→22px, font 13→16. `btnConfirmKit` y=-655 → y=-630.

## Layout bug: uzun metinler satır taşırıp buton/alt satır üzerine biniyordu (Farmer örneği)

Sabit satır yüksekliği (30px) ile `max_line_count` sınırı olmayan label'lar birlikte, uzun metinler ("Medium Armor 4/4 & Fullset Farmer Armor (1 lvl set)", "Super Corn Crafting Magazine (Automatically Readed)") 2-3 satıra sarıp bir sonraki satırın/butonun üzerine taşıyordu.

Fix:
1. Tüm `statLabel0-9`'a `max_line_count="1"` eklendi — güvenlik ağı, artık hiçbir metin bir sonraki satıra taşamaz.
2. Sütun genişlikleri büyütüldü (sol 165→185, sağ 175→160, icon x kaydırıldı 225→245) — daha fazla karakter tek satıra sığsın diye.
3. En uzun ~12 StatEntry metni kısaltıldı (örn. "Medium Armor 4/4 & Fullset Farmer Armor (1 lvl set)" → "Farmer Armor Set (Lvl 1)", "Electrician Crafting Skills 55/100" → "Electrician Skill 55/100" vb.) — kırpılmadan tek satıra sığmaları için.

## Iron Gut + Super Corn Magazine — ikonsuz kalmaları extraction sorunu değil

Kullanıcı tekrar sordu. İkisi de gerçek bir `ProgressionClass` değil:
- `perkIronGut`: `progression.xml`'de hiç yok (daha önce bulduğumuz ayrı bug — Huntsman/Tyson'da vaat edilen bu floor hiçbir zaman uygulanmıyor).
- Super Corn satırı zaten bir progression floor değil, `KitRewards.CustomVars["plantedGraceCorn1"]` — hiç `ProgressionFloors` girişi yok, yani ikon sistemi (progression name → icon) buna zaten uygulanamaz.

Kalıcı çözüm gerekirse iki seçenek var: (a) bu satırlara elle generic bir ikon ata (örn. "gift"/"unlock" sembolü), (b) `perkIronGut` bug'ını gerçek bir progression'a çevirip düzelt.

**Denendi ama geri alındı (kullanıcı beğenmedi, "olmamış" dedi):** 4 satıra generic ikon atanmıştı (Iron Gut→medical, Super Corn Crafting→book, Commando/Fullset Farmer Outfit→light_armor) — kullanıcı bunu istemedi, 4'ü de tekrar ikonsuz haline döndürüldü. Bu satırlar hâlâ progression karşılığı yok (bkz. yukarı), ikon eklenmesi gerekirse farklı bir yaklaşım denenmeli — kullanıcıya sorulmadan tekrar otomatik ikon atanmasın.

## Farmer: uzun satırlar kısaltma yerine ayrı satıra bölündü, grid 10→12 slot

Kullanıcı kısaltmaları beğenmedi ("o kadar küçültmene gerek yok"), tam metin istedi. `MaxStatRows` 10'dan 12'ye çıkarıldı (6 satır x 2 sütun), C# array boyutları ve XML grid'i buna göre yeniden üretildi (`start_y=-415, row_h=30`, aynı sütun x konumları). Farmer'ın "Farmer Armor Set (Lvl 1)" tek satırı ikiye bölündü: "Medium Armor 4/4" (`ui_game_symbol_light_armor2`) + "Fullset Farmer Outfit" (`ui_game_symbol_light_armor`). "Super Corn Magazine (Auto)" → "Super Corn Crafting" oldu. Tüm kitler 12 slot sınırının altında (en yüksek: Farmer 11).

## Sıradaki adımlar

- Oyun içinde gerçek testi yapılmadı henüz (mod yüklenip crash olmadan açılıyor mu, starterKitGroup penceresi çalışıyor mu, floor patch'in 6/7 hedefi çalışıyor mu — `GetLevel()` hâlâ eksik ama try/catch'li, crash etmiyor).
- `ProgressionValue.Level` skill tipinde her zaman MaxLevel dönüyor — floor mantığının bunu nasıl etkilediği oyun içinde doğrulanmalı.

## Diğer branch'ler hakkında kural

v2.5 / v2.6 branch'lerine SADECE referans/karşılaştırma için bakılır, hiçbir zaman yazılmaz. Aktif geliştirme sadece `v3.1-Staging` üzerinde.

