using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrPlane3Sphere3 : Test_Base
	{
		public Transform Plane;
		public Transform Sphere;
		
		private void OnDrawGizmos()
		{
			Plane3 plane = CreatePlane3(Plane);
			Sphere3 sphere = CreateSphere3(Sphere);

			bool test = Intersection.TestPlane3Sphere3(ref plane, ref sphere);
			Plane3Sphere3Intr info;
			bool find = Intersection.FindPlane3Sphere3(ref plane, ref sphere, out info);

			FiguresColor();
			DrawPlane(ref plane, Plane);
			DrawSphere(ref sphere);

			if (find)
			{
				if (info.IntersectionType == IntersectionTypes.Other)
				{
					ResultsColor();
					DrawCircle(ref info.Circle);
				}
			}

			LogInfo(info.IntersectionType);
			if (test != find) LogError("test != find");
		}
	}
}
