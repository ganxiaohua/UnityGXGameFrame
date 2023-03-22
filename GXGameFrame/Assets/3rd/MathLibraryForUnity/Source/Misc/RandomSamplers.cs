using UnityEngine;
using System.Collections.Generic;

namespace Dest.Math
{
	/// <summary>
	/// Random sampler which uses weights to pick a sample.
	/// </summary>
	public class WeightedSampler
	{
		protected float[] _accum;
		protected float   _total;
		protected Rand    _rand;

		protected WeightedSampler(Rand rand)
		{
			_rand = rand;
		}

		protected static int BinarySearch(float[] array, float value)
		{
			int left = 0;
			int right = array.Length - 1;
			
			while (left <= right)
			{
				int ind = left + ((right - left) >> 1);
				float val = array[ind];

				if (val == value)
				{
					return ind;
				}
				if (val < value)
				{
					left = ind + 1;
				}
				else
				{
					right = ind - 1;
				}
			}

			return left;
		}

		/// <summary>
		/// Creates sampler instance from specified weights of the data collection and using Rand.Instance randomizer.
		/// </summary>
		public WeightedSampler(float[] weights)
		{
			_rand = Rand.Instance;
			Init(weights);
		}

		/// <summary>
		/// Creates sampler instance from specified weights of the data collection and specified randomizer.
		/// </summary>
		public WeightedSampler(float[] weights, Rand rand)
		{
			_rand = rand;
			Init(weights);
		}

		protected void Init(float[] weights)
		{
			_accum = new float[weights.Length];
			_accum[0] = weights[0];
			for (int i = 1, len = weights.Length; i < len; ++i)
			{
				_accum[i] = _accum[i - 1] + weights[i];
			}
			_total = _accum[_accum.Length - 1];
		}

		/// <summary>
		/// Returns random index which can be used to access data collection.
		/// </summary>
		public int SampleIndex()
		{
			return BinarySearch(_accum, _rand.NextFloat(0f, _total));
		}
	}

	public class TrianglesSamplerBase : WeightedSampler
	{
		protected Vector3[] _vertices;

		protected TrianglesSamplerBase(Rand rand)
			: base(rand)
		{
		}

		protected float CalclTriangleArea(ref Vector3 e0, ref Vector3 e1)
		{
			float x = e0.y * e1.z - e0.z * e1.y;
			float y = e0.z * e1.x - e0.x * e1.z;
			float z = e0.x * e1.y - e0.y * e1.x;
			return 0.5f * Mathf.Sqrt(x * x + y * y + z * z);
		}
	}

	/// <summary>
	/// Sampler which uniformly samples from a surface defined by the set of indexed triangles.
	/// </summary>
	public class IndexedTrianglesSampler : TrianglesSamplerBase
	{
		protected int[] _indices;

		/// <summary>
		/// Creates sampler instance using specified vertices and indices and using Rand.Instance randomizer.
		/// </summary>
		public IndexedTrianglesSampler(Vector3[] vertices, int[] indices)
			: base(Rand.Instance)
		{
			_vertices = vertices;
			_indices = indices;
			Init();
		}

		/// <summary>
		/// Creates sampler instance using specified vertices and indices and specified randomizer.
		/// </summary>
		public IndexedTrianglesSampler(Vector3[] vertices, int[] indices, Rand rand)
			: base(rand)
		{
			_vertices = vertices;
			_indices = indices;
			Init();
		}

		/// <summary>
		/// Creates sampler instance using mesh.vertices and mesh.GetIndices(0) and using Rand.Instance randomizer.
		/// </summary>
		public IndexedTrianglesSampler(Mesh mesh)
			: base(Rand.Instance)
		{
			_vertices = mesh.vertices;
			_indices = mesh.GetIndices(0);
			Init();
		}

		/// <summary>
		/// Creates sampler instance using mesh.vertices and mesh.GetIndices(0) and specified randomizer.
		/// </summary>
		public IndexedTrianglesSampler(Mesh mesh, Rand rand)
			: base(rand)
		{
			_vertices = mesh.vertices;
			_indices = mesh.GetIndices(0);
			Init();
		}

		protected void Init()
		{
			int count = _indices.Length / 3;
			_accum = new float[count];

			int i0 = _indices[0];
			Vector3 e0 = _vertices[_indices[1]] - _vertices[i0];
			Vector3 e1 = _vertices[_indices[2]] - _vertices[i0];
			_accum[0] = CalclTriangleArea(ref e0, ref e1);
			int imul3;

			for (int i = 1; i < count; ++i)
			{
				imul3 = i * 3;
				i0 = _indices[imul3];
				e0 = _vertices[_indices[imul3 + 1]] - _vertices[i0];
				e1 = _vertices[_indices[imul3 + 2]] - _vertices[i0];
				_accum[i] = _accum[i - 1] + CalclTriangleArea(ref e0, ref e1);
			}
			_total = _accum[count - 1];
		}

		/// <summary>
		/// Samples a random point on the surface.
		/// </summary>
		public Vector3 Sample()
		{
			int index = SampleIndex() * 3;
			return _rand.InTriangle(ref _vertices[_indices[index]], ref _vertices[_indices[index + 1]], ref _vertices[_indices[index + 2]]);
		}

