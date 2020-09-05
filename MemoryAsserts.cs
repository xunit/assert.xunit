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
		//NOTE: MemoryAssert logic is dependent on Span asserts. The memory asserts that are not just redirects within the calss use the .span property of memory to perform assertions

		//NOTE: there is an implicit conversion operator on Memory<T> to ReadOnlyMemory<T> I didn't create permutations of all the combinations 
		//		of Memory and ReadOnlyMemory for each method, but I did create permutations for each argument as a hint to the compiler to help 
		//		it select the right method.

		//NOTE: we could consider StartsWith<T> and EndsWith<T> with both arguments as ReadOnlyMemory<T>, and use the Memory extension methods to check difference
		//		BUT: the current Exceptions for startswith and endswith are only built for string types, so those would need a change (or new non-string versions created).

		/// <summary>
		/// Verifies that a Memory contains a given sub-Memory, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedSubMemory">The sub-Memory expected to be in the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="ContainsException">Thrown when the sub-Memory is not present inside the Memory</exception>
		public static void Contains(Memory<char> expectedSubMemory, ReadOnlyMemory<char> actualMemory)
			=> Contains(expectedSubMemory, actualMemory, StringComparison.CurrentCulture);

		/// <summary>
		/// Verifies that a Memory contains a given sub-Memory, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedSubMemory">The sub-Memory expected to be in the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="ContainsException">Thrown when the sub-Memory is not present inside the Memory</exception>
		public static void Contains(ReadOnlyMemory<char> expectedSubMemory, ReadOnlyMemory<char> actualMemory)
			=> Contains(expectedSubMemory, actualMemory, StringComparison.CurrentCulture);


		/// <summary>
		/// Verifies that a Memory contains a given sub-Memory, using the given comparison type.
		/// </summary>
		/// <param name="expectedSubMemory">The sub-Memory expected to be in the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <param name="comparisonType">The type of string comparison to perform</param>
		/// <exception cref="ContainsException">Thrown when the sub-Memory is not present inside the Memory</exception>
		public static void Contains(ReadOnlyMemory<char> expectedSubMemory, ReadOnlyMemory<char> actualMemory, StringComparison comparisonType = StringComparison.CurrentCulture)
			=> Contains(expectedSubMemory.Span, actualMemory.Span, comparisonType);

		/// <summary>
		/// Verifies that a Memory contains a given sub-Memory
		/// </summary>
		/// <param name="expectedSubMemory">The sub-Memory expected to be in the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="ContainsException">Thrown when the sub-Memory is not present inside the Memory</exception>
		public static void Contains<T>(Memory<T> expectedSubMemory, ReadOnlyMemory<T> actualMemory) where T : IEquatable<T>
			=> Contains(expectedSubMemory.Span, actualMemory.Span);

		/// <summary>
		/// Verifies that a Memory contains a given sub-Memory
		/// </summary>
		/// <param name="expectedSubMemory">The sub-Memory expected to be in the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="ContainsException">Thrown when the sub-Memory is not present inside the Memory</exception>
		public static void Contains<T>(ReadOnlyMemory<T> expectedSubMemory, ReadOnlyMemory<T> actualMemory) where T : IEquatable<T>
			=> Contains(expectedSubMemory.Span, actualMemory.Span);

		/// <summary>
		/// Verifies that a Memory does not contain a given sub-Memory, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedSubMemory">The sub-Memory expected not to be in the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="DoesNotContainException">Thrown when the sub-Memory is present inside the Memory</exception>
		public static void DoesNotContain(Memory<char> expectedSubMemory, ReadOnlyMemory<char> actualMemory)
			=> DoesNotContain(expectedSubMemory, actualMemory, StringComparison.CurrentCulture);

		/// <summary>
		/// Verifies that a Memory does not contain a given sub-Memory, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedSubMemory">The sub-Memory expected not to be in the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="DoesNotContainException">Thrown when the sub-Memory is present inside the Memory</exception>
		public static void DoesNotContain(ReadOnlyMemory<char> expectedSubMemory, ReadOnlyMemory<char> actualMemory)
			=> DoesNotContain(expectedSubMemory, actualMemory, StringComparison.CurrentCulture);


		/// <summary>
		/// Verifies that a Memory does not contain a given sub-Memory, using the given comparison type.
		/// </summary>
		/// <param name="expectedSubMemory">The sub-Memory expected not to be in the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <param name="comparisonType">The type of string comparison to perform</param>
		/// <exception cref="DoesNotContainException">Thrown when the sub-Memory is present inside the Memory</exception>
		public static void DoesNotContain(ReadOnlyMemory<char> expectedSubMemory, ReadOnlyMemory<char> actualMemory, StringComparison comparisonType = StringComparison.CurrentCulture)
			=> DoesNotContain(expectedSubMemory.Span, actualMemory.Span, comparisonType);

		/// <summary>
		/// Verifies that a Memory does not contain a given sub-Memory
		/// </summary>
		/// <param name="expectedSubMemory">The sub-Memory expected not to be in the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="DoesNotContainException">Thrown when the sub-Memory is present inside the Memory</exception>
		public static void DoesNotContain<T>(ReadOnlyMemory<T> expectedSubMemory, ReadOnlyMemory<T> actualMemory) where T : IEquatable<T>
			=> DoesNotContain(expectedSubMemory.Span, actualMemory.Span);

		/// <summary>
		/// Verifies that a Memory does not contain a given sub-Memory
		/// </summary>
		/// <param name="expectedSubMemory">The sub-Memory expected not to be in the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="DoesNotContainException">Thrown when the sub-Memory is present inside the Memory</exception>
		public static void DoesNotContain<T>(Memory<T> expectedSubMemory, ReadOnlyMemory<T> actualMemory) where T : IEquatable<T>
			=> DoesNotContain(expectedSubMemory.Span, actualMemory.Span);

		/// <summary>
		/// Verifies that a Memory starts with a given sub-Memory, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedStartMemory">The sub-Memory expected to be at the start of the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="StartsWithException">Thrown when the Memory does not start with the expected subMemory</exception>
		public static void StartsWith(Memory<char> expectedStartMemory, ReadOnlyMemory<char> actualMemory)
			=> StartsWith(expectedStartMemory, actualMemory, StringComparison.CurrentCulture);

		/// <summary>
		/// Verifies that a Memory starts with a given sub-Memory, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedStartMemory">The sub-Memory expected to be at the start of the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="StartsWithException">Thrown when the Memory does not start with the expected subMemory</exception>
		public static void StartsWith(ReadOnlyMemory<char> expectedStartMemory, ReadOnlyMemory<char> actualMemory)
			=> StartsWith(expectedStartMemory, actualMemory, StringComparison.CurrentCulture);

		/// <summary>
		/// Verifies that a Memory starts with a given sub-Memory, using the given comparison type.
		/// </summary>
		/// <param name="expectedStartMemory">The sub-Memory expected to be at the start of the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <param name="comparisonType">The type of string comparison to perform</param>
		/// <exception cref="StartsWithException">Thrown when the Memory does not start with the expected subMemory</exception>
		public static void StartsWith(ReadOnlyMemory<char> expectedStartMemory, ReadOnlyMemory<char> actualMemory, StringComparison comparisonType = StringComparison.CurrentCulture)
			=> StartsWith(expectedStartMemory.Span, actualMemory.Span, comparisonType);

		/// <summary>
		/// Verifies that a Memory ends with a given sub-Memory, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedEndMemory">The sub-Memory expected to be at the end of the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="EndsWithException">Thrown when the Memory does not end with the expected subMemory</exception>
		public static void EndsWith(Memory<char> expectedEndMemory, ReadOnlyMemory<char> actualMemory)
			=> EndsWith(expectedEndMemory, actualMemory, StringComparison.CurrentCulture);

		/// <summary>
		/// Verifies that a Memory ends with a given sub-Memory, using the default StringComparison.CurrentCulture comparison type.
		/// </summary>
		/// <param name="expectedEndMemory">The sub-Memory expected to be at the end of the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <exception cref="EndsWithException">Thrown when the Memory does not end with the expected subMemory</exception>
		public static void EndsWith(ReadOnlyMemory<char> expectedEndMemory, ReadOnlyMemory<char> actualMemory)
			=> EndsWith(expectedEndMemory, actualMemory, StringComparison.CurrentCulture);

		/// <summary>
		/// Verifies that a Memory ends with a given sub-Memory, using the given comparison type.
		/// </summary>
		/// <param name="expectedEndMemory">The sub-Memory expected to be at the end of the Memory</param>
		/// <param name="actualMemory">The Memory to be inspected</param>
		/// <param name="comparisonType">The type of string comparison to perform</param>
		/// <exception cref="EndsWithException">Thrown when the Memory does not end with the expected subMemory</exception>
		public static void EndsWith(ReadOnlyMemory<char> expectedEndMemory, ReadOnlyMemory<char> actualMemory, StringComparison comparisonType = StringComparison.CurrentCulture)
			=> EndsWith(expectedEndMemory.Span, actualMemory.Span, comparisonType);

		/// <summary>
		/// Verifies that two Memory values are equivalent.
		/// </summary>
		/// <param name="expectedMemory">The expected Memory value.</param>
		/// <param name="actualMemory">The actual Memory value.</param>
		/// <exception cref="EqualException">Thrown when the Memory values are not equivalent.</exception>
		public static void Equal(Memory<char> expectedMemory, ReadOnlyMemory<char> actualMemory)
			=> Equal(expectedMemory, actualMemory, false, false, false);

		/// <summary>
		/// Verifies that two Memory values are equivalent.
		/// </summary>
		/// <param name="expectedMemory">The expected Memory value.</param>
		/// <param name="actualMemory">The actual Memory value.</param>
		/// <exception cref="EqualException">Thrown when the Memory values are not equivalent.</exception>
		public static void Equal(ReadOnlyMemory<char> expectedMemory, ReadOnlyMemory<char> actualMemory)
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
		public static void Equal(Memory<char> expectedMemory, ReadOnlyMemory<char> actualMemory, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false)
			=> Equal(expectedMemory.Span, actualMemory.Span, ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences);

		/// <summary>
		/// Verifies that two Memory values are equivalent.
		/// </summary>
		/// <param name="expectedMemory">The expected Memory value.</param>
		/// <param name="actualMemory">The actual Memory value.</param>
		/// <param name="ignoreCase">If set to <c>true</c>, ignores cases differences. The invariant culture is used.</param>
		/// <param name="ignoreLineEndingDifferences">If set to <c>true</c>, treats \r\n, \r, and \n as equivalent.</param>
		/// <param name="ignoreWhiteSpaceDifferences">If set to <c>true</c>, treats spaces and tabs (in any non-zero quantity) as equivalent.</param>
		/// <exception cref="EqualException">Thrown when the Memory values are not equivalent.</exception>
		public static void Equal(ReadOnlyMemory<char> expectedMemory, ReadOnlyMemory<char> actualMemory, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false)
			=> Equal(expectedMemory.Span, actualMemory.Span, ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences);

		/// <summary>
		/// Verifies that two Memory values are equivalent.
		/// </summary>
		/// <param name="expectedMemory">The expected Memory value.</param>
		/// <param name="actualMemory">The actual Memory value.</param>
		/// <exception cref="EqualException">Thrown when the Memory values are not equivalent.</exception>
		public static void Equal<T>(ReadOnlyMemory<T> expectedMemory, ReadOnlyMemory<T> actualMemory) where T : IEquatable<T>
			=> Equal(expectedMemory.Span, actualMemory.Span);

		/// <summary>
		/// Verifies that two Memory values are equivalent.
		/// </summary>
		/// <param name="expectedMemory">The expected Memory value.</param>
		/// <param name="actualMemory">The actual Memory value.</param>
		/// <exception cref="EqualException">Thrown when the Memory values are not equivalent.</exception>
		public static void Equal<T>(Memory<T> expectedMemory, ReadOnlyMemory<T> actualMemory) where T : IEquatable<T>
			=> Equal(expectedMemory.Span, actualMemory.Span);

		/// <summary>
		/// Verifies that two Memory values are equivalent.
		/// </summary>
		/// <param name="expectedMemory">The expected Memory value.</param>
		/// <param name="actualMemory">The actual Memory value.</param>
		/// <exception cref="EqualException">Thrown when the Memory values are not equivalent.</exception>
		public static void Equal<T>(Memory<T> expectedMemory, Memory<T> actualMemory) where T : IEquatable<T>
			=> Equal(expectedMemory.Span, actualMemory.Span);
	}
}
