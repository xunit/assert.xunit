using System;
using System.Collections;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when a collection is unexpectedly not empty.
    /// </summary>
#if XUNIT_VISIBILITY_INTERNAL 
    internal
#else
    public
#endif
    class EmptyException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="EmptyException"/> class.
        /// </summary>
        public EmptyException(IEnumerable collection)
            : base("Assert.Empty() Failure")
        {
            Collection = collection;
        }

        /// <summary>
        /// The collection that failed the test.
        /// </summary>
        public IEnumerable Collection { get; }

        /// <inheritdoc/>
        public override string Message
        {
            get
            {
                return $"{base.Message}{Environment.NewLine}Collection: {ArgumentFormatter.Format(Collection)}";
            }
        }
    }
}