namespace System.ComponentModel.Design
{
    using System;
    using System.ComponentModel;

    public interface IInheritanceService
    {
        void AddInheritedComponents(IComponent component, IContainer container);
        InheritanceAttribute GetInheritanceAttribute(IComponent component);
    }
}

