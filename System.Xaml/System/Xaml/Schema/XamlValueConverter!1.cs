namespace System.Xaml.Schema
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Xaml;

    public class XamlValueConverter<TConverterBase> : IEquatable<XamlValueConverter<TConverterBase>> where TConverterBase: class
    {
        private TConverterBase _instance;
        private volatile bool _instanceIsSet;
        private ThreeValuedBool _isPublic;

        public XamlValueConverter(Type converterType, XamlType targetType) : this(converterType, targetType, null)
        {
        }

        public XamlValueConverter(Type converterType, XamlType targetType, string name)
        {
            if (((converterType == null) && (targetType == null)) && (name == null))
            {
                throw new ArgumentException(System.Xaml.SR.Get("ArgumentRequired", new object[] { "converterType, targetType, name" }));
            }
            this.ConverterType = converterType;
            this.TargetType = targetType;
            this.Name = name ?? this.GetDefaultName();
        }

        protected virtual TConverterBase CreateInstance()
        {
            if (((this.ConverterType == typeof(EnumConverter)) && (this.TargetType.UnderlyingType != null)) && this.TargetType.UnderlyingType.IsEnum)
            {
                return (TConverterBase) new EnumConverter(this.TargetType.UnderlyingType);
            }
            if (this.ConverterType == null)
            {
                return default(TConverterBase);
            }
            if (!typeof(TConverterBase).IsAssignableFrom(this.ConverterType))
            {
                throw new XamlSchemaException(System.Xaml.SR.Get("ConverterMustDeriveFromBase", new object[] { this.ConverterType, typeof(TConverterBase) }));
            }
            return (TConverterBase) SafeReflectionInvoker.CreateInstance(this.ConverterType, null);
        }

        public override bool Equals(object obj)
        {
            XamlValueConverter<TConverterBase> objA = obj as XamlValueConverter<TConverterBase>;
            if (object.ReferenceEquals(objA, null))
            {
                return false;
            }
            return (this == objA);
        }

        public bool Equals(XamlValueConverter<TConverterBase> other)
        {
            return (this == other);
        }

        private string GetDefaultName()
        {
            if (this.ConverterType == null)
            {
                return this.TargetType.Name;
            }
            if (this.TargetType != null)
            {
                return (this.ConverterType.Name + "(" + this.TargetType.Name + ")");
            }
            return this.ConverterType.Name;
        }

        public override int GetHashCode()
        {
            int hashCode = this.Name.GetHashCode();
            if (this.ConverterType != null)
            {
                hashCode ^= this.ConverterType.GetHashCode();
            }
            if (this.TargetType != null)
            {
                hashCode ^= this.TargetType.GetHashCode();
            }
            return hashCode;
        }

        public static bool operator ==(XamlValueConverter<TConverterBase> converter1, XamlValueConverter<TConverterBase> converter2)
        {
            if (object.ReferenceEquals(converter1, null))
            {
                return object.ReferenceEquals(converter2, null);
            }
            if (object.ReferenceEquals(converter2, null))
            {
                return false;
            }
            return (((converter1.ConverterType == converter2.ConverterType) && (converter1.TargetType == converter2.TargetType)) && (converter1.Name == converter2.Name));
        }

        public static bool operator !=(XamlValueConverter<TConverterBase> converter1, XamlValueConverter<TConverterBase> converter2)
        {
            return !(converter1 == converter2);
        }

        public override string ToString()
        {
            return this.Name;
        }

        public TConverterBase ConverterInstance
        {
            get
            {
                if (!this._instanceIsSet)
                {
                    Interlocked.CompareExchange<TConverterBase>(ref this._instance, this.CreateInstance(), default(TConverterBase));
                    this._instanceIsSet = true;
                }
                return this._instance;
            }
        }

        public Type ConverterType { get; private set; }

        internal virtual bool IsPublic
        {
            get
            {
                if (this._isPublic == ThreeValuedBool.NotSet)
                {
                    this._isPublic = ((this.ConverterType == null) || this.ConverterType.IsVisible) ? ThreeValuedBool.True : ThreeValuedBool.False;
                }
                return (this._isPublic == ThreeValuedBool.True);
            }
        }

        public string Name { get; private set; }

        public XamlType TargetType { get; private set; }
    }
}

