using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrAAB2AAB2 : Test_Base
	{
		public Transform Box0_Point0;
		public Transform Box0_Point1;
		public Transform Box1_Point0;
		public Transform Box1_Point1;

		private void OnDrawGizmos()
		{
			AAB2 box0 = CreateAAB2(Box0_Point0, Box0_Point1);
			AAB2 box1 = CreateAAB2(Box1_Point0, Box1_Point1);

			AAB2 intr;
			bool find = Intersection.FindAAB2AAB2(ref box0, ref box1, out intr);

			Gizmos.color = Color.gray;
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
