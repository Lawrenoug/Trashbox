using Godot;
using System;
using System.Reflection;

namespace Attack
{
    public partial class SkillsUI : Resource
    {
        [Export]
        public SkillDataUI[] SkillDataUIs;
    }
}