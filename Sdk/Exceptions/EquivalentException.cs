#if XUNIT_NULLABLE
#nullable enable
#endif

using System;
using System.Collections.Generic;
using System.Linq;

namespace Xunit.Sdk
{
	/// <summary>
	/// Exception thrown when two values are unexpectedly not equal.
	/// </summary>
#if XUNIT_VISIBILITY_INTERNAL
	internal
#else
	public
#endif
	class EquivalentException : XunitException
	{
		EquivalentException(string message) :
			base(message)
		{ }

		static string FormatMemberNameList(
			IEnumerable<string> memberNames,
			string prefix) =>
				"[" + string.Join(", ", memberNames.Select(k => $"\"{prefix}{k}\"")) + "]";

		/// <summary>
		/// Creates a new instance of <see cref="EquivalentException"/> which shows a message that indicates
		/// a circular reference was discovered.
		/// </summary>
		/// <param name="memberName">The name of the member that caused the circular reference</param>
		public static EquivalentException ForCircularReference(string memberName) =>
			new EquivalentException($"Assert.Equivalent() Failure: Circular reference found in '{memberName}'");

		/// <summary>
		/// Creates a new instance of <see cref="EquivalentException"/> which shows a message that indicates
		/// that the list of available members does not match.
		/// </summary>
		/// <param name="expectedMemberNames">The expected member names</param>
		/// <param name="actualMemberNames">The actual member names</param>
		/// <param name="prefix">The prefix to be applied to the member names (may be an empty string for a
		/// top-level object, or a name in "member." format used as a prefix to show the member name list)</param>
		public static EquivalentException ForMemberListMismatch(
			IEnumerable<string> expectedMemberNames,
			IEnumerable<string> actualMemberNames,
			string prefix) =>
				new EquivalentException(
					"Assert.Equivalent() Failure: Mismatched member list" + Environment.NewLine +
					"Expected: " + FormatMemberNameList(expectedMemberNames, prefix) + Environment.NewLine +
					"Actual:   " + FormatMemberNameList(actualMemberNames, prefix)
				);

		/// <summary>
		/// Creates a new instance of <see cref="EquivalentException"/> which shows a message that indicates
		/// that the fault comes from an individual value mismatch one of the members.
		/// </summary>
		/// <param name="expected">The expected member value</param>
		/// <param name="actual">The actual member value</param>
		/// <param name="memberName">The name of the mismatched member (may be an empty string for a
		/// top-level object)</param>
		public static EquivalentException ForMemberValueMismatch(
#if XUNIT_NULLABLE
			object? expected,
			object? actual,
#else
			object expected,
			object actual,
#endif
			string memberName)
		{
			var formattedExpected = ArgumentFormatter.Format(expected);
			var formattedActual = ArgumentFormatter.Format(actual);

			if (formattedExpected == formattedActual && expected != null && actual != null)
			{
				var expectedType = expected.GetType();
				var actualType = actual.GetType();

				if (expectedType != actualType)
				{
					formattedExpected += $" ({expectedType.FullName})";
					formattedActual += $" ({actualType.FullName})";
				}
			}

			return new EquivalentException(
				"Assert.Equivalent() Failure" + (memberName == string.Empty ? string.Empty : $": Mismatched value on member '{memberName}'") + Environment.NewLine +
				"Expected: " + formattedExpected + Environment.NewLine +
				"Actual:   " + formattedActual
			);
		}

		/// <summary>
		/// Creates a new instance of <see cref="EquivalentException"/> which shows a message that indicates
		/// a value was missing from the <paramref name="actual"/> collection.
		/// </summary>
		/// <param name="expected">The object that was expected to be found in <paramref name="actual"/> collection.</param>
		/// <param name="actual">The actual collection which was missing the object.</param>
		/// <param name="memberName">The name of the member that was being inspected (may be an empty
		/// string for a top-level collection)</param>
		public static EquivalentException ForMissingCollectionValue(
#if XUNIT_NULLABLE
			object? expected,
			IEnumerable<object?> actual,
#else
			object expected,
			IEnumerable<object> actual,
#endif
			string memberName) =>
				new EquivalentException(
					"Assert.Equivalent() Failure: Collection value not found" + (memberName == string.Empty ? string.Empty : $" in member '{memberName}'") + Environment.NewLine +
					"Expected: " + ArgumentFormatter.Format(expected) + Environment.NewLine +
					"In:       " + ArgumentFormatter.Format(actual)
				);

		/// <summary>
		/// Creates a new instance of <see cref="EquivalentException"/> which shows a message that indicates
		/// that <paramref name="actual"/> contained one or more values that were not specified
		/// in <paramref name="expected"/>.
		/// </summary>
		/// <param name="expected">The values expected to be found in the <paramref name="actual"/>
		/// collection.</param>
		/// <param name="actual">The actual collection values.</param>
		/// <param name="actualLeftovers">The values from <paramref name="actual"/> that did not have
		/// matching <paramref name="expected"/> values</param>
		/// <param name="memberName">The name of the member that was being inspected (may be an empty
		/// string for a top-level collection)</param>
		public static EquivalentException ForExtraCollectionValue(
#if XUNIT_NULLABLE
			IEnumerable<object?> expected,
			IEnumerable<object?> actual,
			IEnumerable<object?> actualLeftovers,
#else
			IEnumerable<object> expected,
			IEnumerable<object> actual,
			IEnumerable<object> actualLeftovers,
#endif
			string memberName) =>
				new EquivalentException(
					"Assert.Equivalent() Failure: Extra values found" + (memberName == string.Empty ? string.Empty : $" in member '{memberName}'") + Environment.NewLine +
					"Expected: " + ArgumentFormatter.Format(expected) + Environment.NewLine +
					"Actual:   " + ArgumentFormatter.Format(actualLeftovers) + " left over from " + ArgumentFormatter.Format(actual)
				);
	}
}
