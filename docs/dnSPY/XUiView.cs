// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Globalization;
// using System.Runtime.CompilerServices;
// using Audio;
// using InControl;
// using Platform;
// using UnityEngine;

// // Token: 0x02000F01 RID: 3841
// public class XUiView
// {
// 	// Token: 0x17000C35 RID: 3125
// 	// (get) Token: 0x06007929 RID: 31017 RVA: 0x00314007 File Offset: 0x00312207
// 	// (set) Token: 0x0600792A RID: 31018 RVA: 0x0031400F File Offset: 0x0031220F
// 	public bool RepeatContent { get; set; }

// 	// Token: 0x17000C36 RID: 3126
// 	// (get) Token: 0x0600792B RID: 31019 RVA: 0x00314018 File Offset: 0x00312218
// 	// (set) Token: 0x0600792C RID: 31020 RVA: 0x00314020 File Offset: 0x00312220
// 	public virtual int RepeatCount { get; set; }

// 	// Token: 0x17000C37 RID: 3127
// 	// (get) Token: 0x0600792D RID: 31021 RVA: 0x00314029 File Offset: 0x00312229
// 	public string ID
// 	{
// 		get
// 		{
// 			return this.id;
// 		}
// 	}

// 	// Token: 0x17000C38 RID: 3128
// 	// (get) Token: 0x0600792E RID: 31022 RVA: 0x00314031 File Offset: 0x00312231
// 	// (set) Token: 0x0600792F RID: 31023 RVA: 0x0031405D File Offset: 0x0031225D
// 	public bool IsNavigatable
// 	{
// 		get
// 		{
// 			return this._isNavigatable && this.Enabled && this.IsVisible && this.UiTransform.gameObject.activeInHierarchy;
// 		}
// 		set
// 		{
// 			this._isNavigatable = value;
// 		}
// 	}

// 	// Token: 0x17000C39 RID: 3129
// 	// (get) Token: 0x06007930 RID: 31024 RVA: 0x00314066 File Offset: 0x00312266
// 	// (set) Token: 0x06007931 RID: 31025 RVA: 0x00314070 File Offset: 0x00312270
// 	public XUiView NavUpTarget
// 	{
// 		get
// 		{
// 			return this.navUpTarget;
// 		}
// 		set
// 		{
// 			this.navUpTarget = value;
// 			foreach (XUiController xuiController in this.Controller.Children)
// 			{
// 				xuiController.ViewComponent.NavUpTarget = value;
// 			}
// 		}
// 	}

// 	// Token: 0x17000C3A RID: 3130
// 	// (get) Token: 0x06007932 RID: 31026 RVA: 0x003140D4 File Offset: 0x003122D4
// 	// (set) Token: 0x06007933 RID: 31027 RVA: 0x003140DC File Offset: 0x003122DC
// 	public XUiView NavDownTarget
// 	{
// 		get
// 		{
// 			return this.navDownTarget;
// 		}
// 		set
// 		{
// 			this.navDownTarget = value;
// 			foreach (XUiController xuiController in this.Controller.Children)
// 			{
// 				xuiController.ViewComponent.navDownTarget = value;
// 			}
// 		}
// 	}

// 	// Token: 0x17000C3B RID: 3131
// 	// (get) Token: 0x06007934 RID: 31028 RVA: 0x00314140 File Offset: 0x00312340
// 	// (set) Token: 0x06007935 RID: 31029 RVA: 0x00314148 File Offset: 0x00312348
// 	public XUiView NavLeftTarget
// 	{
// 		get
// 		{
// 			return this.navLeftTarget;
// 		}
// 		set
// 		{
// 			this.navLeftTarget = value;
// 			foreach (XUiController xuiController in this.Controller.Children)
// 			{
// 				xuiController.ViewComponent.navLeftTarget = value;
// 			}
// 		}
// 	}

// 	// Token: 0x17000C3C RID: 3132
// 	// (get) Token: 0x06007936 RID: 31030 RVA: 0x003141AC File Offset: 0x003123AC
// 	// (set) Token: 0x06007937 RID: 31031 RVA: 0x003141B4 File Offset: 0x003123B4
// 	public XUiView NavRightTarget
// 	{
// 		get
// 		{
// 			return this.navRightTarget;
// 		}
// 		set
// 		{
// 			this.navRightTarget = value;
// 			foreach (XUiController xuiController in this.Controller.Children)
// 			{
// 				xuiController.ViewComponent.navRightTarget = value;
// 			}
// 		}
// 	}

// 	// Token: 0x17000C3D RID: 3133
// 	// (get) Token: 0x06007938 RID: 31032 RVA: 0x00314218 File Offset: 0x00312418
// 	// (set) Token: 0x06007939 RID: 31033 RVA: 0x00314220 File Offset: 0x00312420
// 	public bool IsDirty
// 	{
// 		get
// 		{
// 			return this.isDirty;
// 		}
// 		set
// 		{
// 			this.isDirty = value;
// 		}
// 	}

// 	// Token: 0x17000C3E RID: 3134
// 	// (get) Token: 0x0600793A RID: 31034 RVA: 0x00314229 File Offset: 0x00312429
// 	// (set) Token: 0x0600793B RID: 31035 RVA: 0x00314231 File Offset: 0x00312431
// 	public XUiController Controller
// 	{
// 		get
// 		{
// 			return this.controller;
// 		}
// 		set
// 		{
// 			this.controller = value;
// 			if (this.controller.ViewComponent != this)
// 			{
// 				this.controller.ViewComponent = this;
// 			}
// 		}
// 	}

// 	// Token: 0x17000C3F RID: 3135
// 	// (get) Token: 0x0600793C RID: 31036 RVA: 0x00314254 File Offset: 0x00312454
// 	public Transform UiTransform
// 	{
// 		get
// 		{
// 			return this.uiTransform;
// 		}
// 	}

// 	// Token: 0x17000C40 RID: 3136
// 	// (get) Token: 0x0600793D RID: 31037 RVA: 0x0031425C File Offset: 0x0031245C
// 	// (set) Token: 0x0600793E RID: 31038 RVA: 0x00314264 File Offset: 0x00312464
// 	public Vector2i Size
// 	{
// 		get
// 		{
// 			return this.size;
// 		}
// 		set
// 		{
// 			if (this.size != value)
// 			{
// 				this.size = value;
// 				this.isDirty = true;
// 			}
// 		}
// 	}

// 	// Token: 0x17000C41 RID: 3137
// 	// (get) Token: 0x0600793F RID: 31039 RVA: 0x00314282 File Offset: 0x00312482
// 	public Vector2i InnerSize
// 	{
// 		get
// 		{
// 			return new Vector2i(this.size.x - this.padding.SumLeftRight, this.size.y - this.padding.SumTopBottom);
// 		}
// 	}

// 	// Token: 0x17000C42 RID: 3138
// 	// (get) Token: 0x06007940 RID: 31040 RVA: 0x003142B7 File Offset: 0x003124B7
// 	// (set) Token: 0x06007941 RID: 31041 RVA: 0x003142BF File Offset: 0x003124BF
// 	public bool IgnoreParentPadding
// 	{
// 		get
// 		{
// 			return this.ignoreParentPadding;
// 		}
// 		set
// 		{
// 			if (this.ignoreParentPadding != value)
// 			{
// 				this.ignoreParentPadding = value;
// 				this.isDirty = true;
// 			}
// 		}
// 	}

// 	// Token: 0x17000C43 RID: 3139
// 	// (get) Token: 0x06007942 RID: 31042 RVA: 0x003142D8 File Offset: 0x003124D8
// 	// (set) Token: 0x06007943 RID: 31043 RVA: 0x003142E0 File Offset: 0x003124E0
// 	public Vector2i Position
// 	{
// 		get
// 		{
// 			return this.position;
// 		}
// 		set
// 		{
// 			this.position = value;
// 			this.isDirty = true;
// 			this.positionDirty = true;
// 		}
// 	}

// 	// Token: 0x17000C44 RID: 3140
// 	// (get) Token: 0x06007944 RID: 31044 RVA: 0x003142F8 File Offset: 0x003124F8
// 	public Vector2i PaddedPosition
// 	{
// 		get
// 		{
// 			Vector2i one = this.position;
// 			Vector2i other;
// 			if (!this.ignoreParentPadding)
// 			{
// 				XUiController parent = this.Controller.Parent;
// 				Vector2i? vector2i;
// 				if (parent == null)
// 				{
// 					vector2i = null;
// 				}
// 				else
// 				{
// 					XUiView viewComponent = parent.ViewComponent;
// 					vector2i = ((viewComponent != null) ? new Vector2i?(viewComponent.InnerPosition) : null);
// 				}
// 				other = (vector2i ?? Vector2i.zero);
// 			}
// 			else
// 			{
// 				other = Vector2i.zero;
// 			}
// 			return one + other;
// 		}
// 	}

// 	// Token: 0x17000C45 RID: 3141
// 	// (get) Token: 0x06007945 RID: 31045 RVA: 0x0031436F File Offset: 0x0031256F
// 	public Vector2i InnerPosition
// 	{
// 		get
// 		{
// 			return new Vector2i(this.padding.Left, -this.padding.Top);
// 		}
// 	}

// 	// Token: 0x17000C46 RID: 3142
// 	// (get) Token: 0x06007946 RID: 31046 RVA: 0x0031438D File Offset: 0x0031258D
// 	// (set) Token: 0x06007947 RID: 31047 RVA: 0x00314395 File Offset: 0x00312595
// 	public float Rotation
// 	{
// 		get
// 		{
// 			return this.rotation;
// 		}
// 		set
// 		{
// 			this.rotation = value;
// 			this.isDirty = true;
// 			this.rotationDirty = true;
// 		}
// 	}

// 	// Token: 0x17000C47 RID: 3143
// 	// (get) Token: 0x06007948 RID: 31048 RVA: 0x003143AC File Offset: 0x003125AC
// 	// (set) Token: 0x06007949 RID: 31049 RVA: 0x003143B4 File Offset: 0x003125B4
// 	public UIWidget.Pivot Pivot
// 	{
// 		get
// 		{
// 			return this.pivot;
// 		}
// 		set
// 		{
// 			this.pivot = value;
// 			this.isDirty = true;
// 		}
// 	}

