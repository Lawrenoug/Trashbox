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
			skillData = new SkillData();
			// 【修复】确保构造时初始化列表
			skillData.BuffTypes = new List<string>(); 
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
			GD.Print("技能组数量：" + skillCount);
			enableAttack = true;
		}

		public void AttackLoop(float delta, Godot.Vector2 startPosiition)
		{
			_timeDelay += delta;
			if (_attackDelay <= _timeDelay)
			{
				if(!enableAttack) return;
				if (BulletNode == null) return;
				if(skills.Length <= 0) return;

				if (skillsIndex < skillCount)
				{
					// 处理 Amendment (修正类技能)
					while (skillsIndex < skillCount && skills[skillsIndex].skillType == "amendment")
					{
						AddSkillData(skills[skillsIndex++].GetSkillData());
					}
					
					// 处理 Projectile (投射物类技能)
					if (skillsIndex < skillCount && skills[skillsIndex].skillType == "projectile")
					{
						skills[skillsIndex].Projectile(skillData, startPosiition, BulletNode);
						ClearSkillData();
						skillsIndex++;
						_timeDelay = 0;
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
			skillData.RNG = 0;
			skillData.AttackCount = 0;
			skillData.enableTarcking = false;
			
			// 【修复】防止空指针，重新初始化或清空
			if (skillData.BuffTypes == null)
			{
				skillData.BuffTypes = new List<string>();
			}
			else
			{
				skillData.BuffTypes.Clear();
			}
		}
		
		private void AddSkillData(SkillData _skillData)
		{
			skillData.ATK += _skillData.ATK;
			skillData.ATS += _skillData.ATS;
			skillData.RNG += _skillData.RNG;
			skillData.AttackCount += _skillData.AttackCount;
			skillData.enableTarcking = skillData.enableTarcking || _skillData.enableTarcking;
			
			// 【修复】核心修复点：防止列表为空导致的崩溃
			if (skillData.BuffTypes == null) 
			{
				skillData.BuffTypes = new List<string>();
			}

			if (_skillData.BuffTypes != null)
			{
				foreach(string buffType in _skillData.BuffTypes)
				{
					skillData.BuffTypes.Add(buffType);
				}
			}
		}
	}
}
