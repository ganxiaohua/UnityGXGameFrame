using System.Collections.Generic;

namespace Dest.Math
{
	/// <summary>
	/// Represents ShuffleBag generic collection.
	/// </summary>
	public class ShuffleBag<T> : IEnumerable<T>
	{
		// Based on http://kaioa.com/node/53

		private Rand    _rand;
		private List<T> _items;
		private int     _index;

		/// <summary>
		/// Returns collection count.
		/// </summary>
		public int Count { get { return _items.Count; } }

		/// <summary>
		/// Creates new instance of ShuffleBag with Rand.Instance randomizer and zero capacity of the underlying collection.
		/// </summary>
		public ShuffleBag()
		{
			_rand = Rand.Instance;
			_items = new List<T>();
		}

		/// <summary>
		/// Creates new instance of ShuffleBag with Rand.Instance randomizer and specified capacity of the underlying collection.
		/// </summary>
		public ShuffleBag(int capacity)
		{
			_rand = Rand.Instance;
			_items = new List<T>(capacity);
		}

		/// <summary>
		/// Creates new instance of ShuffleBag with specified randomizer and zero capacity of the underlying collection.
		/// </summary>
		public ShuffleBag(Rand rand)
		{
			_rand = rand;
			_items = new List<T>();
		}

		/// <summary>
		/// Creates new instance of ShuffleBag with specified randomizer and specified capacity of the underlying collection.
		/// </summary>
		public ShuffleBag(Rand rand, int capacity)
		{
			_rand = rand;
			_items = new List<T>(capacity);
		}

		/// <summary>
		/// Adds the item to the bag with specified number of entries.
		/// </summary>
		public void Add(T item, uint count = 1)
		{
			for (uint i = 0; i < count; ++i)
			{
				_items.Add(item);
			}
			_index = _items.Count - 1;
		}

		/// <summary>
		/// Draws an item out of the bag.
		/// </summary>
		public T NextItem()
		{
			if (_index < 1)
			{
				_index = _items.Count - 1;
				return _items[0];
			}

			int swapIndex = _rand.NextInt(_index + 1);
			T result = _items[swapIndex];
			_items[swapIndex] = _items[_index];
			_items[_index] = result;
			--_index;

			return result;
		}

		/// <summary>
		/// Resets bag traversal.
		/// </summary>
		public void Reset()
		{
			_index = _items.Count - 1;
		}

		/// <summary>
		/// Removes all items from the bag.
		/// </summary>
		public void Clear()
		{
			_items.Clear();
			_index = 0;
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0, len = _items.Count; i < len; ++i)
			{
				yield return NextItem();
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
