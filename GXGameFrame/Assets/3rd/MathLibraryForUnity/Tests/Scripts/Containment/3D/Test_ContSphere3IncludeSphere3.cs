using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ContSphere3IncludeSphere3 : Test_Base
	{
		public Transform Sphere0;
		public Transform Sphere1;

		private void OnDrawGizmos()
		{
			Sphere3 sphere0 = CreateSphere3(Sphere0);
			Sphere3 sphere1 = CreateSphere3(Sphere1);

			Sphere3 sphere = sphere0;
			sphere.Include(sphere1); // Sphere which merges sphere0 and sphere1

			FiguresColor();
			DrawSphere(ref sphere0);
			DrawSphere(ref sphere1);
			ResultsColor();
			DrawSphere(ref sphere);
		}
	}
}
