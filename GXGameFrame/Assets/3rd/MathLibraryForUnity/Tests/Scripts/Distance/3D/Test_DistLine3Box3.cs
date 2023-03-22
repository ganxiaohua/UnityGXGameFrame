using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistLine3Box3 : Test_Base
	{
		public Transform Line;
		public Transform Box;

		private void OnDrawGizmos()
		{
			Line3 line = CreateLine3(Line);
			Box3 box = CreateBox3(Box);

			Line3Box3Dist info;
			float dist = Distance.Line3Box3(ref line, ref box, out info);

			FiguresColor();
			DrawLine(ref line);
			DrawBox(ref box);

			ResultsColor();
			DrawPoint(info.ClosestPoint0);
			DrawPoint(info.ClosestPoint1);

			LogInfo("Dist: " + dist + " LineParam: " + info.LineParameter);
		}
	}
}
