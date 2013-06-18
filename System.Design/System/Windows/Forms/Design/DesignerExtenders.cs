namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;

    internal class DesignerExtenders
    {
        private IExtenderProviderService extenderService;
        private IExtenderProvider[] providers;

        public DesignerExtenders(IExtenderProviderService ex)
        {
            this.extenderService = ex;
            if (this.providers == null)
            {
                this.providers = new IExtenderProvider[] { new NameExtenderProvider(), new NameInheritedExtenderProvider() };
            }
            for (int i = 0; i < this.providers.Length; i++)
            {
                ex.AddExtenderProvider(this.providers[i]);
            }
        }

        public void Dispose()
        {
            if ((this.extenderService != null) && (this.providers != null))
            {
                for (int i = 0; i < this.providers.Length; i++)
                {
                    this.extenderService.RemoveExtenderProvider(this.providers[i]);
                }
                this.providers = null;
                this.extenderService = null;
            }
        }

        [ProvideProperty("Name", typeof(IComponent))]
        private class NameExtenderProvider : IExtenderProvider
        {
            private IComponent baseComponent;

            internal NameExtenderProvider()
            {
            }

            public virtual bool CanExtend(object o)
            {
                if ((this.GetBaseComponent(o) != o) && !TypeDescriptor.GetAttributes(o)[typeof(InheritanceAttribute)].Equals(InheritanceAttribute.NotInherited))
                {
                    return false;
                }
                return true;
            }

            protected IComponent GetBaseComponent(object o)
            {
                if (this.baseComponent == null)
                {
                    ISite site = ((IComponent) o).Site;
                    if (site != null)
                    {
                        IDesignerHost service = (IDesignerHost) site.GetService(typeof(IDesignerHost));
                        if (service != null)
                        {
                            this.baseComponent = service.RootComponent;
                        }
                    }
                }
                return this.baseComponent;
            }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ParenthesizePropertyName(true), System.Design.SRDescription("DesignerPropName"), MergableProperty(false), Category("Design")]
            public virtual string GetName(IComponent comp)
            {
                ISite site = comp.Site;
                if (site != null)
                {
                    return site.Name;
                }
                return null;
            }

            public void SetName(IComponent comp, string newName)
            {
                ISite site = comp.Site;
                if (site != null)
                {
                    site.Name = newName;
                }
            }
        }

        private class NameInheritedExtenderProvider : DesignerExtenders.NameExtenderProvider
        {
            internal NameInheritedExtenderProvider()
            {
            }

            public override bool CanExtend(object o)
            {
                if (base.GetBaseComponent(o) == o)
                {
                    return false;
                }
                return !TypeDescriptor.GetAttributes(o)[typeof(InheritanceAttribute)].Equals(InheritanceAttribute.NotInherited);
            }

            [ReadOnly(true)]
            public override string GetName(IComponent comp)
            {
                return base.GetName(comp);
            }
        }
    }
}

