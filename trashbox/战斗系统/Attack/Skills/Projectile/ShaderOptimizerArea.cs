using Godot;
using System;
using Enemy;
using Buff;

namespace Attack
{
	public partial class ShaderOptimizerArea : Area2D
	{
		public float duration = 6;
		public float timeElapsed = 0;

		private CollisionShape2D collisionShape2D;
		public override void _Ready()
        {
            base._Ready();
			collisionShape2D = GetNode<CollisionShape2D>("CollisionShape2D");
			
			// 连接body_entered信号
			Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));
        }

		public override void _Process(double delta)
        {
            timeElapsed += (float)delta;
			if (timeElapsed >= duration)
			{
				QueueFree();
			}
        }
        
        // 当有物体进入区域时调用
        public void OnBodyEntered(Node body)
        {
        	// 检查进入区域的是否为敌人
        	if (body.IsInGroup("enemy"))
        	{
        		EnemyBase enemy = body as EnemyBase;
        		
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
				if (gameRenderingCount < 3)
				{
					GameRendering buff = new GameRendering();
					// 使用CallDeferred推迟添加节点操作到下一帧执行
					enemy.CallDeferred("add_child", buff);
				}
        	}
        }
	}
}