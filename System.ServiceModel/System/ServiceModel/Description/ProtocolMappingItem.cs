namespace System.ServiceModel.Description
{
    using System;
    using System.Runtime.CompilerServices;

    internal class ProtocolMappingItem
    {
        public ProtocolMappingItem(string binding, string bindingConfiguration)
        {
            this.Binding = binding;
            this.BindingConfiguration = bindingConfiguration;
        }

        public string Binding { get; set; }

        public string BindingConfiguration { get; set; }
    }
}

