using Godot;

namespace Enemy
{
	// 符卡谱面配置（可在编辑器创建实例）
	[Tool]
	[GlobalClass]
	[System.Serializable]
	public partial class SpellCardConfig : Resource
	{
		[Export] public string SpellName = "未命名符卡"; // 符卡名称
		[Export] public float Duration = 10f;         // 谱面持续时间（秒）
		[Export] public AudioStream Bgm;              // 谱面专属BGM
		[Export] public Godot.Collections.Array<BulletTrackConfig> Tracks; // 多组弹幕轨道（分层核心）
		[Export] public float BeatThreshold = 0.1f;   // 鼓点检测阈值
	}
}