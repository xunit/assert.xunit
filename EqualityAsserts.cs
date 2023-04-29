#if XUNIT_NULLABLE
#nullable enable

using System.Diagnostics.CodeAnalysis;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit
{
#if XUNIT_VISIBILITY_INTERNAL
	internal
#else
	public
#endif
	partial class Assert
	{
		static Type typeofDictionary = typeof(Dictionary<,>);
		static Type typeofHashSet = typeof(HashSet<>);
		static Type typeofSet = typeof(ISet<>);

#if XUNIT_NULLABLE
		static IEnumerable? AsNonStringEnumerable(object? value) =>
#else
		static IEnumerable AsNonStringEnumerable(object value) =>
#endif
			value == null || value is string ? null : value as IEnumerable;

#if XUNIT_NULLABLE
		static CollectionTracker<object>? AsNonStringTracker(object? value) =>
#else
		static CollectionTracker<object> AsNonStringTracker(object value) =>
#endif
			AsNonStringEnumerable(value).AsTracker();

#if XUNIT_SPAN
		/// <summary>
		/// Verifies that two arrays of un-managed type T are equal, using Span&lt;T&gt;.SequenceEqual.
		/// This can be significantly faster than generic enumerables, when the collections are actually
		/// equal, because the system can optimize packed-memory comparisons for value type arrays.
		/// </summary>
		/// <typeparam name="T">The type of items whose arrays are to be compared</typeparam>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The value to be compared against</param>
		/// <remarks>
		/// If Span&lt;T&gt;.SequenceEqual fails, a call to Assert.Equal(object, object) is made,
		/// to provide a more meaningful error message.
		/// </remarks>
		public static void Equal<T>(
#if XUNIT_NULLABLE
			[AllowNull] T[] expected,
			[AllowNull] T[] actual)
				where T : unmanaged, IEquatable<T>
#else
			T[] expected,
			T[] actual)
				where T : IEquatable<T>
#endif
		{
			if (expected == null && actual == null)
				return;

			// Call into Equal<object> so we get proper formatting of the sequence
			if (expected == null || actual == null || !expected.AsSpan().SequenceEqual(actual))
				Equal<object>(expected, actual);
		}
#endif

		/// <summary>
		/// Verifies that two objects are equal, using a default comparer.
		/// </summary>
		/// <typeparam name="T">The type of the objects to be compared</typeparam>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The value to be compared against</param>
		public static void Equal<T>(
#if XUNIT_NULLABLE
			[AllowNull] T expected,
			[AllowNull] T actual) =>
#else
			T expected,
			T actual) =>
