using UnityEngine;
using System.Collections.Generic;
using Dest.Math;

namespace Dest.Math.Tests
{
	public class Test_Base : MonoBehaviour
	{
		private const float _pointRadius = .11f;
		private const float _lineLength = 100f;


		protected void LogInfo(object value)
		{
			Logger.LogInfo(value);
		}

		protected void LogWarning(object value)
		{
			Logger.LogWarning(value);
		}

		protected void LogError(object value)
		{
			Logger.LogError(value);
		}


		protected void SetColor(Color color)
		{
			Gizmos.color = color;
		}

		protected void FiguresColor()
		{
			Gizmos.color = Color.gray;
		}

		protected void ResultsColor()
		{
			Gizmos.color = Color.blue;
		}


		protected AAB2 CreateAAB2(Transform point0, Transform point1)
		{
			// Creates aab from two unsorted points, if you know min and max use constructor
			return AAB2.CreateFromTwoPoints(point0.position, point1.position);
		}

		protected AAB3 CreateAAB3(Transform point0, Transform point1)
		{
			// Creates aab from two unsorted points, if you know min and max use constructor
			return AAB3.CreateFromTwoPoints(point0.position, point1.position);
		}

		protected Box2 CreateBox2(Transform box)
		{
			return new Box2(box.position, box.right, box.up, box.localScale);
		}

		protected Box3 CreateBox3(Transform box)
		{
			return new Box3(box.position, box.right, box.up, box.forward, box.localScale);
		}

		protected Rectangle3 CreateRectangle3(Transform rectangle)
		{
			return new Rectangle3(rectangle.position, rectangle.right, rectangle.forward, rectangle.localScale);
		}

		protected Circle2 CreateCircle2(Transform circle)
		{
			return new Circle2(circle.position, circle.localScale.x);
		}

		protected Circle3 CreateCircle3(Transform circle)
		{
			return new Circle3(circle.position, circle.right, circle.forward, circle.localScale.x);
		}

		protected Sphere3 CreateSphere3(Transform sphere)
		{
			return new Sphere3(sphere.position, sphere.localScale.x);
		}

		protected Line2 CreateLine2(Transform line)
		{
			return new Line2(line.position, line.right);
		}

		protected Line3 CreateLine3(Transform line)
		{
			return new Line3(line.position, line.right);
		}

		protected Ray2 CreateRay2(Transform ray)
		{
			return new Ray2(ray.position, ray.right);
		}

		protected Ray3 CreateRay3(Transform ray)
		{
			return new Ray3(ray.position, ray.right);
		}

		protected Segment2 CreateSegment2(Transform p0, Transform p1)
		{
			return new Segment2(p0.position, p1.position);
		}

		protected Segment3 CreateSegment3(Transform p0, Transform p1)
		{
			return new Segment3(p0.position, p1.position);
		}

		protected Triangle2 CreateTriangle2(Transform v0, Transform v1, Transform v2)
		{
			return new Triangle2(v0.position, v1.position, v2.position);
		}

		protected Triangle3 CreateTriangle3(Transform v0, Transform v1, Transform v2)
		{
			return new Triangle3(v0.position, v1.position, v2.position);
		}

		protected Plane3 CreatePlane3(Transform plane)
		{
			return new Plane3(plane.up, plane.position);
		}

		protected Polygon2 CreatePolygon2(Transform[] polygon)
		{
			Polygon2 result = new Polygon2(polygon.Length);
			for (int i = 0; i < polygon.Length; ++i)
			{
				result[i] = polygon[i].position.ToVector2XY();
			}
			result.UpdateEdges();
			return result;
		}

		protected Capsule3 CreateCapsule3(Transform p0, Transform p1, float radius)
		{
			return new Capsule3(new Segment3(p0.position, p1.position), radius);
		}


		protected Vector2[] GenerateRandomSet2D(float setRadius, int countMin, int countMax, float coeffX = 1f, float coeffY = 1f)
		{
			Transform thisTransform = transform;
			while (thisTransform.childCount != 0)
			{
				Transform child = thisTransform.GetChild(0);
				GameObject.DestroyImmediate(child.gameObject);
			}

			int genCount = Random.Range(countMin, countMax);
			Vector2[] points = new Vector2[genCount];
			for (int i = 0; i < genCount; ++i)
			{
				GameObject child = new GameObject("Point" + i.ToString());
				Transform childTransform = child.transform;
				childTransform.parent = thisTransform;
				Vector2 point = Random.insideUnitCircle * setRadius;
				point.x *= coeffX;
				point.y *= coeffY;
				childTransform.position = point;
				points[i] = point;
			}
			return points;
		}

