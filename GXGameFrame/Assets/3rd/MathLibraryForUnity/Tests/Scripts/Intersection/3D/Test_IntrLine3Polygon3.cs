using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrLine3Polygon3 : Test_Base
	{
		public Transform Line;
		public Transform PolygonPlane;
		public Transform[] PolygonPoints;

		private void OnDrawGizmos()
		{
			Line3 line = CreateLine3(Line);
			Plane3 polygonPlane = CreatePlane3(PolygonPlane);
			Polygon3 polygon = new Polygon3(PolygonPoints.Length, polygonPlane);
			for (int i = 0; i < PolygonPoints.Length; ++i)
			{
				polygon.SetVertexProjected(i, PolygonPoints[i].position);
			}
			polygon.UpdateEdges();

			Line3Polygon3Intr info;
			bool find = Intersection.FindLine3Polygon3(ref line, polygon, out info);

			FiguresColor();
			DrawPolygon(polygon);
			DrawLine(ref line);

			if (find)
			{
				ResultsColor();
				DrawPoint(info.Point);
			}

			LogInfo(info.IntersectionType);
		}
	}
}
