using Godot;
using System;
using System.Collections.Generic;

namespace Enemy
{
	/// <summary>
	/// 支持弹幕攻击的敌人实现类
	/// </summary>
	public partial class DanmakuEnemy : EnemyBase
	{
		// 弹幕相关导出变量
		[Export] public Resource SpellCardResource; // 当前符卡配置
		[Export] public PackedScene BulletScene; // 子弹预制体
		[Export] public float SpellCardDuration = 10.0f; // 符卡持续时间
		
		// 弹幕系统内部变量
		private Node2D _bulletContainer; // 子弹容器节点
		private Dictionary<int, float> _trackTimers; // 每条轨道的计时器
		private float _spellCardTimer; // 符卡计时器
		private bool _isAttacking = false; // 是否正在攻击
		private Random _random = new Random(); // 随机数生成器
		private SpellCardConfig _spellCard; // 当前符卡配置

		public override void _Ready()
		{
			base._Ready();
			
			// 获取子弹容器节点
			_bulletContainer = GetNode<Node2D>("BulletContainer");
			
			// 初始化轨道计时器
			_trackTimers = new Dictionary<int, float>();
			
			// 获取符卡配置
			_spellCard = SpellCardResource as SpellCardConfig;
			
			// 连接区域进入信号，用于检测与玩家子弹的碰撞
			GetNode<Area2D>("Area2D").AreaEntered += OnAreaEntered;
		}

		public override void _Process(double delta)
		{
			base._Process(delta);
			
			// 如果正在攻击，则更新弹幕逻辑
			if (_isAttacking && _spellCard != null)
			{
				UpdateDanmaku((float)delta);
				
				// 检查符卡是否结束
				_spellCardTimer += (float)delta;
				if (_spellCardTimer >= SpellCardDuration)
				{
					EndSpellCard();
				}
			}
		}

		/// <summary>
		/// 开始符卡攻击
		/// </summary>
		public void StartSpellCard()
		{
			if (_spellCard == null || _spellCard.Tracks == null)
			{
				GD.Print("无法开始符卡攻击：未设置符卡或轨道配置为空");
				return;
			}
			
			_isAttacking = true;
			_spellCardTimer = 0f;
			
			// 初始化各轨道计时器
			_trackTimers.Clear();
			for (int i = 0; i < _spellCard.Tracks.Count; i++)
			{
				_trackTimers[i] = 0f;
			}
			
			GD.Print($"{enemyName} 开始符卡攻击: {_spellCard.SpellName}");
		}

		/// <summary>
		/// 结束符卡攻击
		/// </summary>
		public void EndSpellCard()
		{
			_isAttacking = false;
			_spellCardTimer = 0f;
			GD.Print($"{enemyName} 结束符卡攻击");
		}

		/// <summary>
		/// 更新弹幕逻辑
		/// </summary>
		/// <param name="delta">帧时间</param>
		private void UpdateDanmaku(float delta)
		{
			// 遍历所有轨道
			for (int i = 0; i < _spellCard.Tracks.Count; i++)
			{
				var track = _spellCard.Tracks[i];
				_trackTimers[i] += delta;
				
				// 检查是否到达发射间隔
				if (_trackTimers[i] >= track.FireInterval)
				{
					FireBulletTrack(track, i);
					_trackTimers[i] = 0f;
				}
			}
		}

		/// <summary>
		/// 发射一条轨道的子弹
		/// </summary>
		/// <param name="track">轨道配置</param>
		/// <param name="trackIndex">轨道索引</param>
		private void FireBulletTrack(BulletTrackConfig track, int trackIndex)
		{
			// 根据弹幕模式生成子弹
			switch (track.Pattern)
			{
				case BulletPattern.Straight:
					FireStraightBullets(track, trackIndex);
					break;
				case BulletPattern.Ring:
					FireRingBullets(track, trackIndex);
					break;
				case BulletPattern.Spiral:
					FireSpiralBullets(track, trackIndex);
					break;
				case BulletPattern.Wave:
					FireWaveBullets(track, trackIndex);
					break;
				default:
					FireStraightBullets(track, trackIndex);
					break;
			}
		}

		/// <summary>
		/// 发射直线型子弹
		/// </summary>
		private void FireStraightBullets(BulletTrackConfig track, int trackIndex)
		{
			for (int i = 0; i < track.BulletCount; i++)
			{
				float angle = track.StartAngle + track.AngleStep * i;
				CreateBullet(track, angle);
			}
		}

		/// <summary>
		/// 发射环形子弹
		/// </summary>
		private void FireRingBullets(BulletTrackConfig track, int trackIndex)
		{
			float angleStep = 360f / track.BulletCount;
			for (int i = 0; i < track.BulletCount; i++)
			{
				float angle = track.StartAngle + angleStep * i;
				CreateBullet(track, angle);
			}
		}

		/// <summary>
		/// 发射螺旋形子弹
		/// </summary>
		private void FireSpiralBullets(BulletTrackConfig track, int trackIndex)
		{
			float timeFactor = _spellCardTimer * 0.1f;
			for (int i = 0; i < track.BulletCount; i++)
			{
				float angle = track.StartAngle + track.AngleStep * i + timeFactor * track.PhaseOffset;
				CreateBullet(track, angle);
			}
		}

		/// <summary>
		/// 发射波浪形子弹
		/// </summary>
		private void FireWaveBullets(BulletTrackConfig track, int trackIndex)
		{
			float waveOffset = Mathf.Sin(_spellCardTimer * 2f + trackIndex) * 30f;
			for (int i = 0; i < track.BulletCount; i++)
			{
				float angle = track.StartAngle + track.AngleStep * i + waveOffset;
				CreateBullet(track, angle);
			}
		}

		/// <summary>
		/// 创建单个子弹
		/// </summary>
		/// <param name="track">轨道配置</param>
		/// <param name="angle">发射角度</param>
		private void CreateBullet(BulletTrackConfig track, float angle)
		{
			if (BulletScene == null)
			{
				GD.PrintErr("子弹预制体未设置！");
				return;
			}
			
			// 实例化子弹
			var bullet = BulletScene.Instantiate() as RigidBody2D;
			if (bullet == null)
			{
				GD.PrintErr("子弹预制体类型不正确！");
				return;
			}
			
			// 设置子弹初始位置
			bullet.GlobalPosition = GlobalPosition;
			
			// 添加到场景树
			_bulletContainer.AddChild(bullet);
		}

		/// <summary>
		/// 区域进入事件处理
		/// </summary>
		private void OnAreaEntered(Area2D area)
		{
			// 检测是否与玩家子弹碰撞
			if (area.IsInGroup("PlayerBullet"))
			{
				// 这里可以添加受击效果
				GD.Print($"{enemyName} 被玩家子弹击中");
			}
		}

		/// <summary>
		/// 受伤处理
		/// </summary>
		public override void TakeDamage(float damage)
		{
			base.TakeDamage(damage);
			
			// 可以在这里添加特殊的受击效果
			// 比如闪烁、震动等
		}

		/// <summary>
		/// 死亡处理
		/// </summary>
		public override void Die()
		{
			// 停止弹幕攻击
			EndSpellCard();
			
			// 清理子弹
			if (_bulletContainer != null)
			{
				foreach (Node child in _bulletContainer.GetChildren())
				{
					child.QueueFree();
				}
			}
			
			base.Die();
		}
	}
}