using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrAAB3AAB3 : Test_Base
	{
		public Transform Box0_Point0;
		public Transform Box0_Point1;
		public Transform Box1_Point0;
		public Transform Box1_Point1;

		private void OnDrawGizmos()
		{
			AAB3 box0 = CreateAAB3(Box0_Point0, Box0_Point1);
			AAB3 box1 = CreateAAB3(Box1_Point0, Box1_Point1);

			AAB3 intr;
			bool find = Intersection.FindAAB3AAB3(ref box0, ref box1, out intr);

			FiguresColor();
			DrawAAB(ref box0);
			DrawAAB(ref box1);

			if (find)
			{
				ResultsColor();
				DrawAAB(ref intr);
			}

			LogInfo(find);
		}
	}
}
