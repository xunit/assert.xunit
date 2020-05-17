using System;

namespace Xunit.Sdk
{
	/// <summary>
	/// Exception thrown when the collection did not contain exactly one element.
	/// </summary>
#if XUNIT_VISIBILITY_INTERNAL
	internal
#else
	public
#endif
	class SingleException : XunitException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SingleException"/> class.
		/// </summary>
		private SingleException(string errorMessage) : base(errorMessage) { }

		/// <summary>
		/// Creates an instance of <see cref="SingleException"/> for when the collection didn't contain any of the expected value.
		/// </summary>
		public static Exception Empty(string expected) =>
			new SingleException("The collection was expected to contain a single element" +
				(expected == null ? "" : " matching " + expected) +
				", but it " +
				(expected == null ? "was empty." : "contained no matching elements."));

		/// <summary>
		/// Creates an instance of <see cref="SingleException"/> for when the collection had too many of the expected items.
		/// </summary>
		/// <returns></returns>
		public static Exception MoreThanOne(int count, string expected) =>
			new SingleException("The collection was expected to contain a single element" +
				(expected == null ? "" : " matching " + expected) +
				", but it contained " + count + " " +
				(expected == null ? "" : "matching ") +
				"elements.");
	}
}