		/// <summary>
		/// Samples an array of random points on the surface.
		/// </summary>
		public Vector3[] SampleArray(int count)
		{
			Vector3[] result = new Vector3[count];
			for (int i = 0; i < count; ++i)
			{
				int index = SampleIndex() * 3;
				result[i] = _rand.InTriangle(ref _vertices[_indices[index]], ref _vertices[_indices[index + 1]], ref _vertices[_indices[index + 2]]);
			}
			return result;
		}

		/// <summary>
		/// Sampples an array of random points on the surface. Users can provide additional parameters for sampling.
		/// Sample map controls the chance of sampling (texture is read from r channel), where white gives maximum chance
		/// and black gives no chance. To read the texture an array of uv coordinates is used. It's also possible to
		/// explicitly set additional range of chances (so that if sample map has gradient from 0 to 1, then adding
		/// min to 0.5 will result in no samples generated in areas with color &lt;= 0.5). Notice that in this function
		/// count is upper bound of samples to generate, resulting array may have less samples due to some samples
		/// being rejected according to sample map.
		/// </summary>
		public Vector3[] SampleArray(int count, Vector2[] uvs, Texture2D sampleMap, float min = 0f, float max = 1f)
		{
			Vector3[] temp = new Vector3[count];
			int c = 0;
			for (int i = 0; i < count; ++i)
			{
				int index = SampleIndex() * 3;
				int i0 = _indices[index];
				int i1 = _indices[index + 1];
				int i2 = _indices[index + 2];
				Vector3 v0 = _vertices[i0];
				Vector3 v1 = _vertices[i1];
				Vector3 v2 = _vertices[i2];
				Vector3 vec = _rand.InTriangle(ref v0, ref v1, ref v2);
				Vector3 bary;
				Triangle3.CalcBarycentricCoords(ref vec, ref v0, ref v1, ref v2, out bary);
				Vector2 uv = uvs[i0] * bary.x + uvs[i1] * bary.y + uvs[i2] * bary.z;
				float factor = sampleMap.GetPixelBilinear(uv.x, uv.y).r;
				if (min <= factor && factor <= max && _rand.NextFloat() < factor)
				{
					temp[c] = vec;
					++c;
				}
			}
			Vector3[] result = new Vector3[temp.Length];
			System.Array.Copy(temp, result, temp.Length);
			return result;
		}
	}

	/// <summary>
	/// Sampler which uniformly samples from a surface defined by the set of non-indexed triangles.
	/// </summary>
	public class NonIndexedTrianglesSampler : TrianglesSamplerBase
	{
		/// <summary>
		/// Creates sampler instance using specified vertices and using Rand.Instance randomizer.
		/// </summary>
		public NonIndexedTrianglesSampler(Vector3[] vertices)
			: base(Rand.Instance)
		{
			_vertices = vertices;
			Init();
		}

		/// <summary>
		/// Creates sampler instance using specified vertices and specified randomizer.
		/// </summary>
		public NonIndexedTrianglesSampler(Vector3[] vertices, Rand rand)
			: base(rand)
		{
			_vertices = vertices;
			Init();
		}

		/// <summary>
		/// Creates sampler instance using mesh.vertices and using Rand.Instance randomizer.
		/// </summary>
		public NonIndexedTrianglesSampler(Mesh mesh)
			: base(Rand.Instance)
		{
			_vertices = mesh.vertices;
			Init();
		}

		/// <summary>
		/// Creates sampler instance using mesh.vertices and specified randomizer.
		/// </summary>
		public NonIndexedTrianglesSampler(Mesh mesh, Rand rand)
			: base(rand)
		{
			_vertices = mesh.vertices;
			Init();
		}

		protected void Init()
		{
			int count = _vertices.Length / 3;
			_accum = new float[count];

			Vector3 e0 = _vertices[1] - _vertices[0];
			Vector3 e1 = _vertices[2] - _vertices[0];
			_accum[0] = CalclTriangleArea(ref e0, ref e1);
			int i0;

			for (int i = 1; i < count; ++i)
			{
				i0 = i * 3;
				e0 = _vertices[i0 + 1] - _vertices[i0];
				e1 = _vertices[i0 + 2] - _vertices[i0];
				_accum[i] = _accum[i - 1] + CalclTriangleArea(ref e0, ref e1);
			}
			_total = _accum[count - 1];
		}

		/// <summary>
		/// Samples a random point on the surface.
		/// </summary>
		public Vector3 Sample()
		{
			int index = SampleIndex() * 3;
			return _rand.InTriangle(ref _vertices[index], ref _vertices[index + 1], ref _vertices[index + 2]);
		}

		/// <summary>
		/// Samples an array of random point on the surface.
		/// </summary>
		public Vector3[] SampleArray(int count)
		{
			Vector3[] result = new Vector3[count];
			for (int i = 0; i < count; ++i)
			{
				int index = SampleIndex() * 3;
				result[i] = _rand.InTriangle(ref _vertices[index], ref _vertices[index + 1], ref _vertices[index + 2]);
			}
			return result;
		}
	}
}
