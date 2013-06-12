namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Security;
    using System.Windows.Forms;

    [SuppressUnmanagedCodeSecurity]
    internal class Com2ICategorizePropertiesHandler : Com2ExtendedBrowsingHandler
    {
        private string GetCategoryFromObject(object obj, int dispid)
        {
            if ((obj != null) && (obj is NativeMethods.ICategorizeProperties))
            {
                NativeMethods.ICategorizeProperties properties = (NativeMethods.ICategorizeProperties) obj;
                try
                {
                    int categoryID = 0;
                    if (properties.MapPropertyToCategory(dispid, ref categoryID) == 0)
                    {
                        string categoryName = null;
                        switch (categoryID)
                        {
                            case -11:
                                return System.Windows.Forms.SR.GetString("PropertyCategoryDDE");

                            case -10:
                                return System.Windows.Forms.SR.GetString("PropertyCategoryScale");

                            case -9:
                                return System.Windows.Forms.SR.GetString("PropertyCategoryText");

                            case -8:
                                return System.Windows.Forms.SR.GetString("PropertyCategoryList");

                            case -7:
                                return System.Windows.Forms.SR.GetString("PropertyCategoryData");

                            case -6:
                                return System.Windows.Forms.SR.GetString("PropertyCategoryBehavior");

                            case -5:
                                return System.Windows.Forms.SR.GetString("PropertyCategoryAppearance");

                            case -4:
                                return System.Windows.Forms.SR.GetString("PropertyCategoryPosition");

                            case -3:
                                return System.Windows.Forms.SR.GetString("PropertyCategoryFont");

                            case -2:
                                return System.Windows.Forms.SR.GetString("PropertyCategoryMisc");

                            case -1:
                                return "";
                        }
                        if (properties.GetCategoryName(categoryID, CultureInfo.CurrentCulture.LCID, out categoryName) == 0)
                        {
                            return categoryName;
                        }
                    }
                }
                catch
                {
                }
            }
            return null;
        }

        private void OnGetAttributes(Com2PropertyDescriptor sender, GetAttributesEvent attrEvent)
        {
            string categoryFromObject = this.GetCategoryFromObject(sender.TargetObject, sender.DISPID);
            if ((categoryFromObject != null) && (categoryFromObject.Length > 0))
            {
                attrEvent.Add(new CategoryAttribute(categoryFromObject));
            }
        }

        public override void SetupPropertyHandlers(Com2PropertyDescriptor[] propDesc)
        {
            if (propDesc != null)
            {
                for (int i = 0; i < propDesc.Length; i++)
                {
                    propDesc[i].QueryGetBaseAttributes += new GetAttributesEventHandler(this.OnGetAttributes);
                }
            }
        }

        public override System.Type Interface
        {
            get
            {
                return typeof(NativeMethods.ICategorizeProperties);
            }
        }
    }
}

