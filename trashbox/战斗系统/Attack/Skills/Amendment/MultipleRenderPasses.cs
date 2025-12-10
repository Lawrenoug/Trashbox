using Godot;
using System;

namespace Attack
{
	public partial class MultipleRenderPasses : Skill
	{
		public override string skillType { get; set; } = "amendment";
		public override string skillName { get; set; } = "多通道渲染";

		public override string skillDescription { get; set; } = "施加的【游戏渲染中】状态的最大叠加层数提升 2 层,持续时间延长 3 秒";

        public override string skillQuote { get; set; } = "虽然不知道为啥，但多开几个渲染通道，BUG效果更显著了。";

		public override float ATK { get; set; } = 0;//攻击力

		public override float ATS { get; set; } = 0;//攻速

		public override float RNG { get; set; } = 0;//攻击范围

		public override int AttackCount { get; set; } = 0;//弹道

		public override bool enableTarcking { get; set; } = false;//是否跟踪
        public override string buffType { get;set; }="多通道渲染";
	}
}