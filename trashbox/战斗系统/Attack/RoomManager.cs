using Godot;
using System;

namespace Attack
{

	public partial class RoomManager : Node
	{
		public int MaxRoomCount=8;//最大房间数量
		public int roomIndex=0;//房间索引


		//进入房间
		public void EnterRoom(int _index)
		{
			if(_index<MaxRoomCount)
			{
				//进入事件房
				if(_index==3||_index==6)
				{
					
				}
				else if(_index==7)//进入boss
				{
					
				}
				else if(_index==4||_index==5)//精英怪
				{
					
				}
				else if(_index==0||_index==1||_index==2)
				{
					
				}
			}
		}
		//离开房间
		public void ExitRoom()
		{
			
		}
		//胜利随机得到技能
		public PackedScene[] GetSkills(int _count)
		{
			PackedScene[] skills=new PackedScene[_count];
			for(int i=0;i<skills.Length; i++)
			{
				skills[i]=AttackTools.GetSkill();
			}
			return skills;
		}
		//传输技能到背包
		public void SendSkillToBackpack(PackedScene[] _skills)
		{
			var node=GetTree().GetFirstNodeInGroup("技能背包").FindChild("技能组背包");
			int skillIndex = 0;
			foreach(var child in node.GetChildren())
			{
				// 检查child是否有子节点
				if (((Node)child).GetChildCount() == 0 && skillIndex < _skills.Length)
				{
					// 如果没有子节点且还有技能可以添加，则添加一个技能作为子节点
					Node skillNode = _skills[skillIndex].Instantiate<Node>();
					((Node)child).AddChild(skillNode);
					skillIndex++;
				}
			}
		}



	}
}
