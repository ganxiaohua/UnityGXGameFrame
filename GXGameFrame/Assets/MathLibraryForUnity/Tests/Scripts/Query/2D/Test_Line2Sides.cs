using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_Line2Sides : Test_Base
	{
		public Transform Line;
		public Transform Point;
		public Transform AABMin, AABMax;
		public Transform Box;
		public Transform Circle;

		private void OnDrawGizmos()
		{
			Line2   line   = CreateLine2(Line);
			Vector2 point  = Point.position;
			AAB2    aab    = CreateAAB2(AABMin, AABMax);
			Box2    box    = CreateBox2(Box);
			Circle2 circle = CreateCircle2(Circle);

			// Get side information.
			// -1 - on the negative side of the line
			//  0 - on the line or intersecting the line
			// +1 - on the positive side of the line
			int pointSide  = line.QuerySide(point);
			int aabSide    = line.QuerySide(ref aab);
			int boxSide    = line.QuerySide(ref box);
			int circleSide = line.QuerySide(ref circle);

			// true when an object is on the positive side of the line
			bool pointPos  = line.QuerySidePositive(point);
			bool aabPos    = line.QuerySidePositive(ref aab);
			bool boxPos    = line.QuerySidePositive(ref box);
			bool circlePos = line.QuerySidePositive(ref circle);

			// true when an object is on the negative side of the line
			bool pointNeg  = line.QuerySideNegative(point);
			bool aabNeg    = line.QuerySideNegative(ref aab);
			bool boxNeg    = line.QuerySideNegative(ref box);
			bool circleNeg = line.QuerySideNegative(ref circle);

			// Note that positive/negative tests are little bit more optimized than just query,
			// as they don't have separate check for 0 case.

			FiguresColor();
			DrawLine(ref line);

			SetColor(pointSide ); DrawPoint(point);
			SetColor(aabSide   ); DrawAAB(ref aab);
			SetColor(boxSide   ); DrawBox(ref box);
			SetColor(circleSide); DrawCircle(ref circle);

			LogInfo("PointSignedDistance: " + line.SignedDistanceTo(point) + " PointNeg: " + pointNeg + " PointPos: " + pointPos + "     AABNeg: " + aabNeg + " AABPos: " + aabPos + "     BoxNeg: " + boxNeg + " BoxPos: " + boxPos + "     CircleNeg: " + circleNeg + " CirclePos: " + circlePos);
		}

		private void SetColor(int side)
		{
			if (side < 0)
			{
				Gizmos.color = Color.blue;
			}
			else if (side > 0)
			{
				Gizmos.color = Color.red;
			}
			else
			{
				Gizmos.color = new Color(.4f, .4f, .4f);
			}
		}
	}
}
