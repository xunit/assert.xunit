#if XUNIT_AOT

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xunit.Sdk;

namespace Xunit;

#if XUNIT_VISIBILITY_INTERNAL
internal
#else
public
#endif
partial class Assert
{
	const string SET_COMPARISON_WITH_FUNC_NOT_SUPPORTED = "Set comparisons with comparison functions are not supported. For more information, see https://xunit.net/docs/hash-sets-vs-linear-containers";

	/// <summary>
	///
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	/// <param name="expected"></param>
	/// <param name="actual"></param>
	public static void Equal<TKey, TValue>(
		KeyValuePair<TKey, TValue>? expected,
		KeyValuePair<TKey, TValue>? actual)
	{
		if (expected is null)
		{
			if (actual is null)
				return;

			throw EqualException.ForMismatchedValues(expected, actual, "?1");
		}

		if (actual is null)
			throw EqualException.ForMismatchedValues(expected, actual, "?2");

		var keyComparer = new AssertEqualityComparer<TKey>();
		if (!keyComparer.Equals(expected.Value.Key, actual.Value.Key))
			throw EqualException.ForMismatchedValues(expected, actual, "?3");

		var valueComparer = new AssertEqualityComparer<TValue>();
		if (!valueComparer.Equals(expected.Value.Value, actual.Value.Value))
			throw EqualException.ForMismatchedValues(expected, actual, "?4");

		throw new InvalidOperationException("Yo?");
	}

	/// <summary>
	/// Verifies that a set contains the same items as the <paramref name="expected"/> collection.
	/// </summary>
	/// <typeparam name="T">The type of the items in the set</typeparam>
	/// <param name="expected">The expected items to be in the set</param>
	/// <param name="actual">The actual set</param>
	/// <exception cref="EqualException">Thrown if the set is not equal</exception>
	[OverloadResolutionPriority(1)]
	public static void Equal<T>(
		IEnumerable<T> expected,
		ISet<T>? actual)
	{
		GuardArgumentNotNull(nameof(expected), expected);

		if (actual is not null && actual.SetEquals(expected))
			return;

		SetEqualFailure(expected, actual);
	}

	/// <summary>
	/// Verifies that a set contains the same items as the <paramref name="expected"/> collection, using
	/// the given <paramref name="comparer"/>.
	/// </summary>
	/// <typeparam name="T">The type of the items in the set</typeparam>
	/// <param name="expected">The expected items to be in the set</param>
	/// <param name="actual">The actual set</param>
	/// <param name="comparer">The item comparerer to use</param>
	/// <exception cref="EqualException">Thrown if the set is not equal</exception>
	/// <remarks>
	/// Note that this creates a new hash set with the given comparer, using the items from <paramref name="actual"/>.
	/// Because the comparer may create equality differences from the one <paramref name="actual"/> was created with,
	/// the items in the compared container may differ from the one that was passed, since sets are designed to
	/// eliminated duplicate (equal) items.
	/// </remarks>
	[OverloadResolutionPriority(1)]
	public static void Equal<T>(
		IEnumerable<T> expected,
		ISet<T>? actual,
		IEqualityComparer<T> comparer) =>
			Equal(expected, actual == null ? null : new HashSet<T>(actual, comparer));

	/// <summary/>
	[Obsolete(SET_COMPARISON_WITH_FUNC_NOT_SUPPORTED, error: true)]
	[OverloadResolutionPriority(1)]
	public static void Equal<T>(
		IEnumerable<T> expected,
		ISet<T>? actual,
		Func<T, T, bool> comparer) =>
			throw new NotSupportedException(SET_COMPARISON_WITH_FUNC_NOT_SUPPORTED);

	/// <summary>
	/// Verifies that a set contains the same items as the <paramref name="expected"/> collection.
	/// </summary>
	/// <typeparam name="T">The type of the items in the set</typeparam>
	/// <param name="expected">The expected items to be in the set</param>
	/// <param name="actual">The actual set</param>
	/// <exception cref="EqualException">Thrown if the set is not equal</exception>
	[OverloadResolutionPriority(2)]
	public static void Equal<T>(
		IEnumerable<T> expected,
		IReadOnlySet<T>? actual)
	{
		GuardArgumentNotNull(nameof(expected), expected);

		if (actual is not null && actual.SetEquals(expected))
			return;

		SetEqualFailure(expected, actual);
	}

