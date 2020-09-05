#if XUNIT_NULLABLE
#nullable enable
# endif

using System;
using System.Diagnostics;
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
		//NOTE: there is an implicit conversion operator on Memory<T> to ReadOnlyMemory<T> - however, I have found that the compiler sometimes struggles
		//		with identifying the proper methods to use, thus I have overloaded quite a few of the assertions in terms of supplying both
		//		Memory and ReadOnlyMemory based methods

		//NOTE: we could consider StartsWith<T> and EndsWith<T> with both arguments as ReadOnlyMemory<T>, and use the Memory extension methods on Span to check difference
		//		BUT: the current Exceptions for startswith and endswith are only built for string types, so those would need a change (or new non-string versions created).

		//NOTE: Memory and ReadonlyMemory, even when null, are coerced into empty arrays of the specified type when a value is grabbed. Thus some of the code below
		//		for null scenarios looks odd, but is safe and correct.

		/// <summary>
		/// Verifies that a Memory contains a given sub-Memory, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedSubMemory">The sub-Memory expected to be in the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="ContainsException">Thrown when the sub-Memory is not present inside the Memory</exception>
#if XUNIT_NULLABLE
		public static void Contains(Memory<char> expectedSubMemory, ReadOnlyMemory<char>? actualMemory)
#else
		public static void Contains(Memory<char> expectedSubMemory, ReadOnlyMemory<char> actualMemory)
#endif
			=> Contains(expectedSubMemory, actualMemory, StringComparison.CurrentCulture);

		/// <summary>
		/// Verifies that a Memory contains a given sub-Memory, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedSubMemory">The sub-Memory expected to be in the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="ContainsException">Thrown when the sub-Memory is not present inside the Memory</exception>
#if XUNIT_NULLABLE
		public static void Contains(ReadOnlyMemory<char> expectedSubMemory, ReadOnlyMemory<char>? actualMemory)
#else
		public static void Contains(ReadOnlyMemory<char> expectedSubMemory, ReadOnlyMemory<char> actualMemory)
#endif
			=> Contains(expectedSubMemory, actualMemory, StringComparison.CurrentCulture);


		/// <summary>
		/// Verifies that a Memory contains a given sub-Memory, using the given comparison type.
		/// </summary>
		/// <param name="expectedSubMemory">The sub-Memory expected to be in the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <param name="comparisonType">The type of string comparison to perform</param>
		/// <exception cref="ContainsException">Thrown when the sub-Memory is not present inside the Memory</exception>
#if XUNIT_NULLABLE
		public static void Contains(ReadOnlyMemory<char> expectedSubMemory, ReadOnlyMemory<char>? actualMemory, StringComparison comparisonType = StringComparison.CurrentCulture)
#else
		public static void Contains(ReadOnlyMemory<char> expectedSubMemory, ReadOnlyMemory<char> actualMemory, StringComparison comparisonType = StringComparison.CurrentCulture)
#endif
		{
			GuardArgumentNotNull(nameof(expectedSubMemory), expectedSubMemory);
#if XUNIT_NULLABLE
			Contains(expectedSubMemory.ToString(), actualMemory?.ToString(), comparisonType);//use string comparison for it's strong output
#else
			Contains(expectedSubMemory.ToString(), actualMemory.ToString(), comparisonType);//use string comparison for it's strong output
#endif
		}

		/// <summary>
		/// Verifies that a Memory contains a given sub-Memory
		/// </summary>
		/// <param name="expectedSubMemory">The sub-Memory expected to be in the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="ContainsException">Thrown when the sub-Memory is not present inside the Memory</exception>
#if XUNIT_NULLABLE
		public static void Contains<T>(Memory<T> expectedSubMemory, ReadOnlyMemory<T>? actualMemory) where T : IEquatable<T>
#else
		public static void Contains<T>(Memory<T> expectedSubMemory, ReadOnlyMemory<T> actualMemory) where T : IEquatable<T>
#endif
			=> ContainsImp(expectedSubMemory, actualMemory);

		/// <summary>
		/// Verifies that a Memory contains a given sub-Memory
		/// </summary>
		/// <param name="expectedSubMemory">The sub-Memory expected to be in the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="ContainsException">Thrown when the sub-Memory is not present inside the Memory</exception>
#if XUNIT_NULLABLE
		public static void Contains<T>(ReadOnlyMemory<T> expectedSubMemory, ReadOnlyMemory<T>? actualMemory) where T : IEquatable<T>
#else
		public static void Contains<T>(ReadOnlyMemory<T> expectedSubMemory, ReadOnlyMemory<T> actualMemory) where T : IEquatable<T>
