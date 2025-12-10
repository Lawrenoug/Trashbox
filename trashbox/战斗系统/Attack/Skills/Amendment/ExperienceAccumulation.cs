using Godot;
using System;


namespace Attack
{
	public partial class ExperienceAccumulation : Skill
	{
		public override string skillType { get; set; } = "amendment";
		public override string skillName { get; set; } = "经验积累";

		public override string skillDescription { get; set; } = "击败带有负面状态的敌人时，你获得一层【经验】效果，每层使你攻击速度提升 2%";

        public override string skillQuote { get; set; } = "这个BUG我见过，上次也是这么修的。";

		public override float ATK { get; set; } = 0;//攻击力

		public override float ATS { get; set; } = 0;//攻速

		public override float RNG { get; set; } = 0;//攻击范围

		public override int AttackCount { get; set; } = 0;//弹道

		public override bool enableTarcking { get; set; } = false;//是否跟踪

        public override string buffType { get ; set ; }="经验积累";
	}
}