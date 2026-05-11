// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
<<<<<<< HEAD
using Polytoria.Creator.Settings;
=======
//using Polytoria.Creator.Settings;
>>>>>>> a4efa7a (Replace UI with new interface)
using Polytoria.Creator.UI.TextEditor;
using Polytoria.Datamodel;
using Polytoria.Datamodel.Creator;
using Polytoria.Shared;
using System;
using System.Collections.Generic;

namespace Polytoria.Creator.UI;

public sealed partial class Tabs : Control
{
	private int _currentIdx, _selectedIdx;
	
	private readonly List<Control> _orderedControls = [];
	private readonly Dictionary<Control, int> _controlToIdx = [];
	private readonly Dictionary<int, Control> _idxToControl = [];
	
	private readonly Dictionary<string, TextEditorContainer> _openedScripts = [];
		
	private TabBar _tabBar = null!;
	
	private PanelContainer _tabsContainer = null!;
	
	private Button _leftButton = null!, _rightButton = null!;
	private bool _scrollLeft, _scrollRight;
	
	public static Tabs Singleton { get; private set; } = null!;
	public Tabs()
	{
		Singleton = this;
	}
	
	public override void _Ready()
	{
		_tabBar = GetNode<TabBar>("Bar/TabsClip/TabBar");
		_tabsContainer = GetNode<PanelContainer>("Container");
		
		_leftButton = GetNode<Button>("Bar/LeftButton");
		_rightButton = GetNode<Button>("Bar/RightButton");
		
		_tabBar.TabCloseDisplayPolicy = TabBar.CloseButtonDisplayPolicy.ShowAlways;
		_tabBar.TabSelected += idx => _selectedIdx = (int)idx;
		_tabBar.TabChanged += idx =>
		{
			SetCurrentTab((int)idx);
			World? game = null;

			if (idx != -1)
			{
				Control control = _idxToControl[(int)idx];

				if (control is WorldContainer gameContainer)
				{
					game = gameContainer.World;
				}
				if (control is TextEditorContainer textedit)
				{
					game = World.Current;
				}
			}
			
			World.Current = game;
		};
		
		_tabBar.ActiveTabRearranged += newIdx =>
		{
			var control = _orderedControls[(int)_selectedIdx];
			_orderedControls.RemoveAt((int)_selectedIdx);
			_orderedControls.Insert((int)newIdx, control);
			RebuildLookups();
			_selectedIdx = -1;
		};
		
		_tabBar.TabClosePressed += async idx =>
		{
			var control = _orderedControls[(int)idx];
			if (control is WorldContainer || control is TextEditorContainer)
			{
				if (!(control is TextEditorContainer txt && txt.EditorRoot.Saved))
				{
<<<<<<< HEAD
					if (!await CreatorService.Interface.PromptConfirmation("Are you sure you want to close this tab? Any unsaved changes will be lost.", dismissKey: CreatorSettingKeys.Popups.CloseTabWarning)) return;
=======
					//if (!await CreatorService.Interface.PromptConfirmation("Are you sure you want to close this tab? Any unsaved changes will be lost.", dismissKey: CreatorSettingKeys.Popups.CloseTabWarning)) return;
>>>>>>> a4efa7a (Replace UI with new interface)
				}
			}

			if (control is WorldContainer g)
			{
				g.World.ForceDelete();
			}
			else if (control is TextEditorContainer tec)
			{
				_openedScripts.Remove(tec.TargetFilePathAbsolute);
			}
			Remove((int)idx);
			control.QueueFree();
		};
		
		_leftButton.ButtonDown += () => _scrollLeft = true;
		_leftButton.ButtonUp += () => _scrollLeft = false;
		_rightButton.ButtonDown += () => _scrollRight = true;
		_rightButton.ButtonUp += () => _scrollRight = false;
	}
	
