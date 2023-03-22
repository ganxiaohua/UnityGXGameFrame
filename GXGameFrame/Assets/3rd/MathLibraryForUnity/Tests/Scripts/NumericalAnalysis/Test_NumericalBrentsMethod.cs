using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_NumericalBrentsMethod : Test_Base
	{
		public enum Funcs
		{
			Func0,
			Func1,
			Func2
		}

		public Funcs FuncType;
		public float From, To;

		private float Func(float x)
		{
			switch (FuncType)
			{
				case Funcs.Func0: return x * x * x + .5f * x * x + 2;	// Converges to single root
				case Funcs.Func1: return x * x * x - 2 * x;				// Converges to one of the roots
				case Funcs.Func2: return Mathf.Exp(x);					// No roots
				default: return 0f;
			}
		}

		private void OnDrawGizmos()
		{
			FiguresColor();
			DrawFunc(Func, From, To);

			// For method to work as desired, func parameter must have different signs on left and right interval ends
			BrentsRoot root;
			if (RootFinder.BrentsMethod(Func, From, To, out root))
			{
				ResultsColor();
				DrawPoint(new Vector2(root.X, 0));

				LogInfo("Root: " + root.X);
			}
			else
			{
				LogInfo("No root");
			}
		}
	}
}
