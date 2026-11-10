
// oyunda test ettikten sonra oluşan loglar burada oluşuyor.
/home/erano/.local/share/7DaysToDie/logs/

//dlls are here:
/home/{user}/.local/share/Steam/steamapps/common/7 Days To Die/7DaysToDie_Data/Managed/

//xml doc here:
/home/{user}/.local/share/Steam/steamapps/common/7 Days To Die/Data/Config/

--------------------




## docs/ içeriği (V 3.4.0 b10'a göre)

- `docs/xml/` → oyunun `Data/Config/` klasörünün birebir kopyası (XUi_Common / XUi_InGame / XUi_Menu dahil).
  `XML.txt` = TFP'nin resmi XML modding dokümanı. `Localization.csv` (16MB) repo'yu şişirmesin diye kopyalanmadı,
  gerekirse doğrudan oyun klasöründen bakılır.
- `docs/dnSpy/` → modun kullandığı oyun tiplerinin Assembly-CSharp.dll'den decompile edilmiş hali.
  `StarterKits.csproj` içinde `docs\**` derlemeden hariç tutuluyor.

Oyun güncellenince yenilemek için (repo kökünden):

```bash
rm -rf docs/xml && mkdir -p docs/xml && cp -a ../../Data/Config/. docs/xml/ && rm docs/xml/Localization.csv

export DOTNET_ROLL_FORWARD=Major   # ilspycmd 9.x yeni .NET runtime'da bunu istiyor
for t in XUiController XUiView XUiC_InGameMenuWindow XUiC_SimpleButton XUiV_Sprite XUiV_Label \
         ProgressionValue ProgressionClass Progression ModEvents EntityBuffs ItemStack ItemValue; do
  ~/.dotnet/tools/ilspycmd -t "$t" ../../7DaysToDie_Data/Managed/Assembly-CSharp.dll > "docs/dnSpy/$t.cs"
done
sed -i '/^You are not using the latest version/d;/^Latest version is /d' docs/dnSpy/*.cs
```
