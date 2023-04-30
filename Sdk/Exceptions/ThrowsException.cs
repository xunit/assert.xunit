#if XUNIT_NULLABLE
#nullable enable
#endif

using System;

namespace Xunit.Sdk
{
	/// <summary>
	/// Exception thrown when code unexpectedly fails to throw an exception.
	/// </summary>
#if XUNIT_VISIBILITY_INTERNAL
	internal
#else
	public
#endif
	class ThrowsException : XunitException
	{
		ThrowsException(
			string message,
#if XUNIT_NULLABLE
			Exception? innerException = null) :
#else
			Exception innerException = null) :
#endif
				base(message, innerException)
		{ }

		/// <summary>
		/// Creates a new instance of the <see cref="ThrowsException"/> class to be thrown when
		/// an exception of the wrong type was thrown by Assert.Throws.
		/// </summary>
		/// <param name="expected">The expected exception type</param>
		/// <param name="actual">The actual exception</param>
		internal static ThrowsException ForIncorrectExceptionType(
			Type expected,
			Exception actual) =>
				new ThrowsException(
					"Assert.Throws() Failure: Exception type was not an exact match" + Environment.NewLine +
					"Expected: " + ArgumentFormatter2.Format(expected) + Environment.NewLine +
					"Actual:   " + ArgumentFormatter2.Format(actual.GetType()),
					actual
				);

		/// <summary>
		/// Creates a new instance of the <see cref="ThrowsException"/> class to be thrown when
		/// an <see cref="ArgumentException"/> is thrown with the wrong parameter name.
		/// </summary>
		/// <param name="expected">The exception type</param>
		/// <param name="expectedParamName">The expected parameter name</param>
		/// <param name="actualParamName">The actual parameter name</param>
		public static ThrowsException ForIncorrectParameterName(
			Type expected,
#if XUNIT_NULLABLE
			string? expectedParamName,
			string? actualParamName) =>
#else
			string expectedParamName,
			string actualParamName) =>
#endif
				new ThrowsException(
					"Assert.Throws() Failure: Incorrect parameter name" + Environment.NewLine +
					"Exception: " + ArgumentFormatter2.Format(expected) + Environment.NewLine +
					"Expected:  " + ArgumentFormatter2.Format(expectedParamName) + Environment.NewLine +
					"Actual:    " + ArgumentFormatter2.Format(actualParamName)
				);

		/// <summary>
		/// Creates a new instance of the <see cref="ThrowsException"/> class to be thrown when
		/// an exception wasn't thrown by Assert.Throws.
		/// </summary>
		/// <param name="expected">The expected exception type</param>
		public static ThrowsException ForNoException(Type expected) =>
			new ThrowsException(
				"Assert.Throws() Failure: No exception was thrown" + Environment.NewLine +
				"Expected: " + ArgumentFormatter2.Format(expected)
			);
	}
}