#endif
				Equal(expected, actual, GetEqualityComparer<T>());

		/// <summary>
		/// Verifies that two objects are equal, using a custom equatable comparer.
		/// </summary>
		/// <typeparam name="T">The type of the objects to be compared</typeparam>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The value to be compared against</param>
		/// <param name="comparer">The comparer used to compare the two objects</param>
		public static void Equal<T>(
#if XUNIT_NULLABLE
			[AllowNull] T expected,
			[AllowNull] T actual,
#else
			T expected,
			T actual,
#endif
			IEqualityComparer<T> comparer)
		{
			GuardArgumentNotNull(nameof(comparer), comparer);

			if (expected == null && actual == null)
				return;

			var expectedTracker = AsNonStringTracker(expected);
			var actualTracker = AsNonStringTracker(actual);

			// We treat collections specially, since we want to see a pointer
			var haveCollections =
				(expectedTracker != null && actualTracker != null) ||
				(expectedTracker != null && actual == null) ||
				(expected == null && actualTracker != null);

			if (haveCollections)
			{
				int? mismatchedIndex = null;
				bool equal;

				// TODO: We have trackers here, but we don't use them, because neither AssertEqualityComparer<T> nor
				// IEqualityComparer<T> can accept them. To prevent double-enumeration, we will need to find another
				// way to safely enumerate the collection and compare the items. This may mean all this logic ends
				// up in the comparer-less overload, and the version with the comparer here will never attempt to
				// print enumerable values (we're going to have to fix ArgumentFormatter anyway so it won't print
				// out unsafe IEnumerables; we'll have to rely on knowing that we have a safe built-in collection
				// or an interface that allows safe re-enumeration).

				// If they provided us with an AssertEqualityComparer<T>, we can use that to get the mismatched index
				var aec = comparer as AssertEqualityComparer<T>;
				if (aec != null)
					equal = aec.Equals(expected, actual, out mismatchedIndex);
				else
					equal = comparer.Equals(expected, actual);

				if (equal)
					return;

				var expectedStartIdx = -1;
				var expectedEndIdx = -1;
				expectedTracker?.GetMismatchExtents(mismatchedIndex, out expectedStartIdx, out expectedEndIdx);

				var actualStartIdx = -1;
				var actualEndIdx = -1;
				actualTracker?.GetMismatchExtents(mismatchedIndex, out actualStartIdx, out actualEndIdx);

				// If either located index is past the end of the collection, then we want to try to shift
				// the too-short collection start point forward so we can align the equal values for
				// a more readable and obvious output. See CollectionAssertTests+Equals+Arrays.Truncation
				// for overrun examples.
				if (mismatchedIndex.HasValue)
				{
					if (expectedStartIdx > -1 && expectedEndIdx < mismatchedIndex.Value)
						expectedStartIdx = actualStartIdx;
					else if (actualStartIdx > -1 && actualEndIdx < mismatchedIndex.Value)
						actualStartIdx = expectedStartIdx;
				}

				int? expectedPointer = null;
				var formattedExpected = expectedTracker?.FormatIndexedMismatch(expectedStartIdx, expectedEndIdx, mismatchedIndex, out expectedPointer) ?? ArgumentFormatter.Format(expected);
				var expectedItemType = expectedTracker?.TypeAt(mismatchedIndex);

				int? actualPointer = null;
				var formattedActual = actualTracker?.FormatIndexedMismatch(actualStartIdx, actualEndIdx, mismatchedIndex, out actualPointer) ?? ArgumentFormatter.Format(actual);
				var actualItemType = actualTracker?.TypeAt(mismatchedIndex);

#if XUNIT_NULLABLE
				string? collectionDisplay = null;
#else
				string collectionDisplay = null;
#endif

				var expectedType = expected?.GetType();
				var expectedTypeDefinition = SafeGetGenericTypeDefinition(expectedType);
				var expectedInterfaceTypeDefinitions = expectedType?.GetTypeInfo().ImplementedInterfaces.Where(i => i.GetTypeInfo().IsGenericType).Select(i => i.GetGenericTypeDefinition());
				var actualType = actual?.GetType();
				var actualTypeDefinition = SafeGetGenericTypeDefinition(actualType);
				var actualInterfaceTypeDefinitions = actualType?.GetTypeInfo().ImplementedInterfaces.Where(i => i.GetTypeInfo().IsGenericType).Select(i => i.GetGenericTypeDefinition());

				if (expectedTypeDefinition == typeofDictionary && actualTypeDefinition == typeofDictionary)
					collectionDisplay = "Dictionaries";
				else if (expectedTypeDefinition == typeofHashSet && actualTypeDefinition == typeofHashSet)
					collectionDisplay = "HashSets";
				else if (expectedInterfaceTypeDefinitions != null && actualInterfaceTypeDefinitions != null && expectedInterfaceTypeDefinitions.Contains(typeofSet) && actualInterfaceTypeDefinitions.Contains(typeofSet))
					collectionDisplay = "Sets";

				if (expectedType != actualType)
				{
					var expectedTypeName = expectedType == null ? "" : ArgumentFormatter2.FormatTypeName(expectedType) + " ";
					var actualTypeName = actualType == null ? "" : ArgumentFormatter2.FormatTypeName(actualType) + " ";

					var typeNameIndent = Math.Max(expectedTypeName.Length, actualTypeName.Length);

					formattedExpected = expectedTypeName.PadRight(typeNameIndent) + formattedExpected;
					formattedActual = actualTypeName.PadRight(typeNameIndent) + formattedActual;

					if (expectedPointer != null)
						expectedPointer += typeNameIndent;
					if (actualPointer != null)
						actualPointer += typeNameIndent;
				}

				throw EqualException.ForMismatchedCollections(mismatchedIndex, formattedExpected, expectedPointer, expectedItemType, formattedActual, actualPointer, actualItemType, collectionDisplay);
			}
			else if (!comparer.Equals(expected, actual))
				throw EqualException.ForMismatchedValues(expected, actual);
		}

		/// <summary>
		/// Verifies that two <see cref="double"/> values are equal, within the number of decimal
		/// places given by <paramref name="precision"/>. The values are rounded before comparison.
		/// </summary>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The value to be compared against</param>
		/// <param name="precision">The number of decimal places (valid values: 0-15)</param>
		public static void Equal(
			double expected,
			double actual,
			int precision)
		{
			var expectedRounded = Math.Round(expected, precision);
			var actualRounded = Math.Round(actual, precision);

			if (!object.Equals(expectedRounded, actualRounded))
				throw EqualException.ForMismatchedValues(
					$"{expectedRounded:G17} (rounded from {expected:G17})",
					$"{actualRounded:G17} (rounded from {actual:G17})",
					$"Values are not within {precision} decimal place{(precision == 1 ? "" : "s")}"
				);
		}

		/// <summary>
		/// Verifies that two <see cref="double"/> values are equal, within the number of decimal
		/// places given by <paramref name="precision"/>. The values are rounded before comparison.
		/// The rounding method to use is given by <paramref name="rounding" />
		/// </summary>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The value to be compared against</param>
		/// <param name="precision">The number of decimal places (valid values: 0-15)</param>
		/// <param name="rounding">Rounding method to use to process a number that is midway between two numbers</param>
		public static void Equal(
			double expected,
			double actual,
			int precision,
			MidpointRounding rounding)
		{
			var expectedRounded = Math.Round(expected, precision, rounding);
			var actualRounded = Math.Round(actual, precision, rounding);

			if (!object.Equals(expectedRounded, actualRounded))
				throw EqualException.ForMismatchedValues(
					$"{expectedRounded:G17} (rounded from {expected:G17})",
					$"{actualRounded:G17} (rounded from {actual:G17})",
					$"Values are not within {precision} decimal place{(precision == 1 ? "" : "s")}"
				);
		}

		/// <summary>
		/// Verifies that two <see cref="double"/> values are equal, within the tolerance given by
		/// <paramref name="tolerance"/> (positive or negative).
		/// </summary>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The value to be compared against</param>
		/// <param name="tolerance">The allowed difference between values</param>
		public static void Equal(
			double expected,
			double actual,
			double tolerance)
		{
			if (double.IsNaN(tolerance) || double.IsNegativeInfinity(tolerance) || tolerance < 0.0)
				throw new ArgumentException("Tolerance must be greater than or equal to zero", nameof(tolerance));

			if (!(object.Equals(expected, actual) || Math.Abs(expected - actual) <= tolerance))
				throw EqualException.ForMismatchedValues(
					expected.ToString("G17"),
					actual.ToString("G17"),
					$"Values are not within tolerance {tolerance:G17}"
				);
		}

		/// <summary>
		/// Verifies that two <see cref="float"/> values are equal, within the number of decimal
		/// places given by <paramref name="precision"/>. The values are rounded before comparison.
		/// </summary>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The value to be compared against</param>
		/// <param name="precision">The number of decimal places (valid values: 0-15)</param>
		public static void Equal(
			float expected,
			float actual,
			int precision)
		{
			var expectedRounded = Math.Round(expected, precision);
			var actualRounded = Math.Round(actual, precision);

			if (!object.Equals(expectedRounded, actualRounded))
				throw EqualException.ForMismatchedValues(
					$"{expectedRounded:G9} (rounded from {expected:G9})",
					$"{actualRounded:G9} (rounded from {actual:G9})",
					$"Values are not within {precision} decimal place{(precision == 1 ? "" : "s")}"
				);
		}

		/// <summary>
		/// Verifies that two <see cref="float"/> values are equal, within the number of decimal
		/// places given by <paramref name="precision"/>. The values are rounded before comparison.
		/// The rounding method to use is given by <paramref name="rounding" />
		/// </summary>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The value to be compared against</param>
		/// <param name="precision">The number of decimal places (valid values: 0-15)</param>
		/// <param name="rounding">Rounding method to use to process a number that is midway between two numbers</param>
		public static void Equal(
			float expected,
			float actual,
			int precision,
			MidpointRounding rounding)
		{
			var expectedRounded = Math.Round(expected, precision, rounding);
			var actualRounded = Math.Round(actual, precision, rounding);

			if (!object.Equals(expectedRounded, actualRounded))
				throw EqualException.ForMismatchedValues(
					$"{expectedRounded:G9} (rounded from {expected:G9})",
					$"{actualRounded:G9} (rounded from {actual:G9})",
					$"Values are not within {precision} decimal place{(precision == 1 ? "" : "s")}"
				);
		}

		/// <summary>
		/// Verifies that two <see cref="float"/> values are equal, within the tolerance given by
		/// <paramref name="tolerance"/> (positive or negative).
		/// </summary>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The value to be compared against</param>
		/// <param name="tolerance">The allowed difference between values</param>
		public static void Equal(
			float expected,
			float actual,
			float tolerance)
		{
			if (float.IsNaN(tolerance) || float.IsNegativeInfinity(tolerance) || tolerance < 0.0)
				throw new ArgumentException("Tolerance must be greater than or equal to zero", nameof(tolerance));

			if (!(object.Equals(expected, actual) || Math.Abs(expected - actual) <= tolerance))
				throw EqualException.ForMismatchedValues(
					expected.ToString("G9"),
					actual.ToString("G9"),
					$"Values are not within tolerance {tolerance:G9}"
				);
		}

		/// <summary>
		/// Verifies that two <see cref="decimal"/> values are equal, within the number of decimal
		/// places given by <paramref name="precision"/>. The values are rounded before comparison.
		/// </summary>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The value to be compared against</param>
		/// <param name="precision">The number of decimal places (valid values: 0-28)</param>
		public static void Equal(
			decimal expected,
			decimal actual,
			int precision)
		{
			var expectedRounded = Math.Round(expected, precision);
			var actualRounded = Math.Round(actual, precision);

			if (expectedRounded != actualRounded)
				throw EqualException.ForMismatchedValues($"{expectedRounded} (rounded from {expected})", $"{actualRounded} (rounded from {actual})");
		}

		/// <summary>
		/// Verifies that two <see cref="DateTime"/> values are equal.
		/// </summary>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The value to be compared against</param>
		public static void Equal(
			DateTime expected,
			DateTime actual) =>
				Equal(expected, actual, TimeSpan.Zero);

		/// <summary>
		/// Verifies that two <see cref="DateTime"/> values are equal, within the precision
		/// given by <paramref name="precision"/>.
		/// </summary>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The value to be compared against</param>
		/// <param name="precision">The allowed difference in time where the two dates are considered equal</param>
		public static void Equal(
			DateTime expected,
			DateTime actual,
			TimeSpan precision)
		{
			var difference = (expected - actual).Duration();

			if (difference > precision)
			{
				var actualValue =
					ArgumentFormatter2.Format(actual) +
					(precision == TimeSpan.Zero ? "" : $" (difference {difference} is larger than {precision})");

				throw EqualException.ForMismatchedValues(expected, actualValue);
			}
		}

		/// <summary>
		/// Verifies that two <see cref="DateTimeOffset"/> values are equal.
		/// </summary>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The value to be compared against</param>
		public static void Equal(
			DateTimeOffset expected,
			DateTimeOffset actual) =>
				Equal(expected, actual, TimeSpan.Zero);

		/// <summary>
		/// Verifies that two <see cref="DateTimeOffset"/> values are equal, within the precision
		/// given by <paramref name="precision"/>.
		/// </summary>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The value to be compared against</param>
		/// <param name="precision">The allowed difference in time where the two dates are considered equal</param>
		public static void Equal(
			DateTimeOffset expected,
			DateTimeOffset actual,
			TimeSpan precision)
		{
			var difference = (expected - actual).Duration();

			if (difference > precision)
			{
				var actualValue =
					ArgumentFormatter2.Format(actual) +
					(precision == TimeSpan.Zero ? "" : $" (difference {difference} is larger than {precision})");

				throw EqualException.ForMismatchedValues(expected, actualValue);
			}
		}

