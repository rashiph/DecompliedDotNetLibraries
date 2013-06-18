namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Xml.Linq;

    internal static class InstancePersistence
    {
        private static readonly XNamespace activitiesCommandNamespace = XNamespace.Get("urn:schemas-microsoft-com:System.Activities.Persistence/command");
        private static readonly XNamespace activitiesEventNamespace = XNamespace.Get("urn:schemas-microsoft-com:System.Activities.Persistence/event");

        public static bool IsOptional(this InstanceValue value)
        {
            return ((value.Options & InstanceValueOptions.Optional) != InstanceValueOptions.None);
        }

        public static bool IsWriteOnly(this InstanceValue value)
        {
            return ((value.Options & InstanceValueOptions.WriteOnly) != InstanceValueOptions.None);
        }

        public static ReadOnlyDictionary<XName, InstanceValue> ReadOnlyCopy(this IDictionary<XName, InstanceValue> bag, bool allowWriteOnly)
        {
            if ((bag == null) || (bag.Count <= 0))
            {
                return null;
            }
            Dictionary<XName, InstanceValue> dictionary = new Dictionary<XName, InstanceValue>(bag.Count);
            foreach (KeyValuePair<XName, InstanceValue> pair in bag)
            {
                pair.ValidateProperty();
                if (!pair.Value.IsWriteOnly())
                {
                    dictionary.Add(pair.Key, pair.Value);
                }
                else if (!allowWriteOnly)
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.LoadedWriteOnlyValue));
                }
            }
            return new ReadOnlyDictionary<XName, InstanceValue>(dictionary, false);
        }

        public static ReadOnlyDictionary<XName, InstanceValue> ReadOnlyMergeInto(this IDictionary<XName, InstanceValue> bag, IDictionary<XName, InstanceValue> existing, bool allowWriteOnly)
        {
            if ((bag == null) || (bag.Count <= 0))
            {
                return (ReadOnlyDictionary<XName, InstanceValue>) existing;
            }
            Dictionary<XName, InstanceValue> dictionary = (existing == null) ? new Dictionary<XName, InstanceValue>(bag.Count) : new Dictionary<XName, InstanceValue>(existing);
            foreach (KeyValuePair<XName, InstanceValue> pair in bag)
            {
                pair.ValidateProperty(true);
                if (pair.Value.IsDeletedValue)
                {
                    dictionary.Remove(pair.Key);
                }
                else if (!pair.Value.IsWriteOnly())
                {
                    dictionary[pair.Key] = pair.Value;
                }
                else
                {
                    if (!allowWriteOnly)
                    {
                        throw Fx.Exception.AsError(new InvalidOperationException(SRCore.LoadedWriteOnlyValue));
                    }
                    dictionary.Remove(pair.Key);
                }
            }
            return new ReadOnlyDictionary<XName, InstanceValue>(dictionary, false);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void ValidateProperty(this KeyValuePair<XName, InstanceValue> property)
        {
            property.ValidateProperty(false);
        }

        public static void ValidateProperty(this KeyValuePair<XName, InstanceValue> property, bool allowDelete)
        {
            if (property.Key == null)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.MetadataCannotContainNullKey));
            }
            if (property.Value == null)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.MetadataCannotContainNullValue(property.Key)));
            }
            if (!allowDelete && property.Value.IsDeletedValue)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.InitialMetadataCannotBeDeleted(property.Key)));
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void ValidatePropertyBag(this IDictionary<XName, InstanceValue> bag)
        {
            bag.ValidatePropertyBag(false);
        }

        public static void ValidatePropertyBag(this IDictionary<XName, InstanceValue> bag, bool allowDelete)
        {
            if (bag != null)
            {
                foreach (KeyValuePair<XName, InstanceValue> pair in bag)
                {
                    pair.ValidateProperty(allowDelete);
                }
            }
        }

        internal static XNamespace ActivitiesCommandNamespace
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return activitiesCommandNamespace;
            }
        }

        internal static XNamespace ActivitiesEventNamespace
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return activitiesEventNamespace;
            }
        }
    }
}