		protected Vector3[] GenerateRandomSet3D(float setRadius, int countMin, int countMax, float coeffX = 1f, float coeffY = 1f, float coeffZ = 1f)
		{
			Transform thisTransform = transform;
			while (thisTransform.childCount != 0)
			{
				Transform child = thisTransform.GetChild(0);
				GameObject.DestroyImmediate(child.gameObject);
			}

			int genCount = Random.Range(countMin, countMax);
			Vector3[] points = new Vector3[genCount];
			for (int i = 0; i < genCount; ++i)
			{
				GameObject child = new GameObject("Point" + i.ToString());
				Transform childTransform = child.transform;
				childTransform.parent = thisTransform;
				Vector3 point = Random.insideUnitSphere * setRadius;
				childTransform.position = point;
				point.x *= coeffX;
				point.y *= coeffY;
				point.z *= coeffZ;
				points[i] = point;
			}
			return points;
		}

		protected Vector2[] GenerateMemoryRandomSet2D(float setRadius, int countMin, int countMax, float coeffX = 1f, float coeffY = 1f)
		{
			int genCount = Random.Range(countMin, countMax);
			Vector2[] points = new Vector2[genCount];
			for (int i = 0; i < genCount; ++i)
			{
				Vector2 point = Random.insideUnitSphere * setRadius;
				point.x *= coeffX;
				point.y *= coeffY;
				points[i] = point;
			}
			return points;
		}

		protected Vector3[] GenerateMemoryRandomSet3D(float setRadius, int countMin, int countMax, float coeffX = 1f, float coeffY = 1f, float coeffZ = 1f)
		{
			int genCount = Random.Range(countMin, countMax);
			Vector3[] points = new Vector3[genCount];
			for (int i = 0; i < genCount; ++i)
			{
				Vector3 point = Random.insideUnitSphere * setRadius;
				point.x *= coeffX;
				point.y *= coeffY;
				point.z *= coeffZ;
				points[i] = point;
			}
			return points;
		}

		protected Vector2[] CreatePoints2(Transform[] points)
		{
			Vector2[] result = new Vector2[points.Length];
			for (int i = 0; i < points.Length; ++i)
			{
				result[i] = points[i].transform.position;
			}
			return result;
		}

		protected Vector3[] CreatePoints3(Transform[] points)
		{
			Vector3[] result = new Vector3[points.Length];
			for (int i = 0; i < points.Length; ++i)
			{
				result[i] = points[i].transform.position;
			}
			return result;
		}

		protected Vector3[] CreateFromChildren3(Transform parent)
		{
			Vector3[] result = new Vector3[parent.childCount];
			for (int i = 0; i < result.Length; ++i)
			{
				result[i] = parent.GetChild(i).position;
			}
			return result;
		}


		protected void DrawPoint(Vector2 position)
		{
			Gizmos.DrawSphere(position, _pointRadius);
		}

		protected void DrawPoint(Vector3 position)
		{
			Gizmos.DrawSphere(position, _pointRadius);
		}

		protected void DrawPoints(IEnumerable<Vector2> points, float size = .11f)
		{
			foreach (Vector2 point in points)
			{
				Gizmos.DrawSphere(point, size);
			}
		}

		protected void DrawPoints(IEnumerable<Vector3> points, float size = .11f)
		{
			foreach (Vector3 point in points)
			{
				Gizmos.DrawSphere(point, size);
			}
		}

		protected void DrawWirePoints(IList<Vector3> points, float size = 0.05f)
		{
			for (int i = 0, len = points.Count; i < len; ++i)
			{
				Gizmos.DrawWireSphere(points[i], size);
			}
		}

		protected void DrawSegments(Vector2[] points)
		{
			for (int i = 0; i < points.Length - 1; ++i)
			{
				DrawSegment(points[i], points[i + 1]);
			}
		}

		protected void DrawPointsWithSegments(Vector2[] points)
		{
			for (int i = 0; i < points.Length - 1; ++i)
			{
				DrawPoint(points[i]);
				DrawSegment(points[i], points[i + 1]);
			}
			DrawPoint(points[points.Length - 1]);
		}

		protected void DrawSegment(Vector2 p0, Vector2 p1)
		{
			Gizmos.DrawLine(p0, p1);
		}

