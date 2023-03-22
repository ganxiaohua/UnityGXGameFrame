using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ConcaveHull2 : Test_Base
	{
		public Vector2[] _points;
		private int[]    _indicesConvex;
		private int[]    _indicesConcave;
		private bool     _previous;

		public bool  ToggleToGenerate;
		public float Threshold;
		public float GenerateRadius;
		public float CoeffX;
		public float CoeffY;
		public int   GenerateCountMin;
		public int   GenerateCountMax;
		public Test_Vector2Array SourcePoints;
		
		private void OnDrawGizmos()
		{
			if (_points != null)
			{
				DrawPoints(_points);

				if (_indicesConvex != null && _indicesConcave != null)
				{
					FiguresColor();
					for (int i = 0; i < _indicesConvex.Length; ++i)
					{
						DrawSegment(_points[_indicesConvex[i]], _points[_indicesConvex[(i + 1) % _indicesConvex.Length]]);
					}

					ResultsColor();
					for (int i = 0; i < _indicesConcave.Length; ++i)
					{
						DrawSegment(_points[_indicesConcave[i]], _points[_indicesConcave[(i + 1) % _indicesConcave.Length]]);
					}
				}
			}
		}

		private void OnGUI()
		{
			if (GUILayout.Button("btn"))
			{
				//_points = GenerateMemoryRandomSet2D(GenerateRadius, GenerateCountMin, GenerateCountMax, CoeffX, CoeffY);
				ConcaveHull.Create2D(_points, out _indicesConcave, out _indicesConvex, Threshold);
			}
		}

		private void Update()
		{
			if (ToggleToGenerate != _previous)
			{
				if (SourcePoints != null)
				{
					_points = SourcePoints.Array;
				}
				else
				{
					_points = GenerateMemoryRandomSet2D(GenerateRadius, GenerateCountMin, GenerateCountMax, CoeffX, CoeffY);
				}
				
				bool created = ConcaveHull.Create2D(_points, out _indicesConcave, out _indicesConvex, Threshold);
				Logger.LogInfo("Created: " + created + "   ConvexIndexCount: " + _indicesConvex.Length + "   ConcaveIndexCount: " + _indicesConcave.Length);
			}
			_previous = ToggleToGenerate;
		}
	}
}
