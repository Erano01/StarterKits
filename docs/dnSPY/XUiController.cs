// using System;
// using System.Collections.Generic;
// using Platform;
// using UnityEngine;
// using UnityEngine.Scripting;

// // Token: 0x02000C03 RID: 3075
// [Preserve]
// public class XUiController
// {
// 	// Token: 0x170009B4 RID: 2484
// 	// (get) Token: 0x06005DFE RID: 24062 RVA: 0x00262C7C File Offset: 0x00260E7C
// 	public PlayerInputManager.InputStyle CurrentInputStyle
// 	{
// 		[PublicizedFrom(EAccessModifier.Protected)]
// 		get
// 		{
// 			return this.lastInputStyle;
// 		}
// 	}

// 	// Token: 0x14000088 RID: 136
// 	// (add) Token: 0x06005DFF RID: 24063 RVA: 0x00262C84 File Offset: 0x00260E84
// 	// (remove) Token: 0x06005E00 RID: 24064 RVA: 0x00262CBC File Offset: 0x00260EBC
// 	public event XUiEvent_OnPressEventHandler OnPress;

// 	// Token: 0x14000089 RID: 137
// 	// (add) Token: 0x06005E01 RID: 24065 RVA: 0x00262CF4 File Offset: 0x00260EF4
// 	// (remove) Token: 0x06005E02 RID: 24066 RVA: 0x00262D2C File Offset: 0x00260F2C
// 	public event XUiEvent_OnPressEventHandler OnDoubleClick;

// 	// Token: 0x1400008A RID: 138
// 	// (add) Token: 0x06005E03 RID: 24067 RVA: 0x00262D64 File Offset: 0x00260F64
// 	// (remove) Token: 0x06005E04 RID: 24068 RVA: 0x00262D9C File Offset: 0x00260F9C
// 	public event XUiEvent_OnPressEventHandler OnRightPress;

// 	// Token: 0x1400008B RID: 139
// 	// (add) Token: 0x06005E05 RID: 24069 RVA: 0x00262DD4 File Offset: 0x00260FD4
// 	// (remove) Token: 0x06005E06 RID: 24070 RVA: 0x00262E0C File Offset: 0x0026100C
// 	public event XUiEvent_OnHoverEventHandler OnHover;

// 	// Token: 0x1400008C RID: 140
// 	// (add) Token: 0x06005E07 RID: 24071 RVA: 0x00262E44 File Offset: 0x00261044
// 	// (remove) Token: 0x06005E08 RID: 24072 RVA: 0x00262E7C File Offset: 0x0026107C
// 	public event XUiEvent_OnDragEventHandler OnDrag;

// 	// Token: 0x1400008D RID: 141
// 	// (add) Token: 0x06005E09 RID: 24073 RVA: 0x00262EB4 File Offset: 0x002610B4
// 	// (remove) Token: 0x06005E0A RID: 24074 RVA: 0x00262EEC File Offset: 0x002610EC
// 	public event XUiEvent_OnHeldHandler OnHold;

// 	// Token: 0x1400008E RID: 142
// 	// (add) Token: 0x06005E0B RID: 24075 RVA: 0x00262F24 File Offset: 0x00261124
// 	// (remove) Token: 0x06005E0C RID: 24076 RVA: 0x00262F5C File Offset: 0x0026115C
// 	public event XUiEvent_OnScrollEventHandler OnScroll;

// 	// Token: 0x1400008F RID: 143
// 	// (add) Token: 0x06005E0D RID: 24077 RVA: 0x00262F94 File Offset: 0x00261194
// 	// (remove) Token: 0x06005E0E RID: 24078 RVA: 0x00262FCC File Offset: 0x002611CC
// 	public event XUiEvent_OnSelectEventHandler OnSelect;

// 	// Token: 0x14000090 RID: 144
// 	// (add) Token: 0x06005E0F RID: 24079 RVA: 0x00263004 File Offset: 0x00261204
// 	// (remove) Token: 0x06005E10 RID: 24080 RVA: 0x0026303C File Offset: 0x0026123C
// 	public event XUiEvent_OnVisibilityChanged OnVisiblity;

