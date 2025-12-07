using Godot;
using System.Collections.Generic;

namespace Enemy
{
	// 弹幕模式枚举
	public enum BulletPattern
	{
		Straight,   // 直线
		Ring,       // 环形
		Spiral,     // 螺旋
		Bezier,     // 贝塞尔曲线
		Track,      // 追踪
		Wave        // 波浪
	}

	// 单个弹幕轨道的配置（谱面由多个轨道叠加而成）
	[Tool]
	[GlobalClass]
	[System.Serializable]
	public partial class BulletTrackConfig : Resource
	{
		[Export] public BulletPattern Pattern;       // 弹幕模式
		[Export] public int BulletCount = 8;         // 单次发射数量
		[Export] public float FireInterval = 0.5f;   // 轨道发射间隔（秒）
		[Export] public float BaseSpeed = 3f;        // 基础速度
		[Export] public float SpeedGradient = 0.2f;  // 速度渐变（每秒增减）
		[Export] public float StartAngle = 0f;       // 起始角度
		[Export] public float AngleStep = 45f;       // 弹幕角度步长
		[Export] public float PhaseOffset = 10f;     // 相位偏移（避免轨道重叠）
		[Export] public Color BulletColor = Colors.White; // 弹幕颜色
		[Export] public Curve SpeedCurve;   // 速度曲线（编辑器绘制）
		[Export] public Vector2[] BezierPoints;      // 贝塞尔曲线控制点（至少3个）
	}
}