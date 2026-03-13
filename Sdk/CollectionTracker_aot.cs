#if XUNIT_AOT

#pragma warning disable IDE0060 // Parameters here are matched between reflection and AOT

#if XUNIT_NULLABLE
#nullable enable
#else
// In case this is source-imported with global nullable enabled but no XUNIT_NULLABLE
#pragma warning disable CS8603
#pragma warning disable CS8619
#endif

using System;
using System.Collections;
using System.Reflection;

namespace Xunit.Sdk
{
	partial class CollectionTracker
	{
#if XUNIT_NULLABLE
		static AssertEqualityResult? CheckIfSetsAreEqual(
			CollectionTracker? x,
			CollectionTracker? y,
			IEqualityComparer? itemComparer) =>
				null;
#else
		static AssertEqualityResult CheckIfSetsAreEqual(
			CollectionTracker x,
			CollectionTracker y,
			IEqualityComparer itemComparer) =>
				null;
#endif

#if XUNIT_NULLABLE
		static (Type?, MethodInfo?) GetAssertEqualityComparerMetadata(IEqualityComparer itemComparer) =>
#else
		static (Type, MethodInfo) GetAssertEqualityComparerMetadata(IEqualityComparer itemComparer) =>
#endif
			(null, null);
	}
}

#endif  // XUNIT_AOT
