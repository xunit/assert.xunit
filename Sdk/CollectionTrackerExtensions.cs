#if XUNIT_NULLABLE
#nullable enable

using System.Diagnostics.CodeAnalysis;
#endif

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Xunit.Sdk
{
	static class CollectionTrackerExtensions
	{
#if XUNIT_NULLABLE
		[return: NotNullIfNotNull("enumerable")]
		public static CollectionTracker<object>? AsTracker(this IEnumerable? enumerable) =>
#else
		public static CollectionTracker<object> AsTracker(this IEnumerable enumerable) =>
#endif
			enumerable == null
				? null
				: enumerable as CollectionTracker<object> ?? CollectionTracker<object>.Wrap(enumerable.Cast<object>());

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
