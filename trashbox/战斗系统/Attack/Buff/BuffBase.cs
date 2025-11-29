using CharacterManager;
using Enemy;
using Godot;
using System;

namespace Buff
{
	public abstract partial class BuffBase:Node
	{
		public virtual BuffType buffType { get; set; }
		public virtual string buffName { get; set; }
		public virtual float duration { get; set; }//持续时间
		public virtual int layer { get; set; }
		public virtual float timeElapsed { get; set; } = 0;//已持续时间
		public virtual float interval { get; set; } = 1;//间隔时间
		public virtual float intervalElapsed { get; set; } = 0;//已间隔时间
		public virtual float DamageValue { get; set; }
		public virtual void BuffResult()
        {
            
        }

		public virtual void Improve(string ImproveType)
		{

		}
		public override void _Ready()
		{
			
		}
		public override void _Process(double delta)
		{
			timeElapsed += (float)delta;
			if (timeElapsed >= duration)
			{
				QueueFree();
			}
		}
	}

	public partial class GameRendering: BuffBase
    {
        public override BuffType buffType { get; set; } = BuffType.GameRendering;

		public override string buffName { get; set; } = "游戏渲染中";
		public override float duration { get; set; } = 5;
		public override int layer { get; set; }=3;
		public override float timeElapsed { get; set; } = 0;
		public override float DamageValue { get; set; } = 10;
		public override float interval { get; set; } = 1;
		public override float intervalElapsed { get; set; } = 0;

		public override void BuffResult()
		{
			EnemyBase enemy = GetParent<EnemyBase>();
			enemy.TakeDamage(DamageValue);
		}

		public override void Improve(string ImproveType)
		{
			
		}

		public override void _Ready()
		{
			base._Ready();
		}
		public override void _Process(double delta)
		{
			timeElapsed += (float)delta;
			intervalElapsed += (float)delta;
			if (intervalElapsed >= interval)
            {
				intervalElapsed = 0;
				BuffResult();
            }
			if (timeElapsed >= duration)
			{
				QueueFree();
			}
		}
	
    }

	public partial class NetworkFluctuation: BuffBase
    {
        public override BuffType buffType { get; set; } = BuffType.NetworkFluctuation;

		public override string buffName { get; set; } = "网络波动";
		public override float duration { get; set; } = 5;
		public override int layer { get; set; }=3;
		public override float timeElapsed { get; set; } = 0;
		public override float DamageValue { get; set; } = 10;
		public override float interval { get; set; } = 1;
		public override float intervalElapsed { get; set; } = 0;

		public override void BuffResult()
		{
			EnemyBase enemy = GetParent<EnemyBase>();
			if(enemy!=null)
            {
                Godot.Collections.Array<Node> enemies = enemy.GetTree().GetNodesInGroup("enemy");
				
				foreach (Node node in enemies)
				{
					EnemyBase _enemy = node as EnemyBase;
					if (_enemy != null && _enemy != enemy)
					{
						// 计算距离
						float distance = enemy.Position.DistanceTo(_enemy.Position);
						if (distance <= 200) 
						{
							// 对范围内的敌人造成伤害
							_enemy.TakeDamage(DamageValue);
						}
					}
				}
				enemy.TakeDamage(DamageValue);
            }
		}

		public override void _Ready()
		{
			base._Ready();
			BuffResult();
		}
	
    }

	public partial class ProgramDebugging: BuffBase
    {
        public override BuffType buffType { get; set; } = BuffType.ProgramDebugging;

		public override string buffName { get; set; } = "程序调试中";
		public override float duration { get; set; } = 4;
		public override int layer { get; set; }=3;
		public override float timeElapsed { get; set; } = 0;
		public override float DamageValue { get; set; } = 10;
		public override float interval { get; set; } = 1;
		public override float intervalElapsed { get; set; } = 0;

		public override void BuffResult()
		{
			EnemyBase enemy = GetParent<EnemyBase>();
			if (enemy != null)
			{
				enemy.ATS *= 0.5f;
				enemy.MoveSpeed *= 0.7f;
			}
		}

		public override void _Ready()
		{
			base._Ready();
			BuffResult();
		}

		public override void _Process(double delta)
		{
			timeElapsed += (float)delta;
			if (timeElapsed >= duration)
			{
				EnemyBase enemy = GetParent<EnemyBase>();
				if (enemy != null)
				{
					enemy.ATS /= 0.5f;
					enemy.MoveSpeed /= 0.7f;
				}
				QueueFree();
			}
		}
	
    }


	public partial class MemoryWeakPoint: BuffBase
    {
        public override BuffType buffType { get; set; } = BuffType.MemoryWeakPoint;

		public override string buffName { get; set; } = "内存弱点";
		public override float duration { get; set; } = 3;
		public override int layer { get; set; }=3;
		public override float timeElapsed { get; set; } = 0;
		public override float DamageValue { get; set; } = 10;
		public override float interval { get; set; } = 1;
		public override float intervalElapsed { get; set; } = 0;

		public override void BuffResult()
		{
			EnemyBase enemy = GetParent<EnemyBase>();
			if (enemy != null)
			{
				enemy.DamageMultiplier += 0.1f;
			}
			
		}

		public override void _Ready()
		{
			base._Ready();
			BuffResult();
		}

		public override void _Process(double delta)
		{
			timeElapsed += (float)delta;
			if (timeElapsed >= duration)
			{
				EnemyBase enemy = GetParent<EnemyBase>();
				if (enemy != null)
				{
					enemy.DamageMultiplier -= 0.1f;
				}
				QueueFree();
			}
		}
	
    }

	public partial class RealTimeDebugging: BuffBase
    {
        
    }

	public partial class Experience: BuffBase
    {
        public override BuffType buffType { get; set; } = BuffType.Experience;

		public override string buffName { get; set; } = "经验";
		public override float duration { get; set; } = 3;
		public override int layer { get; set; }=3;
		public override float timeElapsed { get; set; } = 0;
		public override float DamageValue { get; set; } = 10;
		public override float interval { get; set; } = 1;
		public override float intervalElapsed { get; set; } = 0;

		public override void BuffResult()
		{
			// PlayerManager player = GetTree().GetNodesInGroup("player")[0] as PlayerManager;
			// if (player != null)
			// {
			// 	player.ATS *= 1.01f;
			// 	player.TotalDamageMultiplier *= 1.01f;
			// }
		}

		public override void _Ready()
		{
			base._Ready();
			BuffResult();
		}
	
    }
	public enum BuffType
	{
		GameRendering,        // 【游戏渲染中】每秒受到10点伤害，持续 5 秒，最多叠3层。
		NetworkFluctuation,   // 【网络波动】对周围半径2米的敌方单位造成一次10点的链接冲击
		ProgramDebugging,     // 【程序调试中】攻击速度降低 50%，移动速度降低 30%，持续 4 秒。
		MemoryWeakPoint,      // 【内存弱点】受到的伤害提高10%，可叠加3层。
		RealTimeDebugging,   // 对身上带有任何负面状态的敌人，你的伤害提高50%
		Experience,          // 【经验】每层使你攻击速度提升 1%，所有伤害提升 1%，每局刷新。
	}
}
