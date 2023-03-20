using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ConvexHull3 : Test_Base
	{
		private Vector3[] _points;
		private int[]     _indices;
		private int       _dim;
		private bool      _previous;

		public bool  ToggleToGenerate;
		public float GenerateRadius;
		public float CoeffX;
		public float CoeffY;
		public float CoeffZ;
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
					if (_dim == 3)
					{
						for (int i = 0; i < _indices.Length; i += 3)
						{
							int i0 = i;
							int i1 = i + 1;
							int i2 = i + 2;
							DrawSegment(_points[_indices[i0]], _points[_indices[i1]]);
							DrawSegment(_points[_indices[i1]], _points[_indices[i2]]);
							DrawSegment(_points[_indices[i2]], _points[_indices[i0]]);
						}
					}
					else if (_dim == 2)
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
				}
			}
		}

		private void Update()
		{
			if (ToggleToGenerate != _previous)
			{
				_points = GenerateMemoryRandomSet3D(GenerateRadius, GenerateCountMin, GenerateCountMax, CoeffX, CoeffY, CoeffZ);

				bool created = ConvexHull.Create3D(_points, out _indices, out _dim);
				Logger.LogInfo("Created: " + created + "   Dimension: " + _dim);

				if (CreateMeshObject) CreateMesh();
			}
			_previous = ToggleToGenerate;
		}

		private void CreateMesh()
		{
			if (_points != null && _indices != null)
			{
				if (_dim == 3)
				{
					Mesh mesh = new Mesh();
					mesh.name = "Hull";
					mesh.vertices = _points;
					mesh.SetIndices(_indices, MeshTopology.Triangles, 0);

					// For rendering only
					mesh.RecalculateNormals();
					mesh.RecalculateBounds();

					// Don't forget to turn on "Textured Wire" mode for the scene window.
					GameObject go = new GameObject("ConvexHull3D");
					go.AddComponent<MeshFilter>().sharedMesh = mesh;
					go.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Diffuse"));
				}
				else if (_dim == 2)
				{
					// To close the line-strip we must duplicate first index at the end
					int[] indices = new int[_indices.Length + 1];
					System.Buffer.BlockCopy(_indices, 0, indices, 0, sizeof(int) * _indices.Length);
					indices[_indices.Length] = _indices[0];

					Mesh mesh = new Mesh();
					mesh.name = "Hull";
					mesh.vertices = _points;
					mesh.SetIndices(indices, MeshTopology.LineStrip, 0);

					GameObject go = new GameObject("ConvexHull2D");
					go.AddComponent<MeshFilter>().sharedMesh = mesh;
					go.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Unlit/Texture"));
				}
			}
		}
	}
}
