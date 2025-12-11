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
				else//进入战斗房间
				{
					
				}
			}
		}
		//离开房间
		public void ExitRoom()
		{
			
		}
		//胜利随机得到技能
		public Skill[] GetSkills(int _index)
		{
			return null;
		}
		//传输技能到背包
		public void SendSkillToBackpack(Skill[] _skills)
		{
			
		}

	}
}
