namespace System.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class DesignerActionList
    {
        private bool _autoShow;
        private IComponent _component;

        public DesignerActionList(IComponent component)
        {
            this._component = component;
        }

        private object GetCustomAttribute(MemberInfo info, Type attributeType)
        {
            object[] customAttributes = info.GetCustomAttributes(attributeType, true);
            if (customAttributes.Length > 0)
            {
                return customAttributes[0];
            }
            return null;
        }

        private void GetMemberDisplayProperties(MemberInfo info, out string displayName, out string description, out string category)
        {
            string str;
            category = str = "";
            displayName = description = str;
            DescriptionAttribute customAttribute = this.GetCustomAttribute(info, typeof(DescriptionAttribute)) as DescriptionAttribute;
            if (customAttribute != null)
            {
                description = customAttribute.Description;
            }
            DisplayNameAttribute attribute2 = this.GetCustomAttribute(info, typeof(DisplayNameAttribute)) as DisplayNameAttribute;
            if (attribute2 != null)
            {
                displayName = attribute2.DisplayName;
            }
            CategoryAttribute attribute3 = this.GetCustomAttribute(info, typeof(CategoryAttribute)) as CategoryAttribute;
            if (attribute2 != null)
            {
                category = attribute3.Category;
            }
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = info.Name;
            }
        }

        public object GetService(Type serviceType)
        {
            if ((this._component != null) && (this._component.Site != null))
            {
                return this._component.Site.GetService(serviceType);
            }
            return null;
        }

        public virtual DesignerActionItemCollection GetSortedActionItems()
        {
            string str;
            string str2;
            string str3;
            SortedList<string, DesignerActionItem> list = new SortedList<string, DesignerActionItem>();
            IList<MethodInfo> list2 = Array.AsReadOnly<MethodInfo>(typeof(DesignerActionList).GetMethods(BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            IList<PropertyInfo> list3 = Array.AsReadOnly<PropertyInfo>(typeof(DesignerActionList).GetProperties(BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            foreach (MethodInfo info in base.GetType().GetMethods(BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if ((!list2.Contains(info) && (info.GetParameters().Length == 0)) && !info.IsSpecialName)
                {
                    this.GetMemberDisplayProperties(info, out str, out str2, out str3);
                    list.Add(info.Name, new DesignerActionMethodItem(this, info.Name, str, str3, str2));
                }
            }
            foreach (PropertyInfo info2 in base.GetType().GetProperties(BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (!list3.Contains(info2))
                {
                    this.GetMemberDisplayProperties(info2, out str, out str2, out str3);
                    list.Add(str, new DesignerActionPropertyItem(info2.Name, str, str3, str2));
                }
            }
            DesignerActionItemCollection items = new DesignerActionItemCollection();
            foreach (DesignerActionItem item in list.Values)
            {
                items.Add(item);
            }
            return items;
        }

        public virtual bool AutoShow
        {
            get
            {
                return this._autoShow;
            }
            set
            {
                if (this._autoShow != value)
                {
                    this._autoShow = value;
                }
            }
        }

        public IComponent Component
        {
            get
            {
                return this._component;
            }
        }
    }
}