// 	// Token: 0x17000C48 RID: 3144
// 	// (get) Token: 0x0600794A RID: 31050 RVA: 0x003143C4 File Offset: 0x003125C4
// 	// (set) Token: 0x0600794B RID: 31051 RVA: 0x003143CC File Offset: 0x003125CC
// 	public int Depth
// 	{
// 		get
// 		{
// 			return this.depth;
// 		}
// 		set
// 		{
// 			this.depth = value;
// 			this.isDirty = true;
// 		}
// 	}

// 	// Token: 0x17000C49 RID: 3145
// 	// (get) Token: 0x0600794C RID: 31052 RVA: 0x003143DC File Offset: 0x003125DC
// 	// (set) Token: 0x0600794D RID: 31053 RVA: 0x003143F4 File Offset: 0x003125F4
// 	public virtual bool IsVisible
// 	{
// 		get
// 		{
// 			return this.isVisible && !this.ForceHide;
// 		}
// 		set
// 		{
// 			if (this.isVisible != value)
// 			{
// 				if (this.ForceHide && value)
// 				{
// 					return;
// 				}
// 				this.isVisible = value;
// 				if (this.uiTransform != null && this.isVisible != this.uiTransform.gameObject.activeSelf)
// 				{
// 					this.uiTransform.gameObject.SetActive(this.isVisible);
// 				}
// 				this.Controller.IsDirty = true;
// 				this.Controller.OnVisibilityChanged(this.isVisible);
// 				if (this.xui.playerUI.CursorController.navigationTarget == this)
// 				{
// 					this.xui.playerUI.RefreshNavigationTarget();
// 				}
// 				this.isDirty = true;
// 			}
// 		}
// 	}

// 	// Token: 0x17000C4A RID: 3146
// 	// (get) Token: 0x0600794E RID: 31054 RVA: 0x003144A7 File Offset: 0x003126A7
// 	// (set) Token: 0x0600794F RID: 31055 RVA: 0x003144AF File Offset: 0x003126AF
// 	public bool ForceHide
// 	{
// 		get
// 		{
// 			return this.forceHide;
// 		}
// 		set
// 		{
// 			this.forceHide = value;
// 		}
// 	}

// 	// Token: 0x17000C4B RID: 3147
// 	// (get) Token: 0x06007950 RID: 31056 RVA: 0x003144B8 File Offset: 0x003126B8
// 	// (set) Token: 0x06007951 RID: 31057 RVA: 0x003144C0 File Offset: 0x003126C0
// 	public virtual bool Enabled
// 	{
// 		get
// 		{
// 			return this.enabled;
// 		}
// 		set
// 		{
// 			if (value != this.enabled)
// 			{
// 				this.enabled = value;
// 				if (!this.enabled && this.xui.playerUI.CursorController.navigationTarget == this)
// 				{
// 					this.xui.playerUI.RefreshNavigationTarget();
// 				}
// 				this.isDirty = true;
// 			}
// 		}
// 	}

// 	// Token: 0x17000C4C RID: 3148
// 	// (get) Token: 0x06007952 RID: 31058 RVA: 0x00314514 File Offset: 0x00312714
// 	// (set) Token: 0x06007953 RID: 31059 RVA: 0x0031451C File Offset: 0x0031271C
// 	public string Value
// 	{
// 		get
// 		{
// 			return this.m_value;
// 		}
// 		set
// 		{
// 			this.m_value = value;
// 		}
// 	}

// 	// Token: 0x17000C4D RID: 3149
// 	// (get) Token: 0x06007954 RID: 31060 RVA: 0x00314525 File Offset: 0x00312725
// 	// (set) Token: 0x06007955 RID: 31061 RVA: 0x00314530 File Offset: 0x00312730
// 	public string ToolTip
// 	{
// 		get
// 		{
// 			return this.toolTip;
// 		}
// 		set
// 		{
// 			if (this.toolTip != value)
// 			{
// 				if (GameManager.Instance.GameIsFocused && this.enabled && this.isOver && this.xui.currentToolTip != null && this.xui.currentToolTip.ToolTip == this.toolTip)
// 				{
// 					this.xui.currentToolTip.ToolTip = value;
// 				}
// 				this.toolTip = value;
// 			}
// 		}
// 	}

// 	// Token: 0x17000C4E RID: 3150
// 	// (get) Token: 0x06007956 RID: 31062 RVA: 0x003145A9 File Offset: 0x003127A9
// 	// (set) Token: 0x06007957 RID: 31063 RVA: 0x003145B4 File Offset: 0x003127B4
// 	public string DisabledToolTip
// 	{
// 		get
// 		{
// 			return this.disabledToolTip;
// 		}
// 		set
// 		{
// 			if (this.disabledToolTip != value)
// 			{
// 				if (GameManager.Instance.GameIsFocused && this.enabled && this.isOver && this.xui.currentToolTip != null && this.xui.currentToolTip.ToolTip == this.disabledToolTip)
// 				{
// 					this.xui.currentToolTip.ToolTip = value;
// 				}
// 				this.disabledToolTip = value;
// 			}
// 		}
// 	}

// 	// Token: 0x17000C4F RID: 3151
// 	// (get) Token: 0x06007958 RID: 31064 RVA: 0x00314630 File Offset: 0x00312830
// 	public Vector2 Center
// 	{
// 		get
// 		{
// 			return this.collider.bounds.center;
// 		}
// 	}

// 	// Token: 0x17000C50 RID: 3152
// 	// (get) Token: 0x06007959 RID: 31065 RVA: 0x00314658 File Offset: 0x00312858
// 	public float heightExtent
// 	{
// 		get
// 		{
// 			return this.collider.bounds.size.y / 2f;
// 		}
// 	}

// 	// Token: 0x17000C51 RID: 3153
// 	// (get) Token: 0x0600795A RID: 31066 RVA: 0x00314684 File Offset: 0x00312884
// 	public float widthExtent
// 	{
// 		get
// 		{
// 			return this.collider.bounds.size.x / 2f;
// 		}
// 	}

// 	// Token: 0x17000C52 RID: 3154
// 	// (get) Token: 0x0600795B RID: 31067 RVA: 0x003146AF File Offset: 0x003128AF
// 	public Bounds bounds
// 	{
// 		get
// 		{
// 			return this.collider.bounds;
// 		}
// 	}

// 	// Token: 0x17000C53 RID: 3155
// 	// (get) Token: 0x0600795C RID: 31068 RVA: 0x003146BC File Offset: 0x003128BC
// 	public Vector2 ScreenPosition
// 	{
// 		get
// 		{
// 			return this.xui.playerUI.uiCamera.cachedCamera.WorldToScreenPoint(this.uiTransform.position);
// 		}
// 	}

// 	// Token: 0x17000C54 RID: 3156
// 	// (get) Token: 0x0600795D RID: 31069 RVA: 0x003146E8 File Offset: 0x003128E8
// 	public bool HasCollider
// 	{
// 		get
// 		{
// 			return this.collider != null;
// 		}
// 	}

// 	// Token: 0x17000C55 RID: 3157
// 	// (get) Token: 0x0600795E RID: 31070 RVA: 0x003146F6 File Offset: 0x003128F6
// 	public bool ColliderEnabled
// 	{
// 		get
// 		{
// 			return this.HasCollider && this.collider.enabled;
// 		}
// 	}

// 	// Token: 0x17000C56 RID: 3158
// 	// (get) Token: 0x0600795F RID: 31071 RVA: 0x0031470D File Offset: 0x0031290D
// 	// (set) Token: 0x06007960 RID: 31072 RVA: 0x00314715 File Offset: 0x00312915
// 	public bool SoundPlayOnClick
// 	{
// 		get
// 		{
// 			return this.soundPlayOnClick;
// 		}
// 		set
// 		{
// 			this.soundPlayOnClick = value;
// 		}
// 	}

// 	// Token: 0x17000C57 RID: 3159
// 	// (get) Token: 0x06007961 RID: 31073 RVA: 0x0031471E File Offset: 0x0031291E
// 	// (set) Token: 0x06007962 RID: 31074 RVA: 0x00314726 File Offset: 0x00312926
// 	public bool SoundPlayOnHover
// 	{
// 		get
// 		{
// 			return this.soundPlayOnHover;
// 		}
// 		set
// 		{
// 			this.soundPlayOnHover = value;
// 		}
// 	}

// 	// Token: 0x17000C58 RID: 3160
// 	// (get) Token: 0x06007963 RID: 31075 RVA: 0x0031472F File Offset: 0x0031292F
// 	public bool HasHoverSound
// 	{
// 		get
// 		{
// 			return this.xuiHoverSound != null;
// 		}
// 	}

// 	// Token: 0x17000C59 RID: 3161
// 	// (get) Token: 0x06007964 RID: 31076 RVA: 0x00314740 File Offset: 0x00312940
// 	public bool HasEvent
// 	{
// 		get
// 		{
// 			return this.EventOnPress || this.EventOnDoubleClick || this.EventOnHover || this.EventOnHeld || this.EventOnDrag || this.EventOnScroll || this.EventOnSelect || !string.IsNullOrEmpty(this.ToolTip);
// 		}
// 	}

// 	// Token: 0x17000C5A RID: 3162
// 	// (get) Token: 0x06007965 RID: 31077 RVA: 0x00314795 File Offset: 0x00312995
// 	// (set) Token: 0x06007966 RID: 31078 RVA: 0x0031479D File Offset: 0x0031299D
// 	public XUi xui
// 	{
// 		get
// 		{
// 			return this.mXUi;
// 		}
// 		set
// 		{
// 			this.mXUi = value;
// 			if (this.viewIndex < 0)
// 			{
// 				this.viewIndex = this.mXUi.RegisterXUiView(this);
// 			}
// 		}
// 	}

