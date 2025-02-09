#pragma warning disable CA1031 // Do not catch general exception types
#pragma warning disable CA1032 // Implement standard exception constructors
#pragma warning disable IDE0063 // Use simple 'using' statement
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE0290 // Use primary constructor
#pragma warning disable IDE0300 // Simplify collection initialization

#if XUNIT_NULLABLE
#nullable enable
#else
// In case this is source-imported with global nullable enabled but no XUNIT_NULLABLE
#pragma warning disable CS8601
#pragma warning disable CS8604
#pragma warning disable CS8605
#pragma warning disable CS8618
#pragma warning disable CS8625
#pragma warning disable CS8767
#endif

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;

#if XUNIT_NULLABLE
using System.Diagnostics.CodeAnalysis;
#endif

namespace Xunit.Sdk
{
	static class AssertEqualityComparer
	{
		static readonly ConcurrentDictionary<Type, IEqualityComparer> cachedDefaultComparers = new ConcurrentDictionary<Type, IEqualityComparer>();
		static readonly ConcurrentDictionary<Type, IEqualityComparer> cachedDefaultInnerComparers = new ConcurrentDictionary<Type, IEqualityComparer>();
#if XUNIT_NULLABLE
		static readonly object?[] singleNullObject = new object?[] { null };
#else
		static readonly object[] singleNullObject = new object[] { null };
#endif

		/// <summary>
		/// Gets the default comparer to be used for the provided <paramref name="type"/> when a custom one
		/// has not been provided. Creates an instance of <see cref="AssertEqualityComparer{T}"/> wrapped
		/// by <see cref="AssertEqualityComparerAdapter{T}"/>.
		/// </summary>
		/// <param name="type">The type to be compared</param>
		internal static IEqualityComparer GetDefaultComparer(Type type) =>
			cachedDefaultComparers.GetOrAdd(type, itemType =>
			{
				var comparerType = typeof(AssertEqualityComparer<>).MakeGenericType(itemType);
				var comparer =
					Activator.CreateInstance(comparerType, singleNullObject)
						?? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Could not create instance of AssertEqualityComparer<{0}>", itemType.FullName ?? itemType.Name));

				var wrapperType = typeof(AssertEqualityComparerAdapter<>).MakeGenericType(itemType);
				var result =
					Activator.CreateInstance(wrapperType, new object[] { comparer }) as IEqualityComparer
						?? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Could not create instance of AssertEqualityComparerAdapter<{0}>", itemType.FullName ?? itemType.Name));

				return result;
			});

		/// <summary>
		/// Gets the default comparer to be used as an inner comparer for the provided <paramref name="type"/>
		/// when a custom one has not been provided. For non-collections, this defaults to an <see cref="object"/>-based
		/// comparer; for collections, this creates an inner comparer based on the item type in the collection.
		/// </summary>
		/// <param name="type">The type to create an inner comparer for</param>
		internal static IEqualityComparer GetDefaultInnerComparer(Type type) =>
			cachedDefaultInnerComparers.GetOrAdd(type, t =>
			{
				var innerType = typeof(object);

				// string is enumerable, but we don't treat it like a collection
				if (t != typeof(string))
				{
					var enumerableOfT =
						t
							.GetInterfaces()
							.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

					if (enumerableOfT != null)
						innerType = enumerableOfT.GenericTypeArguments[0];
				}

				return GetDefaultComparer(innerType);
			});

		/// <summary>
		/// This exception is thrown when an operation failure has occured during equality comparison operations.
		/// This generally indicates that a necessary pre-condition was not met for comparison operations to succeed.
		/// </summary>
		public sealed class OperationalFailureException : Exception
		{
			OperationalFailureException(string message) :
				base(message)
			{ }

			/// <summary>
			/// Gets an exception that indicates that GetHashCode was called on <see cref="AssertEqualityComparer{T}.FuncEqualityComparer"/>
			/// which usually indicates that an item comparison function was used to try to compare two hash sets.
			/// </summary>
			public static OperationalFailureException ForIllegalGetHashCode() =>
				new OperationalFailureException("During comparison of two collections, GetHashCode was called, but only a comparison function was provided. This typically indicates trying to compare two sets with an item comparison function, which is not supported. For more information, see https://xunit.net/docs/hash-sets-vs-linear-containers");
		}
	}

