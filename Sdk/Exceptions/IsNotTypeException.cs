#if XUNIT_NULLABLE
#nullable enable
#endif

using System;

namespace Xunit.Sdk
{
	/// <summary>
	/// Exception thrown when the value is unexpectedly of the exact given type.
	/// </summary>
#if XUNIT_VISIBILITY_INTERNAL
	internal
#else
	public
#endif
	class IsNotTypeException : XunitException
	{
		IsNotTypeException(string message) :
			base(message)
		{ }

		/// <summary>
		/// Creates a new instance of the <see cref="IsNotTypeException"/> class to be thrown
		/// when the object is the exact type.
		/// </summary>
		/// <param name="type">The expected type</param>
		public static IsNotTypeException ForExactType(Type type)
		{
			var formattedType = ArgumentFormatter.Format(type);

			return new IsNotTypeException(
				"Assert.IsNotType() Failure: Value is the exact type" + Environment.NewLine +
				"Expected: " + formattedType + Environment.NewLine +
				"Actual:   " + formattedType
			);
		}
	}
}