// 	// Token: 0x06007967 RID: 31079 RVA: 0x003147C4 File Offset: 0x003129C4
// 	public XUiView(string _id)
// 	{
// 		this.id = _id;
// 	}

// 	// Token: 0x06007968 RID: 31080 RVA: 0x00314854 File Offset: 0x00312A54
// 	public virtual void UpdateData()
// 	{
// 		if (this.positionDirty)
// 		{
// 			this.uiTransform.localPosition = new Vector3((float)this.PaddedPosition.x, (float)this.PaddedPosition.y, this.uiTransform.localPosition.z);
// 			this.positionDirty = false;
// 		}
// 		if (this.rotationDirty)
// 		{
// 			this.uiTransform.localEulerAngles = new Vector3(0f, 0f, this.rotation);
// 			this.rotationDirty = false;
// 		}
// 		this.parseNavigationTargets();
// 	}

// 	// Token: 0x06007969 RID: 31081 RVA: 0x003148E0 File Offset: 0x00312AE0
// 	public void TryUpdatePosition()
// 	{
// 		if (this.positionDirty && this.uiTransform != null)
// 		{
// 			this.uiTransform.localPosition = new Vector3((float)this.PaddedPosition.x, (float)this.PaddedPosition.y, this.uiTransform.localPosition.z);
// 		}
// 	}

// 	// Token: 0x0600796A RID: 31082 RVA: 0x0031493B File Offset: 0x00312B3B
// 	public virtual void OnOpen()
// 	{
// 		if (this.xuiSound != null && this.soundPlayOnOpen)
// 		{
// 			Manager.PlayXUiSound(this.xuiSound, this.soundVolume);
// 		}
// 		this.isPressed = false;
// 		this.isHold = false;
// 	}

// 	// Token: 0x0600796B RID: 31083 RVA: 0x00314974 File Offset: 0x00312B74
// 	public virtual void OnClose()
// 	{
// 		this.isPressed = false;
// 		this.isHold = false;
// 		if (!GameManager.Instance.IsQuitting)
// 		{
// 			if (this.xui.playerUI.CursorController.navigationTarget == this)
// 			{
// 				this.controller.Hovered(false);
// 				this.xui.playerUI.CursorController.SetNavigationTarget(null);
// 			}
// 			if (this.xui.playerUI.CursorController.lockNavigationToView == this)
// 			{
// 				this.xui.playerUI.CursorController.SetNavigationLockView(null, null);
// 			}
// 		}
// 	}

// 	// Token: 0x0600796C RID: 31084 RVA: 0x00002914 File Offset: 0x00000B14
// 	public virtual void Cleanup()
// 	{
// 	}

// 	// Token: 0x0600796D RID: 31085 RVA: 0x00002914 File Offset: 0x00000B14
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public virtual void CreateComponents(GameObject _go)
// 	{
// 	}

// 	// Token: 0x0600796E RID: 31086 RVA: 0x00314A04 File Offset: 0x00312C04
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void BuildView()
// 	{
// 		Type type = base.GetType();
// 		GameObject original;
// 		if (XUiView.componentTemplates.TryGetValue(type, out original))
// 		{
// 			this.uiTransform = UnityEngine.Object.Instantiate<GameObject>(original).transform;
// 			return;
// 		}
// 		if (XUiView.templatesParent == null)
// 		{
// 			Transform transform = this.xui.playerUI.uiCamera.transform;
// 			XUiView.templatesParent = new GameObject("_ViewTemplates").transform;
// 			XUiView.templatesParent.parent = transform;
// 		}
// 		GameObject gameObject = new GameObject(type.Name);
// 		gameObject.layer = 12;
// 		gameObject.transform.parent = XUiView.templatesParent;
// 		gameObject.AddComponent<BoxCollider>().enabled = false;
// 		gameObject.AddComponent<UIAnchor>().enabled = false;
// 		UIEventListener.Get(gameObject);
// 		this.CreateComponents(gameObject);
// 		XUiView.componentTemplates[type] = gameObject;
// 		this.uiTransform = UnityEngine.Object.Instantiate<GameObject>(gameObject).transform;
// 	}

// 	// Token: 0x0600796F RID: 31087 RVA: 0x00314AE4 File Offset: 0x00312CE4
// 	public virtual void InitView()
// 	{
// 		if (this.uiTransform == null)
// 		{
// 			this.BuildView();
// 		}
// 		this.uiTransform.name = this.id;
// 		this.collider = this.uiTransform.gameObject.GetComponent<BoxCollider>();
// 		this.anchor = this.uiTransform.gameObject.GetComponent<UIAnchor>();
// 		XUiController parent = this.controller.Parent;
// 		if (((parent != null) ? parent.ViewComponent : null) != null)
// 		{
// 			XUiView viewComponent = this.controller.Parent.ViewComponent;
// 			this.uiTransform.parent = viewComponent.UiTransform;
// 			this.uiTransform.localScale = Vector3.one;
// 			this.uiTransform.localPosition = new Vector3((float)this.PaddedPosition.x, (float)this.PaddedPosition.y, 0f);
// 			this.uiTransform.localEulerAngles = new Vector3(0f, 0f, this.rotation);
// 		}
// 		else
// 		{
// 			this.setRootNode();
// 		}
// 		if (this.HasEvent)
// 		{
// 			this.collider.enabled = true;
// 			this.RefreshBoxCollider();
// 		}
// 		if (this.isAnchored)
// 		{
// 			this.anchor.enabled = true;
// 			if (string.IsNullOrEmpty(this.anchorContainerName))
// 			{
// 				this.anchor.container = this.uiTransform.parent.gameObject;
// 			}
// 			else if (!this.anchorContainerName.EqualsCaseInsensitive("#none"))
// 			{
// 				this.anchor.container = this.Controller.Parent.GetChildById(this.anchorContainerName).ViewComponent.uiTransform.gameObject;
// 			}
// 			this.anchor.side = this.anchorSide;
// 			this.anchor.runOnlyOnce = this.anchorRunOnce;
// 			this.anchor.pixelOffset = this.anchorOffset;
// 		}
// 		if (this.HasEvent)
// 		{
// 			UIEventListener uieventListener = UIEventListener.Get(this.uiTransform.gameObject);
// 			if (this.EventOnPress)
// 			{
// 				UIEventListener uieventListener2 = uieventListener;
// 				uieventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uieventListener2.onClick, new UIEventListener.VoidDelegate(this.OnClick));
// 			}
// 			if (this.EventOnDoubleClick)
// 			{
// 				UIEventListener uieventListener3 = uieventListener;
// 				uieventListener3.onDoubleClick = (UIEventListener.VoidDelegate)Delegate.Combine(uieventListener3.onDoubleClick, new UIEventListener.VoidDelegate(this.OnDoubleClick));
// 			}
// 			if (this.EventOnHover || !string.IsNullOrEmpty(this.ToolTip))
// 			{
// 				UIEventListener uieventListener4 = uieventListener;
// 				uieventListener4.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uieventListener4.onHover, new UIEventListener.BoolDelegate(this.OnHover));
// 			}
// 			if (this.EventOnDrag)
// 			{
// 				UIEventListener uieventListener5 = uieventListener;
// 				uieventListener5.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uieventListener5.onDrag, new UIEventListener.VectorDelegate(this.OnDrag));
// 				UIEventListener uieventListener6 = uieventListener;
// 				uieventListener6.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uieventListener6.onPress, new UIEventListener.BoolDelegate(this.OnPress));
// 			}
// 			if (this.EventOnScroll)
// 			{
// 				UIEventListener uieventListener7 = uieventListener;
// 				uieventListener7.onScroll = (UIEventListener.FloatDelegate)Delegate.Combine(uieventListener7.onScroll, new UIEventListener.FloatDelegate(this.OnScroll));
// 			}
// 			if (this.EventOnSelect)
// 			{
// 				UIEventListener uieventListener8 = uieventListener;
// 				uieventListener8.onSelect = (UIEventListener.BoolDelegate)Delegate.Combine(uieventListener8.onSelect, new UIEventListener.BoolDelegate(this.OnSelect));
// 			}
// 			if (this.EventOnHeld)
// 			{
// 				UIEventListener uieventListener9 = uieventListener;
// 				uieventListener9.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uieventListener9.onPress, new UIEventListener.BoolDelegate(this.OnHeldPress));
// 				UIEventListener uieventListener10 = uieventListener;
// 				uieventListener10.onDragOut = (UIEventListener.VoidDelegate)Delegate.Combine(uieventListener10.onDragOut, new UIEventListener.VoidDelegate(this.OnHeldDragOut));
// 			}
// 		}
// 		if (this.uiTransform.gameObject.activeSelf != this.isVisible)
// 		{
// 			this.uiTransform.gameObject.SetActive(this.isVisible);
// 		}
// 		if (!this.gamepadSelectableSetFromAttributes)
// 		{
// 			this.IsNavigatable = (this.IsSnappable = this.EventOnPress);
// 		}
// 	}

// 	// Token: 0x06007970 RID: 31088 RVA: 0x00314E94 File Offset: 0x00313094
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void OnScroll(GameObject _go, float _delta)
// 	{
// 		if (this.EventOnScroll)
// 		{
// 			this.controller.Scrolled(_delta);
// 		}
// 	}

// 	// Token: 0x06007971 RID: 31089 RVA: 0x00314EAA File Offset: 0x003130AA
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void OnSelect(GameObject _go, bool _selected)
// 	{
// 		if (this.EventOnSelect && this.enabled)
// 		{
// 			this.controller.Selected(_selected);
// 		}
// 	}

// 	// Token: 0x06007972 RID: 31090 RVA: 0x00314EC8 File Offset: 0x003130C8
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void OnDrag(GameObject _go, Vector2 _delta)
// 	{
// 		if (this.EventOnDrag && this.enabled)
// 		{
// 			EDragType dragType = this.wasDragging ? EDragType.Dragging : EDragType.DragStart;
// 			this.wasDragging = true;
// 			this.controller.Dragged(_delta, dragType);
// 		}
// 	}

