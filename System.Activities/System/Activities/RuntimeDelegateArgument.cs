namespace System.Activities
{
    using System;
    using System.Runtime.CompilerServices;

    public sealed class RuntimeDelegateArgument
    {
        public RuntimeDelegateArgument(string name, System.Type type, ArgumentDirection direction, DelegateArgument boundArgument)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("name");
            }
            if (type == null)
            {
                throw FxTrace.Exception.ArgumentNull("type");
            }
            ArgumentDirectionHelper.Validate(direction, "direction");
            if (boundArgument != null)
            {
                boundArgument.Bind(this);
            }
            this.Name = name;
            this.Type = type;
            this.Direction = direction;
            this.BoundArgument = boundArgument;
        }

        public DelegateArgument BoundArgument { get; private set; }

        public ArgumentDirection Direction { get; private set; }

        public string Name { get; private set; }

        public System.Type Type { get; private set; }
    }
}

