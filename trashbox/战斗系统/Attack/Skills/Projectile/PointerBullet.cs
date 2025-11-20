using Godot;
using System;

namespace Attack
{
	public partial class PointerBullet : Bullet
	{
		public  override float ATK { get; set; } = 0;
        public override float ATS { get; set; } = 1;
        public override bool enableTarcking { get; set; } = false;


        public override void Initialize(SkillData _skillData)
        {
            var label = GetNode<Label>("Label");
            label.Scale = new Godot.Vector2(_skillData.RNG*0.1f, _skillData.RNG*0.1f);
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