// 	// Token: 0x06007973 RID: 31091 RVA: 0x00314F08 File Offset: 0x00313108
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void OnPress(GameObject _go, bool _pressed)
// 	{
// 		if (this.EventOnDrag && this.enabled && !_pressed && this.wasDragging)
// 		{
// 			this.wasDragging = false;
// 			this.controller.Dragged(default(Vector2), EDragType.DragEnd);
// 		}
// 	}

// 	// Token: 0x06007974 RID: 31092 RVA: 0x00314F4C File Offset: 0x0031314C
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void OnHeldPress(GameObject _go, bool _pressed)
// 	{
// 		if (this.EventOnHeld)
// 		{
// 			if (_pressed && !this.isPressed && this.enabled)
// 			{
// 				this.isPressed = true;
// 				this.isHold = false;
// 				this.pressStartTime = Time.unscaledTime;
// 				this.holdStartTime = -1f;
// 				return;
// 			}
// 			if (!_pressed)
// 			{
// 				this.isPressed = false;
// 				bool flag = this.isHold;
// 				this.isHold = false;
// 				if (flag)
// 				{
// 					this.controller.Held(EHoldType.HoldEnd, Time.unscaledTime - this.holdStartTime, -1f);
// 				}
// 			}
// 		}
// 	}

// 	// Token: 0x06007975 RID: 31093 RVA: 0x00314FD0 File Offset: 0x003131D0
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void OnHeldDragOut(GameObject _go)
// 	{
// 		if (this.EventOnHeld && this.isPressed)
// 		{
// 			this.isPressed = false;
// 			bool flag = this.isHold;
// 			this.isHold = false;
// 			if (flag)
// 			{
// 				this.controller.Held(EHoldType.HoldEnd, Time.unscaledTime - this.holdStartTime, -1f);
// 			}
// 		}
// 	}

// 	// Token: 0x06007976 RID: 31094 RVA: 0x00315020 File Offset: 0x00313220
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void OnClick(GameObject _go)
// 	{
// 		if (this.EventOnPress && this.enabled)
// 		{
// 			if (this.xuiSound != null && this.soundPlayOnClick && UICamera.currentTouchID == -1)
// 			{
// 				Manager.PlayXUiSound(this.xuiSound, this.soundVolume);
// 			}
// 			this.controller.Pressed(UICamera.currentTouchID);
// 		}
// 	}

// 	// Token: 0x06007977 RID: 31095 RVA: 0x0031507C File Offset: 0x0031327C
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void OnDoubleClick(GameObject _go)
// 	{
// 		if (this.EventOnDoubleClick && this.enabled)
// 		{
// 			if (this.xuiSound != null && this.soundPlayOnClick && UICamera.currentTouchID == -1)
// 			{
// 				Manager.PlayXUiSound(this.xuiSound, this.soundVolume);
// 			}
// 			this.controller.DoubleClicked(UICamera.currentTouchID);
// 		}
// 	}

// 	// Token: 0x06007978 RID: 31096 RVA: 0x003150D8 File Offset: 0x003132D8
// 	public virtual void OnHover(GameObject _go, bool _isOver)
// 	{
// 		if (this.xui.playerUI.playerInput.LastDeviceClass == InputDeviceClass.Keyboard && !Cursor.visible)
// 		{
// 			_isOver = false;
// 		}
// 		bool flag = _isOver && !this.enabled && !string.IsNullOrEmpty(this.DisabledToolTip);
// 		_isOver &= this.enabled;
// 		bool flag2 = _isOver && !string.IsNullOrEmpty(this.ToolTip);
// 		if (this.controllerOnlyTooltip && PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
// 		{
// 			flag = false;
// 			flag2 = false;
// 		}
// 		if (_isOver != this.isOver && _isOver)
// 		{
// 			this.PlayHoverSound();
// 		}
// 		this.isOver = _isOver;
// 		if (this.EventOnHover)
// 		{
// 			this.controller.Hovered(_isOver);
// 		}
// 		if (this.xui.currentToolTip != null)
// 		{
// 			if (flag2)
// 			{
// 				this.xui.currentToolTip.ToolTip = this.ToolTip;
// 			}
// 			else if (flag)
// 			{
// 				this.xui.currentToolTip.ToolTip = this.DisabledToolTip;
// 			}
// 			else
// 			{
// 				this.xui.currentToolTip.ToolTip = "";
// 			}
// 		}
// 		this.xui.playerUI.CursorController.HoverTarget = (_isOver ? this : null);
// 	}

// 	// Token: 0x06007979 RID: 31097 RVA: 0x00315206 File Offset: 0x00313406
// 	public void PlayHoverSound()
// 	{
// 		if (this.xuiHoverSound != null && this.soundPlayOnHover && this.enabled && GameManager.Instance.GameIsFocused)
// 		{
// 			Manager.PlayXUiSound(this.xuiHoverSound, this.soundVolume);
// 		}
// 	}

// 	// Token: 0x0600797A RID: 31098 RVA: 0x00315243 File Offset: 0x00313443
// 	public void PlayClickSound()
// 	{
// 		if (this.xuiSound != null)
// 		{
// 			Manager.PlayXUiSound(this.xuiSound, this.soundVolume);
// 		}
// 	}

// 	// Token: 0x0600797B RID: 31099 RVA: 0x00315264 File Offset: 0x00313464
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public virtual void setRootNode()
// 	{
// 		if (this.rootNode == null)
// 		{
// 			this.rootNode = this.xui.transform.Find("CenterTop").transform;
// 		}
// 		this.uiTransform.parent = this.rootNode;
// 		this.uiTransform.gameObject.layer = 12;
// 		this.uiTransform.localScale = Vector3.one;
// 		this.uiTransform.localPosition = new Vector3((float)this.PaddedPosition.x, (float)this.PaddedPosition.y, 0f);
// 		this.uiTransform.localEulerAngles = new Vector3(0f, 0f, this.rotation);
// 	}

// 	// Token: 0x0600797C RID: 31100 RVA: 0x00315320 File Offset: 0x00313520
// 	public virtual void RefreshBoxCollider()
// 	{
// 		if (this.collider != null)
// 		{
// 			float num = (float)this.size.x * 0.5f;
// 			float num2 = (float)this.size.y * 0.5f;
// 			float x;
// 			float y;
// 			switch (this.pivot)
// 			{
// 			case UIWidget.Pivot.TopLeft:
// 				x = num;
// 				y = 0f - num2;
// 				break;
// 			case UIWidget.Pivot.Top:
// 				x = 0f;
// 				y = 0f - num2;
// 				break;
// 			case UIWidget.Pivot.TopRight:
// 				x = 0f - num;
// 				y = 0f - num2;
// 				break;
// 			case UIWidget.Pivot.Left:
// 				x = num;
// 				y = 0f;
// 				break;
// 			case UIWidget.Pivot.Center:
// 				x = 0f;
// 				y = 0f;
// 				break;
// 			case UIWidget.Pivot.Right:
// 				x = 0f - num;
// 				y = 0f;
// 				break;
// 			case UIWidget.Pivot.BottomLeft:
// 				x = num;
// 				y = num2;
// 				break;
// 			case UIWidget.Pivot.Bottom:
// 				x = 0f;
// 				y = num2;
// 				break;
// 			case UIWidget.Pivot.BottomRight:
// 				x = 0f - num;
// 				y = num2;
// 				break;
// 			default:
// 				throw new ArgumentOutOfRangeException();
// 			}
// 			this.collider.center = new Vector3(x, y, 0f);
// 			this.collider.size = new Vector3((float)this.size.x * this.colliderScale, (float)this.size.y * this.colliderScale, 0f);
// 		}
// 	}

// 	// Token: 0x0600797D RID: 31101 RVA: 0x00315464 File Offset: 0x00313664
// 	public virtual void Update(float _dt)
// 	{
// 		if (this.isOver && UICamera.hoveredObject != this.UiTransform.gameObject)
// 		{
// 			this.OnHover(this.UiTransform.gameObject, false);
// 		}
// 		if (this.isPressed && this.enabled)
// 		{
// 			float unscaledTime = Time.unscaledTime;
// 			if (!this.isHold)
// 			{
// 				this.isHold = (unscaledTime - this.pressStartTime >= this.holdDelay);
// 				if (this.isHold)
// 				{
// 					this.holdStartTime = unscaledTime;
// 					this.controller.Held(EHoldType.HoldStart, 0f, -1f);
// 					this.holdEventNextTime = 0f;
// 					this.holdEventLastTime = unscaledTime;
// 					this.holdEventIntervalChangeSpeed = 0f;
// 					this.holdEventIntervalCurrent = this.holdEventIntervalInitial;
// 				}
// 			}
// 			else
// 			{
// 				this.controller.Held(EHoldType.Hold, unscaledTime - this.holdStartTime, -1f);
// 			}
// 			if (this.isHold && unscaledTime >= this.holdEventNextTime)
// 			{
// 				this.holdEventIntervalCurrent = Mathf.SmoothDamp(this.holdEventIntervalCurrent, this.holdEventIntervalFinal, ref this.holdEventIntervalChangeSpeed, this.holdEventIntervalAcceleration, float.PositiveInfinity, _dt);
// 				this.holdEventNextTime = unscaledTime + this.holdEventIntervalCurrent;
// 				this.controller.Held(EHoldType.HoldTimed, unscaledTime - this.holdStartTime, unscaledTime - this.holdEventLastTime);
// 				this.holdEventLastTime = unscaledTime;
// 			}
// 		}
// 		if (this.isDirty)
// 		{
// 			this.UpdateData();
// 			this.isDirty = false;
// 		}
// 	}

// 	// Token: 0x0600797E RID: 31102 RVA: 0x003155C8 File Offset: 0x003137C8
// 	public Vector2 GetClosestPoint(Vector3 point)
// 	{
// 		if (this.collider != null)
// 		{
// 			return this.collider.ClosestPointOnBounds(point);
// 		}
// 		Log.Warning("XUiView: Attempting to get closest point to a view without a box collider");
// 		return this.uiTransform.position;
// 	}