#endif
			=> ContainsImp(expectedSubMemory, actualMemory);

#if XUNIT_NULLABLE
		private static void ContainsImp<T>(ReadOnlyMemory<T> expectedSubMemory, ReadOnlyMemory<T>? actualMemory) where T : IEquatable<T>
#else
		private static void ContainsImp<T>(ReadOnlyMemory<T> expectedSubMemory, ReadOnlyMemory<T> actualMemory) where T : IEquatable<T>
#endif
		{
			GuardArgumentNotNull(nameof(expectedSubMemory), expectedSubMemory);
#if XUNIT_NULLABLE
			if (actualMemory == null || actualMemory.Value.Span.IndexOf(expectedSubMemory.Span) < 0)
#else
			if (actualMemory.Span.IndexOf(expectedSubMemory.Span) < 0)
#endif
				throw new ContainsException(expectedSubMemory, actualMemory);
		}

		/// <summary>
		/// Verifies that a Memory does not contain a given sub-Memory, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedSubMemory">The sub-Memory expected not to be in the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="DoesNotContainException">Thrown when the sub-Memory is present inside the Memory</exception>
#if XUNIT_NULLABLE
		public static void DoesNotContain(Memory<char> expectedSubMemory, ReadOnlyMemory<char>? actualMemory)
#else
		public static void DoesNotContain(Memory<char> expectedSubMemory, ReadOnlyMemory<char> actualMemory)
#endif
			=> DoesNotContain(expectedSubMemory, actualMemory, StringComparison.CurrentCulture);

		/// <summary>
		/// Verifies that a Memory does not contain a given sub-Memory, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedSubMemory">The sub-Memory expected not to be in the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="DoesNotContainException">Thrown when the sub-Memory is present inside the Memory</exception>
#if XUNIT_NULLABLE
		public static void DoesNotContain(ReadOnlyMemory<char> expectedSubMemory, ReadOnlyMemory<char>? actualMemory)
#else
		public static void DoesNotContain(ReadOnlyMemory<char> expectedSubMemory, ReadOnlyMemory<char> actualMemory)
#endif
			=> DoesNotContain(expectedSubMemory, actualMemory, StringComparison.CurrentCulture);


		/// <summary>
		/// Verifies that a Memory does not contain a given sub-Memory, using the given comparison type.
		/// </summary>
		/// <param name="expectedSubMemory">The sub-Memory expected not to be in the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <param name="comparisonType">The type of string comparison to perform</param>
		/// <exception cref="DoesNotContainException">Thrown when the sub-Memory is present inside the Memory</exception>
#if XUNIT_NULLABLE
		public static void DoesNotContain(ReadOnlyMemory<char> expectedSubMemory, ReadOnlyMemory<char>? actualMemory, StringComparison comparisonType = StringComparison.CurrentCulture)
#else
		public static void DoesNotContain(ReadOnlyMemory<char> expectedSubMemory, ReadOnlyMemory<char> actualMemory, StringComparison comparisonType = StringComparison.CurrentCulture)
#endif
		{
			GuardArgumentNotNull(nameof(expectedSubMemory), expectedSubMemory);
#if XUNIT_NULLABLE0
			DoesNotContain(expectedSubMemory.ToString(), actualMemory?.ToString(), comparisonType);//use string comparison for it's strong output
#else
			DoesNotContain(expectedSubMemory.ToString(), actualMemory.ToString(), comparisonType);//use string comparison for it's strong output
#endif
		}

		/// <summary>
		/// Verifies that a Memory does not contain a given sub-Memory
		/// </summary>
		/// <param name="expectedSubMemory">The sub-Memory expected not to be in the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="DoesNotContainException">Thrown when the sub-Memory is present inside the Memory</exception>
#if XUNIT_NULLABLE
		public static void DoesNotContain<T>(ReadOnlyMemory<T> expectedSubMemory, ReadOnlyMemory<T>? actualMemory) where T : IEquatable<T>
#else
		public static void DoesNotContain<T>(ReadOnlyMemory<T> expectedSubMemory, ReadOnlyMemory<T> actualMemory) where T : IEquatable<T>
#endif
			=> DoesNotContainImp(expectedSubMemory, actualMemory);

		/// <summary>
		/// Verifies that a Memory does not contain a given sub-Memory
		/// </summary>
		/// <param name="expectedSubMemory">The sub-Memory expected not to be in the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="DoesNotContainException">Thrown when the sub-Memory is present inside the Memory</exception>
#if XUNIT_NULLABLE
		public static void DoesNotContain<T>(Memory<T> expectedSubMemory, ReadOnlyMemory<T>? actualMemory) where T : IEquatable<T>
