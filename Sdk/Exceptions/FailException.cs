using System;
using System.Collections.Generic;
using System.Text;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when a test is explicitly failed.
    /// </summary>
#if XUNIT_VISIBILITY_INTERNAL
    internal
#else
    public
#endif
    class FailException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="FailException"/> class.
        /// </summary>
        public FailException()
            :this("Assert.Fail() Failure")
        { }

        /// <summary>
        /// Creates a new instance of the <see cref="FailException"/> class.
        /// </summary>
        /// <param name="userMessage">The message to show as reason for failure.</param>
        public FailException(string userMessage)
            : base(userMessage)
        { }
    }
}
