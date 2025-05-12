#pragma warning disable CA1031 // Do not catch general exception types
#pragma warning disable IDE0019 // Use pattern matching
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE0300 // Collection initialization can be simplified
#pragma warning disable IDE0301 // Simplify collection initialization
#pragma warning disable IDE0305 // Simplify collection initialization

#if XUNIT_NULLABLE
#nullable enable
#else
// In case this is source-imported with global nullable enabled but no XUNIT_NULLABLE
#pragma warning disable CS8600
#pragma warning disable CS8601
#pragma warning disable CS8603
#pragma warning disable CS8604
#pragma warning disable CS8620
#pragma warning disable CS8621
#pragma warning disable CS8625
#pragma warning disable CS8767
#endif

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Xunit.Sdk;

#if XUNIT_NULLABLE
using System.Diagnostics.CodeAnalysis;
#endif

#if NET8_0_OR_GREATER
using System.Threading.Tasks;
#endif

namespace Xunit.Internal
{
	/// <summary>
	/// INTERNAL CLASS. DO NOT USE.
	/// </summary>
#if XUNIT_VISIBILITY_INTERNAL
	internal
#else
	public
#endif
	static class AssertHelper
	{
		static readonly IReadOnlyList<IReadOnlyList<string>> emptyExclusions = Array.Empty<IReadOnlyList<string>>();
		static readonly Dictionary<char, string> encodings = new Dictionary<char, string>
		{
			{ '\0', @"\0" },  // Null
			{ '\a', @"\a" },  // Alert
			{ '\b', @"\b" },  // Backspace
			{ '\f', @"\f" },  // Form feed
			{ '\n', @"\n" },  // New line
			{ '\r', @"\r" },  // Carriage return
			{ '\t', @"\t" },  // Horizontal tab
			{ '\v', @"\v" },  // Vertical tab
			{ '\\', @"\\" },  // Backslash
		};

#if XUNIT_NULLABLE
		static readonly ConcurrentDictionary<Type, Dictionary<string, Func<object?, object?>>> gettersByType = new ConcurrentDictionary<Type, Dictionary<string, Func<object?, object?>>>();
#else
		static readonly ConcurrentDictionary<Type, Dictionary<string, Func<object, object>>> gettersByType = new ConcurrentDictionary<Type, Dictionary<string, Func<object, object>>>();
#endif

#if XUNIT_NULLABLE
		static readonly Lazy<Type?> fileSystemInfoType = new Lazy<Type?>(() => GetTypeByName("System.IO.FileSystemInfo"));
		static readonly Lazy<PropertyInfo?> fileSystemInfoFullNameProperty = new Lazy<PropertyInfo?>(() => fileSystemInfoType.Value?.GetProperty("FullName"));
#else
		static readonly Lazy<Type> fileSystemInfoType = new Lazy<Type>(() => GetTypeByName("System.IO.FileSystemInfo"));
		static readonly Lazy<PropertyInfo> fileSystemInfoFullNameProperty = new Lazy<PropertyInfo>(() => fileSystemInfoType.Value?.GetProperty("FullName"));
#endif

