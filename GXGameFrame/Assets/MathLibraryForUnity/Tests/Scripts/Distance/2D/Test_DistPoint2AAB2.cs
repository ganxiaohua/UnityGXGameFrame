using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistPoint2AAB2 : Test_Base
	{
		public Transform Point;
		public Transform Box_Point0, Box_Point1;

		private void OnDrawGizmos()
		{
			Vector2 point = Point.position;
			AAB2 box = CreateAAB2(Box_Point0, Box_Point1);

			Vector2 closestPoint;
			float dist = Distance.Point2AAB2(ref point, ref box, out closestPoint);
			float dist1 = Distance.SqrPoint2AAB2(ref point, ref box);
			float dist2 = box.DistanceTo(point);

			FiguresColor();
			DrawAAB(ref box);

			ResultsColor();
			DrawPoint(closestPoint);

			LogInfo(dist + " " + Mathf.Sqrt(dist1) + " " + dist2);
		}
	}
}
