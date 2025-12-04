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
```



## Notes for development

--- XUi ---
Mantık ve düzen tarafı.
Pencerelerin, butonların, label’ların, layout’un tanımlandığı sistem.
windows.xml gibi dosyalarda <window>, <button>, <label> yazdığın yer burası.
C# tarafında XUiController / XUiC_* sınıflarıyla davranış (event, tıklama, açılma-kapanma) kontrol edilir.


--- UIAtlases ---
Görsel kaynak tarafı.
UI’da kullanılan tüm ikonlar, buton görselleri, arka planlar tek bir texture atlas içinde toplanır.
Atlas XML’i: “şu görsel atlasın şu koordinatında” bilgisini tutar.
XUi içindeki <sprite>, <button> vs. elementler görsel olarak ne kullanacaksa sprite/atlas isimleriyle buraya referans verir.