	/// <summary>
	/// Default implementation of <see cref="IAssertEqualityComparer{T}" /> used by the assertion library.
	/// </summary>
	/// <typeparam name="T">The type that is being compared.</typeparam>
	sealed class AssertEqualityComparer<T> : IAssertEqualityComparer<T>
	{
		internal static readonly IEqualityComparer DefaultInnerComparer = AssertEqualityComparer.GetDefaultInnerComparer(typeof(T));

		static readonly ConcurrentDictionary<Type, Type> cacheOfIComparableOfT = new ConcurrentDictionary<Type, Type>();
		static readonly ConcurrentDictionary<Type, Type> cacheOfIEquatableOfT = new ConcurrentDictionary<Type, Type>();
		readonly Lazy<IEqualityComparer> innerComparer;
		static readonly Type typeKeyValuePair = typeof(KeyValuePair<,>);

		/// <summary>
		/// Initializes a new instance of the <see cref="AssertEqualityComparer{T}" /> class.
		/// </summary>
		/// <param name="innerComparer">The inner comparer to be used when the compared objects are enumerable.</param>
#if XUNIT_NULLABLE
		public AssertEqualityComparer(IEqualityComparer? innerComparer = null)
#else
		public AssertEqualityComparer(IEqualityComparer innerComparer = null)
#endif
		{
			// Use a thunk to delay evaluation of DefaultInnerComparer
			this.innerComparer = new Lazy<IEqualityComparer>(() => innerComparer ?? AssertEqualityComparer<T>.DefaultInnerComparer);
		}

		public IEqualityComparer InnerComparer =>
			innerComparer.Value;

		/// <inheritdoc/>
		public bool Equals(
#if XUNIT_NULLABLE
			T? x,
			T? y)
#else
			T x,
			T y)
#endif
		{
			using (var xTracker = x.AsNonStringTracker())
			using (var yTracker = y.AsNonStringTracker())
				return Equals(x, xTracker, y, yTracker).Equal;
		}

		/// <inheritdoc/>
		public AssertEqualityResult Equals(
#if XUNIT_NULLABLE
			T? x,
			CollectionTracker? xTracker,
			T? y,
			CollectionTracker? yTracker)
#else
			T x,
			CollectionTracker xTracker,
			T y,
			CollectionTracker yTracker)
