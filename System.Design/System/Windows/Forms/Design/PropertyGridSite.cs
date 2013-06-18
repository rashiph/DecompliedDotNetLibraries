namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;

    internal class PropertyGridSite : ISite, IServiceProvider
    {
        private IComponent comp;
        private bool inGetService;
        private IServiceProvider sp;

        public PropertyGridSite(IServiceProvider sp, IComponent comp)
        {
            this.sp = sp;
            this.comp = comp;
        }

        public object GetService(Type t)
        {
            if (!this.inGetService && (this.sp != null))
            {
                try
                {
                    this.inGetService = true;
                    return this.sp.GetService(t);
                }
                finally
                {
                    this.inGetService = false;
                }
            }
            return null;
        }

        public IComponent Component
        {
            get
            {
                return this.comp;
            }
        }

        public IContainer Container
        {
            get
            {
                return null;
            }
        }

        public bool DesignMode
        {
            get
            {
                return false;
            }
        }

        public string Name
        {
            get
            {
                return null;
            }
            set
            {
            }
        }
    }
}