// 	// Token: 0x0600797F RID: 31103 RVA: 0x00315604 File Offset: 0x00313804
// 	public void ClearNavigationTargets()
// 	{
// 		this.NavUpTarget = (this.NavDownTarget = (this.NavLeftTarget = (this.NavRightTarget = null)));
// 	}

// 	// Token: 0x06007980 RID: 31104 RVA: 0x00315634 File Offset: 0x00313834
// 	public virtual void SetDefaults(XUiController _parent)
// 	{
// 		this.Pivot = UIWidget.Pivot.TopLeft;
// 		this.RepeatContent = false;
// 		this.RepeatCount = 1;
// 		this.Size = Vector2i.min;
// 		this.IsVisible = true;
// 		int? num;
// 		if (_parent == null)
// 		{
// 			num = null;
// 		}
// 		else
// 		{
// 			XUiView viewComponent = _parent.ViewComponent;
// 			num = ((viewComponent != null) ? new int?(viewComponent.Depth) : null);
// 		}
// 		int? num2 = num;
// 		this.Depth = num2.GetValueOrDefault();
// 		this.ToolTip = "";
// 		this.soundLoop = false;
// 		this.soundPlayOnClick = true;
// 		this.soundPlayOnOpen = false;
// 		this.soundPlayOnHover = true;
// 		this.soundVolume = 1f;
// 	}

// 	// Token: 0x06007981 RID: 31105 RVA: 0x003156D8 File Offset: 0x003138D8
// 	public virtual void SetPostParsingDefaults(XUiController _parent)
// 	{
// 		XUiView xuiView = (_parent != null) ? _parent.ViewComponent : null;
// 		Vector2i vector2i = this.Size;
// 		if (vector2i.x == -2147483648)
// 		{
// 			vector2i.x = (this.ignoreParentPadding ? ((xuiView != null) ? xuiView.Size.x : 0) : ((xuiView != null) ? xuiView.InnerSize.x : 0));
// 		}
// 		if (vector2i.y == -2147483648)
// 		{
// 			vector2i.y = (this.ignoreParentPadding ? ((xuiView != null) ? xuiView.Size.y : 0) : ((xuiView != null) ? xuiView.InnerSize.y : 0));
// 		}
// 		this.Size = vector2i;
// 	}

// 	// Token: 0x06007982 RID: 31106 RVA: 0x00315780 File Offset: 0x00313980
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public void parseNavigationTargets()
// 	{
// 		if (this.navUpSetFromXML)
// 		{
// 			this.NavUpTarget = this.<parseNavigationTargets>g__findView|218_0(this.navUpTargetString);
// 		}
// 		if (this.navDownSetFromXML)
// 		{
// 			this.NavDownTarget = this.<parseNavigationTargets>g__findView|218_0(this.navDownTargetString);
// 		}
// 		if (this.navLeftSetFromXML)
// 		{
// 			this.NavLeftTarget = this.<parseNavigationTargets>g__findView|218_0(this.navLeftTargetString);
// 		}
// 		if (this.navRightSetFromXML)
// 		{
// 			this.NavRightTarget = this.<parseNavigationTargets>g__findView|218_0(this.navRightTargetString);
// 		}
// 	}

// 	// Token: 0x06007983 RID: 31107 RVA: 0x003157F8 File Offset: 0x003139F8
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public void parseAnchors(UIWidget _target, bool _fixSize = true)
// 	{
// 		if (_target == null)
// 		{
// 			return;
// 		}
// 		if (this.parseAnchorString(this.anchorLeft, ref this.anchorLeftParsed, _target.leftAnchor, _target) | this.parseAnchorString(this.anchorRight, ref this.anchorRightParsed, _target.rightAnchor, _target) | this.parseAnchorString(this.anchorBottom, ref this.anchorBottomParsed, _target.bottomAnchor, _target) | this.parseAnchorString(this.anchorTop, ref this.anchorTopParsed, _target.topAnchor, _target))
// 		{
// 			this.isDirty = true;
// 			_target.ResetAnchors();
// 		}
// 		if (_fixSize)
// 		{
// 			if ((!_target.leftAnchor.target || !_target.rightAnchor.target) && _target.width != this.size.x)
// 			{
// 				_target.width = this.size.x;
// 			}
// 			if ((!_target.bottomAnchor.target || !_target.topAnchor.target) && _target.height != this.size.y)
// 			{
// 				_target.height = this.size.y;
// 			}
// 		}
// 		ThreadManager.StartCoroutine(XUiView.<parseAnchors>g__markAsChangedLater|219_0(_target));
// 	}

// 	// Token: 0x06007984 RID: 31108 RVA: 0x00315928 File Offset: 0x00313B28
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public bool parseAnchorString(string _anchorString, ref string _parsedString, UIRect.AnchorPoint _anchor, UIWidget _target)
// 	{
// 		if (string.IsNullOrEmpty(_anchorString))
// 		{
// 			return false;
// 		}
// 		if (_anchorString == _parsedString && _anchor.target != null)
// 		{
// 			return false;
// 		}
// 		_parsedString = _anchorString;
// 		int num = _anchorString.IndexOf(',');
// 		if (num < 0)
// 		{
// 			throw new ArgumentException("Invalid anchor string '" + _anchorString + "', expected '<target>,<relative>,<absolute>'");
// 		}
// 		string text = _anchorString.Substring(0, num);
// 		int num2 = _anchorString.IndexOf(',', num + 1);
// 		if (num2 < 0)
// 		{
// 			throw new ArgumentException("Invalid anchor string '" + _anchorString + "', expected '<target>,<relative>,<absolute>'");
// 		}
// 		float relative = StringParsers.ParseFloat(_anchorString, num + 1, num2 - 1, NumberStyles.Any);
// 		int absolute = StringParsers.ParseSInt32(_anchorString, num2 + 1, -1, NumberStyles.Integer);
// 		if (text.Length == 0)
// 		{
// 			throw new ArgumentException("Invalid anchor string '" + _anchorString + "', expected '<target>,<relative>,<absolute>'");
// 		}
// 		if (text.EqualsCaseInsensitive("#parent"))
// 		{
// 			_anchor.target = this.uiTransform.parent;
// 		}
// 		else if (text.EqualsCaseInsensitive("#cam"))
// 		{
// 			UICamera componentInParent = this.uiTransform.gameObject.GetComponentInParent<UICamera>();
// 			if (componentInParent == null)
// 			{
// 				throw new Exception("UICamera not found");
// 			}
// 			_anchor.target = componentInParent.transform;
// 		}
// 		else if (text[0] == '#')
// 		{
// 			string text2 = text.Substring(1);
// 			UIAnchor.Side side;
// 			if (!EnumUtils.TryParse<UIAnchor.Side>(text2, out side, true))
// 			{
// 				throw new ArgumentException("Invalid anchor side name '" + text2 + "', expected any of '\tBottomLeft,Left,TopLeft,Top,TopRight,Right,BottomRight,Bottom,Center'");
// 			}
// 			_anchor.target = this.xui.GetAnchor(side).transform;
// 		}
// 		else
// 		{
// 			XUiView xuiView = this.findHierarchyClosestView(text);
// 			if (xuiView == null)
// 			{
// 				throw new ArgumentException(string.Concat(new string[]
// 				{
// 					"Invalid anchor string '",
// 					_anchorString,
// 					"', view component with name '",
// 					text,
// 					"' not found.\nOn: ",
// 					this.controller.GetXuiHierarchy()
// 				}));
// 			}
// 			_anchor.target = xuiView.UiTransform;
// 			XUiV_Grid xuiV_Grid = xuiView as XUiV_Grid;
// 			if (xuiV_Grid != null)
// 			{
// 				xuiV_Grid.OnSizeChangedSimple += _target.UpdateAnchors;
// 			}
// 		}
// 		_anchor.relative = relative;
// 		_anchor.absolute = absolute;
// 		return true;
// 	}

// 	// Token: 0x06007985 RID: 31109 RVA: 0x00315B30 File Offset: 0x00313D30
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public XUiView findHierarchyClosestView(string _name)
// 	{
// 		XUiController xuiController = this.Controller.WindowGroup.Controller;
// 		XUiController parent = this.Controller.Parent;
// 		XUiController childById;
// 		for (;;)
// 		{
// 			childById = parent.GetChildById(_name);
// 			if (childById != null)
// 			{
// 				break;
// 			}
// 			parent = parent.Parent;
// 			if (parent == xuiController)
// 			{
// 				goto Block_2;
// 			}
// 		}
// 		return childById.ViewComponent;
// 		Block_2:
// 		return null;
// 	}

// 	// Token: 0x06007986 RID: 31110 RVA: 0x00315B78 File Offset: 0x00313D78
// 	public bool ParseAttributeViewAndController(string _attribute, string _value, XUiController _parent, bool _allowBindingCreation = true)
// 	{
// 		if (_value.Contains("{"))
// 		{
// 			if (_allowBindingCreation)
// 			{
// 				new BindingInfo(this, _attribute, _value);
// 				return true;
// 			}
// 			if (XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
// 			{
// 				Log.Warning(string.Concat(new string[]
// 				{
// 					"[XUi] Refreshed binding contained '{': ",
// 					_attribute,
// 					"='",
// 					_value,
// 					"' on ",
// 					this.id
// 				}));
// 			}
// 		}
// 		if (this.ParseAttribute(_attribute, _value, _parent))
// 		{
// 			return true;
// 		}
// 		if (this.Controller != null)
// 		{
// 			if (!this.Controller.ParseAttribute(_attribute, _value, _parent))
// 			{
// 				this.Controller.CustomAttributes[_attribute] = _value;
// 			}
// 			return true;
// 		}
// 		return false;
// 	}