		protected void DrawSegment(Vector3 p0, Vector3 p1)
		{
			Gizmos.DrawLine(p0, p1);
		}

		protected void DrawAAB(ref AAB2 box)
		{
			Vector2 v0, v1, v2, v3;
			box.CalcVertices(out v0, out v1, out v2, out v3);
			Gizmos.DrawLine(v0, v1);
			Gizmos.DrawLine(v1, v2);
			Gizmos.DrawLine(v2, v3);
			Gizmos.DrawLine(v3, v0);
		}

		protected void DrawAAB(ref AAB3 box)
		{
			Vector3 v0, v1, v2, v3, v4, v5, v6, v7;
			box.CalcVertices(out v0, out v1, out v2, out v3, out v4, out v5, out v6, out v7);
			Gizmos.DrawLine(v0, v1);
			Gizmos.DrawLine(v1, v2);
			Gizmos.DrawLine(v2, v3);
			Gizmos.DrawLine(v3, v0);
			Gizmos.DrawLine(v4, v5);
			Gizmos.DrawLine(v5, v6);
			Gizmos.DrawLine(v6, v7);
			Gizmos.DrawLine(v7, v4);
			Gizmos.DrawLine(v0, v4);
			Gizmos.DrawLine(v1, v5);
			Gizmos.DrawLine(v2, v6);
			Gizmos.DrawLine(v3, v7);
		}

		protected void DrawBox(ref Box2 box)
		{
			Vector2 v0, v1, v2, v3;
			box.CalcVertices(out v0, out v1, out v2, out v3);
			Gizmos.DrawLine(v0, v1);
			Gizmos.DrawLine(v1, v2);
			Gizmos.DrawLine(v2, v3);
			Gizmos.DrawLine(v3, v0);
		}

		protected void DrawBox(ref Box3 box)
		{
			Vector3 v0, v1, v2, v3, v4, v5, v6, v7;
			box.CalcVertices(out v0, out v1, out v2, out v3, out v4, out v5, out v6, out v7);
			Gizmos.DrawLine(v0, v1);
			Gizmos.DrawLine(v1, v2);
			Gizmos.DrawLine(v2, v3);
			Gizmos.DrawLine(v3, v0);
			Gizmos.DrawLine(v4, v5);
			Gizmos.DrawLine(v5, v6);
			Gizmos.DrawLine(v6, v7);
			Gizmos.DrawLine(v7, v4);
			Gizmos.DrawLine(v0, v4);
			Gizmos.DrawLine(v1, v5);
			Gizmos.DrawLine(v2, v6);
			Gizmos.DrawLine(v3, v7);
		}

		protected void DrawRectangle(ref Rectangle3 rectangle)
		{
			Vector3 v0, v1, v2, v3;
			rectangle.CalcVertices(out v0, out v1, out v2, out v3);
			Gizmos.DrawLine(v0, v1);
			Gizmos.DrawLine(v1, v2);
			Gizmos.DrawLine(v2, v3);
			Gizmos.DrawLine(v3, v0);
		}

		protected void DrawCircle(ref Circle2 circle)
		{
			int count = 40;
			float delta = 2f * Mathf.PI / count;
			Vector3 prev = circle.Eval(0);
			for (int i = 1; i <= count; ++i)
			{
				Vector3 curr = circle.Eval(i * delta);
				Gizmos.DrawLine(prev, curr);
				prev = curr;
			}
		}

		protected void DrawCircle(Vector2 center, float radius)
		{
			Circle2 circle = new Circle2(ref center, radius);
			int count = 40;
			float delta = 2f * Mathf.PI / count;
			Vector3 prev = circle.Eval(0);
			for (int i = 1; i <= count; ++i)
			{
				Vector3 curr = circle.Eval(i * delta);
				Gizmos.DrawLine(prev, curr);
				prev = curr;
			}
		}

		protected void DrawCircle(ref Circle3 circle, int count = 20)
		{
			float delta = 2f * Mathf.PI / count;
			Vector3 prev = circle.Eval(0);
			for (int i = 1; i <= count; ++i)
			{
				Vector3 curr = circle.Eval(i * delta);
				Gizmos.DrawLine(prev, curr);
				prev = curr;
			}
		}

		protected void DrawSphere(ref Sphere3 sphere)
		{
			Gizmos.DrawWireSphere(sphere.Center, sphere.Radius);
		}

		protected void DrawLine(ref Line2 line)
		{
			Gizmos.DrawLine(line.Center - line.Direction * _lineLength, line.Center + line.Direction * _lineLength);
		}

