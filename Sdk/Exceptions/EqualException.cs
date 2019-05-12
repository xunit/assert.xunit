using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
        const int MaxPrintLength = 100;

        static readonly Dictionary<char, string> Encodings = new Dictionary<char, string>
        {
            { '\r', "\\r" },
            { '\n', "\\n" },
            { '\t', "\\t" },
            { '\0', "\\0" }
        };

        static readonly Tuple<int, int>[] IndexRanges = {
            Tuple.Create(-2, 2),
            Tuple.Create(-1, 2),
            Tuple.Create(0, 2),
            Tuple.Create(0, 1)
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
        /// Creates a new instance of the <see cref="EqualException"/> class for enumerable comparisons.
        /// </summary>
        /// <param name="expected">The expected enumerable</param>
        /// <param name="actual">The actual enumerable</param>
        /// <param name="index">The first index where the enumerable values differ</param>
        public EqualException(IEnumerable expected, IEnumerable actual, int index)
            : base(expected, actual, "Assert.Equal() Failure")
        {
            message = CreateEnumerableMessage(expected, actual, index);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="EqualException"/> class for string comparisons.
        /// </summary>
        /// <param name="expected">The expected string value</param>
        /// <param name="actual">The actual string value</param>
        /// <param name="expectedIndex">The first index in the expected string where the strings differ</param>
        /// <param name="actualIndex">The first index in the actual string where the strings differ</param>
        public EqualException(string expected, string actual, int expectedIndex, int actualIndex)
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

            Tuple<string, string> printedExpected = ShortenAndEncode(Expected, ExpectedIndex, '↓');
            Tuple<string, string> printedActual = ShortenAndEncode(Actual, ActualIndex, '↑');

            return string.Format(
                CultureInfo.CurrentCulture,
                "{1}{0}          {2}{0}Expected: {3}{0}Actual:   {4}{0}          {5}",
                Environment.NewLine,
                UserMessage,
                printedExpected.Item2,
                printedExpected.Item1 ?? "(null)",
                printedActual.Item1 ?? "(null)",
                printedActual.Item2
            );
        }

        string CreateEnumerableMessage(IEnumerable expected, IEnumerable actual, int index)
        {
            var expectedValues = expected.Cast<object>().ToList();
            var actualValues = actual.Cast<object>().ToList();

            Tuple<string, int> printedExpected = default;
            Tuple<string, int> printedActual = default;

            CreatePrintedEnumerables(index, expectedValues, actualValues, ref printedExpected, ref printedActual);

            return string.Format(
                CultureInfo.CurrentCulture,
                "{1}{0}          {2}{0}Expected: {3}{0}Actual:   {4}{0}          {5}",
                Environment.NewLine,
                UserMessage,
                CreatePrintedPointer(printedExpected, index, "↓"),
                printedExpected.Item1 ?? "(null)",
                printedActual.Item1 ?? "(null)",
                CreatePrintedPointer(printedActual, index, "↑")
            );
        }

        static void CreatePrintedEnumerables(int index, List<object> expectedValues, List<object> actualValues,
            ref Tuple<string, int> printedExpected, ref Tuple<string, int> printedActual)
        {
            foreach (var indexRange in IndexRanges)
            {
                printedExpected = FormatEnumerable(expectedValues, index, indexRange.Item1, indexRange.Item2, false);
                printedActual = FormatEnumerable(actualValues, index, indexRange.Item1, indexRange.Item2, false);
                if (printedExpected.Item1.Length <= MaxPrintLength && printedActual.Item1.Length <= MaxPrintLength) break;
            }

            if (printedExpected.Item1.Length > MaxPrintLength || printedActual.Item1.Length > MaxPrintLength)
            {
                printedExpected = FormatEnumerable(expectedValues, index, 0, 0, true);
                printedActual = FormatEnumerable(actualValues, index, 0, 0, true);
            }
        }

        static string CreatePrintedPointer(Tuple<string, int> valueWithIndex, int index, string pointer)
        {
            var printedPointerValues = Enumerable.Repeat(" ", valueWithIndex.Item2 + 1).ToList();
            printedPointerValues[valueWithIndex.Item2] = pointer;
            return string.Concat(string.Join("", printedPointerValues), $" (pos {index})");
        }

        static Tuple<string, string> ShortenAndEncode(string value, int position, char pointer)
        {
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
                    printedPointer.AppendFormat("{0} (pos {1})", pointer, position);
            }

            if (value.Length == position)
                printedPointer.AppendFormat("{0} (pos {1})", pointer, position);

            if (end < value.Length)
                printedValue.Append("···");

            return new Tuple<string, string>(printedValue.ToString(), printedPointer.ToString());
        }


        static string ConvertToString(object value)
        {
            if (value is string stringValue)
                return stringValue;

            return ArgumentFormatter.Format(value);
        }

        static Tuple<string, int> FormatEnumerable(IEnumerable<object> enumerableValues, int diffIndex, int rangeMinIndex, int rangeMaxIndex, bool trimToMaxLength)
        {
            var enumerable = enumerableValues.ToList();
            var rangeLength = Math.Abs(rangeMinIndex) + Math.Abs(rangeMaxIndex) + 1;

            var firstIndex = diffIndex + rangeMinIndex >= 0 ? diffIndex + rangeMinIndex : 0;
            var lastIndex = firstIndex + rangeLength < enumerable.Count - 1 ? firstIndex + rangeLength : enumerable.Count - 1;
            var values = enumerable.GetRange(firstIndex, lastIndex - firstIndex + 1);

            var stringedValues = values.Select(ConvertToString).ToList();

            var indexDiffElement = 1;

            if (diffIndex >= 2 && rangeMinIndex == -2)
            {
                indexDiffElement = string.Join(", ", stringedValues.GetRange(0, 2)).Length + 3;
            }
            else if (diffIndex == 1 && rangeMinIndex == -1)
            {
                indexDiffElement = string.Join(", ", stringedValues.GetRange(0, 1)).Length + 3;
            }

            if (firstIndex > 0)
            {
                stringedValues.Insert(0, "...");
                indexDiffElement += 5;
            }

            if (lastIndex < enumerable.Count - 1)
            {
                stringedValues.Add("...");
            }

            var printedValues = string.Join(", ", stringedValues);

            if (trimToMaxLength && printedValues.Length >= MaxPrintLength)
            {
                printedValues = string.Concat(printedValues.Substring(0, MaxPrintLength - 3), "...");
            }
            return new Tuple<string, int>($"[{printedValues}]", indexDiffElement);
        }
    }
}