// 	// Token: 0x06007987 RID: 31111 RVA: 0x00315C1C File Offset: 0x00313E1C
// 	public virtual bool ParseAttribute(string _attribute, string _value, XUiController _parent)
// 	{
// 		uint num = <PrivateImplementationDetails>.ComputeStringHash(_attribute);
// 		if (num <= 2427138910U)
// 		{
// 			if (num <= 1214160313U)
// 			{
// 				if (num <= 450319007U)
// 				{
// 					if (num <= 273325558U)
// 					{
// 						if (num != 49525662U)
// 						{
// 							if (num != 235771284U)
// 							{
// 								if (num != 273325558U)
// 								{
// 									return false;
// 								}
// 								if (!(_attribute == "anchor_run_once"))
// 								{
// 									return false;
// 								}
// 								this.anchorRunOnce = StringParsers.ParseBool(_value, 0, -1, true);
// 								return true;
// 							}
// 							else
// 							{
// 								if (!(_attribute == "sound"))
// 								{
// 									return false;
// 								}
// 								this.xui.LoadData<AudioClip>(_value, delegate(AudioClip _o)
// 								{
// 									this.xuiSound = _o;
// 								});
// 								return true;
// 							}
// 						}
// 						else
// 						{
// 							if (!(_attribute == "enabled"))
// 							{
// 								return false;
// 							}
// 							this.Enabled = StringParsers.ParseBool(_value, 0, -1, true);
// 							return true;
// 						}
// 					}
// 					else if (num <= 353658589U)
// 					{
// 						if (num != 324448160U)
// 						{
// 							if (num != 353658589U)
// 							{
// 								return false;
// 							}
// 							if (!(_attribute == "gamepad_selectable"))
// 							{
// 								return false;
// 							}
// 							this.IsNavigatable = StringParsers.ParseBool(_value, 0, -1, true);
// 							this.gamepadSelectableSetFromAttributes = true;
// 							return true;
// 						}
// 						else
// 						{
// 							if (!(_attribute == "anchor_top"))
// 							{
// 								return false;
// 							}
// 							this.anchorTop = _value;
// 							this.isDirty = true;
// 							return true;
// 						}
// 					}
// 					else if (num != 449234616U)
// 					{
// 						if (num != 450319007U)
// 						{
// 							return false;
// 						}
// 						if (!(_attribute == "use_selection_box"))
// 						{
// 							return false;
// 						}
// 						this.UseSelectionBox = StringParsers.ParseBool(_value, 0, -1, true);
// 						return true;
// 					}
// 					else
// 					{
// 						if (!(_attribute == "repeat_content"))
// 						{
// 							return false;
// 						}
// 						this.RepeatContent = StringParsers.ParseBool(_value, 0, -1, true);
// 						return true;
// 					}
// 				}
// 				else if (num <= 847898140U)
// 				{
// 					if (num <= 568213883U)
// 					{
// 						if (num != 564937055U)
// 						{
// 							if (num != 568213883U)
// 							{
// 								return false;
// 							}
// 							if (!(_attribute == "sound_volume"))
// 							{
// 								return false;
// 							}
// 							this.soundVolume = StringParsers.ParseFloat(_value, 0, -1, NumberStyles.Any);
// 							return true;
// 						}
// 						else
// 						{
// 							if (!(_attribute == "rotation"))
// 							{
// 								return false;
// 							}
// 							this.Rotation = (float)int.Parse(_value);
// 							return true;
// 						}
// 					}
// 					else if (num != 597743964U)
// 					{
// 						if (num != 847898140U)
// 						{
// 							return false;
// 						}
// 						if (!(_attribute == "anchor_left"))
// 						{
// 							return false;
// 						}
// 						this.anchorLeft = _value;
// 						this.isDirty = true;
// 						return true;
// 					}
// 					else
// 					{
// 						if (!(_attribute == "size"))
// 						{
// 							return false;
// 						}
// 						this.Size = StringParsers.ParseVector2i(_value, ',');
// 						return true;
// 					}
// 				}
// 				else if (num <= 1113510858U)
// 				{
// 					if (num != 920647948U)
// 					{
// 						if (num != 1113510858U)
// 						{
// 							return false;
// 						}
// 						if (!(_attribute == "value"))
// 						{
// 							return false;
// 						}
// 						this.Value = _value;
// 						return true;
// 					}
// 					else
// 					{
// 						if (!(_attribute == "hold_timed_step_acceleration"))
// 						{
// 							return false;
// 						}
// 						this.holdEventIntervalAcceleration = StringParsers.ParseFloat(_value, 0, -1, NumberStyles.Any);
// 						return true;
// 					}
// 				}
// 				else if (num != 1194818820U)
// 				{
// 					if (num != 1214160313U)
// 					{
// 						return false;
// 					}
// 					if (!(_attribute == "aspect_ratio"))
// 					{
// 						return false;
// 					}
// 					this.aspectRatio = StringParsers.ParseFloat(_value, 0, -1, NumberStyles.Any);
// 					return true;
// 				}
// 				else
// 				{
// 					if (!(_attribute == "padding_bottom"))
// 					{
// 						return false;
// 					}
// 					this.padding = this.padding.SetBottom(StringParsers.ParseSInt32(_value, 0, -1, NumberStyles.Integer));
// 					return true;
// 				}
// 			}
// 			else if (num <= 1988106130U)
// 			{
// 				if (num <= 1412654217U)
// 				{
// 					if (num != 1259646524U)
// 					{
// 						if (num != 1322393624U)
// 						{
// 							if (num != 1412654217U)
// 							{
// 								return false;
// 							}
// 							if (!(_attribute == "pos"))
// 							{
// 								return false;
// 							}
// 						}
// 						else
// 						{
// 							if (!(_attribute == "on_doubleclick"))
// 							{
// 								return false;
// 							}
// 							this.EventOnDoubleClick = StringParsers.ParseBool(_value, 0, -1, true);
// 							return true;
// 						}
// 					}
// 					else
// 					{
// 						if (!(_attribute == "on_press"))
// 						{
// 							return false;
// 						}
// 						this.EventOnPress = StringParsers.ParseBool(_value, 0, -1, true);
// 						return true;
// 					}
// 				}
// 				else if (num <= 1577512446U)
// 				{
// 					if (num != 1425709473U)
// 					{
// 						if (num != 1577512446U)
// 						{
// 							return false;
// 						}
// 						if (!(_attribute == "anchor_bottom"))
// 						{
// 							return false;
// 						}
// 						this.anchorBottom = _value;
// 						this.isDirty = true;
// 						return true;
// 					}
// 					else
// 					{
// 						if (!(_attribute == "visible"))
// 						{
// 							return false;
// 						}
// 						this.IsVisible = StringParsers.ParseBool(_value, 0, -1, true);
// 						return true;
// 					}
// 				}
// 				else if (num != 1936866307U)
// 				{
// 					if (num != 1988106130U)
// 					{
// 						return false;
// 					}
// 					if (!(_attribute == "padding_left"))
// 					{
// 						return false;
// 					}
// 					this.padding = this.padding.SetLeft(StringParsers.ParseSInt32(_value, 0, -1, NumberStyles.Integer));
// 					return true;
// 				}
// 				else
// 				{
// 					if (!(_attribute == "nav_down"))
// 					{
// 						return false;
// 					}
// 					if (!string.IsNullOrEmpty(_value))
// 					{
// 						this.navDownTargetString = _value;
// 						this.navDownSetFromXML = true;
// 						this.isDirty = true;
// 						return true;
// 					}
// 					return true;
// 				}
// 			}
// 			else if (num <= 2180766004U)
// 			{
// 				if (num <= 2157316278U)
// 				{
// 					if (num != 2010377433U)
// 					{
// 						if (num != 2157316278U)
// 						{
// 							return false;
// 						}
// 						if (!(_attribute == "padding"))
// 						{
// 							return false;
// 						}
// 						XUiSideSizes.TryParse(_value, out this.padding, _attribute);
// 						return true;
// 					}
// 					else
// 					{
// 						if (!(_attribute == "hold_timed_step_divider"))
// 						{
// 							return false;
// 						}
// 						return true;
// 					}
// 				}
// 				else if (num != 2168617663U)
// 				{
// 					if (num != 2180766004U)
// 					{
// 						return false;
// 					}
// 					if (!(_attribute == "tooltip_key"))
// 					{
// 						return false;
// 					}
// 					this.ToolTip = Localization.Get(_value, false);
// 					return true;
// 				}
// 				else
// 				{
// 					if (!(_attribute == "padding_right"))
// 					{
// 						return false;
// 					}
// 					this.padding = this.padding.SetRight(StringParsers.ParseSInt32(_value, 0, -1, NumberStyles.Integer));
// 					return true;
// 				}
// 			}
// 			else if (num <= 2291184263U)
// 			{
// 				if (num != 2237619868U)
// 				{
// 					if (num != 2291184263U)
// 					{
// 						return false;
// 					}
// 					if (!(_attribute == "anchor_parent_id"))
// 					{
// 						return false;
// 					}
// 					this.anchorContainerName = _value;
// 					return true;
// 				}
// 				else
// 				{
// 					if (!(_attribute == "sound_play_on_hover"))
// 					{
// 						return false;
// 					}
// 					this.xui.LoadData<AudioClip>(_value, delegate(AudioClip _o)
// 					{
// 						this.xuiHoverSound = _o;
// 					});
// 					return true;
// 				}
// 			}
// 			else if (num != 2369371622U)
// 			{
// 				if (num != 2427138910U)
// 				{
// 					return false;
// 				}
// 				if (!(_attribute == "nav_left"))
// 				{
// 					return false;
// 				}
// 				if (!string.IsNullOrEmpty(_value))
// 				{
// 					this.navLeftTargetString = _value;
// 					this.navLeftSetFromXML = true;
// 					this.isDirty = true;
// 					return true;
// 				}
// 				return true;
// 			}
// 			else
// 			{
// 				if (!(_attribute == "name"))
// 				{
// 					return false;
// 				}
// 				this.id = _value;
// 				return true;
// 			}
// 		}
// 		else if (num <= 3136805134U)
// 		{
// 			if (num <= 2796611931U)
// 			{
// 				if (num <= 2608083357U)
// 				{
// 					if (num != 2471448074U)
// 					{
// 						if (num != 2508680735U)
// 						{
// 							if (num != 2608083357U)
// 							{
// 								return false;
// 							}
// 							if (!(_attribute == "snap"))
// 							{
// 								return false;
// 							}
// 							this.IsSnappable = StringParsers.ParseBool(_value, 0, -1, true);
// 							return true;
// 						}
// 						else
// 						{
// 							if (!(_attribute == "width"))
// 							{
// 								return false;
// 							}
// 							int y = this.Size.y;
// 							int num2;
// 							if (_value.Contains("%"))
// 							{
// 								_value = _value.Replace("%", "");
// 								if (int.TryParse(_value, out num2))
// 								{
// 									num2 = (int)((float)num2 / 100f) * _parent.ViewComponent.Size.x;
// 								}
// 							}
// 							else
// 							{
// 								int.TryParse(_value, out num2);
// 							}
// 							this.Size = new Vector2i(num2, y);
// 							return true;
// 						}
// 					}
// 					else if (!(_attribute == "position"))
// 					{
// 						return false;
// 					}
// 				}
// 				else if (num <= 2664078777U)
// 				{
// 					if (num != 2641117421U)
// 					{
// 						if (num != 2664078777U)
// 						{
// 							return false;
// 						}
// 						if (!(_attribute == "hold_timed_final_interval"))
// 						{
// 							return false;
// 						}
// 						this.holdEventIntervalFinal = StringParsers.ParseFloat(_value, 0, -1, NumberStyles.Any);
// 						return true;
// 					}
// 					else
// 					{
// 						if (!(_attribute == "hold_timed_initial_interval"))
// 						{
// 							return false;
// 						}
// 						this.holdEventIntervalInitial = StringParsers.ParseFloat(_value, 0, -1, NumberStyles.Any);
// 						return true;
// 					}
// 				}
// 				else if (num != 2667237188U)
// 				{
// 					if (num != 2796611931U)
// 					{
// 						return false;
// 					}
// 					if (!(_attribute == "on_drag"))
// 					{
// 						return false;
// 					}
// 					this.EventOnDrag = StringParsers.ParseBool(_value, 0, -1, true);
// 					return true;
// 				}
// 				else
// 				{
// 					if (!(_attribute == "collider_scale"))
// 					{
// 						return false;
// 					}
// 					this.colliderScale = StringParsers.ParseFloat(_value, 0, -1, NumberStyles.Any);
// 					return true;
// 				}
// 			}
// 			else if (num <= 2857717125U)
// 			{
// 				if (num <= 2845250466U)
// 				{
// 					if (num != 2830671030U)
// 					{
// 						if (num != 2845250466U)
// 						{
// 							return false;
// 						}
// 						if (!(_attribute == "padding_top"))
// 						{
// 							return false;
// 						}
// 						this.padding = this.padding.SetTop(StringParsers.ParseSInt32(_value, 0, -1, NumberStyles.Integer));
// 						return true;
// 					}
// 					else
// 					{
// 						if (!(_attribute == "on_held"))
// 						{
// 							return false;
// 						}
// 						this.EventOnHeld = StringParsers.ParseBool(_value, 0, -1, true);
// 						return true;
// 					}
// 				}
// 				else if (num != 2854231290U)
// 				{
// 					if (num != 2857717125U)
// 					{
// 						return false;
// 					}
// 					if (!(_attribute == "disabled_tooltip_key"))
// 					{
// 						return false;
// 					}
// 					this.DisabledToolTip = Localization.Get(_value, false);
// 					return true;
// 				}
// 				else
// 				{
// 					if (!(_attribute == "anchor_side"))
// 					{
// 						return false;
// 					}
// 					this.isAnchored = true;
// 					this.anchorSide = EnumUtils.Parse<UIAnchor.Side>(_value, true);
// 					return true;
// 				}
// 			}
// 			else if (num <= 2966252344U)
// 			{
// 				if (num != 2902596496U)
// 				{
// 					if (num != 2966252344U)
// 					{
// 						return false;
// 					}
// 					if (!(_attribute == "hold_delay"))
// 					{
// 						return false;
// 					}
// 					this.holdDelay = StringParsers.ParseFloat(_value, 0, -1, NumberStyles.Any);
// 					return true;
// 				}
// 				else
// 				{
// 					if (!(_attribute == "ignoreparentpadding"))
// 					{
// 						return false;
// 					}
// 					this.IgnoreParentPadding = StringParsers.ParseBool(_value, 0, -1, true);
// 					return true;
// 				}
// 			}
// 			else if (num != 3135069273U)
// 			{
// 				if (num != 3136805134U)
// 				{
// 					return false;
// 				}
// 				if (!(_attribute == "anchor_offset"))
// 				{
// 					return false;
// 				}
// 				this.anchorOffset = StringParsers.ParseVector2(_value);
// 				return true;
// 			}
// 			else
// 			{
// 				if (!(_attribute == "force_hide"))
// 				{
// 					return false;
// 				}
// 				this.ForceHide = StringParsers.ParseBool(_value, 0, -1, true);
// 				return true;
// 			}
// 		}
// 		else if (num <= 3705907993U)
// 		{
// 			if (num <= 3529586537U)
// 			{
// 				if (num != 3431509877U)
// 				{
// 					if (num != 3460649205U)
// 					{
// 						if (num != 3529586537U)
// 						{
// 							return false;
// 						}
// 						if (!(_attribute == "anchor_right"))
// 						{
// 							return false;
// 						}
// 						this.anchorRight = _value;
// 						this.isDirty = true;
// 						return true;
// 					}
// 					else
// 					{
// 						if (!(_attribute == "disabled_tooltip"))
// 						{
// 							return false;
// 						}
// 						this.DisabledToolTip = _value;
// 						return true;
// 					}
// 				}
// 				else
// 				{
// 					if (!(_attribute == "on_hover"))
// 					{
// 						return false;
// 					}
// 					this.EventOnHover = StringParsers.ParseBool(_value, 0, -1, true);
// 					return true;
// 				}
// 			}
// 			else if (num <= 3585981250U)
// 			{
// 				if (num != 3545960405U)
// 				{
// 					if (num != 3585981250U)
// 					{
// 						return false;
// 					}
// 					if (!(_attribute == "height"))
// 					{
// 						return false;
// 					}
// 					int x = this.Size.x;
// 					int num3;
// 					if (_value.Contains("%"))
// 					{
// 						_value = _value.Replace("%", "");
// 						if (int.TryParse(_value, out num3))
// 						{
// 							num3 = (int)((float)num3 / 100f) * _parent.ViewComponent.Size.y;
// 						}
// 					}
// 					else
// 					{
// 						int.TryParse(_value, out num3);
// 					}
// 					this.Size = new Vector2i(x, num3);
// 					return true;
// 				}
// 				else
// 				{
// 					if (!(_attribute == "on_select"))
// 					{
// 						return false;
// 					}
// 					this.EventOnSelect = StringParsers.ParseBool(_value, 0, -1, true);
// 					return true;
// 				}
// 			}
// 			else if (num != 3623547202U)
// 			{
// 				if (num != 3705907993U)
// 				{
// 					return false;
// 				}
// 				if (!(_attribute == "sound_play_on_press"))
// 				{
// 					return false;
// 				}
// 				this.soundPlayOnClick = StringParsers.ParseBool(_value, 0, -1, true);
// 				return true;
// 			}
// 			else
// 			{
// 				if (!(_attribute == "nav_up"))
// 				{
// 					return false;
// 				}
// 				if (!string.IsNullOrEmpty(_value))
// 				{
// 					this.navUpTargetString = _value;
// 					this.navUpSetFromXML = true;
// 					this.isDirty = true;
// 					return true;
// 				}
// 				return true;
// 			}
// 		}
// 		else if (num <= 4041470899U)
// 		{
// 			if (num <= 3950266292U)
// 			{
// 				if (num != 3741212336U)
// 				{
// 					if (num != 3950266292U)
// 					{
// 						return false;
// 					}
// 					if (!(_attribute == "on_scroll"))
// 					{
// 						return false;
// 					}
// 					this.EventOnScroll = StringParsers.ParseBool(_value, 0, -1, true);
// 					return true;
// 				}
// 				else
// 				{
// 					if (!(_attribute == "tooltip"))
// 					{
// 						return false;
// 					}
// 					this.ToolTip = _value;
// 					return true;
// 				}
// 			}
// 			else if (num != 3983471730U)
// 			{
// 				if (num != 4041470899U)
// 				{
// 					return false;
// 				}
// 				if (!(_attribute == "nav_right"))
// 				{
// 					return false;
// 				}
// 				if (!string.IsNullOrEmpty(_value))
// 				{
// 					this.navRightTargetString = _value;
// 					this.navRightSetFromXML = true;
// 					this.isDirty = true;
// 					return true;
// 				}
// 				return true;
// 			}
// 			else
// 			{
// 				if (!(_attribute == "sound_play_on_open"))
// 				{
// 					return false;
// 				}
// 				this.soundPlayOnOpen = StringParsers.ParseBool(_value, 0, -1, true);
// 				return true;
// 			}
// 		}
// 		else if (num <= 4226831639U)
// 		{
// 			if (num != 4121269289U)
// 			{
// 				if (num != 4226831639U)
// 				{
// 					return false;
// 				}
// 				if (!(_attribute == "pivot"))
// 				{
// 					return false;
// 				}
// 				this.Pivot = EnumUtils.Parse<UIWidget.Pivot>(_value, true);
// 				return true;
// 			}
// 			else
// 			{
// 				if (!(_attribute == "keep_aspect_ratio"))
// 				{
// 					return false;
// 				}
// 				this.keepAspectRatio = EnumUtils.Parse<UIWidget.AspectRatioSource>(_value, false);
// 				return true;
// 			}
// 		}
// 		else if (num != 4265554070U)
// 		{
// 			if (num != 4269121258U)
// 			{
// 				return false;
// 			}
// 			if (!(_attribute == "depth"))
// 			{
// 				return false;
// 			}
// 			int num4;
// 			int.TryParse(_value, out num4);
// 			int num5 = num4;
// 			XUiView viewComponent = _parent.ViewComponent;
// 			this.Depth = num5 + ((viewComponent != null) ? viewComponent.Depth : 0);
// 			return true;
// 		}
// 		else
// 		{
// 			if (!(_attribute == "repeat_count"))
// 			{
// 				return false;
// 			}
// 			this.RepeatCount = StringParsers.ParseSInt32(_value, 0, -1, NumberStyles.Integer);
// 			return true;
// 		}
// 		this.Position = StringParsers.ParseVector2i(_value, ',');
// 		return true;
// 	}

