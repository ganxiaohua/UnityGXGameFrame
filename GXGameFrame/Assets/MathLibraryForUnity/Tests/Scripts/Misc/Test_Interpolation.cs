using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	public class Test_Interpolation : Test_Base
	{
		public enum Types
		{
			Lerp,
			Sigerp,
			Sinerp,
			Coserp,
			Berp,
			Curverp,
			Funcerp
		}

		private float _timer;

		public string Readme = "Press Play To Launch";
		public Types Type;
		public AnimationCurve Curve;
		public Vector2 From;
		public Vector2 To;
		public float Speed;
		public float Time;

		private float Func(float value)
		{
			return Mathf.Sqrt(value);
		}

		private void Update()
		{
			_timer += UnityEngine.Time.deltaTime * Speed;
			if (_timer > Time) _timer -= Time;
		}

		private void OnDrawGizmos()
		{
			System.Func<float, float, float, float> method = null;
			switch (Type)
			{
				case Types.Lerp   : method = Mathfex.Lerp;           break;
				case Types.Sigerp : method = Mathfex.SigmoidInterp;  break;
				case Types.Sinerp : method = Mathfex.SinInterp;      break;
				case Types.Coserp : method = Mathfex.CosInterp;      break;
				case Types.Berp   : method = Mathfex.WobbleInterp;   break;
				case Types.Curverp: method = delegate(float v0, float v1, float factor) { return Mathfex.CurveInterp(v0, v1, factor, Curve); }; break;
				case Types.Funcerp: method = delegate(float v0, float v1, float factor) { return Mathfex.FuncInterp (v0, v1, factor, Func ); }; break;
			}

			float t = _timer / Time;
			Vector2 v;
			v.x = method(From.x, To.x, t);
			v.y = method(From.y, To.y, t);

			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(From, .5f);
			Gizmos.DrawSphere(To, .5f);

			Gizmos.color = Color.white;
			Gizmos.DrawSphere(v, .25f);
		}
	}
}