#if XUNIT_SPAN
		/// <summary>
		/// Verifies that two arrays of un-managed type T are not equal, using Span&lt;T&gt;.SequenceEqual.
		/// </summary>
		/// <typeparam name="T">The type of items whose arrays are to be compared</typeparam>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The value to be compared against</param>
		public static void NotEqual<T>(
#if XUNIT_NULLABLE
			[AllowNull] T[] expected,
			[AllowNull] T[] actual)
				where T : unmanaged, IEquatable<T>
#else
			T[] expected,
			T[] actual)
				where T : IEquatable<T>
#endif
		{
			// Call into NotEqual<object> so we get proper formatting of the sequence
			if (expected == null && actual == null)
				NotEqual<object>(expected, actual);
			if (expected == null || actual == null)
				return;
			if (expected.AsSpan().SequenceEqual(actual))
				NotEqual<object>(expected, actual);
		}
#endif

		/// <summary>
		/// Verifies that two objects are not equal, using a default comparer.
		/// </summary>
		/// <typeparam name="T">The type of the objects to be compared</typeparam>
		/// <param name="expected">The expected object</param>
		/// <param name="actual">The actual object</param>
		public static void NotEqual<T>(
#if XUNIT_NULLABLE
			[AllowNull] T expected,
			[AllowNull] T actual) =>