// 	// Token: 0x06007988 RID: 31112 RVA: 0x00002914 File Offset: 0x00000B14
// 	public virtual void setRepeatContentTemplateParams(Dictionary<string, object> _templateParams, int _curRepeatNum)
// 	{
// 	}

// 	// Token: 0x0600798A RID: 31114 RVA: 0x00316AC0 File Offset: 0x00314CC0
// 	[CompilerGenerated]
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public XUiView <parseNavigationTargets>g__findView|218_0(string _name)
// 	{
// 		if (string.IsNullOrEmpty(_name))
// 		{
// 			return null;
// 		}
// 		XUiView xuiView = this.findHierarchyClosestView(_name);
// 		if (xuiView == null)
// 		{
// 			XUiController childById = this.controller.WindowGroup.Controller.GetChildById(_name);
// 			xuiView = ((childById != null) ? childById.ViewComponent : null);
// 			if (xuiView == null)
// 			{
// 				throw new ArgumentException("Invalid navigation target, view component with name '" + _name + "' not found.\nOn: " + this.controller.GetXuiHierarchy());
// 			}
// 		}
// 		return xuiView;
// 	}

// 	// Token: 0x0600798B RID: 31115 RVA: 0x00316B2A File Offset: 0x00314D2A
// 	[CompilerGenerated]
// 	[PublicizedFrom(EAccessModifier.Internal)]
// 	public static IEnumerator <parseAnchors>g__markAsChangedLater|219_0(UIWidget _target)
// 	{
// 		yield return new WaitForEndOfFrame();
// 		_target.MarkAsChanged();
// 		yield break;
// 	}

