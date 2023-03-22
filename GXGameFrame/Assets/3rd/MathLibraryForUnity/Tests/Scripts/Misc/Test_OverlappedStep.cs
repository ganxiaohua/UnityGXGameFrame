using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	public class Test_OverlappedStep : MonoBehaviour
	{
		public enum InOutTypes
		{
			Linear,		// y = x
			Squared,	// y = x^2
			Cubic,		// y = x^3
			InvSquared,	// y = x^(1/2)
			InvCubic,	// y = x^(1/3)
			Sigmoid		// y = S-LikeShape(x) (see formula in comments to MathfEx.EvalSigmoid)
		}

		private float _timer;

		public string Readme = "Press Play To Launch";
		
		public float       Time;
		public Transform[] AnimatedObjects;
		public float       Left;
		public float       Middle;
		public float       Right;
		public float       Overlap;
		public InOutTypes  LeftType  = InOutTypes.InvSquared;
		public InOutTypes  RightType = InOutTypes.Squared;

		private void Awake()
		{
			_timer = 0f;
		}

		private float EvalInOut(float value, InOutTypes type)
		{
			switch (type)
			{
				default:
				case InOutTypes.Linear:     return value;
				case InOutTypes.Squared:    return Mathfex.EvalSquared(value);
				case InOutTypes.Cubic:      return Mathfex.EvalCubic(value);
				case InOutTypes.InvSquared: return Mathfex.EvalInvSquared(value);
				case InOutTypes.InvCubic:   return Mathfex.EvalInvCubic(value);
			}
		}

		private void Update()
		{
			float halfTime = Time * .5f;

			if (_timer < halfTime)
			{
				float coeff = _timer / halfTime;
				for (int i = 0; i < AnimatedObjects.Length; ++i)
				{
					float itemCoeff = Mathfex.EvalOverlappedStep(coeff, Overlap, i, AnimatedObjects.Length);
					itemCoeff = EvalInOut(itemCoeff, LeftType);

					float coord = Mathf.Lerp(Left, Middle, itemCoeff);
					Vector3 position = AnimatedObjects[i].position;
					position.x = coord;
					AnimatedObjects[i].position = position;
				}
			}
			else
			{
				float coeff = (_timer - halfTime) / halfTime;
				for (int i = 0; i < AnimatedObjects.Length; ++i)
				{
					float itemCoeff = Mathfex.EvalOverlappedStep(coeff, Overlap, i, AnimatedObjects.Length);
					itemCoeff = EvalInOut(itemCoeff, RightType);

					float coord = Mathf.Lerp(Middle, Right, itemCoeff); 
					Vector3 position = AnimatedObjects[i].position;
					position.x = coord;
					AnimatedObjects[i].position = position;
				}
			}

			_timer += UnityEngine.Time.deltaTime;
			if (_timer > Time)
			{
				_timer -= Time;
			}
		}
	}
}
