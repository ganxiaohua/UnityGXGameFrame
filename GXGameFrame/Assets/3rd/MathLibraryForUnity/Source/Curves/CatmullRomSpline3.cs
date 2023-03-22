using UnityEngine;
using System.Collections.Generic;

namespace Dest.Math
{
	public class CatmullRomSpline3 : SplineBase
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
					UpdateAdjacentSegments(0);
					UpdateAdjacentSegments(_data.Count - 1);
				}
			}
		}


		/// <summary>
		/// Creates empty spline.
		/// </summary>
		public static CatmullRomSpline3 Create()
		{
			return new GameObject("CatmullRomSpline3").AddComponent<CatmullRomSpline3>();
		}

		/// <summary>
		/// Creates spline from supplied points.
		/// </summary>
		public static CatmullRomSpline3 Create(IList<Vector3> points, SplineTypes type)
		{
			CatmullRomSpline3 spline = new GameObject("CatmullRomSpline3").AddComponent<CatmullRomSpline3>();
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
				for (int i = 0, len = spline.SegmentCount; i < len; ++i)
				{
					spline.UpdateSegment(i);
				}
			}
			spline._recalcSegmentsLength = true;
			return spline;
		}


		private void UpdateSegment(int index)
		{
			int segmentCount = SegmentCount;
			int lastSegment  = segmentCount - 1;
			int vertexCount  = _data.Count;

			Vector3 p1 = _data[index].Position;
			Vector3 p0, p2, p3;
			if (_type == SplineTypes.Open)
			{
				p2 = _data[index + 1].Position;
				p0 = index == 0           ? p1 : _data[index - 1].Position;
				p3 = index == lastSegment ? p2 : _data[index + 2].Position;
				//p0 = index == 0           ? (2f * p1 - p2) : _data[index - 1].Position;
				//p3 = index == lastSegment ? (2f * p2 - p1) : _data[index + 2].Position;
			}
			else
			{
				p0 = index == 0 ? _data[lastSegment].Position : _data[index - 1].Position;
				p2 = _data[(index + 1) % vertexCount].Position;
				p3 = _data[(index + 2) % vertexCount].Position;
			}

			ItemData segment = _data[index];
			segment.A = p1;
			segment.B = 0.5f * (-p0 + p2);
			segment.C = p0 - 2.5f * p1 + 2f * p2 - 0.5f * p3;
			segment.D = 0.5f * (-p0 + 3f * p1 - 3f * p2 + p3);

#if UNITY_EDITOR
			segment.EnsureRenderPointsValidity();
			segment.UpdateRenderPoints();
#endif
		}

		private void UpdateAdjacentSegments(int vertexIndex)
		{
			int vertexCount = _data.Count;
			if (vertexCount < 2) return;

			int before1 = vertexIndex - 2;
			int before0 = vertexIndex - 1;
			int after1 = vertexIndex + 1;

			if (_type == SplineTypes.Open)
			{
				int segmentCount = SegmentCount;
				if (before1 >= 0) UpdateSegment(before1);
				if (before0 >= 0) UpdateSegment(before0);
				if (vertexIndex >= 0 && vertexIndex < segmentCount) UpdateSegment(vertexIndex);
				if (after1 < segmentCount) UpdateSegment(after1);
			}
			else
			{
				if (before1 < 0) before1 += vertexCount;
				if (before0 < 0) before0 += vertexCount;
				if (vertexIndex < 0) vertexIndex += vertexCount;
				if (after1 >= vertexCount) after1 -= vertexCount;

				UpdateSegment(before1);
				UpdateSegment(before0);
				UpdateSegment(vertexIndex);
				UpdateSegment(after1);
			}
		}
		

		/// <summary>
		/// Adds vertex in the beginning of the spline.
		/// </summary>
		public override void AddVertexFirst(Vector3 position)
		{
			InsertBefore(0, position);
		}

		/// <summary>
		/// Adds vertex to the end of the spline.
		/// </summary>
		public override void AddVertexLast(Vector3 position)
		{
			ItemData vertex = new ItemData();
			vertex.Position = position;
			_data.Add(vertex);

			UpdateAdjacentSegments(_data.Count - 1);
			_recalcSegmentsLength = true;
		}

		/// <summary>
		/// Removes vertex from the spline. Valid index is [0..VertexCount-1].
		/// </summary>
		public override void RemoveVertex(int index)
		{
			_data.RemoveAt(index);
			--index;
			UpdateAdjacentSegments(index);
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

			UpdateAdjacentSegments(vertexIndex);
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

			UpdateAdjacentSegments(vertexIndex + 1);
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

			UpdateAdjacentSegments(vertexIndex);
			_recalcSegmentsLength = true;
		}


#if UNITY_EDITOR
		public override void _ForceUpdate()
		{
			for (int i = 0; i < SegmentCount; ++i)
			{
				UpdateSegment(i);
			}
		}
#endif
	}
}
