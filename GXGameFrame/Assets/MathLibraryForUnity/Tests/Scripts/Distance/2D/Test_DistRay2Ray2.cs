using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistRay2Ray2 : Test_Base
	{
		public Transform Ray0;
		public Transform Ray1;

		private void OnDrawGizmos()
		{
			Ray2 ray0 = CreateRay2(Ray0);
			Ray2 ray1 = CreateRay2(Ray1);

			Vector2 closestPoint0, closestPoint1;
			float dist = Distance.Ray2Ray2(ref ray0, ref ray1, out closestPoint0, out closestPoint1);

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
