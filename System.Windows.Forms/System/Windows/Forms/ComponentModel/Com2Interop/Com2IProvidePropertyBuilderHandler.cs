namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Security;
    using System.Windows.Forms;

    [SuppressUnmanagedCodeSecurity]
    internal class Com2IProvidePropertyBuilderHandler : Com2ExtendedBrowsingHandler
    {
        private bool GetBuilderGuidString(NativeMethods.IProvidePropertyBuilder target, int dispid, ref string strGuidBldr, int[] bldrType)
        {
            bool builderAvailable = false;
            string[] pbstrGuidBldr = new string[1];
            if (NativeMethods.Failed(target.MapPropertyToBuilder(dispid, bldrType, pbstrGuidBldr, ref builderAvailable)))
            {
                builderAvailable = false;
            }
            if (builderAvailable && ((bldrType[0] & 2) == 0))
            {
                builderAvailable = false;
            }
            if (!builderAvailable)
            {
                return false;
            }
            if (pbstrGuidBldr[0] == null)
            {
                strGuidBldr = Guid.Empty.ToString();
            }
            else
            {
                strGuidBldr = pbstrGuidBldr[0];
            }
            return true;
        }

        private void OnGetBaseAttributes(Com2PropertyDescriptor sender, GetAttributesEvent attrEvent)
        {
            NativeMethods.IProvidePropertyBuilder targetObject = sender.TargetObject as NativeMethods.IProvidePropertyBuilder;
            if (targetObject != null)
            {
                string strGuidBldr = null;
                bool flag = this.GetBuilderGuidString(targetObject, sender.DISPID, ref strGuidBldr, new int[1]);
                if ((sender.CanShow && flag) && typeof(UnsafeNativeMethods.IDispatch).IsAssignableFrom(sender.PropertyType))
                {
                    attrEvent.Add(BrowsableAttribute.Yes);
                }
            }
        }

        private void OnGetTypeConverterAndTypeEditor(Com2PropertyDescriptor sender, GetTypeConverterAndTypeEditorEvent gveevent)
        {
            object targetObject = sender.TargetObject;
            if (targetObject is NativeMethods.IProvidePropertyBuilder)
            {
                NativeMethods.IProvidePropertyBuilder target = (NativeMethods.IProvidePropertyBuilder) targetObject;
                int[] bldrType = new int[1];
                string strGuidBldr = null;
                if (this.GetBuilderGuidString(target, sender.DISPID, ref strGuidBldr, bldrType))
                {
                    gveevent.TypeEditor = new Com2PropertyBuilderUITypeEditor(sender, strGuidBldr, bldrType[0], (UITypeEditor) gveevent.TypeEditor);
                }
            }
        }

        public override void SetupPropertyHandlers(Com2PropertyDescriptor[] propDesc)
        {
            if (propDesc != null)
            {
                for (int i = 0; i < propDesc.Length; i++)
                {
                    propDesc[i].QueryGetBaseAttributes += new GetAttributesEventHandler(this.OnGetBaseAttributes);
                    propDesc[i].QueryGetTypeConverterAndTypeEditor += new GetTypeConverterAndTypeEditorEventHandler(this.OnGetTypeConverterAndTypeEditor);
                }
            }
        }

        public override System.Type Interface
        {
            get
            {
                return typeof(NativeMethods.IProvidePropertyBuilder);
            }
        }
    }
}

