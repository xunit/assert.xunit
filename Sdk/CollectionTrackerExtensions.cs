#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE0300 // Simplify collection initialization

#if XUNIT_NULLABLE
#nullable enable
#else
// In case this is source-imported with global nullable enabled but no XUNIT_NULLABLE
#pragma warning disable CS8601
#pragma warning disable CS8603
#pragma warning disable CS8604
#endif

using System.Collections;
using System.Collections.Generic;

#if !XUNIT_AOT
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
#endif

#if XUNIT_AOT || XUNIT_NULLABLE
using System.Diagnostics.CodeAnalysis;
#endif

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
#if !XUNIT_AOT

#if XUNIT_NULLABLE
		static readonly MethodInfo? asTrackerOpenGeneric =
#else
		static readonly MethodInfo asTrackerOpenGeneric =
#endif
			 typeof(CollectionTrackerExtensions).GetRuntimeMethods().FirstOrDefault(m => m.Name == nameof(AsTracker) && m.IsGenericMethod);

		static readonly ConcurrentDictionary<Type, MethodInfo> cacheOfAsTrackerByType = new ConcurrentDictionary<Type, MethodInfo>();

#endif

#if XUNIT_NULLABLE
		internal static CollectionTracker? AsNonStringTracker(this object? value)
#else
		internal static CollectionTracker AsNonStringTracker(this object value)
#endif
		{
			if (value == null || value is string)
				return null;

			return AsTracker(value as IEnumerable);
		}

		/// <summary>
		/// Wraps the given enumerable in an instance of <see cref="CollectionTracker{T}"/>.
		/// </summary>
		/// <param name="enumerable">The enumerable to be wrapped</param>
#if XUNIT_NULLABLE
		[return: NotNullIfNotNull(nameof(enumerable))]
		public static CollectionTracker? AsTracker(this IEnumerable? enumerable)
#else
		public static CollectionTracker AsTracker(this IEnumerable enumerable)
#endif
		{
			if (enumerable == null)
				return null;

			if (enumerable is CollectionTracker result)
				return result;

#if XUNIT_AOT
			return CollectionTracker.Wrap(enumerable);
#else
			// CollectionTracker.Wrap for the non-T enumerable uses the CastIterator, which has terrible
			// performance during iteration. We do our best to try to get a T and dynamically invoke the
			// generic version of AsTracker as we can.
			var iEnumerableOfT = enumerable.GetType().GetInterfaces().FirstOrDefault(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
			if (iEnumerableOfT == null)
				return CollectionTracker.Wrap(enumerable);

			var enumerableType = iEnumerableOfT.GenericTypeArguments[0];
#if XUNIT_NULLABLE
			var method = cacheOfAsTrackerByType.GetOrAdd(enumerableType, t => asTrackerOpenGeneric!.MakeGenericMethod(enumerableType));
#else
			var method = cacheOfAsTrackerByType.GetOrAdd(enumerableType, t => asTrackerOpenGeneric.MakeGenericMethod(enumerableType));
#endif

			return method.Invoke(null, new object[] { enumerable }) as CollectionTracker ?? CollectionTracker.Wrap(enumerable);
#endif
		}

		/// <summary>
		/// Wraps the given enumerable in an instance of <see cref="CollectionTracker{T}"/>.
		/// </summary>
		/// <typeparam name="T">The item type of the collection</typeparam>
		/// <param name="enumerable">The enumerable to be wrapped</param>
#if XUNIT_NULLABLE
		[return: NotNullIfNotNull(nameof(enumerable))]
		public static CollectionTracker<T>? AsTracker<T>(this IEnumerable<T>? enumerable) =>
#else
		public static CollectionTracker<T> AsTracker<T>(this IEnumerable<T> enumerable) =>
#endif
			enumerable == null
				? null
				: enumerable as CollectionTracker<T> ?? CollectionTracker<T>.Wrap(enumerable);

		/// <summary>
		/// Enumerates the elements inside the collection tracker.
		/// </summary>
		public static IEnumerator GetEnumerator(this CollectionTracker tracker)
		{
			Assert.GuardArgumentNotNull(nameof(tracker), tracker);

			return tracker.GetSafeEnumerator();
		}
	}
}