#else
		public static void DoesNotContain<T>(Memory<T> expectedSubMemory, ReadOnlyMemory<T> actualMemory) where T : IEquatable<T>
#endif
			=> DoesNotContainImp(expectedSubMemory, actualMemory);

#if XUNIT_NULLABLE
		private static void DoesNotContainImp<T>(ReadOnlyMemory<T> expectedSubMemory, ReadOnlyMemory<T>? actualMemory) where T : IEquatable<T>
#else
		private static void DoesNotContainImp<T>(ReadOnlyMemory<T> expectedSubMemory, ReadOnlyMemory<T> actualMemory) where T : IEquatable<T>
#endif
		{
			GuardArgumentNotNull(nameof(expectedSubMemory), expectedSubMemory);
#if XUNIT_NULLABLE
			if (actualMemory != null && actualMemory.Value.Span.IndexOf(expectedSubMemory.Span) >= 0)
#else
			if (actualMemory.Span.IndexOf(expectedSubMemory.Span) >= 0)
#endif
				throw new DoesNotContainException(expectedSubMemory, actualMemory);
		}

		/// <summary>
		/// Verifies that a Memory starts with a given sub-Memory, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedStartMemory">The sub-Memory expected to be at the start of the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="StartsWithException">Thrown when the Memory does not start with the expected subMemory</exception>
#if XUNIT_NULLABLE
		public static void StartsWith(Memory<char> expectedStartMemory, ReadOnlyMemory<char>? actualMemory)
#else
		public static void StartsWith(Memory<char> expectedStartMemory, ReadOnlyMemory<char> actualMemory)
#endif
		=> StartsWith(expectedStartMemory, actualMemory, StringComparison.CurrentCulture);

		/// <summary>
		/// Verifies that a Memory starts with a given sub-Memory, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedStartMemory">The sub-Memory expected to be at the start of the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="StartsWithException">Thrown when the Memory does not start with the expected subMemory</exception>
#if XUNIT_NULLABLE
		public static void StartsWith(ReadOnlyMemory<char> expectedStartMemory, ReadOnlyMemory<char>? actualMemory)
#else
		public static void StartsWith(ReadOnlyMemory<char> expectedStartMemory, ReadOnlyMemory<char> actualMemory)
#endif
			=> StartsWith(expectedStartMemory, actualMemory, StringComparison.CurrentCulture);

		/// <summary>
		/// Verifies that a Memory starts with a given sub-Memory, using the given comparison type.
		/// </summary>
		/// <param name="expectedStartMemory">The sub-Memory expected to be at the start of the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <param name="comparisonType">The type of string comparison to perform</param>
		/// <exception cref="StartsWithException">Thrown when the Memory does not start with the expected subMemory</exception>
#if XUNIT_NULLABLE
		public static void StartsWith(ReadOnlyMemory<char> expectedStartMemory, ReadOnlyMemory<char>? actualMemory, StringComparison comparisonType = StringComparison.CurrentCulture)
#else
		public static void StartsWith(ReadOnlyMemory<char> expectedStartMemory, ReadOnlyMemory<char> actualMemory, StringComparison comparisonType = StringComparison.CurrentCulture)
#endif
		{
			GuardArgumentNotNull(nameof(expectedStartMemory), expectedStartMemory);
#if XUNIT_NULLABLE
			StartsWith(expectedStartMemory.ToString(), actualMemory?.ToString(), comparisonType);//use string comparison for it's strong output
#else
			StartsWith(expectedStartMemory.ToString(), actualMemory.ToString(), comparisonType);//use string comparison for it's strong output
#endif
		}

		/// <summary>
		/// Verifies that a Memory ends with a given sub-Memory, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedEndMemory">The sub-Memory expected to be at the end of the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="EndsWithException">Thrown when the Memory does not end with the expected subMemory</exception>
#if XUNIT_NULLABLE
		public static void EndsWith(Memory<char> expectedEndMemory, ReadOnlyMemory<char>? actualMemory)
#else
		public static void EndsWith(Memory<char> expectedEndMemory, ReadOnlyMemory<char> actualMemory)
#endif
			=> EndsWith(expectedEndMemory, actualMemory, StringComparison.CurrentCulture);

		/// <summary>
		/// Verifies that a Memory ends with a given sub-Memory, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedEndMemory">The sub-Memory expected to be at the end of the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="EndsWithException">Thrown when the Memory does not end with the expected subMemory</exception>
#if XUNIT_NULLABLE
		public static void EndsWith(ReadOnlyMemory<char> expectedEndMemory, ReadOnlyMemory<char>? actualMemory)
