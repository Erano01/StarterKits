## Notes for the Contributers

## VSCode Instructions
If you're using VS Code, follow these steps:

1. Create the project as a Class Library (.NET Framework) — not as a regular .NET project.
2. Preferably target .NET Framework 4.8.

Install your project under:

```text
C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods
```

In the same folder, manually copy the `0_TFP_Harmony` mod from:

```text
C:\Program Files (x86)\Steam\steamapps\common\7 Days to Die Dedicated Server\Mods
```

Make sure the C# Dev Kit extension is installed in VS Code.

To build the project, use the following command:

```powershell
dotnet msbuild ".\StarterKits.csproj" /p:Configuration=Debug
```


Important Locations:
```text
%APPDATA%\7DaysToDie\logs
C:\Users\rokel\AppData\Roaming\7DaysToDie\Mods
C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods
C:\Program Files (x86)\Steam\steamapps\common\7 Days to Die Dedicated Server\Mods

--for xml modding doc--
C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Data\Config
```

You have to use dnSPY to view ModAPI documentation like javadoc.
Edit -> search assemblies -> Sınıf modul arama için
File -> Open -> API neredeyse oyun dosyaları

Assembly-CSharp.dll'i açmak yeterli.

## Development Formula
use events -(if not possible)> use harmony patches -(if not possible)> use scripts -(if not possible)> use transpilers

if you are developing UI's(XML) you can reload UI without restarting game.

use this in game when you change the UI: 
```text
xui reload
```

## Notes for development

--- XUi ---
Mantık ve düzen tarafı.
Pencerelerin, butonların, label’ların, layout’un tanımlandığı sistem.
windows.xml gibi dosyalarda <window>, <button>, <label> yazdığın yer burası.
C# tarafında XUiController / XUiC_* sınıflarıyla davranış (event, tıklama, açılma-kapanma) kontrol edilir.

Oyun içinde test için, spawn olduktan sonra konsolda şu komutları dene:
xui openwindow starterKitWindow
xui openwindow starterKitGroup


--- UIAtlases ---
Görsel kaynak tarafı.
UI’da kullanılan tüm ikonlar, buton görselleri, arka planlar tek bir texture atlas içinde toplanır.
Atlas XML’i: “şu görsel atlasın şu koordinatında” bilgisini tutar.
XUi içindeki <sprite>, <button> vs. elementler görsel olarak ne kullanacaksa sprite/atlas isimleriyle buraya referans verir.

https://7daystodie.fandom.com/wiki/ModAPI
https://7daystodie.fandom.com/wiki/XPath_Explained#String_Searches
https://7daystodie.fandom.com/wiki/Mod_Structure