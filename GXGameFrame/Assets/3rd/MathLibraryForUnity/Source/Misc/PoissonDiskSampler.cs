using UnityEngine;
using System.Collections.Generic;

namespace Dest.Math
{
	public class PoissonDiskSampler
	{
		// http://people.cs.ubc.ca/~rbridson/docs/bridson-siggraph07-poissondisk.pdf

		private static readonly float one_div_sqrtTwo;

		private Rand          _rand;
		private List<int>     _activeList;
		private List<Vector2> _points;
		private int?[,]       _grid;
		private float         _r;
		private float         _rSquared;
		private float         _rTwo;
		private float         _rMin;

		private Vector2 _min;
		private Vector2 _max;
		private Vector2 _size;
		private float   _cellSize;
		private int     _cellsX;
		private int     _cellsY;

		public delegate float PointDelegate(ref Vector2 point);

		public PointDelegate DistanceFilter;

		public int PointsPerStep = 30;

		public int MaxPoints { get; set; }

		static PoissonDiskSampler()
		{
			one_div_sqrtTwo = 1f / Mathf.Sqrt(2.0f);
		}

		public PoissonDiskSampler(
			Rand rand, Vector2 minCorner, Vector2 maxCorner, float minDistanceOuter, float minDistanceInner = 1f)
		{
			if (minCorner.x < maxCorner.x)
			{
				_min.x = minCorner.x;
				_max.x = maxCorner.x;
			}
			else
			{
				_min.x = maxCorner.x;
				_max.x = minCorner.x;
			}
			if (minCorner.y < maxCorner.y)
			{
				_min.y = minCorner.y;
				_max.y = maxCorner.y;
			}
			else
			{
				_min.y = maxCorner.y;
				_max.y = minCorner.y;
			}

			_size = _max - _min;
			_cellSize = minDistanceOuter * one_div_sqrtTwo;
			_cellsX = Mathf.CeilToInt(_size.x / _cellSize);
			_cellsY = Mathf.CeilToInt(_size.y / _cellSize);

			_rand = rand;
			_activeList = new List<int>();
			_points = new List<Vector2>();
			_grid = new int?[_cellsX, _cellsY];
			_r = minDistanceOuter;
			_rTwo = _r * 2.0f;
			_rSquared = _r * _r;
			_rMin = minDistanceInner;

			MaxPoints = int.MaxValue;
		}

		private void CalcGridIndices(ref Vector2 point, out int i, out int j)
		{
			i = (int)((point.x - _min.x) / _cellSize);
			j = (int)((point.y - _min.y) / _cellSize);
		}

		private void InsertIntoGrid(ref Vector2 point, int index)
		{
			int cellI, cellJ;
			CalcGridIndices(ref point, out cellI, out cellJ);
			_grid[cellI, cellJ] = index;
		}

		private bool AddPoint(ref Vector2 center)
		{
			Vector2 point = _rand.InCircle(_r, _rTwo);
			point.x += center.x;
			point.y += center.y;

			float distanceSquared;
			if (DistanceFilter != null)
			{
				float factor = DistanceFilter(ref point);
				distanceSquared = Mathf.Lerp(_rMin, _r, factor);
				distanceSquared *= distanceSquared;
			}
			else
			{
				distanceSquared = _rSquared;
			}

			if (_min.x <= point.x && point.x <= _max.x &&
				_min.y <= point.y && point.y <= _max.y)
			{
				int i, j, n, m;
				CalcGridIndices(ref point, out i, out j);

				int n_min = Mathf.Max(0      , i - 2);
				int n_max = Mathf.Min(_cellsX, i + 2);
				int m_min = Mathf.Max(0      , j - 2);
				int m_max = Mathf.Min(_cellsY, j + 2);

				bool farFromPoints = true;

				for (n = n_min; n < n_max && farFromPoints; ++n)
				{
					for (m = m_min; m < m_max; ++m)
					{
						int? cell = _grid[n, m];
						if (cell.HasValue)
						{
							Vector2 neighbor = _points[cell.Value];
							Vector2 delta = new Vector2(neighbor.x - point.x, neighbor.y - point.y);
							if (delta.x * delta.x + delta.y * delta.y < distanceSquared)
							{
								farFromPoints = false;
								break;
							}
						}
					}
				}

				if (farFromPoints)
				{
					_points.Add(point);
					int index = _points.Count - 1;
					_activeList.Add(index);
					_grid[i, j] = index;
					return true;
				}
			}

			return false;
		}

		public List<Vector2> Sample()
		{
			Vector2 initialSample = new Vector2(_rand.NextFloat(_min.x, _max.x), _rand.NextFloat(_min.y, _max.y));
			InsertIntoGrid(ref initialSample, 0);
			_points.Add(initialSample);
			_activeList.Add(0);

			while (_activeList.Count != 0 && _points.Count < MaxPoints)
			{
				int index = _rand.NextInt(_activeList.Count);
				bool found = false;
				Vector2 center = _points[_activeList[index]];
				
				for (int k = 0; k < PointsPerStep; ++k)
				{
					found |= AddPoint(ref center);
				}

				if (!found)
				{
					_activeList.RemoveAt(index);
				}
			}

			return _points;
		}
	}
}
