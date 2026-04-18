// using System;
// using System.Collections;
// using System.Runtime.CompilerServices;
// using Platform;
// using UnityEngine.Scripting;

// // Token: 0x02000CC8 RID: 3272
// [Preserve]
// public class XUiC_InGameMenuWindow : XUiController
// {
// 	// Token: 0x06006534 RID: 25908 RVA: 0x002908B8 File Offset: 0x0028EAB8
// 	public override void Init()
// 	{
// 		base.Init();
// 		XUiC_InGameMenuWindow.ID = base.WindowGroup.ID;
// 		this.unstuckPrompt = base.GetChildByType<XUiC_ConfirmationPrompt>();
// 		this.btnInvite = base.GetChildById("btnInvite").GetChildByType<XUiC_SimpleButton>();
// 		this.btnInvite.OnPressed += this.BtnInvite_OnPressed;
// 		this.btnOptions = base.GetChildById("btnOptions").GetChildByType<XUiC_SimpleButton>();
// 		this.btnOptions.OnPressed += this.BtnOptions_OnPressed;
// 		this.btnHelp = base.GetChildById("btnHelp").GetChildByType<XUiC_SimpleButton>();
// 		this.btnHelp.OnPressed += this.BtnHelp_OnPressed;
// 		this.btnSave = base.GetChildById("btnSave").GetChildByType<XUiC_SimpleButton>();
// 		this.btnSave.OnPressed += this.BtnSave_OnPressed;
// 		this.btnExit = base.GetChildById("btnExit").GetChildByType<XUiC_SimpleButton>();
// 		this.btnExit.OnPressed += this.BtnExit_OnPressed;
// 		this.btnExportPrefab = base.GetChildById("btnExportPrefab").GetChildByType<XUiC_SimpleButton>();
// 		this.btnExportPrefab.OnPressed += this.BtnExportPrefab_OnPressed;
// 		this.btnTpPoi = base.GetChildById("btnTpPoi").GetChildByType<XUiC_SimpleButton>();
// 		this.btnTpPoi.OnPressed += this.BtnTpPoi_OnPressed;
// 		XUiController childById = base.GetChildById("btnUnstuck");
// 		this.btnUnstuck = ((childById != null) ? childById.GetChildByType<XUiC_SimpleButton>() : null);
// 		if (this.btnUnstuck != null)
// 		{
// 			this.btnUnstuck.OnPressed += this.BtnUnstuck_OnPressed;
// 		}
// 		this.btnOpenConsole = base.GetChildById("btnOpenConsole").GetChildByType<XUiC_SimpleButton>();
// 		this.btnOpenConsole.OnPressed += this.BtnOpenConsole_OnPressed;
// 		this.btnBugReport = base.GetChildById("btnBugReport").GetChildByType<XUiC_SimpleButton>();
// 		this.btnBugReport.OnPressed += this.BtnBugReport_OnPressed;
// 		XUiController xuiController = base.xui.FindWindowGroupByName(XUiC_InGameMenuWindow.ServerInfoWindowGroupName);
// 		if (xuiController != null)
// 		{
// 			this.serverInfoWindowGroup = xuiController.WindowGroup.ID;
// 		}
// 	}

// 	// Token: 0x06006535 RID: 25909 RVA: 0x00290ADD File Offset: 0x0028ECDD
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void BtnInvite_OnPressed(XUiController _sender, int _mouseButton)
// 	{
// 		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
// 		IMultiplayerInvitationDialog multiplayerInvitationDialog = PlatformManager.NativePlatform.MultiplayerInvitationDialog;
// 		if (multiplayerInvitationDialog == null)
// 		{
// 			return;
// 		}
// 		multiplayerInvitationDialog.ShowInviteDialog();
// 	}

// 	// Token: 0x06006536 RID: 25910 RVA: 0x00290B13 File Offset: 0x0028ED13
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void BtnOptions_OnPressed(XUiController _sender, int _mouseButton)
// 	{
// 		this.continueGamePause = true;
// 		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
// 		LocalPlayerUI.primaryUI.windowManager.Open(XUiC_OptionsMenu.ID, true, false, true);
// 	}

