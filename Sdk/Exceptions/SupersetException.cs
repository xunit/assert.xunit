using System.Collections;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when a set is not a superset of another set.
    /// </summary>
#if XUNIT_VISIBILITY_INTERNAL 
    internal
#else
    public
#endif
    class SupersetException : AssertActualExpectedException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="SupersetException"/> class.
        /// </summary>
        public SupersetException(IEnumerable expected, IEnumerable actual)
            : base(expected, actual, "Assert.Superset() Failure")
        { }
    }
}