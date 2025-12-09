using Godot;
using System;
using System.Collections.Generic;

namespace Attack
{
	public partial class AttackManager : Node
	{
		public Node BulletNode;

		public bool enableAttack = true; // 是否开启攻击
		private int skillCount = 0;
		private float _attackDelay = 0.5f; // 间隔时间 (默认设小一点方便测试)
		private float _timeDelay = 0;      // 记录间隔时间
		private Skill[] skills = new Skill[]{};

		private int skillsIndex = 0;

		// 【修改】新增一个存储全局Buff属性的变量
		private SkillData _globalBuffData;

		public AttackManager(Node _BulletNode)
		{
			BulletNode = _BulletNode;
			InitializeGlobalData();
		}

		private void InitializeGlobalData()
		{
			_globalBuffData = new SkillData();
			_globalBuffData.BuffTypes = new List<string>();
			_globalBuffData.ATK = 0;
			_globalBuffData.ATS = 0;
			_globalBuffData.RNG = 0;
			_globalBuffData.AttackCount = 0;
			_globalBuffData.enableTarcking = false;
		}

		// 添加减少攻击延迟的方法
		public void ReduceAttackDelay(float percentage)
		{
			_attackDelay *= (1.0f - percentage);
		}

		// 【核心修改】插入技能组时，直接预计算所有全局 Buff
		public void InsertSkill(List<Skill> _skills)
		{
			enableAttack = false;
			skills = _skills.ToArray();
			skillCount = _skills.Count;
			
			// 1. 重置全局数据
			InitializeGlobalData();
			
			// 2. 遍历所有技能，如果是修正类(amendment)，直接加到全局数据里
			foreach (var skill in _skills)
			{
				if (skill.skillType == "amendment")
				{
					AddSkillDataToGlobal(skill.GetSkillData());
				}
			}
			
			GD.Print($"技能组更新：共 {skillCount} 个技能。全局Buff：ATK+{_globalBuffData.ATK}, 追踪={_globalBuffData.enableTarcking}");
			
			// 重置循环索引
			skillsIndex = 0;
			_timeDelay = _attackDelay; // 让它立刻准备好发射第一个
			enableAttack = true;
		}

		public void AttackLoop(float delta, Godot.Vector2 startPosiition)
		{
			if(!enableAttack) return;
			if (BulletNode == null) return;
			if(skills.Length <= 0) return;

			_timeDelay += delta;
			
			if (_attackDelay <= _timeDelay)
			{
				// 寻找下一个可发射的投射物
				while (skillsIndex < skillCount)
				{
					var currentSkill = skills[skillsIndex];

					// 如果是投射物，发射它！
					if (currentSkill.skillType == "projectile")
					{
						// 【关键】把预计算好的 _globalBuffData 传给它
						currentSkill.Projectile(_globalBuffData, startPosiition, BulletNode);
						
						// 发射完，索引+1，重置时间，跳出循环等待下一次 Update
						skillsIndex++;
						_timeDelay = 0;
						return; 
					}
					
					// 如果是 amendment (Buff)，因为我们在 InsertSkill 时已经算进全局了，
					// 这里直接跳过，找下一个
					skillsIndex++;
				}

				// 如果循环走到了末尾 (skillsIndex >= skillCount)
				// 重置索引回 0，但这帧可能来不及发射了，等待下一帧
				if (skillsIndex >= skillCount)
				{
					skillsIndex = 0;
					// 这里不重置 _timeDelay，让下一帧立即尝试发射第0个
				}
			}
		}
		
		// 【辅助】将单个技能数据累加到全局数据中
		private void AddSkillDataToGlobal(SkillData _data)
		{
			_globalBuffData.ATK += _data.ATK;
			_globalBuffData.ATS += _data.ATS;
			_globalBuffData.RNG += _data.RNG;
			_globalBuffData.AttackCount += _data.AttackCount;
			_globalBuffData.enableTarcking = _globalBuffData.enableTarcking || _data.enableTarcking;
			
			if (_globalBuffData.BuffTypes == null) 
				_globalBuffData.BuffTypes = new List<string>();

			if (_data.BuffTypes != null)
			{
				foreach(string buff in _data.BuffTypes)
				{
					_globalBuffData.BuffTypes.Add(buff);
				}
			}
		}
	}
}
