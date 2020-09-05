#if XUNIT_NULLABLE
#nullable enable
#endif
using System;

namespace Xunit.Sdk
{
	/// <summary>
	/// Exception thrown when a collection unexpectedly contains the expected value.
	/// </summary>
#if XUNIT_VISIBILITY_INTERNAL
	internal
#else
	public
#endif
	class DoesNotContainException : AssertActualExpectedException
	{
		/// <summary>
		/// Creates a new instance of the <see cref="DoesNotContainException"/> class.
		/// </summary>
		/// <param name="expected">The expected object value</param>
		/// <param name="actual">The actual value</param>
#if XUNIT_NULLABLE
		public DoesNotContainException(object? expected, object? actual)
#else
		public DoesNotContainException(object expected, object actual)
#endif
			: base(expected, actual, "Assert.DoesNotContain() Failure", "Found", "In value")
		{ }

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="expected">The expected object value</param>
		/// <param name="actual">The actual value</param>
		/// <returns></returns>
		public static DoesNotContainException Create<T>(ReadOnlySpan<T> expected, ReadOnlySpan<T> actual) =>
			RefStructExceptionHelper.CreateException(expected, actual, (e, a) => new DoesNotContainException(e, a));

	}
}
