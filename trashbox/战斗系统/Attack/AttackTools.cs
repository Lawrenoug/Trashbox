using Godot;
using System;
using System.Collections.Generic;

namespace Attack
{

	public class SkillsListData
	{
		public string Path { get; set; }
		public float Probability { get; set; }
		//public PackedScene Scene { get; set; }
	}
	public class NormalEnemyListData
	{
		public string Name { get; set; }
		public string Path { get; set; }
		
	}
	public static class AttackTools
	{
		
		public static List<SkillsListData> SceneList { get; private set; } =
		new List<SkillsListData>()
		{
			new SkillsListData()
			{
				Path = "trashbox\\战斗系统\\Attack\\Skills\\Amendment\\动态绑定.tscn",
				Probability = 0.5f
			},
			new SkillsListData()
			{
				Path = "trashbox\\战斗系统\\Attack\\Skills\\Amendment\\多通道渲染.tscn",
				Probability = 0.5f
			},
			new SkillsListData()
			{
				Path = "trashbox\\战斗系统\\Attack\\Skills\\Amendment\\多线程开发.tscn",
				Probability = 0.5f
			},
			new SkillsListData()
			{
				Path = "trashbox\\战斗系统\\Attack\\Skills\\Amendment\\视觉特效图层.tscn",
				Probability = 0.5f
			},
			new SkillsListData()
			{
				Path = "trashbox\\战斗系统\\Attack\\Skills\\Amendment\\算法优化___空间复杂度降低代码块.tscn",
				Probability = 0.5f
			},
			new SkillsListData()
			{
				Path = "trashbox\\战斗系统\\Attack\\Skills\\Amendment\\算法优化___时间复杂度降低代码块.tscn",
				Probability = 0.5f
			},
			new SkillsListData()
			{
				Path = "trashbox\\战斗系统\\Attack\\Skills\\Projectile\\材质球代码块.tscn",
				Probability = 0.5f
			},
			new SkillsListData()
			{
				Path = "trashbox\\战斗系统\\Attack\\Skills\\Projectile\\后期处理代码块.tscn",
				Probability = 0.5f
			},
			new SkillsListData()
			{
				Path = "trashbox\\战斗系统\\Attack\\Skills\\Projectile\\内存剖析器代码块.tscn",
				Probability = 0.5f
			},
			new SkillsListData()
			{
				Path = "trashbox\\战斗系统\\Attack\\Skills\\Projectile\\网络通信代码块.tscn",
				Probability = 0.5f
			},
			new SkillsListData()
			{
				Path = "trashbox\\战斗系统\\Attack\\Skills\\Projectile\\音频测试代码块.tscn",
				Probability = 0.5f
			},
			new SkillsListData()
			{
				Path = "trashbox\\战斗系统\\Attack\\Skills\\Projectile\\着色器优化器代码块.tscn",
				Probability = 0.5f
			},
			new SkillsListData()
			{
				Path = "trashbox\\战斗系统\\Attack\\Skills\\Projectile\\帧调试器代码块.tscn",
				Probability = 0.5f
			},
			new SkillsListData()
			{
				Path = "trashbox\\战斗系统\\Attack\\Skills\\Projectile\\指针代码块.tscn",
				Probability = 0.5f
			},
			new SkillsListData()
			{
				Path = "trashbox\\战斗系统\\Attack\\Skills\\Projectile\\字符代码块.tscn",
				Probability = 0.5f
			}
		};

		public static PackedScene GetSkill()
		{
			Random random = new Random();
			double randomNumber = random.NextDouble();
			double cumulativeProbability = 0.0;
			
			// 计算总概率
			double totalProbability = 0.0;
			foreach (var skill in SceneList)
			{
				totalProbability += skill.Probability;
			}
			
			// 如果总概率为0，返回null
			if (totalProbability <= 0)
				return null;
			
			// 标准化随机数到总概率范围
			randomNumber *= totalProbability;
			
			// 根据累积概率选择技能
			foreach (var skill in SceneList)
			{
				cumulativeProbability += skill.Probability;
				if (randomNumber <= cumulativeProbability)
				{
					PackedScene scene = GD.Load<PackedScene>(skill.Path);
					return scene;
				}
			}
			
			// 如果由于浮点数精度问题没有匹配到任何技能，返回最后一个技能
			if (SceneList.Count > 0)
			{
				PackedScene scene = GD.Load<PackedScene>(SceneList[SceneList.Count - 1].Path);
				return scene;
			}
			
			return null;
		}
	

	
	}
}