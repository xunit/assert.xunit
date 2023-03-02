#if XUNIT_NULLABLE
#nullable enable
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Xunit.Sdk
{
	/// <summary>
	/// Exception thrown when two values are unexpectedly not equal.
	/// </summary>
#if XUNIT_VISIBILITY_INTERNAL
	internal
#else
	public
#endif
	class EqualException : AssertActualExpectedException
	{
		static readonly Dictionary<char, string> Encodings = new Dictionary<char, string>
		{
			{ '\r', "\\r" },
			{ '\n', "\\n" },
			{ '\t', "\\t" },
			{ '\0', "\\0" }
		};

#if XUNIT_NULLABLE
		string? message;
#else
		string message;
#endif

		/// <summary>
		/// Creates a new instance of the <see cref="EqualException"/> class.
		/// </summary>
		/// <param name="expected">The expected object value</param>
		/// <param name="actual">The actual object value</param>
		public EqualException(
#if XUNIT_NULLABLE
			object? expected,
			object? actual) :
#else
			object expected,
			object actual) :
#endif
				base(expected, actual, "Assert.Equal() Failure")
		{
			ActualIndex = -1;
			ExpectedIndex = -1;
		}

		/// <summary>
		/// Creates a new instance of the <see cref="EqualException"/> class for string comparisons.
		/// </summary>
		/// <param name="expected">The expected string value</param>
		/// <param name="actual">The actual string value</param>
		/// <param name="expectedIndex">The first index in the expected string where the strings differ</param>
		/// <param name="actualIndex">The first index in the actual string where the strings differ</param>
		public EqualException(
#if XUNIT_NULLABLE
			string? expected,
			string? actual,
			int expectedIndex,
			int actualIndex) :
#else
			string expected,
			string actual,
			int expectedIndex,
			int actualIndex) :
#endif
				this(expected, actual, expectedIndex, actualIndex, null)
		{ }

		EqualException(
#if XUNIT_NULLABLE
			string? expected,
			string? actual,
			int expectedIndex,
			int actualIndex,
			int? pointerPosition) :
#else
			string expected,
			string actual,
			int expectedIndex,
			int actualIndex,
			int? pointerPosition) :
#endif
				base(expected, actual, "Assert.Equal() Failure")
		{
			ActualIndex = actualIndex;
			ExpectedIndex = expectedIndex;
			PointerPosition = pointerPosition;
		}

		EqualException(
#if XUNIT_NULLABLE
			string? expected,
			string? actual,
			int expectedIndex,
			int actualIndex,
			string? expectedLength,
			string? actualLength,
			string? expectedType,
			string? actualType,
			int? pointerPosition) :
#else
			string expected,
			string actual,
			int expectedIndex,
			int actualIndex,
			string expectedLength,
			string actualLength,
			string expectedType,
			string actualType,
			int? pointerPosition) :
#endif
				this(expected, actual, expectedIndex, actualIndex, pointerPosition)
		{
			ActualLength = actualLength;
			ExpectedLength = expectedLength;
			ActualType = actualType;
			ExpectedType = expectedType;
		}

		/// <summary>
		/// Gets the index into the actual value where the values first differed.
		/// Returns -1 if the difference index points were not provided.
		/// </summary>
		public int ActualIndex { get; }

		/// <summary>
		/// Gets the index into the expected value where the values first differed.
		/// Returns -1 if the difference index points were not provided.
		/// </summary>
		public int ExpectedIndex { get; }

		/// <summary>
		/// Gets the type of the actual value of the first values differed.
		/// Returns null if the type was not provided.
		/// </summary>
#if XUNIT_NULLABLE
		public string? ActualType { get; }
#else
		public string ActualType { get; }
#endif

		/// <summary>
		/// Gets the type of the expected value of the first values differed.
		/// Returns null if the type was not provided.
		/// </summary>
#if XUNIT_NULLABLE
		public string? ExpectedType { get; }
#else
		public string ExpectedType { get; }
#endif

		/// <summary>
		/// Gets the length of the actual collection.
		/// Returns null if the length was not provided.
		/// </summary>
#if XUNIT_NULLABLE
		public string? ActualLength { get; }
#else
		public string ActualLength { get; }
#endif

		/// <summary>
		/// Gets the length of the expected collection.
		/// Returns null if the length was not provided.
		/// </summary>
#if XUNIT_NULLABLE
		public string? ExpectedLength { get; }
#else
		public string ExpectedLength { get; }
