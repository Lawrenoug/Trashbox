using Godot;
using System;
using Buff;
using Enemy;
namespace Attack
{
	public partial class MemoryProfilerBullet : Bullet
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
				MemoryWeakPoint buff = new MemoryWeakPoint();
				// 检查敌方单位身上的MemoryWeakPoint buff数量
				int memoryWeakPointCount = 0;
				foreach (Node child in enemy.GetChildren())
				{
					if (child is MemoryWeakPoint)
					{
						memoryWeakPointCount++;
					}
				}
				
				// 只有当buff数量小于最大层数时才添加新的buff
				if (memoryWeakPointCount < buff.layer) // MemoryWeakPoint的最大层数是3
				{
					enemy.AddChild(buff);
				}
				
				enemy.TakeDamage(ATK);
				QueueFree();
			}
		}
	}
}