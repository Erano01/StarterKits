## StarterKits - 7 Days To Die Mod
Starter Kits lets players choose a starting kit that shapes not just their gear, but their character build. Each kit unlocks a curated set of sub-skills within specific skill categories, without requiring the root skill that would normally gate them, so players can jump straight into the playstyle they want (e.g. a stealth archer, a demolitions specialist, a medic) instead of spending hours grinding skill points just to reach the perks that make that build actually work.

In vanilla 7 Days to Die, committing to a roleplay build usually means a long detour through the skill tree before it becomes viable. Starter Kits removes that friction: pick a kit, and the relevant sub-skills are already active, letting the player express their chosen build from the very start, without the game forcing a generic, one-size-fits-all early game on them.

Kit selection is tracked per world and per save slot, once chosen, it's locked in for that save (reconnecting won't reopen the selection, even for admins), and picks are made through a custom in-game UI built on 7DTD's XUi system.

## Custom Kits (Kit Editor)

Since 1.3 kits are no longer hard-coded. Open **Starter Kits** in the main menu (above Quit) to:

- edit the 14 built-in kits or create, duplicate and delete your own,
- grant any attribute, perk (grouped Attribute > Category > Perk), book, whole book set or magazine, with a level,
- give any item, item mod or player-placeable block, with live search suggestions, count and quality,
- pick a kit picture from the built-in portraits, any icon, or **Import** your own .png/.jpg and crop it to the portrait size,
- let the "Kit Stats" lines be generated automatically or keep hand-written ones.

Files:

```text
Mods/StarterKits/Kits/defaults.xml     built-in kits (shipped, read-only)
Mods/StarterKits/Kits/kits.xml         your kits (created on first Save, safe across mod updates)
Mods/StarterKits/Kits/Images/          imported kit pictures (200x200 png)
Mods/StarterKits/Kits/ImageImport/     drop pictures here to import them
```

`kits.xml` can also be edited by hand. Changes apply the next time the kit window opens; no restart needed.

**Multiplayer:** the server sends its own `kits.xml` and the pictures those kits use to every player when they join. Players only need the mod installed (same version as the server); they never copy kit files. To set up a server, build the kits in your own game and copy `Kits/kits.xml` and `Kits/Images/` to the server's `Mods/StarterKits/Kits/`.

## Notes for the Contributers

## VSCode Instructions
If you're using VS Code, follow these steps:

1. Create the project as a Class Library (.NET Framework) — not as a regular .NET project.
2. Preferably target .NET Framework 4.8.

Install your project under:

```text
C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods
or

"/home/{user}/.local/share/Steam/steamapps/common/7 Days To Die/Mods"
```

In the same folder, manually copy the `0_TFP_Harmony` mod from:

```text
C:\Program Files (x86)\Steam\steamapps\common\7 Days to Die Dedicated Server\Mods

"/home/{user}/.local/share/Steam/steamapps/common/7 Days to Die Dedicated Server/"

```

Make sure the C# Dev Kit extension is installed in VS Code.

To build the project, use the following command:

```powershell
// windows
dotnet msbuild ".\StarterKits.csproj" /p:Configuration=Debug

// linux
dotnet msbuild StarterKits.csproj /restore /p:Configuration=Debug
//or
dotnet build StarterKits.csproj -v minimal

```


Important Locations:
```text
%APPDATA%\7DaysToDie\logs
C:\Users\rokel\AppData\Roaming\7DaysToDie\Mods
C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods
C:\Program Files (x86)\Steam\steamapps\common\7 Days to Die Dedicated Server\Mods

--for xml modding doc--
C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Data\Config


//linux log files
/home/erano/.local/share/7DaysToDie/logs/

//dlls are here:
/home/{user}/.local/share/Steam/steamapps/common/7 Days To Die/7DaysToDie_Data/Managed/

//if you are linux and using dnspy via AUR:
Z:\home\{user}\.local\share\Steam\steamapps\common\7 Days To Die\7DaysToDie_Data\Managed\Assembly-CSharp.dll

```

You have to use dnSPY to view ModAPI documentation like javadoc.
Edit -> search assemblies -> Sınıf modul arama için
File -> Open -> API neredeyse oyun dosyaları

Assembly-CSharp.dll'i açmak yeterli.

## Development Formula
use xml(modlet) -(if not possible)> use GameAPI events -(if not possible)> use harmony patches -(if not possible)> use scripts -(if not possible)> use transpilers

## Starter Kit DB Behavior

Starter kit selection is persisted per active world/save slot.

Path format:

```text
Mods/StarterKits/Data/saves/<world-name>/<save-name>/db.txt
```

Examples:

```text
Mods/StarterKits/Data/saves/Navezgane/test/db.txt
Mods/StarterKits/Data/saves/Navezgane/test2/db.txt
Mods/StarterKits/Data/saves/RWG8K02/Survival/db.txt
Mods/StarterKits/Data/saves/RWG10K01/Hardcore/db.txt
```

Rules:

- A player can select a starter kit only once for that save.
- Rejoin/reconnect does not reopen selection if already chosen.
- This restriction applies regardless of admin status.
- On mod load, the mod scans known worlds/saves and logs them for diagnostics.
- If save path is not ready yet during early load, the selection lock is still kept in memory and flushed when save path becomes available.

if you are developing UI's(XML) you can reload UI without restarting game.

use this To easily test changes in the UI, use f1 and then
```text
xui reload
```
To quickly see changes you made.

dm -> debugmenu for godmode

## Notes for development

--- XUi ---
Mantık ve düzen tarafı.
Pencerelerin, butonların, label’ların, layout’un tanımlandığı sistem.
windows.xml gibi dosyalarda <window>, <button>, <label> yazdığın yer burası.
C# tarafında XUiController / XUiC_* sınıflarıyla davranış (event, tıklama, açılma-kapanma) kontrol edilir.

Oyun içinde test için, spawn olduktan sonra konsolda şu komutları dene:
```
xui openwindow starterKitWindow
xui openwindow starterKitGroup
```


--- UIAtlases ---
Görsel kaynak tarafı.
UI’da kullanılan tüm ikonlar, buton görselleri, arka planlar tek bir texture atlas içinde toplanır.
Atlas XML’i: “şu görsel atlasın şu koordinatında” bilgisini tutar.
XUi içindeki <sprite>, <button> vs. elementler görsel olarak ne kullanacaksa sprite/atlas isimleriyle buraya referans verir.

https://7daystodie.fandom.com/wiki/ModAPI
https://7daystodie.fandom.com/wiki/XPath_Explained#String_Searches
https://7daystodie.fandom.com/wiki/Mod_Structure

--- Quartz is also good for viewing UIAtlases ---
// Console command -> Quartz UIAtlas
https://github.com/s7092910/Quartz