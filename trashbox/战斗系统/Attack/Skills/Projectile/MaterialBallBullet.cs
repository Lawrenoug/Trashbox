using Godot;
using System;
using Enemy;
using Buff;

namespace Attack
{
	public partial class MaterialBallBullet : Bullet
	{
		public  override float ATK { get; set; } = 0;
        public override float ATS { get; set; } = 1;
        public override bool enableTarcking { get; set; } = false;


        public override void Initialize(SkillData _skillData)
        {
			base.Initialize(_skillData);
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
				GameRendering buff = new GameRendering();

				if(skillData.BuffTypes!=null&&skillData.BuffTypes.Count>0)
				{
					foreach(string buffType in skillData.BuffTypes)
					{
						if (buffType == "多通道渲染")
						{
							buff.layer += 2;
							buff.duration += 3;
						}
						if(buffType == "视觉特效图层")
						{
							buff.DamageValue*=1.2f;
						}
					}
				}
				// 检查敌方单位身上的GameRendering buff数量
				int gameRenderingCount = 0;
				foreach (Node child in enemy.GetChildren())
				{
					if (child is GameRendering)
					{
						gameRenderingCount++;
					}
				}
				
				// 只有当buff数量小于最大层数时才添加新的buff
				if (gameRenderingCount < buff.layer) // GameRendering的最大层数是3
				{
					enemy.AddChild(buff);
				}
				
				enemy.TakeDamage(ATK);
				QueueFree();
			}
		}
	}
}