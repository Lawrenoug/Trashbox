using Godot;
using System;

namespace Attack
{
	//[GlobalClass]
	public partial class Pixel : Skill
	{

		public override string skillType { get; set; } = "projectile";
		public override string skillName { get; set; } = "像素点";

		public override string skillDescription { get; set; } = "";

		public override string skillQuote { get; set; } = "";

		public override float ATK { get; set; } = 1;//攻击力

		public override float ATS { get; set; } = 1;//攻速

		public override float RNG { get; set; } = 1;//攻击范围

		public override int AttackCount { get; set; } = 1;//弹道

		public override bool enableTarcking { get; set; } = false;//是否跟踪



		public override void Projectile(SkillData skillData, Godot.Vector2 startPosiition, Node BulletNode)//发射
		{
			// 添加对BulletNode的有效性检查
			if (BulletNode == null || !GodotObject.IsInstanceValid(BulletNode) || BulletNode.IsQueuedForDeletion())
			{
				return;
			}
			
			skillData = NormalData.AddData(GetSkillData(), skillData);

			int count = skillData.AttackCount;
			//GD.Print(count);
			if (count % 2 == 0)
			{
				float size = NormalData.AttackLength / (count + 1);
				for (int i = 1; i <= count; i++)
				{
					PackedScene bulletScene = GD.Load<PackedScene>("res://trashbox/战斗系统/Attack/Skills/Prefab/投射物.tscn");

					Node bulletNode = bulletScene.Instantiate();

					if (bulletNode is PixelBullet bullet)
					{
						// 添加对BulletNode的有效性检查
						if (BulletNode == null || !GodotObject.IsInstanceValid(BulletNode) || BulletNode.IsQueuedForDeletion())
						{
							bullet.QueueFree();
							return;
						}
						
						//int count = skillData.AttackCount;
						//GD.Print(count);

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
				float spacing = NormalData.AttackLength / count;

				for (int i = 0; i < count; i++)
				{
					PackedScene bulletScene = GD.Load<PackedScene>("res://trashbox/战斗系统/Attack/Skills/Prefab/投射物.tscn");
					Node bulletNode = bulletScene.Instantiate();

					if (bulletNode is PixelBullet bullet)
					{
						// 添加对BulletNode的有效性检查
						if (BulletNode == null || !GodotObject.IsInstanceValid(BulletNode) || BulletNode.IsQueuedForDeletion())
						{
							bullet.QueueFree();
							return;
						}
						
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
