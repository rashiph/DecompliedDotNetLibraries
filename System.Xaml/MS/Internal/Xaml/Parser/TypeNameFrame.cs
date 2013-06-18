namespace MS.Internal.Xaml.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal class TypeNameFrame
    {
        private List<XamlTypeName> _typeArgs;

        public void AllocateTypeArgs()
        {
            this._typeArgs = new List<XamlTypeName>();
        }

        public string Name { get; set; }

        public string Namespace { get; set; }

        public List<XamlTypeName> TypeArgs
        {
            get
            {
                return this._typeArgs;
            }
        }
    }
}