// 	// Token: 0x06006537 RID: 25911 RVA: 0x00290B53 File Offset: 0x0028ED53
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void BtnHelp_OnPressed(XUiController _sender, int _mouseButton)
// 	{
// 		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
// 		base.xui.playerUI.windowManager.Open(XUiC_PrefabEditorHelp.ID, true, false, true);
// 	}

// 	// Token: 0x06006538 RID: 25912 RVA: 0x00290B94 File Offset: 0x0028ED94
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void BtnSave_OnPressed(XUiController _sender, int _mouseButton)
// 	{
// 		if (PrefabEditModeManager.Instance.IsActive())
// 		{
// 			XUiC_SaveDirtyPrefab.Show(base.xui, new Action<XUiC_SaveDirtyPrefab.ESelectedAction>(this.savePrefab), XUiC_SaveDirtyPrefab.EMode.ForceSave);
// 			return;
// 		}
// 		GameManager.Instance.SaveLocalPlayerData();
// 		GameManager.Instance.SaveWorld();
// 		GameManager.ShowTooltip(GameManager.Instance.World.GetLocalPlayers()[0], Localization.Get("xuiWorldEditorSaved", false), false, false, 0f);
// 	}

// 	// Token: 0x06006539 RID: 25913 RVA: 0x00290C06 File Offset: 0x0028EE06
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void savePrefab(XUiC_SaveDirtyPrefab.ESelectedAction _action)
// 	{
// 		base.xui.playerUI.windowManager.Open(XUiC_InGameMenuWindow.ID, true, false, true);
// 	}

// 	// Token: 0x0600653A RID: 25914 RVA: 0x00290C25 File Offset: 0x0028EE25
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void BtnExit_OnPressed(XUiController _sender, int _mouseButton)
// 	{
// 		if (PrefabEditModeManager.Instance.IsActive() && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
// 		{
// 			XUiC_SaveDirtyPrefab.Show(base.xui, new Action<XUiC_SaveDirtyPrefab.ESelectedAction>(this.exitGame), XUiC_SaveDirtyPrefab.EMode.AskSaveIfDirty);
// 			return;
// 		}
// 		this.exitGame(XUiC_SaveDirtyPrefab.ESelectedAction.DontSave);
// 	}

// 	// Token: 0x0600653B RID: 25915 RVA: 0x00290C60 File Offset: 0x0028EE60
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void exitGame(XUiC_SaveDirtyPrefab.ESelectedAction _action)
// 	{
// 		if (_action == XUiC_SaveDirtyPrefab.ESelectedAction.Cancel)
// 		{
// 			base.xui.playerUI.windowManager.Open(XUiC_InGameMenuWindow.ID, true, false, true);
// 			return;
// 		}
// 		GameManager.Instance.SetActiveBlockTool(null);
// 		if (PlatformApplicationManager.IsRestartRequired)
// 		{
// 			ThreadManager.StartCoroutine(this.DisconnectAfterDisplayingExitingGameMessage());
// 			return;
// 		}
// 		GameManager.Instance.Disconnect();
// 	}

// 	// Token: 0x0600653C RID: 25916 RVA: 0x00290CB8 File Offset: 0x0028EEB8
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public IEnumerator DisconnectAfterDisplayingExitingGameMessage()
// 	{
// 		yield return GameManager.Instance.ShowExitingGameUICoroutine();
// 		GameManager.Instance.Disconnect();
// 		yield break;
// 	}

// 	// Token: 0x0600653D RID: 25917 RVA: 0x00290CC0 File Offset: 0x0028EEC0
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void BtnExportPrefab_OnPressed(XUiController _sender, int _mouseButton)
// 	{
// 		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
// 		XUiC_ExportPrefab.Open(base.xui);
// 	}