// 	// Token: 0x170009B5 RID: 2485
// 	// (get) Token: 0x06005E11 RID: 24081 RVA: 0x00263071 File Offset: 0x00261271
// 	// (set) Token: 0x06005E12 RID: 24082 RVA: 0x00263079 File Offset: 0x00261279
// 	public XUiView ViewComponent
// 	{
// 		get
// 		{
// 			return this.viewComponent;
// 		}
// 		set
// 		{
// 			this.viewComponent = value;
// 		}
// 	}

// 	// Token: 0x170009B6 RID: 2486
// 	// (get) Token: 0x06005E13 RID: 24083 RVA: 0x00263082 File Offset: 0x00261282
// 	// (set) Token: 0x06005E14 RID: 24084 RVA: 0x0026308A File Offset: 0x0026128A
// 	public XUiController Parent
// 	{
// 		get
// 		{
// 			return this.parent;
// 		}
// 		set
// 		{
// 			this.parent = value;
// 		}
// 	}

// 	// Token: 0x170009B7 RID: 2487
// 	// (get) Token: 0x06005E15 RID: 24085 RVA: 0x00263093 File Offset: 0x00261293
// 	public List<XUiController> Children
// 	{
// 		get
// 		{
// 			return this.children;
// 		}
// 	}

// 	// Token: 0x170009B8 RID: 2488
// 	// (get) Token: 0x06005E16 RID: 24086 RVA: 0x0026309B File Offset: 0x0026129B
// 	// (set) Token: 0x06005E17 RID: 24087 RVA: 0x002630A3 File Offset: 0x002612A3
// 	public XUiWindowGroup WindowGroup
// 	{
// 		get
// 		{
// 			return this.windowGroup;
// 		}
// 		set
// 		{
// 			this.windowGroup = value;
// 		}
// 	}

// 	// Token: 0x170009B9 RID: 2489
// 	// (get) Token: 0x06005E18 RID: 24088 RVA: 0x002630AC File Offset: 0x002612AC
// 	// (set) Token: 0x06005E19 RID: 24089 RVA: 0x002630B4 File Offset: 0x002612B4
// 	public XUi xui { get; set; }

// 	// Token: 0x170009BA RID: 2490
// 	// (get) Token: 0x06005E1A RID: 24090 RVA: 0x002630BD File Offset: 0x002612BD
// 	// (set) Token: 0x06005E1B RID: 24091 RVA: 0x002630C5 File Offset: 0x002612C5
// 	public bool IsOpen { get; [PublicizedFrom(EAccessModifier.Protected)] set; }

// 	// Token: 0x06005E1C RID: 24092 RVA: 0x002630CE File Offset: 0x002612CE
// 	public XUiController()
// 	{
// 		this.parent = null;
// 	}

// 	// Token: 0x06005E1D RID: 24093 RVA: 0x00263108 File Offset: 0x00261308
// 	public XUiController(XUiController _parent)
// 	{
// 		this.parent = _parent;
// 		XUiController xuiController = this.parent;
// 		if (xuiController == null)
// 		{
// 			return;
// 		}
// 		xuiController.AddChild(this);
// 	}

// 	// Token: 0x06005E1E RID: 24094 RVA: 0x0026315C File Offset: 0x0026135C
// 	public virtual void Init()
// 	{
// 		if (this.viewComponent != null)
// 		{
// 			this.viewComponent.InitView();
// 		}
// 		for (int i = 0; i < this.children.Count; i++)
// 		{
// 			this.children[i].Init();
// 		}
// 		this.curInputStyle = PlatformManager.NativePlatform.Input.CurrentInputStyle;
// 	}

// 	// Token: 0x06005E1F RID: 24095 RVA: 0x002631B8 File Offset: 0x002613B8
// 	public virtual void UpdateInput()
// 	{
// 		for (int i = 0; i < this.children.Count; i++)
// 		{
// 			if (!this.children[i].IsDormant)
// 			{
// 				this.children[i].UpdateInput();
// 			}
// 		}
// 	}

