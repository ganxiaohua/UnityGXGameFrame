using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ContPoint3Sphere3 : Test_Base
	{
		public Transform Point;
		public Transform Sphere;

		private void OnDrawGizmos()
		{
			Vector3 point = Point.position;
			Sphere3 sphere = CreateSphere3(Sphere);

			bool cont = sphere.Contains(point);

			FiguresColor();
			DrawSphere(ref sphere);
			if (cont) ResultsColor();
			DrawPoint(point);

			LogInfo(cont);
		}
	}
}