#else
		public static void EndsWith(ReadOnlyMemory<char> expectedEndMemory, ReadOnlyMemory<char> actualMemory)
#endif
			=> EndsWith(expectedEndMemory, actualMemory, StringComparison.CurrentCulture);

		/// <summary>
		/// Verifies that a Memory ends with a given sub-Memory, using the given comparison type.
		/// </summary>
		/// <param name="expectedEndMemory">The sub-Memory expected to be at the end of the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <param name="comparisonType">The type of string comparison to perform</param>
		/// <exception cref="EndsWithException">Thrown when the Memory does not end with the expected subMemory</exception>
#if XUNIT_NULLABLE
		public static void EndsWith(ReadOnlyMemory<char> expectedEndMemory, ReadOnlyMemory<char>? actualMemory, StringComparison comparisonType = StringComparison.CurrentCulture)
#else
		public static void EndsWith(ReadOnlyMemory<char> expectedEndMemory, ReadOnlyMemory<char> actualMemory, StringComparison comparisonType = StringComparison.CurrentCulture)
#endif
		{
			GuardArgumentNotNull(nameof(expectedEndMemory), expectedEndMemory);
#if XUNIT_NULLABLE
			EndsWith(expectedEndMemory.ToString(), actualMemory?.ToString(), comparisonType);//use string comparison for it's strong output
#else
			EndsWith(expectedEndMemory.ToString(), actualMemory.ToString(), comparisonType);//use string comparison for it's strong output
#endif
		}

		/// <summary>
		/// Verifies that two Memory values are equivalent.
		/// </summary>
		/// <param name="expectedMemory">The expected Memory value.</param>
		/// <param name="actualMemory">The actual Memory value.</param>
		/// <exception cref="EqualException">Thrown when the Memory values are not equivalent.</exception>
#if XUNIT_NULLABLE
		public static void Equal(Memory<char> expectedMemory, ReadOnlyMemory<char>? actualMemory)
#else
		public static void Equal(Memory<char> expectedMemory, ReadOnlyMemory<char> actualMemory)
#endif
			=> Equal(expectedMemory, actualMemory, false, false, false);

		/// <summary>
		/// Verifies that two Memory values are equivalent.
		/// </summary>
		/// <param name="expectedMemory">The expected Memory value.</param>
		/// <param name="actualMemory">The actual Memory value.</param>
		/// <exception cref="EqualException">Thrown when the Memory values are not equivalent.</exception>
#if XUNIT_NULLABLE
		public static void Equal(ReadOnlyMemory<char> expectedMemory, ReadOnlyMemory<char>? actualMemory)
#else
		public static void Equal(ReadOnlyMemory<char> expectedMemory, ReadOnlyMemory<char> actualMemory)
#endif
			=> Equal(expectedMemory, actualMemory, false, false, false);

		/// <summary>
		/// Verifies that two Memory values are equivalent.
		/// </summary>
		/// <param name="expectedMemory">The expected Memory value.</param>
		/// <param name="actualMemory">The actual Memory value.</param>
		/// <param name="ignoreCase">If set to <c>true</c>, ignores cases differences. The invariant culture is used.</param>
		/// <param name="ignoreLineEndingDifferences">If set to <c>true</c>, treats \r\n, \r, and \n as equivalent.</param>
		/// <param name="ignoreWhiteSpaceDifferences">If set to <c>true</c>, treats spaces and tabs (in any non-zero quantity) as equivalent.</param>
		/// <exception cref="EqualException">Thrown when the Memory values are not equivalent.</exception>
#if XUNIT_NULLABLE
		public static void Equal(Memory<char> expectedMemory, ReadOnlyMemory<char>? actualMemory, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false)
#else
		public static void Equal(Memory<char> expectedMemory, ReadOnlyMemory<char> actualMemory, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false)
#endif
			=> EqualImp(expectedMemory, actualMemory, ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences);

		/// <summary>
		/// Verifies that two Memory values are equivalent.
		/// </summary>
		/// <param name="expectedMemory">The expected Memory value.</param>
		/// <param name="actualMemory">The actual Memory value.</param>
		/// <param name="ignoreCase">If set to <c>true</c>, ignores cases differences. The invariant culture is used.</param>
		/// <param name="ignoreLineEndingDifferences">If set to <c>true</c>, treats \r\n, \r, and \n as equivalent.</param>
		/// <param name="ignoreWhiteSpaceDifferences">If set to <c>true</c>, treats spaces and tabs (in any non-zero quantity) as equivalent.</param>
		/// <exception cref="EqualException">Thrown when the Memory values are not equivalent.</exception>
