using Godot;
using System;
using Enemy;
using CharacterManager;

namespace Attack
{

	public partial class RoomManager : Node
	{
		public int MaxRoomCount=8;//最大房间数量
		public int roomIndex=0;//房间索引
		public Node2D room;

		public bool isInAttack=false;
		private float attackDelay=0;
		private int flow=1;

		//private string UIpath="";
		private Control contion,contionParent;
		private PlayerManager player;
		private Node2D playerBulletContainer;

		private Node2D playerBulletContainerParent,playerParent;
		private string playerBulletContainerParentPath,playerParentPath,contionParentPath;
		

		public override void _Process(double delta)
		{
			// 加强对room对象的有效性检查
			if(room != null && GodotObject.IsInstanceValid(room) && !room.IsQueuedForDeletion())
			{
				if(isInAttack)
				{
					if(room.GetChildCount()==0)
					{
						GD.Print("空");
						if(flow==0&&(roomIndex==0||roomIndex==1||roomIndex==2))
						{
							GD.Print("结束普通房间战斗");
							isInAttack=false;
							
							// 直接调用LevelBase的_on_skip_level_pressed方法
							CallLevelBaseSkipMethod();
						}
						// 添加对room对象有效性的检查
						else if(flow==0&&(roomIndex==4||roomIndex==5||roomIndex==7))
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
						else if(flow==1&&(roomIndex==4||roomIndex==5||roomIndex==7))
						{
							GD.Print("结束精英/Boss房间战斗");
							isInAttack=false;
							
							// 直接调用LevelBase的_on_skip_level_pressed方法
							CallLevelBaseSkipMethod();
						}
					}
				}
			}
			else
			{
				//GD.Print("111");
			}
		}
		
		// 自动触发关卡跳过效果
		private void AutoSkipLevel()
		{
			GD.Print("自动触发关卡跳过效果");
			
			
			// 调用endAttack清理战斗状态
			endAttack();
			
			
		}
		
		// 调用LevelBase中的_on_skip_level_pressed方法
		private void CallLevelBaseSkipMethod()
		{
			GD.Print("直接调用LevelBase的_on_skip_level_pressed方法");
			
			// 获取当前场景树的根节点
			var root = GetTree().Root;
			
			// 查找LevelBase场景节点（通常是当前场景）
			Node levelBase = GetTree().GetFirstNodeInGroup("战斗房间");
			
			
			// 如果找到了LevelBase节点，则调用其_on_skip_level_pressed方法
			if (levelBase != null && levelBase.HasMethod("_on_skip_level_pressed"))
			{
				levelBase.Call("_on_skip_level_pressed");
			}
			else
			{
				GD.PrintErr("找不到LevelBase节点或_on_skip_level_pressed方法");
			}
		}
		
		// 调用LevelBase中的_on_skip_level_pressed方法
		private void CallSkipLevel()
		{
			GD.Print("调用LevelBase的_on_skip_level_pressed方法");
			
			// 获取当前场景树的根节点
			var root = GetTree().Root;
			
			// 查找LevelBase场景节点（通常是当前场景）
			Node levelBase = GetTree().GetFirstNodeInGroup("战斗房间") as Node;
			
			
			// 如果找到了LevelBase节点，则调用其_on_skip_level_pressed方法
			if (levelBase != null && levelBase.HasMethod("_on_skip_level_pressed"))
			{
				levelBase.Call("_on_skip_level_pressed");
			}
			else
			{
				// 如果找不到LevelBase节点，则使用原来的自动跳关逻辑
				GD.Print("找不到LevelBase节点，使用备用自动跳关逻辑");
				AutoSkipLevel();
			}
		}
		
