using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistPoint2Triangle2 : Test_Base
	{
		public Transform Point;
		public Transform V0, V1, V2;

		private void OnDrawGizmos()
		{
			Vector2 point = Point.position;
			Triangle2 triangle = CreateTriangle2(V0, V1, V2);

			Vector2 closestPoint;
			float dist = Distance.Point2Triangle2(ref point, ref triangle, out closestPoint);
			float dist1 = Distance.SqrPoint2Triangle2(ref point, ref triangle, out closestPoint);

			FiguresColor();
			DrawTriangle(ref triangle);

			ResultsColor();
			DrawPoint(closestPoint);

			LogInfo(dist + " " + Mathf.Sqrt(dist1));
		}
	}
}
