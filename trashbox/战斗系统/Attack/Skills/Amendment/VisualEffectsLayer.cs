using Godot;
using System;

namespace Attack
{
	public partial class VisualEffectsLayer : Skill
	{
		public override string skillType { get; set; } = "amendment";
		public override string skillName { get; set; } = "视觉特效图层";

		public override string skillDescription { get; set; } = "施加的【游戏渲染中】状态的伤害提升20%";

        public override string skillQuote { get; set; } = "管他什么美术风格统一，特效叠得越多，伤害越高！";

		public override float ATK { get; set; } = 0;//攻击力

		public override float ATS { get; set; } = 0;//攻速

		public override float RNG { get; set; } = 0;//攻击范围

		public override int AttackCount { get; set; } = 0;//弹道

		public override bool enableTarcking { get; set; } = false;//是否跟踪
        public override string buffType { get;set; }="多通道渲染";
	}
}