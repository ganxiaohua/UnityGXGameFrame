using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	public class Test_Random : MonoBehaviour
	{
		private Rand rand;
		private string[] _data;

		public string Readme = "Press Play To Launch";
		public int Count;
		public int IntMin;
		public int IntMax;
		public bool UseUnityRandom; // Does not affect byte, bool and color32 generation

		private void Awake()
		{
			rand = new Rand();
		}

		private void OnGUI()
		{
			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Test Int")) TestInt();
				if (GUILayout.Button("Test Int Max")) TestIntMax();
				if (GUILayout.Button("Test Int Range")) TestIntRange();
				if (GUILayout.Button("Test Float")) TestFloat();
				if (GUILayout.Button("Test Byte")) TestByte();
				if (GUILayout.Button("Test Bool")) TestBool();
				if (GUILayout.Button("Test Color32")) TestColor32();
			}
			GUILayout.EndHorizontal();

			if (_data != null)
			{
				float hei = (Screen.height - 30f) / 20f;
				float width = 100f;
				Rect r = new Rect(0f, 30f, width, hei);
				for (int i = 0; i < _data.Length; ++i)
				{
					GUI.Label(r, _data[i]);
					r.y += hei;
					if (r.y > Screen.height)
					{
						r.y = 30f;
						r.x += width;
					}
				}
			}
		}

		private void TestInt()
		{
			_data = new string[Count];
			for (int k = 0; k < Count; ++k)
			{
				var value = UseUnityRandom ? Random.Range(int.MinValue, int.MaxValue) : rand.NextInt();
				_data[k] = value.ToString();
			}
		}

		private void TestIntMax()
		{
			_data = new string[Count];
			for (int k = 0; k < Count; ++k)
			{
				var value = UseUnityRandom ? Random.Range(0, IntMax) : rand.NextInt(IntMax);
				_data[k] = value.ToString();
			}
		}

		private void TestIntRange()
		{
			_data = new string[Count];
			for (int k = 0; k < Count; ++k)
			{
				var value = UseUnityRandom ? Random.Range(IntMin, IntMax) : rand.NextInt(IntMin, IntMax);
				_data[k] = value.ToString();
			}
		}

		private void TestFloat()
		{
			_data = new string[Count];
			for (int k = 0; k < Count; ++k)
			{
				var value = UseUnityRandom ? Random.value : rand.NextFloat();
				_data[k] = value.ToString();
			}
		}

		private void TestByte()
		{
			_data = new string[Count];
			for (int k = 0; k < Count; ++k)
			{
				var value = rand.NextByte();
				_data[k] = value.ToString();
			}
		}

		private void TestBool()
		{
			_data = new string[Count];
			for (int k = 0; k < Count; ++k)
			{
				var value = rand.NextBool();
				_data[k] = value.ToString();
			}
		}

		private void TestColor32()
		{
			_data = new string[Count];
			for (int k = 0; k < Count; ++k)
			{
				var value = rand.RandomColor32Opaque();
				_data[k] = string.Format("({0},{1},{2})", value.r.ToString(), value.g.ToString(), value.b.ToString());
			}
		}	
	}
}
