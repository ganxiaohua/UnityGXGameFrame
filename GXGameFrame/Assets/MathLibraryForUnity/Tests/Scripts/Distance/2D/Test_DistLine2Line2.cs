using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistLine2Line2 : Test_Base
	{
		public Transform Line0;
		public Transform Line1;

		private void OnDrawGizmos()
		{
			Line2 line0 = CreateLine2(Line0);
			Line2 line1 = CreateLine2(Line1);

			Vector2 closestPoint0, closestPoint1;
			float dist = Distance.Line2Line2(ref line0, ref line1, out closestPoint0, out closestPoint1);

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