// 	// Token: 0x0600653E RID: 25918 RVA: 0x00290CED File Offset: 0x0028EEED
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void BtnTpPoi_OnPressed(XUiController _sender, int _mouseButton)
// 	{
// 		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
// 		base.xui.playerUI.windowManager.Open(XUiC_PoiTeleportMenu.ID, true, false, true);
// 	}

// 	// Token: 0x0600653F RID: 25919 RVA: 0x00290D2C File Offset: 0x0028EF2C
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void BtnUnstuck_OnPressed(XUiController _sender, int _mouseButton)
// 	{
// 		this.unstuckPrompt.ShowPrompt(Localization.Get("xuiMenuUnstuck", false), Localization.Get("xuiMenuUnstuckConfirmation", false), Localization.Get("xuiCancel", false), Localization.Get("btnOk", false), new Action<XUiC_ConfirmationPrompt.Result>(this.<BtnUnstuck_OnPressed>g__UnstuckConfirmed|26_0));
// 	}

// 	// Token: 0x06006540 RID: 25920 RVA: 0x00290D7C File Offset: 0x0028EF7C
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void BtnOpenConsole_OnPressed(XUiController _sender, int _mouseButton)
// 	{
// 		GameManager.Instance.SetConsoleWindowVisible(!GameManager.Instance.m_GUIConsole.isShowing);
// 		base.xui.playerUI.windowManager.Close(XUiC_InGameMenuWindow.ID);
// 	}

// 	// Token: 0x06006541 RID: 25921 RVA: 0x00290DB4 File Offset: 0x0028EFB4
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void BtnBugReport_OnPressed(XUiController _sender, int _mouseButton)
// 	{
// 		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
// 		XUiC_BugReportWindow.Open(base.xui, false);
// 	}

// 	// Token: 0x06006542 RID: 25922 RVA: 0x00290DE4 File Offset: 0x0028EFE4
// 	public override void OnOpen()
// 	{
// 		base.OnOpen();
// 		if (PlatformManager.NativePlatform.MultiplayerInvitationDialog != null)
// 		{
// 			int num = SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount() + (GameManager.IsDedicatedServer ? 0 : 1);
// 			int @int = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);
// 			this.btnInvite.Enabled = (PlatformManager.NativePlatform.MultiplayerInvitationDialog.CanShow && (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || num < @int));
// 			this.btnInvite.ViewComponent.IsVisible = (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || SingletonMonoBehaviour<ConnectionManager>.Instance.HasRunningServers);
// 		}
// 		else
// 		{
// 			this.btnInvite.ViewComponent.IsVisible = false;
// 		}
// 		this.btnSave.ViewComponent.IsVisible = (GameManager.Instance.IsEditMode() && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer);
// 		this.btnHelp.ViewComponent.IsVisible = GameManager.Instance.IsEditMode();
// 		this.btnExportPrefab.ViewComponent.IsVisible = GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled);
// 		this.btnTpPoi.ViewComponent.IsVisible = (GameManager.Instance.GetDynamicPrefabDecorator() != null && (GameManager.Instance.IsEditMode() || GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled)));
// 		this.continueGamePause = false;
// 		GameManager.Instance.Pause(true);
// 		XUiC_FocusedBlockHealth.SetData(base.xui.playerUI, null, 0f);
// 		base.xui.playerUI.windowManager.Close("toolbelt");
// 		XUi.InGameMenuOpen = true;
// 		if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
// 		{
// 			base.xui.playerUI.windowManager.Open(XUiC_EditorPanelSelector.ID, false, false, true);
// 		}
// 		if (this.serverInfoWindowGroup != null)
// 		{
// 			base.xui.playerUI.windowManager.Open(this.serverInfoWindowGroup, false, false, true);
// 		}
// 		if (this.btnInvite.ViewComponent.IsVisible)
// 		{
// 			this.btnInvite.SelectCursorElement(true, false);
// 		}
// 		else
// 		{
// 			this.btnOptions.SelectCursorElement(true, false);
// 		}
// 		base.RefreshBindings(false);
// 	}

