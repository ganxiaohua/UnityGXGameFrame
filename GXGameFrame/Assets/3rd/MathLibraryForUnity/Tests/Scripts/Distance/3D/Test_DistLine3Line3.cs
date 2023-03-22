using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistLine3Line3 : Test_Base
	{
		public Transform Line0;
		public Transform Line1;

		private void OnDrawGizmos()
		{
			Line3 line0 = CreateLine3(Line0);
			Line3 line1 = CreateLine3(Line1);

			Vector3 closestPoint0, closestPoint1;
			float dist = Distance.Line3Line3(ref line0, ref line1, out closestPoint0, out closestPoint1);

			FiguresColor();
			DrawLine(ref line0);
			DrawLine(ref line1);

			ResultsColor();
			DrawPoint(closestPoint0);
			DrawPoint(closestPoint1);

			LogInfo(dist);
		}
	}
}
