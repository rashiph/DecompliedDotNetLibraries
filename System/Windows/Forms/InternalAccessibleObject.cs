namespace System.Windows.Forms
{
    using Accessibility;
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    internal sealed class InternalAccessibleObject : StandardOleMarshalObject, System.Windows.Forms.UnsafeNativeMethods.IAccessibleInternal, IReflect, System.Windows.Forms.UnsafeNativeMethods.IEnumVariant, System.Windows.Forms.UnsafeNativeMethods.IOleWindow
    {
        private IAccessible publicIAccessible;
        private System.Windows.Forms.UnsafeNativeMethods.IEnumVariant publicIEnumVariant;
        private System.Windows.Forms.UnsafeNativeMethods.IOleWindow publicIOleWindow;
        private IReflect publicIReflect;

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        internal InternalAccessibleObject(AccessibleObject accessibleImplemention)
        {
            this.publicIAccessible = accessibleImplemention;
            this.publicIEnumVariant = accessibleImplemention;
            this.publicIOleWindow = accessibleImplemention;
            this.publicIReflect = accessibleImplemention;
        }

        private object AsNativeAccessible(object accObject)
        {
            if (accObject is AccessibleObject)
            {
                return new InternalAccessibleObject(accObject as AccessibleObject);
            }
            return accObject;
        }

        FieldInfo IReflect.GetField(string name, BindingFlags bindingAttr)
        {
            return this.publicIReflect.GetField(name, bindingAttr);
        }

        FieldInfo[] IReflect.GetFields(BindingFlags bindingAttr)
        {
            return this.publicIReflect.GetFields(bindingAttr);
        }

        MemberInfo[] IReflect.GetMember(string name, BindingFlags bindingAttr)
        {
            return this.publicIReflect.GetMember(name, bindingAttr);
        }

        MemberInfo[] IReflect.GetMembers(BindingFlags bindingAttr)
        {
            return this.publicIReflect.GetMembers(bindingAttr);
        }

        MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr)
        {
            return this.publicIReflect.GetMethod(name, bindingAttr);
        }

        MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr, Binder binder, System.Type[] types, ParameterModifier[] modifiers)
        {
            return this.publicIReflect.GetMethod(name, bindingAttr, binder, types, modifiers);
        }

        MethodInfo[] IReflect.GetMethods(BindingFlags bindingAttr)
        {
            return this.publicIReflect.GetMethods(bindingAttr);
        }

        PropertyInfo[] IReflect.GetProperties(BindingFlags bindingAttr)
        {
            return this.publicIReflect.GetProperties(bindingAttr);
        }

        PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr)
        {
            return this.publicIReflect.GetProperty(name, bindingAttr);
        }

        PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr, Binder binder, System.Type returnType, System.Type[] types, ParameterModifier[] modifiers)
        {
            return this.publicIReflect.GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
        }

        object IReflect.InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            IntSecurity.UnmanagedCode.Demand();
            return this.publicIReflect.InvokeMember(name, invokeAttr, binder, this.publicIAccessible, args, modifiers, culture, namedParameters);
        }

        void System.Windows.Forms.UnsafeNativeMethods.IAccessibleInternal.accDoDefaultAction(object childID)
        {
            IntSecurity.UnmanagedCode.Assert();
            this.publicIAccessible.accDoDefaultAction(childID);
        }

        object System.Windows.Forms.UnsafeNativeMethods.IAccessibleInternal.accHitTest(int xLeft, int yTop)
        {
            IntSecurity.UnmanagedCode.Assert();
            return this.AsNativeAccessible(this.publicIAccessible.accHitTest(xLeft, yTop));
        }

        void System.Windows.Forms.UnsafeNativeMethods.IAccessibleInternal.accLocation(out int l, out int t, out int w, out int h, object childID)
        {
            IntSecurity.UnmanagedCode.Assert();
            this.publicIAccessible.accLocation(out l, out t, out w, out h, childID);
        }

        object System.Windows.Forms.UnsafeNativeMethods.IAccessibleInternal.accNavigate(int navDir, object childID)
        {
            IntSecurity.UnmanagedCode.Assert();
            return this.AsNativeAccessible(this.publicIAccessible.accNavigate(navDir, childID));
        }

        void System.Windows.Forms.UnsafeNativeMethods.IAccessibleInternal.accSelect(int flagsSelect, object childID)
        {
            IntSecurity.UnmanagedCode.Assert();
            this.publicIAccessible.accSelect(flagsSelect, childID);
        }

        object System.Windows.Forms.UnsafeNativeMethods.IAccessibleInternal.get_accChild(object childID)
        {
            IntSecurity.UnmanagedCode.Assert();
            return this.AsNativeAccessible(this.publicIAccessible.get_accChild(childID));
        }

        int System.Windows.Forms.UnsafeNativeMethods.IAccessibleInternal.get_accChildCount()
        {
            IntSecurity.UnmanagedCode.Assert();
            return this.publicIAccessible.accChildCount;
        }

        string System.Windows.Forms.UnsafeNativeMethods.IAccessibleInternal.get_accDefaultAction(object childID)
        {
            IntSecurity.UnmanagedCode.Assert();
            return this.publicIAccessible.get_accDefaultAction(childID);
        }

        string System.Windows.Forms.UnsafeNativeMethods.IAccessibleInternal.get_accDescription(object childID)
        {
            IntSecurity.UnmanagedCode.Assert();
            return this.publicIAccessible.get_accDescription(childID);
        }

        object System.Windows.Forms.UnsafeNativeMethods.IAccessibleInternal.get_accFocus()
        {
            IntSecurity.UnmanagedCode.Assert();
            return this.AsNativeAccessible(this.publicIAccessible.accFocus);
        }

        string System.Windows.Forms.UnsafeNativeMethods.IAccessibleInternal.get_accHelp(object childID)
        {
            IntSecurity.UnmanagedCode.Assert();
            return this.publicIAccessible.get_accHelp(childID);
        }

        int System.Windows.Forms.UnsafeNativeMethods.IAccessibleInternal.get_accHelpTopic(out string pszHelpFile, object childID)
        {
            IntSecurity.UnmanagedCode.Assert();
            return this.publicIAccessible.get_accHelpTopic(out pszHelpFile, childID);
        }

        string System.Windows.Forms.UnsafeNativeMethods.IAccessibleInternal.get_accKeyboardShortcut(object childID)
        {
            IntSecurity.UnmanagedCode.Assert();
            return this.publicIAccessible.get_accKeyboardShortcut(childID);
        }

        string System.Windows.Forms.UnsafeNativeMethods.IAccessibleInternal.get_accName(object childID)
        {
            IntSecurity.UnmanagedCode.Assert();
            return this.publicIAccessible.get_accName(childID);
        }

        object System.Windows.Forms.UnsafeNativeMethods.IAccessibleInternal.get_accParent()
        {
            IntSecurity.UnmanagedCode.Assert();
            return this.AsNativeAccessible(this.publicIAccessible.accParent);
        }

        object System.Windows.Forms.UnsafeNativeMethods.IAccessibleInternal.get_accRole(object childID)
        {
            IntSecurity.UnmanagedCode.Assert();
            return this.publicIAccessible.get_accRole(childID);
        }

        object System.Windows.Forms.UnsafeNativeMethods.IAccessibleInternal.get_accSelection()
        {
            IntSecurity.UnmanagedCode.Assert();
            return this.AsNativeAccessible(this.publicIAccessible.accSelection);
        }

        object System.Windows.Forms.UnsafeNativeMethods.IAccessibleInternal.get_accState(object childID)
        {
            IntSecurity.UnmanagedCode.Assert();
            return this.publicIAccessible.get_accState(childID);
        }

        string System.Windows.Forms.UnsafeNativeMethods.IAccessibleInternal.get_accValue(object childID)
        {
            IntSecurity.UnmanagedCode.Assert();
            return this.publicIAccessible.get_accValue(childID);
        }

        void System.Windows.Forms.UnsafeNativeMethods.IAccessibleInternal.set_accName(object childID, string newName)
        {
            IntSecurity.UnmanagedCode.Assert();
            this.publicIAccessible.set_accName(childID, newName);
        }

        void System.Windows.Forms.UnsafeNativeMethods.IAccessibleInternal.set_accValue(object childID, string newValue)
        {
            IntSecurity.UnmanagedCode.Assert();
            this.publicIAccessible.set_accValue(childID, newValue);
        }

        void System.Windows.Forms.UnsafeNativeMethods.IEnumVariant.Clone(System.Windows.Forms.UnsafeNativeMethods.IEnumVariant[] v)
        {
            IntSecurity.UnmanagedCode.Assert();
            this.publicIEnumVariant.Clone(v);
        }

        int System.Windows.Forms.UnsafeNativeMethods.IEnumVariant.Next(int n, IntPtr rgvar, int[] ns)
        {
            IntSecurity.UnmanagedCode.Assert();
            return this.publicIEnumVariant.Next(n, rgvar, ns);
        }

        void System.Windows.Forms.UnsafeNativeMethods.IEnumVariant.Reset()
        {
            IntSecurity.UnmanagedCode.Assert();
            this.publicIEnumVariant.Reset();
        }

        void System.Windows.Forms.UnsafeNativeMethods.IEnumVariant.Skip(int n)
        {
            IntSecurity.UnmanagedCode.Assert();
            this.publicIEnumVariant.Skip(n);
        }

        void System.Windows.Forms.UnsafeNativeMethods.IOleWindow.ContextSensitiveHelp(int fEnterMode)
        {
            IntSecurity.UnmanagedCode.Assert();
            this.publicIOleWindow.ContextSensitiveHelp(fEnterMode);
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleWindow.GetWindow(out IntPtr hwnd)
        {
            IntSecurity.UnmanagedCode.Assert();
            return this.publicIOleWindow.GetWindow(out hwnd);
        }

        System.Type IReflect.UnderlyingSystemType
        {
            get
            {
                return this.publicIReflect.UnderlyingSystemType;
            }
        }
    }
}

