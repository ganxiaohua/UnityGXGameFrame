using UnityEngine;
using System.Collections.Generic;

namespace Dest.Math
{
	public static class PointsFilter
	{
		private class Data
		{
			private Vector3[] _points;
			private List<int>[, ,] _grid;
			private Vector3 _min;
			private Vector3 _max;
			private int _cellsX;
			private int _cellsY;
			private int _cellsZ;
			private float _cellSize;
			private float _radius;
			private Rand _rand;

			public Data(Vector3[] points, float radius, Rand rand, AAB3 aab)
			{
				_min = aab.Min;
				_max = aab.Max;
				Vector3 size = _max - _min;
				_radius = radius;

				_cellSize = radius / Mathf.Sqrt(3.0f);
				_cellsX = Mathf.CeilToInt(size.x / _cellSize);
				_cellsY = Mathf.CeilToInt(size.y / _cellSize);
				_cellsZ = Mathf.CeilToInt(size.z / _cellSize);

				_points = points;
				_grid = new List<int>[_cellsX, _cellsY, _cellsZ];
				_rand = rand;
			}

			private static int BinarySearch(List<int> array, int value)
			{
				int left = 0;
				int right = array.Count - 1;

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

				return ~left;
			}

			private static int BinarySearch(int[] array, int length, int value)
			{
				int left = 0;
				int right = length - 1;

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

			private void CalcGridIndices(ref Vector3 point, out int i, out int j, out int k)
			{
				i = (int)((point.x - _min.x) / _cellSize);
				j = (int)((point.y - _min.y) / _cellSize);
				k = (int)((point.z - _min.z) / _cellSize);
			}

			public List<int> Filter()
			{
				int i, j, k, x, y, z, n, counter, neighborIndex, indicesCount;
				int pointsCount = _points.Length;

				for (n = 0; n < pointsCount; ++n)
				{
					CalcGridIndices(ref _points[n], out i, out j, out k);
					List<int> cellList = _grid[i, j, k];
					if (cellList == null)
					{
						_grid[i, j, k] = cellList = new List<int>();
					}
					cellList.Add(n);
				}

				List<int> result = new List<int>(pointsCount);
				int[] pool0 = new int[pointsCount];
				for (i = 0; i < pointsCount; ++i)
				{
					pool0[i] = i;
				}
				int[] pool1 = new int[pointsCount];
				int[] pool = pool0;
				int poolIndex = 0;
				int[] poolNext;
				int[][] pools = { pool0, pool1 };

				float distSqr = _radius * _radius;
				int index, removeIndex, minX, minY, minZ, maxX, maxY, maxZ;
				Vector3 delta;
				int poolLength = pointsCount;
				int sizeofInt = sizeof(int);

				while (poolLength > 0)
				{
					removeIndex = _rand.NextInt(poolLength);
					index = pool[removeIndex];
					result.Add(index);
					
					poolIndex = (poolIndex + 1) & 1;
					poolNext = pools[poolIndex];
					if (removeIndex > 0) System.Buffer.BlockCopy(pool, 0, poolNext, 0, sizeofInt * removeIndex);
					int num = poolLength - removeIndex - 1;
					if (num > 0) System.Buffer.BlockCopy(pool, sizeofInt * (removeIndex + 1), poolNext, sizeofInt * removeIndex, sizeofInt * num);
					pool = poolNext;
					--poolLength;

					CalcGridIndices(ref _points[index], out i, out j, out k);
					List<int> indices = _grid[i, j, k];
					indices.Remove(index);
					if (indices.Count == 0)
					{
						_grid[i, j, k] = null;
					}

					minX = i - 2;
					if (minX < 0) minX = 0;
					minY = j - 2;
					if (minY < 0) minY = 0;
					minZ = k - 2;
					if (minZ < 0) minZ = 0;
					maxX = i + 2;
					if (maxX > _cellsX) maxX = _cellsX;
					maxY = j + 2;
					if (maxY > _cellsY) maxY = _cellsY;
					maxZ = k + 2;
					if (maxZ > _cellsZ) maxZ = _cellsZ;

					for (x = minX; x < maxX; ++x)
					{
						for (y = minY; y < maxY; ++y)
						{
							for (z = minZ; z < maxZ; ++z)
							{
								indices = _grid[x, y, z];
								if (indices != null)
								{
									counter = 0;
									indicesCount = indices.Count;

									for (n = indicesCount - 1; n >= 0; --n)
									{
										neighborIndex = indices[n];
										if (neighborIndex != -1)
										{
											delta = _points[neighborIndex] - _points[index];
											if (delta.x * delta.x + delta.y * delta.y + delta.z * delta.z < distSqr)
											{
												removeIndex = BinarySearch(pool, poolLength, neighborIndex);
												
												poolIndex = (poolIndex + 1) & 1;
												poolNext = pools[poolIndex];
												if (removeIndex > 0) System.Buffer.BlockCopy(pool, 0, poolNext, 0, sizeofInt * removeIndex);
												num = poolLength - removeIndex - 1;
												if (num > 0) System.Buffer.BlockCopy(pool, sizeofInt * (removeIndex + 1), poolNext, sizeofInt * removeIndex, sizeofInt * num);
												pool = poolNext;
												--poolLength;

												indices[n] = -1;
												++counter;
											}
										}
										else
										{
											++counter;
										}
									}

									if (indicesCount == counter)
									{
										_grid[x, y, z] = null;
									}
								}
							}
						}
					}
				}

				return result;
			}
		}

		public static List<int> DistanceFilter(Vector3[] points, AAB3 pointsAAB, float radius, Rand rand)
		{
			Data data = new Data(points, radius, rand, pointsAAB);
			return data.Filter();
		}
	}
}
