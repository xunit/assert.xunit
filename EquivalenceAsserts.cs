#pragma warning disable CA1052 // Static holder types should be static

#if XUNIT_NULLABLE
#nullable enable
#else
// In case this is source-imported with global nullable enabled but no XUNIT_NULLABLE
#pragma warning disable CS8604
#endif

using System;
using System.Linq.Expressions;

#if XUNIT_AOT
using System.Diagnostics.CodeAnalysis;
#else
using System.Linq;
using Xunit.Internal;
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
#if XUNIT_AOT
		/// <summary>
		/// This assertion requires reflection, which is not available in Native AOT.
		/// </summary>
		[RequiresDynamicCode("This requires reflection, which is not available in Native AOT")]
#else
		/// <summary>
		/// Verifies that two objects are equivalent, using a default comparer. This comparison is done
		/// without regard to type, and only inspects public property and field values for individual
		/// equality. Deep equivalence tests (meaning, property or fields which are themselves complex
		/// types) are supported.
		/// </summary>
		/// <remarks>
		/// With strict mode off, object comparison allows <paramref name="actual"/> to have extra public
		/// members that aren't part of <paramref name="expected"/>, and collection comparison allows
		/// <paramref name="actual"/> to have more data in it than is present in <paramref name="expected"/>;
		/// with strict mode on, those rules are tightened to require exact member list (for objects) or
		/// data (for collections).
		/// </remarks>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The actual value</param>
		/// <param name="strict">A flag which enables strict comparison mode</param>
#endif
		public static void Equivalent(
#if XUNIT_NULLABLE
			object? expected,
			object? actual,
#else
			object expected,
			object actual,
#endif
			bool strict = false)
		{
#if XUNIT_AOT
			throw new PlatformNotSupportedException("This requires reflection, which is not available in Native AOT");
#else
			var ex = AssertHelper.VerifyEquivalence(expected, actual, strict);
			if (ex != null)
				throw ex;
#endif
		}

#if XUNIT_AOT
		/// <summary>
		/// This assertion requires reflection, which is not available in Native AOT.
		/// </summary>
		[RequiresDynamicCode("This requires reflection, which is not available in Native AOT")]
#else
		/// <summary>
		/// Verifies that two objects are equivalent, using a default comparer. This comparison is done
		/// without regard to type, and only inspects public property and field values for individual
		/// equality. Deep equivalence tests (meaning, property or fields which are themselves complex
		/// types) are supported. Members can be excluded from the comparison by passing them as
		/// expressions via <paramref name="exclusionExpressions"/> (using lambda expressions).
		/// </summary>
		/// <typeparam name="T">The type of the actual value</typeparam>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The actual value</param>
		/// <param name="exclusionExpressions">The expressions for exclusions</param>
#endif
		public static void EquivalentWithExclusions<T>(
#if XUNIT_NULLABLE
			object? expected,
#else
			object expected,
#endif
			T actual,
#if XUNIT_NULLABLE
			params Expression<Func<T, object?>>[] exclusionExpressions) =>
#else
			params Expression<Func<T, object>>[] exclusionExpressions) =>
#endif
				EquivalentWithExclusions(expected, actual, strict: false, exclusionExpressions);

#if XUNIT_AOT
		/// <summary>
		/// This assertion requires reflection, which is not available in Native AOT.
		/// </summary>
		[RequiresDynamicCode("This requires reflection, which is not available in Native AOT")]
#else
		/// <summary>
		/// Verifies that two objects are equivalent, using a default comparer. This comparison is done
		/// without regard to type, and only inspects public property and field values for individual
		/// equality. Deep equivalence tests (meaning, property or fields which are themselves complex
		/// types) are supported. Members can be excluded from the comparison by passing them as
		/// expressions via <paramref name="exclusionExpressions"/> (using lambda expressions).
		/// </summary>
		/// <remarks>
		/// With strict mode off, object comparison allows <paramref name="actual"/> to have extra public
		/// members that aren't part of <paramref name="expected"/>, and collection comparison allows
		/// <paramref name="actual"/> to have more data in it than is present in <paramref name="expected"/>;
		/// with strict mode on, those rules are tightened to require exact member list (for objects) or
		/// data (for collections).
		/// </remarks>
		/// <typeparam name="T">The type of the actual value</typeparam>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The actual value</param>
		/// <param name="strict">A flag which enables strict comparison mode</param>
		/// <param name="exclusionExpressions">The expressions for exclusions</param>
