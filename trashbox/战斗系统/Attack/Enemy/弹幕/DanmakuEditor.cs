using Godot;
using System;

namespace Enemy
{
	/// <summary>
	/// 弹幕编辑器工具面板
	/// 可以在编辑器中使用此脚本创建和编辑弹幕谱面
	/// </summary>
	[Tool]
	public partial class DanmakuEditor : Control
	{
		// UI控件引用
		private LineEdit _spellCardNameInput;
		private SpinBox _durationInput;
		private OptionButton _patternSelector;
		private SpinBox _bulletCountInput;
		private SpinBox _fireIntervalInput;
		private SpinBox _baseSpeedInput;
		private SpinBox _speedGradientInput;
		private SpinBox _startAngleInput;
		private SpinBox _angleStepInput;
		private SpinBox _phaseOffsetInput;
		private ColorPicker _bulletColorPicker;
		private Button _addTrackButton;
		private Button _saveSpellCardButton;
		private Button _loadSpellCardButton;
		private ItemList _tracksList;
		private FileDialog _saveDialog;
		private FileDialog _loadDialog;
		
		// 当前编辑的符卡和轨道
		private SpellCardConfig _currentSpellCard;
		private BulletTrackConfig _currentTrack;
		
		public override void _Ready()
		{
			// 仅在编辑器中运行
			if (Engine.IsEditorHint())
			{
			}
				InitializeEditorInterface();
		}
		
