using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrSphere3Sphere3 : Test_Base
	{
		public Transform Sphere0;
		public Transform Sphere1;

		private void OnDrawGizmos()
		{
			Sphere3 sphere0 = CreateSphere3(Sphere0);
			Sphere3 sphere1 = CreateSphere3(Sphere1);

			bool test = Intersection.TestSphere3Sphere3(ref sphere0, ref sphere1);
			Sphere3Sphere3Intr info;
			bool find = Intersection.FindSphere3Sphere3(ref sphere0, ref sphere1, out info);

			FiguresColor();
			DrawSphere(ref sphere0);
			DrawSphere(ref sphere1);

			if (find)
			{
				ResultsColor();
				
				if (info.IntersectionType == Sphere3Sphere3IntrTypes.Same)
				{
					DrawSphere(ref sphere0);
				}
				else if (info.IntersectionType == Sphere3Sphere3IntrTypes.Point)
				{
					DrawPoint(info.ContactPoint);
				}
				else if (info.IntersectionType == Sphere3Sphere3IntrTypes.Sphere0Point)
				{
					DrawSphere(ref sphere0);
					DrawPoint(info.ContactPoint);
				}
				else if (info.IntersectionType == Sphere3Sphere3IntrTypes.Sphere1Point)
				{
					DrawSphere(ref sphere1);
					DrawPoint(info.ContactPoint);
				}
				else if (info.IntersectionType == Sphere3Sphere3IntrTypes.Sphere0)
				{
					DrawSphere(ref sphere0);
				}
				else if (info.IntersectionType == Sphere3Sphere3IntrTypes.Sphere1)
				{
					DrawSphere(ref sphere1);
				}
				else if (info.IntersectionType == Sphere3Sphere3IntrTypes.Circle)
				{
					DrawCircle(ref info.Circle);
				}
			}

			LogInfo(info.IntersectionType);
			if (test != find) LogError("test != find");
		}
	}
}