#endif
		public static void EquivalentWithExclusions<T>(
#if XUNIT_NULLABLE
			object? expected,
#else
			object expected,
#endif
			T actual,
			bool strict,
#if XUNIT_NULLABLE
			params Expression<Func<T, object?>>[] exclusionExpressions)
#else
			params Expression<Func<T, object>>[] exclusionExpressions)
#endif
		{
#if XUNIT_AOT
			throw new PlatformNotSupportedException("This requires reflection, which is not available in Native AOT");
#else
			var exclusions = AssertHelper.ParseExclusionExpressions(exclusionExpressions);

			var ex = AssertHelper.VerifyEquivalence(expected, actual, strict, exclusions);
			if (ex != null)
				throw ex;
#endif
		}

#if XUNIT_AOT
		/// <summary>
		/// This assertion requires reflection, which is not available in Native AOT.
		/// </summary>
		[RequiresDynamicCode("This requires reflection, which is not available in Native AOT")]
#else
		/// <summary>
		/// Verifies that two objects are equivalent, using a default comparer. This comparison is done
		/// without regard to type, and only inspects public property and field values for individual
		/// equality. Deep equivalence tests (meaning, property or fields which are themselves complex
		/// types) are supported. Members can be excluded from the comparison by passing them as
		/// expressions via <paramref name="exclusionExpressions"/> (using <c>"Member.SubMember.SubSubMember"</c>
		/// form).
		/// </summary>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The actual value</param>
		/// <param name="exclusionExpressions">The expressions for exclusions. This should be provided
		/// in <c>"Member.SubMember.SubSubMember"</c> form for deep exclusions.</param>
#endif
		public static void EquivalentWithExclusions(
#if XUNIT_NULLABLE
			object? expected,
			object? actual,
#else
			object expected,
			object actual,
#endif
			params string[] exclusionExpressions) =>
				EquivalentWithExclusions(expected, actual, strict: false, exclusionExpressions);

#if XUNIT_AOT
		/// <summary>
		/// This assertion requires reflection, which is not available in Native AOT.
		/// </summary>
		[RequiresDynamicCode("This requires reflection, which is not available in Native AOT")]
#else
		/// <summary>
		/// Verifies that two objects are equivalent, using a default comparer. This comparison is done
		/// without regard to type, and only inspects public property and field values for individual
		/// equality. Deep equivalence tests (meaning, property or fields which are themselves complex
		/// types) are supported. Members can be excluded from the comparison by passing them as
		/// expressions via <paramref name="exclusionExpressions"/> (using <c>"Member.SubMember.SubSubMember"</c>
		/// form).
		/// </summary>
		/// <remarks>
		/// With strict mode off, object comparison allows <paramref name="actual"/> to have extra public
		/// members that aren't part of <paramref name="expected"/>, and collection comparison allows
		/// <paramref name="actual"/> to have more data in it than is present in <paramref name="expected"/>;
		/// with strict mode on, those rules are tightened to require exact member list (for objects) or
		/// data (for collections).
		/// </remarks>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The actual value</param>
		/// <param name="strict">A flag which enables strict comparison mode</param>
		/// <param name="exclusionExpressions">The expressions for exclusions. This should be provided
		/// in <c>"Member1.Member2.Member3"</c> form for deep exclusions.</param>
#endif
		public static void EquivalentWithExclusions(
#if XUNIT_NULLABLE
			object? expected,
			object? actual,
#else
			object expected,
			object actual,
#endif
			bool strict,
			params string[] exclusionExpressions)
		{
#if XUNIT_AOT
			throw new PlatformNotSupportedException("This requires reflection, which is not available in Native AOT");
#else
			var exclusions = AssertHelper.ParseExclusionExpressions(exclusionExpressions);

			var ex = AssertHelper.VerifyEquivalence(expected, actual, strict, exclusions);
			if (ex != null)
				throw ex;
#endif
		}
	}
}
