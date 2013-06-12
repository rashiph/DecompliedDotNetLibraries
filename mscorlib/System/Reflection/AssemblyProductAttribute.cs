namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Assembly, Inherited=false), ComVisible(true)]
    public sealed class AssemblyProductAttribute : Attribute
    {
        private string m_product;

        public AssemblyProductAttribute(string product)
        {
            this.m_product = product;
        }

        public string Product
        {
            get
            {
                return this.m_product;
            }
        }
    }
}

