using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ContCreateScribeCircle2 : Test_Base
	{
		public Transform V0, V1, V2;

		private void OnDrawGizmos()
		{
			Vector2 v0 = V0.position;
			Vector2 v1 = V1.position;
			Vector2 v2 = V2.position;
			Triangle2 triangle = CreateTriangle2(V0, V1, V2);

			Circle2 circumscribed;
			bool b0 = Circle2.CreateCircumscribed(v0, v1, v2, out circumscribed);
			Circle2 inscribed;
			bool b1 = Circle2.CreateInscribed(v0, v1, v2, out inscribed);

			FiguresColor();
			DrawTriangle(ref triangle);
			
			ResultsColor();
			if (b0) DrawCircle(ref circumscribed);
			if (b1) DrawCircle(ref inscribed);

			LogInfo("Circumscribed: " + b0 + "   Inscribed: " + b1);
		} 
	}
}
