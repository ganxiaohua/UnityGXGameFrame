using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_NumericalIntegrator : Test_Base
	{
		public enum Funcs
		{
			Func0,
			Func1,
			Func2
		}

		public Funcs FuncType;
		public float a, b;
		public int TrapezoidSamples;	// More is better
		public int RombergOrder;		// More is better

		private float Func(float x)
		{
			switch (FuncType)
			{
				case Funcs.Func0: return Mathf.Sqrt(x);
				case Funcs.Func1: return 0.1f * x * x;
				case Funcs.Func2: return Mathf.Sin(x) + 2;
			}
			return 0f;
		}

		private float Integral()
		{
			switch (FuncType)
			{
				case Funcs.Func0: return 2f * Mathf.Pow(b, 3f / 2f) / 3f - 2f * Mathf.Pow(a, 3f / 2f) / 3f;
				case Funcs.Func1: return 0.03333333333333333f * Mathf.Pow(b, 3f) - 0.03333333333333333f * Mathf.Pow(a, 3f);
				case Funcs.Func2: return 2f * b - Mathf.Cos(b) - (2f * a - Mathf.Cos(a));
			}
			return 0f;
		}

		private void OnDrawGizmos()
		{
			FiguresColor();
			DrawFunc(Func, a, b);

			LogInfo(
				     "Analytical: " + Integral() +
				"     Trapezoid: "  + Integrator.TrapezoidRule(Func, a, b, TrapezoidSamples) +
				"     Romberg: "    + Integrator.RombergIntegral(Func, a, b, RombergOrder) + 
				"     Gaussian: "   + Integrator.GaussianQuadrature(Func, a, b));
		}
	}
}
