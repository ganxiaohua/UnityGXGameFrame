using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_SphericalCoords : Test_Base
	{
		public float Rho;
		public float Theta;
		public float Phi;

		private void OnDrawGizmos()
		{
			Sphere3 sphere = new Sphere3(Vector3ex.Zero, Rho);
			Vector3 cartesian = Mathfex.SphericalToCartesian(new Vector3(Rho, Theta, Phi));
			Vector3 spherical = Mathfex.CartesianToSpherical(cartesian);

			FiguresColor();
			DrawSphere(ref sphere);
			ResultsColor();
			DrawSegment(Vector3ex.Zero, cartesian);
			DrawPoint(cartesian);

			LogInfo("Cartesian: " + cartesian.ToStringEx() + "   Spherical: " + spherical.ToStringEx());
		}
	}
}
