using Godot;
using System;

namespace Attack
{
	public partial class MaterialBall : Skill
	{
		public override string skillType { get; set; } = "projectile";
		public override string skillName { get; set; } = "材质球";

		public override string skillDescription { get; set; } = "对敌人造成40点伤害，并使敌人陷入【游戏渲染中】状态，每秒受到10点法术伤害，持续 5 秒，最多叠3层。";

        public override string skillQuote { get; set; } = "你这法线贴图方向都不对！连PBR流程都没走对，还好意思当BUG？";

		public override float ATK { get; set; } = 40;//攻击力

		public override float ATS { get; set; } = 0.5f;//攻速

		public override float RNG { get; set; } = 1;//攻击范围

		public override int AttackCount { get; set; } = 1;//弹道

		public override bool enableTarcking { get; set; } = false;//是否跟踪



		public override void Projectile(SkillData skillData, Godot.Vector2 startPosiition, Node BulletNode)//发射
		{
			skillData = NormalData.AddData(GetSkillData(), skillData);
			//GD.Print("发射");
			int count = skillData.AttackCount;
			//GD.Print(count);
			if (count % 2 == 0)
			{
				float size = NormalData.AttackLength / (count + 1);
				for (int i = 1; i <= count; i++)
				{
					PackedScene bulletScene = GD.Load<PackedScene>("res://trashbox/战斗系统/Attack/Skills/Projectile/材质球投射物.tscn");

					Node bulletNode = bulletScene.Instantiate();

					if (bulletNode is MaterialBallBullet bullet)
					{
						//int count = skillData.AttackCount;
						//GD.Print(count);
						//GD.Print("发射");
						float y = (NormalData.AttackLength / 2.0f) - (size * i);
						bullet.Initialize(skillData);
						BulletNode.AddChild(bullet);
						//bullet.GlobalPosition = startPosiition;
						Godot.Vector2 vector2 = new Vector2(startPosiition.X, startPosiition.Y + y);
						bullet.GlobalPosition = vector2;
						//GD.Print(i + " " + vector2);

					}

					
				}
			}
			else
			{
				//GD.Print("1");
				float spacing = NormalData.AttackLength / count;
				//GD.Print("2");
				for (int i = 0; i < count; i++)
				{
					PackedScene bulletScene = GD.Load<PackedScene>("res://trashbox/战斗系统/Attack/Skills/Projectile/材质球投射物.tscn");
					Node bulletNode = bulletScene.Instantiate();
					//GD.Print("3");
					if (bulletNode is MaterialBallBullet bullet)
					{
						//GD.Print("发射");
						bullet.Initialize(skillData);
						BulletNode.AddChild(bullet);

						// 计算位置：以中心为对称轴
						float y = (i - (count - 1) / 2.0f) * spacing;

						Godot.Vector2 vector2 = new Vector2(startPosiition.X, startPosiition.Y + y);
						bullet.GlobalPosition = vector2;
						//GD.Print((i + 1) + " " + vector2);
					}
				}
			}
		}
	}
}