		/// <summary>
		/// 初始化编辑器界面
		/// </summary>
		private void InitializeEditorInterface()
		{
			// 创建UI控件
			VBoxContainer mainContainer = new VBoxContainer();
			AddChild(mainContainer);
			
			// 符卡基本信息编辑区
			VBoxContainer spellCardInfo = new VBoxContainer();
			spellCardInfo.AddChild(new Label { Text = "符卡名称:" });
			_spellCardNameInput = new LineEdit { PlaceholderText = "输入符卡名称" };
			_spellCardNameInput.TextChanged += OnSpellCardNameChanged;
			spellCardInfo.AddChild(_spellCardNameInput);
			
			spellCardInfo.AddChild(new Label { Text = "持续时间(秒):" });
			_durationInput = new SpinBox { MinValue = 1, MaxValue = 60, Step = 0.5, Value = 10 };
			_durationInput.ValueChanged += OnDurationChanged;
			spellCardInfo.AddChild(_durationInput);
			
			mainContainer.AddChild(spellCardInfo);
			
			// 轨道编辑区
			VBoxContainer trackEditor = new VBoxContainer();
			trackEditor.AddChild(new Label { Text = "轨道配置:" });
			
			// 弹幕模式选择
			HBoxContainer patternRow = new HBoxContainer();
			patternRow.AddChild(new Label { Text = "弹幕模式:" });
			_patternSelector = new OptionButton();
			_patternSelector.AddItem("直线", (int)BulletPattern.Straight);
			_patternSelector.AddItem("环形", (int)BulletPattern.Ring);
			_patternSelector.AddItem("螺旋", (int)BulletPattern.Spiral);
			_patternSelector.AddItem("波浪", (int)BulletPattern.Wave);
			_patternSelector.Selected = 0;
			_patternSelector.ItemSelected += OnPatternSelected;
			patternRow.AddChild(_patternSelector);
			trackEditor.AddChild(patternRow);
			
			// 子弹数量
			HBoxContainer bulletCountRow = new HBoxContainer();
			bulletCountRow.AddChild(new Label { Text = "子弹数量:" });
			_bulletCountInput = new SpinBox { MinValue = 1, MaxValue = 100, Value = 8 };
			_bulletCountInput.ValueChanged += OnBulletCountChanged;
			bulletCountRow.AddChild(_bulletCountInput);
			trackEditor.AddChild(bulletCountRow);
			
			// 发射间隔
			HBoxContainer fireIntervalRow = new HBoxContainer();
			fireIntervalRow.AddChild(new Label { Text = "发射间隔(秒):" });
			_fireIntervalInput = new SpinBox { MinValue = 0.1, MaxValue = 5, Step = 0.1, Value = 0.5 };
			_fireIntervalInput.ValueChanged += OnFireIntervalChanged;
			fireIntervalRow.AddChild(_fireIntervalInput);
			trackEditor.AddChild(fireIntervalRow);
			
			// 基础速度
			HBoxContainer baseSpeedRow = new HBoxContainer();
			baseSpeedRow.AddChild(new Label { Text = "基础速度:" });
			_baseSpeedInput = new SpinBox { MinValue = 10, MaxValue = 500, Value = 100 };
			_baseSpeedInput.ValueChanged += OnBaseSpeedChanged;
			baseSpeedRow.AddChild(_baseSpeedInput);
			trackEditor.AddChild(baseSpeedRow);
			
			// 速度渐变
			HBoxContainer speedGradientRow = new HBoxContainer();
			speedGradientRow.AddChild(new Label { Text = "速度渐变:" });
			_speedGradientInput = new SpinBox { MinValue = -100, MaxValue = 100, Value = 0 };
			_speedGradientInput.ValueChanged += OnSpeedGradientChanged;
			speedGradientRow.AddChild(_speedGradientInput);
			trackEditor.AddChild(speedGradientRow);
			
			// 起始角度
			HBoxContainer startAngleRow = new HBoxContainer();
			startAngleRow.AddChild(new Label { Text = "起始角度:" });
			_startAngleInput = new SpinBox { MinValue = 0, MaxValue = 360, Value = 0 };
			_startAngleInput.ValueChanged += OnStartAngleChanged;
			startAngleRow.AddChild(_startAngleInput);
			trackEditor.AddChild(startAngleRow);
			
			// 角度步长
			HBoxContainer angleStepRow = new HBoxContainer();
			angleStepRow.AddChild(new Label { Text = "角度步长:" });
			_angleStepInput = new SpinBox { MinValue = -360, MaxValue = 360, Value = 45 };
			_angleStepInput.ValueChanged += OnAngleStepChanged;
			angleStepRow.AddChild(_angleStepInput);
			trackEditor.AddChild(angleStepRow);
			
			// 相位偏移
			HBoxContainer phaseOffsetRow = new HBoxContainer();
			phaseOffsetRow.AddChild(new Label { Text = "相位偏移:" });
			_phaseOffsetInput = new SpinBox { MinValue = -360, MaxValue = 360, Value = 0 };
			_phaseOffsetInput.ValueChanged += OnPhaseOffsetChanged;
			phaseOffsetRow.AddChild(_phaseOffsetInput);
			trackEditor.AddChild(phaseOffsetRow);
			
			// 子弹颜色
			HBoxContainer colorRow = new HBoxContainer();
			colorRow.AddChild(new Label { Text = "子弹颜色:" });
			_bulletColorPicker = new ColorPicker { PresetsVisible = false };
			_bulletColorPicker.ColorChanged += OnBulletColorChanged;
			colorRow.AddChild(_bulletColorPicker);
			trackEditor.AddChild(colorRow);
			
			// 添加轨道按钮
			_addTrackButton = new Button { Text = "添加轨道" };
			_addTrackButton.Pressed += OnAddTrackPressed;
			trackEditor.AddChild(_addTrackButton);
			
			mainContainer.AddChild(trackEditor);
			
			// 轨道列表
			_tracksList = new ItemList();
			_tracksList.SizeFlagsVertical = SizeFlags.ExpandFill;
			mainContainer.AddChild(_tracksList);
			
			// 保存/加载按钮
			HBoxContainer fileButtons = new HBoxContainer();
			_saveSpellCardButton = new Button { Text = "保存符卡" };
			_saveSpellCardButton.Pressed += OnSaveSpellCardPressed;
			fileButtons.AddChild(_saveSpellCardButton);
			
			_loadSpellCardButton = new Button { Text = "加载符卡" };
			_loadSpellCardButton.Pressed += OnLoadSpellCardPressed;
			fileButtons.AddChild(_loadSpellCardButton);
			
			mainContainer.AddChild(fileButtons);
			
			// 文件对话框
			_saveDialog = new FileDialog
			{
				FileMode = FileDialog.FileModeEnum.SaveFile,
				Access = FileDialog.AccessEnum.Resources,
				Filters = new string[] { "*.tres" },
				CurrentDir = "res://data/spellcards/"
			};
			_saveDialog.FileSelected += OnSaveFileDialogSelected;
			AddChild(_saveDialog);
			
			_loadDialog = new FileDialog
			{
				FileMode = FileDialog.FileModeEnum.OpenFile,
				Access = FileDialog.AccessEnum.Resources,
				Filters = new string[] { "*.tres" },
				CurrentDir = "res://data/spellcards/"
			};
			_loadDialog.FileSelected += OnLoadFileDialogSelected;
			AddChild(_loadDialog);
			
			// 初始化当前符卡
			_currentSpellCard = new SpellCardConfig
			{
				SpellName = "新符卡",
				Duration = 10.0f,
				Tracks = new Godot.Collections.Array<BulletTrackConfig>()
			};
			
			// 初始化当前轨道
			_currentTrack = new BulletTrackConfig
			{
				Pattern = BulletPattern.Straight,
				BulletCount = 8,
				FireInterval = 0.5f,
				BaseSpeed = 100f,
				SpeedGradient = 0f,
				StartAngle = 0f,
				AngleStep = 45f,
				PhaseOffset = 0f,
				BulletColor = Colors.Red
			};
			
			// 更新UI显示
			UpdateUI();
		}
		
