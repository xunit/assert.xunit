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
        /// Verifies that an object is the default value for its type.
        /// </summary>
        /// <param name="t">The object to be inspected</param>
        /// <typeparam name="T">The type of the object to be inspected</typeparam>
        /// <exception cref="DefaultException">Thrown when the object is not the default value</exception>
        public static void Default<T>(T t)
        {
            if (t != default(T))
                throw new DefaultException(t);
        }

        /// <summary>
        /// Verifies that an object is not the default value for its type.
        /// </summary>
        /// <param name="t">The object to be inspected</param>
        /// <typeparam name="T">The type of the object to be inspected</typeparam>
        /// <exception cref="NotDefaultException">Thrown when the object is the default value</exception>
        public static void NotDefault<T>(T t)
        {
            if (t == default(T))
                throw new NotDefaultException(t);
        }
    }
}
