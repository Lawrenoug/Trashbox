using Godot;
using System;
using Enemy;

namespace Attack
{

	public partial class RoomManager : Node
	{
		public int MaxRoomCount=8;//最大房间数量
		public int roomIndex=0;//房间索引
		public Node2D room;

		public bool isInAttack=false;
		private float attackDelay=0;
		private int flow=0;

		//private string UIpath="";
		private Control contion,contionParent;
		private RigidBody2D player;
		private Node2D playerBulletContainer;

		private Node2D playerBulletContainerParent,playerParent;
		

		public override void _Process(double delta)
		{
			// 加强对room对象的有效性检查
			if(room != null && GodotObject.IsInstanceValid(room) && !room.IsQueuedForDeletion())
			{
				if(isInAttack)
				{
					// 添加对room对象有效性的检查
					if(room.GetChildCount()==0&&flow==0)
					{
						attackDelay+=(float)delta;
						if (attackDelay > 2)
						{
							attackDelay = 0;
							if (roomIndex == 4 || roomIndex == 5)
							{
								EnterEliteRoom();
								flow = 1;
							}
							if (roomIndex == 7)
							{
								EnterBossRoom();
								flow = 1;
							}
						}
					}
				}
			}
		}

		public void startAttack()
		{
			contion=GetTree().GetFirstNodeInGroup("移动列表节点") as Control;
			contionParent=contion.GetParent() as Control;
			//UIpath=contion.GetParent().GetPath();
			contion.Reparent(GetTree().Root,true);

			player=GetTree().GetFirstNodeInGroup("player") as RigidBody2D;
			playerParent=player.GetParent() as Node2D;
			player.Reparent(GetTree().Root,true);

			playerBulletContainer=GetTree().GetFirstNodeInGroup("player") as Node2D;
			playerBulletContainerParent=playerBulletContainer.GetParent() as Node2D;
			playerBulletContainer.Reparent(GetTree().Root,true);

			
		}
		//进入房间
		public void EnterRoom(int _index)
		{

			room=GetTree().GetFirstNodeInGroup("战斗房间") as Node2D;

			var playerparent=GetTree().GetFirstNodeInGroup("战斗场景玩家父节点");
			player.Reparent(playerparent,true);
			playerBulletContainer.Reparent(playerparent,true);
			
			if(_index<MaxRoomCount)
			{
				//进入事件房
				if(_index==3||_index==6)
				{
					
				}
				else if(_index==7)//进入boss
				{
					EnterNormalRoom();
					isInAttack=true;
					flow=0;
				}
				else if(_index==4||_index==5)//精英怪
				{
					EnterNormalRoom();
					isInAttack=true;
					flow=0;
				}
				else if(_index==0||_index==1||_index==2)//小怪
				{
					EnterNormalRoom();
					isInAttack=true;
					flow=0;
				}
			}
		}
		//离开房间
		public void ExitRoom()
		{
			player.Reparent(GetTree().Root,true);
			playerBulletContainer.Reparent(GetTree().Root,true);

			// 添加对room对象的有效性检查
			if (room != null && GodotObject.IsInstanceValid(room) && !room.IsQueuedForDeletion())
			{
				// 创建子节点列表的副本以避免在迭代时修改集合
				var children = new Godot.Collections.Array<Node>(room.GetChildren());
				foreach (Node child in children)
				{
					if (child != null && GodotObject.IsInstanceValid(child) && !child.IsQueuedForDeletion())
					{
						room.RemoveChild(child);
						child.QueueFree();
					}
				}
			}
		}
		public void endAttack()
		{
			player.Reparent(playerParent,true);
			playerBulletContainer.Reparent(playerBulletContainerParent,true);
			contion.Reparent(contionParent,true);
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

		private void EnterNormalRoom()
		{
			// 修复：确保随机数范围不超过EnemySet的大小
			int _index = new Random().Next(0, EnemyTools.EnemySet.Count);
			PackedScene[] enemies = EnemyTools.GetNormalEnemy(_index);
			for(int i=0;i<enemies.Length;i++)
			{
				if(enemies[i] != null)
				{
					var enemyInstance = enemies[i].Instantiate() as EnemyBase;
					room.AddChild(enemyInstance);
					enemyInstance.GlobalPosition=EnemyTools.startPosition[i];
					enemyInstance.MoveTo(EnemyTools.endPosition[i]);
					enemyInstance.state=EnemyState.Moving;
				}
			}
		}
		private void EnterEliteRoom()
		{
			if (roomIndex == 4)
			{
				PackedScene enemy = EnemyTools.GetEliteEnemy(5);
				var enemyInstance = enemy.Instantiate() as EnemyBase;
				room.AddChild(enemyInstance);
				enemyInstance.GlobalPosition = EnemyTools.startPosition[0];
				enemyInstance.MoveTo(EnemyTools.endPosition[0]);
				enemyInstance.state = EnemyState.Moving;
			}
			else if (roomIndex == 5)
			{
				PackedScene enemy = EnemyTools.GetEliteEnemy(6);
				var enemyInstance = enemy.Instantiate() as EnemyBase;
				room.AddChild(enemyInstance);
				enemyInstance.GlobalPosition = EnemyTools.startPosition[0];
				enemyInstance.MoveTo(EnemyTools.endPosition[0]);
				enemyInstance.state = EnemyState.Moving;
			}
		}
		private void EnterBossRoom()
		{
			PackedScene enemy = EnemyTools.GetBossEnemy();
			var enemyInstance = enemy.Instantiate() as EnemyBase;
			room.AddChild(enemyInstance);
			enemyInstance.GlobalPosition = EnemyTools.startPosition[0];
				enemyInstance.MoveTo(EnemyTools.endPosition[0]);
				enemyInstance.state = EnemyState.Moving;
		}
	}
}
