namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Workflow.ComponentModel.Compiler;

    [Serializable]
    public sealed class DependencyProperty : ISerializable
    {
        private PropertyMetadata defaultMetadata;
        private static IDictionary<int, DependencyProperty> dependencyProperties = new Dictionary<int, DependencyProperty>();
        [NonSerialized]
        private bool isEvent;
        private bool isRegistered;
        private byte knownIndex;
        private static KnownDependencyProperty[] knownProperties = new KnownDependencyProperty[0x100];
        private string name = string.Empty;
        private Type ownerType;
        private Type propertyType;
        private Type validatorType;

        private DependencyProperty(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata, Type validatorType, bool isRegistered)
        {
            this.name = name;
            this.propertyType = propertyType;
            this.ownerType = ownerType;
            this.validatorType = validatorType;
            this.isRegistered = isRegistered;
            this.defaultMetadata = defaultMetadata;
            this.defaultMetadata.Seal(this, propertyType);
            this.isEvent = typeof(Delegate).IsAssignableFrom(this.propertyType) && ((this.defaultMetadata == null) || (((byte) (this.defaultMetadata.Options & DependencyPropertyOptions.DelegateProperty)) == 0));
        }

        internal static DependencyProperty FromKnown(byte byteVal)
        {
            if (knownProperties[byteVal] == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_NotRegisteredAs", new object[] { knownProperties[byteVal].dependencyProperty.ToString() }));
            }
            return knownProperties[byteVal].dependencyProperty;
        }

        public static DependencyProperty FromName(string propertyName, Type ownerType)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }
            if (ownerType == null)
            {
                throw new ArgumentNullException("ownerType");
            }
            DependencyProperty property = null;
            while ((property == null) && (ownerType != null))
            {
                RuntimeHelpers.RunClassConstructor(ownerType.TypeHandle);
                int key = propertyName.GetHashCode() ^ ownerType.GetHashCode();
                lock (((ICollection) dependencyProperties).SyncRoot)
                {
                    if (dependencyProperties.ContainsKey(key))
                    {
                        property = dependencyProperties[key];
                    }
                }
                ownerType = ownerType.BaseType;
            }
            return property;
        }

        public static IList<DependencyProperty> FromType(Type ownerType)
        {
            if (ownerType == null)
            {
                throw new ArgumentNullException("ownerType");
            }
            RuntimeHelpers.RunClassConstructor(ownerType.TypeHandle);
            List<DependencyProperty> list = new List<DependencyProperty>();
            lock (((ICollection) dependencyProperties).SyncRoot)
            {
                foreach (DependencyProperty property in dependencyProperties.Values)
                {
                    if (TypeProvider.IsSubclassOf(ownerType, property.ownerType) || (ownerType == property.ownerType))
                    {
                        list.Add(property);
                    }
                }
            }
            return list.AsReadOnly();
        }

        private static object GetDefaultValue(string name, Type propertyType, Type ownerType)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(SR.GetString("Error_EmptyArgument"), "name");
            }
            if (propertyType == null)
            {
                throw new ArgumentNullException("propertyType");
            }
            if (ownerType == null)
            {
                throw new ArgumentNullException("ownerType");
            }
            object obj2 = null;
            if (propertyType.IsValueType)
            {
                try
                {
                    if (propertyType.IsEnum)
                    {
                        Array values = Enum.GetValues(propertyType);
                        if (values.Length > 0)
                        {
                            return values.GetValue(0);
                        }
                        return Activator.CreateInstance(propertyType);
                    }
                    obj2 = Activator.CreateInstance(propertyType);
                }
                catch
                {
                }
            }
            return obj2;
        }

        public override int GetHashCode()
        {
            return (this.name.GetHashCode() ^ this.ownerType.GetHashCode());
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static DependencyProperty Register(string name, Type propertyType, Type ownerType)
        {
            return ValidateAndRegister(name, propertyType, ownerType, null, null, true);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static DependencyProperty Register(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata)
        {
            return ValidateAndRegister(name, propertyType, ownerType, defaultMetadata, null, true);
        }

        internal static void RegisterAsKnown(DependencyProperty dependencyProperty, byte byteVal, PropertyValidity propertyValidity)
        {
            if (dependencyProperty == null)
            {
                throw new ArgumentNullException("dependencyProperty");
            }
            if (knownProperties[byteVal] != null)
            {
                throw new InvalidOperationException(SR.GetString("Error_AlreadyRegisteredAs", new object[] { knownProperties[byteVal].dependencyProperty.ToString() }));
            }
            dependencyProperty.KnownIndex = byteVal;
            knownProperties[byteVal] = new KnownDependencyProperty(dependencyProperty, propertyValidity);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static DependencyProperty RegisterAttached(string name, Type propertyType, Type ownerType)
        {
            return ValidateAndRegister(name, propertyType, ownerType, null, null, false);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static DependencyProperty RegisterAttached(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata)
        {
            return ValidateAndRegister(name, propertyType, ownerType, defaultMetadata, null, false);
        }

        public static DependencyProperty RegisterAttached(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata, Type validatorType)
        {
            if (validatorType == null)
            {
                throw new ArgumentNullException("validatorType");
            }
            if (!typeof(Validator).IsAssignableFrom(validatorType))
            {
                throw new ArgumentException(SR.GetString("Error_ValidatorTypeIsInvalid"), "validatorType");
            }
            return ValidateAndRegister(name, propertyType, ownerType, defaultMetadata, validatorType, false);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("type", this.ownerType);
            info.AddValue("name", this.name);
            info.SetType(typeof(DependencyPropertyReference));
        }

        public override string ToString()
        {
            return this.name;
        }

        private static DependencyProperty ValidateAndRegister(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata, Type validatorType, bool isRegistered)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(SR.GetString("Error_EmptyArgument"), "name");
            }
            if (propertyType == null)
            {
                throw new ArgumentNullException("propertyType");
            }
            if (ownerType == null)
            {
                throw new ArgumentNullException("ownerType");
            }
            FieldInfo field = null;
            bool flag = typeof(Delegate).IsAssignableFrom(propertyType) && ((defaultMetadata == null) || (((byte) (defaultMetadata.Options & DependencyPropertyOptions.DelegateProperty)) == 0));
            if ((flag && (defaultMetadata != null)) && defaultMetadata.IsMetaProperty)
            {
                throw new ArgumentException(SR.GetString("Error_DPAddHandlerMetaProperty"), "defaultMetadata");
            }
            if (flag)
            {
                field = ownerType.GetField(name + "Event", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            }
            else
            {
                field = ownerType.GetField(name + "Property", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            }
            if (field == null)
            {
                throw new ArgumentException(SR.GetString(flag ? "Error_DynamicEventNotSupported" : "Error_DynamicPropertyNotSupported", new object[] { ownerType.FullName, name }), "ownerType");
            }
            PropertyMetadata metadata = null;
            if (defaultMetadata == null)
            {
                metadata = new PropertyMetadata(GetDefaultValue(name, propertyType, ownerType));
            }
            else
            {
                metadata = defaultMetadata;
                if (metadata.DefaultValue == null)
                {
                    metadata.DefaultValue = GetDefaultValue(name, propertyType, ownerType);
                }
            }
            DependencyProperty property = new DependencyProperty(name, propertyType, ownerType, metadata, validatorType, isRegistered);
            lock (((ICollection) dependencyProperties).SyncRoot)
            {
                if (dependencyProperties.ContainsKey(property.GetHashCode()))
                {
                    throw new InvalidOperationException(SR.GetString("Error_DPAlreadyExist", new object[] { name, ownerType.FullName }));
                }
                dependencyProperties.Add(property.GetHashCode(), property);
            }
            return property;
        }

        public PropertyMetadata DefaultMetadata
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.defaultMetadata;
            }
        }

        public bool IsAttached
        {
            get
            {
                return !this.isRegistered;
            }
        }

        public bool IsEvent
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isEvent;
            }
        }

        internal bool IsKnown
        {
            get
            {
                return (this.knownIndex != 0);
            }
        }

        internal byte KnownIndex
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.knownIndex;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.knownIndex = value;
            }
        }

        public string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
        }

        public Type OwnerType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.ownerType;
            }
        }

        public Type PropertyType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.propertyType;
            }
        }

        public Type ValidatorType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.validatorType;
            }
        }

        internal PropertyValidity Validity
        {
            get
            {
                if (!this.IsKnown)
                {
                    return PropertyValidity.Always;
                }
                return knownProperties[this.knownIndex].propertyValidity;
            }
        }

        [Serializable]
        private sealed class DependencyPropertyReference : IObjectReference
        {
            private string name;
            private Type type;

            public object GetRealObject(StreamingContext context)
            {
                return DependencyProperty.FromName(this.name, this.type);
            }
        }

        private class KnownDependencyProperty
        {
            internal DependencyProperty dependencyProperty;
            internal DependencyProperty.PropertyValidity propertyValidity;

            internal KnownDependencyProperty(DependencyProperty dependencyProperty, DependencyProperty.PropertyValidity propertyValidity)
            {
                this.dependencyProperty = dependencyProperty;
                this.propertyValidity = propertyValidity;
            }
        }

        internal enum PropertyValidity
        {
            Uninitialize,
            Reexecute,
            Always
        }
    }
}

