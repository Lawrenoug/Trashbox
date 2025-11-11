using Godot;
using System;
using System.Numerics;

namespace Attack
{
	public partial class PixelBullet : RigidBody2D
	{
		public float ATK=0;
		public float ATS=10;
		public bool enableTarcking=false;

		public void Initialize(SkillData _skillData)
        {
			ATK = _skillData.ATK;
			ATS = _skillData.ATS;
			enableTarcking = _skillData.enableTarcking;
        }

		public override void _Process(double delta)
        {
			if (enableTarcking)
			{

			}
			else
			{
				
				float moveDistance = ATS * (float)delta;
				
				Godot.Vector2 movement = Transform.X * moveDistance;
				
				GlobalPosition += movement;
			}
		}
	}
}