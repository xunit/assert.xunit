using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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

        string message;

        /// <summary>
        /// Creates a new instance of the <see cref="EqualException"/> class.
        /// </summary>
        /// <param name="expected">The expected object value</param>
        /// <param name="actual">The actual object value</param>
        public EqualException(object expected, object actual)
            : base(expected, actual, "Assert.Equal() Failure")
        {
            ActualIndex = -1;
            ExpectedIndex = -1;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="EqualException"/> class for IEnumerable comparisons.
        /// </summary>
        /// <param name="expected">The expected object value</param>
        /// <param name="actual">The actual object value</param>
        /// <param name="mismatchIndex">The first index in the expected IEnumerable where the strings differ</param>
        public EqualException(IEnumerable expected, IEnumerable actual, int mismatchIndex)
            : base(CreateIEnumerableMessage(expected, mismatchIndex, out int? pointerPositionExpected),
                  CreateIEnumerableMessage(actual, mismatchIndex, out int? pointerPositionActual), "Assert.Equal() Failure")
        {
            ActualIndex = mismatchIndex;
            ExpectedIndex = mismatchIndex;
            PointerPosition = (pointerPositionExpected ?? -1) > (pointerPositionActual ?? -1) ? pointerPositionExpected : pointerPositionActual;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="EqualException"/> class for string comparisons.
        /// </summary>
        /// <param name="expected">The expected string value</param>
        /// <param name="actual">The actual string value</param>
        /// <param name="expectedIndex">The first index in the expected string where the strings differ</param>
        /// <param name="actualIndex">The first index in the actual string where the strings differ</param>
        public EqualException(object expected, object actual, int expectedIndex, int actualIndex)
            : base(expected, actual, "Assert.Equal() Failure")
        {
            ActualIndex = actualIndex;
            ExpectedIndex = expectedIndex;
        }

        /// <summary>
        /// Gets the index into the actual value where the values first differed.
        /// Returns -1 if the difference index points were not provided.
        /// </summary>
        public int ActualIndex { get; private set; }

        /// <summary>
        /// Gets the index into the expected value where the values first differed.
        /// Returns -1 if the difference index points were not provided.
        /// </summary>
        public int ExpectedIndex { get; private set; }

        /// <summary>
        /// Gets the index of the difference between the IEunmerables when converted to a string.
        /// </summary>
        public int? PointerPosition { get; private set; }

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

        string CreateMessage()
        {
            if (ExpectedIndex == -1)
                return base.Message;

            Tuple<string, string> printedExpected = ShortenAndEncode(Expected, PointerPosition ?? ExpectedIndex, '↓', ExpectedIndex);
            Tuple<string, string> printedActual = ShortenAndEncode(Actual, PointerPosition ?? ActualIndex, '↑', ActualIndex);

            StringBuilder toFormat = new StringBuilder();
            toFormat.Append("{1}{0}");
            if (!string.IsNullOrWhiteSpace(printedExpected.Item2))
                toFormat.Append("          {2}{0}");
            toFormat.Append("Expected: {3}{0}");
            toFormat.Append("Actual:   {4}");
            if (!string.IsNullOrWhiteSpace(printedActual.Item2))
                toFormat.Append("{0}          {5}");

            return string.Format(
                CultureInfo.CurrentCulture,
                toFormat.ToString(),
                Environment.NewLine,
                UserMessage,
                printedExpected.Item2,
                printedExpected.Item1 ?? "(null)",
                printedActual.Item1 ?? "(null)",
                printedActual.Item2
            );
        }

        static Tuple<string, string> ShortenAndEncode(string value, int position, char pointer, int? index = null)
        {
            index = index ?? position;
            int start = Math.Max(position - 20, 0);
            int end = Math.Min(position + 41, value.Length);
            var printedValue = new StringBuilder(100);
            var printedPointer = new StringBuilder(100);

            if (start > 0)
            {
                printedValue.Append("···");
                printedPointer.Append("   ");
            }

            for (int idx = start; idx < end; ++idx)
            {
                char c = value[idx];
                string encoding;
                int paddingLength = 1;

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
                    printedPointer.AppendFormat("{0} (pos {1})", pointer, index);
            }

            if (value.Length == position)
                printedPointer.AppendFormat("{0} (pos {1})", pointer, index);

            if (end < value.Length)
                printedValue.Append("···");

            return new Tuple<string, string>(printedValue.ToString(), printedPointer.ToString());
        }

        static object CreateIEnumerableMessage(object value, int mismatchIndex, out int? pointerPosition)
        {
            return ArgumentFormatter.Format(value, out pointerPosition, mismatchIndex);
        }

    }
}
