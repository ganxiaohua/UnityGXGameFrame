using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_Random3D : Test_Base
	{
		public enum Types
		{
			InCube,
			OnCube,
			InSphere,
			OnSphere,
		}

		private bool _previous;
		private Types _lastGenType;
		private Vector3[][] _arrays;

		public bool ToggleToGenerate;
		public Types GenType;
		public int[] Counts;
		public Vector3[] Offsets;
		public float CubeSide;
		public float SphereRadius;

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
			_arrays = new Vector3[Counts.Length][];
			switch (GenType)
			{
				case Types.InCube:
					for (int i = 0; i < Counts.Length; ++i)
					{
						Vector3[] array = new Vector3[Counts[i]];
						Vector3 offset = Offsets[i];
						for (int k = 0, len = Counts[i]; k < len; ++k)
						{
							array[k] = Rand.Instance.InCube(CubeSide) + offset;
						}
						_arrays[i] = array;
					}
					break;

				case Types.OnCube:
					for (int i = 0; i < Counts.Length; ++i)
					{
						Vector3[] array = new Vector3[Counts[i]];
						Vector3 offset = Offsets[i];
						for (int k = 0, len = Counts[i]; k < len; ++k)
						{
							array[k] = Rand.Instance.OnCube(CubeSide) + offset;
						}
						_arrays[i] = array;
					}
					break;

				case Types.InSphere:
					for (int i = 0; i < Counts.Length; ++i)
					{
						Vector3[] array = new Vector3[Counts[i]];
						Vector3 offset = Offsets[i];
						for (int k = 0, len = Counts[i]; k < len; ++k)
						{
							array[k] = Rand.Instance.InSphere(SphereRadius) + offset;
						}
						_arrays[i] = array;
					}
					break;

				case Types.OnSphere:
					for (int i = 0; i < Counts.Length; ++i)
					{
						Vector3[] array = new Vector3[Counts[i]];
						Vector3 offset = Offsets[i];
						for (int k = 0, len = Counts[i]; k < len; ++k)
						{
							array[k] = Rand.Instance.OnSphere(SphereRadius) + offset;
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
					case Types.InCube:
						for (int i = 0; i < _arrays.Length; ++i)
						{
							Vector3[] array = _arrays[i];
							if (array != null)
							{
								DrawPoints(array);
								Gizmos.DrawWireCube(Offsets[i], Vector3ex.One * CubeSide);
							}
						}
						break;

					case Types.OnCube:
						for (int i = 0; i < _arrays.Length; ++i)
						{
							Vector3[] array = _arrays[i];
							if (array != null)
							{
								DrawPoints(array);
							}
						}
						break;

					case Types.InSphere:
						for (int i = 0; i < _arrays.Length; ++i)
						{
							Vector3[] array = _arrays[i];
							if (array != null)
							{
								DrawPoints(array);
								Gizmos.DrawWireSphere(Offsets[i], SphereRadius);
							}
						}
						break;

					case Types.OnSphere:
						for (int i = 0; i < _arrays.Length; ++i)
						{
							Vector3[] array = _arrays[i];
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
