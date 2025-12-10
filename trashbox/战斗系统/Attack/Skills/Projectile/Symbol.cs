using Godot;
using System;

namespace Attack
{
	public partial class Symbol : Skill
	{
		public override string skillType { get; set; } = "projectile";
		public override string skillName { get; set; } = "字符";

		public override string skillDescription { get; set; } = "发射一枚随机字符，对敌人造成10点伤害";

		public override string skillQuote { get; set; } = "//TODO: 这里以后要换个酷炫的特效";

		public override float ATK { get; set; } = 10;//攻击力

		public override float ATS { get; set; } = 1;//攻速

		public override float RNG { get; set; } = 1;//攻击范围

		public override int AttackCount { get; set; } = 1;//弹道

		public override bool enableTarcking { get; set; } = false;//是否跟踪

		private Random random = new Random();

		private string[] basicChars = new string[] {
	// 数字
	"0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
	
	// 大写字母
	"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
	"N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
	
	// 小写字母
	"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
	"n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
	
	// 标点符号
	".", ",", ";", ":", "!", "?", "\"", "'", "(", ")", "[", "]", "{", "}",
	
	// 特殊字符
	"@", "#", "$", "%", "&", "*", "+", "-", "=", "/", "\\", "|", "<", ">",
	"~", "`", "^", "_"
};

		public override void Projectile(SkillData skillData, Godot.Vector2 startPosiition, Node BulletNode)//发射
		{
			skillData = NormalData.AddData(GetSkillData(), skillData);

			int count = skillData.AttackCount;
			//GD.Print(count);
			if (count % 2 == 0)
			{
				float size = NormalData.AttackLength / (count + 1);
				for (int i = 1; i <= count; i++)
				{
					PackedScene bulletScene = GD.Load<PackedScene>("res://trashbox/战斗系统/Attack/Skills/Projectile/字符投射物.tscn");
					
					Node bulletNode = bulletScene.Instantiate();

					if (bulletNode is SymbolBullet bullet)
					{
						//int count = skillData.AttackCount;
						//GD.Print(count);
						int randomIndex = random.Next(basicChars.Length);
						float y = (NormalData.AttackLength / 2.0f) - (size * i);
						bullet.Initialize(skillData,basicChars[randomIndex]);
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
					PackedScene bulletScene = GD.Load<PackedScene>("res://trashbox/战斗系统/Attack/Skills/Projectile/字符投射物.tscn");
					Node bulletNode = bulletScene.Instantiate();

					if (bulletNode is SymbolBullet bullet)
					{
						int randomIndex = random.Next(basicChars.Length);
						bullet.Initialize(skillData,basicChars[randomIndex]);
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
