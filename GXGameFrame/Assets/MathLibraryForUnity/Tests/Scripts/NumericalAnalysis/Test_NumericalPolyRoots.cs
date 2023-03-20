using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	public class Test_NumericalPolyRoots : MonoBehaviour
	{
		public string Readme = "Press Play To Launch";

		private string[] _linear;
		private string[] _quadratic;
		private string[] _cubic;
		private string[] _quartic;
		private string[] _poly;

		private string _linearMessage;
		private string _quadraticMessage;
		private string _cubicMessage;
		private string _quarticMessage;
		private string _polyMessage;

		private void Awake()
		{
			_linear    = CreateArray(1);
			_quadratic = CreateArray(2);
			_cubic     = CreateArray(3);
			_quartic   = CreateArray(4);
			_poly      = CreateArray(5);
			//_poly = new string[] { "20", "1", "-70", "-20", "14", "1" };

			_linearMessage = _quadraticMessage = _cubicMessage = _quarticMessage = _polyMessage = "Press 'Solve' to solve equation";
		}

		private void OnGUI()
		{
			GUILayout.Space(20);
			Draw(_linear   , ref _linearMessage   );
			Draw(_quadratic, ref _quadraticMessage);
			Draw(_cubic    , ref _cubicMessage    );
			Draw(_quartic  , ref _quarticMessage  );
			Draw(_poly     , ref _polyMessage     );
		}

		private void Draw(string[] coeffs, ref string message)
		{
			GUILayout.BeginHorizontal();

			var width = GUILayout.Width(35);
			for (int i = coeffs.Length - 1; i >=0 ; --i)
			{
				coeffs[i] = GUILayout.TextField(coeffs[i], width);
				GUILayout.Label(i != 0 ? "x^" + i.ToString() + " + " : " = 0", width);
			}
			GUILayout.Space(10);
			if (GUILayout.Button("Solve", GUILayout.Width(100)))
			{
				bool success = true;
				float[] array = new float[coeffs.Length];
				for (int i = 0; i < coeffs.Length; ++i)
				{
					float temp;
					if (float.TryParse(coeffs[i], out temp))
					{
						array[i] = temp;
					}
					else
					{
						message = coeffs[i] + " is not a number!";
						success = false;
						break;
					}
				}
				if (success)
				{
					Solve(array, ref message);
				}
			}
			GUILayout.Space(10);
			GUILayout.Label(message);

			GUILayout.EndHorizontal();
		}

		private string[] CreateArray(int size)
		{
			string[] result = new string[size + 1];
			for (int i = 0; i < result.Length; ++i)
			{
				result[i] = "0";
			}
			return result;
		}

		private void Solve(float[] array, ref string message)
		{
			switch (array.Length)
			{
				case 2:
					{
						float root;
						if (RootFinder.Linear(array[0], array[1], out root))
						{
							message = "X=" + root;
							return;
						}
					}
					break;

				case 3:
					{
						QuadraticRoots roots;
						if (RootFinder.Quadratic(array[0], array[1], array[2], out roots))
						{
							message = "RootCount: " + roots.RootCount + "   X0=" + roots.X0 + "   X1=" + roots.X1;
							return;
						}
					}
					break;

				case 4:
					{
						CubicRoots roots;
						if (RootFinder.Cubic(array[0], array[1], array[2], array[3], out roots))
						{
							message = "RootCount: " + roots.RootCount + "   X0=" + roots.X0 + "   X1=" + roots.X1 + "   X2=" + roots.X2;
							return;
						}
					}
					break;

				case 5:
					{
						QuarticRoots roots;
						if (RootFinder.Quartic(array[0], array[1], array[2], array[3], array[4], out roots))
						{
							message = "RootCount: " + roots.RootCount + "   X0=" + roots.X0 + "   X1=" + roots.X1 + "   X2=" + roots.X2 + "   X3=" + roots.X3;
							return;
						}
					}
					break;

				default:
					{
						int degree = array.Length - 1;
						Polynomial poly = new Polynomial(degree);
						for (int i = 0; i <= degree; ++i) // Notice i <= degree, as poly of order n has n+1 coeffs
						{
							poly[i] = array[i];
						}

						float[] roots;
						if (RootFinder.Polynomial(poly, out roots))
						{
							message = "RootCount: " + roots.Length;
							for (int i = 0; i < roots.Length; ++i)
							{
								message += "   X" + i.ToString() + "=" + roots[i].ToString();
							}
							return;
						}
					}
					break;
			}

			message = "Has no solution";
		}
	}
}