#endif

		/// <inheritdoc/>
		public override string Message
		{
			get
			{
				if (message == null)
					message = CreateMessage();

				return message;
			}
		}

		/// <summary>
		/// Gets the index of the difference between the IEnumerables when converted to a string.
		/// </summary>
		public int? PointerPosition { get; private set; }

		string CreateMessage()
		{
			if (ExpectedIndex == -1)
				return base.Message;

			var undefinedType = string.IsNullOrEmpty(ActualType) || string.IsNullOrEmpty(ExpectedType);

			var actualTypeMessage = undefinedType || ExpectedType == ActualType ? string.Empty : ActualType;
			var expectedTypeMessage = undefinedType || ExpectedType == ActualType ? string.Empty : ExpectedType;

			var undefinedLength = string.IsNullOrEmpty(ActualLength) || string.IsNullOrEmpty(ExpectedLength);

			var actualLengthMessage = undefinedLength || ExpectedLength == ActualLength ? string.Empty : ActualLength;
			var expectedLengthMessage = undefinedLength || ExpectedLength == ActualLength ? string.Empty : ExpectedLength;

			var printedExpected = ShortenAndEncode(Expected, expectedTypeMessage, expectedLengthMessage, PointerPosition ?? ExpectedIndex, '↓', ExpectedIndex);
			var printedActual = ShortenAndEncode(Actual, actualTypeMessage, actualLengthMessage, PointerPosition ?? ActualIndex, '↑', ActualIndex);

			var sb = new StringBuilder();
			sb.Append(UserMessage);

			if (!string.IsNullOrWhiteSpace(printedExpected.Item2))
				sb.AppendFormat(
					CultureInfo.CurrentCulture,
					"{0}          {1}",
					Environment.NewLine,
					printedExpected.Item2
				);

			sb.AppendFormat(
				CultureInfo.CurrentCulture,
				"{0}Expected: {1}{0}Actual:   {2}",
				Environment.NewLine,
				printedExpected.Item1,
				printedActual.Item1
			);

			if (!string.IsNullOrWhiteSpace(printedActual.Item2))
				sb.AppendFormat(
					CultureInfo.CurrentCulture,
					"{0}          {1}",
					Environment.NewLine,
					printedActual.Item2
				);

			return sb.ToString();
		}

		/// <summary>
		/// Creates a new instance of the <see cref="EqualException"/> class for IEnumerable comparisons.
		/// </summary>
		/// <param name="expected">The expected object value</param>
		/// <param name="actual">The actual object value</param>
		/// <param name="mismatchIndex">The first index in the expected IEnumerable where the strings differ</param>
		public static EqualException FromEnumerable(
#if XUNIT_NULLABLE
			IEnumerable? expected,
			IEnumerable? actual,
#else
			IEnumerable expected,
			IEnumerable actual,
#endif
			int mismatchIndex)
		{
			int? pointerPositionExpected;
			int? pointerPositionActual;

			var expectedText = ArgumentFormatter.Format(expected, out pointerPositionExpected, mismatchIndex);
			var actualText = ArgumentFormatter.Format(actual, out pointerPositionActual, mismatchIndex);
			var pointerPosition = (pointerPositionExpected ?? -1) > (pointerPositionActual ?? -1) ? pointerPositionExpected : pointerPositionActual;

			var expectedEnumerable = expected?.Cast<object>();
			var actualEnumerable = actual?.Cast<object>();

			bool mismatchLength = expectedEnumerable?.Count() != actualEnumerable?.Count();
			var expectedLength = mismatchLength ? expectedEnumerable?.Count().ToString() : string.Empty;
			var actualLength = mismatchLength ? actualEnumerable?.Count().ToString() : string.Empty;

			var expectedType = mismatchIndex < expectedEnumerable?.Count() ? expectedEnumerable.ElementAt(mismatchIndex)?.GetType().FullName : string.Empty;
			var actualType = mismatchIndex < actualEnumerable?.Count() ? actualEnumerable.ElementAt(mismatchIndex)?.GetType().FullName : string.Empty;

			return new EqualException(expectedText, actualText, mismatchIndex, mismatchIndex, expectedLength, actualLength, expectedType, actualType, pointerPosition);
		}


		static Tuple<string, string> ShortenAndEncode(
#if XUNIT_NULLABLE
			string? value,
			string? type,
			string? length,
#else
			string value,
			string type,
			string length,
#endif
			int position,
			char pointer,
			int? index = null)
		{
			if (value == null)
				return Tuple.Create("(null)", "");

			index = index ?? position;

			var start = Math.Max(position - 20, 0);
			var end = Math.Min(position + 41, value.Length);
			var printedValue = new StringBuilder(100);
			var printedPointer = new StringBuilder(100);

			if (start > 0)
			{
				printedValue.Append("···");
				printedPointer.Append("   ");
			}

			for (var idx = start; idx < end; ++idx)
			{
				var c = value[idx];
				var paddingLength = 1;

#if XUNIT_NULLABLE
				string? encoding;
#else
				string encoding;
#endif

				if (Encodings.TryGetValue(c, out encoding))
				{
					printedValue.Append(encoding);
					paddingLength = encoding.Length;
				}
				else
					printedValue.Append(c);

				if (idx < position)
					printedPointer.Append(' ', paddingLength);
				else if (idx == position)
				{
					printedPointer.AppendFormat("{0} (pos {1}", pointer, index);
					if (!string.IsNullOrEmpty(type))
						printedPointer.AppendFormat(", type {0}", type);
					if (!string.IsNullOrEmpty(length))
						printedPointer.AppendFormat(", length {0}", length);
					printedPointer.Append(')');
				}
			}

			if (value.Length == position)
				printedPointer.AppendFormat("{0} (pos {1})", pointer, index);

			if (end < value.Length)
				printedValue.Append("···");

			return Tuple.Create(printedValue.ToString(), printedPointer.ToString());
		}
	}
}
