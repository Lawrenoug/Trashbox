using Godot;
using System;

namespace Attack
{
	public partial class Pixel : Skill
	{
		public override string skillType { get; set; } = "projectile";
		public override string skillName { get; set; } = "像素点";
		
		public override float ATK { get; set; } = 0;//攻击力

        public override float ATS { get; set; } = 0;//攻速

        public override float RNG { get; set; } = 1;//攻击范围

        public override int AttackCount { get; set; } = 1;//弹道

        public override bool enableTarcking { get; set; } = false;//是否跟踪

		public override void Projectile(SkillData skillData)//发射
		{
			GD.Print("像素点攻击");
		}
		
	}
}