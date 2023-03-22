using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_Rectangle3 : Test_Base
	{
		public Transform P0;
		public Transform P1;
		public Transform P2;
		public Transform P3;

		private void OnDrawGizmos()
		{
			Rectangle3 rect = Rectangle3.CreateFromCCWPoints(P0.position, P1.position, P2.position, P3.position);

			FiguresColor();
			DrawRectangle(ref rect);
		}
	}
}
