using UnityEngine;
using System.Collections.Generic;
using Dest.Math;
using System;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_NumericalOdeSolver : Test_Base
	{
		private class OdeTask
		{
			public float              t0;
			public float              tEnd;
			public float              y0;
			public float              step;
			public OdeFunction        Func;
			public Func<float, float> ExactSolution;
			public float              ScaleY;
		}

		public enum OdeTypes
		{
			Ode0,
			Ode1,
			Ode2
		}

		private OdeTask[] _odes;

		public OdeTypes OdeType;

		public string ExactSolution = "is gray";
		public string Euler         = "is blue";
		public string Midpoint      = "is red";
		public string RungeKutta    = "is green";

		public bool DrawEuler       = true;
		public bool DrawMidpoint    = true;
		public bool DrawKungeKutta  = true;

		private void OnEnable()
		{
			_odes = new OdeTask[]
			{
				// y'(t) = y(t), y(0) = 1
				new OdeTask()
				{
					t0 = 0.0f,
					tEnd = 4.0f,
					y0 = 1.0f,
					step = 0.5f,
					Func = delegate(float t, float[] y, float[] F) { F[0] = y[0]; },
					ExactSolution = t => Mathf.Exp(t),
					ScaleY = 0.1f
				},

				// y'(t) = (cos(t))^2 - tan(t) * y, y(0) = 2
				new OdeTask()
				{
					t0 = 0.0f,
					tEnd = 4.0f,
					y0 = 2.0f,
					step = 0.3f,
					Func = delegate(float t, float[] y, float[] F) { F[0] = Mathf.Pow(Mathf.Cos(t), 2) - Mathf.Tan(t) * y[0]; },
					ExactSolution = t => (Mathf.Sin(t) + 2.0f) * Mathf.Cos(t),
					ScaleY = 0.1f
				},

				// t * y'(t) - 2 * y(t) = t^5 * sin(2*t) - t^3 + 4 * t^4
				new OdeTask()
				{
					t0 = Mathfex.Pi,
					tEnd = 7.0f,
					y0 = 3f / 2f  * Mathf.Pow(Mathfex.Pi, 4.0f),
					step = 0.3f,
					Func = (t, y, F) => F[0] = (Mathf.Pow(t, 5.0f) * Mathf.Sin(2f * t) - Mathf.Pow(t, 3.0f) + 4.0f * Mathf.Pow(t, 4.0f) + 2.0f * y[0]) / t,
					ExactSolution = t =>
						-0.25f * t * t * (-8f  * t * t  + (2 * t * t  - 1f) * Mathf.Cos(2 * t) + 4 * t - 2 * t * Mathf.Sin(2 * t) - Mathfex.Pi * 4 + 1f),
					ScaleY = 0.001f
				}
			};
		}

		private void OnDrawGizmos()
		{
			OdeTask task = _odes[(int)OdeType];
					
			// Initial value variables
			float t;
			float[] y = new float[1];

			// Get exact solution
			t = task.t0;
			List<Vector2> exactPoints = new List<Vector2>();
			while (t <= task.tEnd)
			{
				exactPoints.Add(new Vector2(t, task.ExactSolution(t) * task.ScaleY));
				t += task.step;
			}
			
			// Solve with Euler's method
			t    = task.t0;
			y[0] = task.y0;
			OdeEuler eulerSolver = new OdeEuler(1, task.step, task.Func);
			List<Vector2> eulerPoints = new List<Vector2>();
			while (t <= task.tEnd)
			{
				eulerPoints.Add(new Vector2(t, y[0] * task.ScaleY));
				eulerSolver.Update(t, y, ref t, y);
			}

			// Solve with midpoint method
			t    = task.t0;
			y[0] = task.y0;
			OdeMidpoint midpointSolver = new OdeMidpoint(1, task.step, task.Func);
			List<Vector2> midpointPoints = new List<Vector2>();
			while (t < task.tEnd)
			{
				midpointPoints.Add(new Vector2(t, y[0] * task.ScaleY));
				midpointSolver.Update(t, y, ref t, y);
			}

			// Solve with Runge-Kutta method
			t    = task.t0;
			y[0] = task.y0;
			OdeRungeKutta4 rkSolver = new OdeRungeKutta4(1, task.step, task.Func);
			List<Vector2> rkPoints = new List<Vector2>();
			while (t < task.tEnd)
			{
				rkPoints.Add(new Vector2(t, y[0] * task.ScaleY));
				rkSolver.Update(t, y, ref t, y);
			}

			// Draw results
			FiguresColor();
			DrawSegments(exactPoints.ToArray());
			if (DrawEuler)
			{
				Gizmos.color = Color.blue;
				DrawSegments(eulerPoints.ToArray());
			}
			if (DrawMidpoint)
			{
				Gizmos.color = Color.red;
				DrawSegments(midpointPoints.ToArray());
			}
			if (DrawKungeKutta)
			{
				Gizmos.color = Color.green;
				DrawSegments(rkPoints.ToArray());
			}
		}
	}
}
