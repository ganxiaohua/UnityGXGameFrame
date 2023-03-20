using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	public class Test_Triangle2 : MonoBehaviour
	{
		public Transform V0;
		public Transform V1;
		public Transform V2;

		public float c0;
		public float c1;

		private void OnDrawGizmos()
		{
			Triangle2 triangle = new Triangle2(V0.position, V1.position, V2.position);

			Gizmos.color = Color.gray;
			Gizmos.DrawLine(triangle.V0, triangle.V1);
			Gizmos.DrawLine(triangle.V1, triangle.V2);
			Gizmos.DrawLine(triangle.V2, triangle.V0);

			var ori = triangle.CalcOrientation();
			Vector3 angles = triangle.CalcAnglesDeg();
									
			Gizmos.color = Color.blue;
			float radius = .25f;

			Vector2 baryPoint = triangle.EvalBarycentric(c0, c1);
			Vector3 baryCoords = triangle.CalcBarycentricCoords(ref baryPoint);
			Gizmos.DrawSphere(baryPoint, radius);

			Logger.LogInfo("orientation: " + ori + "     Angles: " + angles.ToStringEx() + "    Bary: " + baryCoords.ToStringEx());
		}
	}
}
