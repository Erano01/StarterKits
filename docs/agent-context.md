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

## Sıradaki adımlar

- Oyun içinde gerçek testi yapılmadı henüz (mod yüklenip crash olmadan açılıyor mu, starterKitGroup penceresi çalışıyor mu, floor patch'in 6/7 hedefi çalışıyor mu — `GetLevel()` hâlâ eksik ama try/catch'li, crash etmiyor).
- `ProgressionValue.Level` skill tipinde her zaman MaxLevel dönüyor — floor mantığının bunu nasıl etkilediği oyun içinde doğrulanmalı.

## Diğer branch'ler hakkında kural

v2.5 / v2.6 branch'lerine SADECE referans/karşılaştırma için bakılır, hiçbir zaman yazılmaz. Aktif geliştirme sadece `v3.1-Staging` üzerinde.

