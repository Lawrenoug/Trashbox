using Godot;
using System;

namespace Attack
{
    [GlobalClass]
    public partial class SkillDataUI: Resource
    {
        [Export]
        public Texture2D Icon;
        [Export]
        public string name;
        [Export]
        public string introduction;
        [Export]
        public string quote;
        [Export]
        public Skill skill;
    }
}