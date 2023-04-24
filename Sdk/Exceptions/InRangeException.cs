#if XUNIT_NULLABLE
#nullable enable
#endif

using System;

namespace Xunit.Sdk
{
	/// <summary>
	/// Exception thrown when a value is unexpectedly not in the given range.
	/// </summary>
#if XUNIT_VISIBILITY_INTERNAL
	internal
#else
	public
#endif
	class InRangeException : XunitException
	{
		InRangeException(string message) :
			base(message)
		{ }

		/// <summary>
		/// Creates a new instance of the <see cref="InRangeException"/> class to be thrown when
		/// the given value is not in the given range.
		/// </summary>
		/// <param name="actual">The actual object value</param>
		/// <param name="low">The low value of the range</param>
		/// <param name="high">The high value of the range</param>
		public static InRangeException ForValueNotInRange(
			object actual,
			object low,
			object high) =>
				new InRangeException(
					"Assert.InRange() Failure: Value not in range" + Environment.NewLine +
					"Range:  (" + ArgumentFormatter2.Format(low) + " - " + ArgumentFormatter2.Format(high) + ")" + Environment.NewLine +
					"Actual: " + ArgumentFormatter2.Format(actual)
				);
	}
}
