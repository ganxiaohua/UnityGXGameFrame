using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistPoint3Sphere3 : Test_Base
	{
		public Transform Point;
		public Transform Sphere;

		private void OnDrawGizmos()
		{
			Vector3 point = Point.position;
			Sphere3 sphere = CreateSphere3(Sphere);

			Vector3 closestPoint;
			float dist = Distance.Point3Sphere3(ref point, ref sphere, out closestPoint);

			FiguresColor();
			DrawSphere(ref sphere);

			ResultsColor();
			DrawPoint(closestPoint);

			LogInfo(dist);
		}
	}
}