// 	// Token: 0x06005E20 RID: 24096 RVA: 0x00263200 File Offset: 0x00261400
// 	public virtual void Update(float _dt)
// 	{
// 		if (this.viewComponent != null && this.windowGroup != null && this.windowGroup.isShowing && this.viewComponent.IsVisible)
// 		{
// 			this.viewComponent.Update(_dt);
// 		}
// 		if (this.curInputStyle != this.lastInputStyle)
// 		{
// 			PlayerInputManager.InputStyle oldStyle = this.lastInputStyle;
// 			this.lastInputStyle = this.curInputStyle;
// 			this.RefreshBindings(false);
// 			this.InputStyleChanged(oldStyle, this.lastInputStyle);
// 		}
// 		for (int i = 0; i < this.children.Count; i++)
// 		{
// 			XUiController xuiController = this.children[i];
// 			if (!xuiController.IsDormant)
// 			{
// 				xuiController.Update(_dt);
// 			}
// 		}
// 	}

// 	// Token: 0x06005E21 RID: 24097 RVA: 0x00002914 File Offset: 0x00000B14
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public virtual void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
// 	{
// 	}

// 	// Token: 0x06005E22 RID: 24098 RVA: 0x002632AC File Offset: 0x002614AC
// 	public void ForceInputStyleChange(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
// 	{
// 		if (this.registeredForInputStyleChanges)
// 		{
// 			this.InputStyleChanged(_oldStyle, _newStyle);
// 		}
// 		foreach (XUiController xuiController in this.children)
// 		{
// 			this.ForceInputStyleChange(_oldStyle, _newStyle);
// 		}
// 	}

// 	// Token: 0x06005E23 RID: 24099 RVA: 0x00263310 File Offset: 0x00261510
// 	public bool TryGetChildByIdAndType<T>(string _id, out T _child) where T : XUiController
// 	{
// 		if (this.viewComponent != null && string.Equals(this.viewComponent.ID, _id, StringComparison.OrdinalIgnoreCase))
// 		{
// 			T t = this as T;
// 			if (t != null)
// 			{
// 				_child = t;
// 				return true;
// 			}
// 		}
// 		for (int i = 0; i < this.children.Count; i++)
// 		{
// 			if (this.children[i].TryGetChildByIdAndType<T>(_id, out _child))
// 			{
// 				return true;
// 			}
// 		}
// 		_child = default(T);
// 		return false;
// 	}

// 	// Token: 0x06005E24 RID: 24100 RVA: 0x0026338C File Offset: 0x0026158C
// 	public XUiController GetChildById(string _id)
// 	{
// 		XUiController xuiController = null;
// 		if (this.viewComponent != null && string.Equals(this.viewComponent.ID, _id, StringComparison.OrdinalIgnoreCase))
// 		{
// 			xuiController = this;
// 		}
// 		else
// 		{
// 			for (int i = 0; i < this.children.Count; i++)
// 			{
// 				xuiController = this.children[i].GetChildById(_id);
// 				if (xuiController != null)
// 				{
// 					break;
// 				}
// 			}
// 		}
// 		return xuiController;
// 	}

// 	// Token: 0x06005E25 RID: 24101 RVA: 0x002633E8 File Offset: 0x002615E8
// 	public XUiController[] GetChildrenById(string _id, List<XUiController> _list = null)
// 	{
// 		List<XUiController> list;
// 		if (_list == null)
// 		{
// 			list = new List<XUiController>();
// 		}
// 		else
// 		{
// 			list = _list;
// 		}
// 		if (this.viewComponent != null && string.Equals(this.viewComponent.ID, _id, StringComparison.OrdinalIgnoreCase))
// 		{
// 			list.Add(this);
// 		}
// 		else
// 		{
// 			for (int i = 0; i < this.children.Count; i++)
// 			{
// 				this.children[i].GetChildrenById(_id, list);
// 			}
// 		}
// 		if (_list == null)
// 		{
// 			return list.ToArray();
// 		}
// 		return null;
// 	}

// 	// Token: 0x06005E26 RID: 24102 RVA: 0x0026345C File Offset: 0x0026165C
// 	public T GetChildByType<T>() where T : XUiController
// 	{
// 		T t = this as T;
// 		if (t == null)
// 		{
// 			foreach (XUiController xuiController in this.children)
// 			{
// 				t = xuiController.GetChildByType<T>();
// 				if (t != null)
// 				{
// 					break;
// 				}
// 			}
// 		}
// 		return t;
// 	}