		static readonly Lazy<Assembly[]> getAssemblies = new Lazy<Assembly[]>(AppDomain.CurrentDomain.GetAssemblies);
		static readonly Lazy<int> maxCompareDepth = new Lazy<int>(() =>
		{
			var stringValue = Environment.GetEnvironmentVariable(EnvironmentVariables.AssertEquivalentMaxDepth);
			if (stringValue is null || !int.TryParse(stringValue, out var intValue) || intValue <= 0)
				return EnvironmentVariables.Defaults.AssertEquivalentMaxDepth;
			return intValue;
		});
		static readonly Type objectType = typeof(object);
		static readonly IEqualityComparer<object> referenceEqualityComparer = new ReferenceEqualityComparer();

#if XUNIT_NULLABLE
		static Dictionary<string, Func<object?, object?>> GetGettersForType(Type type) =>
#else
		static Dictionary<string, Func<object, object>> GetGettersForType(Type type) =>
#endif
			gettersByType.GetOrAdd(type, _type =>
			{
				var fieldGetters =
					_type
						.GetRuntimeFields()
						.Where(f => f.IsPublic && !f.IsStatic)
#if XUNIT_NULLABLE
						.Select(f => new { name = f.Name, getter = (Func<object?, object?>)f.GetValue });
#else
						.Select(f => new { name = f.Name, getter = (Func<object, object>)f.GetValue });
#endif

				var propertyGetters =
					_type
						.GetRuntimeProperties()
						.Where(p =>
							p.CanRead
							&& p.GetMethod != null
							&& p.GetMethod.IsPublic
							&& !p.GetMethod.IsStatic
#if NET8_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
							&& !p.GetMethod.ReturnType.IsByRefLike
#endif
							&& p.GetIndexParameters().Length == 0
							&& !p.GetCustomAttributes<ObsoleteAttribute>().Any()
							&& !p.GetMethod.GetCustomAttributes<ObsoleteAttribute>().Any()
						)
						.GroupBy(p => p.Name)
						.Select(group =>
						{
							// When there is more than one property with the same name, we take the one from
							// the most derived class. Start assuming the first one is the correct one, and then
							// visit each in turn to see whether it's more derived or not.
							var targetProperty = group.First();

							foreach (var candidateProperty in group.Skip(1))
								for (var candidateType = candidateProperty.DeclaringType?.BaseType; candidateType != null; candidateType = candidateType.BaseType)
									if (targetProperty.DeclaringType == candidateType)
									{
										targetProperty = candidateProperty;
										break;
									}

#if XUNIT_NULLABLE
							return new { name = targetProperty.Name, getter = (Func<object?, object?>)targetProperty.GetValue };
#else
							return new { name = targetProperty.Name, getter = (Func<object, object>)targetProperty.GetValue };
#endif
						});

				return
					fieldGetters
						.Concat(propertyGetters)
						.ToDictionary(g => g.name, g => g.getter);
			});

#if XUNIT_NULLABLE
		static Type? GetTypeByName(string typeName)
#else
		static Type GetTypeByName(string typeName)
#endif
		{
			try
			{
				foreach (var assembly in getAssemblies.Value)
				{
					var type = assembly.GetType(typeName);
					if (type != null)
						return type;
				}

				return null;
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Fatal error: Exception occurred while trying to retrieve type '{0}'", typeName), ex);
			}
		}

		internal static bool IsCompilerGenerated(Type type) =>
			type.CustomAttributes.Any(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute");

		/// <summary/>
		public static IReadOnlyList<IReadOnlyList<string>> ParseMemberExpressions(params LambdaExpression[] expressions)
		{
			var result = new List<IReadOnlyList<string>>();

			foreach (var expression in expressions ?? throw new ArgumentNullException(nameof(expressions)))
			{
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
						nameof(expressions)
					);

				var pieces = new Stack<string>();

				while (true)
				{
					pieces.Push(memberExpression.Member.Name);

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
							nameof(expressions)
						);
				}

				result.Add(pieces.ToArray());
			}

			return result;
		}

