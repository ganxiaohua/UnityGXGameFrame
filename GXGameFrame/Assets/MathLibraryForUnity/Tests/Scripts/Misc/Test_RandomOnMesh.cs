using UnityEngine;
using System.Collections.Generic;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_RandomOnMesh : Test_Base
	{
		private bool _previous;
		public bool ToggleToGenerate;
		public bool SavePoints2D;
		public ProjectionPlanes Projection;
		public bool SavePoints3D;

		public int Count;
		public bool DistanceFiltering;
		public float Distance;
		public float ParticleSize;
		public bool UseSpawnMap;
		public Texture2D SpawnMap;
		public float MinFactor;
		public float MaxFactor;
		public MeshFilter MeshFilter;
		public ParticleSystem Particles;

		private void Update()
		{
			if (ToggleToGenerate != _previous) Generate();
			_previous = ToggleToGenerate;
		}

		private void Generate()
		{
			Mesh m = MeshFilter.sharedMesh;
			IndexedTrianglesSampler sampler = new IndexedTrianglesSampler(m);
			Vector3[] points = UseSpawnMap ?
				sampler.SampleArray(Count, m.uv, SpawnMap, MinFactor, MaxFactor) :
				sampler.SampleArray(Count);

			string log = points.Length + " points were generated";
			if (DistanceFiltering)
			{
				List<int> indices = PointsFilter.DistanceFilter(points, MeshFilter.sharedMesh.bounds, Distance, Rand.Instance);
				Vector3[] filteredPoints = new Vector3[indices.Count];
				for (int i = 0; i < indices.Count; ++i)
				{
					filteredPoints[i] = points[indices[i]];
				}
				points = filteredPoints;
				log += ". " + indices.Count + " points left after filtering";
			}
			Logger.LogInfo(log);

			ParticleSystem.Particle[] particles = new ParticleSystem.Particle[Count];
			for (int i = 0; i < points.Length; ++i)
			{
				particles[i] = new ParticleSystem.Particle()
				{
					position = points[i],
					color = Color.blue,
					size = ParticleSize
				};
			}
			Particles.SetParticles(particles, particles.Length);

			if (SavePoints2D)
			{
				GameObject pointsObject = new GameObject("Vector2Array");
				Test_Vector2Array sc = pointsObject.AddComponent<Test_Vector2Array>();
				Vector2[] arr = new Vector2[points.Length];
				if (Projection == ProjectionPlanes.XY)
				{
					for (int i = 0, len = points.Length; i < len; ++i)
					{
						arr[i] = new Vector2(points[i].x, points[i].y);
					}
				}
				else if (Projection == ProjectionPlanes.XZ)
				{
					for (int i = 0, len = points.Length; i < len; ++i)
					{
						arr[i] = new Vector2(points[i].x, points[i].z);
					}
				}
				else
				{
					for (int i = 0, len = points.Length; i < len; ++i)
					{
						arr[i] = new Vector2(points[i].y, points[i].z);
					}
				}
				sc.Array = arr;
			}
			if (SavePoints3D)
			{
				GameObject pointsObject = new GameObject("Vector3Array");
				Test_Vector3Array sc = pointsObject.AddComponent<Test_Vector3Array>();
				sc.Array = points;
			}
		}
	}
}