	/// <summary>
	/// Verifies that a set contains the same items as the <paramref name="expected"/> collection, using
	/// the given <paramref name="comparer"/>.
	/// </summary>
	/// <typeparam name="T">The type of the items in the set</typeparam>
	/// <param name="expected">The expected items to be in the set</param>
	/// <param name="actual">The actual set</param>
	/// <param name="comparer">The item comparerer to use</param>
	/// <exception cref="EqualException">Thrown if the set is not equal</exception>
	/// <remarks>
	/// Note that this creates a new hash set with the given comparer, using the items from <paramref name="actual"/>.
	/// Because the comparer may create equality differences from the one <paramref name="actual"/> was created with,
	/// the items in the compared container may differ from the one that was passed, since sets are designed to
	/// eliminated duplicate (equal) items.
	/// </remarks>
	[OverloadResolutionPriority(2)]
	public static void Equal<T>(
		IEnumerable<T> expected,
		IReadOnlySet<T>? actual,
		IEqualityComparer<T> comparer) =>
			Equal(expected, actual == null ? null : new HashSet<T>(actual, comparer));

	/// <summary/>
	[Obsolete(SET_COMPARISON_WITH_FUNC_NOT_SUPPORTED, error: true)]
	[OverloadResolutionPriority(2)]
	public static void Equal<T>(
		IEnumerable<T> expected,
		IReadOnlySet<T>? actual,
		Func<T, T, bool> comparer) =>
			throw new NotSupportedException(SET_COMPARISON_WITH_FUNC_NOT_SUPPORTED);

	/// <summary>
	/// Verifies that a set does not contain the same items as the <paramref name="expected"/> collection.
	/// </summary>
	/// <typeparam name="T">The type of the items in the set</typeparam>
	/// <param name="expected">The expected items to be in the set</param>
	/// <param name="actual">The actual set</param>
	/// <exception cref="NotEqualException">Thrown if the set is equal</exception>
	[OverloadResolutionPriority(1)]
	public static void NotEqual<T>(
		IEnumerable<T> expected,
		ISet<T>? actual)
	{
		GuardArgumentNotNull(nameof(expected), expected);

		if (actual is not null && !actual.SetEquals(expected))
			return;

		SetNotEqualFailure(expected, actual);
	}

	/// <summary>
	/// Verifies that a set does not contain the same items as the <paramref name="expected"/> collection, using
	/// the given <paramref name="comparer"/>.
	/// </summary>
	/// <typeparam name="T">The type of the items in the set</typeparam>
	/// <param name="expected">The expected items to be in the set</param>
	/// <param name="actual">The actual set</param>
	/// <param name="comparer">The item comparerer to use</param>
	/// <exception cref="NotEqualException">Thrown if the set is equal</exception>
	/// <remarks>
	/// Note that this creates a new hash set with the given comparer, using the items from <paramref name="actual"/>.
	/// Because the comparer may create equality differences from the one <paramref name="actual"/> was created with,
	/// the items in the compared container may differ from the one that was passed, since sets are designed to
	/// eliminated duplicate (equal) items.
	/// </remarks>
	[OverloadResolutionPriority(1)]
	public static void NotEqual<T>(
		IEnumerable<T> expected,
		ISet<T>? actual,
		IEqualityComparer<T> comparer) =>
			NotEqual(expected, actual == null ? null : new HashSet<T>(actual, comparer));

	/// <summary/>
	[Obsolete(SET_COMPARISON_WITH_FUNC_NOT_SUPPORTED, error: true)]
	[OverloadResolutionPriority(1)]
	public static void NotEqual<T>(
		IEnumerable<T> expected,
		ISet<T>? actual,
		Func<T, T, bool> comparer) =>
			throw new NotSupportedException(SET_COMPARISON_WITH_FUNC_NOT_SUPPORTED);

	/// <summary>
	/// Verifies that a set does not contain the same items as the <paramref name="expected"/> collection.
	/// </summary>
	/// <typeparam name="T">The type of the items in the set</typeparam>
	/// <param name="expected">The expected items to be in the set</param>
	/// <param name="actual">The actual set</param>
	/// <exception cref="NotEqualException">Thrown if the set is equal</exception>
	[OverloadResolutionPriority(2)]
	public static void NotEqual<T>(
		IEnumerable<T> expected,
		IReadOnlySet<T>? actual)
	{
		GuardArgumentNotNull(nameof(expected), expected);

		if (actual is not null && !actual.SetEquals(expected))
			return;

		SetNotEqualFailure(expected, actual);
	}