		protected void DrawLine(ref Line3 line)
		{
			Gizmos.DrawLine(line.Center - line.Direction * _lineLength, line.Center + line.Direction * _lineLength);
		}

		protected void DrawRay(ref Ray2 ray)
		{
			Gizmos.DrawLine(ray.Center, ray.Center + ray.Direction * _lineLength);
		}

		protected void DrawRay(ref Ray3 ray)
		{
			Gizmos.DrawLine(ray.Center, ray.Center + ray.Direction * _lineLength);
		}
		
		protected void DrawSegment(ref Segment2 segment)
		{
			Gizmos.DrawLine(segment.P0, segment.P1);
		}

		protected void DrawSegment(ref Segment3 segment)
		{
			Gizmos.DrawLine(segment.P0, segment.P1);
		}

		protected void DrawTriangle(ref Triangle2 triangle)
		{
			Gizmos.DrawLine(triangle.V0, triangle.V1);
			Gizmos.DrawLine(triangle.V1, triangle.V2);
			Gizmos.DrawLine(triangle.V2, triangle.V0);
		}

		protected void DrawTriangle(ref Triangle3 triangle)
		{
			Gizmos.DrawLine(triangle.V0, triangle.V1);
			Gizmos.DrawLine(triangle.V1, triangle.V2);
			Gizmos.DrawLine(triangle.V2, triangle.V0);
		}

		protected void DrawPlane(ref Plane3 plane, Transform Plane)
		{
			Vector3 u, v, n;
			plane.CreateOrthonormalBasis(out u, out v, out n);
			Matrix4x4 m = new Matrix4x4();
			m.SetColumn(0, u);
			m.SetColumn(1, n);
			m.SetColumn(2, v);
			m.SetColumn(3, Plane.position);
			m.m33 = 1f;
			Gizmos.matrix = m;
			Gizmos.DrawCube(Vector3.zero, new Vector3(10, 0, 10));
			Gizmos.matrix = Matrix4x4.identity;
		}

		protected void DrawPolygon(Polygon2 polygon)
		{
			for (int i0 = 0, i1 = polygon.VertexCount - 1; i0 < polygon.VertexCount; i1 = i0, ++i0)
			{
				Gizmos.DrawLine(polygon[i0], polygon[i1]);
			}
		}

		protected void DrawPolygon(Polygon3 polygon)
		{
			for (int i0 = 0, i1 = polygon.VertexCount - 1; i0 < polygon.VertexCount; i1 = i0, ++i0)
			{
				Gizmos.DrawLine(polygon[i0], polygon[i1]);
			}
		}

		protected void DrawFunc(System.Func<float, float> func, float from, float to, int count = 100)
		{
			Vector2 prev = new Vector2(from, func(from));
			//Gizmos.DrawSphere(prev, _pointRadius);
			float delta = (to - from) / count;
			for (int i = 1; i <= count; ++i)
			{
				float x = from + i * delta;
				float y = func(x);
				Vector2 curr = new Vector2(x, y);
				Gizmos.DrawLine(prev, curr);
				//Gizmos.DrawSphere(curr, _pointRadius);
				prev = curr;
			}
		}

		protected void DrawTetrahedron(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
		{
			DrawSegment(v0, v1);
			DrawSegment(v0, v2);
			DrawSegment(v0, v3);
			DrawSegment(v1, v2);
			DrawSegment(v1, v3);
			DrawSegment(v2, v3);
		}

		protected void DrawCapsule(ref Capsule3 capsule)
		{
			Vector3 axis = capsule.Segment.Direction;
			ProjectionPlanes projPlane = axis.GetProjectionPlane();
			Vector3 side = projPlane == ProjectionPlanes.YZ ? Vector3ex.UnitZ : Vector3ex.UnitX;
			side = axis.Cross(ref side);
			Vector3 side1 = side.Cross(ref axis);

			Vector3 p0 = capsule.Segment.P0;
			Vector3 p1 = capsule.Segment.P1;
			Vector3 offset = side * capsule.Radius;

			DrawSegment(p0 + offset, p1 + offset);
			DrawSegment(p0 - offset, p1 - offset);
			offset = side1 * capsule.Radius;
			DrawSegment(p0 + offset, p1 + offset);
			DrawSegment(p0 - offset, p1 - offset);

			Gizmos.DrawWireSphere(p0, capsule.Radius);
			Gizmos.DrawWireSphere(p1, capsule.Radius);
		}
	}
}
