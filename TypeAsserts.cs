#pragma warning disable CA1052 // Static holder types should be static
#pragma warning disable CA1720 // Identifier contains type name

#if XUNIT_NULLABLE
#nullable enable
#else
// In case this is source-imported with global nullable enabled but no XUNIT_NULLABLE
#pragma warning disable CS8604
#pragma warning disable CS8625
#endif

using System;
using System.Globalization;
using Xunit.Sdk;

#if XUNIT_NULLABLE
using System.Diagnostics.CodeAnalysis;
#endif

namespace Xunit
{
#if XUNIT_VISIBILITY_INTERNAL
	internal
#else
	public
#endif
	partial class Assert
	{
		/// <summary>
		/// Verifies that an object is of the given type or a derived type.
		/// </summary>
		/// <typeparam name="T">The type the object should be</typeparam>
		/// <param name="object">The object to be evaluated</param>
		/// <returns>The object, casted to type T when successful</returns>
		/// <exception cref="IsAssignableFromException">Thrown when the object is not the given type</exception>
#if XUNIT_NULLABLE
		public static T IsAssignableFrom<T>(object? @object)
#else
		public static T IsAssignableFrom<T>(object @object)
#endif
		{
#pragma warning disable xUnit2007
			IsAssignableFrom(typeof(T), @object);
#pragma warning restore xUnit2007
			return (T)@object;
		}

		/// <summary>
		/// Verifies that an object is of the given type or a derived type.
		/// </summary>
		/// <param name="expectedType">The type the object should be</param>
		/// <param name="object">The object to be evaluated</param>
		/// <exception cref="IsAssignableFromException">Thrown when the object is not the given type</exception>
		public static void IsAssignableFrom(
			Type expectedType,
#if XUNIT_NULLABLE
			[NotNull] object? @object)
#else
			object @object)
#endif
		{
			GuardArgumentNotNull(nameof(expectedType), expectedType);

			if (@object == null || !expectedType.IsAssignableFrom(@object.GetType()))
				throw IsAssignableFromException.ForIncompatibleType(expectedType, @object);
		}

		/// <summary>
		/// Verifies that an object is not of the given type or a derived type.
		/// </summary>
		/// <typeparam name="T">The type the object should not be</typeparam>
		/// <param name="object">The object to be evaluated</param>
		/// <returns>The object, casted to type T when successful</returns>
		/// <exception cref="IsNotAssignableFromException">Thrown when the object is of the given type</exception>
#if XUNIT_NULLABLE
		public static void IsNotAssignableFrom<T>(object? @object) =>
#else
		public static void IsNotAssignableFrom<T>(object @object) =>
#endif
			IsNotAssignableFrom(typeof(T), @object);

		/// <summary>
		/// Verifies that an object is not of the given type or a derived type.
		/// </summary>
		/// <param name="expectedType">The type the object should not be</param>
		/// <param name="object">The object to be evaluated</param>
		/// <exception cref="IsNotAssignableFromException">Thrown when the object is of the given type</exception>
		public static void IsNotAssignableFrom(
			Type expectedType,
#if XUNIT_NULLABLE
			object? @object)
#else
			object @object)
#endif
		{
			GuardArgumentNotNull(nameof(expectedType), expectedType);

			if (@object != null && expectedType.IsAssignableFrom(@object.GetType()))
				throw IsNotAssignableFromException.ForCompatibleType(expectedType, @object);
		}

		/// <summary>
		/// Verifies that an object is not exactly the given type.
		/// </summary>
		/// <typeparam name="T">The type the object should not be</typeparam>
		/// <param name="object">The object to be evaluated</param>
		/// <exception cref="IsNotTypeException">Thrown when the object is the given type</exception>
#if XUNIT_NULLABLE
		public static void IsNotType<T>(object? @object) =>
#else
		public static void IsNotType<T>(object @object) =>
#endif
#pragma warning disable xUnit2007
			IsNotType(typeof(T), @object);
#pragma warning restore xUnit2007

		/// <summary>
		/// Verifies that an object is not of the given type.
		/// </summary>
		/// <typeparam name="T">The type the object should not be</typeparam>
		/// <param name="object">The object to be evaluated</param>
		/// <param name="exactMatch">Will only fail with an exact type match when <c>true</c> is
		/// passed; will fail with a compatible type match when <c>false</c> is passed.</param>
		/// <exception cref="IsNotTypeException">Thrown when the object is the given type</exception>
#if XUNIT_NULLABLE
		public static void IsNotType<T>(
			object? @object,
#else
		public static void IsNotType<T>(
			object @object,
#endif
			bool exactMatch) =>
#pragma warning disable xUnit2007
				IsNotType(typeof(T), @object, exactMatch);
#pragma warning restore xUnit2007

		/// <summary>
		/// Verifies that an object is not exactly the given type.
		/// </summary>
		/// <param name="expectedType">The type the object should not be</param>
		/// <param name="object">The object to be evaluated</param>
		/// <exception cref="IsNotTypeException">Thrown when the object is the given type</exception>
		public static void IsNotType(
			Type expectedType,
#if XUNIT_NULLABLE
			object? @object) =>
#else
			object @object) =>