// 	// Token: 0x06005E27 RID: 24103 RVA: 0x002634CC File Offset: 0x002616CC
// 	public T[] GetChildrenByType<T>(List<T> _list = null) where T : XUiController
// 	{
// 		List<T> list;
// 		if (_list == null)
// 		{
// 			list = new List<T>();
// 		}
// 		else
// 		{
// 			list = _list;
// 		}
// 		T t = this as T;
// 		if (t != null)
// 		{
// 			list.Add(t);
// 		}
// 		else
// 		{
// 			foreach (XUiController xuiController in this.children)
// 			{
// 				xuiController.GetChildrenByType<T>(list);
// 			}
// 		}
// 		if (_list == null)
// 		{
// 			return list.ToArray();
// 		}
// 		return null;
// 	}

// 	// Token: 0x06005E28 RID: 24104 RVA: 0x00263554 File Offset: 0x00261754
// 	public T[] GetChildrenByViewType<T>(List<T> _list = null) where T : XUiView
// 	{
// 		List<T> list;
// 		if (_list == null)
// 		{
// 			list = new List<T>();
// 		}
// 		else
// 		{
// 			list = _list;
// 		}
// 		T t = this.viewComponent as T;
// 		if (t != null)
// 		{
// 			list.Add(t);
// 		}
// 		else
// 		{
// 			foreach (XUiController xuiController in this.children)
// 			{
// 				xuiController.GetChildrenByViewType<T>(list);
// 			}
// 		}
// 		if (_list == null)
// 		{
// 			return list.ToArray();
// 		}
// 		return null;
// 	}

// 	// Token: 0x06005E29 RID: 24105 RVA: 0x002635E0 File Offset: 0x002617E0
// 	public T GetParentByType<T>() where T : XUiController
// 	{
// 		if (this is T)
// 		{
// 			return this as T;
// 		}
// 		if (this.Parent != null)
// 		{
// 			return this.Parent.GetParentByType<T>();
// 		}
// 		return default(T);
// 	}

// 	// Token: 0x06005E2A RID: 24106 RVA: 0x0026361E File Offset: 0x0026181E
// 	public bool IsChildOf(XUiController _controller)
// 	{
// 		return this.Parent != null && (this.Parent == _controller || this.Parent.IsChildOf(_controller));
// 	}

// 	// Token: 0x06005E2B RID: 24107 RVA: 0x00263644 File Offset: 0x00261844
// 	public XUiV_Window GetParentWindow()
// 	{
// 		XUiV_Window xuiV_Window = this.ViewComponent as XUiV_Window;
// 		if (xuiV_Window != null)
// 		{
// 			return xuiV_Window;
// 		}
// 		XUiController xuiController = this.Parent;
// 		if (xuiController == null)
// 		{
// 			return null;
// 		}
// 		return xuiController.GetParentWindow();
// 	}

// 	// Token: 0x06005E2C RID: 24108 RVA: 0x00263673 File Offset: 0x00261873
// 	public void AddChild(XUiController _child)
// 	{
// 		this.children.Add(_child);
// 	}

// 	// Token: 0x06005E2D RID: 24109 RVA: 0x00263681 File Offset: 0x00261881
// 	public void Pressed(int _mouseButton)
// 	{
// 		this.OnPressed(_mouseButton);
// 	}

// 	// Token: 0x06005E2E RID: 24110 RVA: 0x0026368A File Offset: 0x0026188A
// 	public void DoubleClicked(int _mouseButton)
// 	{
// 		this.OnDoubleClicked(_mouseButton);
// 	}

// 	// Token: 0x06005E2F RID: 24111 RVA: 0x00263693 File Offset: 0x00261893
// 	public void Hovered(bool _isOver)
// 	{
// 		this.OnHovered(_isOver);
// 	}

// 	// Token: 0x06005E30 RID: 24112 RVA: 0x0026369C File Offset: 0x0026189C
// 	public void Scrolled(float _delta)
// 	{
// 		this.OnScrolled(_delta);
// 	}

// 	// Token: 0x06005E31 RID: 24113 RVA: 0x002636A5 File Offset: 0x002618A5
// 	public void Selected(bool _selected)
// 	{
// 		this.OnSelected(_selected);
// 	}

// 	// Token: 0x06005E32 RID: 24114 RVA: 0x002636AE File Offset: 0x002618AE
// 	public void Dragged(Vector2 _mouseDelta, EDragType _dragType)
// 	{
// 		this.OnDragged(_dragType, _mouseDelta);
// 	}