#endif
		{
			// Null?
			if (x == null && y == null)
				return AssertEqualityResult.ForResult(true, x, y);
			if (x == null || y == null)
				return AssertEqualityResult.ForResult(false, x, y);

			// If you point at the same thing, you're equal
			if (ReferenceEquals(x, y))
				return AssertEqualityResult.ForResult(true, x, y);

			// We want the inequality indices for strings
			if (x is string xString && y is string yString)
				return StringAssertEqualityComparer.Equivalent(xString, yString);

			var xType = x.GetType();
			var yType = y.GetType();

			// ImmutableArray<T> defines IEquatable<ImmutableArray<T>> in a way that isn't consistent with the
			// needs of an assertion library. https://github.com/xunit/xunit/issues/3137
			if (!xType.IsGenericType || xType.GetGenericTypeDefinition() != typeof(ImmutableArray<>))
			{
				// Implements IEquatable<T>?
				if (x is IEquatable<T> equatable)
					return AssertEqualityResult.ForResult(equatable.Equals(y), x, y);

				// Implements IEquatable<typeof(y)>?
				if (xType != yType)
				{
					var iequatableY = cacheOfIEquatableOfT.GetOrAdd(yType, (t) => typeof(IEquatable<>).MakeGenericType(t));
					if (iequatableY.IsAssignableFrom(xType))
					{
						var equalsMethod = iequatableY.GetMethod(nameof(IEquatable<T>.Equals));
						if (equalsMethod == null)
							return AssertEqualityResult.ForResult(false, x, y);

#if XUNIT_NULLABLE
						return AssertEqualityResult.ForResult(equalsMethod.Invoke(x, new object[] { y }) is true, x, y);
#else
						return AssertEqualityResult.ForResult((bool)equalsMethod.Invoke(x, new object[] { y }), x, y);
#endif
					}
				}
			}

			// Special case collections (before IStructuralEquatable because arrays implement that in a way we don't want to call)
			if (xTracker != null && yTracker != null)
				return CollectionTracker.AreCollectionsEqual(xTracker, yTracker, InnerComparer, InnerComparer == DefaultInnerComparer);

			// Implements IStructuralEquatable?
			if (x is IStructuralEquatable structuralEquatable && structuralEquatable.Equals(y, new TypeErasedEqualityComparer(innerComparer.Value)))
				return AssertEqualityResult.ForResult(true, x, y);

			// Implements IComparable<T>?
			if (x is IComparable<T> comparableGeneric)
				try
				{
					return AssertEqualityResult.ForResult(comparableGeneric.CompareTo(y) == 0, x, y);
				}
				catch
				{
					// Some implementations of IComparable<T>.CompareTo throw exceptions in
					// certain situations, such as if x can't compare against y.
					// If this happens, just swallow up the exception and continue comparing.
				}

			// Implements IComparable<typeof(y)>?
			if (xType != yType)
			{
				var icomparableY = cacheOfIComparableOfT.GetOrAdd(yType, (t) => typeof(IComparable<>).MakeGenericType(t));
				if (icomparableY.IsAssignableFrom(xType))
				{
					var compareToMethod = icomparableY.GetMethod(nameof(IComparable<T>.CompareTo));
					if (compareToMethod == null)
						return AssertEqualityResult.ForResult(false, x, y);

					try
					{
#if XUNIT_NULLABLE
						return AssertEqualityResult.ForResult(compareToMethod.Invoke(x, new object[] { y }) is 0, x, y);
#else
						return AssertEqualityResult.ForResult((int)compareToMethod.Invoke(x, new object[] { y }) == 0, x, y);
#endif
					}
					catch
					{
						// Some implementations of IComparable.CompareTo throw exceptions in
						// certain situations, such as if x can't compare against y.
						// If this happens, just swallow up the exception and continue comparing.
					}
				}
			}

			// Implements IComparable?
			if (x is IComparable comparable)
				try
				{
					return AssertEqualityResult.ForResult(comparable.CompareTo(y) == 0, x, y);
				}
				catch
				{
					// Some implementations of IComparable.CompareTo throw exceptions in
					// certain situations, such as if x can't compare against y.
					// If this happens, just swallow up the exception and continue comparing.
				}

			// Special case KeyValuePair<K,V>
			if (xType.IsConstructedGenericType &&
				xType.GetGenericTypeDefinition() == typeKeyValuePair &&
				yType.IsConstructedGenericType &&
				yType.GetGenericTypeDefinition() == typeKeyValuePair)
			{
				var xKey = xType.GetRuntimeProperty("Key")?.GetValue(x);
				var yKey = yType.GetRuntimeProperty("Key")?.GetValue(y);

				if (xKey == null)
				{
					if (yKey != null)
						return AssertEqualityResult.ForResult(false, x, y);
				}
				else
				{
					var xKeyType = xKey.GetType();
					var yKeyType = yKey?.GetType();

					var keyComparer = AssertEqualityComparer.GetDefaultComparer(xKeyType == yKeyType ? xKeyType : typeof(object));
					if (!keyComparer.Equals(xKey, yKey))
						return AssertEqualityResult.ForResult(false, x, y);
				}

				var xValue = xType.GetRuntimeProperty("Value")?.GetValue(x);
				var yValue = yType.GetRuntimeProperty("Value")?.GetValue(y);

				if (xValue == null)
					return AssertEqualityResult.ForResult(yValue is null, x, y);

				var xValueType = xValue.GetType();
				var yValueType = yValue?.GetType();

				var valueComparer = AssertEqualityComparer.GetDefaultComparer(xValueType == yValueType ? xValueType : typeof(object));
				return AssertEqualityResult.ForResult(valueComparer.Equals(xValue, yValue), x, y);
			}

			// Last case, rely on object.Equals
			return AssertEqualityResult.ForResult(object.Equals(x, y), x, y);
		}

