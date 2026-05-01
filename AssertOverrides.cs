#pragma warning disable CA1052 // Static holder types should be static

#if XUNIT_NULLABLE
#nullable enable
#endif

using Xunit.Sdk;

namespace Xunit
{
	partial class Assert
	{
		/// <summary>
		/// Overrides the maximum number of items to show for enumerables in assertion failure messages
		/// (for the current test).
		/// </summary>
		/// <param name="maxLength">Pass <c>0</c> to disable truncation; pass <see langword="null"/>
		/// to revert to the global setting</param>
		public static void OverrideMaxEnumerableLength(int? maxLength)
			=> ArgumentFormatter.OverrideMaxEnumerableLength(maxLength);

		/// <summary>
		/// Overrides the maximum depth to show for complex objects in assertion failure messages
		/// (for the current test).
		/// </summary>
		/// <param name="maxDepth">Pass <c>0</c> to disable truncation; pass <see langword="null"/>
		/// to revert to the global setting</param>
		/// <remarks>
		/// In Native AOT mode, complex objects are not printed; setting this value will have no effect on the resulting assertion message.
		/// </remarks>
		public static void OverrideMaxObjectDepth(int? maxDepth)
			=> ArgumentFormatter.OverrideMaxObjectDepth(maxDepth);

		/// <summary>
		/// Overrides the maximum number of members to show for complex objects in assertion failure messages
		/// (for current test).
		/// </summary>
		/// <param name="maxCount">Pass <c>0</c> to disable truncation; pass <see langword="null"/>
		/// to revert to the global setting</param>
		/// <remarks>
		/// In Native AOT mode, complex objects are not printed; setting this value will have no effect on the resulting assertion message.
		/// </remarks>
		public static void OverrideMaxObjectMemberCount(int? maxCount)
			=> ArgumentFormatter.OverrideMaxObjectMemberCount(maxCount);

		/// <summary>
		/// Overrides the maximum string length used in assertion failure messages
		/// (for current test).
		/// </summary>
		/// <param name="maxLength">Pass <c>0</c> to disable truncation; pass <see langword="null"/>
		/// to revert to the global setting</param>
		public static void OverrideMaxStringLength(int? maxLength)
			=> ArgumentFormatter.OverrideMaxStringLength(maxLength);
	}
}
