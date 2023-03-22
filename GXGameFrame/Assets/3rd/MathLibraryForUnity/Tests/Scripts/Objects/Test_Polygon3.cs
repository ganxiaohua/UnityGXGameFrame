using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_Polygon3 : Test_Base
	{
		public Transform Plane;
		public Transform[] Points;

		private void OnDrawGizmos()
		{
			Plane3 plane = CreatePlane3(Plane);

			Polygon3 polygon = new Polygon3(Points.Length, plane);
			for (int i = 0; i < Points.Length; ++i)
			{
				polygon.SetVertexProjected(i, Points[i].position);
			}
			polygon.UpdateEdges();

			DrawPolygon(polygon);
		}
	}
}
