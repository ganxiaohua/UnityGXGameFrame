using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistLine3Ray3 : Test_Base
	{
		public Transform Line;
		public Transform Ray;

		private void OnDrawGizmos()
		{
			Line3 line = CreateLine3(Line);
			Ray3 ray = CreateRay3(Ray);

			Vector3 closestPoint0, closestPoint1;
			float dist = Distance.Line3Ray3(ref line, ref ray, out closestPoint0, out closestPoint1);

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
