using System;
using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistRay3Ray3 : Test_Base
	{
		public Transform Ray0;
		public Transform Ray1;

		private void OnDrawGizmos()
		{
			Ray3 ray0 = CreateRay3(Ray0);
			Ray3 ray1 = CreateRay3(Ray1);

			Vector3 closestPoint0, closestPoint1;
			float dist = Distance.Ray3Ray3(ref ray0, ref ray1, out closestPoint0, out closestPoint1);

			FiguresColor();
			DrawRay(ref ray0);
			DrawRay(ref ray1);

			ResultsColor();
			DrawPoint(closestPoint0);
			DrawPoint(closestPoint1);

			LogInfo(dist);
		}
	}
}
