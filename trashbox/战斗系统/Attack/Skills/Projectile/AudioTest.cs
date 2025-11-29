using Godot;
using System;

namespace Attack
{
	public partial class AudioTest : Skill
	{
		public override string skillType { get; set; } = "projectile";
		public override string skillName { get; set; } = "音频测试";

		public override string skillDescription { get; set; } = "对所有敌人造成25点伤害";

        public override string skillQuote { get; set; } = "测试音频，1，2，3……啊，好像音量开太大了。";

		public override float ATK { get; set; } = 25;//攻击力

		public override float ATS { get; set; } = 0.5f;//攻速

		public override float RNG { get; set; } = 1;//攻击范围

		public override int AttackCount { get; set; } = 1;//弹道

		public override bool enableTarcking { get; set; } = false;//是否跟踪



		public override void Projectile(SkillData skillData, Godot.Vector2 startPosiition, Node BulletNode)//发射
		{
			skillData = NormalData.AddData(GetSkillData(), skillData);
			
			// 获取所有敌人节点
			Godot.Collections.Array<Node> enemies = BulletNode.GetTree().GetNodesInGroup("enemy");
			
			// 对所有敌人造成伤害，重复AttackCount次
			for (int i = 0; i < skillData.AttackCount; i++)
			{
				foreach (Node enemyNode in enemies)
				{
					if (enemyNode is Enemy.EnemyBase enemy)
					{
						enemy.TakeDamage(skillData.ATK);
					}
				}
			}
		}
	}
}