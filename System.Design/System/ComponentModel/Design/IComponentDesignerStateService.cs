namespace System.ComponentModel.Design
{
    using System;
    using System.ComponentModel;

    public interface IComponentDesignerStateService
    {
        object GetState(IComponent component, string key);
        void SetState(IComponent component, string key, object value);
    }
}

