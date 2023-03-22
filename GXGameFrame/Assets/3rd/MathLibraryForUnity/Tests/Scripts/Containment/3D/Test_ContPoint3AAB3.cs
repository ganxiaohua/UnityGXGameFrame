using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ContPoint3AAB3 : Test_Base
	{
		public Transform Point;
		public Transform Box_Point0, Box_Point1;

		private void OnDrawGizmos()
		{
			Vector3 point = Point.position;
			AAB3 box = CreateAAB3(Box_Point0, Box_Point1);

			bool cont = box.Contains(point);

			FiguresColor();
			DrawAAB(ref box);
			if (cont) ResultsColor();
			DrawPoint(point);

			LogInfo(cont);
		}
	}
}
