using Godot;
using System;

namespace Attack
{
	public partial class DynamicBinding : Skill
	{
		public override string skillType { get; set; } = "amendment";
		public override string skillName { get; set; } = "动态绑定";

		public override string skillDescription { get; set; } = "使你所有的投射物获得追踪效果，自动攻击距离最近的敌方单位";

        public override string skillQuote { get; set; } = "只要绑得够多，就没有打不中的敌人！";

		public override float ATK { get; set; } = 0;//攻击力

		public override float ATS { get; set; } = 0;//攻速

		public override float RNG { get; set; } = 0;//攻击范围

		public override int AttackCount { get; set; } = 0;//弹道

		public override bool enableTarcking { get; set; } = true;//是否跟踪
	}
}