		internal static string ShortenAndEncodeString(
#if XUNIT_NULLABLE
			string? value,
#else
			string value,
#endif
			int index,
			out int pointerIndent)
		{
			if (value == null)
			{
				pointerIndent = -1;
				return "null";
			}

			int start, end;

			if (ArgumentFormatter.MaxStringLength == int.MaxValue)
			{
				start = 0;
				end = value.Length;
			}
			else
			{
				var halfMaxLength = ArgumentFormatter.MaxStringLength / 2;
				start = Math.Max(index - halfMaxLength, 0);
				end = Math.Min(start + ArgumentFormatter.MaxStringLength, value.Length);
				start = Math.Max(end - ArgumentFormatter.MaxStringLength, 0);
			}

			// Set the initial buffer to include the possibility of quotes and ellipses, plus a few extra
			// characters for encoding before needing reallocation.
			var printedValue = new StringBuilder(end - start + 10);
			pointerIndent = 0;

			if (start > 0)
			{
				printedValue.Append(ArgumentFormatter.Ellipsis);
				pointerIndent += 3;
			}

			printedValue.Append('\"');
			pointerIndent++;

			for (var idx = start; idx < end; ++idx)
			{
				var c = value[idx];
				var paddingLength = 1;

				if (encodings.TryGetValue(c, out var encoding))
				{
					printedValue.Append(encoding);
					paddingLength = encoding.Length;
				}
				else
					printedValue.Append(c);

				if (idx < index)
					pointerIndent += paddingLength;
			}

			printedValue.Append('\"');

			if (end < value.Length)
				printedValue.Append(ArgumentFormatter.Ellipsis);

			return printedValue.ToString();
		}

#if XUNIT_NULLABLE
		internal static string ShortenAndEncodeString(string? value) =>
#else
		internal static string ShortenAndEncodeString(string value) =>
#endif
			ShortenAndEncodeString(value, 0, out var _);

#if XUNIT_NULLABLE
		internal static string ShortenAndEncodeStringEnd(string? value) =>
#else
		internal static string ShortenAndEncodeStringEnd(string value) =>
#endif
			ShortenAndEncodeString(value, (value?.Length - 1) ?? 0, out var _);

#if NET8_0_OR_GREATER

#if XUNIT_NULLABLE
		[return: NotNullIfNotNull(nameof(data))]
		internal static IEnumerable<T>? ToEnumerable<T>(IAsyncEnumerable<T>? data) =>
#else
		internal static IEnumerable<T> ToEnumerable<T>(IAsyncEnumerable<T> data) =>
#endif
			data == null ? null : ToEnumerableImpl(data);

		static IEnumerable<T> ToEnumerableImpl<T>(IAsyncEnumerable<T> data)
		{
			var enumerator = data.GetAsyncEnumerator();

			try
			{
				while (WaitForValueTask(enumerator.MoveNextAsync()))
					yield return enumerator.Current;
			}
			finally
			{
				WaitForValueTask(enumerator.DisposeAsync());
			}
		}

#endif  // NET8_0_OR_GREATER

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

#if XUNIT_NULLABLE
		static object? UnwrapLazy(
			object? value,
#else
		static object UnwrapLazy(
			object value,
#endif
			out Type valueType)
		{
			if (value == null)
			{
				valueType = objectType;

				return null;
			}

			valueType = value.GetType();

			if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Lazy<>))
			{
				var property = valueType.GetRuntimeProperty("Value");
				if (property != null)
				{
					valueType = valueType.GenericTypeArguments[0];
					return property.GetValue(value);
				}
			}

			return value;
		}

		/// <summary/>
#if XUNIT_NULLABLE
		public static EquivalentException? VerifyEquivalence(
			object? expected,
			object? actual,
#else
		public static EquivalentException VerifyEquivalence(
			object expected,
			object actual,
#endif
			bool strict,
#if XUNIT_NULLABLE
			IReadOnlyList<IReadOnlyList<string>>? exclusions = null) =>
#else
			IReadOnlyList<IReadOnlyList<string>> exclusions = null) =>
#endif
				VerifyEquivalence(
					expected,
					actual,
					strict,
					string.Empty,
					new HashSet<object>(referenceEqualityComparer),
					new HashSet<object>(referenceEqualityComparer),
					1,
					exclusions ?? emptyExclusions
				);

