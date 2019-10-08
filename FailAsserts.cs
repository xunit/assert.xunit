using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Sdk;

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
        /// Explicitly fail a test.
        /// </summary>
        public static void Fail()
        {
            throw new FailException();
        }

        /// <summary>
        /// Explicitly fail a test.
        /// </summary>
        /// <param name="userMessage">The message to show as reason for failure.</param>
        public static void Fail(string userMessage)
        {
            throw new FailException(userMessage);
        }
    }
}
