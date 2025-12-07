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
					//GD.Print("左键");
					var item = GetChildOrNull<Sprite2D>(0);
					var seltItem = GetTree().GetFirstNodeInGroup(groupName) as Sprite2D;
					if (seltItem == null)
					{
						if (GetChildCount() > 0)
						{

							if (item != null)
							{
								//GD.Print("获取 null ！null");
								item.AddToGroup(groupName, true);
								item.ZIndex = 1;
							}
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

							var parent = GetNodeOrNull<Control>("/root/Node2D/技能战斗列表");
							if (parent != null)
							{
								var skillGroupsUIManager = parent.GetChild<SkillGroupsUIManager>(0);
								skillGroupsUIManager.RequestUpdate();

								var player = GetNodeOrNull<PlayerManager>("/root/Node2D/Player");
								if (player != null)
								{
									//player.attackManager.InsertSkill(skillGroupsUIManager.GetSkillList());
								}
							}
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

							var parent = GetNode<Control>("/root/Node2D/技能战斗列表");
							var skillGroupsUIManager = parent.GetChild<SkillGroupsUIManager>(0);
							skillGroupsUIManager.RequestUpdate();

							var player = GetNode<PlayerManager>("/root/Node2D/Player");
							//player.attackManager.InsertSkill(skillGroupsUIManager.GetSkillList());
						}
					}

					
				}
				
			} 
		}
	}
}
