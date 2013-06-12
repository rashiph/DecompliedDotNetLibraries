namespace System.Web.UI
{
    using System;
    using System.ComponentModel;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class TemplateContainerAttribute : Attribute
    {
        private System.ComponentModel.BindingDirection _bindingDirection;
        private Type _containerType;

        public TemplateContainerAttribute(Type containerType) : this(containerType, System.ComponentModel.BindingDirection.OneWay)
        {
        }

        public TemplateContainerAttribute(Type containerType, System.ComponentModel.BindingDirection bindingDirection)
        {
            this._containerType = containerType;
            this._bindingDirection = bindingDirection;
        }

        public System.ComponentModel.BindingDirection BindingDirection
        {
            get
            {
                return this._bindingDirection;
            }
        }

        public Type ContainerType
        {
            get
            {
                return this._containerType;
            }
        }
    }
}

