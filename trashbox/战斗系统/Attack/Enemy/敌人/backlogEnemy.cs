using Godot;
using System;

namespace Enemy
{
	public partial class backlogEnemy : EnemyBase
	{
		public override float MaxHP { get; set; } = 100;
		public override float CurrentHP { get; set; } = 100;
		public override float ATK { get; set; } = 15;
		public override float ATS { get; set; } = 5;
		public override float MoveSpeed { get; set; } = 80;
		public override string enemyName { get; set; } = "待办事项";
		public override void _Ready()
        {
            base._Ready();
			state = EnemyState.Attacking;
        }
		
		public override void _Process(double delta)
        {
            base._Process(delta);
        }
	}
}