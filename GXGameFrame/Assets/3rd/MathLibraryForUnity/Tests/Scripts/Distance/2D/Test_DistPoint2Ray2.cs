using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistPoint2Ray2 : Test_Base
	{
		public Transform Point;
		public Transform Ray;

		private void OnDrawGizmos()
		{
			Vector2 point = Point.position;
			Ray2 ray = CreateRay2(Ray);

			Vector2 closestPoint;
			float dist0 = Distance.Point2Ray2(ref point, ref ray, out closestPoint);
			float dist1 = ray.DistanceTo(point);

			FiguresColor();
			DrawRay(ref ray);

			ResultsColor();
			DrawPoint(closestPoint);

			LogInfo(dist0 + "   " + dist1);
		}
	}
}
