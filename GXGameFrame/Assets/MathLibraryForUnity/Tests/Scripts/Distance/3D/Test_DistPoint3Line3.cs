using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistPoint3Line3 : Test_Base
	{
		public Transform Point;
		public Transform Line;

		private void OnDrawGizmos()
		{
			Vector3 point = Point.position;
			Line3 line = CreateLine3(Line);

			Vector3 closestPoint;
			float dist0 = Distance.Point3Line3(ref point, ref line, out closestPoint);
			float dist1 = line.DistanceTo(point);

			FiguresColor();
			DrawLine(ref line);

			ResultsColor();
			DrawPoint(closestPoint);

			LogInfo(dist0 + "   " + dist1);
		}
	}
}
