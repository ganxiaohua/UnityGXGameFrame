using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ContPoint3Box3 : Test_Base
	{
		public Transform Point;
		public Transform Box;

		private void OnDrawGizmos()
		{
			Vector3 point = Point.position;
			Box3 box = CreateBox3(Box);

			bool cont = box.Contains(point);

			FiguresColor();
			DrawBox(ref box);
			if (cont) ResultsColor();
			DrawPoint(point);

			LogInfo(cont);
		}
	}
}