// 	// Token: 0x06006543 RID: 25923 RVA: 0x00290FEC File Offset: 0x0028F1EC
// 	public override void OnClose()
// 	{
// 		base.OnClose();
// 		base.xui.playerUI.windowManager.Close(XUiC_EditorPanelSelector.ID);
// 		if (!this.continueGamePause)
// 		{
// 			GameManager.Instance.Pause(false);
// 			XUi.InGameMenuOpen = false;
// 		}
// 		if (this.serverInfoWindowGroup != null)
// 		{
// 			base.xui.playerUI.windowManager.Close(this.serverInfoWindowGroup);
// 		}
// 	}

// 	// Token: 0x06006544 RID: 25924 RVA: 0x00291058 File Offset: 0x0028F258
// 	public override void Update(float _dt)
// 	{
// 		base.Update(_dt);
// 		if (PrefabEditModeManager.Instance.IsActive())
// 		{
// 			bool enabled = PrefabEditModeManager.Instance.VoxelPrefab != null;
// 			this.btnSave.Enabled = enabled;
// 		}
// 		this.btnExportPrefab.Enabled = BlockToolSelection.Instance.SelectionActive;
// 	}

// 	// Token: 0x06006545 RID: 25925 RVA: 0x002910A8 File Offset: 0x0028F2A8
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
// 	{
// 		if (_bindingName == "creativeenabled")
// 		{
// 			_value = AchievementUtils.IsCreativeModeActive().ToString();
// 			return true;
// 		}
// 		if (_bindingName == "bug_reporting")
// 		{
// 			_value = BacktraceUtils.BugReportFeature.ToString();
// 			return true;
// 		}
// 		if (!(_bindingName == "console_button"))
// 		{
// 			return base.GetBindingValueInternal(ref _value, _bindingName);
// 		}
// 		_value = GamePrefs.GetBool(EnumGamePrefs.OptionsShowConsoleButton).ToString();
// 		return true;
// 	}

// 	// Token: 0x06006548 RID: 25928 RVA: 0x00291134 File Offset: 0x0028F334
// 	[CompilerGenerated]
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void <BtnUnstuck_OnPressed>g__UnstuckConfirmed|26_0(XUiC_ConfirmationPrompt.Result result)
// 	{
// 		if (result == XUiC_ConfirmationPrompt.Result.Confirmed)
// 		{
// 			base.xui.playerUI.entityPlayer.RequestUnstuck();
// 			base.xui.playerUI.windowManager.Close(XUiC_InGameMenuWindow.ID);
// 		}
// 	}

// 	// Token: 0x04004C72 RID: 19570
// 	public static string ID = "";

// 	// Token: 0x04004C73 RID: 19571
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public XUiC_SimpleButton btnInvite;

// 	// Token: 0x04004C74 RID: 19572
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public XUiC_SimpleButton btnOptions;

// 	// Token: 0x04004C75 RID: 19573
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public XUiC_SimpleButton btnHelp;

// 	// Token: 0x04004C76 RID: 19574
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public XUiC_SimpleButton btnSave;

// 	// Token: 0x04004C77 RID: 19575
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public XUiC_SimpleButton btnExit;

// 	// Token: 0x04004C78 RID: 19576
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public XUiC_SimpleButton btnExportPrefab;

// 	// Token: 0x04004C79 RID: 19577
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public XUiC_SimpleButton btnTpPoi;

// 	// Token: 0x04004C7A RID: 19578
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public XUiC_SimpleButton btnUnstuck;

// 	// Token: 0x04004C7B RID: 19579
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public XUiC_SimpleButton btnOpenConsole;

// 	// Token: 0x04004C7C RID: 19580
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public XUiC_SimpleButton btnBugReport;

// 	// Token: 0x04004C7D RID: 19581
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public XUiC_ConfirmationPrompt unstuckPrompt;

// 	// Token: 0x04004C7E RID: 19582
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public bool continueGamePause;

// 	// Token: 0x04004C7F RID: 19583
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public static readonly string ServerInfoWindowGroupName = "serverinfowindow";

// 	// Token: 0x04004C80 RID: 19584
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public string serverInfoWindowGroup;
// }
