using Godot;
using System;
using System.Collections.Generic;

namespace Attack
{
	public partial class SkillGroupsUIManager : GridContainer
	{
		public List<Skill> skillUIDatas=new List<Skill>();

		[Signal]
		public delegate void UpdateSkillListRequestedEventHandler();
		public override void _Ready()
        {
            UpdateSkillListRequested += UpdateSkillList;
        }

		public override void _Process(double delta)
		{
		}

		public List<Skill> GetSkillList()
        {
			// var _skillList = new List<Skill>();
			// for (int i = 0; i < skillUIDatas.Count; i++)
			// {
			// 	_skillList.Add(skillUIDatas[0].skillDataUI.skill);
			// }
			return skillUIDatas;
        }
		public void UpdateSkillList()
		{
			//Godot.Collections.Array<Node> children = GetChildren();
			skillUIDatas.Clear();
			// 遍历子节点
			foreach (Node child in GetChildren())
			{
				//GD.Print($"子节点名称：{child.Name}，类型：{child.GetType().Name}");
				if (child.GetChildCount() > 0)
				{
					var skillUIData = child.GetChildOrNull<Skill>(0);
					if (skillUIData != null)
					{
						skillUIDatas.Add(skillUIData);

					}
				}
			}

			GD.Print("元素数量" + skillUIDatas.Count);
		}
		
		public void RequestUpdate()
		{
			EmitSignal(SignalName.UpdateSkillListRequested);
            

		}
	}
}