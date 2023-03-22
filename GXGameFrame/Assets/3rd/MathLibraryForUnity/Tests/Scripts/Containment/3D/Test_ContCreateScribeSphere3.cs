using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ContCreateScribeSphere3 : Test_Base
	{
		public Transform V0, V1, V2, V3;

		private void OnDrawGizmos()
		{
			Vector3 v0 = V0.position;
			Vector3 v1 = V1.position;
			Vector3 v2 = V2.position;
			Vector3 v3 = V3.position;

			Sphere3 circumscribed;
			bool b0 = Sphere3.CreateCircumscribed(v0, v1, v2, v3, out circumscribed);
			Sphere3 inscribed;
			bool b1 = Sphere3.CreateInscribed(v0, v1, v2, v3, out inscribed);

			FiguresColor();
			DrawTetrahedron(v0, v1, v2, v3);

			ResultsColor();
			if (b0) DrawSphere(ref circumscribed);
			if (b1) DrawSphere(ref inscribed);

			LogInfo("Circumscribed: " + b0 + "   Inscribed: " + b1);
		}
	}
}