// 	// Token: 0x06005E33 RID: 24115 RVA: 0x002636B8 File Offset: 0x002618B8
// 	public void Held(EHoldType _event, float _holdDuration, float _deltaSinceLastTimedEvent = -1f)
// 	{
// 		this.OnHeld(_event, _holdDuration, _deltaSinceLastTimedEvent);
// 	}

// 	// Token: 0x06005E34 RID: 24116 RVA: 0x002636C3 File Offset: 0x002618C3
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public virtual void OnPressed(int _mouseButton)
// 	{
// 		if (_mouseButton != -1)
// 		{
// 			if (_mouseButton == -2)
// 			{
// 				XUiEvent_OnPressEventHandler onRightPress = this.OnRightPress;
// 				if (onRightPress == null)
// 				{
// 					return;
// 				}
// 				onRightPress(this, _mouseButton);
// 			}
// 			return;
// 		}
// 		XUiEvent_OnPressEventHandler onPress = this.OnPress;
// 		if (onPress == null)
// 		{
// 			return;
// 		}
// 		onPress(this, _mouseButton);
// 	}

// 	// Token: 0x06005E35 RID: 24117 RVA: 0x002636F3 File Offset: 0x002618F3
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public virtual void OnDoubleClicked(int _mouseButton)
// 	{
// 		XUiEvent_OnPressEventHandler onDoubleClick = this.OnDoubleClick;
// 		if (onDoubleClick == null)
// 		{
// 			return;
// 		}
// 		onDoubleClick(this, _mouseButton);
// 	}

// 	// Token: 0x06005E36 RID: 24118 RVA: 0x00263707 File Offset: 0x00261907
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public virtual void OnHovered(bool _isOver)
// 	{
// 		XUiEvent_OnHoverEventHandler onHover = this.OnHover;
// 		if (onHover == null)
// 		{
// 			return;
// 		}
// 		onHover(this, _isOver);
// 	}

// 	// Token: 0x06005E37 RID: 24119 RVA: 0x0026371B File Offset: 0x0026191B
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public virtual void OnDragged(EDragType _dragType, Vector2 _mousePositionDelta)
// 	{
// 		XUiEvent_OnDragEventHandler onDrag = this.OnDrag;
// 		if (onDrag == null)
// 		{
// 			return;
// 		}
// 		onDrag(this, _dragType, _mousePositionDelta);
// 	}

// 	// Token: 0x06005E38 RID: 24120 RVA: 0x00263730 File Offset: 0x00261930
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void OnHeld(EHoldType _event, float _holdDuration, float _deltaSinceLastTimedEvent)
// 	{
// 		XUiEvent_OnHeldHandler onHold = this.OnHold;
// 		if (onHold == null)
// 		{
// 			return;
// 		}
// 		onHold(this, _event, _holdDuration, _deltaSinceLastTimedEvent);
// 	}

// 	// Token: 0x06005E39 RID: 24121 RVA: 0x00263746 File Offset: 0x00261946
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public virtual void OnScrolled(float _delta)
// 	{
// 		XUiEvent_OnScrollEventHandler onScroll = this.OnScroll;
// 		if (onScroll == null)
// 		{
// 			return;
// 		}
// 		onScroll(this, _delta);
// 	}

// 	// Token: 0x06005E3A RID: 24122 RVA: 0x0026375A File Offset: 0x0026195A
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public virtual void OnSelected(bool _selected)
// 	{
// 		XUiEvent_OnSelectEventHandler onSelect = this.OnSelect;
// 		if (onSelect == null)
// 		{
// 			return;
// 		}
// 		onSelect(this, _selected);
// 	}

// 	// Token: 0x06005E3B RID: 24123 RVA: 0x00263770 File Offset: 0x00261970
// 	public virtual void OnOpen()
// 	{
// 		this.IsOpen = true;
// 		for (int i = 0; i < this.children.Count; i++)
// 		{
// 			this.children[i].OnOpen();
// 		}
// 		if (this.ViewComponent != null)
// 		{
// 			if (this.ViewComponent.ForceHide)
// 			{
// 				this.ViewComponent.IsVisible = false;
// 				return;
// 			}
// 			if (!this.ViewComponent.IsVisible)
// 			{
// 				this.ViewComponent.OnOpen();
// 				this.ViewComponent.IsVisible = true;
// 			}
// 		}
// 	}

