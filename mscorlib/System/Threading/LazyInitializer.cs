namespace System.Threading
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public static class LazyInitializer
    {
        private static volatile object s_barrier;

        public static T EnsureInitialized<T>(ref T target) where T: class
        {
            if (((T) target) != null)
            {
                volatile object local1 = s_barrier;
                return target;
            }
            return EnsureInitializedCore<T>(ref target, LazyHelpers<T>.s_activatorFactorySelector);
        }

        public static T EnsureInitialized<T>(ref T target, Func<T> valueFactory) where T: class
        {
            if (((T) target) != null)
            {
                volatile object local1 = s_barrier;
                return target;
            }
            return EnsureInitializedCore<T>(ref target, valueFactory);
        }

        public static T EnsureInitialized<T>(ref T target, ref bool initialized, ref object syncLock)
        {
            if (initialized)
            {
                volatile object local1 = s_barrier;
                return target;
            }
            return EnsureInitializedCore<T>(ref target, ref initialized, ref syncLock, LazyHelpers<T>.s_activatorFactorySelector);
        }

        public static T EnsureInitialized<T>(ref T target, ref bool initialized, ref object syncLock, Func<T> valueFactory)
        {
            if (initialized)
            {
                volatile object local1 = s_barrier;
                return target;
            }
            return EnsureInitializedCore<T>(ref target, ref initialized, ref syncLock, valueFactory);
        }

        private static T EnsureInitializedCore<T>(ref T target, Func<T> valueFactory) where T: class
        {
            T local = valueFactory();
            if (local == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Lazy_StaticInit_InvalidOperation"));
            }
            Interlocked.CompareExchange<T>(ref target, local, default(T));
            return target;
        }

        private static T EnsureInitializedCore<T>(ref T target, ref bool initialized, ref object syncLock, Func<T> valueFactory)
        {
            object obj2 = syncLock;
            if (obj2 == null)
            {
                object obj3 = new object();
                obj2 = Interlocked.CompareExchange(ref syncLock, obj3, null);
                if (obj2 == null)
                {
                    obj2 = obj3;
                }
            }
            lock (obj2)
            {
                if (!initialized)
                {
                    target = valueFactory();
                    initialized = true;
                }
            }
            return target;
        }

        private static class LazyHelpers<T>
        {
            internal static Func<T> s_activatorFactorySelector;

            static LazyHelpers()
            {
                LazyInitializer.LazyHelpers<T>.s_activatorFactorySelector = new Func<T>(LazyInitializer.LazyHelpers<T>.ActivatorFactorySelector);
            }

            private static T ActivatorFactorySelector()
            {
                T local;
                try
                {
                    local = (T) Activator.CreateInstance(typeof(T));
                }
                catch (MissingMethodException)
                {
                    throw new MissingMemberException(Environment.GetResourceString("Lazy_CreateValue_NoParameterlessCtorForT"));
                }
                return local;
            }
        }
    }
}

