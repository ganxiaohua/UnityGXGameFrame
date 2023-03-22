using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_PolarCoords : Test_Base
	{
		public float Rho;
		public float Phi;

		private void OnDrawGizmos()
		{
			Circle2 circle = new Circle2(Vector2ex.Zero, Rho);
			Vector2 cartesian = Mathfex.PolarToCartesian(new Vector2(Rho, Phi));
			Vector2 polar = Mathfex.CartesianToPolar(cartesian);

			FiguresColor();
			DrawCircle(ref circle);
			ResultsColor();
			DrawSegment(Vector2ex.Zero, cartesian);
			DrawPoint(cartesian);

			LogInfo("Cartesian: " + cartesian.ToStringEx() + "    Polar: " + polar.ToStringEx());
		}
	}
}