// 	// Token: 0x06005E3C RID: 24124 RVA: 0x002637F4 File Offset: 0x002619F4
// 	public virtual void OnClose()
// 	{
// 		this.IsOpen = false;
// 		for (int i = 0; i < this.children.Count; i++)
// 		{
// 			this.children[i].OnClose();
// 		}
// 		if (this.ViewComponent != null && this.ViewComponent.IsVisible)
// 		{
// 			this.ViewComponent.OnClose();
// 			this.ViewComponent.IsVisible = false;
// 		}
// 	}

// 	// Token: 0x06005E3D RID: 24125 RVA: 0x0026385B File Offset: 0x00261A5B
// 	public virtual void OnVisibilityChanged(bool _isVisible)
// 	{
// 		XUiEvent_OnVisibilityChanged onVisiblity = this.OnVisiblity;
// 		if (onVisiblity == null)
// 		{
// 			return;
// 		}
// 		onVisiblity(this, _isVisible);
// 	}

// 	// Token: 0x06005E3E RID: 24126 RVA: 0x0000FB42 File Offset: 0x0000DD42
// 	public virtual bool ParseAttribute(string _name, string _value, XUiController _parent)
// 	{
// 		return false;
// 	}

// 	// Token: 0x06005E3F RID: 24127 RVA: 0x00263870 File Offset: 0x00261A70
// 	public bool GetBindingValue(ref string _value, string _bindingName)
// 	{
// 		bool bindingValueInternal;
// 		try
// 		{
// 			bindingValueInternal = this.GetBindingValueInternal(ref _value, _bindingName);
// 		}
// 		catch
// 		{
// 			Log.Error("Unhandled exception in GetBindingValue. Binding name: " + _bindingName);
// 			throw;
// 		}
// 		return bindingValueInternal;
// 	}

// 	// Token: 0x06005E40 RID: 24128 RVA: 0x002638AC File Offset: 0x00261AAC
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public virtual bool GetBindingValueInternal(ref string _value, string _bindingName)
// 	{
// 		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
// 		if (num <= 2577987401U)
// 		{
// 			if (num <= 1573359332U)
// 			{
// 				if (num != 820250566U)
// 				{
// 					if (num != 1383400110U)
// 					{
// 						if (num == 1573359332U)
// 						{
// 							if (_bindingName == "is_playtesting")
// 							{
// 								_value = GameUtils.IsPlaytesting().ToString();
// 								return true;
// 							}
// 						}
// 					}
// 					else if (_bindingName == "is_world_loaded")
// 					{
// 						_value = (GameManager.Instance.World != null).ToString();
// 						return true;
// 					}
// 				}
// 				else if (_bindingName == "is_unityeditor")
// 				{
// 					_value = "false";
// 					return true;
// 				}
// 			}
// 			else if (num != 1899437583U)
// 			{
// 				if (num != 2161620009U)
// 				{
// 					if (num == 2577987401U)
// 					{
// 						if (_bindingName == "is_editmode")
// 						{
// 							_value = GameManager.Instance.IsEditMode().ToString();
// 							return true;
// 						}
// 					}
// 				}
// 				else if (_bindingName == "gamelanguage")
// 				{
// 					_value = Localization.language;
// 					return true;
// 				}
// 			}
// 			else if (_bindingName == "is_modal")
// 			{
// 				_value = this.WindowGroup.isModal.ToString();
// 				return true;
// 			}
// 		}
// 		else if (num <= 2934648263U)
// 		{
// 			if (num != 2745931225U)
// 			{
// 				if (num != 2759425756U)
// 				{
// 					if (num == 2934648263U)
// 					{
// 						if (_bindingName == "is_console")
// 						{
// 							_value = (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent().ToString();
// 							return true;
// 						}
// 					}
// 				}
// 				else if (_bindingName == "is_prefab_editor")
// 				{
// 					_value = PrefabEditModeManager.Instance.IsActive().ToString();
// 					return true;
// 				}
// 			}
// 			else if (_bindingName == "is_server")
// 			{
// 				_value = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer.ToString();
// 				return true;
// 			}
// 		}
// 		else if (num != 3445481009U)
// 		{
// 			if (num != 3556752319U)
// 			{
// 				if (num == 4180283068U)
// 				{
// 					if (_bindingName == "inputstyle")
// 					{
// 						this.RegisterForInputStyleChanges();
// 						_value = this.lastInputStyle.ToStringCached<PlayerInputManager.InputStyle>();
// 						return true;
// 					}
// 				}
// 			}
// 			else if (_bindingName == "is_creative")
// 			{
// 				_value = (GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) || GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled)).ToString();
// 				return true;
// 			}
// 		}
// 		else if (_bindingName == "is_controller_input")
// 		{
// 			this.RegisterForInputStyleChanges();
// 			_value = (this.lastInputStyle != PlayerInputManager.InputStyle.Keyboard).ToString();
// 			return true;
// 		}
// 		return false;
// 	}

