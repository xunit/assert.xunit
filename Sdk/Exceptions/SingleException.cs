namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when the collection did not contain exactly one element.
    /// </summary>
#if XUNIT_VISIBILITY_INTERNAL
    internal
#else
    public
#endif
    class SingleException : XunitException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleException"/> class.
        /// </summary>
        private SingleException(string errorMessage) : base(errorMessage) { }

        /// <summary>
        /// Creates an instance of <see cref="SingleException"/> for when the collection was empty.
        /// </summary>
        public static SingleException Empty() =>
            new SingleException("The collection was expected to contain a single element, but it was empty.");

        /// <summary>
        /// Creates an instance of <see cref="SingleException"/> for when the collection had too many items.
        /// </summary>
        /// <returns></returns>
        public static SingleException MoreThanOne() =>
            new SingleException("The collection was expected to contain a single element, but it contained more than one element.");
    }
}
