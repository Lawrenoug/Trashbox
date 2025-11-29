using Godot;
using System;
using Enemy;
using Buff;

namespace Attack
{

	public partial class ShaderOptimizerBullet : Bullet
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
					PackedScene area = GD.Load<PackedScene>("res://trashbox/战斗系统/Attack/Skills/Projectile/优化区域.tscn");
					Node optimizerArea = area.Instantiate();
					if (optimizerArea is ShaderOptimizerArea shaderOptimizerArea)
					{
						shaderOptimizerArea.GlobalPosition=this.GlobalPosition;
						// 使用CallDeferred推迟添加节点操作到下一帧执行
						GetTree().CurrentScene.CallDeferred("add_child", shaderOptimizerArea);
					}
				
				}
			
				enemy.TakeDamage(ATK);
				// 使用CallDeferred推迟销毁操作到下一帧执行
				CallDeferred("queue_free");
			}
		}
	}
}