using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Attack
{
	public partial class AttackManager : Node
	{
		public Node BulletNode;

		public bool enableAttack = true;//是否开启攻击
		private int skillCount = 8;
		private float _attackDelay = 2.0f;//间隔时间
		private float _timeDelay = 0;//记录间隔时间
		private Skill[] skills=new Skill[]{};

		private int skillsIndex = 0;

		private SkillData skillData;

		

		public AttackManager(Node _BulletNode)
		{
			BulletNode = _BulletNode;
			// List<Skill> _test = new List<Skill>();
			// _test.Add(new Pixel());
			// InsertSkill(_test);
			skillData = new SkillData();
			ClearSkillData();

		}

		// 添加减少攻击延迟的方法
		public void ReduceAttackDelay(float percentage)
		{
			_attackDelay *= (1.0f - percentage);
		}

		// 插入技能组
		public void InsertSkill(List<Skill> _skills)
		{
			enableAttack = false;
			skills = _skills.ToArray();
			skillCount = _skills.Count;
			GD.Print("技能组："+skillCount);
			enableAttack = true;
		}

		public void AttackLoop(float delta,Godot.Vector2 startPosiition)
		{
			_timeDelay += delta;
			if (_attackDelay <= _timeDelay)
			{
				if(!enableAttack)
                {
					return;
                }
				if (BulletNode == null)
				{
					return;
				}
				if(skills.Length<=0)
                {
					return;
                }
				if (skillsIndex < skillCount)
				{
					while (skillsIndex < skillCount && skills[skillsIndex].skillType == "amendment")
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
					ClearSkillData();
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
			skillData.BuffTypes.Clear();
		}
		
		private void AddSkillData(SkillData _skillData)
        {
			skillData.ATK += _skillData.ATK;
			skillData.ATS += _skillData.ATS;
			skillData.RNG += _skillData.RNG;
			skillData.AttackCount += _skillData.AttackCount;
			skillData.enableTarcking = skillData.enableTarcking || _skillData.enableTarcking;
			foreach(string buffType in _skillData.BuffTypes)
			{
				skillData.BuffTypes.Add(buffType);
			}
        }
	}
}