#if XUNIT_NULLABLE
#nullable enable
#endif

namespace Xunit.Sdk
{
	/// <summary>
	/// Exception thrown when Assert.NotEmpty fails.
	/// </summary>
#if XUNIT_VISIBILITY_INTERNAL
	internal
#else
	public
#endif
	partial class NotEmptyException : XunitException
	{
		NotEmptyException(string message) :
			base(message)
		{ }

		/// <summary>
		/// Creates a new instance of the <see cref="NotEmptyException"/> class to be thrown
		/// when a container was unexpectedly empty.
		/// </summary>
		public static NotEmptyException ForNonEmptyContainer() =>
			new NotEmptyException("Assert.NotEmpty() Failure: Container was empty");
	}
}
