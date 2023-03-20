using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_PoissonDiskSampler : Test_Base
	{
		private Vector2 _min;
		private Vector2 _max;
		private Vector2 _size;

		private bool _previous;
		public bool ToggleToGenerate;

		public Transform QuadRegion;
		public ParticleSystem Particles;
		public bool UseDistanceMap;
		public float DistOuter;
		public float DistInner;
		public Texture2D DistanceMap;
		public float ParticleScale;
		public int MaxPoints;

		private void Update()
		{
			if (ToggleToGenerate != _previous) Generate();
			_previous = ToggleToGenerate;
		}

		private float DistanceFactor(ref Vector2 point)
		{
			float u = (point.x - _min.x) / _size.x;
			float v = (point.y - _min.y) / _size.y;
			Color color = DistanceMap.GetPixelBilinear(u, v);
			return color.r;
		}

		private void Generate()
		{
			_max = QuadRegion.localScale.ToVector2XY() * .5f;
			_min = -_max;
			_size = _max - _min;

			// Inner distance is only used if DistanceFilter delegate is set (by sampling the texture distance is
			// changed from inner to outer using r channel of the texture).
			// Otherwise it's always outerDistance.
			PoissonDiskSampler sampler = new PoissonDiskSampler(Rand.Instance, _min, _max, DistOuter, DistInner);
			if (UseDistanceMap)
			{
				sampler.DistanceFilter = DistanceFactor;
				sampler.MaxPoints = MaxPoints;
			}
			Vector2[] points = sampler.Sample().ToArray();
			Logger.LogInfo(points.Length + " points were generated");
			
			ParticleSystem.Particle[] particles = new ParticleSystem.Particle[points.Length];
			for (int i = 0; i < points.Length; ++i)
			{
				particles[i] = new ParticleSystem.Particle()
				{
					position = points[i].ToVector3XY(),
					color = Color.blue,
					size = ParticleScale,
				};
			}
			Particles.SetParticles(particles, particles.Length);
		}
	}
}
