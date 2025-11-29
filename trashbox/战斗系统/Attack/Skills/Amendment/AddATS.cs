using Godot;
using System;

namespace Attack
{

	public partial class AddATS : Skill
	{
		public override string skillType { get; set; } = "amendment";
		public override string skillName { get; set; } = "算法优化 - 时间复杂度降低";

		public override string skillDescription { get; set; } = "提升你投射物 20%的移动速度";

        public override string skillQuote { get; set; } = "我把省下来的内存都堆到伤害值里了，没想到吧？";

		public override float ATK { get; set; } = 0;//攻击力

		public override float ATS { get; set; } = 20f;//攻速

		public override float RNG { get; set; } = 0;//攻击范围

		public override int AttackCount { get; set; } = 1;//弹道

		public override bool enableTarcking { get; set; } = false;//是否跟踪
	}
}