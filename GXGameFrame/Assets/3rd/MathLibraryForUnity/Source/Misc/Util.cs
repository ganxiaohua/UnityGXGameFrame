using System.Collections.Generic;

namespace Dest.Math
{
	public static class Util
	{
		/// <summary>
		/// Shuffles the collection using Rand.Instance.
		/// </summary>
		public static void Shuffle<T>(this IList<T> collection)
		{
			// http://en.wikipedia.org/wiki/Knuth_shuffle#The_modern_algorithm
			Rand rand = Rand.Instance;
			int count = collection.Count;
			while (count > 1)
			{
				--count;
				int swapIndex = rand.NextInt(count + 1);
				T value = collection[swapIndex];
				collection[swapIndex] = collection[count];
				collection[count] = value;
			}
		}

		/// <summary>
		/// Shuffles the collection using specified random generator.
		/// </summary>
		public static void Shuffle<T>(this IList<T> collection, Rand rand)
		{
			int count = collection.Count;
			while (count > 1)
			{
				--count;
				int swapIndex = rand.NextInt(count + 1);
				T value = collection[swapIndex];
				collection[swapIndex] = collection[count];
				collection[count] = value;
			}
		}

		/// <summary>
		/// Shuffles the collection using Rand.Instance.
		/// </summary>
		public static void Shuffle<T>(this T[] collection)
		{
			Rand rand = Rand.Instance;
			int count = collection.Length;
			while (count > 1)
			{
				--count;
				int swapIndex = rand.NextInt(count + 1);
				T value = collection[swapIndex];
				collection[swapIndex] = collection[count];
				collection[count] = value;
			}
		}

		/// <summary>
		/// Shuffles the collection using specified random generator.
		/// </summary>
		public static void Shuffle<T>(this T[] collection, Rand rand)
		{
			int count = collection.Length;
			while (count > 1)
			{
				--count;
				int swapIndex = rand.NextInt(count + 1);
				T value = collection[swapIndex];
				collection[swapIndex] = collection[count];
				collection[count] = value;
			}
		}
	}
}
