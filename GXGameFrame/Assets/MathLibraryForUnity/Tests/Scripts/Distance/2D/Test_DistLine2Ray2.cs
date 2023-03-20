using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistLine2Ray2 : Test_Base
	{
		public Transform Line;
		public Transform Ray;

		private void OnDrawGizmos()
		{
			Line2 line = CreateLine2(Line);
			Ray2 ray = CreateRay2(Ray);

			Vector2 closestPoint0, closestPoint1;
			float dist = Distance.Line2Ray2(ref line, ref ray, out closestPoint0, out closestPoint1);

			FiguresColor();
			DrawLine(ref line);
			DrawRay(ref ray);

			ResultsColor();
			DrawPoint(closestPoint0);
			DrawPoint(closestPoint1);

			LogInfo(dist);
		}
	}
}
