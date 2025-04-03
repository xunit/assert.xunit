#pragma warning disable CA1032 // Implement standard exception constructors
#pragma warning disable IDE0090 // Use 'new(...)'

#if XUNIT_NULLABLE
#nullable enable
#endif

using System;
using System.Collections.Generic;
using System.Linq;

namespace Xunit.Sdk
{
	/// <summary>
	/// Exception thrown when Assert.AsyncMultiple fails w/ multiple errors (when a single error
	/// occurs, it is thrown directly).
	/// </summary>
#if XUNIT_VISIBILITY_INTERNAL
	internal
#else
	public
#endif
		partial class AsyncMultipleException : XunitException
	{
		AsyncMultipleException(IEnumerable<Exception> innerExceptions) :
			base("Assert.MultipleAsync() Failure: Multiple failures were encountered")
		{
			Assert.GuardArgumentNotNull(nameof(innerExceptions), innerExceptions);

			InnerExceptions = innerExceptions.ToList();
		}

		/// <summary>
		/// Gets the list of inner exceptions that were thrown.
		/// </summary>
		public IReadOnlyCollection<Exception> InnerExceptions { get; }

		/// <inheritdoc/>
#if XUNIT_NULLABLE
		public override string? StackTrace =>
#else
		public override string StackTrace =>
#endif
			"Inner stack traces:";

		/// <summary>
		/// Creates a new instance of the <see cref="AsyncMultipleException"/> class to be thrown
		/// when <see cref="Assert.MultipleAsync"/> caught 2 or more exceptions.
		/// </summary>
		/// <param name="innerExceptions">The inner exceptions</param>
		public static AsyncMultipleException ForFailures(IReadOnlyCollection<Exception> innerExceptions) =>
			new AsyncMultipleException(innerExceptions);
	}
}
