#if XUNIT_NULLABLE
#nullable enable
#endif

namespace Xunit.Sdk
{
	/// <summary>
	/// Exception thrown when Assert.NotNull fails.
	/// </summary>
#if XUNIT_VISIBILITY_INTERNAL
	internal
#else
	public
#endif
	partial class NotNullException : XunitException
	{
		NotNullException(string message) :
			base(message)
		{ }

		/// <summary>
		/// Creates a new instance of the <see cref="NotNullException"/> class to be
		/// thrown when a value is <c>null</c>.
		/// </summary>
		public static NotNullException ForNullValue() =>
			new NotNullException("Assert.NotNull() Failure: Value is null");
	}
}
