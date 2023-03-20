using UnityEngine;
using System.Collections.Generic;

namespace Dest.Math
{
	public class CubicSpline3 : SplineBase
	{
		/// <summary>
		/// Gets or set spline type.
		/// </summary>
		public override SplineTypes SplineType
		{
			get { return _type; }
			set
			{
				if (_type != value)
				{
					_type = value;
					_recalcSegmentsLength = true;
					BuildSpline();
				}
			}
		}


		/// <summary>
		/// Creates empty spline.
		/// </summary>
		public static CubicSpline3 Create()
		{
			return new GameObject("CubicSpline3").AddComponent<CubicSpline3>();
		}

		/// <summary>
		/// Creates spline from supplied points.
		/// </summary>
		public static CubicSpline3 Create(IList<Vector3> points, SplineTypes type)
		{
			CubicSpline3 spline = new GameObject("CubicSpline3").AddComponent<CubicSpline3>();
			spline._type = type;
			spline._data = new List<ItemData>(points.Count);
			for (int i = 0; i < points.Count; ++i)
			{
				ItemData item = new ItemData();
				item.Position = points[i];
				spline._data.Add(item);
			}
			if (points.Count >= 2)
			{
				spline.BuildSpline();
			}
			spline._recalcSegmentsLength = true;
			return spline;
		}


		private void BuildSpline()
		{
			int vertexCount = _data.Count;
			if (vertexCount < 2) return;

			switch (_type)
			{
				case SplineTypes.Open: CreateOpenedSpline(); break;
				case SplineTypes.Closed: CreateClosedSpline(); break;
				default: return;
			}

#if UNITY_EDITOR
			for (int i = 0, len = SegmentCount; i < len; ++i)
			{
				_data[i].EnsureRenderPointsValidity();
				_data[i].UpdateRenderPoints();
			}
#endif
		}

		private void CreateOpenedSpline()
		{
			int m = _data.Count - 1;
			float[] gamma = new float[m + 1];
			Vector3[] delta = new Vector3[m + 1];

			gamma[0] = 0.5f;
			delta[0] = 3f * (_data[1].Position - _data[0].Position) * gamma[0];
			for (int i = 1; i < m; ++i)
			{
				gamma[i] = 1f / (4f - gamma[i - 1]);
				delta[i] = (3f * (_data[i + 1].Position - _data[i - 1].Position) - delta[i - 1]) * gamma[i];
			}
			gamma[m] = 1f / (2f - gamma[m - 1]);
			delta[m] = (3f * (_data[m].Position - _data[m - 1].Position) - delta[m - 1]) * gamma[m];

			Vector3 D_i;
			Vector3 D_ip1 = delta[m];
			Vector3 positionDelta;
			Vector3 position = _data[m].Position;
			for (int i = m - 1; i >= 0; --i)
			{
				ItemData item = _data[i];
				positionDelta = position - item.Position;
				D_i = delta[i] - gamma[i] * D_ip1;

				item.A = _data[i].Position;
				item.B = D_i;
				item.C = 3f * positionDelta - 2f * D_i - D_ip1;
				item.D = -2f * positionDelta + D_i + D_ip1;

				position = item.Position;
				D_ip1 = D_i;
			}
		}

