namespace System.Xml.Schema
{
    using System;
    using System.Xml;

    internal abstract class XmlValueConverter
    {
        protected XmlValueConverter()
        {
        }

        public abstract object ChangeType(bool value, Type destinationType);
        public abstract object ChangeType(DateTime value, Type destinationType);
        public abstract object ChangeType(DateTimeOffset value, Type destinationType);
        public abstract object ChangeType(decimal value, Type destinationType);
        public abstract object ChangeType(double value, Type destinationType);
        public abstract object ChangeType(int value, Type destinationType);
        public abstract object ChangeType(long value, Type destinationType);
        public abstract object ChangeType(object value, Type destinationType);
        public abstract object ChangeType(float value, Type destinationType);
        public abstract object ChangeType(string value, Type destinationType);
        public abstract object ChangeType(object value, Type destinationType, IXmlNamespaceResolver nsResolver);
        public abstract object ChangeType(string value, Type destinationType, IXmlNamespaceResolver nsResolver);
        public abstract bool ToBoolean(bool value);
        public abstract bool ToBoolean(DateTime value);
        public abstract bool ToBoolean(DateTimeOffset value);
        public abstract bool ToBoolean(decimal value);
        public abstract bool ToBoolean(double value);
        public abstract bool ToBoolean(int value);
        public abstract bool ToBoolean(long value);
        public abstract bool ToBoolean(object value);
        public abstract bool ToBoolean(float value);
        public abstract bool ToBoolean(string value);
        public abstract DateTime ToDateTime(bool value);
        public abstract DateTime ToDateTime(DateTime value);
        public abstract DateTime ToDateTime(DateTimeOffset value);
        public abstract DateTime ToDateTime(decimal value);
        public abstract DateTime ToDateTime(double value);
        public abstract DateTime ToDateTime(int value);
        public abstract DateTime ToDateTime(long value);
        public abstract DateTime ToDateTime(object value);
        public abstract DateTime ToDateTime(float value);
        public abstract DateTime ToDateTime(string value);
        public abstract DateTimeOffset ToDateTimeOffset(bool value);
        public abstract DateTimeOffset ToDateTimeOffset(DateTime value);
        public abstract DateTimeOffset ToDateTimeOffset(DateTimeOffset value);
        public abstract DateTimeOffset ToDateTimeOffset(decimal value);
        public abstract DateTimeOffset ToDateTimeOffset(double value);
        public abstract DateTimeOffset ToDateTimeOffset(int value);
        public abstract DateTimeOffset ToDateTimeOffset(long value);
        public abstract DateTimeOffset ToDateTimeOffset(object value);
        public abstract DateTimeOffset ToDateTimeOffset(float value);
        public abstract DateTimeOffset ToDateTimeOffset(string value);
        public abstract decimal ToDecimal(bool value);
        public abstract decimal ToDecimal(DateTime value);
        public abstract decimal ToDecimal(DateTimeOffset value);
        public abstract decimal ToDecimal(decimal value);
        public abstract decimal ToDecimal(double value);
        public abstract decimal ToDecimal(int value);
        public abstract decimal ToDecimal(long value);
        public abstract decimal ToDecimal(object value);
        public abstract decimal ToDecimal(float value);
        public abstract decimal ToDecimal(string value);
        public abstract double ToDouble(bool value);
        public abstract double ToDouble(DateTime value);
        public abstract double ToDouble(DateTimeOffset value);
        public abstract double ToDouble(decimal value);
        public abstract double ToDouble(double value);
        public abstract double ToDouble(int value);
        public abstract double ToDouble(long value);
        public abstract double ToDouble(object value);
        public abstract double ToDouble(float value);
        public abstract double ToDouble(string value);
        public abstract int ToInt32(bool value);
        public abstract int ToInt32(DateTime value);
        public abstract int ToInt32(DateTimeOffset value);
        public abstract int ToInt32(decimal value);
        public abstract int ToInt32(double value);
        public abstract int ToInt32(int value);
        public abstract int ToInt32(long value);
        public abstract int ToInt32(object value);
        public abstract int ToInt32(float value);
        public abstract int ToInt32(string value);
        public abstract long ToInt64(bool value);
        public abstract long ToInt64(DateTime value);
        public abstract long ToInt64(DateTimeOffset value);
        public abstract long ToInt64(decimal value);
        public abstract long ToInt64(double value);
        public abstract long ToInt64(int value);
        public abstract long ToInt64(long value);
        public abstract long ToInt64(object value);
        public abstract long ToInt64(float value);
        public abstract long ToInt64(string value);
        public abstract float ToSingle(bool value);
        public abstract float ToSingle(DateTime value);
        public abstract float ToSingle(DateTimeOffset value);
        public abstract float ToSingle(decimal value);
        public abstract float ToSingle(double value);
        public abstract float ToSingle(int value);
        public abstract float ToSingle(long value);
        public abstract float ToSingle(object value);
        public abstract float ToSingle(float value);
        public abstract float ToSingle(string value);
        public abstract string ToString(bool value);
        public abstract string ToString(DateTime value);
        public abstract string ToString(DateTimeOffset value);
        public abstract string ToString(decimal value);
        public abstract string ToString(double value);
        public abstract string ToString(int value);
        public abstract string ToString(long value);
        public abstract string ToString(object value);
        public abstract string ToString(float value);
        public abstract string ToString(string value);
        public abstract string ToString(object value, IXmlNamespaceResolver nsResolver);
        public abstract string ToString(string value, IXmlNamespaceResolver nsResolver);
    }
}