#if XUNIT_NULLABLE
		static EquivalentException? VerifyEquivalence(
			object? expected,
			object? actual,
#else
		static EquivalentException VerifyEquivalence(
			object expected,
			object actual,
#endif
			bool strict,
			string prefix,
			HashSet<object> expectedRefs,
			HashSet<object> actualRefs,
			int depth,
			IReadOnlyList<IReadOnlyList<string>> exclusions)
		{
			// Check for exceeded depth
			if (depth > maxCompareDepth.Value)
				return EquivalentException.ForExceededDepth(maxCompareDepth.Value, prefix);

			// Unwrap Lazy<T>
			expected = UnwrapLazy(expected, out var expectedType);
			actual = UnwrapLazy(actual, out var actualType);

			// Check for null equivalence
			if (expected == null)
				return
					actual == null
						? null
						: EquivalentException.ForMemberValueMismatch(expected, actual, prefix);

			if (actual == null)
				return EquivalentException.ForMemberValueMismatch(expected, actual, prefix);

			// Check for identical references
			if (ReferenceEquals(expected, actual))
				return null;

			// Prevent circular references
			if (expectedRefs.Contains(expected))
				return EquivalentException.ForCircularReference(string.Format(CultureInfo.CurrentCulture, "{0}.{1}", nameof(expected), prefix));

			if (actualRefs.Contains(actual))
				return EquivalentException.ForCircularReference(string.Format(CultureInfo.CurrentCulture, "{0}.{1}", nameof(actual), prefix));

			try
			{
				expectedRefs.Add(expected);
				actualRefs.Add(actual);

				// Primitive types, enums and strings should just fall back to their Equals implementation
				if (expectedType.IsPrimitive || expectedType.IsEnum || expectedType == typeof(string) || expectedType == typeof(decimal) || expectedType == typeof(Guid))
					return VerifyEquivalenceIntrinsics(expected, actual, prefix);

				// DateTime and DateTimeOffset need to be compared via IComparable (because of a circular
				// reference via the Date property).
				if (expectedType == typeof(DateTime) || expectedType == typeof(DateTimeOffset))
					return VerifyEquivalenceDateTime(expected, actual, prefix);

				// FileSystemInfo has a recursion problem when getting the root directory
				if (fileSystemInfoType.Value != null)
					if (fileSystemInfoType.Value.IsAssignableFrom(expectedType) && fileSystemInfoType.Value.IsAssignableFrom(actualType))
						return VerifyEquivalenceFileSystemInfo(expected, actual, strict, prefix, expectedRefs, actualRefs, depth, exclusions);

				// Uri can throw for relative URIs
				var expectedUri = expected as Uri;
				var actualUri = actual as Uri;
				if (expectedUri != null && actualUri != null)
					return VerifyEquivalenceUri(expectedUri, actualUri, prefix);

				// IGrouping<TKey,TValue> is special, since it implements IEnumerable<TValue>
				var expectedGroupingTypes = ArgumentFormatter.GetGroupingTypes(expected);
				if (expectedGroupingTypes != null)
				{
					var actualGroupingTypes = ArgumentFormatter.GetGroupingTypes(actual);
					if (actualGroupingTypes != null)
						return VerifyEquivalenceGroupings(expected, expectedGroupingTypes, actual, actualGroupingTypes, strict);
				}

				// Enumerables? Check equivalence of individual members
				if (expected is IEnumerable enumerableExpected && actual is IEnumerable enumerableActual)
					return VerifyEquivalenceEnumerable(enumerableExpected, enumerableActual, strict, prefix, expectedRefs, actualRefs, depth, exclusions);

				return VerifyEquivalenceReference(expected, actual, strict, prefix, expectedRefs, actualRefs, depth, exclusions);
			}
			finally
			{
				expectedRefs.Remove(expected);
				actualRefs.Remove(actual);
			}
		}

#if XUNIT_NULLABLE
		static EquivalentException? VerifyEquivalenceDateTime(
#else
		static EquivalentException VerifyEquivalenceDateTime(
#endif
			object expected,
			object actual,
			string prefix)
		{
			try
			{
				if (expected is IComparable expectedComparable)
					return
						expectedComparable.CompareTo(actual) == 0
							? null
							: EquivalentException.ForMemberValueMismatch(expected, actual, prefix);
			}
			catch (Exception ex)
			{
				return EquivalentException.ForMemberValueMismatch(expected, actual, prefix, ex);
			}

			try
			{
				if (actual is IComparable actualComparable)
					return
						actualComparable.CompareTo(expected) == 0
							? null
							: EquivalentException.ForMemberValueMismatch(expected, actual, prefix);
			}
			catch (Exception ex)
			{
				return EquivalentException.ForMemberValueMismatch(expected, actual, prefix, ex);
			}

			throw new InvalidOperationException(
				string.Format(
					CultureInfo.CurrentCulture,
					"VerifyEquivalenceDateTime was given non-DateTime(Offset) objects; typeof(expected) = {0}, typeof(actual) = {1}",
					ArgumentFormatter.FormatTypeName(expected.GetType()),
					ArgumentFormatter.FormatTypeName(actual.GetType())
				)
			);
		}

#if XUNIT_NULLABLE
		static EquivalentException? VerifyEquivalenceEnumerable(
#else
		static EquivalentException VerifyEquivalenceEnumerable(
#endif
			IEnumerable expected,
			IEnumerable actual,
			bool strict,
			string prefix,
			HashSet<object> expectedRefs,
			HashSet<object> actualRefs,
			int depth,
			IReadOnlyList<IReadOnlyList<string>> exclusions)
		{
#if XUNIT_NULLABLE
			var expectedValues = expected.Cast<object?>().ToList();
			var actualValues = actual.Cast<object?>().ToList();
#else
			var expectedValues = expected.Cast<object>().ToList();
			var actualValues = actual.Cast<object>().ToList();
#endif
			var actualOriginalValues = actualValues.ToList();

			// Walk the list of expected values, and look for actual values that are equivalent
			foreach (var expectedValue in expectedValues)
			{
				var actualIdx = 0;

				for (; actualIdx < actualValues.Count; ++actualIdx)
					if (VerifyEquivalence(expectedValue, actualValues[actualIdx], strict, "", expectedRefs, actualRefs, depth, exclusions) == null)
						break;

				if (actualIdx == actualValues.Count)
					return EquivalentException.ForMissingCollectionValue(expectedValue, actualOriginalValues, prefix);

				actualValues.RemoveAt(actualIdx);
			}

			if (strict && actualValues.Count != 0)
				return EquivalentException.ForExtraCollectionValue(expectedValues, actualOriginalValues, actualValues, prefix);

			return null;
		}

#if XUNIT_NULLABLE
		static EquivalentException? VerifyEquivalenceFileSystemInfo(
#else
		static EquivalentException VerifyEquivalenceFileSystemInfo(
#endif
			object expected,
			object actual,
			bool strict,
			string prefix,
			HashSet<object> expectedRefs,
			HashSet<object> actualRefs,
			int depth,
			IReadOnlyList<IReadOnlyList<string>> exclusions)
		{
			if (fileSystemInfoFullNameProperty.Value == null)
				throw new InvalidOperationException("Could not find 'FullName' property on type 'System.IO.FileSystemInfo'");

			var expectedType = expected.GetType();
			var actualType = actual.GetType();

			if (expectedType != actualType)
				return EquivalentException.ForMismatchedTypes(expectedType, actualType, prefix);

			var fullName = fileSystemInfoFullNameProperty.Value.GetValue(expected);
			var expectedAnonymous = new { FullName = fullName };

			return VerifyEquivalenceReference(expectedAnonymous, actual, strict, prefix, expectedRefs, actualRefs, depth, exclusions);
		}

#if XUNIT_NULLABLE
		static EquivalentException? VerifyEquivalenceGroupings(
#else
		static EquivalentException VerifyEquivalenceGroupings(
#endif
			object expected,
			Type[] expectedGroupingTypes,
			object actual,
			Type[] actualGroupingTypes,
			bool strict)
		{
			var expectedKey = typeof(IGrouping<,>).MakeGenericType(expectedGroupingTypes).GetRuntimeProperty("Key")?.GetValue(expected);
			var actualKey = typeof(IGrouping<,>).MakeGenericType(actualGroupingTypes).GetRuntimeProperty("Key")?.GetValue(actual);

			var keyException = VerifyEquivalence(expectedKey, actualKey, strict: false);
			if (keyException != null)
				return keyException;

			var toArrayMethod =
				typeof(Enumerable)
					.GetRuntimeMethods()
					.FirstOrDefault(m => m.IsStatic && m.IsPublic && m.Name == nameof(Enumerable.ToArray) && m.GetParameters().Length == 1)
						?? throw new InvalidOperationException("Could not find method Enumerable.ToArray<>");

			// Convert everything to an array so it doesn't endlessly loop on the IGrouping<> test
			var expectedToArrayMethod = toArrayMethod.MakeGenericMethod(expectedGroupingTypes[1]);
			var expectedValues = expectedToArrayMethod.Invoke(null, new[] { expected });

			var actualToArrayMethod = toArrayMethod.MakeGenericMethod(actualGroupingTypes[1]);
			var actualValues = actualToArrayMethod.Invoke(null, new[] { actual });

			if (VerifyEquivalence(expectedValues, actualValues, strict) != null)
				throw EquivalentException.ForGroupingWithMismatchedValues(expectedValues, actualValues, ArgumentFormatter.Format(expectedKey));

			return null;
		}

#if XUNIT_NULLABLE
		static EquivalentException? VerifyEquivalenceIntrinsics(
#else
		static EquivalentException VerifyEquivalenceIntrinsics(
#endif
			object expected,
			object actual,
			string prefix)
		{
			var result = expected.Equals(actual);

			if (!result && TryConvert(expected, actual.GetType(), out var converted))
				result = converted.Equals(actual);
			if (!result && TryConvert(actual, expected.GetType(), out converted))
				result = converted.Equals(expected);

			return result ? null : EquivalentException.ForMemberValueMismatch(expected, actual, prefix);
		}

#if XUNIT_NULLABLE
		static EquivalentException? VerifyEquivalenceReference(
#else
		static EquivalentException VerifyEquivalenceReference(
#endif
			object expected,
			object actual,
			bool strict,
			string prefix,
			HashSet<object> expectedRefs,
			HashSet<object> actualRefs,
			int depth,
			IReadOnlyList<IReadOnlyList<string>> exclusions)
		{
			Assert.GuardArgumentNotNull(nameof(prefix), prefix);

			var prefixDot = prefix.Length == 0 ? string.Empty : prefix + ".";

			// Enumerate over public instance fields and properties and validate equivalence
			var expectedGetters = GetGettersForType(expected.GetType());
			var actualGetters = GetGettersForType(actual.GetType());

			if (strict && expectedGetters.Count != actualGetters.Count)
				return EquivalentException.ForMemberListMismatch(expectedGetters.Keys, actualGetters.Keys, prefixDot);

			var excludedAtThisLevel =
				new HashSet<string>(
					exclusions
						.Select(e => e.Count >= depth ? e[depth - 1] : null)
						.Where(e => e != null)
#if XUNIT_NULLABLE
						.Select(e => e!)
#endif
				);

			foreach (var kvp in expectedGetters)
			{
				if (excludedAtThisLevel.Contains(kvp.Key))
					continue;

				if (!actualGetters.TryGetValue(kvp.Key, out var actualGetter))
					return EquivalentException.ForMemberListMismatch(expectedGetters.Keys, actualGetters.Keys, prefixDot);

				var expectedMemberValue = kvp.Value(expected);
				var actualMemberValue = actualGetter(actual);

				var ex = VerifyEquivalence(expectedMemberValue, actualMemberValue, strict, prefixDot + kvp.Key, expectedRefs, actualRefs, depth + 1, exclusions);
				if (ex != null)
					return ex;
			}

			return null;
		}

#if XUNIT_NULLABLE
		static EquivalentException? VerifyEquivalenceUri(
#else
		static EquivalentException VerifyEquivalenceUri(
#endif
			Uri expected,
			Uri actual,
			string prefix)
		{
			if (expected.OriginalString != actual.OriginalString)
				return EquivalentException.ForMemberValueMismatch(expected, actual, prefix);

			return null;
		}

#if NET8_0_OR_GREATER

		static void WaitForValueTask(ValueTask valueTask)
		{
			var valueTaskAwaiter = valueTask.GetAwaiter();
			if (valueTaskAwaiter.IsCompleted)
				return;

			// Let the task complete on a thread pool thread while we block the main thread
			Task.Run(valueTask.AsTask).GetAwaiter().GetResult();
		}

		static T WaitForValueTask<T>(ValueTask<T> valueTask)
		{
			var valueTaskAwaiter = valueTask.GetAwaiter();
			if (valueTaskAwaiter.IsCompleted)
				return valueTaskAwaiter.GetResult();

			// Let the task complete on a thread pool thread while we block the main thread
			return Task.Run(valueTask.AsTask).GetAwaiter().GetResult();
		}

#endif
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
