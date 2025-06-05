//#pragma warning disable CA1031 // Do not catch general exception types
//#pragma warning disable IDE0019 // Use pattern matching
#pragma warning disable IDE0090 // Use 'new(...)'
//#pragma warning disable IDE0300 // Collection initialization can be simplified
//#pragma warning disable IDE0301 // Simplify collection initialization
//#pragma warning disable IDE0305 // Simplify collection initialization

#if XUNIT_NULLABLE
#nullable enable
#else
// In case this is source-imported with global nullable enabled but no XUNIT_NULLABLE
//#pragma warning disable CS8600
//#pragma warning disable CS8601
#pragma warning disable CS8603
//#pragma warning disable CS8604
//#pragma warning disable CS8620
//#pragma warning disable CS8621
//#pragma warning disable CS8625
//#pragma warning disable CS8767
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit.Sdk;

#if NET8_0_OR_GREATER
using System.Threading.Tasks;
#endif

#if XUNIT_NULLABLE
using System.Diagnostics.CodeAnalysis;
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
	static partial class AssertHelper
	{
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

		internal static bool IsCompilerGenerated(Type type) =>
			type.CustomAttributes.Any(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute");

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

#endif  // NET8_0_OR_GREATER

	}
}
