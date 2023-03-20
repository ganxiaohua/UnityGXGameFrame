using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistPoint3Box3 : Test_Base
	{
		public Transform Point;
		public Transform Box;

		private void OnDrawGizmos()
		{
			Vector3 point = Point.position;
			Box3 box = CreateBox3(Box);

			Vector3 closestPoint;
			float dist = Distance.Point3Box3(ref point, ref box, out closestPoint);

			FiguresColor();
			DrawBox(ref box);

			ResultsColor();
			DrawPoint(closestPoint);

			LogInfo(dist);
		}
	}
}