		public void startAttack()
		{
			contion=GetTree().GetFirstNodeInGroup("移动列表节点") as Control;
			//GD.Print(contion.GetPath());
			contionParent=contion.GetParent() as Control;
			//UIpath=contion.GetParent().GetPath();
			contion.Reparent(GetTree().Root,true);
			//GD.Print("contion修改后"+contion.GetPath());
			contion.Visible=false;

			player=GetTree().GetFirstNodeInGroup("player") as PlayerManager;
			//GD.Print(player.GetPath());
			playerParent=player.GetParent() as Node2D;
			player.Reparent(GetTree().Root,true);
			//GD.Print("player修改后1"+player.GetPath());


			playerBulletContainer=GetTree().GetFirstNodeInGroup("玩家子弹") as Node2D;
			//GD.Print(playerBulletContainer.GetPath());
			playerBulletContainerParent=playerBulletContainer.GetParent() as Node2D;
			playerBulletContainer.Reparent(GetTree().Root,true);
			//GD.Print("playerBulletContainer修改后1"+playerBulletContainer.GetPath());

			GD.Print("playerParent:"+playerParent.GetPath()+" playerBulletContainerParent:"+playerBulletContainerParent.GetPath());
			playerParentPath=playerParent.GetPath();
			playerBulletContainerParentPath=playerBulletContainerParent.GetPath();
			contionParentPath=contionParent.GetPath();
			
		}
		//进入房间
		public void EnterRoom(int _index)
		{

			room=GetTree().GetFirstNodeInGroup("战斗房间怪物节点") as Node2D;

			var playerparent=GetTree().GetFirstNodeInGroup("战斗场景玩家父节点");
			player.Reparent(playerparent,true);
			GD.Print("player修改后2"+player.GetPath());
			playerBulletContainer.Reparent(playerparent,true);
			GD.Print("playerBulletContainer修改后2"+playerBulletContainer.GetPath());
			
			if(_index<MaxRoomCount)
			{
				//进入事件房
				if(_index==3||_index==6)
				{
					
				}
				else if(_index==7)//进入boss
				{
					EnterBossRoom();
					isInAttack=true;
					flow=0;
				}
				else if(_index==4||_index==5)//精英怪
				{
					EnterEliteRoom();
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
			//GD.Print("playerParent:"+playerParent.GetPath()+" playerBulletContainerParent:"+playerBulletContainerParent.GetPath());
			player.Reparent(GetTree().Root,true);
			playerBulletContainer.Reparent(GetTree().Root,true);
			contion.Reparent(GetTree().Root,true);

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
			// contion=GetTree().GetFirstNodeInGroup("移动列表节点") as Control;
			// playerBulletContainer=GetTree().GetFirstNodeInGroup("玩家子弹") as Node2D;
			// player=GetTree().GetFirstNodeInGroup("player") as RigidBody2D;

			//GD.Print("contion:"+contion.GetPath()+" player:"+player.GetPath()+" playerBulletContainer:"+playerBulletContainer.GetPath());
			//GD.Print("playerParent:"+playerParent.GetPath()+" playerBulletContainerParent:"+playerBulletContainerParent.GetPath());

			//contion=GetTree().GetFirstNodeInGroup("移动列表节点") as Control;
			Godot.Collections.Array<Node> contionNodes = GetTree().GetNodesInGroup("移动列表节点");
			if(contionNodes.Count==2)
			{
				contionNodes[1].QueueFree();
			}
			Godot.Collections.Array<Node> playerNodes = GetTree().GetNodesInGroup("player");
			if(playerNodes.Count==2)
			{
				playerNodes[1].QueueFree();
			}
			Godot.Collections.Array<Node> playerBulletContainerNodes = GetTree().GetNodesInGroup("玩家子弹");
			if(playerBulletContainerNodes.Count==2)
			{
				playerBulletContainerNodes[1].QueueFree();
			}

			player=playerNodes[0] as PlayerManager;
			contion=contionNodes[0] as Control;
			playerBulletContainer=playerBulletContainerNodes[0] as Node2D;

			// 添加对节点路径的有效性检查
			if (!string.IsNullOrEmpty(playerParentPath) && !string.IsNullOrEmpty(playerBulletContainerParentPath) && !string.IsNullOrEmpty(contionParentPath))
			{
				GD.Print("尝试获取父节点: playerParentPath=", playerParentPath, " playerBulletContainerParentPath=", playerBulletContainerParentPath, " contionParentPath=", contionParentPath);
				
				playerParent=GetTree().GetFirstNodeInGroup("玩家节点") as Node2D;
				playerBulletContainerParent=GetTree().GetFirstNodeInGroup("玩家节点") as Node2D;
				
				contionParent=GetNodeOrNull<Control>(contionParentPath);
				
				// 添加对获取到的父节点的有效性检查
				GD.Print("节点获取结果: player=", player != null, " playerParent=", playerParent != null, " playerBulletContainer=", playerBulletContainer != null, " playerBulletContainerParent=", playerBulletContainerParent != null, " contion=", contion != null, " contionParent=", contionParent != null);
				
				if (player != null && GodotObject.IsInstanceValid(player) && !player.IsQueuedForDeletion() &&
					playerParent != null && GodotObject.IsInstanceValid(playerParent) && !playerParent.IsQueuedForDeletion())
				{
					GD.Print("Reparent player to playerParent");
					player.Reparent(playerParent,true);
				}
				else
				{
					GD.Print("player or playerParent is invalid: player=", player != null, " playerParent=", playerParent != null);
				}
				
				if (playerBulletContainer != null && GodotObject.IsInstanceValid(playerBulletContainer) && !playerBulletContainer.IsQueuedForDeletion() &&
					playerBulletContainerParent != null && GodotObject.IsInstanceValid(playerBulletContainerParent) && !playerBulletContainerParent.IsQueuedForDeletion())
				{
					GD.Print("Reparent playerBulletContainer to playerBulletContainerParent");
					playerBulletContainer.Reparent(playerBulletContainerParent,true);
				}
				else
				{
					GD.Print("playerBulletContainer or playerBulletContainerParent is invalid: playerBulletContainer=", playerBulletContainer != null, " playerBulletContainerParent=", playerBulletContainerParent != null);
				}
				
				if (contion != null && GodotObject.IsInstanceValid(contion) && !contion.IsQueuedForDeletion() &&
					contionParent != null && GodotObject.IsInstanceValid(contionParent) && !contionParent.IsQueuedForDeletion())
				{
					// 先将contion从当前父节点中移除（如果有的话）
					if (contion.GetParent() != null)
					{
						GD.Print("从当前父节点移除contion");
						contion.GetParent().RemoveChild(contion);
					}
					
					// 将contion添加为contionParent的第一个子节点
					GD.Print("添加contion到contionParent的第一个位置");
					contionParent.AddChild(contion);
					contionParent.MoveChild(contion, 0);
					
					contion.Visible=true;
				}
				else
				{
					GD.Print("contion or contionParent is invalid: contion=", contion != null, " contionParent=", contionParent != null);
				}
			}
			else
			{
				GD.PrintErr("无法获取有效的父节点路径，跳过reparent操作: playerParentPath=", playerParentPath, " playerBulletContainerParentPath=", playerBulletContainerParentPath, " contionParentPath=", contionParentPath);
			}
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
			var node=GetTree().GetFirstNodeInGroup("技能背包").GetChildren()[0];
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
			GD.Print("进入精英怪");
			// 注意：这个方法只是生成精英敌人，不会直接触发场景跳转
			// 场景跳转是在_Process方法中检测到房间清空且flow==1时触发的
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
