using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistPoint3Rectangle3 : Test_Base
	{
		public Transform Point;
		public Transform Rectangle;

		private void OnDrawGizmos()
		{
			Vector3 point = Point.position;
			Rectangle3 rectangle = CreateRectangle3(Rectangle);

			Vector3 closestPoint;
			float dist = Distance.Point3Rectangle3(ref point, ref rectangle, out closestPoint);
			float dist1 = Distance.SqrPoint3Rectangle3(ref point, ref rectangle, out closestPoint);
			float dist2 = rectangle.DistanceTo(point);

			FiguresColor();
			DrawRectangle(ref rectangle);

			ResultsColor();
			DrawPoint(closestPoint);

			LogInfo(dist + " " + Mathf.Sqrt(dist1) + " " + dist2);
		}
	}
}
