using UnityEngine;
using System.Collections.Generic;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_Polygon2 : MonoBehaviour
	{
		public Transform[] Points;

		private void OnDrawGizmos()
		{
			List<Vector2> pointsList = new List<Vector2>();
			foreach (Transform tr in Points)
			{
				if (tr != null) pointsList.Add(tr.position);
			}
			Polygon2 pol = new Polygon2(pointsList.ToArray());

			Gizmos.color = Color.gray;

			for (int i0 = 0, i1 = pol.VertexCount - 1; i0 < pol.VertexCount; i1 = i0, i0++)
			{
				Gizmos.DrawLine(pol[i0], pol[i1]);
			}

			Orientations or;
			bool convex = pol.IsConvex(out or);
			bool hasDegen = pol.HasZeroCorners();

			Logger.LogInfo("Area: " + pol.CalcArea() + "     Per: " + pol.CalcPerimeter() + "     Convex: " + convex + "     Orient: " + or + "     ZeroCorners: " + hasDegen);
		}
	}
}
