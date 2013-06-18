namespace System.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public static class AttachablePropertyServices
    {
        private static DefaultAttachedPropertyStore attachedProperties = new DefaultAttachedPropertyStore();

        public static void CopyPropertiesTo(object instance, KeyValuePair<AttachableMemberIdentifier, object>[] array, int index)
        {
            if (instance != null)
            {
                IAttachedPropertyStore store = instance as IAttachedPropertyStore;
                if (store != null)
                {
                    store.CopyPropertiesTo(array, index);
                }
                else
                {
                    attachedProperties.CopyPropertiesTo(instance, array, index);
                }
            }
        }

        public static int GetAttachedPropertyCount(object instance)
        {
            if (instance == null)
            {
                return 0;
            }
            IAttachedPropertyStore store = instance as IAttachedPropertyStore;
            if (store != null)
            {
                return store.PropertyCount;
            }
            return attachedProperties.GetPropertyCount(instance);
        }

        public static bool RemoveProperty(object instance, AttachableMemberIdentifier name)
        {
            if (instance == null)
            {
                return false;
            }
            IAttachedPropertyStore store = instance as IAttachedPropertyStore;
            if (store != null)
            {
                return store.RemoveProperty(name);
            }
            return attachedProperties.RemoveProperty(instance, name);
        }

        public static void SetProperty(object instance, AttachableMemberIdentifier name, object value)
        {
            if (instance != null)
            {
                if (name == null)
                {
                    throw new ArgumentNullException("name");
                }
                IAttachedPropertyStore store = instance as IAttachedPropertyStore;
                if (store != null)
                {
                    store.SetProperty(name, value);
                }
                else
                {
                    attachedProperties.SetProperty(instance, name, value);
                }
            }
        }

        public static bool TryGetProperty(object instance, AttachableMemberIdentifier name, out object value)
        {
            return TryGetProperty<object>(instance, name, out value);
        }

        public static bool TryGetProperty<T>(object instance, AttachableMemberIdentifier name, out T value)
        {
            object obj2;
            if (instance == null)
            {
                value = default(T);
                return false;
            }
            IAttachedPropertyStore store = instance as IAttachedPropertyStore;
            if (store == null)
            {
                return attachedProperties.TryGetProperty<T>(instance, name, out value);
            }
            if (store.TryGetProperty(name, out obj2) && (obj2 is T))
            {
                value = (T) obj2;
                return true;
            }
            value = default(T);
            return false;
        }

        private sealed class DefaultAttachedPropertyStore
        {
            private Lazy<ConditionalWeakTable<object, Dictionary<AttachableMemberIdentifier, object>>> instanceStorage = new Lazy<ConditionalWeakTable<object, Dictionary<AttachableMemberIdentifier, object>>>();

            public void CopyPropertiesTo(object instance, KeyValuePair<AttachableMemberIdentifier, object>[] array, int index)
            {
                Dictionary<AttachableMemberIdentifier, object> dictionary;
                if (this.instanceStorage.IsValueCreated && this.instanceStorage.Value.TryGetValue(instance, out dictionary))
                {
                    lock (dictionary)
                    {
                        dictionary.CopyTo(array, index);
                    }
                }
            }

            public int GetPropertyCount(object instance)
            {
                Dictionary<AttachableMemberIdentifier, object> dictionary;
                if (this.instanceStorage.IsValueCreated && this.instanceStorage.Value.TryGetValue(instance, out dictionary))
                {
                    lock (dictionary)
                    {
                        return dictionary.Count;
                    }
                }
                return 0;
            }

            public bool RemoveProperty(object instance, AttachableMemberIdentifier name)
            {
                Dictionary<AttachableMemberIdentifier, object> dictionary;
                if (this.instanceStorage.IsValueCreated && this.instanceStorage.Value.TryGetValue(instance, out dictionary))
                {
                    lock (dictionary)
                    {
                        return dictionary.Remove(name);
                    }
                }
                return false;
            }

            public void SetProperty(object instance, AttachableMemberIdentifier name, object value)
            {
                Dictionary<AttachableMemberIdentifier, object> dictionary;
                if (!this.instanceStorage.Value.TryGetValue(instance, out dictionary))
                {
                    dictionary = new Dictionary<AttachableMemberIdentifier, object>();
                    try
                    {
                        this.instanceStorage.Value.Add(instance, dictionary);
                    }
                    catch (ArgumentException)
                    {
                        if (!this.instanceStorage.Value.TryGetValue(this.instanceStorage, out dictionary))
                        {
                            throw new InvalidOperationException(System.Xaml.SR.Get("DefaultAttachablePropertyStoreCannotAddInstance"));
                        }
                    }
                }
                lock (dictionary)
                {
                    dictionary[name] = value;
                }
            }

            public bool TryGetProperty<T>(object instance, AttachableMemberIdentifier name, out T value)
            {
                Dictionary<AttachableMemberIdentifier, object> dictionary;
                if (this.instanceStorage.IsValueCreated && this.instanceStorage.Value.TryGetValue(instance, out dictionary))
                {
                    lock (dictionary)
                    {
                        object obj2;
                        if (dictionary.TryGetValue(name, out obj2) && (obj2 is T))
                        {
                            value = (T) obj2;
                            return true;
                        }
                    }
                }
                value = default(T);
                return false;
            }
        }
    }
}