// 	// Token: 0x06005E41 RID: 24129 RVA: 0x00263B64 File Offset: 0x00261D64
// 	public virtual void SetAllChildrenDirty(bool _includeViewComponents = false)
// 	{
// 		for (int i = 0; i < this.children.Count; i++)
// 		{
// 			this.children[i].SetAllChildrenDirty(false);
// 		}
// 		if (this.viewComponent != null)
// 		{
// 			this.viewComponent.IsDirty = true;
// 		}
// 		this.IsDirty = true;
// 	}

// 	// Token: 0x06005E42 RID: 24130 RVA: 0x00263BB4 File Offset: 0x00261DB4
// 	public virtual void RefreshBindingsSelfAndChildren()
// 	{
// 		for (int i = 0; i < this.children.Count; i++)
// 		{
// 			this.children[i].RefreshBindingsSelfAndChildren();
// 		}
// 		this.RefreshBindings(true);
// 	}

// 	// Token: 0x06005E43 RID: 24131 RVA: 0x00263BF0 File Offset: 0x00261DF0
// 	public void RefreshBindings(bool _forceAll = false)
// 	{
// 		for (int i = 0; i < this.BindingList.Count; i++)
// 		{
// 			this.BindingList[i].RefreshValue(_forceAll);
// 		}
// 	}

// 	// Token: 0x06005E44 RID: 24132 RVA: 0x00263C25 File Offset: 0x00261E25
// 	public void AddBinding(BindingInfo _info)
// 	{
// 		if (!this.BindingList.Contains(_info))
// 		{
// 			this.BindingList.Add(_info);
// 		}
// 	}

// 	// Token: 0x06005E45 RID: 24133 RVA: 0x0000FB42 File Offset: 0x0000DD42
// 	public virtual bool AlwaysUpdate()
// 	{
// 		return false;
// 	}

// 	// Token: 0x06005E46 RID: 24134 RVA: 0x00263C44 File Offset: 0x00261E44
// 	public virtual void Cleanup()
// 	{
// 		foreach (XUiController xuiController in this.children)
// 		{
// 			xuiController.Cleanup();
// 		}
// 		if (this.registeredForInputStyleChanges)
// 		{
// 			IPlatform nativePlatform = PlatformManager.NativePlatform;
// 			if (((nativePlatform != null) ? nativePlatform.Input : null) != null)
// 			{
// 				PlatformManager.NativePlatform.Input.OnLastInputStyleChanged -= this.OnLastInputStyleChanged;
// 			}
// 		}
// 		XUiView xuiView = this.ViewComponent;
// 		if (xuiView == null)
// 		{
// 			return;
// 		}
// 		xuiView.Cleanup();
// 	}

// 	// Token: 0x06005E47 RID: 24135 RVA: 0x00263CDC File Offset: 0x00261EDC
// 	public void FindNavigatableChildren(List<XUiView> views)
// 	{
// 		foreach (XUiController xuiController in this.children)
// 		{
// 			if (xuiController.viewComponent.IsNavigatable)
// 			{
// 				views.Add(xuiController.viewComponent);
// 			}
// 			xuiController.FindNavigatableChildren(views);
// 		}
// 	}

// 	// Token: 0x06005E48 RID: 24136 RVA: 0x00263D48 File Offset: 0x00261F48
// 	public bool TryFindFirstNavigableChild(out XUiView foundView)
// 	{
// 		foundView = null;
// 		foreach (XUiController xuiController in this.children)
// 		{
// 			if (xuiController.ViewComponent.IsNavigatable && xuiController.viewComponent.IsVisible && xuiController.viewComponent.UiTransform.gameObject.activeInHierarchy)
// 			{
// 				foundView = xuiController.viewComponent;
// 				return true;
// 			}
// 			if (xuiController.TryFindFirstNavigableChild(out foundView))
// 			{
// 				return true;
// 			}
// 		}
// 		return false;
// 	}

