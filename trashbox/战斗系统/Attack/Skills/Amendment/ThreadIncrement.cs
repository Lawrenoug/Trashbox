using Godot;
using System;

namespace Attack
{
	public partial class ThreadIncrement : Skill
	{
		public override string skillType { get; set; } = "amendment";
		public override string skillName { get; set; } = "线程增加";

		public override string skillDescription { get; set; } = "";

        public override string skillQuote { get; set; } = "";

		public override float ATK { get; set; } = 0;//攻击力

		public override float ATS { get; set; } = 0;//攻速

		public override float RNG { get; set; } = 0;//攻击范围

		public override int AttackCount { get; set; } = 1;//弹道

		public override bool enableTarcking { get; set; } = false;//是否跟踪


	}
}