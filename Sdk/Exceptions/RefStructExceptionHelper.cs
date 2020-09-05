using System;
using System.Collections.Generic;
using System.Text;

namespace Xunit.Sdk
{
	/// <summary>
	/// Class containing helper methods for creating ref struct specific exceptions (e.g. Span or ReadOnlySpan types)
	/// As these ref struct types do not support boxing to object, and thus cannot use the common constructors
	/// </summary>
	public static class RefStructExceptionHelper
	{
		/// <summary>
		/// used to invoke constructors that need 2 parameters of type object. Object is not boxable by ReadOnlySpan.
		/// This method simply converts the ReadOnlySpans to arrays of the native type, and then invokes the function (presumably a constructor)
		/// </summary>
		/// <typeparam name="T">The Type of object to return (generally the exception type</typeparam>
		/// <typeparam name="U">The type contained within the ReadOnlySpan (or Span due to implicit conversation operator)</typeparam>
		/// <param name="expected">The Expected Value</param>
		/// <param name="actual">The acutal Value</param>
		/// <param name="func">the function to invoke, this function generally will be a constructor</param>
		/// <returns></returns>
		public static T CreateException<T, U>(ReadOnlySpan<U> expected, ReadOnlySpan<U> actual, Func<object, object, T> func)
			=> func.Invoke(expected.ToArray(), actual.ToArray());
	}
}
