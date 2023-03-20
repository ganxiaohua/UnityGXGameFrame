using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistPoint3AAB3 : Test_Base
	{
		public Transform Point;
		public Transform Box_Point0, Box_Point1;

		private void OnDrawGizmos()
		{
			Vector3 point = Point.position;
			AAB3 box = CreateAAB3(Box_Point0, Box_Point1);

			Vector3 closestPoint;
			float dist = Distance.Point3AAB3(ref point, ref box, out closestPoint);
			float dist1 = Distance.SqrPoint3AAB3(ref point, ref box);
			float dist2 = box.DistanceTo(point);

			FiguresColor();
			DrawAAB(ref box);

			ResultsColor();
			DrawPoint(closestPoint);

			LogInfo(dist + " " + Mathf.Sqrt(dist1) + " " + dist2);
		}
	}
}
