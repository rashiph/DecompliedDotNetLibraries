namespace System.Web.UI
{
    using System;

    public abstract class BuilderPropertyEntry : PropertyEntry
    {
        private ControlBuilder _builder;

        internal BuilderPropertyEntry()
        {
        }

        public ControlBuilder Builder
        {
            get
            {
                return this._builder;
            }
            set
            {
                this._builder = value;
            }
        }
    }
}