#if XUNIT_NULLABLE
		public static IEqualityComparer<T?> FromComparer(Func<T, T, bool> comparer) =>
#else
		public static IEqualityComparer<T> FromComparer(Func<T, T, bool> comparer) =>
#endif
			new FuncEqualityComparer(comparer);

		/// <inheritdoc/>
		public int GetHashCode(T obj) =>
			innerComparer.Value.GetHashCode(GuardArgumentNotNull(nameof(obj), obj));

#if XUNIT_NULLABLE
		sealed class FuncEqualityComparer : IEqualityComparer<T?>
#else
		sealed class FuncEqualityComparer : IEqualityComparer<T>
#endif
		{
			readonly Func<T, T, bool> comparer;

			public FuncEqualityComparer(Func<T, T, bool> comparer) =>
				this.comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));

			public bool Equals(
#if XUNIT_NULLABLE
				T? x,
				T? y)
#else
				T x,
				T y)
#endif
			{
				if (x == null)
					return y == null;

				if (y == null)
					return false;

				return comparer(x, y);
			}

#if XUNIT_NULLABLE
			public int GetHashCode(T? obj)
#else
			public int GetHashCode(T obj)
#endif
			{
#pragma warning disable CA1065  // This method should never be called, and this exception is a way to highlight if it does
				throw AssertEqualityComparer.OperationalFailureException.ForIllegalGetHashCode();
#pragma warning restore CA1065
			}
		}

		sealed class TypeErasedEqualityComparer : IEqualityComparer
		{
			readonly IEqualityComparer innerComparer;

			public TypeErasedEqualityComparer(IEqualityComparer innerComparer)
			{
				this.innerComparer = innerComparer;
			}

#if XUNIT_NULLABLE
			static MethodInfo? s_equalsMethod;
#else
			static MethodInfo s_equalsMethod;
#endif

			public new bool Equals(
#if XUNIT_NULLABLE
				object? x,
				object? y)
#else
				object x,
				object y)
#endif
			{
				if (x == null)
					return y == null;
				if (y == null)
					return false;

				// Delegate checking of whether two objects are equal to AssertEqualityComparer.
				// To get the best result out of AssertEqualityComparer, we attempt to specialize the
				// comparer for the objects that we are checking.
				// If the objects are the same, great! If not, assume they are objects.
				// This is more naive than the C# compiler which tries to see if they share any interfaces
				// etc. but that's likely overkill here as AssertEqualityComparer<object> is smart enough.
				var objectType = x.GetType() == y.GetType() ? x.GetType() : typeof(object);

				// Lazily initialize and cache the EqualsGeneric<U> method.
				if (s_equalsMethod == null)
				{
					s_equalsMethod = typeof(TypeErasedEqualityComparer).GetMethod(nameof(EqualsGeneric), BindingFlags.NonPublic | BindingFlags.Instance);
					if (s_equalsMethod == null)
						return false;
				}

#if XUNIT_NULLABLE
				return s_equalsMethod.MakeGenericMethod(objectType).Invoke(this, new object[] { x, y }) is true;
#else
				return (bool)s_equalsMethod.MakeGenericMethod(objectType).Invoke(this, new object[] { x, y });
#endif
			}

			bool EqualsGeneric<U>(
				U x,
				U y) =>
					new AssertEqualityComparer<U>(innerComparer: innerComparer).Equals(x, y);

			public int GetHashCode(object obj) =>
				GuardArgumentNotNull(nameof(obj), obj).GetHashCode();
		}

		/// <summary/>
#if XUNIT_NULLABLE
		[return: NotNull]
#endif
		internal static TArg GuardArgumentNotNull<TArg>(
			string argName,
#if XUNIT_NULLABLE
			[NotNull] TArg? argValue)
#else
			TArg argValue)
#endif
		{
			if (argValue == null)
				throw new ArgumentNullException(argName.TrimStart('@'));

			return argValue;
		}
	}
}