	/// <summary>
	/// Verifies that a set does not contain the same items as the <paramref name="expected"/> collection, using
	/// the given <paramref name="comparer"/>.
	/// </summary>
	/// <typeparam name="T">The type of the items in the set</typeparam>
	/// <param name="expected">The expected items to be in the set</param>
	/// <param name="actual">The actual set</param>
	/// <param name="comparer">The item comparerer to use</param>
	/// <exception cref="NotEqualException">Thrown if the set is equal</exception>
	/// <remarks>
	/// Note that this creates a new hash set with the given comparer, using the items from <paramref name="actual"/>.
	/// Because the comparer may create equality differences from the one <paramref name="actual"/> was created with,
	/// the items in the compared container may differ from the one that was passed, since sets are designed to
	/// eliminated duplicate (equal) items.
	/// </remarks>
	[OverloadResolutionPriority(2)]
	public static void NotEqual<T>(
		IEnumerable<T> expected,
		IReadOnlySet<T>? actual,
		IEqualityComparer<T> comparer) =>
			NotEqual(expected, actual == null ? null : new HashSet<T>(actual, comparer));

	/// <summary/>
	[Obsolete(SET_COMPARISON_WITH_FUNC_NOT_SUPPORTED, error: true)]
	[OverloadResolutionPriority(2)]
	public static void NotEqual<T>(
		IEnumerable<T> expected,
		IReadOnlySet<T>? actual,
		Func<T, T, bool> comparer) =>
			throw new NotSupportedException(SET_COMPARISON_WITH_FUNC_NOT_SUPPORTED);

	static void SetEqualFailure<T>(
		IEnumerable<T> expected,
		IEnumerable<T>? actual)
	{
		var expectedFormatted = CollectionTracker<T>.FormatStart(expected);
		var actualFormatted = actual is null ? "null" : CollectionTracker<T>.FormatStart(actual);
		var expectedTypeFormatted = default(string);
		var actualTypeFormatted = default(string);
		var expectedType = expected.GetType();
		var actualType = actual?.GetType();

		if (actualType is not null && expectedType != actualType)
		{
			expectedTypeFormatted = ArgumentFormatter.FormatTypeName(expectedType);
			actualTypeFormatted = ArgumentFormatter.FormatTypeName(actualType);
		}

		var expectedTypeDefinition = SafeGetGenericTypeDefinition(expectedType);
		var actualTypeDefinition = SafeGetGenericTypeDefinition(actualType);
		var collectionDisplay =
			expectedTypeDefinition == typeofHashSet && actualTypeDefinition == typeofHashSet
				? "HashSets"
				: "Sets";

		throw EqualException.ForMismatchedSets(expectedFormatted, expectedTypeFormatted, actualFormatted, actualTypeFormatted, collectionDisplay);
	}

	static void SetNotEqualFailure<T>(
		IEnumerable<T> expected,
		IEnumerable<T>? actual)
	{
		var expectedFormatted = CollectionTracker<T>.FormatStart(expected);
		var actualFormatted = actual is null ? "null" : CollectionTracker<T>.FormatStart(actual);
		var expectedTypeFormatted = default(string);
		var actualTypeFormatted = default(string);
		var expectedType = expected.GetType();
		var actualType = actual?.GetType();

		if (actualType is not null && expectedType != actualType)
		{
			expectedTypeFormatted = ArgumentFormatter.FormatTypeName(expectedType);
			actualTypeFormatted = ArgumentFormatter.FormatTypeName(actualType);
		}

		var expectedTypeDefinition = SafeGetGenericTypeDefinition(expectedType);
		var actualTypeDefinition = SafeGetGenericTypeDefinition(actualType);
		var collectionDisplay =
			expectedTypeDefinition == typeofHashSet && actualTypeDefinition == typeofHashSet
				? "HashSets"
				: "Sets";

		throw NotEqualException.ForEqualSets(expectedFormatted, expectedTypeFormatted, actualFormatted, actualTypeFormatted, collectionDisplay);
	}
}

#endif  // XUNIT_AOT
