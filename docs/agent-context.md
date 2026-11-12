// Kendi contextini buraya yazabilirsin daha sonra kullanmak için, dökümantasyondan elde ettiğin bilgileri vesaire:

# Proje referansı (v3.1)

## Kural: C# değişikliği restart ister, XML değişikliği `xui reload` yeter

Mod XML patch'leri (append/xpath) ve C# static data (`KitOverview` dictionary'si dahil) sadece oyun **boot**'unda bir kere yükleniyor. `xui reload` sadece zaten yüklenmiş XUi layout'unu yeniden çiziyor, ne mod XML patch'lerini yeniden uygulıyor ne C# assembly'sini yeniden yüklüyor. Kural: Src/*.cs değiştiyse → tam restart. Sadece Config/XUi_InGame/*.xml değiştiyse → `xui reload` yeter.

## v3.1'in v2.5/v2.6'dan API farkları (Assembly-CSharp.dll, dnSpy ile doğrulandı)

- `Config/XUi/` tek klasördü, v3.1'de `XUi_Common` / `XUi_InGame` / `XUi_Menu` olarak üçe bölündü. Bizim mod dosyaları `Config/XUi_InGame/` altında (ESC menüsü ve starter kit penceresi oyun-içi olduğu için).
- `xui.xml`'de `<ruleset>` elementi kaldırıldı. `<xui>` artık `window_group`'ları doğrudan child alıyor — xpath `/xui/ruleset[@name='default']` değil `/xui`.
- `XUiView.IsDirty` artık public settable property değil (private field). Dışarıdan `SetDirty()` metoduyla tetiklenir.
- `backgroundcolor` attribute'ü `<panel>`/`<rect>` üzerinde artık render edilmiyor (handler yok). Solid arka plan için explicit `<sprite sprite="menu_empty3px" .../>` child'ı gerekiyor.
- O sprite'ta `fillcenter="false"` = merkez **Invisible** (sadece ince border kalır, dolu görünmez). Solid dolgu için `fillcenter="true"` şart.
- `globalopacitymod="0"` = oyuncunun UI background opacity slider ayarından bağımsız, sprite'ın kendi alpha'sını hiç çarpmadan kullan.
- `Audio.Client.Play()` imzasına `volumeScale` parametresi eklendi (eski imza artık yok). Bu yüzden repo'daki eski `Harmony/Examples.cs` (tutorial kodu, hiç kullanılmıyordu) `harmony.PatchAll()`'da crash atıp **tüm modun** init olmasını engelliyordu — dosya silindi.
- `ProgressionValue.GetLevel()` kaldırıldı (floor patch hâlâ bunu hedefliyor ama `TryPatch` sessizce atlıyor, crash yok — 6/7 floor hedefi hâlâ çalışıyor).
- `ProgressionValue.Level`, Skill tipi progression class'larda her zaman `ProgressionClass.MaxLevel` döner, `level` field'ı ignore edilir (v2.5/2.6'dan farklı olabilir).

## Perk/skill ikon sistemi

- Gerçek atlas adı **`UIAtlas`** (Discord'dan doğrulandı — TFP çalışanı). `progression.xml`'deki `icon="ui_game_symbol_*"` değerleri buradan geliyor. Bu atlas sıkıştırılmış Unity AssetBundle içinde, extraction/görsel doğrulama yapılamıyor — sadece isim eşleşmesiyle güveniliyor.
- Eşya ikonları (kit portreleri gibi) ayrı bir atlas: `ItemIconAtlas`, mod'un kendi `UIAtlases/ItemIconAtlas/*.png` dosyalarıyla besleniyor. Vanilla eşya ikonları da aynı atlas'tan (`items.xml`'deki `CustomIcon`/item adı → `Data/ItemIcons/*.png` ile çapraz doğrulanabilir).
- `Src/XUiC_KitSelectionMenu.cs`: `StatEntry(text, iconSpriteName, iconAtlas="UIAtlas", iconColorHex=null)` — satır bazında farklı atlas/renk verilebiliyor (örn. Farmer'ın "Super Corn Crafting" satırı `ItemIconAtlas` + `plantedCorn1` + `ff9f9f` tint kullanıyor, diğerleri `UIAtlas`).
- UI'de "Kit Stats" grid'i: `Config/XUi_InGame/windows.xml`, `statIcon0..11` + `statLabel0..11` (12 slot, 2 sütun x 6 satır, `MaxStatRows` sabiti C#'ta). `ApplyStatRows()` doldurur/gizler.

## Düzeltilmiş bug: `perkIronGut` yanlış key'di

`KitRewards.ProgressionFloors`'da "Iron Gut" ödülü `perkIronGut` diye yanlış bir key'le tanımlıydı — bu isim `progression.xml`'de hiç yok, floor hiçbir zaman uygulanmıyordu. Gerçek internal ad: **`perkSlowMetabolism`** (localization key'i "Iron Gut", `parent="skillFortitudeRecovery"`, `icon="ui_game_symbol_stomach"`). Düzeltildi, artık Huntsman/Tyson gerçekten bu floor'u alıyor.

## Deploy / dağıtım

`deployscript.sh` (repo kökünde) → `ModInfo.xml`, `StarterKits.dll`, `Config/`, `UIAtlases/` dosyalarını `DEPLOY_DIR`'a kopyalayıp zip'liyor (kaynak kod, docs, .git hariç). Her çalıştırmada `DEPLOY_DIR`'ı önce temizliyor (`rm -rf`).

`0_TFP_Harmony` bağımlılığı ayrıca dağıtılmıyor — vanilla oyun kurulumu bunu zaten `Mods/` altında getiriyor.

GitHub Release: CI/CD şart değil, `gh release create <tag> <zip> --target <branch>` ile manuel yeterli.

## Branch kuralı

Sadece **v3.1** branch'inde çalışılır (v3.1-Staging kullanıcı tarafından silindi). v2.5/v2.6 sadece referans/karşılaştırma için okunur, hiçbir zaman yazılmaz.

## Kit verisi artık XML'de (feature/main-menu-kit-editor)

- Kitler C#'ta değil: `Kits/defaults.xml` (modla gelir, salt okunur) + `Kits/kits.xml` (oyuncunun dosyası, ilk Save'de oluşur, deploy'a girmez, .gitignore'da).
- Ana menü "Starter Kits" butonu → `Src/Editor/XUiC_KitEditorWindow.cs` + `Config/XUi_Menu/windows.xml` (bu XML `gen_editor_xml.py` tarzı script ile üretildi; slot sayıları C# sabitleriyle eşleşmeli: KitRows 14, ProgRows 8, CatRows 11, ItemRows 4).
- Oyun içi seçim penceresi (`XUiC_KitSelectionMenu`) her açılışta `KitRepository.LoadKits()` ile okur → editör değişiklikleri restart gerektirmez. 14 genel slot (`btnKitSlot0..13`) + 14'ten fazla kitte sayfalama, tek önizleme sprite'ı `texKitPreview`.
- Ana menüde `progression.xml` / `items.xml` henüz yüklenmemiş olur (StartGame'de yüklenir; atlas + localization boot'ta hazır). `GameDataCatalog` bu yüzden vanilla `Data/Config` dosyalarını kendisi okur; başka modların eklediği perk/item'lar katalogda yok.
- Hiyerarşi: perk.parent = skill kategorisi (ör. skillAgilityCombat), skill.parent = attribute. 5 attribute'ün hepsinde "Combat Perks" var. General perkler: attGeneralPerks → skillGeneralPerks. Kitap setleri `book_group`, kitaplar `book` (parent = set).
- Item kataloğu: `items.xml` + `item_modifiers.xml` (mod'lar; şemalar items.xml'de) + `blocks.xml` (yalnız CreativeMode=Player). `Extends` param1 = miras ALINMAYACAK özellikler (ör. mod'larda CustomIcon).
- Kit görseli import: atlas'lar boot'ta paketlendiği için yeni PNG atlas'a eklenemez. Import edilen portreler `Kits/Images/<ad>.png` (200x200), kit'te `image_atlas="File"`; UI'da `<texture>` ile gösterilir (`KitImageStore`). Kaynak resimler `Kits/ImageImport/`. NGUI.dll + UnityEngine.ImageConversionModule.dll referansları bunun için eklendi.
- Multiplayer senkronu (`Src/Network/`): sunucu `PlayerSpawning`'de (yedek: `PlayerSpawnedInWorld`) kendi `kits.xml`'ini diskten taze okuyup `NetPackageStarterKitsData` (XML metni, sıkıştırılmış) + kullanılan her `Kits/Images` resmi için 32 KB'lık `NetPackageStarterKitsImage` parçaları gönderir. Uzak istemci (`IsClient && !IsServer`) bunları sadece bellekte tutar, kendi `Kits/` dosyalarını yok sayar; `WorldShuttingDown`'da temizlenir. Host'un kendi oyuncusu (ClientInfo null) dosyaları direkt okur.
- V3.4 NetPackage: `GetLength()` yok. `PooledBinaryWriter`'ın Span overload'ları net48'de derlenmez → `(BinaryWriter)` cast ile yaz.
- Oyun içi seçim penceresi (1.3 redesign): `gen_ingame_xml.py` tarzı script ile üretildi. Slotlar C# sabitleriyle eşleşmeli: KitCardCount 8 (dikey kart listesi, tekerlekle 1'er kayar, kaydırma çubuğu `kitListThumb` Position/Size ile), MaxStatRows 16 (2 sütun, tekerlek = sayfa), ItemSlotCount 10 (quality rengi `QualityInfo.GetQualityColor`). Kartlar `rect` + `on_press/on_hover/on_scroll`; controller'da `OnPress/OnHover/OnScroll` event'leri. Arka plan `fullscreencollider_new` (sadece tıklamayı bloklar, kapatmaz).