#else
			T expected,
			T actual) =>
#endif
				NotEqual(expected, actual, GetEqualityComparer<T>());

		/// <summary>
		/// Verifies that two objects are not equal, using a custom equality comparer.
		/// </summary>
		/// <typeparam name="T">The type of the objects to be compared</typeparam>
		/// <param name="expected">The expected object</param>
		/// <param name="actual">The actual object</param>
		/// <param name="comparer">The comparer used to examine the objects</param>
		public static void NotEqual<T>(
#if XUNIT_NULLABLE
			[AllowNull] T expected,
			[AllowNull] T actual,
#else
			T expected,
			T actual,
#endif
			IEqualityComparer<T> comparer)
		{
			GuardArgumentNotNull(nameof(comparer), comparer);

			if (comparer.Equals(expected, actual))
				throw new NotEqualException(ArgumentFormatter.Format(expected), ArgumentFormatter.Format(actual));
		}

		/// <summary>
		/// Verifies that two <see cref="double"/> values are not equal, within the number of decimal
		/// places given by <paramref name="precision"/>.
		/// </summary>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The value to be compared against</param>
		/// <param name="precision">The number of decimal places (valid values: 0-15)</param>
		public static void NotEqual(
			double expected,
			double actual,
			int precision)
		{
			var expectedRounded = Math.Round(expected, precision);
			var actualRounded = Math.Round(actual, precision);

			if (object.Equals(expectedRounded, actualRounded))
				throw new NotEqualException($"{expectedRounded} (rounded from {expected})", $"{actualRounded} (rounded from {actual})");
		}

		/// <summary>
		/// Verifies that two <see cref="decimal"/> values are not equal, within the number of decimal
		/// places given by <paramref name="precision"/>.
		/// </summary>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The value to be compared against</param>
		/// <param name="precision">The number of decimal places (valid values: 0-28)</param>
		public static void NotEqual(
			decimal expected,
			decimal actual,
			int precision)
		{
			var expectedRounded = Math.Round(expected, precision);
			var actualRounded = Math.Round(actual, precision);

			if (expectedRounded == actualRounded)
				throw new NotEqualException($"{expectedRounded} (rounded from {expected})", $"{actualRounded} (rounded from {actual})");
		}

		/// <summary>
		/// Verifies that two objects are strictly not equal, using the type's default comparer.
		/// </summary>
		/// <typeparam name="T">The type of the objects to be compared</typeparam>
		/// <param name="expected">The expected object</param>
		/// <param name="actual">The actual object</param>
		public static void NotStrictEqual<T>(
#if XUNIT_NULLABLE
			[AllowNull] T expected,
			[AllowNull] T actual) =>
				NotEqual(expected, actual, EqualityComparer<T?>.Default);
#else
			T expected,
			T actual) =>
				NotEqual(expected, actual, EqualityComparer<T>.Default);
#endif

		/// <summary>
		/// Verifies that two objects are strictly equal, using the type's default comparer.
		/// </summary>
		/// <typeparam name="T">The type of the objects to be compared</typeparam>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The value to be compared against</param>
		public static void StrictEqual<T>(
#if XUNIT_NULLABLE
			[AllowNull] T expected,
			[AllowNull] T actual) =>
				Equal(expected, actual, EqualityComparer<T?>.Default);
#else
			T expected,
			T actual) =>
				Equal(expected, actual, EqualityComparer<T>.Default);
#endif
	}
}