		/// <summary>
		/// 更新UI显示
		/// </summary>
		private void UpdateUI()
		{
			if (!Engine.IsEditorHint()) return;
			
			if (_currentSpellCard != null)
			{
				_spellCardNameInput.Text = _currentSpellCard.SpellName;
				_durationInput.Value = _currentSpellCard.Duration;
			}
			
			if (_currentTrack != null)
			{
				_patternSelector.Selected = (int)_currentTrack.Pattern;
				_bulletCountInput.Value = _currentTrack.BulletCount;
				_fireIntervalInput.Value = _currentTrack.FireInterval;
				_baseSpeedInput.Value = _currentTrack.BaseSpeed;
				_speedGradientInput.Value = _currentTrack.SpeedGradient;
				_startAngleInput.Value = _currentTrack.StartAngle;
				_angleStepInput.Value = _currentTrack.AngleStep;
				_phaseOffsetInput.Value = _currentTrack.PhaseOffset;
				_bulletColorPicker.Color = _currentTrack.BulletColor;
			}
			
			// 更新轨道列表
			_tracksList.Clear();
			if (_currentSpellCard?.Tracks != null)
			{
				for (int i = 0; i < _currentSpellCard.Tracks.Count; i++)
				{
					var track = _currentSpellCard.Tracks[i];
					_tracksList.AddItem($"轨道 {i}: {track.Pattern}, {track.BulletCount} 子弹");
				}
			}
		}
		
		// UI事件处理函数
		private void OnSpellCardNameChanged(string newText)
		{
			if (_currentSpellCard != null)
			{
				_currentSpellCard.SpellName = newText;
			}
		}
		
		private void OnDurationChanged(double newValue)
		{
			if (_currentSpellCard != null)
			{
				_currentSpellCard.Duration = (float)newValue;
			}
		}
		
		private void OnPatternSelected(long index)
		{
			if (_currentTrack != null)
			{
				_currentTrack.Pattern = (BulletPattern)index;
			}
		}
		
		private void OnBulletCountChanged(double newValue)
		{
			if (_currentTrack != null)
			{
				_currentTrack.BulletCount = (int)newValue;
			}
		}
		
		private void OnFireIntervalChanged(double newValue)
		{
			if (_currentTrack != null)
			{
				_currentTrack.FireInterval = (float)newValue;
			}
		}
		
		private void OnBaseSpeedChanged(double newValue)
		{
			if (_currentTrack != null)
			{
				_currentTrack.BaseSpeed = (float)newValue;
			}
		}
		
		private void OnSpeedGradientChanged(double newValue)
		{
			if (_currentTrack != null)
			{
				_currentTrack.SpeedGradient = (float)newValue;
			}
		}
		
		private void OnStartAngleChanged(double newValue)
		{
			if (_currentTrack != null)
			{
				_currentTrack.StartAngle = (float)newValue;
			}
		}
		
		private void OnAngleStepChanged(double newValue)
		{
			if (_currentTrack != null)
			{
				_currentTrack.AngleStep = (float)newValue;
			}
		}
		
		private void OnPhaseOffsetChanged(double newValue)
		{
			if (_currentTrack != null)
			{
				_currentTrack.PhaseOffset = (float)newValue;
			}
		}
		
		private void OnBulletColorChanged(Color color)
		{
			if (_currentTrack != null)
			{
				_currentTrack.BulletColor = color;
			}
		}
		
		private void OnAddTrackPressed()
		{
			if (_currentSpellCard != null && _currentTrack != null)
			{
				// 创建轨道副本并添加到符卡
				var newTrack = new BulletTrackConfig
				{
					Pattern = _currentTrack.Pattern,
					BulletCount = _currentTrack.BulletCount,
					FireInterval = _currentTrack.FireInterval,
					BaseSpeed = _currentTrack.BaseSpeed,
					SpeedGradient = _currentTrack.SpeedGradient,
					StartAngle = _currentTrack.StartAngle,
					AngleStep = _currentTrack.AngleStep,
					PhaseOffset = _currentTrack.PhaseOffset,
					BulletColor = _currentTrack.BulletColor
				};
				
				if (_currentSpellCard.Tracks == null)
				{
					_currentSpellCard.Tracks = new Godot.Collections.Array<BulletTrackConfig>();
				}
				
				_currentSpellCard.Tracks.Add(newTrack);
				UpdateUI();
			}
		}
		
		private void OnSaveSpellCardPressed()
		{
			_saveDialog.PopupCentered();
		}
		
		private void OnLoadSpellCardPressed()
		{
			_loadDialog.PopupCentered();
		}
		
		private void OnSaveFileDialogSelected(string path)
		{
			if (_currentSpellCard != null)
			{
				ResourceSaver.Save(_currentSpellCard, path);
				GD.Print($"符卡已保存到: {path}");
			}
		}
		
		private void OnLoadFileDialogSelected(string path)
		{
			var loadedSpellCard = ResourceLoader.Load<SpellCardConfig>(path);
			if (loadedSpellCard != null)
			{
				_currentSpellCard = loadedSpellCard;
				UpdateUI();
				GD.Print($"符卡已从 {path} 加载");
			}
		}
	}
}