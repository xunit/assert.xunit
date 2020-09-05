using System;
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
		//NOTE: ref struct types (Span, ReadOnlySpan) are not Nullable, and thus there is no XUNIT_NULLABLE usage currently in this class
		//		This also means that null spans are identical to empty spans, (both in essence point to a 0 sized array of whatever type)

		//NOTE: we could consider StartsWith<T> and EndsWith<T> and use the Span extension methods to check difference, but, the current 
		//		Exceptions for startswith and endswith are only built for string types, so those would need a change (or new non-string versions created).

		//NOTE: there is an implicit conversion operator on Span<T> to ReadOnlySpan<T> - however, I have found that the compiler sometimes struggles
		//		with identifying the proper methods to use, thus I have overloaded quite a few of the assertions in terms of supplying both
		//		Span and ReadOnlySpan based methods

		/// <summary>
		/// Verifies that a span contains a given sub-span, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedSubSpan">The sub-span expected to be in the span</param>
		/// <param name="actualSpan">The span to be inspected</param>
		/// <exception cref="ContainsException">Thrown when the sub-span is not present inside the span</exception>
		public static void Contains(Span<char> expectedSubSpan, ReadOnlySpan<char> actualSpan)
			=> Contains(expectedSubSpan, actualSpan, StringComparison.CurrentCulture);

		/// <summary>
		/// Verifies that a span contains a given sub-span, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedSubSpan">The sub-span expected to be in the span</param>
		/// <param name="actualSpan">The span to be inspected</param>
		/// <exception cref="ContainsException">Thrown when the sub-span is not present inside the span</exception>
		public static void Contains(ReadOnlySpan<char> expectedSubSpan, ReadOnlySpan<char> actualSpan)
			=> Contains(expectedSubSpan, actualSpan, StringComparison.CurrentCulture);


		/// <summary>
		/// Verifies that a span contains a given sub-span, using the given comparison type.
		/// </summary>
		/// <param name="expectedSubSpan">The sub-span expected to be in the span</param>
		/// <param name="actualSpan">The span to be inspected</param>
		/// <param name="comparisonType">The type of string comparison to perform</param>
		/// <exception cref="ContainsException">Thrown when the sub-span is not present inside the span</exception>
		public static void Contains(ReadOnlySpan<char> expectedSubSpan, ReadOnlySpan<char> actualSpan, StringComparison comparisonType = StringComparison.CurrentCulture)
			=> Contains(expectedSubSpan.ToString(), actualSpan.ToString(), comparisonType);

		/// <summary>
		/// Verifies that a span contains a given sub-span
		/// </summary>
		/// <param name="expectedSubSpan">The sub-span expected to be in the span</param>
		/// <param name="actualSpan">The span to be inspected</param>
		/// <exception cref="ContainsException">Thrown when the sub-span is not present inside the span</exception>
		public static void Contains<T>(Span<T> expectedSubSpan, ReadOnlySpan<T> actualSpan) where T : IEquatable<T>
		{
			if (actualSpan == null || actualSpan.IndexOf(expectedSubSpan) < 0)
				throw ContainsException.Create(expectedSubSpan, actualSpan);
		}

		/// <summary>
		/// Verifies that a span contains a given sub-span
		/// </summary>
		/// <param name="expectedSubSpan">The sub-span expected to be in the span</param>
		/// <param name="actualSpan">The span to be inspected</param>
		/// <exception cref="ContainsException">Thrown when the sub-span is not present inside the span</exception>
		public static void Contains<T>(ReadOnlySpan<T> expectedSubSpan, ReadOnlySpan<T> actualSpan) where T : IEquatable<T>
		{
			if (actualSpan == null || actualSpan.IndexOf(expectedSubSpan) < 0)
				throw ContainsException.Create(expectedSubSpan, actualSpan);
		}

		/// <summary>
		/// Verifies that a span does not contain a given sub-span, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedSubSpan">The sub-span expected not to be in the span</param>
		/// <param name="actualSpan">The span to be inspected</param>
		/// <exception cref="DoesNotContainException">Thrown when the sub-span is present inside the span</exception>
		public static void DoesNotContain(Span<char> expectedSubSpan, ReadOnlySpan<char> actualSpan)
			=> DoesNotContain(expectedSubSpan, actualSpan, StringComparison.CurrentCulture);

		/// <summary>
		/// Verifies that a span does not contain a given sub-span, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedSubSpan">The sub-span expected not to be in the span</param>
		/// <param name="actualSpan">The span to be inspected</param>
		/// <exception cref="DoesNotContainException">Thrown when the sub-span is present inside the span</exception>
		public static void DoesNotContain(ReadOnlySpan<char> expectedSubSpan, ReadOnlySpan<char> actualSpan)
			=> DoesNotContain(expectedSubSpan, actualSpan, StringComparison.CurrentCulture);


		/// <summary>
		/// Verifies that a span does not contain a given sub-span, using the given comparison type.
		/// </summary>
		/// <param name="expectedSubSpan">The sub-span expected not to be in the span</param>
		/// <param name="actualSpan">The span to be inspected</param>
		/// <param name="comparisonType">The type of string comparison to perform</param>
		/// <exception cref="DoesNotContainException">Thrown when the sub-span is present inside the span</exception>
		public static void DoesNotContain(ReadOnlySpan<char> expectedSubSpan, ReadOnlySpan<char> actualSpan, StringComparison comparisonType = StringComparison.CurrentCulture)
			=> DoesNotContain(expectedSubSpan.ToString(), actualSpan.ToString(), comparisonType);

		/// <summary>
		/// Verifies that a span does not contain a given sub-span
		/// </summary>
		/// <param name="expectedSubSpan">The sub-span expected not to be in the span</param>
		/// <param name="actualSpan">The span to be inspected</param>
		/// <exception cref="DoesNotContainException">Thrown when the sub-span is present inside the span</exception>
		public static void DoesNotContain<T>(ReadOnlySpan<T> expectedSubSpan, ReadOnlySpan<T> actualSpan) where T : IEquatable<T>
		{
			if (actualSpan != null && actualSpan.IndexOf(expectedSubSpan) >= 0)
				throw DoesNotContainException.Create(expectedSubSpan, actualSpan);
		}

		/// <summary>
		/// Verifies that a span does not contain a given sub-span
		/// </summary>
		/// <param name="expectedSubSpan">The sub-span expected not to be in the span</param>
		/// <param name="actualSpan">The span to be inspected</param>
		/// <exception cref="DoesNotContainException">Thrown when the sub-span is present inside the span</exception>
		public static void DoesNotContain<T>(Span<T> expectedSubSpan, ReadOnlySpan<T> actualSpan) where T : IEquatable<T>
		{
			if (actualSpan != null && actualSpan.IndexOf(expectedSubSpan) >= 0)
				throw DoesNotContainException.Create(expectedSubSpan, actualSpan);
		}

		/// <summary>
		/// Verifies that a span starts with a given sub-span, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedStartSpan">The sub-span expected to be at the start of the span</param>
		/// <param name="actualSpan">The span to be inspected</param>
		/// <exception cref="StartsWithException">Thrown when the span does not start with the expected subspan</exception>
		public static void StartsWith(Span<char> expectedStartSpan, ReadOnlySpan<char> actualSpan)
			=> StartsWith(expectedStartSpan, actualSpan, StringComparison.CurrentCulture);

		/// <summary>
		/// Verifies that a span starts with a given sub-span, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedStartSpan">The sub-span expected to be at the start of the span</param>
		/// <param name="actualSpan">The span to be inspected</param>
		/// <exception cref="StartsWithException">Thrown when the span does not start with the expected subspan</exception>
		public static void StartsWith(ReadOnlySpan<char> expectedStartSpan, ReadOnlySpan<char> actualSpan)
			=> StartsWith(expectedStartSpan, actualSpan, StringComparison.CurrentCulture);

		/// <summary>
		/// Verifies that a span starts with a given sub-span, using the given comparison type.
		/// </summary>
		/// <param name="expectedStartSpan">The sub-span expected to be at the start of the span</param>
		/// <param name="actualSpan">The span to be inspected</param>
		/// <param name="comparisonType">The type of string comparison to perform</param>
		/// <exception cref="StartsWithException">Thrown when the span does not start with the expected subspan</exception>
		public static void StartsWith(ReadOnlySpan<char> expectedStartSpan, ReadOnlySpan<char> actualSpan, StringComparison comparisonType = StringComparison.CurrentCulture)
			=> StartsWith(expectedStartSpan.ToString(), actualSpan.ToString(), comparisonType);

		/// <summary>
		/// Verifies that a span ends with a given sub-span, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedEndSpan">The sub-span expected to be at the end of the span</param>
		/// <param name="actualSpan">The span to be inspected</param>
		/// <exception cref="EndsWithException">Thrown when the span does not end with the expected subspan</exception>
		public static void EndsWith(Span<char> expectedEndSpan, ReadOnlySpan<char> actualSpan)
			=> EndsWith(expectedEndSpan, actualSpan, StringComparison.CurrentCulture);

		/// <summary>
		/// Verifies that a span ends with a given sub-span, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedEndSpan">The sub-span expected to be at the end of the span</param>
		/// <param name="actualSpan">The span to be inspected</param>
		/// <exception cref="EndsWithException">Thrown when the span does not end with the expected subspan</exception>
		public static void EndsWith(ReadOnlySpan<char> expectedEndSpan, ReadOnlySpan<char> actualSpan)
			=> EndsWith(expectedEndSpan, actualSpan, StringComparison.CurrentCulture);

		/// <summary>
		/// Verifies that a span ends with a given sub-span, using the given comparison type.
		/// </summary>
		/// <param name="expectedEndSpan">The sub-span expected to be at the end of the span</param>
		/// <param name="actualSpan">The span to be inspected</param>
		/// <param name="comparisonType">The type of string comparison to perform</param>
		/// <exception cref="EndsWithException">Thrown when the span does not end with the expected subspan</exception>
		public static void EndsWith(ReadOnlySpan<char> expectedEndSpan, ReadOnlySpan<char> actualSpan, StringComparison comparisonType = StringComparison.CurrentCulture)
			=> EndsWith(expectedEndSpan.ToString(), actualSpan.ToString(), comparisonType);

		/// <summary>
		/// Verifies that two spans are equivalent.
		/// </summary>
		/// <param name="expectedSpan">The expected span value.</param>
		/// <param name="actualSpan">The actual span value.</param>
		/// <exception cref="EqualException">Thrown when the spans are not equivalent.</exception>
		public static void Equal(Span<char> expectedSpan, ReadOnlySpan<char> actualSpan)
			=> Equal(expectedSpan, actualSpan, false, false, false);

		/// <summary>
		/// Verifies that two spans are equivalent.
		/// </summary>
		/// <param name="expectedSpan">The expected span value.</param>
		/// <param name="actualSpan">The actual span value.</param>
		/// <exception cref="EqualException">Thrown when the spans are not equivalent.</exception>
		public static void Equal(ReadOnlySpan<char> expectedSpan, ReadOnlySpan<char> actualSpan)
			=> Equal(expectedSpan, actualSpan, false, false, false);

		/// <summary>
		/// Verifies that two spans are equivalent.
		/// </summary>
		/// <param name="expectedSpan">The expected span value.</param>
		/// <param name="actualSpan">The actual span value.</param>
		/// <param name="ignoreCase">If set to <c>true</c>, ignores cases differences. The invariant culture is used.</param>
		/// <param name="ignoreLineEndingDifferences">If set to <c>true</c>, treats \r\n, \r, and \n as equivalent.</param>
		/// <param name="ignoreWhiteSpaceDifferences">If set to <c>true</c>, treats spaces and tabs (in any non-zero quantity) as equivalent.</param>
		/// <exception cref="EqualException">Thrown when the spans are not equivalent.</exception>
		public static void Equal(Span<char> expectedSpan, ReadOnlySpan<char> actualSpan, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false)
			=> Equal(expectedSpan.ToString(), actualSpan.ToString(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences);

		/// <summary>
		/// Verifies that two spans are equivalent.
		/// </summary>
		/// <param name="expectedSpan">The expected span value.</param>
		/// <param name="actualSpan">The actual span value.</param>
		/// <param name="ignoreCase">If set to <c>true</c>, ignores cases differences. The invariant culture is used.</param>
		/// <param name="ignoreLineEndingDifferences">If set to <c>true</c>, treats \r\n, \r, and \n as equivalent.</param>
		/// <param name="ignoreWhiteSpaceDifferences">If set to <c>true</c>, treats spaces and tabs (in any non-zero quantity) as equivalent.</param>
		/// <exception cref="EqualException">Thrown when the spans are not equivalent.</exception>
		public static void Equal(ReadOnlySpan<char> expectedSpan, ReadOnlySpan<char> actualSpan, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false)
			=> Equal(expectedSpan.ToString(), actualSpan.ToString(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences);

		/// <summary>
		/// Verifies that two spans are equivalent.
		/// </summary>
		/// <param name="expectedSpan">The expected span value.</param>
		/// <param name="actualSpan">The actual span value.</param>
		/// <exception cref="EqualException">Thrown when the spans are not equivalent.</exception>
		public static void Equal<T>(ReadOnlySpan<T> expectedSpan, ReadOnlySpan<T> actualSpan) where T : IEquatable<T>
			=> Equal(expectedSpan.ToArray(), actualSpan.ToArray(), GetEqualityComparer<T>());

		/// <summary>
		/// Verifies that two spans are equivalent.
		/// </summary>
		/// <param name="expectedSpan">The expected span value.</param>
		/// <param name="actualSpan">The actual span value.</param>
		/// <exception cref="EqualException">Thrown when the spans are not equivalent.</exception>
		public static void Equal<T>(Span<T> expectedSpan, ReadOnlySpan<T> actualSpan) where T : IEquatable<T>
			=> Equal(expectedSpan.ToArray(), actualSpan.ToArray(), GetEqualityComparer<T>());

		/// <summary>
		/// Verifies that two spans are equivalent.
		/// </summary>
		/// <param name="expectedSpan">The expected span value.</param>
		/// <param name="actualSpan">The actual span value.</param>
		/// <exception cref="EqualException">Thrown when the spans are not equivalent.</exception>
		public static void Equal<T>(Span<T> expectedSpan, Span<T> actualSpan) where T : IEquatable<T>
			=> Equal(expectedSpan.ToArray(), actualSpan.ToArray(), GetEqualityComparer<T>());

		/// <summary>
		/// Verifies that two spans are equivalent.
		/// </summary>
		/// <param name="expectedSpan">The expected span value.</param>
		/// <param name="actualSpan">The actual span value.</param>
		/// <exception cref="EqualException">Thrown when the spans are not equivalent.</exception>
		public static void Equal<T>(ReadOnlySpan<T> expectedSpan, Span<T> actualSpan) where T : IEquatable<T>
			=> Equal(expectedSpan.ToArray(), actualSpan.ToArray(), GetEqualityComparer<T>());
	}
}
