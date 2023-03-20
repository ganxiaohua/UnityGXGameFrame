using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ConvexHull2 : Test_Base
	{
		private Vector2[] _points;
		private int[]     _indices;
		private int       _dim;
		private bool      _previous;

		public bool  ToggleToGenerate;
		public float GenerateRadius;
		public float CoeffX;
		public float CoeffY;
		public int   GenerateCountMin;
		public int   GenerateCountMax;
		public bool  CreateMeshObject;

		private void OnDrawGizmos()
		{
			if (_points != null)
			{
				DrawPoints(_points);

				if (_indices != null)
				{
					ResultsColor();
					if (_dim == 2)
					{
						for (int i = 0; i < _indices.Length; ++i)
						{
							DrawSegment(_points[_indices[i]], _points[_indices[(i + 1) % _indices.Length]]);
						}
					}
					else if (_dim == 1)
					{
						DrawSegment(_points[_indices[0]], _points[_indices[1]]);
					}

					SetColor(Color.red);
					for (int i = 0; i < _indices.Length; ++i)
					{
						Gizmos.DrawSphere(_points[_indices[i]], 0.085f);
					}
				}
			}
		}

		private void Update()
		{
			if (ToggleToGenerate != _previous)
			{
				_points = GenerateMemoryRandomSet2D(GenerateRadius, GenerateCountMin, GenerateCountMax, CoeffX, CoeffY);

				bool created = ConvexHull.Create2D(_points, out _indices, out _dim);
				Logger.LogInfo("Created: " + created + "   Dimension: " + _dim);

				if (CreateMeshObject) CreateMesh();
			}
			_previous = ToggleToGenerate;
		}

		private void CreateMesh()
		{
			if (_points != null && _indices != null && _dim == 2)
			{
				Vector3[] points = new Vector3[_points.Length];
				for (int i = 0; i < points.Length; ++i)
				{
					points[i] = _points[i].ToVector3XY();
				}

				// To close the line-strip we must duplicate first index at the end
				int[] indices = new int[_indices.Length + 1];
				System.Buffer.BlockCopy(_indices, 0, indices, 0, sizeof(int) * _indices.Length);
				indices[_indices.Length] = _indices[0];

				Mesh mesh = new Mesh();
				mesh.name = "Hull";
				mesh.vertices = points;
				mesh.SetIndices(indices, MeshTopology.LineStrip, 0);

				GameObject go = new GameObject("ConvexHull2D");
				go.AddComponent<MeshFilter>().sharedMesh = mesh;
				go.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Unlit/Texture"));
			}
		}
	}
}
