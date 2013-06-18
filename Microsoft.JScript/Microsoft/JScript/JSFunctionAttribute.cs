namespace Microsoft.JScript
{
    using System;

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public class JSFunctionAttribute : Attribute
    {
        internal JSFunctionAttributeEnum attributeValue;
        internal JSBuiltin builtinFunction;

        public JSFunctionAttribute(JSFunctionAttributeEnum value)
        {
            this.attributeValue = value;
            this.builtinFunction = JSBuiltin.None;
        }

        public JSFunctionAttribute(JSFunctionAttributeEnum value, JSBuiltin builtinFunction)
        {
            this.attributeValue = value;
            this.builtinFunction = builtinFunction;
        }

        public JSFunctionAttributeEnum GetAttributeValue()
        {
            return this.attributeValue;
        }
    }
}

