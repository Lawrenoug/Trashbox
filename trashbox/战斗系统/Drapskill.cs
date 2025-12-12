using CharacterManager;
using Godot;
using System;

namespace Attack
{
	public partial class Drapskill : TextureRect
	{
		public string groupName = "_skill_ui_group";

		public override void _Ready()
		{
			GuiInput += (@event) => on_gui_input(@event);
		}

		public override void _Process(double delta)
		{
			if(GetTree().GetFirstNodeInGroup(groupName) is Sprite2D item)
			{
				item.GlobalPosition = GetGlobalMousePosition();
			}
		}

		public void on_gui_input(InputEvent @event)
		{
			if(@event is InputEventMouseButton mb)
			{
				if (mb.Pressed && mb.ButtonIndex == MouseButton.Left)
				{
					// 1. 获取当前格子里的技能节点
					var currentItem = GetChildOrNull<Node>(0);
					
					// 2. 寻找 Engine 节点
					var engineNode = GetTree().GetFirstNodeInGroup("EngineUI");

					// --- 【修复1：点击显示文字描述】 ---
					if (currentItem != null && engineNode != null)
					{
						// 调用 engine.gd 里的函数显示文字
						engineNode.Call("preview_skill_instance", currentItem);
					}

					// --- 原有的拖拽/交换逻辑 ---
					var item = GetChildOrNull<Sprite2D>(0);
					var seltItem = GetTree().GetFirstNodeInGroup(groupName) as Sprite2D;
					
					if (seltItem == null)
					{
						if (GetChildCount() > 0 && item != null)
						{
							item.AddToGroup(groupName, true);
							item.ZIndex = 1;
						}
					}
					else
					{
						
						
						if (item == null)
						{
							seltItem.Reparent(this);
							seltItem.ZIndex = 0;
							seltItem.Position = new Godot.Vector2(16, 16);
							seltItem.RemoveFromGroup(groupName);

							// NodePath ControlnodePath=new NodePath("*/技能战斗列表");
							// var parent = GetNode(ControlnodePath) as Control;

							var parent =GetTree().GetFirstNodeInGroup("技能列表") as Control;
							//var parent = GetNode<Control>("/root/DesktopScreen/WindowBase/BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/LeftColumn/SkillLogPanel/技能战斗列表");
							var skillGroupsUIManager = parent.GetChild<SkillGroupsUIManager>(0);
							skillGroupsUIManager.RequestUpdate();

							// NodePath PlayManagerPath=new NodePath("*/Player");
							// var player=GetNode(PlayManagerPath) as PlayerManager;

							var player =GetTree().GetFirstNodeInGroup("player") as PlayerManager;
							//var player = GetNode<PlayerManager>("/root/DesktopScreen/WindowBase/BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/SplitSub/PreviewViewport/GameViewContainer/GameViewport/SkillPreviewStage/Player");
							player.attackManager.InsertSkill(skillGroupsUIManager.GetSkillList());

						}
						else
						{
							var seltItemParent = seltItem.GetParentOrNull<TextureRect>();
							seltItem.Reparent(this);
							seltItem.ZIndex = 0;
							seltItem.Position = new Godot.Vector2(16, 16);

							item.Reparent(seltItemParent);
							item.ZIndex = 0;
							item.Position = new Godot.Vector2(16, 16);

							seltItem.RemoveFromGroup(groupName);

							// NodePath ControlnodePath=new NodePath("*/技能战斗列表");
							// var parent = GetNode(ControlnodePath) as Control;

							var parent =GetTree().GetFirstNodeInGroup("技能列表") as Control;
							//var parent = GetNode<Control>("/root/DesktopScreen/WindowBase/BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/LeftColumn/SkillLogPanel/技能战斗列表");
							var skillGroupsUIManager = parent.GetChild<SkillGroupsUIManager>(0);
							skillGroupsUIManager.RequestUpdate();

							// NodePath PlayManagerPath=new NodePath("*/Player");
							// var player=GetNode(PlayManagerPath) as PlayerManager;

							var player =GetTree().GetFirstNodeInGroup("player") as PlayerManager;
							//var player = GetNode<PlayerManager>("/root/DesktopScreen/WindowBase/BgColor/MainLayout/ContentSlot/EditorRoot/SplitMain/SplitSub/PreviewViewport/GameViewContainer/GameViewport/SkillPreviewStage/Player");
							player.attackManager.InsertSkill(skillGroupsUIManager.GetSkillList());
						}
					}
				}
			} 
		}

		
	}
}
