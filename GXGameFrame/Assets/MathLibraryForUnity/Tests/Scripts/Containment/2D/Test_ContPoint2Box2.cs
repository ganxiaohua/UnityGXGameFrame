using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ContPoint2Box2 : Test_Base
	{
		public Transform Point;
		public Transform Box;

		private void OnDrawGizmos()
		{
			Vector2 point = Point.position;
			Box2 box = CreateBox2(Box);

			bool cont = box.Contains(point);

			FiguresColor();
			DrawBox(ref box);
			if (cont) ResultsColor();
			DrawPoint(point);

			LogInfo(cont);
		}
	}
}
