namespace System.Runtime.InteropServices
{
    using System;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, Inherited=false), ComVisible(true)]
    public sealed class BestFitMappingAttribute : Attribute
    {
        internal bool _bestFitMapping;
        public bool ThrowOnUnmappableChar;

        public BestFitMappingAttribute(bool BestFitMapping)
        {
            this._bestFitMapping = BestFitMapping;
        }

        public bool BestFitMapping
        {
            get
            {
                return this._bestFitMapping;
            }
        }
    }
}