// 	// Token: 0x04005BEC RID: 23532
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public static readonly Dictionary<Type, GameObject> componentTemplates = new Dictionary<Type, GameObject>();

// 	// Token: 0x04005BED RID: 23533
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public static Transform templatesParent;

// 	// Token: 0x04005BF0 RID: 23536
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public string id;

// 	// Token: 0x04005BF1 RID: 23537
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public Transform uiTransform;

// 	// Token: 0x04005BF2 RID: 23538
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public BoxCollider collider;

// 	// Token: 0x04005BF3 RID: 23539
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public UIAnchor anchor;

// 	// Token: 0x04005BF4 RID: 23540
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public Vector2i size;

// 	// Token: 0x04005BF5 RID: 23541
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public XUiSideSizes padding;

// 	// Token: 0x04005BF6 RID: 23542
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public bool ignoreParentPadding;

// 	// Token: 0x04005BF7 RID: 23543
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public Vector2i position;

// 	// Token: 0x04005BF8 RID: 23544
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public bool positionDirty;

// 	// Token: 0x04005BF9 RID: 23545
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public float rotation;

// 	// Token: 0x04005BFA RID: 23546
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public bool rotationDirty;

// 	// Token: 0x04005BFB RID: 23547
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public bool isVisible;

// 	// Token: 0x04005BFC RID: 23548
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public bool forceHide;

// 	// Token: 0x04005BFD RID: 23549
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public UIWidget.Pivot pivot;

// 	// Token: 0x04005BFE RID: 23550
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public XUi.Alignment alignment;

// 	// Token: 0x04005BFF RID: 23551
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public int depth;

// 	// Token: 0x04005C00 RID: 23552
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public XUiController controller;

// 	// Token: 0x04005C01 RID: 23553
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public string m_value;

// 	// Token: 0x04005C02 RID: 23554
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public bool initialized;

// 	// Token: 0x04005C03 RID: 23555
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public UIWidget.AspectRatioSource keepAspectRatio;

// 	// Token: 0x04005C04 RID: 23556
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public float aspectRatio;

// 	// Token: 0x04005C05 RID: 23557
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public string anchorLeft;

// 	// Token: 0x04005C06 RID: 23558
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public string anchorRight;

// 	// Token: 0x04005C07 RID: 23559
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public string anchorBottom;

// 	// Token: 0x04005C08 RID: 23560
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public string anchorTop;

// 	// Token: 0x04005C09 RID: 23561
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public string anchorLeftParsed;

// 	// Token: 0x04005C0A RID: 23562
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public string anchorRightParsed;

// 	// Token: 0x04005C0B RID: 23563
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public string anchorBottomParsed;

// 	// Token: 0x04005C0C RID: 23564
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public string anchorTopParsed;

// 	// Token: 0x04005C0D RID: 23565
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public bool isAnchored;

// 	// Token: 0x04005C0E RID: 23566
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public UIAnchor.Side anchorSide = UIAnchor.Side.Center;

// 	// Token: 0x04005C0F RID: 23567
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public bool anchorRunOnce = true;

// 	// Token: 0x04005C10 RID: 23568
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public Vector2 anchorOffset = Vector2.zero;

// 	// Token: 0x04005C11 RID: 23569
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public string anchorContainerName;

// 	// Token: 0x04005C12 RID: 23570
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public Transform rootNode;

// 	// Token: 0x04005C13 RID: 23571
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public AudioClip xuiSound;

// 	// Token: 0x04005C14 RID: 23572
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public AudioClip xuiHoverSound;

// 	// Token: 0x04005C15 RID: 23573
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public string toolTip;

// 	// Token: 0x04005C16 RID: 23574
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public string disabledToolTip;

// 	// Token: 0x04005C17 RID: 23575
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public bool soundPlayOnClick;

// 	// Token: 0x04005C18 RID: 23576
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public bool soundPlayOnHover;

// 	// Token: 0x04005C19 RID: 23577
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public bool soundPlayOnOpen;

// 	// Token: 0x04005C1A RID: 23578
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public bool soundLoop;

// 	// Token: 0x04005C1B RID: 23579
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public float soundVolume;

// 	// Token: 0x04005C1C RID: 23580
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public float holdDelay = 0.5f;

// 	// Token: 0x04005C1D RID: 23581
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public float holdEventIntervalInitial = 0.5f;

// 	// Token: 0x04005C1E RID: 23582
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public float holdEventIntervalFinal = 0.06f;

// 	// Token: 0x04005C1F RID: 23583
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public float holdEventIntervalAcceleration = 0.015f;

// 	// Token: 0x04005C20 RID: 23584
// 	public bool EventOnHover;

// 	// Token: 0x04005C21 RID: 23585
// 	public bool EventOnPress;

// 	// Token: 0x04005C22 RID: 23586
// 	public bool EventOnDoubleClick;

// 	// Token: 0x04005C23 RID: 23587
// 	public bool EventOnHeld;

// 	// Token: 0x04005C24 RID: 23588
// 	public bool EventOnScroll;

// 	// Token: 0x04005C25 RID: 23589
// 	public bool EventOnDrag;

// 	// Token: 0x04005C26 RID: 23590
// 	public bool EventOnSelect;

// 	// Token: 0x04005C27 RID: 23591
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public bool enabled = true;

// 	// Token: 0x04005C28 RID: 23592
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public bool isDirty;

// 	// Token: 0x04005C29 RID: 23593
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public float colliderScale = 1f;

// 	// Token: 0x04005C2A RID: 23594
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public bool _isNavigatable = true;

// 	// Token: 0x04005C2B RID: 23595
// 	public bool IsSnappable = true;

// 	// Token: 0x04005C2C RID: 23596
// 	public bool UseSelectionBox = true;

// 	// Token: 0x04005C2D RID: 23597
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public bool gamepadSelectableSetFromAttributes;

// 	// Token: 0x04005C2E RID: 23598
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public bool navLeftSetFromXML;

// 	// Token: 0x04005C2F RID: 23599
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public bool navRightSetFromXML;

// 	// Token: 0x04005C30 RID: 23600
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public bool navUpSetFromXML;

// 	// Token: 0x04005C31 RID: 23601
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public bool navDownSetFromXML;

// 	// Token: 0x04005C32 RID: 23602
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public string navUpTargetString;

// 	// Token: 0x04005C33 RID: 23603
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public XUiView navUpTarget;

// 	// Token: 0x04005C34 RID: 23604
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public string navDownTargetString;

// 	// Token: 0x04005C35 RID: 23605
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public XUiView navDownTarget;

// 	// Token: 0x04005C36 RID: 23606
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public string navLeftTargetString;

// 	// Token: 0x04005C37 RID: 23607
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public XUiView navLeftTarget;

// 	// Token: 0x04005C38 RID: 23608
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public string navRightTargetString;

// 	// Token: 0x04005C39 RID: 23609
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public XUiView navRightTarget;

// 	// Token: 0x04005C3A RID: 23610
// 	public bool controllerOnlyTooltip;

// 	// Token: 0x04005C3B RID: 23611
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public int viewIndex = -1;

// 	// Token: 0x04005C3C RID: 23612
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public XUi mXUi;

// 	// Token: 0x04005C3D RID: 23613
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public bool wasDragging;

// 	// Token: 0x04005C3E RID: 23614
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public bool isPressed;

// 	// Token: 0x04005C3F RID: 23615
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public bool isHold;

// 	// Token: 0x04005C40 RID: 23616
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public float pressStartTime;

// 	// Token: 0x04005C41 RID: 23617
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public float holdStartTime;

// 	// Token: 0x04005C42 RID: 23618
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public float holdEventIntervalCurrent;

// 	// Token: 0x04005C43 RID: 23619
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public float holdEventIntervalChangeSpeed;

// 	// Token: 0x04005C44 RID: 23620
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public float holdEventNextTime;

// 	// Token: 0x04005C45 RID: 23621
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public float holdEventLastTime;

// 	// Token: 0x04005C46 RID: 23622
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public bool isOver;
// }
