#if XUNIT_AOT

using System;
using System.Collections;
using System.ComponentModel;

namespace Xunit
{
	/// <summary>
	/// An implementation of <see cref="IEqualityComparer"/> that uses the same logic
	/// from <see cref="Assert.Equivalent"/>.
	/// </summary>
	[Obsolete("Assert.Equivalent is not available in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
#if XUNIT_VISIBILITY_INTERNAL
	internal
#else
	public
#endif
	class AssertEquivalenceComparer
	{ }
}

#endif  // XUNIT_AOT
