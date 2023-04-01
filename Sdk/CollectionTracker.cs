#if XUNIT_NULLABLE
#nullable enable
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Xunit.Sdk
{
	class CollectionTracker<T> : IEnumerable<T>
	{
		readonly IEnumerable<T> collection;
#if XUNIT_NULLABLE
		Enumerator? enumerator = null;
#else
		Enumerator enumerator = null;
#endif

		public CollectionTracker(IEnumerable<T> collection)
		{
			this.collection = collection;
		}

		public int IterationCount =>
			enumerator == null ? 0 : enumerator.CurrentIndex + 1;

		internal string FormatIndexedMismatch(
			int mismatchedIndex,
			out int pointerIndent,
			int depth = 0)
		{
			if (enumerator == null)
				enumerator = new Enumerator(collection.GetEnumerator());

			if (depth == ArgumentFormatter.MAX_DEPTH)
			{
				pointerIndent = 1;
				return "[···]";
			}

			pointerIndent = 0;

			var printedValues = new StringBuilder("[");
			var startIndex = Math.Max(0, mismatchedIndex - ArgumentFormatter.MAX_ENUMERABLE_LENGTH_HALF);
			var endIndex = startIndex + ArgumentFormatter.MAX_ENUMERABLE_LENGTH - 1;

			// Make sure our window starts with startIndex and ends with endIndex, as appropriate
			while (enumerator.CurrentIndex < endIndex)
				if (!enumerator.MoveNext())
					break;

			endIndex = enumerator.CurrentIndex;
			startIndex = Math.Max(0, endIndex - ArgumentFormatter.MAX_ENUMERABLE_LENGTH + 1);
			if (startIndex != 0)
				printedValues.Append("···, ");

			for (var idx = startIndex; idx <= endIndex; ++idx)
			{
				if (idx != startIndex)
					printedValues.Append(", ");

				if (idx == mismatchedIndex)
					pointerIndent = printedValues.Length;

				printedValues.Append(ArgumentFormatter.FormatInner(enumerator.CurrentItems[idx], depth + 1));
			}

			if (enumerator.MoveNext())
				printedValues.Append(", ···");

			printedValues.Append(']');
			return printedValues.ToString();
		}

		internal string FormatStart(int depth = 0)
		{
			if (depth == ArgumentFormatter.MAX_DEPTH)
				return "[···]";

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
			int depth = 0)
		{
			if (depth == ArgumentFormatter.MAX_DEPTH)
				return "[···]";

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
			int depth = 0)
		{
			if (depth == ArgumentFormatter.MAX_DEPTH)
				return "[···]";

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

		internal static string FormatStart(
			List<T> startItems,
			int currentIndex,
			int depth)
		{
			var printedValues = new StringBuilder("[");
			var printLength = Math.Min(currentIndex + 1, ArgumentFormatter.MAX_ENUMERABLE_LENGTH);

			for (var idx = 0; idx < printLength; ++idx)
			{
				if (idx != 0)
					printedValues.Append(", ");

				printedValues.Append(ArgumentFormatter.FormatInner(startItems[idx], depth + 1));
			}

			if (currentIndex >= ArgumentFormatter.MAX_ENUMERABLE_LENGTH)
				printedValues.Append(", ···");

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
