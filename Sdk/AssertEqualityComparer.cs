using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="IEqualityComparer{T}"/> used by the xUnit.net equality assertions.
    /// </summary>
    /// <typeparam name="T">The type that is being compared.</typeparam>
    class AssertEqualityComparer<T> : IEqualityComparer<T>
    {
        static readonly IEqualityComparer DefaultInnerComparer = new AssertEqualityComparerAdapter<object>(new AssertEqualityComparer<object>());

        readonly Func<IEqualityComparer> innerComparerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertEqualityComparer{T}" /> class.
        /// </summary>
        /// <param name="innerComparer">The inner comparer to be used when the compared objects are enumerable.</param>
        public AssertEqualityComparer(IEqualityComparer innerComparer = null)
        {
            // Use a thunk to delay evaluation of DefaultInnerComparer
            innerComparerFactory = () => innerComparer ?? DefaultInnerComparer;
        }

        /// <inheritdoc/>
        public bool Equals(T x, T y)
        {
            // Null?
            if (typeof(T).IsReferenceTypeOrNullable())
            {
                if (x == null)
                    return y == null;

                if (y == null)
                    return false;
            }

            // IMPORTANT: The user's notion of equality takes priority over xUnit's notion of equality.
            // All methods the user can use to signal that (s)he thinks two objects are/aren't equal, e.g.
            // by implementing equatable/comparable interfaces, or overriding object.Equals, should be
            // picked up on first before comparing the objects in ways the user can't control.
            // For example, if x and y are two collections with contents [1, 2] and [3, 4] but the user
            // has overridden Equals on them to say that they are equal, then we will agree with the user
            // although the collections have different contents.

            // Implements IEquatable<T>?
            var equatable = x as IEquatable<T>;
            if (equatable != null)
                return equatable.Equals(y);

            // Implements IComparable<T>?
            var comparableGeneric = x as IComparable<T>;
            if (comparableGeneric != null)
            {
                try
                {
                    return comparableGeneric.CompareTo(y) == 0;
                }
                catch
                {
                    // Some implementations of IComparable<T>.CompareTo throw exceptions in
                    // certain situations, such as if x can't compare against y.
                    // If this happens, just swallow up the exception and continue comparing.
                }
            }

            // Implements IComparable?
            var comparable = x as IComparable;
            if (comparable != null)
            {
                try
                {
                    return comparable.CompareTo(y) == 0;
                }
                catch
                {
                    // Some implementations of IComparable.CompareTo throw exceptions in
                    // certain situations, such as if x can't compare against y.
                    // If this happens, just swallow up the exception and continue comparing.
                }
            }

            // object.Equals is either supplied by the user or not overridden.
            // - If it returns true, either Equals wasn't overridden and the objects are reference-equal
            //   (in which case they'll be equal by all other standards) or the user says they are equal
            //   (in which case we will agree with the user).
            // - If it returns false, it's indecisive because there's a broad category of potential reasons
            //   why the other object wasn't considered equal, and there's a chance it could compare as
            //   equal by some other criteria.
            //   - For example, two arrays (which don't override Equals) can be in different memory locations but store the same contents.
            //   - Even if the user has overridden Equals and returns false, (s)he may not have taken all possible criteria into account.
            //     For example, new ArraySegment<T>(Array.Empty<T>()).Equals(new List<T>()) is false because the types do not match up, but
            //     the contents of the collections are equal.
            if (object.Equals(x, y))
            {
                return true;
            }
            
            // Implements IStructuralEquatable?
            var structuralEquatable = x as IStructuralEquatable;
            if (structuralEquatable != null && structuralEquatable.Equals(y, new TypeErasedEqualityComparer(innerComparerFactory())))
                return true;

            // Implements IEquatable<typeof(y)>?
            Type iequatableY = y.GetType().MakeEquatableType();
            if (iequatableY.IsAssignableFrom(x.GetType()))
            {
                MethodInfo equalsMethod = iequatableY.GetDeclaredMethod(nameof(IEquatable<T>.Equals));
                return equalsMethod.Invoke<bool>(x, y);
            }

            // Implements IComparable<typeof(y)>?
            Type icomparableY = y.GetType().MakeComparableType();
            if (icomparableY.IsAssignableFrom(x.GetType()))
            {
                MethodInfo compareToMethod = icomparableY.GetDeclaredMethod(nameof(IComparable<T>.CompareTo));
                try
                {
                    return compareToMethod.Invoke<int>(x, y) == 0;
                }
                catch
                {
                    // Some implementations of IComparable.CompareTo throw exceptions in
                    // certain situations, such as if x can't compare against y.
                    // If this happens, just swallow up the exception and continue comparing.
                }
            }

            // Dictionaries?
            var dictionariesEqual = CheckIfDictionariesAreEqual(x, y);
            if (dictionariesEqual.HasValue)
                return dictionariesEqual.GetValueOrDefault();

            // Sets?
            var setsEqual = CheckIfSetsAreEqual(x, y);
            if (setsEqual.HasValue)
                return setsEqual.GetValueOrDefault();

            // Enumerable?
            var enumerablesEqual = CheckIfEnumerablesAreEqual(x, y);
            if (enumerablesEqual.HasValue)
            {
                if (!enumerablesEqual.GetValueOrDefault())
                {
                    return false;
                }

                // Array.GetEnumerator() flattens out the array, ignoring array ranks and lengths.
                // Arrays with different shapes could have been compared as equal when they aren't.
                return CheckIfArrayShapesAreEqual(x, y) ?? true;
            }
            
            return false;
        }

        bool? CheckIfArrayShapesAreEqual(T x, T y)
        {
            Array xArray = x as Array;
            Array yArray = y as Array;
            if (xArray != null && yArray != null)
            {
                // new object[2,1] != new object[2]
                if (xArray.Rank != yArray.Rank)
                {
                    return false;
                }

                // new object[2,1] != new object[1,2]
                for (int i = 0; i < xArray.Rank; i++)
                {
                    if (xArray.GetLength(i) != yArray.GetLength(i))
                    {
                        return false;
                    }
                }

                return true;
            }

            return null;
        }

        bool? CheckIfEnumerablesAreEqual(T x, T y)
        {
            var enumerableX = x as IEnumerable;
            var enumerableY = y as IEnumerable;

            if (enumerableX == null || enumerableY == null)
                return null;

            IEnumerator enumeratorX = null, enumeratorY = null;
            try
            {
                enumeratorX = enumerableX.GetEnumerator();
                enumeratorY = enumerableY.GetEnumerator();
                var equalityComparer = innerComparerFactory();

                while (true)
                {
                    var hasNextX = enumeratorX.MoveNext();
                    var hasNextY = enumeratorY.MoveNext();

                    if (!hasNextX || !hasNextY)
                        return hasNextX == hasNextY;

                    if (!equalityComparer.Equals(enumeratorX.Current, enumeratorY.Current))
                        return false;
                }
            }
            finally
            {
                var asDisposable = enumeratorX as IDisposable;
                if (asDisposable != null)
                    asDisposable.Dispose();
                asDisposable = enumeratorY as IDisposable;
                if (asDisposable != null)
                    asDisposable.Dispose();
            }
        }

        bool? CheckIfDictionariesAreEqual(T x, T y)
        {
            var dictionaryX = x as IDictionary;
            var dictionaryY = y as IDictionary;

            if (dictionaryX == null || dictionaryY == null)
                return null;

            if (dictionaryX.Count != dictionaryY.Count)
                return false;

            var equalityComparer = innerComparerFactory();
            var dictionaryYKeys = new HashSet<object>(dictionaryY.Keys.Cast<object>());

            foreach (var key in dictionaryX.Keys)
            {
                if (!dictionaryYKeys.Contains(key))
                    return false;

                var valueX = dictionaryX[key];
                var valueY = dictionaryY[key];

                if (!equalityComparer.Equals(valueX, valueY))
                    return false;

                dictionaryYKeys.Remove(key);
            }

            return dictionaryYKeys.Count == 0;
        }

        private static MethodInfo s_compareTypedSetsMethod;

        bool? CheckIfSetsAreEqual(T x, T y)
        {
            if (!typeof(T).IsSet())
                return null;

            var enumX = x as IEnumerable;
            var enumY = y as IEnumerable;
            if (enumX == null || enumY == null)
                return null;

            Type elementType;
            if (typeof(T).GenericTypeArguments.Length != 1)
                elementType = typeof(object);
            else
                elementType = typeof(T).GenericTypeArguments[0];

            if (s_compareTypedSetsMethod == null)
                s_compareTypedSetsMethod = GetType().GetTypeInfo().GetDeclaredMethod(nameof(CompareTypedSets));

            MethodInfo method = s_compareTypedSetsMethod.MakeGenericMethod(new Type[] { elementType });
            return method.Invoke<bool>(this, enumX, enumY);
        }

        bool CompareTypedSets<R>(IEnumerable enumX, IEnumerable enumY)
        {
            var setX = new HashSet<R>(enumX.Cast<R>());
            var setY = new HashSet<R>(enumY.Cast<R>());
            return setX.SetEquals(setY);
        }

        /// <inheritdoc/>
        [SuppressMessage("Code Notifications", "RECS0083:Shows NotImplementedException throws in the quick task bar", Justification = "This class is not intended to be used in a hased container")]
        public int GetHashCode(T obj)
        {
            throw new NotImplementedException();
        }

        private class TypeErasedEqualityComparer : IEqualityComparer
        {
            private readonly IEqualityComparer innerComparer;

            public TypeErasedEqualityComparer(IEqualityComparer innerComparer)
            {
                this.innerComparer = innerComparer;
            }

            private static MethodInfo s_equalsMethod;

            public new bool Equals(object x, object y)
            {
                if (x == null)
                    return y == null;
                if (y == null)
                    return false;

                // Delegate checking of whether two objects are equal to AssertEqualityComparer.
                // To get the best result out of AssertEqualityComparer, we attempt to specialize the
                // comparer for the objects that we are checking.
                // If the objects are the same, great! If not, assume they are objects.
                // This is more naive than the C# compiler which tries to see if they share any interfaces
                // etc. but that's likely overkill here as AssertEqualityComparer<object> is smart enough.
                Type objectType = x.GetType() == y.GetType() ? x.GetType() : typeof(object);

                // Lazily initialize and cache the EqualsGeneric<U> method.
                if (s_equalsMethod == null)
                    s_equalsMethod = typeof(TypeErasedEqualityComparer).GetTypeInfo().GetDeclaredMethod(nameof(EqualsGeneric));

                return s_equalsMethod.MakeGenericMethod(objectType).Invoke<bool>(this, x, y);
            }

            private bool EqualsGeneric<U>(U x, U y) => new AssertEqualityComparer<U>(innerComparer: innerComparer).Equals(x, y);

            [SuppressMessage("Code Notifications", "RECS0083:Shows NotImplementedException throws in the quick task bar", Justification = "This class is not intended to be used in a hased container")]
            public int GetHashCode(object obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}