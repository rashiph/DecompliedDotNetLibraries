namespace System.Xaml
{
    using System;

    public class AmbientPropertyValue
    {
        private XamlMember _property;
        private object _value;

        public AmbientPropertyValue(XamlMember property, object value)
        {
            this._property = property;
            this._value = value;
        }

        public XamlMember RetrievedProperty
        {
            get
            {
                return this._property;
            }
        }

        public object Value
        {
            get
            {
                return this._value;
            }
        }
    }
}

