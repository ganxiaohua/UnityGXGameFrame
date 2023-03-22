using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ContPoint2AAB2 : Test_Base
	{
		public Transform Point;
		public Transform Box_Point0, Box_Point1;

		private void OnDrawGizmos()
		{
			Vector2 point = Point.position;
			AAB2 box = CreateAAB2(Box_Point0, Box_Point1);

			bool cont = box.Contains(point);

			FiguresColor();
			DrawAAB(ref box);
			if (cont) ResultsColor();
			DrawPoint(point);

			LogInfo(cont);
		}
	}
}
