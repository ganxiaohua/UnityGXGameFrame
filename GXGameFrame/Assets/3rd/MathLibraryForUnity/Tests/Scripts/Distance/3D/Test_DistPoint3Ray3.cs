using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistPoint3Ray3 : Test_Base
	{
		public Transform Point;
		public Transform Ray;

		private void OnDrawGizmos()
		{
			Vector3 point = Point.position;
			Ray3 ray = CreateRay3(Ray);

			Vector3 closestPoint;
			float dist0 = Distance.Point3Ray3(ref point, ref ray, out closestPoint);
			float dist1 = ray.DistanceTo(point);

			FiguresColor();
			DrawRay(ref ray);

			ResultsColor();
			DrawPoint(closestPoint);

			LogInfo(dist0 + "   " + dist1);
		}
	}
}
