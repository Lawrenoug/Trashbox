using Godot;
using System;
using System.Collections.Generic;

namespace Enemy
{
	/// <summary>
	/// 弹幕管理系统，用于创建和编辑弹幕谱面
	/// </summary>
	[GlobalClass]
	public partial class DanmakuManager : Node
	{
		// 单例实例
		private static DanmakuManager _instance;
		public static DanmakuManager Instance => _instance;
		
		// 存储所有符卡配置
		private Dictionary<string, SpellCardConfig> _spellCards = new Dictionary<string, SpellCardConfig>();
		
		public override void _Ready()
		{
			_instance = this;
			// 设置为不随场景切换而销毁
			// ProcessMode = ProcessModeEnum.Always;
		}
		
		/// <summary>
		/// 创建新的符卡配置
		/// </summary>
		/// <param name="name">符卡名称</param>
		/// <returns>新的符卡配置</returns>
		public SpellCardConfig CreateSpellCard(string name)
		{
			var spellCard = new SpellCardConfig
			{
				SpellName = name,
				Duration = 10.0f,
				Tracks = new Godot.Collections.Array<BulletTrackConfig>()
			};
			
			_spellCards[name] = spellCard;
			return spellCard;
		}
		
		/// <summary>
		/// 添加轨道到符卡
		/// </summary>
		/// <param name="spellCard">符卡配置</param>
		/// <param name="track">轨道配置</param>
		public void AddTrackToSpellCard(SpellCardConfig spellCard, BulletTrackConfig track)
		{
			if (spellCard.Tracks == null)
			{
				spellCard.Tracks = new Godot.Collections.Array<BulletTrackConfig>();
			}
			
			spellCard.Tracks.Add(track);
		}
		
		/// <summary>
		/// 创建标准轨道配置
		/// </summary>
		/// <returns>标准轨道配置</returns>
		public BulletTrackConfig CreateStandardTrack()
		{
			return new BulletTrackConfig
			{
				Pattern = BulletPattern.Straight,
				BulletCount = 8,
				FireInterval = 0.5f,
				BaseSpeed = 100f,
				SpeedGradient = 0f,
				StartAngle = 0f,
				AngleStep = 45f,
				PhaseOffset = 0f,
				BulletColor = Colors.Red
			};
		}
		
		/// <summary>
		/// 创建环形轨道配置
		/// </summary>
		/// <returns>环形轨道配置</returns>
		public BulletTrackConfig CreateRingTrack()
		{
			return new BulletTrackConfig
			{
				Pattern = BulletPattern.Ring,
				BulletCount = 16,
				FireInterval = 1.0f,
				BaseSpeed = 80f,
				SpeedGradient = 0f,
				StartAngle = 0f,
				AngleStep = 22.5f,
				PhaseOffset = 0f,
				BulletColor = Colors.Blue
			};
		}
		
		/// <summary>
		/// 创建螺旋轨道配置
		/// </summary>
		/// <returns>螺旋轨道配置</returns>
		public BulletTrackConfig CreateSpiralTrack()
		{
			return new BulletTrackConfig
			{
				Pattern = BulletPattern.Spiral,
				BulletCount = 12,
				FireInterval = 0.3f,
				BaseSpeed = 60f,
				SpeedGradient = 0f,
				StartAngle = 0f,
				AngleStep = 30f,
				PhaseOffset = 10f,
				BulletColor = Colors.Green
			};
		}
		
		/// <summary>
		/// 创建波浪轨道配置
		/// </summary>
		/// <returns>波浪轨道配置</returns>
		public BulletTrackConfig CreateWaveTrack()
		{
			return new BulletTrackConfig
			{
				Pattern = BulletPattern.Wave,
				BulletCount = 10,
				FireInterval = 0.4f,
				BaseSpeed = 90f,
				SpeedGradient = 0f,
				StartAngle = 90f,
				AngleStep = 10f,
				PhaseOffset = 0f,
				BulletColor = Colors.Yellow
			};
		}
		
		/// <summary>
		/// 保存符卡配置到文件
		/// </summary>
		/// <param name="spellCard">符卡配置</param>
		/// <param name="path">保存路径</param>
		public void SaveSpellCard(SpellCardConfig spellCard, string path)
		{
			ResourceSaver.Save(spellCard, path);
			GD.Print($"符卡已保存到: {path}");
		}
		
		/// <summary>
		/// 从文件加载符卡配置
		/// </summary>
		/// <param name="path">文件路径</param>
		/// <returns>符卡配置</returns>
		public SpellCardConfig LoadSpellCard(string path)
		{
			var spellCard = ResourceLoader.Load<SpellCardConfig>(path);
			if (spellCard != null && !string.IsNullOrEmpty(spellCard.SpellName))
			{
				_spellCards[spellCard.SpellName] = spellCard;
			}
			return spellCard;
		}
		
		/// <summary>
		/// 获取符卡配置
		/// </summary>
		/// <param name="name">符卡名称</param>
		/// <returns>符卡配置</returns>
		public SpellCardConfig GetSpellCard(string name)
		{
			return _spellCards.ContainsKey(name) ? _spellCards[name] : null;
		}
		
		/// <summary>
		/// 获取所有符卡名称
		/// </summary>
		/// <returns>符卡名称列表</returns>
		public List<string> GetAllSpellCardNames()
		{
			List<string> names = new List<string>();
			foreach (var key in _spellCards.Keys)
			{
				names.Add(key);
			}
			return names;
		}
	}
}