namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when an object reference is unexpectedly default.
    /// </summary>
#if XUNIT_VISIBILITY_INTERNAL
    internal
#else
    public
#endif
    class NotDefaultException : AssertActualExpectedException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="NotDefaultException"/> class.
        /// </summary>
        /// <param name="actual"></param>
        public NotDefaultException(object actual)
            : base(null, actual, "Assert.NotDefault() Failure")
        { }
    }
}