		private void CreateClosedSpline()
		{
			int n = _data.Count - 1;
			float[] w = new float[n + 1];
			float[] v = new float[n + 1];
			Vector3[] y = new Vector3[n + 1];
			Vector3[] D = new Vector3[n + 1];
			float z, G, H;
			Vector3 F;
			int k;

			w[1] = v[1] = z = 0.25f;
			y[0] = z * 3 * (_data[1].Position - _data[n].Position);
			H = 4f;
			F = 3f * (_data[0].Position - _data[n - 1].Position);
			G = 1f;
			for (k = 1; k < n; k++)
			{
				v[k + 1] = z = 1f / (4f - v[k]);
				w[k + 1] = -z * w[k];
				y[k] = z * (3f * (_data[k + 1].Position - _data[k - 1].Position) - y[k - 1]);
				H = H - G * w[k];
				F = F - G * y[k - 1];
				G = -v[k] * G;
			}
			H = H - (G + 1f) * (v[n] + w[n]);
			y[n] = F - (G + 1f) * y[n - 1];

			D[n] = y[n] / H;
			D[n - 1] = y[n - 1] - (v[n] + w[n]) * D[n];
			for (k = n - 2; k >= 0; k--)
			{
				D[k] = y[k] - v[k + 1] * D[k + 1] - w[k + 1] * D[n];
			}

			ItemData item;
			Vector3 positionDelta;
			Vector3 position = _data[n].Position;
			for (int i = n - 1; i >= 0; --i)
			{
				item = _data[i];
				positionDelta = position - item.Position;

				item.A = _data[i].Position;
				item.B = D[i];
				item.C = 3f * positionDelta - 2f * D[i] - D[i + 1];
				item.D = -2f * positionDelta + D[i] + D[i + 1];

				position = item.Position;
			}
			item = _data[n];
			positionDelta = position - item.Position;
			item.A = item.Position;
			item.B = D[n];
			item.C = 3f * positionDelta - 2f * D[n] - D[0];
			item.D = -2f * positionDelta + D[n] + D[0];
		}


		/// <summary>
		/// Adds vertex in the beginning of the spline.
		/// </summary>
		public override void AddVertexFirst(Vector3 position)
		{
			ItemData vertex = new ItemData();
			vertex.Position = position;
			_data.Insert(0, vertex);

			BuildSpline();
			_recalcSegmentsLength = true;
		}

		/// <summary>
		/// Adds vertex to the end of the spline.
		/// </summary>
		public override void AddVertexLast(Vector3 position)
		{
			ItemData vertex = new ItemData();
			vertex.Position = position;
			_data.Add(vertex);

			BuildSpline();
			_recalcSegmentsLength = true;
		}

		/// <summary>
		/// Removes vertex from the spline. Valid index is [0..VertexCount-1].
		/// </summary>
		public override void RemoveVertex(int index)
		{
			_data.RemoveAt(index);

			BuildSpline();
			_recalcSegmentsLength = true;
		}

		/// <summary>
		/// Removes all vertices.
		/// </summary>
		public override void Clear()
		{
			_data.Clear();
			_recalcSegmentsLength = true;
		}

		/// <summary>
		/// Inserts vertex before specified index. Valid index is [0..VertexCount].
		/// </summary>
		public override void InsertBefore(int vertexIndex, Vector3 position)
		{
			ItemData vertex = new ItemData();
			vertex.Position = position;
			_data.Insert(vertexIndex, vertex);

			BuildSpline();
			_recalcSegmentsLength = true;
		}

		/// <summary>
		/// Inserts vertex after specified index. Valid index is [-1..VertexCount-1]
		/// </summary>
		public override void InsertAfter(int vertexIndex, Vector3 position)
		{
			ItemData vertex = new ItemData();
			vertex.Position = position;
			_data.Insert(vertexIndex + 1, vertex);

			BuildSpline();
			_recalcSegmentsLength = true;
		}

		/// <summary>
		/// Gets vertex position. Valid index is [0..VertexCount-1].
		/// </summary>
		public override Vector3 GetVertex(int vertexIndex)
		{
			return _data[vertexIndex].Position;
		}

		/// <summary>
		/// Sets vertex position. Valid index is [0..VertexCount-1].
		/// </summary>
		public override void SetVertex(int vertexIndex, Vector3 position)
		{
			_data[vertexIndex].Position = position;

			BuildSpline();
			_recalcSegmentsLength = true;
		}


#if UNITY_EDITOR
		public override void _ForceUpdate()
		{
			BuildSpline();
		}
#endif
	}
}
