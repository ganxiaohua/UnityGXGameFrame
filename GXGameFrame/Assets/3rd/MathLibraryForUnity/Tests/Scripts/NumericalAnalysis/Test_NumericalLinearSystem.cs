using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	public class Test_NumericalLinearSystem : MonoBehaviour
	{
		public string Readme = "Press Play To Launch";

		private string    _textfield;
		private int       _size;
		private string[,] _A;
		private string[]  _B;
		private string[]  _X;
		private string    _message;

		private void Awake()
		{
			SetSize(2);
			_textfield = _size.ToString();
			_message = string.Empty;
		}

		private void OnGUI()
		{
			GUILayout.Label("System Size: ");
			_textfield = GUILayout.TextField(_textfield);
			if (GUILayout.Button("Set Size"))
			{
				int size;
				if (int.TryParse(_textfield, out size))
				{
					if (size > 0)
					{
						SetSize(size);
					}
				}
			}
			GUILayout.Space(20);
			GUILayout.BeginVertical();
			for (int i = 0; i < _size; ++i)
			{
				GUILayout.BeginHorizontal();
				for (int j = 0; j < _size; ++j)
				{
					_A[i, j] = GUILayout.TextField(_A[i, j], GUILayout.Width(50f));
				}
				GUILayout.Label(" ", GUILayout.Width(50f));
				_B[i] = GUILayout.TextField(_B[i], GUILayout.Width(50f));
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
			GUILayout.Space(20);
			if (GUILayout.Button("Solve System"))
			{
				SolveSystem();
			}
			GUILayout.Space(20);
			GUILayout.BeginVertical();
			for (int i = 0; i < _size; ++i)
			{
				GUILayout.TextField(_X[i], GUILayout.Width(50f));
			}
			GUILayout.EndVertical();
			GUILayout.Space(20);
			GUILayout.Label(_message);
		}

		private void SetSize(int size)
		{
			_size = size;
			_A = new string[size, size];
			_B = new string[size];
			_X = new string[size];
			for (int i = 0; i < size; ++i)
			{
				for (int j = 0; j < size; ++j)
				{
					_A[i, j] = "0";
				}
				_B[i] = "0";
				_X[i] = string.Empty;
			}
		}

		private void SolveSystem()
		{
			float[,] A = new float[_size, _size];
			float[]  B = new float[_size];
			float temp;

			for (int i = 0; i < _size; ++i)
			{
				for (int j = 0; j < _size; ++j)
				{
					if (float.TryParse(_A[i, j], out temp))
					{
						A[i, j] = temp;
					}
					else
					{
						_message = "Table contains not a number";
						return;
					}
				}

				if (float.TryParse(_B[i], out temp))
				{
					B[i] = temp;
				}
				else
				{
					_message = "Table contains not a number";
					return;
				}
			}

			float[] X;
			if (LinearSystem.Solve(A, B, out X))
			{
				for (int i = 0; i < _size; ++i)
				{
					_X[i] = X[i].ToString();
				}
				_message = "System successfuly solved";
			}
			else
			{
				_message = "System has no solution";
			}
		}
	}
}
