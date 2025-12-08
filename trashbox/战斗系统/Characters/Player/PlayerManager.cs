using Godot;
using System;
using Attack;

namespace CharacterManager
{
	public partial class PlayerManager : RigidBody2D
	{
		[Export]
		public Node2D startShootPosition;
		[Export]
		public Control UIControl;
		private float blood = 100;
		private float speed = 300;
		private float SpeedCutTime=3;
		public AttackManager attackManager;
		private SkillGroupsUIManager skillGroupsUIManager;

		public override void _Ready()
		{
			skillGroupsUIManager=UIControl.GetChildOrNull<SkillGroupsUIManager>(0);
			var node = GetNode<Node>("/root/Node2D/玩家子弹");
			if (node != null)
			{
				attackManager = new AttackManager(node);
			}
			else
            {
				GD.Print("没找到");
            }
        }

		public override void _PhysicsProcess(double delta)
		{
			base._PhysicsProcess(delta);
			Move(delta);
			if(attackManager!=null)
			{
				attackManager.AttackLoop((float)delta,startShootPosition.GlobalPosition);
                
            }
			if(blood<=0)
			{
				GD.Print("玩家死亡");
			}
			if(speed>=0)
			{
				SpeedCutTime--;
			}
			else
            {
                SpeedRestore();
            }
		}

		private void Move(double delta)
		{
			if(GlobalPosition.X<=0)
            {
				GlobalPosition=new Vector2(0,GlobalPosition.Y);
            }
			if(GlobalPosition.X>=GetViewportRect().Size.X)
			{
				GlobalPosition=new Vector2(GetViewportRect().Size.X,GlobalPosition.Y);
			}
			if(GlobalPosition.Y<=0)
			{
				GlobalPosition=new Vector2(GlobalPosition.X,0);
			}
			if(GlobalPosition.Y>=GetViewportRect().Size.Y)
			{
				GlobalPosition=new Vector2(GlobalPosition.X,GetViewportRect().Size.Y);
			}
			Vector2 inputDirection = Input.GetVector("左", "右", "上", "下");
			var Velocity = inputDirection * speed;
			Position += Velocity * (float)delta;
		}
		
		public void Damage(float damage)
		{
			blood -= damage;
			GD.Print($"玩家受到{damage}点伤害，当前血量：{blood}");
		}
		public float GetBlood()
		{
			return blood;
		}

		public void SpeedCut()
        {
            speed *= 0.5f;
			SpeedCutTime = 3;
        }

		public void SpeedRestore()
        {
            speed *= 2f;
        }
		
	}
}