// 	// Token: 0x06005E49 RID: 24137 RVA: 0x00263DE4 File Offset: 0x00261FE4
// 	public bool SelectCursorElement(bool _withDelay = false, bool _overrideCursorMode = false)
// 	{
// 		if (this.ViewComponent == null)
// 		{
// 			return false;
// 		}
// 		if (this.xui.playerUI.CursorController.CursorModeActive && !_overrideCursorMode)
// 		{
// 			return false;
// 		}
// 		XUiView xuiView = this.ViewComponent;
// 		if (xuiView.IsNavigatable && xuiView.IsVisible)
// 		{
// 			if (_withDelay)
// 			{
// 				this.xui.playerUI.CursorController.SetNavigationTargetLater(xuiView);
// 			}
// 			else
// 			{
// 				this.xui.playerUI.CursorController.SetNavigationTarget(xuiView);
// 			}
// 			return true;
// 		}
// 		this.TryFindFirstNavigableChild(out xuiView);
// 		if (xuiView != null)
// 		{
// 			if (_withDelay)
// 			{
// 				this.xui.playerUI.CursorController.SetNavigationTargetLater(xuiView);
// 			}
// 			else
// 			{
// 				this.xui.playerUI.CursorController.SetNavigationTarget(xuiView);
// 			}
// 			return true;
// 		}
// 		return false;
// 	}

// 	// Token: 0x06005E4A RID: 24138 RVA: 0x00263EA1 File Offset: 0x002620A1
// 	public virtual void OnCursorSelected()
// 	{
// 		if (this.parent != null)
// 		{
// 			this.parent.OnCursorSelected();
// 		}
// 	}

// 	// Token: 0x06005E4B RID: 24139 RVA: 0x00263EB6 File Offset: 0x002620B6
// 	public virtual void OnCursorUnSelected()
// 	{
// 		if (this.parent != null)
// 		{
// 			this.parent.OnCursorUnSelected();
// 		}
// 	}

// 	// Token: 0x06005E4C RID: 24140 RVA: 0x00263ECB File Offset: 0x002620CB
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public void RegisterForInputStyleChanges()
// 	{
// 		if (!this.registeredForInputStyleChanges)
// 		{
// 			this.registeredForInputStyleChanges = true;
// 			PlatformManager.NativePlatform.Input.OnLastInputStyleChanged += this.OnLastInputStyleChanged;
// 		}
// 	}

// 	// Token: 0x06005E4D RID: 24141 RVA: 0x00263EF7 File Offset: 0x002620F7
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public void OnLastInputStyleChanged(PlayerInputManager.InputStyle _style)
// 	{
// 		this.curInputStyle = _style;
// 	}

// 	// Token: 0x04004709 RID: 18185
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public XUiView viewComponent;

// 	// Token: 0x0400470A RID: 18186
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public XUiController parent;

// 	// Token: 0x0400470B RID: 18187
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public readonly List<XUiController> children = new List<XUiController>();

// 	// Token: 0x0400470C RID: 18188
// 	[PublicizedFrom(EAccessModifier.Protected)]
// 	public XUiWindowGroup windowGroup;

// 	// Token: 0x0400470D RID: 18189
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public PlayerInputManager.InputStyle lastInputStyle = PlayerInputManager.InputStyle.Count;

// 	// Token: 0x0400470E RID: 18190
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public PlayerInputManager.InputStyle curInputStyle;

// 	// Token: 0x0400470F RID: 18191
// 	[PublicizedFrom(EAccessModifier.Private)]
// 	public bool registeredForInputStyleChanges;

// 	// Token: 0x04004710 RID: 18192
// 	public bool IsDirty;

// 	// Token: 0x04004711 RID: 18193
// 	public bool IsDormant;

// 	// Token: 0x04004712 RID: 18194
// 	public object CustomData;

// 	// Token: 0x0400471C RID: 18204
// 	public readonly Dictionary<string, string> CustomAttributes = new Dictionary<string, string>();

// 	// Token: 0x0400471D RID: 18205
// 	public readonly List<BindingInfo> BindingList = new List<BindingInfo>();
// }