#endif
				IsNotType(expectedType, @object, exactMatch: true);

		/// <summary>
		/// Verifies that an object is not of the given type.
		/// </summary>
		/// <param name="expectedType">The type the object should not be</param>
		/// <param name="object">The object to be evaluated</param>
		/// <param name="exactMatch">Will only fail with an exact type match when <c>true</c> is
		/// passed; will fail with a compatible type match when <c>false</c> is passed.</param>
		/// <exception cref="IsNotTypeException">Thrown when the object is the given type</exception>
		public static void IsNotType(
			Type expectedType,
#if XUNIT_NULLABLE
			object? @object,
#else
			object @object,
#endif
			bool exactMatch)
		{
			GuardArgumentNotNull(nameof(expectedType), expectedType);

			if (exactMatch)
			{
				if (@object != null && expectedType.Equals(@object.GetType()))
					throw IsNotTypeException.ForExactType(expectedType);
			}
			else
			{
				var actualType = @object?.GetType();

				if (actualType != null && expectedType.IsAssignableFrom(actualType))
					throw IsNotTypeException.ForCompatibleType(expectedType, actualType);
			}
		}

		/// <summary>
		/// Verifies that an object is exactly the given type (and not a derived type).
		/// </summary>
		/// <typeparam name="T">The type the object should be</typeparam>
		/// <param name="object">The object to be evaluated</param>
		/// <returns>The object, casted to type T when successful</returns>
		/// <exception cref="IsTypeException">Thrown when the object is not the given type</exception>
#if XUNIT_NULLABLE
		public static T IsType<T>([NotNull] object? @object)
#else
		public static T IsType<T>(object @object)
#endif
		{
#pragma warning disable CA2263
#pragma warning disable xUnit2007
			IsType(typeof(T), @object, exactMatch: true);
#pragma warning restore xUnit2007
#pragma warning restore CA2263
			return (T)@object;
		}

		/// <summary>
		/// Verifies that an object of is the given type.
		/// </summary>
		/// <typeparam name="T">The type the object should be</typeparam>
		/// <param name="object">The object to be evaluated</param>
		/// <param name="exactMatch">Will only pass with an exact type match when <c>true</c> is
		/// passed; will pass with a compatible type match when <c>false</c> is passed.</param>
		/// <returns>The object, casted to type T when successful</returns>
		/// <exception cref="IsTypeException">Thrown when the object is not the given type</exception>
#if XUNIT_NULLABLE
		public static T IsType<T>(
			[NotNull] object? @object,
#else
		public static T IsType<T>(
			object @object,
#endif
			bool exactMatch)
		{
#pragma warning disable xUnit2007
			IsType(typeof(T), @object, exactMatch);
#pragma warning restore xUnit2007
			return (T)@object;
		}

		/// <summary>
		/// Verifies that an object is exactly the given type (and not a derived type).
		/// </summary>
		/// <param name="expectedType">The type the object should be</param>
		/// <param name="object">The object to be evaluated</param>
		/// <exception cref="IsTypeException">Thrown when the object is not the given type</exception>
		public static void IsType(
			Type expectedType,
#if XUNIT_NULLABLE
			[NotNull] object? @object) =>
#else
			object @object) =>
#endif
				IsType(expectedType, @object, exactMatch: true);

		/// <summary>
		/// Verifies that an object is of the given type.
		/// </summary>
		/// <param name="expectedType">The type the object should be</param>
		/// <param name="object">The object to be evaluated</param>
		/// <param name="exactMatch">Will only pass with an exact type match when <c>true</c> is
		/// passed; will pass with a compatible type match when <c>false</c> is passed.</param>
		/// <exception cref="IsTypeException">Thrown when the object is not the given type</exception>
		public static void IsType(
			Type expectedType,
#if XUNIT_NULLABLE
			[NotNull] object? @object,
#else
			object @object,
#endif
			bool exactMatch)
		{
			GuardArgumentNotNull(nameof(expectedType), expectedType);

			if (@object == null)
			{
				if (exactMatch)
					throw IsTypeException.ForMismatchedType(ArgumentFormatter.Format(expectedType), null);
				else
					throw IsTypeException.ForIncompatibleType(ArgumentFormatter.Format(expectedType), null);
			}

			var actualType = @object.GetType();
			var compatible =
				exactMatch
					? expectedType == actualType
					: expectedType.IsAssignableFrom(actualType);

			if (!compatible)
			{
				var expectedTypeName = ArgumentFormatter.Format(expectedType);
				var actualTypeName = ArgumentFormatter.Format(actualType);

				if (expectedTypeName == actualTypeName)
				{
					expectedTypeName += string.Format(CultureInfo.CurrentCulture, " (from {0})", expectedType.Assembly.GetName().FullName);
					actualTypeName += string.Format(CultureInfo.CurrentCulture, " (from {0})", actualType.Assembly.GetName().FullName);
				}

				if (exactMatch)
					throw IsTypeException.ForMismatchedType(expectedTypeName, actualTypeName);
				else
					throw IsTypeException.ForIncompatibleType(expectedTypeName, actualTypeName);
			}
		}
	}
}
