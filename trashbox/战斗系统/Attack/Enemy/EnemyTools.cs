using Godot;
using System;
using System.Collections.Generic;

namespace Enemy
{

	public class EnemyListData
	{
		public string Path { get; set; }
		public int Level{get; set; }

		public int Index{get; set; }
		
	}
	public static class EnemyTools
	{
		public static List<Vector2> startPosition=new List<Vector2>()
		{
			new Vector2(2000,540),
			new Vector2(2000,320),
			new Vector2(2000,760),
			new Vector2(2000,100),
			new Vector2(2000,980)
		};
		public static List<Vector2> endPosition=new List<Vector2>()
		{
			new Vector2(1200,540),
			new Vector2(1400,320),
			new Vector2(1400,760),
			new Vector2(1600,100),
			new Vector2(1600,980)
		};

		public static List<Vector2> EliteenemyPosition=new List<Vector2>()
		{
			new Vector2(2000,550),
			new Vector2(1400,550)
		};

		public static List<List<int>> EnemySet=new List<List<int>>()
		{
			new List<int>()
			{
				2,2,1,1,1
			},
			new List<int>()
			{
				2,2,3,3,4
			},
			new List<int>()
			{
				4,4,4,1,1
			},
			new List<int>()
			{
				1,1,2,2,3
			},
			new List<int>()
			{
				4,4,1,2,2
			},

		};
		public static List<EnemyListData> SceneList { get; private set; } = 
		new List<EnemyListData>()
		{
			new EnemyListData()
			{
				Path=@"trashbox\战斗系统\Attack\Enemy\敌人\材质丢失.tscn",
				Level=0,
				Index=3
			},
			new EnemyListData()
			{
				Path=@"trashbox\战斗系统\Attack\Enemy\敌人\待办事项.tscn",
				Level=0,
				Index=4
			},
			new EnemyListData()
			{
				Path=@"trashbox\战斗系统\Attack\Enemy\敌人\设定冲突.tscn",
				Level=1,
				Index=6
			},
			new EnemyListData()
			{
				Path=@"trashbox\战斗系统\Attack\Enemy\敌人\屎山代码.tscn",
				Level=0,
				Index=0
			},
			new EnemyListData()
			{
				Path=@"trashbox\战斗系统\Attack\Enemy\敌人\完美主义.tscn",
				Level=0,
				Index=1
			},
			new EnemyListData()
			{
				Path=@"trashbox\战斗系统\Attack\Enemy\敌人\性能瓶颈.tscn",
				Level=1,
				Index=5
			},
			new EnemyListData()
			{
				Path=@"trashbox\战斗系统\Attack\Enemy\敌人\语法错误.tscn",
				Level=0,
				Index=2
			},
			new EnemyListData()
			{
				Path=@"trashbox\战斗系统\Attack\Enemy\敌人\最终截止日期.tscn",
				Level=2,
				Index=7
			},
		};

        //生成小怪
		public static PackedScene[] GetNormalEnemy(int _index)
		{
			// 添加索引范围检查，防止越界访问
			if (_index < 0 || _index >= EnemySet.Count)
			{
				GD.PrintErr($"Invalid enemy set index: {_index}. Valid range is 0-{EnemySet.Count - 1}");
				return new PackedScene[0]; // 返回空数组而不是抛出异常
			}
			
			List<int> normalEnemy=EnemySet[_index];
			PackedScene[] normalEnemyList=new PackedScene[normalEnemy.Count];
			for(int i=0;i<normalEnemy.Count;i++)
			{
				foreach(var Enemy in SceneList)
				{
					if(Enemy.Index==normalEnemy[i])
					{
						PackedScene scene = GD.Load<PackedScene>(Enemy.Path);
						normalEnemyList[i]=scene;
					}
					
				}
			}
			return normalEnemyList;
		}
		//生成精英怪
		public static PackedScene GetEliteEnemy(int _index)
		{
			foreach(var Enemy in SceneList)
			{
				if(Enemy.Index==_index)
				{
					PackedScene scene = GD.Load<PackedScene>(Enemy.Path);
					return scene;
				}
			}
			return null;
		}

		//生成boss
		public static PackedScene GetBossEnemy()
		{
			foreach(var Enemy in SceneList)
			{
				if(Enemy.Index==7)
				{
					PackedScene scene = GD.Load<PackedScene>(Enemy.Path);
					return scene;
				}
			}
			return null;
		}
	}
}