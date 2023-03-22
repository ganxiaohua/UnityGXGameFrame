using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_MathfEx : MonoBehaviour
	{
		private void OnDrawGizmos()
		{
			DrawSigmoid(Vector2.zero);
			DrawOverlappedStep(Vector2.right);
			DrawSmoothOverlappedStep(Vector2.right * 2);
			DrawGaussian(-Vector2.up * 2 + Vector2.right * 1.5f);
		}

		private void DrawSigmoid(Vector2 offset)
		{
			float r = .02f;
			int count = 20;
			float delta = 1f / count;
			Gizmos.color = Color.white;
			Vector2 prev = new Vector2(0, Mathfex.EvalSigmoid(0f));
			Gizmos.DrawSphere(prev + offset, r);
			Vector2 curr;
			for (int i = 1; i <= count; ++i)
			{
				float x = i * delta;
				curr = new Vector2(x, Mathfex.EvalSigmoid(x));
				Gizmos.DrawLine(prev + offset, curr + offset);
				Gizmos.DrawSphere(curr + offset, r);
				prev = curr;
			}
		}

		private void DrawGaussian(Vector2 offset)
		{
			float r = .02f;
			int count = 20;
			float delta = 2f / count;
			Gizmos.color = Color.white;
			Vector2 prev = new Vector2(-count * delta, Mathfex.EvalGaussian(-count * delta, 1.5f, 0f, .5f));
			Gizmos.DrawSphere(prev + offset, r);
			Vector2 curr;
			for (int i = -count + 1; i <= count; ++i)
			{
				float x = i * delta;
				curr = new Vector2(x, Mathfex.EvalGaussian(x, 1.5f, 0f, .5f));
				Gizmos.DrawLine(prev + offset, curr + offset);
				Gizmos.DrawSphere(curr + offset, r);
				prev = curr;
			}
		}

		private void DrawOverlappedStep(Vector2 offset)
		{
			float r = .02f;
			int count = 20;
			float delta = 1f / count;
			Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan };
			
			float overlap = .5f;
			int objCount = 3;
			Vector2[] prev = new Vector2[objCount];
			for (int k = 0; k < objCount; ++k)
			{
				prev[k] = new Vector2(0, Mathfex.EvalOverlappedStep(0f, overlap, k, objCount));
				Gizmos.color = colors[k % colors.Length];
				Gizmos.DrawSphere(prev[k] + offset, r);
			}
			Vector2[] curr = new Vector2[3];
			for (int i = 1; i <= count; ++i)
			{
				float x = i * delta;
				for (int k = 0; k < objCount; ++k)
				{
					curr[k] = new Vector2(x, Mathfex.EvalOverlappedStep(x, overlap, k, objCount));
					Gizmos.color = colors[k % colors.Length];
					Gizmos.DrawLine(prev[k] + offset, curr[k] + offset);
					Gizmos.DrawSphere(curr[k] + offset, r);
					prev[k] = curr[k];
				}
			}
		}

		private void DrawSmoothOverlappedStep(Vector2 offset)
		{
			float r = .02f;
			int count = 20;
			float delta = 1f / count;
			Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan };

			float overlap = .5f;
			int objCount = 3;
			Vector2[] prev = new Vector2[objCount];
			for (int k = 0; k < objCount; ++k)
			{
				prev[k] = new Vector2(0, Mathfex.EvalSmoothOverlappedStep(0f, overlap, k, objCount));
				Gizmos.color = colors[k % colors.Length];
				Gizmos.DrawSphere(prev[k] + offset, r);
			}
			Vector2[] curr = new Vector2[3];
			for (int i = 1; i <= count; ++i)
			{
				float x = i * delta;
				for (int k = 0; k < objCount; ++k)
				{
					curr[k] = new Vector2(x, Mathfex.EvalSmoothOverlappedStep(x, overlap, k, objCount));
					Gizmos.color = colors[k % colors.Length];
					Gizmos.DrawLine(prev[k] + offset, curr[k] + offset);
					Gizmos.DrawSphere(curr[k] + offset, r);
					prev[k] = curr[k];
				}
			}
		}
	}
}
