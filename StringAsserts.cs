#if XUNIT_NULLABLE
#nullable enable
#endif

using System;
using System.Text.RegularExpressions;
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
		/// <summary>
		/// Verifies that a string contains a given sub-string, using the current culture.
		/// </summary>
		/// <param name="expectedSubstring">The sub-string expected to be in the string</param>
		/// <param name="actualString">The string to be inspected</param>
		/// <exception cref="ContainsException">Thrown when the sub-string is not present inside the string</exception>
#if XUNIT_NULLABLE
		public static void Contains(string expectedSubstring, string? actualString)
#else
		public static void Contains(string expectedSubstring, string actualString)
#endif
		{
			Contains(expectedSubstring, actualString, StringComparison.CurrentCulture);
		}

		/// <summary>
		/// Verifies that a string contains a given sub-string, using the given comparison type.
		/// </summary>
		/// <param name="expectedSubstring">The sub-string expected to be in the string</param>
		/// <param name="actualString">The string to be inspected</param>
		/// <param name="comparisonType">The type of string comparison to perform</param>
		/// <exception cref="ContainsException">Thrown when the sub-string is not present inside the string</exception>
#if XUNIT_NULLABLE
		public static void Contains(string expectedSubstring, string? actualString, StringComparison comparisonType)
#else
		public static void Contains(string expectedSubstring, string actualString, StringComparison comparisonType)
#endif
		{
			GuardArgumentNotNull(nameof(expectedSubstring), expectedSubstring);

			if (actualString == null || actualString.IndexOf(expectedSubstring, comparisonType) < 0)
				throw new ContainsException(expectedSubstring, actualString);
		}

		/// <summary>
		/// Verifies that a string does not contain a given sub-string, using the current culture.
		/// </summary>
		/// <param name="expectedSubstring">The sub-string which is expected not to be in the string</param>
		/// <param name="actualString">The string to be inspected</param>
		/// <exception cref="DoesNotContainException">Thrown when the sub-string is present inside the string</exception>
#if XUNIT_NULLABLE
		public static void DoesNotContain(string expectedSubstring, string? actualString)
#else
		public static void DoesNotContain(string expectedSubstring, string actualString)
#endif
		{
			DoesNotContain(expectedSubstring, actualString, StringComparison.CurrentCulture);
		}

		/// <summary>
		/// Verifies that a string does not contain a given sub-string, using the current culture.
		/// </summary>
		/// <param name="expectedSubstring">The sub-string which is expected not to be in the string</param>
		/// <param name="actualString">The string to be inspected</param>
		/// <param name="comparisonType">The type of string comparison to perform</param>
		/// <exception cref="DoesNotContainException">Thrown when the sub-string is present inside the given string</exception>
#if XUNIT_NULLABLE
		public static void DoesNotContain(string expectedSubstring, string? actualString, StringComparison comparisonType)
#else
		public static void DoesNotContain(string expectedSubstring, string actualString, StringComparison comparisonType)
#endif
		{
			GuardArgumentNotNull(nameof(expectedSubstring), expectedSubstring);

			if (actualString != null && actualString.IndexOf(expectedSubstring, comparisonType) >= 0)
				throw new DoesNotContainException(expectedSubstring, actualString);
		}

		/// <summary>
		/// Verifies that a string starts with a given string, using the current culture.
		/// </summary>
		/// <param name="expectedStartString">The string expected to be at the start of the string</param>
		/// <param name="actualString">The string to be inspected</param>
		/// <exception cref="ContainsException">Thrown when the string does not start with the expected string</exception>
#if XUNIT_NULLABLE
		public static void StartsWith(string? expectedStartString, string? actualString)
#else
		public static void StartsWith(string expectedStartString, string actualString)
#endif
		{
			StartsWith(expectedStartString, actualString, StringComparison.CurrentCulture);
		}

		/// <summary>
		/// Verifies that a string starts with a given string, using the given comparison type.
		/// </summary>
		/// <param name="expectedStartString">The string expected to be at the start of the string</param>
		/// <param name="actualString">The string to be inspected</param>
		/// <param name="comparisonType">The type of string comparison to perform</param>
		/// <exception cref="ContainsException">Thrown when the string does not start with the expected string</exception>
#if XUNIT_NULLABLE
		public static void StartsWith(string? expectedStartString, string? actualString, StringComparison comparisonType)
#else
		public static void StartsWith(string expectedStartString, string actualString, StringComparison comparisonType)
#endif
		{
			if (expectedStartString == null || actualString == null || !actualString.StartsWith(expectedStartString, comparisonType))
				throw new StartsWithException(expectedStartString, actualString);
		}

		/// <summary>
		/// Verifies that a string ends with a given string, using the current culture.
		/// </summary>
		/// <param name="expectedEndString">The string expected to be at the end of the string</param>
		/// <param name="actualString">The string to be inspected</param>
		/// <exception cref="ContainsException">Thrown when the string does not end with the expected string</exception>
#if XUNIT_NULLABLE
		public static void EndsWith(string? expectedEndString, string? actualString)
#else
		public static void EndsWith(string expectedEndString, string actualString)
