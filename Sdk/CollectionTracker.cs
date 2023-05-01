#if XUNIT_NULLABLE
#nullable enable
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Xunit.Sdk
{
	static class CollectionTracker
	{
		static MethodInfo openGenericCompareTypedSetsMethod =
			typeof(CollectionTracker)
				.GetRuntimeMethods()
				.Single(m => m.Name == nameof(CompareTypedSets));

		internal static bool AreCollectionsEqual(
#if XUNIT_NULLABLE
			object? expected,
			IEnumerable? expectedTracker,
			object? actual,
			IEnumerable? actualTracker,
#else
			object expected,
			IEnumerable expectedTracker,
			object actual,
			IEnumerable actualTracker,
#endif
			IEqualityComparer itemComparer,
			out int? mismatchedIndex)
		{
			mismatchedIndex = null;

			return
				CheckIfDictionariesAreEqual(expected, actual, itemComparer) ??
				CheckIfSetsAreEqual(expected, actual) ??
				CheckIfArraysAreEqual(expected, expectedTracker, actual, actualTracker, itemComparer, out mismatchedIndex) ??
				CheckIfEnumerablesAreEqual(expectedTracker, actualTracker, itemComparer, out mismatchedIndex);
		}

		static bool? CheckIfArraysAreEqual(
#if XUNIT_NULLABLE
			object? expected,
			IEnumerable? expectedTracker,
			object? actual,
			IEnumerable? actualTracker,
#else
			object expected,
			IEnumerable expectedTracker,
			object actual,
			IEnumerable actualTracker,
#endif
			IEqualityComparer itemComparer,
			out int? mismatchedIndex)
		{
			mismatchedIndex = null;

			var expectedArray = expected as Array;
			var actualArray = actual as Array;

			if (expectedArray == null || actualArray == null || expectedTracker == null || actualTracker == null)
				return null;

			// If we have single-dimensional zero-based arrays, then we delegate to the enumerable
			// version, since that's uses the trackers and gets us the mismatch pointer.
			if (expectedArray.Rank == 1 && expectedArray.GetLowerBound(0) == 0 &&
				actualArray.Rank == 1 && actualArray.GetLowerBound(0) == 0)
				return CheckIfEnumerablesAreEqual(expectedTracker, actualTracker, itemComparer, out mismatchedIndex);

			if (expectedArray.Rank != actualArray.Rank)
				return false;

			// Differing bounds, aka object[2,1] vs. object[1,2]
			// You can also have non-zero-based arrays, so we don't just check lengths
			for (var rank = 0; rank < expectedArray.Rank; rank++)
				if (expectedArray.GetLowerBound(rank) != actualArray.GetLowerBound(rank) || expectedArray.GetUpperBound(rank) != actualArray.GetUpperBound(rank))
					return false;

			// Enumeration will flatten everything identically, so just enumerate at this point
			var expectedEnumerator = expectedTracker.GetEnumerator();
			var actualEnumerator = actualTracker.GetEnumerator();

			while (true)
			{
				var hasExpected = expectedEnumerator.MoveNext();
				var hasActual = actualEnumerator.MoveNext();

				if (!hasExpected || !hasActual)
					return hasExpected == hasActual;

				if (!itemComparer.Equals(expectedEnumerator.Current, actualEnumerator.Current))
					return false;
			}
		}

		static bool? CheckIfDictionariesAreEqual(
#if XUNIT_NULLABLE
			object? x,
			object? y,
#else
			object x,
			object y,
#endif
			IEqualityComparer itemComparer)
		{
			var dictionaryX = x as IDictionary;
			var dictionaryY = y as IDictionary;

			if (dictionaryX == null || dictionaryY == null)
				return null;

			if (dictionaryX.Count != dictionaryY.Count)
				return false;

			var dictionaryYKeys = new HashSet<object>(dictionaryY.Keys.Cast<object>());

			foreach (var key in dictionaryX.Keys.Cast<object>())
			{
				if (!dictionaryYKeys.Contains(key))
					return false;

				var valueX = dictionaryX[key];
				var valueY = dictionaryY[key];

				if (!itemComparer.Equals(valueX, valueY))
					return false;

				dictionaryYKeys.Remove(key);
			}

			return dictionaryYKeys.Count == 0;
		}

		static bool CheckIfEnumerablesAreEqual(
#if XUNIT_NULLABLE
			IEnumerable? x,
			IEnumerable? y,
#else
			IEnumerable x,
			IEnumerable y,
#endif
			IEqualityComparer itemComparer,
			out int? mismatchIndex)
		{
			mismatchIndex = null;

			if (x == null)
				return y == null;
			if (y == null)
				return false;

			var enumeratorX = x.GetEnumerator();
			var enumeratorY = y.GetEnumerator();

			mismatchIndex = 0;

			while (true)
			{
				var hasNextX = enumeratorX.MoveNext();
				var hasNextY = enumeratorY.MoveNext();

				if (!hasNextX || !hasNextY)
				{
					if (hasNextX == hasNextY)
					{
						mismatchIndex = null;
						return true;
					}

					return false;
				}

				var expectedCurrent = enumeratorX.Current;
				var expectedCurrentEnumerable = expectedCurrent as IEnumerable;
				var actualCurrent = enumeratorY.Current;
				var actualCurrentEnumerable = actualCurrent as IEnumerable;

				if (expectedCurrentEnumerable != null && actualCurrentEnumerable != null)
				{
					int? _;
					var innerCompare = CheckIfEnumerablesAreEqual(expectedCurrentEnumerable, actualCurrentEnumerable, EqualityComparer<object>.Default, out _);
					if (innerCompare == false)
						return false;
				}
				else if (!itemComparer.Equals(expectedCurrent, actualCurrent))
					return false;

				mismatchIndex++;
			}
		}

		static bool? CheckIfSetsAreEqual(
#if XUNIT_NULLABLE
			object? x,
			object? y)
#else
			object x,
			object y)
#endif
		{
			var elementTypeX = GetSetElementType(x);
			var elementTypeY = GetSetElementType(y);

			if (x == null || elementTypeX == null || y == null || elementTypeY == null)
				return null;

			if (elementTypeX != elementTypeY)
				return false;

			var genericCompareMethod = openGenericCompareTypedSetsMethod.MakeGenericMethod(elementTypeX);
#if XUNIT_NULLABLE
			return (bool)genericCompareMethod.Invoke(null, new[] { x, y })!;
#else
			return (bool)genericCompareMethod.Invoke(null, new[] { x, y });
#endif
		}

		static bool CompareTypedSets<TSet>(
			IEnumerable enumX,
			IEnumerable enumY)
		{
			var setX = new HashSet<TSet>(enumX.Cast<TSet>());
			var setY = new HashSet<TSet>(enumY.Cast<TSet>());

			return setX.SetEquals(setY);
		}

#if XUNIT_NULLABLE
		internal static Type? GetSetElementType(object? obj)
#else
		internal static Type GetSetElementType(object obj)
#endif
		{
			if (obj == null)
				return null;

			var setInterface = (from @interface in obj.GetType().GetTypeInfo().ImplementedInterfaces
								where @interface.GetTypeInfo().IsGenericType
								let genericTypeDefinition = @interface.GetGenericTypeDefinition()
								where genericTypeDefinition == typeof(ISet<>)
								select @interface.GetTypeInfo()).FirstOrDefault();

			return setInterface == null ? null : setInterface.GenericTypeArguments[0];
		}

#if XUNIT_NULLABLE
		internal static bool SafeToMultiEnumerate(object? collection) =>
#else
		internal  static bool SafeToMultiEnumerate(object collection) =>
#endif
			collection is Array ||
			collection is IList ||
			collection is IDictionary ||
			GetSetElementType(collection) != null;
	}

	class CollectionTracker<T> : IEnumerable<T>, ICollectionTracker
	{
		readonly IEnumerable<T> collection;
#if XUNIT_NULLABLE
		Enumerator? enumerator = null;
#else
		Enumerator enumerator = null;
#endif

		CollectionTracker(IEnumerable<T> collection)
		{
			this.collection = collection;
		}

		public int IterationCount =>
			enumerator == null ? 0 : enumerator.CurrentIndex + 1;

		public void Dispose() =>
			enumerator?.Dispose();

		internal string FormatIndexedMismatch(
			int? mismatchedIndex,
			out int? pointerIndent,
			int depth = 1)
		{
			if (depth == ArgumentFormatter.MAX_DEPTH)
			{
				pointerIndent = 1;
				return "[" + ArgumentFormatter.Ellipsis + "]";
			}

			int startIndex;
			int endIndex;

			GetMismatchExtents(mismatchedIndex, out startIndex, out endIndex);

			return FormatIndexedMismatch(
#if XUNIT_NULLABLE
				enumerator!.CurrentItems,
#else
				enumerator.CurrentItems,
#endif
				() => enumerator.MoveNext(),
				startIndex,
				endIndex,
				mismatchedIndex,
				out pointerIndent,
				depth
			);
		}

		public string FormatIndexedMismatch(
			int startIndex,
			int endIndex,
			int? mismatchedIndex,
			out int? pointerIndent,
			int depth = 1)
		{
			if (enumerator == null)
				throw new InvalidOperationException("Called FormatIndexedMismatch with indices without calling GetMismatchExtents first");

			return FormatIndexedMismatch(
				enumerator.CurrentItems,
				() => enumerator.MoveNext(),
				startIndex,
				endIndex,
				mismatchedIndex,
				out pointerIndent,
				depth
			);
		}

#if XUNIT_SPAN
		internal static string FormatIndexedMismatch(
			ReadOnlySpan<T> span,
			int? mismatchedIndex,
			out int? pointerIndent,
			int depth = 1)
		{
			if (depth == ArgumentFormatter.MAX_DEPTH)
			{
				pointerIndent = 1;
				return "[" + ArgumentFormatter.Ellipsis + "]";
			}

			var startIndex = Math.Max(0, (mismatchedIndex ?? 0) - ArgumentFormatter.MAX_ENUMERABLE_LENGTH_HALF);
			var endIndex = Math.Min(span.Length - 1, startIndex + ArgumentFormatter.MAX_ENUMERABLE_LENGTH - 1);
			startIndex = Math.Max(0, endIndex - ArgumentFormatter.MAX_ENUMERABLE_LENGTH + 1);

			var moreItemsPastEndIndex = endIndex < span.Length - 1;
			var items = new Dictionary<int, T>();

			for (var idx = startIndex; idx <= endIndex; ++idx)
				items[idx] = span[idx];

			return FormatIndexedMismatch(
				items,
				() => moreItemsPastEndIndex,
				startIndex,
				endIndex,
				mismatchedIndex,
				out pointerIndent,
				depth
			);
		}
#endif

		static string FormatIndexedMismatch(
			Dictionary<int, T> items,
			Func<bool> moreItemsPastEndIndex,
			int startIndex,
			int endIndex,
			int? mismatchedIndex,
			out int? pointerIndent,
			int depth)
		{
			pointerIndent = null;

			var printedValues = new StringBuilder("[");
			if (startIndex != 0)
				printedValues.Append(ArgumentFormatter.Ellipsis + ", ");

			for (var idx = startIndex; idx <= endIndex; ++idx)
			{
				if (idx != startIndex)
					printedValues.Append(", ");

				if (idx == mismatchedIndex)
					pointerIndent = printedValues.Length;

				printedValues.Append(ArgumentFormatter.Format(items[idx], depth));
			}

			if (moreItemsPastEndIndex())
				printedValues.Append(", " + ArgumentFormatter.Ellipsis);

			printedValues.Append(']');
			return printedValues.ToString();
		}

		public string FormatStart(int depth = 1)
		{
			if (depth == ArgumentFormatter.MAX_DEPTH)
				return "[" + ArgumentFormatter.Ellipsis + "]";

			if (enumerator == null)
				enumerator = new Enumerator(collection.GetEnumerator());

			// Ensure we have already seen enough data to format
			while (enumerator.CurrentIndex <= ArgumentFormatter.MAX_ENUMERABLE_LENGTH)
				if (!enumerator.MoveNext())
					break;

			return FormatStart(enumerator.StartItems, enumerator.CurrentIndex, depth);
		}

		internal static string FormatStart(
			IEnumerable<T> collection,
			int depth = 1)
		{
			if (depth == ArgumentFormatter.MAX_DEPTH)
				return "[" + ArgumentFormatter.Ellipsis + "]";

			var startItems = new List<T>();
			var currentIndex = -1;
			var spanEnumerator = collection.GetEnumerator();

			// Ensure we have already seen enough data to format
			while (currentIndex <= ArgumentFormatter.MAX_ENUMERABLE_LENGTH)
			{
				if (!spanEnumerator.MoveNext())
					break;

				startItems.Add(spanEnumerator.Current);
				++currentIndex;
			}

			return FormatStart(startItems, currentIndex, depth);
		}

#if XUNIT_SPAN
		internal static string FormatStart(
			ReadOnlySpan<T> span,
			int depth = 1)
		{
			if (depth == ArgumentFormatter.MAX_DEPTH)
				return "[" + ArgumentFormatter.Ellipsis + "]";

			var startItems = new List<T>();
			var currentIndex = -1;
			var spanEnumerator = span.GetEnumerator();

			// Ensure we have already seen enough data to format
			while (currentIndex <= ArgumentFormatter.MAX_ENUMERABLE_LENGTH)
			{
				if (!spanEnumerator.MoveNext())
					break;

				startItems.Add(spanEnumerator.Current);
				++currentIndex;
			}

			return FormatStart(startItems, currentIndex, depth);
		}
#endif

		static string FormatStart(
			List<T> items,
			int currentIndex,
			int depth)
		{
			var printedValues = new StringBuilder("[");
			var printLength = Math.Min(currentIndex + 1, ArgumentFormatter.MAX_ENUMERABLE_LENGTH);

			for (var idx = 0; idx < printLength; ++idx)
			{
				if (idx != 0)
					printedValues.Append(", ");

				printedValues.Append(ArgumentFormatter.Format(items[idx], depth));
			}

			if (currentIndex >= ArgumentFormatter.MAX_ENUMERABLE_LENGTH)
				printedValues.Append(", " + ArgumentFormatter.Ellipsis);

			printedValues.Append(']');
			return printedValues.ToString();
		}

		public IEnumerator<T> GetEnumerator()
		{
			if (enumerator != null)
				throw new InvalidOperationException("Multiple enumeration is not supported");

			enumerator = new Enumerator(collection.GetEnumerator());
			return enumerator;
		}

		IEnumerator IEnumerable.GetEnumerator() =>
			GetEnumerator();

		internal void GetMismatchExtents(
			int? mismatchedIndex,
			out int startIndex,
			out int endIndex)
		{
			if (enumerator == null)
				enumerator = new Enumerator(collection.GetEnumerator());

			startIndex = Math.Max(0, (mismatchedIndex ?? 0) - ArgumentFormatter.MAX_ENUMERABLE_LENGTH_HALF);
			endIndex = startIndex + ArgumentFormatter.MAX_ENUMERABLE_LENGTH - 1;

			// Make sure our window starts with startIndex and ends with endIndex, as appropriate
			while (enumerator.CurrentIndex < endIndex)
				if (!enumerator.MoveNext())
					break;

			endIndex = enumerator.CurrentIndex;
			startIndex = Math.Max(0, endIndex - ArgumentFormatter.MAX_ENUMERABLE_LENGTH + 1);
		}

#if XUNIT_NULLABLE
		public string? TypeAt(int? value)
#else
		public string TypeAt(int? value)
#endif
		{
			if (enumerator == null || !value.HasValue)
				return null;

#if XUNIT_NULLABLE
			T? item;
#else
			T item;
#endif
			if (!enumerator.CurrentItems.TryGetValue(value.Value, out item))
				return null;

			return item?.GetType().FullName;
		}

		internal static CollectionTracker<T> Wrap(IEnumerable<T> collection) =>
			new CollectionTracker<T>(collection);

		class Enumerator : IEnumerator<T>
		{
			readonly IEnumerator<T> innerEnumerator;

			public Enumerator(IEnumerator<T> innerEnumerator)
			{
				this.innerEnumerator = innerEnumerator;
			}

			public T Current =>
				innerEnumerator.Current;

#if XUNIT_NULLABLE
			object? IEnumerator.Current =>
#else
			object IEnumerator.Current =>
#endif
				Current;

			public int CurrentIndex { get; private set; } = -1;

			public Dictionary<int, T> CurrentItems { get; } = new Dictionary<int, T>();

			public List<T> StartItems { get; } = new List<T>();

			public void Dispose()
			{
				innerEnumerator.Dispose();
			}

			public bool MoveNext()
			{
				if (!innerEnumerator.MoveNext())
					return false;

				CurrentIndex++;
				var current = innerEnumerator.Current;

				// Keep (MAX_ENUMERABLE_LENGTH + 1) items here, so we can
				// print the start of the collection when lengths differ
				if (CurrentIndex <= ArgumentFormatter.MAX_ENUMERABLE_LENGTH)
					StartItems.Add(current);

				// Keep the most recent MAX_ENUMERABLE_LENGTH in the dictionary,
				// so we can print out the items when we've found a bad index
				CurrentItems[CurrentIndex] = current;

				if (CurrentIndex >= ArgumentFormatter.MAX_ENUMERABLE_LENGTH)
					CurrentItems.Remove(CurrentIndex - ArgumentFormatter.MAX_ENUMERABLE_LENGTH);

				return true;
			}

			public void Reset()
			{
				throw new InvalidOperationException("This enumerator does not support resetting");
			}
		}
	}
}
