#if XUNIT_NULLABLE
#nullable enable
#endif

using System.Collections;
using System.Collections.Generic;

namespace Xunit.Sdk
{
	/// <summary>
	/// Represents an assert equality comparer that may know additional information about the
	/// type of objects it's comparing; in particular, to turn an <see cref="object"/> into
	/// a potentially type-aware <see cref="CollectionTracker"/> for non-strings.
	/// </summary>
	public interface IAssertEqualityComparer : IEqualityComparer
	{
		/// <summary>
		/// Gets the inner comparer used when comparing items inside of a collection.
		/// </summary>
		IEqualityComparer InnerComparer { get; }

		// NOTE: In order to make this work, what you need to do is construct a Func of some kind when
		// making the AssertEqualityComparer<T> when you know it's IEnumerable<T> so that it can use
		// that func to create the CollectionTracker<T>.
		// 
		// Also, since we're here, should we eliminate all tests against AEC<T> and instead push as
		// much of the public interface into IAEC or IAEC<T> instead?
		//
		// And also, IAEC<T> should derive from IAEC, and then maybe we can get rid of the type
		// erased wrapper, since everything would be forced to support object in addition to T.

		/// <summary>
		/// Get a potentially type-safe instance of <see cref="CollectionTracker{T}"/> given an
		/// object which may implement <see cref="IEnumerable{T}"/>. May also return an <see cref="object"/>
		/// version if the object only implements <see cref="IEnumerable"/>.
		/// </summary>
		/// <param name="obj">The potential collection object to wrap with a tracker</param>
#if XUNIT_NULLABLE
		CollectionTracker? AsNonStringTracker(object? obj);
#else
		CollectionTracker AsNonStringTracker(object obj);
#endif
	}

	/// <summary>
	/// Represents a specialized version of <see cref="IEqualityComparer{T}"/> that returns information useful
	/// when formatting results for assertion failures.
	/// </summary>
	/// <typeparam name="T">The type of the objects being compared.</typeparam>
	public interface IAssertEqualityComparer<T> : IEqualityComparer<T>
	{
		/// <summary>
		/// Compares two values and determines if they are equal.
		/// </summary>
		/// <param name="x">The first value</param>
		/// <param name="xTracker">The first value as a <see cref="CollectionTracker"/> (if it's a collection)</param>
		/// <param name="y">The second value</param>
		/// <param name="yTracker">The second value as a <see cref="CollectionTracker"/> (if it's a collection)</param>
		/// <returns>Success or failure information</returns>
		AssertEqualityResult Equals(
#if XUNIT_NULLABLE
			T? x,
			CollectionTracker? xTracker,
			T? y,
			CollectionTracker? yTracker);
#else
			T x,
			CollectionTracker xTracker,
			T y,
			CollectionTracker yTracker);
#endif
	}
}