#endif
		{
			EndsWith(expectedEndString, actualString, StringComparison.CurrentCulture);
		}

		/// <summary>
		/// Verifies that a string ends with a given string, using the given comparison type.
		/// </summary>
		/// <param name="expectedEndString">The string expected to be at the end of the string</param>
		/// <param name="actualString">The string to be inspected</param>
		/// <param name="comparisonType">The type of string comparison to perform</param>
		/// <exception cref="ContainsException">Thrown when the string does not end with the expected string</exception>
#if XUNIT_NULLABLE
		public static void EndsWith(string? expectedEndString, string? actualString, StringComparison comparisonType)
#else
		public static void EndsWith(string expectedEndString, string actualString, StringComparison comparisonType)
#endif
		{
			if (expectedEndString == null || actualString == null || !actualString.EndsWith(expectedEndString, comparisonType))
				throw new EndsWithException(expectedEndString, actualString);
		}

		/// <summary>
		/// Verifies that a string matches a regular expression.
		/// </summary>
		/// <param name="expectedRegexPattern">The regex pattern expected to match</param>
		/// <param name="actualString">The string to be inspected</param>
		/// <exception cref="MatchesException">Thrown when the string does not match the regex pattern</exception>
#if XUNIT_NULLABLE
		public static void Matches(string expectedRegexPattern, string? actualString)
#else
		public static void Matches(string expectedRegexPattern, string actualString)
#endif
		{
			GuardArgumentNotNull(nameof(expectedRegexPattern), expectedRegexPattern);

			if (actualString == null || !Regex.IsMatch(actualString, expectedRegexPattern))
				throw new MatchesException(expectedRegexPattern, actualString);
		}

		/// <summary>
		/// Verifies that a string matches a regular expression.
		/// </summary>
		/// <param name="expectedRegex">The regex expected to match</param>
		/// <param name="actualString">The string to be inspected</param>
		/// <exception cref="MatchesException">Thrown when the string does not match the regex</exception>
#if XUNIT_NULLABLE
		public static void Matches(Regex expectedRegex, string? actualString)
#else
		public static void Matches(Regex expectedRegex, string actualString)
#endif
		{
			GuardArgumentNotNull(nameof(expectedRegex), expectedRegex);

			if (actualString == null || !expectedRegex.IsMatch(actualString))
				throw new MatchesException(expectedRegex.ToString(), actualString);
		}

		/// <summary>
		/// Verifies that a string does not match a regular expression.
		/// </summary>
		/// <param name="expectedRegexPattern">The regex pattern expected not to match</param>
		/// <param name="actualString">The string to be inspected</param>
		/// <exception cref="DoesNotMatchException">Thrown when the string matches the regex pattern</exception>
#if XUNIT_NULLABLE
		public static void DoesNotMatch(string expectedRegexPattern, string? actualString)
#else
		public static void DoesNotMatch(string expectedRegexPattern, string actualString)
#endif
		{
			GuardArgumentNotNull(nameof(expectedRegexPattern), expectedRegexPattern);

			if (actualString != null && Regex.IsMatch(actualString, expectedRegexPattern))
				throw new DoesNotMatchException(expectedRegexPattern, actualString);
		}

		/// <summary>
		/// Verifies that a string does not match a regular expression.
		/// </summary>
		/// <param name="expectedRegex">The regex expected not to match</param>
		/// <param name="actualString">The string to be inspected</param>
		/// <exception cref="DoesNotMatchException">Thrown when the string matches the regex</exception>
#if XUNIT_NULLABLE
		public static void DoesNotMatch(Regex expectedRegex, string? actualString)
#else
		public static void DoesNotMatch(Regex expectedRegex, string actualString)
#endif
		{
			GuardArgumentNotNull(nameof(expectedRegex), expectedRegex);

			if (actualString != null && expectedRegex.IsMatch(actualString))
				throw new DoesNotMatchException(expectedRegex.ToString(), actualString);
		}

		/// <summary>
		/// Verifies that two strings are equivalent.
		/// </summary>
		/// <param name="expected">The expected string value.</param>
		/// <param name="actual">The actual string value.</param>
		/// <exception cref="EqualException">Thrown when the strings are not equivalent.</exception>
#if XUNIT_NULLABLE
		public static void Equal(string? expected, string? actual)
#else
		public static void Equal(string expected, string actual)
#endif
		{
			Equal(expected, actual, false, false, false);
		}

		/// <summary>
		/// Verifies that two strings are equivalent.
		/// </summary>
		/// <param name="expected">The expected string value.</param>
		/// <param name="actual">The actual string value.</param>
		/// <param name="ignoreCase">If set to <c>true</c>, ignores cases differences. The invariant culture is used.</param>
		/// <param name="ignoreLineEndingDifferences">If set to <c>true</c>, treats \r\n, \r, and \n as equivalent.</param>
		/// <param name="ignoreWhiteSpaceDifferences">If set to <c>true</c>, treats spaces and tabs (in any non-zero quantity) as equivalent.</param>
		/// <exception cref="EqualException">Thrown when the strings are not equivalent.</exception>
#if XUNIT_NULLABLE
		public static void Equal(string? expected, string? actual, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false)
#else
		public static void Equal(string expected, string actual, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false)
#endif
		{
			if (expected == null && actual == null)
				return;
			if (expected == null || actual == null)
				throw new EqualException(expected, actual, -1, -1);

			Equal(expected.AsSpan(), actual.AsSpan(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences);
		}
	}
}
