using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrRay3Polygon3 : Test_Base
	{
		public Transform Ray;
		public Transform PolygonPlane;
		public Transform[] PolygonPoints;

		private void OnDrawGizmos()
		{
			Ray3 ray = CreateRay3(Ray);
			Plane3 polygonPlane = CreatePlane3(PolygonPlane);
			Polygon3 polygon = new Polygon3(PolygonPoints.Length, polygonPlane);
			for (int i = 0; i < PolygonPoints.Length; ++i)
			{
				polygon.SetVertexProjected(i, PolygonPoints[i].position);
			}
			polygon.UpdateEdges();

			Ray3Polygon3Intr info;
			bool find = Intersection.FindRay3Polygon3(ref ray, polygon, out info);

			FiguresColor();
			DrawPolygon(polygon);
			DrawRay(ref ray);

			if (find)
			{
				ResultsColor();
				DrawPoint(info.Point);
			}

			LogInfo(info.IntersectionType);
		}
	}
}
