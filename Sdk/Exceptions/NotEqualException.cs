#if XUNIT_NULLABLE
#nullable enable
#else
// In case this is source-imported with global nullable enabled but no XUNIT_NULLABLE
#pragma warning disable CS8625
#endif

using System;
using System.Globalization;

namespace Xunit.Sdk
{
	/// <summary>
	/// Exception thrown when Assert.NotEqual fails.
	/// </summary>
#if XUNIT_VISIBILITY_INTERNAL
	internal
#else
	public
#endif
	partial class NotEqualException : XunitException
	{
		NotEqualException(string message) :
			base(message)
		{ }

		/// <summary>
		/// Creates a new instance of <see cref="NotEqualException"/> to be thrown when two collections
		/// are equal.
		/// </summary>
		/// <param name="expected">The expected collection</param>
		/// <param name="actual">The actual collection</param>
		/// <param name="collectionDisplay">The display name for the collection type (defaults to "Collections")</param>
		public static NotEqualException ForEqualCollections(
			string expected,
			string actual,
#if XUNIT_NULLABLE
			string? collectionDisplay = null)
#else
			string collectionDisplay = null)
#endif
		{
			return new NotEqualException(
				string.Format(
					CultureInfo.CurrentCulture,
					"Assert.NotEqual() Failure: {0} are equal{1}Expected: Not {2}{3}Actual:       {4}",
					collectionDisplay ?? "Collections",
					Environment.NewLine,
					Assert.GuardArgumentNotNull(nameof(expected), expected),
					Environment.NewLine,
					Assert.GuardArgumentNotNull(nameof(actual), actual)
				)
			);
		}

		/// <summary>
		/// Creates a new instance of <see cref="NotEqualException"/> to be thrown when two values
		/// are equal. This may be simple values (like intrinsics) or complex values (like
		/// classes or structs).
		/// </summary>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The actual value</param>
		/// <param name="banner">The banner to show; if <c>null</c>, then the standard
		/// banner of "Values are equal" will be used</param>
		public static NotEqualException ForEqualValues(
			string expected,
			string actual,
#if XUNIT_NULLABLE
			string? banner = null)
#else
			string banner = null)
#endif
		{
			return new NotEqualException(
				string.Format(
					CultureInfo.CurrentCulture,
					"Assert.NotEqual() Failure: {0}{1}Expected: Not {2}{3}Actual:       {4}",
					banner ?? "Values are equal",
					Environment.NewLine,
					Assert.GuardArgumentNotNull(nameof(expected), expected),
					Environment.NewLine,
					Assert.GuardArgumentNotNull(nameof(actual), actual)
				)
			);
		}
	}
}
