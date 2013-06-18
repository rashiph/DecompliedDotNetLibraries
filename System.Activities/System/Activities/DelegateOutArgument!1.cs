namespace System.Activities
{
    using System;

    public sealed class DelegateOutArgument<T> : DelegateOutArgument
    {
        public DelegateOutArgument()
        {
        }

        public DelegateOutArgument(string name)
        {
            base.Name = name;
        }

        internal override Location CreateLocation()
        {
            return new Location<T>();
        }

        public T Get(ActivityContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            return context.GetValue<T>((LocationReference) this);
        }

        public Location<T> GetLocation(ActivityContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            return context.GetLocation<T>(this);
        }

        public void Set(ActivityContext context, T value)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            context.SetValue<T>((LocationReference) this, value);
        }

        protected override Type TypeCore
        {
            get
            {
                return typeof(T);
            }
        }
    }
}

