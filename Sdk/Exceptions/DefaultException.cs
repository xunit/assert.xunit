namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when an object reference is unexpectedly not default.
    /// </summary>
#if XUNIT_VISIBILITY_INTERNAL
    internal
#else
    public
#endif
    class DefaultException : AssertActualExpectedException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DefaultException"/> class.
        /// </summary>
        /// <param name="actual"></param>
        public DefaultException(object actual)
            : base(null, actual, "Assert.Default() Failure")
        { }
    }
}