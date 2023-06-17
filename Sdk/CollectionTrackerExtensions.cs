#if XUNIT_NULLABLE
#nullable enable

using System.Diagnostics.CodeAnalysis;
#endif

using System;
using System.Collections;
using System.Collections.Generic;

namespace Xunit.Sdk
{
	/// <summary>
	/// Extension methods related to <see cref="CollectionTracker{T}"/>.
	/// </summary>
#if XUNIT_VISIBILITY_INTERNAL
	internal
#else
	public
#endif
	static class CollectionTrackerExtensions
	{
		static readonly Type typeofIEnumerableOfT = typeof(IEnumerable<>);

#if XUNIT_NULLABLE
		internal static IEnumerable? AsNonStringEnumerable(this object? value) =>
#else
		internal static IEnumerable AsNonStringEnumerable(this object value) =>
#endif
			value == null || value is string ? null : value as IEnumerable;

#if XUNIT_NULLABLE
		internal static CollectionTracker<object>? AsNonStringTracker(this object? value) =>
#else
		internal static CollectionTracker<object> AsNonStringTracker(this object value) =>
#endif
			AsTracker(AsNonStringEnumerable(value));

		/// <summary>
		/// Wraps the given enumerable in an instance of <see cref="CollectionTracker{T}"/>.
		/// </summary>
		/// <param name="enumerable">The enumerable to be wrapped</param>
#if XUNIT_NULLABLE
		[return: NotNullIfNotNull("enumerable")]
		public static CollectionTracker<object>? AsTracker(this IEnumerable? enumerable) =>
#else
		public static CollectionTracker<object> AsTracker(this IEnumerable enumerable) =>
#endif
			enumerable == null
				? null
				: enumerable as CollectionTracker<object> ?? CollectionTracker.Wrap(enumerable);

		/// <summary>
		/// Wraps the given enumerable in an instance of <see cref="CollectionTracker{T}"/>.
		/// </summary>
		/// <typeparam name="T">The item type of the collection</typeparam>
		/// <param name="enumerable">The enumerable to be wrapped</param>
#if XUNIT_NULLABLE
		[return: NotNullIfNotNull("enumerable")]
		public static CollectionTracker<T>? AsTracker<T>(this IEnumerable<T>? enumerable) =>
#else
		public static CollectionTracker<T> AsTracker<T>(this IEnumerable<T> enumerable) =>
#endif
			enumerable == null
				? null
				: enumerable as CollectionTracker<T> ?? CollectionTracker<T>.Wrap(enumerable);
	}
}
