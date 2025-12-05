using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class XUiC_KitSelectionMenu : XUiController
{
    public override void OnOpen()
    {
        var player = GameManager.Instance.World.GetPrimaryPlayer();
        if (!player.Buffs.HasCustomVar("starterKitSelected"))
        {
            xui.playerUI.windowManager.Open("starterKitWindow", true);
        }
    }

    public void eventChooseKit(XUiController sender)
    {
        string kit = sender.;

        // Item / buff / class verme
        //GiveStarterKit(kit);

        // Seçildi olarak işaretle
        var player = GameManager.Instance.World.GetPrimaryPlayer();
        player.Buffs.SetCustomVar("starterKitSelected", 1);

        // Pencereyi kapat
        xui.playerUI.windowManager.Close("starterKitWindow");
    }

}