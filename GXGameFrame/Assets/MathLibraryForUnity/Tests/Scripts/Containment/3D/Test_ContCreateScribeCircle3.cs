using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ContCreateScribeCircle3 : Test_Base
	{
		public Transform V0, V1, V2;

		private void OnDrawGizmos()
		{
			Vector3 v0 = V0.position;
			Vector3 v1 = V1.position;
			Vector3 v2 = V2.position;
			Triangle3 triangle = CreateTriangle3(V0, V1, V2);

			Circle3 circumscribed;
			bool b0 = Circle3.CreateCircumscribed(v0, v1, v2, out circumscribed);
			Circle3 inscribed;
			bool b1 = Circle3.CreateInscribed(v0, v1, v2, out inscribed);

			FiguresColor();
			DrawTriangle(ref triangle);

			ResultsColor();
			if (b0) DrawCircle(ref circumscribed);
			if (b1) DrawCircle(ref inscribed);

			LogInfo("Circumscribed: " + b0 + "   Inscribed: " + b1);
		}
	}
}
