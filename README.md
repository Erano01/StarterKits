## VS Code Setup Instructions

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