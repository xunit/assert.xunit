#pragma warning disable IDE0057 // Use range operator
#pragma warning disable IDE0305 // Simplify collection initialization

#if XUNIT_NULLABLE
#nullable enable
#else
// In case this is source-imported with global nullable enabled but no XUNIT_NULLABLE
#pragma warning disable CS8625
#pragma warning disable CS8767
#endif

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

#if XUNIT_NULLABLE
using System.Diagnostics.CodeAnalysis;
#endif

namespace Xunit.Internal
{
#if XUNIT_VISIBILITY_INTERNAL
	internal
#else
	public
#endif
	static partial class AssertHelper
	{
		/// <summary/>
		public static IReadOnlyList<(string Prefix, string Member)> ParseExclusionExpressions(params string[] exclusionExpressions)
		{
			var result = new List<(string Prefix, string Member)>();

			foreach (var expression in exclusionExpressions ?? throw new ArgumentNullException(nameof(exclusionExpressions)))
			{
				if (expression is null || expression.Length is 0)
					throw new ArgumentException("Null/empty expressions are not valid.", nameof(exclusionExpressions));

				var lastDotIdx = expression.LastIndexOf('.');
				if (lastDotIdx == 0)
					throw new ArgumentException(
						string.Format(
							CultureInfo.CurrentCulture,
							"Expression '{0}' is not valid. Expressions may not start with a period.",
							expression
						),
						nameof(exclusionExpressions)
					);

				if (lastDotIdx == expression.Length - 1)
					throw new ArgumentException(
						string.Format(
							CultureInfo.CurrentCulture,
							"Expression '{0}' is not valid. Expressions may not end with a period.",
							expression
						),
						nameof(exclusionExpressions)
					);

				if (lastDotIdx < 0)
					result.Add((string.Empty, expression));
				else
					result.Add((expression.Substring(0, lastDotIdx), expression.Substring(lastDotIdx + 1)));
			}

			return result;
		}

		/// <summary/>
		public static IReadOnlyList<(string Prefix, string Member)> ParseExclusionExpressions(params LambdaExpression[] exclusionExpressions)
		{
			var result = new List<(string Prefix, string Member)>();

			foreach (var expression in exclusionExpressions ?? throw new ArgumentNullException(nameof(exclusionExpressions)))
			{
				if (expression is null)
					throw new ArgumentException("Null expression is not valid.", nameof(exclusionExpressions));

				var memberExpression = default(MemberExpression);

				// The incoming expressions are T => object?, so any boxed struct starts with a conversion
				if (expression.Body.NodeType == ExpressionType.Convert && expression.Body is UnaryExpression unaryExpression)
					memberExpression = unaryExpression.Operand as MemberExpression;
				else
					memberExpression = expression.Body as MemberExpression;

				if (memberExpression is null)
					throw new ArgumentException(
						string.Format(
							CultureInfo.CurrentCulture,
							"Expression '{0}' is not supported. Only property or field expressions from the lambda parameter are supported.",
							expression
						),
						nameof(exclusionExpressions)
					);

				var pieces = new LinkedList<string>();

				while (true)
				{
					pieces.AddFirst(memberExpression.Member.Name);

					if (memberExpression.Expression?.NodeType == ExpressionType.Parameter)
						break;

					memberExpression = memberExpression.Expression as MemberExpression;

					if (memberExpression is null)
						throw new ArgumentException(
							string.Format(
								CultureInfo.CurrentCulture,
								"Expression '{0}' is not supported. Only property or field expressions from the lambda parameter are supported.",
								expression
							),
							nameof(exclusionExpressions)
						);
				}

				if (pieces.Last is null)
					continue;

				var member = pieces.Last.Value;
				pieces.RemoveLast();

				var prefix = string.Join(".", pieces.ToArray());
				result.Add((prefix, member));
			}

			return result;
		}

		static bool TryConvert(
			object value,
			Type targetType,
#if XUNIT_NULLABLE
			[NotNullWhen(true)] out object? converted)
#else
			out object converted)
#endif
		{
			try
			{
				converted = Convert.ChangeType(value, targetType, CultureInfo.CurrentCulture);
				return converted != null;
			}
			catch (InvalidCastException)
			{
				converted = null;
				return false;
			}
		}

		sealed class ReferenceEqualityComparer : IEqualityComparer<object>
		{
			public new bool Equals(
#if XUNIT_NULLABLE
				object? x,
				object? y) =>
#else
			object x,
			object y) =>
#endif
					ReferenceEquals(x, y);

#if XUNIT_NULLABLE
			public int GetHashCode([DisallowNull] object obj) =>
#else
		public int GetHashCode(object obj) =>
#endif
				obj.GetHashCode();
		}
	}
}
