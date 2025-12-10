using Godot;
using System;
using Attack;
using Enemy;
using Buff;
namespace Attack
{
	public partial class PostProcessingBullet : Bullet
	{
		public  override float ATK { get; set; } = 0;
        public override float ATS { get; set; } = 1;
        public override bool enableTarcking { get; set; } = false;


        public override void Initialize(SkillData _skillData)
        {
            var Sprite2D = GetNode<Sprite2D>("Sprite2D");
            Sprite2D.Scale = new Godot.Vector2(_skillData.RNG, _skillData.RNG);
            var collisionShape2D = GetNode<CollisionShape2D>("CollisionShape2D");
            collisionShape2D.Scale = new Godot.Vector2(_skillData.RNG, _skillData.RNG);
			
            ATK += _skillData.ATK;
            ATS += _skillData.ATS;
            enableTarcking = _skillData.enableTarcking;
        }

        public override void _Ready()
        {
            base._Ready();
        }
        public override void _Process(double delta)
        {
            base._Process(delta);
        }

		public override void OnBodyEntered(Node body)
		{
			if (body.IsInGroup("enemy"))
			{
				GD.Print("子弹碰撞到敌人，自动销毁");
				EnemyBase enemy = body as EnemyBase;
				if (enemy != null)
				{
					// 创建临时列表避免在遍历时修改集合
					var debuffsToProcess = new Godot.Collections.Array<Node>();
					
					foreach(var child in enemy.GetChildren())
					{
						if(child is GameRendering debuff && child != null)
						{
							debuffsToProcess.Add(debuff);
						}
					}
					
					// 处理收集到的debuff
					float totalDamage = 0;
					foreach(var debuffNode in debuffsToProcess)
					{
						if(debuffNode is GameRendering debuff)
						{
							// 更精确地计算剩余伤害
							float remainingTime = Mathf.Max(0, debuff.duration - debuff.timeElapsed);
							float value = Mathf.Round(remainingTime) * debuff.DamageValue;
							totalDamage += value;
							debuff.QueueFree();
						}
					}
					
					// 一次性应用总伤害
					if(totalDamage > 0)
					{
						enemy.TakeDamage(totalDamage);
					}
				}
			
				enemy.TakeDamage(ATK);
				// 使用CallDeferred推迟销毁操作到下一帧执行
				CallDeferred("queue_free");
			}
		}
	}
}