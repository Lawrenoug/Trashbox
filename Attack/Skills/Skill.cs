using Godot;
using System;

namespace Attack
{
    public abstract partial class Skill : Node
    {
        public virtual string skillType { get; set; } = "";
        public virtual string skillName { get; set; } = "";

        public virtual float ATK { get; set; } = 0;//攻击力

        public virtual float ATS { get; set; } = 0;//攻速

        public virtual float RNG { get; set; } = 1;//攻击范围

        public virtual int AttackCount { get; set; } = 1;//弹道

        public virtual bool enableTarcking { get; set; } = false;

        public virtual SkillData GetSkillData()
        {
            SkillData skillDataForNext = new SkillData();
            skillDataForNext.ATK =ATK;
            skillDataForNext.ATS =ATS;
            skillDataForNext.RNG =RNG;
            skillDataForNext.AttackCount =  AttackCount;
            skillDataForNext.enableTarcking = enableTarcking;
            return skillDataForNext;
        }

        public virtual SkillData Amendment(SkillData skillData)
        {
            SkillData skillDataForNext = new SkillData();
            skillDataForNext.ATK = skillData.ATK + ATK;
            skillDataForNext.ATS = skillData.ATS + ATS;
            skillDataForNext.RNG = skillData.RNG + RNG;
            skillDataForNext.AttackCount = skillData.AttackCount + AttackCount;
            skillDataForNext.enableTarcking = skillData.enableTarcking || enableTarcking;
            return skillDataForNext;
        }

        public virtual void Projectile(SkillData skillData)
        {
            
        }
    }

    public struct SkillData
    {
        public float ATK;
        public float ATS;
        public float RNG;
        public int AttackCount;
        public bool enableTarcking;
    }
}