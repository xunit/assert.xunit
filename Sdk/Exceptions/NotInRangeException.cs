#if XUNIT_NULLABLE
#nullable enable
#endif

using System;

namespace Xunit.Sdk
{
	/// <summary>
	/// Exception thrown when a value is unexpectedly in the given range.
	/// </summary>
#if XUNIT_VISIBILITY_INTERNAL
	internal
#else
	public
#endif
	class NotInRangeException : XunitException
	{
		NotInRangeException(string message) :
			base(message)
		{ }

		/// <summary>
		/// Creates a new instance of the <see cref="NotInRangeException"/> class to be thrown when
		/// a value was unexpected with the range of two other values.
		/// </summary>
		/// <param name="actual">The actual object value</param>
		/// <param name="low">The low value of the range</param>
		/// <param name="high">The high value of the range</param>
		public static NotInRangeException ForValueInRange(
			object actual,
			object low,
			object high) =>
				new NotInRangeException(
					"Assert.NotInRange() Failure: Value in range" + Environment.NewLine +
					"Range:  (" + ArgumentFormatter2.Format(low) + " - " + ArgumentFormatter2.Format(high) + ")" + Environment.NewLine +
					"Actual: " + ArgumentFormatter2.Format(actual)
				);
	}
}
