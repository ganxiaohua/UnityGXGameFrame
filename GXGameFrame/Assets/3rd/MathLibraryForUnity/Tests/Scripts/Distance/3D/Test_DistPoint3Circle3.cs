using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistPoint3Circle3 : Test_Base
	{
		public Transform Point;
		public Transform Circle;

		private void OnDrawGizmos()
		{
			Vector3 point = Point.position;
			Circle3 circle = CreateCircle3(Circle);

			Vector3 closestPoint;
			float dist = Distance.Point3Circle3(ref point, ref circle, out closestPoint);
			float dist1 = Distance.SqrPoint3Circle3(ref point, ref circle);
			float dist2 = circle.DistanceTo(point);

			FiguresColor();
			DrawCircle(ref circle);

			ResultsColor();
			DrawPoint(closestPoint);

			LogInfo(dist + " " + Mathf.Sqrt(dist1) + " " + dist2);
		}
	}
}
