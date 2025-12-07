using Godot;
using System;
namespace Enemy
{
	// 弹幕模式枚举
	public partial class EnemyBullet : RigidBody2D
	{
		[Export] public int Damage = 1;              // 伤害
		[Export] public float TrackSpeed = 2f;       // 追踪速度
		[Export] public float MaxLifetime = 5f;      // 最大生命周期（防内存泄漏）

		private BulletPattern _pattern;              // 当前弹幕模式
		private BulletTrackConfig _trackConfig;      // 轨道配置
		private Node2D _player;                      // 追踪目标（玩家）
		private float _spawnTime;                    // 生成时间
		private float _angle;                        // 螺旋/环形轨迹的角度
		private float _radius;                       // 螺旋轨迹的半径

		// 初始化弹幕（由敌人调用）
		public void Init(BulletPattern pattern, BulletTrackConfig config, Vector2 spawnPos)
		{
			_pattern = pattern;
			_trackConfig = config;
			Position = spawnPos;
			_spawnTime = Time.GetTicksMsec() / 1000f;
			_angle = config.StartAngle;
			_radius = 1f;

			// 设置颜色
			GetNode<Sprite2D>("Sprite2D").Modulate = config.BulletColor;

			// 找到玩家（按标签查找，需给玩家节点设置Tag="Player"）
			_player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
		}

		public override void _Process(double delta)
		{
			// 生命周期检测
			if (Time.GetTicksMsec() / 1000f - _spawnTime > MaxLifetime)
			{
				QueueFree();
				return;
			}

			// 按模式更新轨迹
			UpdateTrajectory((float)delta);
		}

		// 轨迹更新核心逻辑
		private void UpdateTrajectory(float delta)
		{
			float currentTime = Time.GetTicksMsec() / 1000f - _spawnTime;
			float currentSpeed = CalculateCurrentSpeed(currentTime);

			switch (_pattern)
			{
				case BulletPattern.Straight:
					LinearVelocity = GetDirectionFromAngle(_angle) * currentSpeed;
					break;
				case BulletPattern.Ring:
					_angle += _trackConfig.AngleStep * delta;
					LinearVelocity = GetDirectionFromAngle(_angle) * currentSpeed;
					break;
				case BulletPattern.Spiral:
					_angle += _trackConfig.PhaseOffset * delta;
					_radius += _trackConfig.SpeedGradient * delta;
					Vector2 spiralDir = GetDirectionFromAngle(_angle) * _radius;
					LinearVelocity = spiralDir.Normalized() * currentSpeed;
					break;
				case BulletPattern.Bezier:
					if (_trackConfig.BezierPoints.Length < 3) break;
					// 贝塞尔曲线插值（t∈[0,1]）
					float t = Mathf.Clamp(currentTime / _trackConfig.FireInterval, 0, 1);
					Position = BezierInterpolate(
						_trackConfig.BezierPoints, t
					);
					// 朝向运动方向
					Vector2 nextPos = BezierInterpolate(
						_trackConfig.BezierPoints, Mathf.Clamp(t + 0.01f, 0, 1)
					);
					LookAt(nextPos);
					break;
				case BulletPattern.Track:
					if (_player == null) break;
					Vector2 trackDir = (_player.GlobalPosition - GlobalPosition).Normalized();
					LinearVelocity = trackDir * (currentSpeed + TrackSpeed);
					break;
				case BulletPattern.Wave:
					// 正弦波偏移（左右摆动）
					float waveOffset = Mathf.Sin(currentTime * 5f) * 0.5f;
					Vector2 waveDir = GetDirectionFromAngle(_angle + waveOffset * 10f);
					LinearVelocity = waveDir * currentSpeed;
					break;
			}
		}

		// 贝塞尔插值函数
		private Vector2 BezierInterpolate(Vector2[] points, float t)
		{
			if (points.Length < 3) return Vector2.Zero;
			
			// 简化的二次贝塞尔插值（使用中间点作为控制点）
			Vector2 p0 = points[0];
			Vector2 p1 = points[1]; // 控制点
			Vector2 p2 = points[2]; // 终点
			
			float u = 1 - t;
			float tt = t * t;
			float uu = u * u;
			
			Vector2 p = uu * p0;
			p += 2 * u * t * p1;
			p += tt * p2;
			
			return p;
		}

		// 计算当前速度（支持渐变和曲线）
		private float CalculateCurrentSpeed(float currentTime)
		{
			float speed = _trackConfig.BaseSpeed + _trackConfig.SpeedGradient * currentTime;
			if (_trackConfig.SpeedCurve != null)
			{
				speed *= _trackConfig.SpeedCurve.Sample(currentTime / MaxLifetime);
			}
			return speed;
		}

		// 角度转方向向量（Godot角度是顺时针为正，需适配）
		private Vector2 GetDirectionFromAngle(float angleDeg)
		{
			float angleRad = Mathf.DegToRad(angleDeg);
			return new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
		}

		// 碰撞检测（击中玩家）
		private void OnBodyEntered(Node2D body)
		{
			if (body.IsInGroup("Player"))
			{
				// 触发玩家受伤信号（需玩家脚本实现）
				body.CallDeferred("emit_signal", "TakeDamage", Damage);
				QueueFree();
			}
		}

		// 绑定碰撞信号（在编辑器中连接Area2D的BodyEntered信号到该方法）
		public override void _Ready()
		{
			GetNode<Area2D>("Area2D").BodyEntered += OnBodyEntered;
		}
	}
}