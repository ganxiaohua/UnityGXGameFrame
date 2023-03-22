using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_Random2D : Test_Base
	{
		public enum Types
		{
			InSquare,
			OnSquare,
			InCircle,
			InRing,
			OnCircle,
		}

		private bool _previous;
		private Types _lastGenType;
		private Vector2[][] _arrays;

		public bool ToggleToGenerate;
		public Types GenType;
		public int[] Counts;
		public Vector2[] Offsets;
		public float SquareSide;
		public float CircleRadius;
		public float CircleRadiusMin;

		private void Update()
		{
			if (ToggleToGenerate != _previous)
			{
				Generate();
			}
			_previous = ToggleToGenerate;
		}

		private void Generate()
		{
			_arrays = new Vector2[Counts.Length][];
			switch (GenType)
			{
				case Types.InSquare:
					for (int i = 0; i < Counts.Length; ++i)
					{
						Vector2[] array = new Vector2[Counts[i]];
						Vector2 offset = Offsets[i];
						for (int k = 0, len = Counts[i]; k < len; ++k)
						{
							array[k] = Rand.Instance.InSquare(SquareSide) + offset;
						}
						_arrays[i] = array;
					}
					break;

				case Types.OnSquare:
					for (int i = 0; i < Counts.Length; ++i)
					{
						Vector2[] array = new Vector2[Counts[i]];
						Vector2 offset = Offsets[i];
						for (int k = 0, len = Counts[i]; k < len; ++k)
						{
							array[k] = Rand.Instance.OnSquare(SquareSide) + offset;
						}
						_arrays[i] = array;
					}
					break;

				case Types.InCircle:
					for (int i = 0; i < Counts.Length; ++i)
					{
						Vector2[] array = new Vector2[Counts[i]];
						Vector2 offset = Offsets[i];
						for (int k = 0, len = Counts[i]; k < len; ++k)
						{
							array[k] = Rand.Instance.InCircle(CircleRadius) + offset;
						}
						_arrays[i] = array;
					}
					break;

				case Types.InRing:
					for (int i = 0; i < Counts.Length; ++i)
					{
						Vector2[] array = new Vector2[Counts[i]];
						Vector2 offset = Offsets[i];
						for (int k = 0, len = Counts[i]; k < len; ++k)
						{
							array[k] = Rand.Instance.InCircle(CircleRadiusMin, CircleRadius) + offset;
						}
						_arrays[i] = array;
					}
					break;

				case Types.OnCircle:
					for (int i = 0; i < Counts.Length; ++i)
					{
						Vector2[] array = new Vector2[Counts[i]];
						Vector2 offset = Offsets[i];
						for (int k = 0, len = Counts[i]; k < len; ++k)
						{
							array[k] = Rand.Instance.OnCircle(CircleRadius) + offset;
						}
						_arrays[i] = array;
					}
					break;
			}
			_lastGenType = GenType;
		}

		private void OnDrawGizmos()
		{
			if (_arrays != null)
			{
				switch (_lastGenType)
				{
					case Types.InSquare:
						for (int i = 0; i < _arrays.Length; ++i)
						{
							Vector2[] array = _arrays[i];
							if (array != null)
							{
								DrawPoints(array);
								Gizmos.DrawWireCube(Offsets[i], Vector2ex.One * SquareSide);
							}
						}
						break;

					case Types.OnSquare:
						for (int i = 0; i < _arrays.Length; ++i)
						{
							Vector2[] array = _arrays[i];
							if (array != null)
							{
								DrawPoints(array);
							}
						}
						break;

					case Types.InCircle:
						for (int i = 0; i < _arrays.Length; ++i)
						{
							Vector2[] array = _arrays[i];
							if (array != null)
							{
								DrawPoints(array);
								DrawCircle(Offsets[i], CircleRadius);
							}
						}
						break;

					case Types.InRing:
						for (int i = 0; i < _arrays.Length; ++i)
						{
							Vector2[] array = _arrays[i];
							if (array != null)
							{
								DrawPoints(array);
								DrawCircle(Offsets[i], CircleRadiusMin);
								DrawCircle(Offsets[i], CircleRadius);
							}
						}
						break;

					case Types.OnCircle:
						for (int i = 0; i < _arrays.Length; ++i)
						{
							Vector2[] array = _arrays[i];
							if (array != null)
							{
								DrawPoints(array);
							}
						}
						break;
				}
			}
		}
	}
}
