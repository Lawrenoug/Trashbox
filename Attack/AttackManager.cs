using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Attack
{
	public partial class AttackManager : Node
	{
		public Node BulletNode;
		private int skillCount = 8;
		private float _attackDelay = 0.5f;//间隔时间
		private float _timeDelay = 0;//记录间隔时间
		private Skill[] skills;

		private int skillsIndex = 0;

		private SkillData skillData;

		public AttackManager(Node _BulletNode)
		{
			BulletNode = _BulletNode;
			List<Skill> _test = new List<Skill>();
			_test.Add(new Pixel());
			InsertSkill(_test);
			skillData = new SkillData();
			ClearSkillData();

		}

		// 插入技能组
		public void InsertSkill(List<Skill> _skills)
		{
			skills = _skills.ToArray();
			skillCount = _skills.Count;
			GD.Print(skillCount);
		}

		public void AttackLoop(float delta,Godot.Vector2 startPosiition)
		{
			_timeDelay += delta;
			if (_attackDelay <= _timeDelay)
			{
				if(BulletNode==null)
                {
					return;
                }
				if (skillsIndex < skillCount)
				{
					while (skillsIndex < skillCount && skills[skillsIndex].skillType != "projectile")
					{
						AddSkillData(skills[skillsIndex++].GetSkillData());
					}
					
						if (skillsIndex < skillCount && skills[skillsIndex].skillType == "projectile")
						{
							//GD.Print("发射点："+startPosiition);
							skills[skillsIndex].Projectile(skillData, startPosiition,BulletNode);
							ClearSkillData();
							skillsIndex++;
							_timeDelay = 0;
							//GD.Print("攻击");
						}
					
				}
				else
				{
					skillsIndex = 0;
                }
			}
		}

		private void ClearSkillData()
		{
			skillData.ATK = 0;
			skillData.ATS = 0;
			skillData.RNG =0;
			skillData.AttackCount = 0;
			skillData.enableTarcking = false;
		}
		
		private void AddSkillData(SkillData _skillData)
        {
			skillData.ATK += _skillData.ATK;
			skillData.ATS += _skillData.ATS;
			skillData.RNG += _skillData.RNG;
			skillData.AttackCount += _skillData.AttackCount;
			skillData.enableTarcking = skillData.enableTarcking || _skillData.enableTarcking;
        }
	}
}