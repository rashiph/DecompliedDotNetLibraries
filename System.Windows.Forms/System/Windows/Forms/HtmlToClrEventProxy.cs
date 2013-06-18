namespace System.Windows.Forms
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class HtmlToClrEventProxy : IReflect
    {
        private EventHandler eventHandler;
        private string eventName;
        private object sender;
        private IReflect typeIReflectImplementation;

        public HtmlToClrEventProxy(object sender, string eventName, EventHandler eventHandler)
        {
            this.eventHandler = eventHandler;
            this.eventName = eventName;
            System.Type type = typeof(HtmlToClrEventProxy);
            this.typeIReflectImplementation = type;
        }

        private void InvokeClrEvent()
        {
            if (this.eventHandler != null)
            {
                this.eventHandler(this.sender, EventArgs.Empty);
            }
        }

        [DispId(0)]
        public void OnHtmlEvent()
        {
            this.InvokeClrEvent();
        }

        FieldInfo IReflect.GetField(string name, BindingFlags bindingAttr)
        {
            return this.typeIReflectImplementation.GetField(name, bindingAttr);
        }

        FieldInfo[] IReflect.GetFields(BindingFlags bindingAttr)
        {
            return this.typeIReflectImplementation.GetFields(bindingAttr);
        }

        MemberInfo[] IReflect.GetMember(string name, BindingFlags bindingAttr)
        {
            return this.typeIReflectImplementation.GetMember(name, bindingAttr);
        }

        MemberInfo[] IReflect.GetMembers(BindingFlags bindingAttr)
        {
            return this.typeIReflectImplementation.GetMembers(bindingAttr);
        }

        MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr)
        {
            return this.typeIReflectImplementation.GetMethod(name, bindingAttr);
        }

        MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr, Binder binder, System.Type[] types, ParameterModifier[] modifiers)
        {
            return this.typeIReflectImplementation.GetMethod(name, bindingAttr, binder, types, modifiers);
        }

        MethodInfo[] IReflect.GetMethods(BindingFlags bindingAttr)
        {
            return this.typeIReflectImplementation.GetMethods(bindingAttr);
        }

        PropertyInfo[] IReflect.GetProperties(BindingFlags bindingAttr)
        {
            return this.typeIReflectImplementation.GetProperties(bindingAttr);
        }

        PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr)
        {
            return this.typeIReflectImplementation.GetProperty(name, bindingAttr);
        }

        PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr, Binder binder, System.Type returnType, System.Type[] types, ParameterModifier[] modifiers)
        {
            return this.typeIReflectImplementation.GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
        }

        object IReflect.InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            if (name == "[DISPID=0]")
            {
                this.OnHtmlEvent();
                return null;
            }
            return this.typeIReflectImplementation.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
        }

        public string EventName
        {
            get
            {
                return this.eventName;
            }
        }

        System.Type IReflect.UnderlyingSystemType
        {
            get
            {
                return this.typeIReflectImplementation.UnderlyingSystemType;
            }
        }
    }
}

