using Godot;
using System;
using System.Collections.Generic;

namespace Attack
{
    //[GlobalClass]
    public abstract partial class Skill: Sprite2D
    {
        //[Export] public virtual Texture2D skillIcon { get; set; }
        public virtual string skillType { get; set; } = "";
        public virtual string skillName { get; set; } = "";

        public virtual string skillDescription { get; set; } = "";

        public virtual string skillQuote { get; set; } = "";

        public virtual float ATK { get; set; } = 0;//攻击力

        public virtual float ATS { get; set; } = 0;//攻速

        public virtual float RNG { get; set; } = 1;//攻击范围

        public virtual int AttackCount { get; set; } = 1;//弹道

        public virtual bool enableTarcking { get; set; } = false;

        public virtual string buffType{get;set;}="";

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

        public virtual void Projectile(SkillData skillData,Godot.Vector2 startPosotion,Node BulletNode)
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
        public List<string> BuffTypes;
    }

    public static class NormalData
    {
        public static float AttackLength = 150;
        public static float ATS = 100;

        public static SkillData AddData(SkillData skillData_1, SkillData skillData_2)
        {
            SkillData newData = new SkillData();
            newData.ATK = skillData_1.ATK * (1 + skillData_2.ATK);
            newData.ATS = skillData_1.ATS * (1 + skillData_2.ATS);
            newData.RNG = skillData_1.RNG * (1 + skillData_2.RNG);
            newData.AttackCount = skillData_1.AttackCount + skillData_2.AttackCount;
            newData.enableTarcking = skillData_1.enableTarcking || skillData_2.enableTarcking;
            return newData;
        }
    }

}