#if XUNIT_NULLABLE
		public static void Equal(ReadOnlyMemory<char> expectedMemory, ReadOnlyMemory<char>? actualMemory, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false)
#else
		public static void Equal(ReadOnlyMemory<char> expectedMemory, ReadOnlyMemory<char> actualMemory, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false)
#endif
			=> EqualImp(expectedMemory, actualMemory, ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences);

		/// <summary>
		/// Verifies that two Memory values are equivalent.
		/// </summary>
		/// <param name="expectedMemory">The expected Memory value.</param>
		/// <param name="actualMemory">The actual Memory value.</param>
		/// <param name="ignoreCase">If set to <c>true</c>, ignores cases differences. The invariant culture is used.</param>
		/// <param name="ignoreLineEndingDifferences">If set to <c>true</c>, treats \r\n, \r, and \n as equivalent.</param>
		/// <param name="ignoreWhiteSpaceDifferences">If set to <c>true</c>, treats spaces and tabs (in any non-zero quantity) as equivalent.</param>
		/// <exception cref="EqualException">Thrown when the Memory values are not equivalent.</exception>
#if XUNIT_NULLABLE
		public static void Equal(Memory<char> expectedMemory, Memory<char>? actualMemory, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false)
#else
		public static void Equal(Memory<char> expectedMemory, Memory<char> actualMemory, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false)
#endif
			=> EqualImp(expectedMemory, actualMemory, ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences);

#if XUNIT_NULLABLE
		private static void EqualImp(ReadOnlyMemory<char> expectedMemory, ReadOnlyMemory<char>? actualMemory, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false)
#else
		private static void EqualImp(ReadOnlyMemory<char> expectedMemory, ReadOnlyMemory<char> actualMemory, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false)
#endif
		{
#if XUNIT_NULLABLE
			Equal(expectedMemory.ToString(), actualMemory?.ToString(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences);//use string comparison for it's strong output
#else
			Equal(expectedMemory.ToString(), actualMemory.ToString(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences);//use string comparison for it's strong output
#endif
		}

		/// <summary>
		/// Verifies that two Memory values are equivalent.
		/// </summary>
		/// <param name="expectedMemory">The expected Memory value.</param>
		/// <param name="actualMemory">The actual Memory value.</param>
		/// <exception cref="EqualException">Thrown when the Memory values are not equivalent.</exception>
#if XUNIT_NULLABLE
		public static void Equal<T>(ReadOnlyMemory<T> expectedMemory, ReadOnlyMemory<T> actualMemory) where T : IEquatable<T>
#else
		public static void Equal<T>(ReadOnlyMemory<T> expectedMemory, ReadOnlyMemory<T> actualMemory) where T : IEquatable<T>
#endif
			=> EqualImp<T>(expectedMemory, actualMemory);

		/// <summary>
		/// Verifies that two Memory values are equivalent.
		/// </summary>
		/// <param name="expectedMemory">The expected Memory value.</param>
		/// <param name="actualMemory">The actual Memory value.</param>
		/// <exception cref="EqualException">Thrown when the Memory values are not equivalent.</exception>
#if XUNIT_NULLABLE
		public static void Equal<T>(Memory<T> expectedMemory, ReadOnlyMemory<T> actualMemory) where T : IEquatable<T>
#else
		public static void Equal<T>(Memory<T> expectedMemory, ReadOnlyMemory<T> actualMemory) where T : IEquatable<T>
#endif
			=> EqualImp<T>(expectedMemory, actualMemory);

		/// <summary>
		/// Verifies that two Memory values are equivalent.
		/// </summary>
		/// <param name="expectedMemory">The expected Memory value.</param>
		/// <param name="actualMemory">The actual Memory value.</param>
		/// <exception cref="EqualException">Thrown when the Memory values are not equivalent.</exception>
#if XUNIT_NULLABLE
		public static void Equal<T>(Memory<T> expectedMemory, Memory<T> actualMemory) where T : IEquatable<T>
#else
		public static void Equal<T>(Memory<T> expectedMemory, Memory<T> actualMemory) where T : IEquatable<T>
#endif
			=> EqualImp<T>(expectedMemory, actualMemory);

		private static void EqualImp<T>(ReadOnlyMemory<T> expectedMemory, ReadOnlyMemory<T> actualMemory) where T : IEquatable<T>
		{
			GuardArgumentNotNull(nameof(expectedMemory), expectedMemory);
			Equal(expectedMemory.Span.ToArray(), actualMemory.Span.ToArray(), GetEqualityComparer<T>());
		}
	}
}
