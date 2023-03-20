using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistPoint3Plane3 : Test_Base
	{
		public Transform Point;
		public Transform Plane;

		private void OnDrawGizmos()
		{
			Vector3 point = Point.position;
			Plane3 plane = CreatePlane3(Plane);

			Vector3 closestPoint;
			float dist0 = Distance.Point3Plane3(ref point, ref plane, out closestPoint);
			float dist1 = plane.DistanceTo(point);

			FiguresColor();
			DrawPlane(ref plane, Plane);

			ResultsColor();
			DrawPoint(closestPoint);

			LogInfo(dist0 + "   " + dist1);
		}
	}
}