	private void Remove(int idx)
	{
		_orderedControls.RemoveAt((int)idx);
		_tabBar.RemoveTab((int)idx);
		RebuildLookups();
		
		_tabBar.Size = new Vector2(0, _tabBar.Size.Y);
		_tabBar.Position = new Vector2(Mathf.Clamp(_tabBar.Position.X, -Mathf.Max(_tabBar.Size.X - this.Size.X + 96, 0), 0), _tabBar.Position.Y);
		SetCurrentTab(_tabBar.CurrentTab);
	}
	
	private void RebuildLookups()
	{
		_controlToIdx.Clear();
		_idxToControl.Clear();
		for (int i = 0; i < _tabBar.TabCount; i++)
		{
			var c = _orderedControls[i];
			_idxToControl[i] = c;
			_controlToIdx[c] = i;
		}
	}
	
	public void CloseTabsOfSession(CreatorSession session)
	{
		foreach ((string k, TextEditorContainer c) in _openedScripts)
		{
			if (c.TargetSession == session)
			{
				_openedScripts.Remove(k);
				c.QueueFree();
			}
		}
	}
	
	public void SetCurrentTab(int idx)
	{
		_idxToControl[_currentIdx].Visible = false;
		
		_currentIdx = idx;
		_tabBar.CurrentTab = idx;
		_idxToControl[_currentIdx].Visible = true;
	}
	
	public void SetTabTitle(Control c, string to)
	{
		_tabBar.SetTabTitle(_controlToIdx[c], to);
	}
	
	public void Insert(TabData other, string? title = null)
	{
		Control container;
		string icon;

		if (other is GameTab gt)
		{
			container = new WorldContainer(gt.World);
			icon = "World";

			void deleted()
			{
				gt.World.Deleted -= deleted;
				if (IsInstanceValid(container))
					container.QueueFree();
			}

			gt.World.Deleted += deleted;
		}
		else if (other is TextEditorTab txt)
		{
			string fullPath = txt.Session.GlobalizePath(txt.TargetPath);
			if (_openedScripts.TryGetValue(fullPath, out TextEditorContainer? existingTec))
			{
				SetCurrentTab(_controlToIdx[existingTec]);
				return;
			}
			TextEditorContainer tec = new(txt.TargetPath, fullPath, txt.Session) { OriginTabName = txt.Title ?? "" };
			container = tec;
			ScriptTypeEnum st = CreatorService.GetScriptTypeFromPath(txt.TargetPath);
			switch (st)
			{
				case (ScriptTypeEnum.Module):
					icon = "ModuleScript";
					break;
				case (ScriptTypeEnum.Server):
					icon = "ServerScript";
					break;
				case (ScriptTypeEnum.Client):
					icon = "ClientScript";
					break;
				default:
					icon = "Script";
					break;
			}
			_openedScripts[fullPath] = tec;
		}
		else
		{
			throw new NotImplementedException();
		}
		int idx = _tabBar.TabCount;
		_orderedControls.Add(container);
		_idxToControl[idx] = container;
		_controlToIdx[container] = idx;

		_tabsContainer.AddChild(container, true);
		_tabBar.AddTab(title ?? other.Title, Globals.LoadIcon(icon));
		SetCurrentTab(idx);
	}
	
	public class TextEditorTab : TabData
	{
		public string TargetPath = null!;
		public CreatorSession Session = null!;
	}

	public class GameTab : TabData
	{
		public World World = null!;
	}

	public class TabData
	{
		public string Title = "Tab";
	}
	
	public override void _Process(double delta)
	{
		if (_scrollLeft) SlideBar((float)(640 * delta));
		if (_scrollRight) SlideBar((float)(-640 * delta));
	}
	
	private void SlideBar(float delta)
	{
		var pos = _tabBar.Position;
		pos.X = Mathf.Clamp(pos.X + delta, -Mathf.Max(_tabBar.Size.X - this.Size.X + 96, 0), 0);
		_tabBar.Position = pos;
	}
}
