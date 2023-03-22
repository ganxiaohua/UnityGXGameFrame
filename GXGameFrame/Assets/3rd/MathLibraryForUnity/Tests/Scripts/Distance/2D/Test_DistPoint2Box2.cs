using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistPoint2Box2 : Test_Base
	{
		public Transform Point;
		public Transform Box;

		private void OnDrawGizmos()
		{
			Vector2 point = Point.position;
			Box2 box = CreateBox2(Box);

			Vector2 closestPoint;
			float dist = Distance.Point2Box2(ref point, ref box, out closestPoint);

			FiguresColor();
			DrawBox(ref box);

			ResultsColor();
			DrawPoint(closestPoint);

			LogInfo(dist);
		}
	}
}
