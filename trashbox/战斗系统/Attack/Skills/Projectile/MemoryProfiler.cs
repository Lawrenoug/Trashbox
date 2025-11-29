using Godot;
using System;

namespace Attack
{
	public partial class MemoryProfiler : Skill
	{
		public override string skillType { get; set; } = "projectile";
		public override string skillName { get; set; } = "内存剖析器";

		public override string skillDescription { get; set; } = "对敌方单位造成30点伤害，并附加【内存弱点】持续3秒，附加【内存弱点】的敌方单位受到的伤害提高10%，可叠加3层。";

        public override string skillQuote { get; set; } = "找到你的内存泄漏了，现在，让我们把它变成伤害泄漏。";

		public override float ATK { get; set; } = 30;//攻击力

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
					PackedScene bulletScene = GD.Load<PackedScene>("res://trashbox/战斗系统/Attack/Skills/Projectile/帧调试器投射物.tscn");

					Node bulletNode = bulletScene.Instantiate();

					if (bulletNode is FrameDebuggerBullet bullet)
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
					PackedScene bulletScene = GD.Load<PackedScene>("res://trashbox/战斗系统/Attack/Skills/Projectile/帧调试器投射物.tscn");
					Node bulletNode = bulletScene.Instantiate();
					//GD.Print("3");
					if (bulletNode is FrameDebuggerBullet bullet)
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