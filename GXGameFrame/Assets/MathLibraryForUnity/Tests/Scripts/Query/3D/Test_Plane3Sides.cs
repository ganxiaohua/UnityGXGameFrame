using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_Plane3Sides : Test_Base
	{
		public Transform Plane;
		public Transform Point;
		public Transform AABMin, AABMax;
		public Transform Box;
		public Transform Sphere;

		private void OnDrawGizmos()
		{
			Plane3  plane  = CreatePlane3(Plane);
			Vector3 point  = Point.position;
			AAB3    aab    = CreateAAB3(AABMin, AABMax);
			Box3    box    = CreateBox3(Box);
			Sphere3 sphere = CreateSphere3(Sphere);

			// Get side information.
			// -1 - on the negative side of the plane
			//  0 - on the plane or intersecting the plane
			// +1 - on the positive side of the plane
			int pointSide  = plane.QuerySide(point);
			int aabSide    = plane.QuerySide(ref aab);
			int boxSide    = plane.QuerySide(ref box);
			int sphereSide = plane.QuerySide(ref sphere);

			// true when an object is on the positive side of the plane
			bool pointPos  = plane.QuerySidePositive(point);
			bool aabPos    = plane.QuerySidePositive(ref aab);
			bool boxPos    = plane.QuerySidePositive(ref box);
			bool spherePos = plane.QuerySidePositive(ref sphere);

			// true when an object is on the negative side of the plane
			bool pointNeg  = plane.QuerySideNegative(point);
			bool aabNeg    = plane.QuerySideNegative(ref aab);
			bool boxNeg    = plane.QuerySideNegative(ref box);
			bool sphereNeg = plane.QuerySideNegative(ref sphere);

			// Note that positive/negative tests are little bit more optimized than just query,
			// as they don't have separate check for 0 case.

			FiguresColor();
			DrawPlane(ref plane, Plane);

			SetColor(pointSide ); DrawPoint(point);
			SetColor(aabSide   ); DrawAAB(ref aab);
			SetColor(boxSide   ); DrawBox(ref box);
			SetColor(sphereSide); DrawSphere(ref sphere);

			LogInfo("PointSignedDistance: " + plane.SignedDistanceTo(point) + " PointNeg: " + pointNeg + " PointPos: " + pointPos + "     AABNeg: " + aabNeg + " AABPos: " + aabPos + "     BoxNeg: " + boxNeg + " BoxPos: " + boxPos + "     SphereNeg: " + sphereNeg + " SpherePos: " + spherePos);
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
