using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrSegment3Polygon3 : Test_Base
	{
		public Transform P0, P1;
		public Transform PolygonPlane;
		public Transform[] PolygonPoints;

		private void OnDrawGizmos()
		{
			Segment3 segment = CreateSegment3(P0, P1);
			Plane3 polygonPlane = CreatePlane3(PolygonPlane);
			Polygon3 polygon = new Polygon3(PolygonPoints.Length, polygonPlane);
			for (int i = 0; i < PolygonPoints.Length; ++i)
			{
				polygon.SetVertexProjected(i, PolygonPoints[i].position);
			}
			polygon.UpdateEdges();

			Segment3Polygon3Intr info;
			bool find = Intersection.FindSegment3Polygon3(ref segment, polygon, out info);

			FiguresColor();
			DrawPolygon(polygon);
			DrawSegment(ref segment);

			if (find)
			{
				ResultsColor();
				DrawPoint(info.Point);
			}

			LogInfo(info.IntersectionType);
		}
	}
}
