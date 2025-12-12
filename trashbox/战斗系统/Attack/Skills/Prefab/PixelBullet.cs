using Godot;
using System;
using System.Numerics;

namespace Attack
{
	public partial class PixelBullet : Bullet
	{
		public override float ATK { get; set; } = 0;
		public override float ATS { get; set; } = 1;
		public override bool enableTarcking { get; set; } = false;

		private Node2D target; // 跟踪目标
		private float trackingRange = 1000f; // 跟踪范围


		public override void Initialize(SkillData _skillData)
		{
			var sprite2D = GetNode<Sprite2D>("Sprite2D");
			sprite2D.Scale = new Godot.Vector2(_skillData.RNG, _skillData.RNG);
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

		
	}
}
