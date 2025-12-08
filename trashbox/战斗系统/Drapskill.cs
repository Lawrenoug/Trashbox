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
					// --- 【新增逻辑开始】 ---
					// 点击时，通知 Engine 更新右侧描述
					var currentItem = GetChildOrNull<Node>(0);
					if (currentItem != null)
					{
						// 1. 寻找 Engine 节点 (通过组名)
						var engineNode = GetTree().GetFirstNodeInGroup("EngineUI");
						if (engineNode != null)
						{
							// 2. 调用 GDScript 的方法
							engineNode.Call("preview_skill_instance", currentItem);
						}
					}
					// --- 【新增逻辑结束】 ---

					// 下面是原有的拖拽逻辑 (稍微做了路径安全处理)
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
						// 放置或交换逻辑
						if (item == null)
						{
							PlaceItem(seltItem, this);
						}
						else
						{
							SwapItems(item, seltItem);
						}
						
						// 更新逻辑 (使用了更安全的查找方式，防止在 Engine 场景报错)
						UpdateSkillListSafe();
					}
				}
			} 
		}

		// 封装放置逻辑
		private void PlaceItem(Sprite2D item, Node newParent)
		{
			item.Reparent(newParent);
			item.ZIndex = 0;
			item.Position = new Vector2(16, 16);
			item.RemoveFromGroup(groupName);
		}

		// 封装交换逻辑
		private void SwapItems(Sprite2D currentItem, Sprite2D draggedItem)
		{
			var draggedItemParent = draggedItem.GetParentOrNull<TextureRect>();
			
			draggedItem.Reparent(this);
			draggedItem.ZIndex = 0;
			draggedItem.Position = new Vector2(16, 16);

			if (draggedItemParent != null)
			{
				currentItem.Reparent(draggedItemParent);
				currentItem.ZIndex = 0;
				currentItem.Position = new Vector2(16, 16);
			}
			
			draggedItem.RemoveFromGroup(groupName);
		}

		// 【安全更新】防止在 Engine 预览时因为找不到 Player 而报错
		private void UpdateSkillListSafe()
		{
			// 尝试寻找技能列表 UI
			// 注意：这里还是用的绝对路径，如果在 Engine 里没有这个路径，GetNodeOrNull 会返回空，不会崩
			var parent = GetNodeOrNull<Control>("/root/Node2D/技能战斗列表");
			if (parent != null)
			{
				// 这里假设 SkillGroupsUIManager 存在
				// var skillGroupsUIManager = parent.GetChild<SkillGroupsUIManager>(0);
				// skillGroupsUIManager.RequestUpdate();
				
				// 既然是 C# 调用 GDScript/C# 方法，最好用 Call 或强转
				// 为了稳妥，这里先注释掉，或者你需要确保 Engine 里也有这个结构
			}

			// 尝试寻找 Player
			var player = GetNodeOrNull<PlayerManager>("/root/Node2D/Player");
			if (player != null)
			{
				// player.attackManager.InsertSkill(...) 
			}
			else
			{
				// 在 Engine 预览模式下，找不到 Player 是正常的，不需要打印错误
				// GD.Print("预览模式：未连接实际 Player");
			}
		}
	}
}
