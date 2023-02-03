using System;
using System.Collections;
using System.Collections.Generic;

namespace LibPixelPet {
	/// <summary>
	/// A dictionary where a key can have any number of values.
	/// </summary>
	/// <typeparam name="TKey">The type of the keys.</typeparam>
	/// <typeparam name="TValue">The type of the values.</typeparam>
	public class MultiValueDictionary<TKey, TValue> : IDictionary<TKey, IList<TValue>> {
		/// <summary>
		/// A read-only list that is always empty, and therefore, Clear() does not need to throw an exception.
		/// </summary>
		private sealed class ReadOnlyEmptyList : IList<TValue> {
			public TValue this[int index] {
				get => throw new ArgumentOutOfRangeException(nameof(index));
				set => throw new ArgumentOutOfRangeException(nameof(index));
			}

			public int Count => 0;

			public bool IsReadOnly => true;

			public void Add(TValue item) => throw new NotSupportedException("The empty set is read-only.");
			public void Clear() { } // Does not throw
			public bool Contains(TValue item) => false; // Empty set contains nothing
			public void CopyTo(TValue[] array, int arrayIndex) { } // Nothing to copy

			public IEnumerator<TValue> GetEnumerator() {
				// Empty enumerator
				yield break;
			}

			public int IndexOf(TValue item) => -1;
			public void Insert(int index, TValue item) => throw new NotSupportedException("The empty set is read-only.");
			public bool Remove(TValue item) => false;
			public void RemoveAt(int index) => throw new ArgumentOutOfRangeException(nameof(index));

			IEnumerator IEnumerable.GetEnumerator() {
				// Empty enumerator
				yield break;
			}
		}

		private static readonly ReadOnlyEmptyList EmptySet = new();

		private Dictionary<TKey, IList<TValue>> BaseDictionary { get; }

		/// <summary>
		/// Creates a new empty multi-valued dictionary.
		/// </summary>
		public MultiValueDictionary() {
			BaseDictionary = new Dictionary<TKey, IList<TValue>>();
		}

		/// <summary>
		/// Gets all values associated with the specified key.
		/// </summary>
		/// <param name="key">The key to get the values for.</param>
		/// <returns>The values associated with the key, or an empty set if no values were found.</returns>
		public IList<TValue> this[TKey key] {
			get {
				if (BaseDictionary.TryGetValue(key, out IList<TValue> list)) {
					return list;
				} else {
					return MultiValueDictionary<TKey, TValue>.EmptySet;
				}
			}
			set {
				throw new NotSupportedException("Multi-valued dictionary does not support setting multiple values at once.");
			}
		}

		/// <summary>
		/// Adds the specified value to the dictionary under the specified key.
		/// </summary>
		/// <param name="key">The key to add a value for.</param>
		public void Add(in TKey key, in TValue value) {
			if (BaseDictionary.TryGetValue(key, out IList<TValue> list)) {
				list.Add(value);
			} else {
				BaseDictionary[key] = new List<TValue>() { value };
			}
		}

		public void Add(in KeyValuePair<TKey, TValue> item)
			=> Add(item.Key, item.Value);

		public ICollection<TKey> Keys => ((IDictionary<TKey, IList<TValue>>)BaseDictionary).Keys;

		public ICollection<IList<TValue>> Values => ((IDictionary<TKey, IList<TValue>>)BaseDictionary).Values;

		public int Count
			=> ((IDictionary<TKey, IList<TValue>>)BaseDictionary).Count;

		public bool IsReadOnly
			=> ((IDictionary<TKey, IList<TValue>>)BaseDictionary).IsReadOnly;

		public void Add(TKey key, IList<TValue> value)
			=> ((IDictionary<TKey, IList<TValue>>)BaseDictionary).Add(key, value);

		public void Add(KeyValuePair<TKey, IList<TValue>> item)
			=> ((IDictionary<TKey, IList<TValue>>)BaseDictionary).Add(item);

		public void Clear()
			=> ((IDictionary<TKey, IList<TValue>>)BaseDictionary).Clear();

		public bool Contains(KeyValuePair<TKey, IList<TValue>> item)
			=> ((IDictionary<TKey, IList<TValue>>)BaseDictionary).Contains(item);

		public bool ContainsKey(TKey key)
			=> ((IDictionary<TKey, IList<TValue>>)BaseDictionary).ContainsKey(key);

		public void CopyTo(KeyValuePair<TKey, IList<TValue>>[] array, int arrayIndex) {
			((IDictionary<TKey, IList<TValue>>)BaseDictionary).CopyTo(array, arrayIndex);
		}

		public IEnumerator<KeyValuePair<TKey, IList<TValue>>> GetEnumerator()
			=> ((IDictionary<TKey, IList<TValue>>)BaseDictionary).GetEnumerator();

		public bool Remove(TKey key)
			=> ((IDictionary<TKey, IList<TValue>>)BaseDictionary).Remove(key);

		public bool Remove(KeyValuePair<TKey, IList<TValue>> item)
			=> ((IDictionary<TKey, IList<TValue>>)BaseDictionary).Remove(item);

		public bool TryGetValue(TKey key, out IList<TValue> value)
			=> ((IDictionary<TKey, IList<TValue>>)BaseDictionary).TryGetValue(key, out value);

		IEnumerator IEnumerable.GetEnumerator()
			=> ((IDictionary<TKey, IList<TValue>>)BaseDictionary).GetEnumerator();
	}
}