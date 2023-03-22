using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_CylindricalCoords : Test_Base
	{
		public float Rho;
		public float Phi;
		public float Height;

		private void OnDrawGizmos()
		{
			Circle3 circle = new Circle3(Vector3ex.Zero, Vector3ex.UnitY, Rho);
			Vector3 cartesian = Mathfex.CylindricalToCartesian(new Vector3(Rho, Phi, Height));
			Vector3 cylindrical = Mathfex.CartesianToCylindrical(cartesian);

			FiguresColor();
			DrawCircle(ref circle);
			ResultsColor();
			DrawSegment(new Vector3(cartesian.x, 0f, cartesian.z), cartesian);
			DrawPoint(cartesian);

			LogInfo("Cartesian: " + cartesian.ToStringEx() + "   Cylindrical: " + cylindrical.ToStringEx());
		}
	}
}
