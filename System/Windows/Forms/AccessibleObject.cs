namespace System.Windows.Forms
{
    using Accessibility;
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [ComVisible(true)]
    public class AccessibleObject : StandardOleMarshalObject, IReflect, IAccessible, System.Windows.Forms.UnsafeNativeMethods.IEnumVariant, System.Windows.Forms.UnsafeNativeMethods.IOleWindow
    {
        private int accObjId;
        private System.Windows.Forms.UnsafeNativeMethods.IEnumVariant enumVariant;
        private IAccessible systemIAccessible;
        private System.Windows.Forms.UnsafeNativeMethods.IEnumVariant systemIEnumVariant;
        private System.Windows.Forms.UnsafeNativeMethods.IOleWindow systemIOleWindow;
        private bool systemWrapper;

        public AccessibleObject()
        {
            this.accObjId = -4;
        }

        private AccessibleObject(IAccessible iAcc)
        {
            this.accObjId = -4;
            this.systemIAccessible = iAcc;
            this.systemWrapper = true;
        }

        void IAccessible.accDoDefaultAction(object childID)
        {
            System.Windows.Forms.IntSecurity.UnmanagedCode.Demand();
            if (this.IsClientObject)
            {
                this.ValidateChildID(ref childID);
                if (childID.Equals(0))
                {
                    this.DoDefaultAction();
                    return;
                }
                AccessibleObject accessibleChild = this.GetAccessibleChild(childID);
                if (accessibleChild != null)
                {
                    accessibleChild.DoDefaultAction();
                    return;
                }
            }
            if (this.systemIAccessible != null)
            {
                try
                {
                    this.systemIAccessible.accDoDefaultAction(childID);
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode != -2147352573)
                    {
                        throw exception;
                    }
                }
            }
        }

        object IAccessible.accHitTest(int xLeft, int yTop)
        {
            if (this.IsClientObject)
            {
                AccessibleObject obj2 = this.HitTest(xLeft, yTop);
                if (obj2 != null)
                {
                    return this.AsVariant(obj2);
                }
            }
            if (this.systemIAccessible != null)
            {
                try
                {
                    return this.systemIAccessible.accHitTest(xLeft, yTop);
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode != -2147352573)
                    {
                        throw exception;
                    }
                }
            }
            return null;
        }

        void IAccessible.accLocation(out int pxLeft, out int pyTop, out int pcxWidth, out int pcyHeight, object childID)
        {
            pxLeft = 0;
            pyTop = 0;
            pcxWidth = 0;
            pcyHeight = 0;
            if (this.IsClientObject)
            {
                this.ValidateChildID(ref childID);
                if (childID.Equals(0))
                {
                    Rectangle bounds = this.Bounds;
                    pxLeft = bounds.X;
                    pyTop = bounds.Y;
                    pcxWidth = bounds.Width;
                    pcyHeight = bounds.Height;
                    return;
                }
                AccessibleObject accessibleChild = this.GetAccessibleChild(childID);
                if (accessibleChild != null)
                {
                    Rectangle rectangle2 = accessibleChild.Bounds;
                    pxLeft = rectangle2.X;
                    pyTop = rectangle2.Y;
                    pcxWidth = rectangle2.Width;
                    pcyHeight = rectangle2.Height;
                    return;
                }
            }
            if (this.systemIAccessible != null)
            {
                try
                {
                    this.systemIAccessible.accLocation(out pxLeft, out pyTop, out pcxWidth, out pcyHeight, childID);
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode != -2147352573)
                    {
                        throw exception;
                    }
                }
            }
        }

        object IAccessible.accNavigate(int navDir, object childID)
        {
            System.Windows.Forms.IntSecurity.UnmanagedCode.Demand();
            if (this.IsClientObject)
            {
                this.ValidateChildID(ref childID);
                if (childID.Equals(0))
                {
                    AccessibleObject obj2 = this.Navigate((AccessibleNavigation) navDir);
                    if (obj2 != null)
                    {
                        return this.AsVariant(obj2);
                    }
                }
                AccessibleObject accessibleChild = this.GetAccessibleChild(childID);
                if (accessibleChild != null)
                {
                    return this.AsVariant(accessibleChild.Navigate((AccessibleNavigation) navDir));
                }
            }
            if (this.systemIAccessible != null)
            {
                try
                {
                    object obj4;
                    if (!this.SysNavigate(navDir, childID, out obj4))
                    {
                        obj4 = this.systemIAccessible.accNavigate(navDir, childID);
                    }
                    return obj4;
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode != -2147352573)
                    {
                        throw exception;
                    }
                }
            }
            return null;
        }

        void IAccessible.accSelect(int flagsSelect, object childID)
        {
            System.Windows.Forms.IntSecurity.UnmanagedCode.Demand();
            if (this.IsClientObject)
            {
                this.ValidateChildID(ref childID);
                if (childID.Equals(0))
                {
                    this.Select((AccessibleSelection) flagsSelect);
                    return;
                }
                AccessibleObject accessibleChild = this.GetAccessibleChild(childID);
                if (accessibleChild != null)
                {
                    accessibleChild.Select((AccessibleSelection) flagsSelect);
                    return;
                }
            }
            if (this.systemIAccessible != null)
            {
                try
                {
                    this.systemIAccessible.accSelect(flagsSelect, childID);
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode != -2147352573)
                    {
                        throw exception;
                    }
                }
            }
        }

        object IAccessible.get_accChild(object childID)
        {
            if (this.IsClientObject)
            {
                this.ValidateChildID(ref childID);
                if (childID.Equals(0))
                {
                    return this.AsIAccessible(this);
                }
                AccessibleObject accessibleChild = this.GetAccessibleChild(childID);
                if (accessibleChild != null)
                {
                    if (accessibleChild == this)
                    {
                        return null;
                    }
                    return this.AsIAccessible(accessibleChild);
                }
            }
            if (this.systemIAccessible != null)
            {
                return this.systemIAccessible.get_accChild(childID);
            }
            return null;
        }

        string IAccessible.get_accDefaultAction(object childID)
        {
            if (this.IsClientObject)
            {
                this.ValidateChildID(ref childID);
                if (childID.Equals(0))
                {
                    return this.DefaultAction;
                }
                AccessibleObject accessibleChild = this.GetAccessibleChild(childID);
                if (accessibleChild != null)
                {
                    return accessibleChild.DefaultAction;
                }
            }
            if (this.systemIAccessible != null)
            {
                try
                {
                    return this.systemIAccessible.get_accDefaultAction(childID);
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode != -2147352573)
                    {
                        throw exception;
                    }
                }
            }
            return null;
        }

        string IAccessible.get_accDescription(object childID)
        {
            if (this.IsClientObject)
            {
                this.ValidateChildID(ref childID);
                if (childID.Equals(0))
                {
                    return this.Description;
                }
                AccessibleObject accessibleChild = this.GetAccessibleChild(childID);
                if (accessibleChild != null)
                {
                    return accessibleChild.Description;
                }
            }
            if (this.systemIAccessible != null)
            {
                try
                {
                    return this.systemIAccessible.get_accDescription(childID);
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode != -2147352573)
                    {
                        throw exception;
                    }
                }
            }
            return null;
        }

        string IAccessible.get_accHelp(object childID)
        {
            if (this.IsClientObject)
            {
                this.ValidateChildID(ref childID);
                if (childID.Equals(0))
                {
                    return this.Help;
                }
                AccessibleObject accessibleChild = this.GetAccessibleChild(childID);
                if (accessibleChild != null)
                {
                    return accessibleChild.Help;
                }
            }
            if (this.systemIAccessible != null)
            {
                try
                {
                    return this.systemIAccessible.get_accHelp(childID);
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode != -2147352573)
                    {
                        throw exception;
                    }
                }
            }
            return null;
        }

        int IAccessible.get_accHelpTopic(out string pszHelpFile, object childID)
        {
            if (this.IsClientObject)
            {
                this.ValidateChildID(ref childID);
                if (childID.Equals(0))
                {
                    return this.GetHelpTopic(out pszHelpFile);
                }
                AccessibleObject accessibleChild = this.GetAccessibleChild(childID);
                if (accessibleChild != null)
                {
                    return accessibleChild.GetHelpTopic(out pszHelpFile);
                }
            }
            if (this.systemIAccessible != null)
            {
                try
                {
                    return this.systemIAccessible.get_accHelpTopic(out pszHelpFile, childID);
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode != -2147352573)
                    {
                        throw exception;
                    }
                }
            }
            pszHelpFile = null;
            return -1;
        }

        string IAccessible.get_accKeyboardShortcut(object childID)
        {
            return this.get_accKeyboardShortcutInternal(childID);
        }

        string IAccessible.get_accName(object childID)
        {
            return this.get_accNameInternal(childID);
        }

        object IAccessible.get_accRole(object childID)
        {
            if (this.IsClientObject)
            {
                this.ValidateChildID(ref childID);
                if (childID.Equals(0))
                {
                    return (int) this.Role;
                }
                AccessibleObject accessibleChild = this.GetAccessibleChild(childID);
                if (accessibleChild != null)
                {
                    return (int) accessibleChild.Role;
                }
            }
            if (this.systemIAccessible != null)
            {
                return this.systemIAccessible.get_accRole(childID);
            }
            return null;
        }

        object IAccessible.get_accState(object childID)
        {
            if (this.IsClientObject)
            {
                this.ValidateChildID(ref childID);
                if (childID.Equals(0))
                {
                    return (int) this.State;
                }
                AccessibleObject accessibleChild = this.GetAccessibleChild(childID);
                if (accessibleChild != null)
                {
                    return (int) accessibleChild.State;
                }
            }
            if (this.systemIAccessible != null)
            {
                return this.systemIAccessible.get_accState(childID);
            }
            return null;
        }

        string IAccessible.get_accValue(object childID)
        {
            System.Windows.Forms.IntSecurity.UnmanagedCode.Demand();
            if (this.IsClientObject)
            {
                this.ValidateChildID(ref childID);
                if (childID.Equals(0))
                {
                    return this.Value;
                }
                AccessibleObject accessibleChild = this.GetAccessibleChild(childID);
                if (accessibleChild != null)
                {
                    return accessibleChild.Value;
                }
            }
            if (this.systemIAccessible != null)
            {
                try
                {
                    return this.systemIAccessible.get_accValue(childID);
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode != -2147352573)
                    {
                        throw exception;
                    }
                }
            }
            return null;
        }

        void IAccessible.set_accName(object childID, string newName)
        {
            if (this.IsClientObject)
            {
                this.ValidateChildID(ref childID);
                if (childID.Equals(0))
                {
                    this.Name = newName;
                    return;
                }
                AccessibleObject accessibleChild = this.GetAccessibleChild(childID);
                if (accessibleChild != null)
                {
                    accessibleChild.Name = newName;
                    return;
                }
            }
            if (this.systemIAccessible != null)
            {
                this.systemIAccessible.set_accName(childID, newName);
            }
        }

        void IAccessible.set_accValue(object childID, string newValue)
        {
            System.Windows.Forms.IntSecurity.UnmanagedCode.Demand();
            if (this.IsClientObject)
            {
                this.ValidateChildID(ref childID);
                if (childID.Equals(0))
                {
                    this.Value = newValue;
                    return;
                }
                AccessibleObject accessibleChild = this.GetAccessibleChild(childID);
                if (accessibleChild != null)
                {
                    accessibleChild.Value = newValue;
                    return;
                }
            }
            if (this.systemIAccessible != null)
            {
                try
                {
                    this.systemIAccessible.set_accValue(childID, newValue);
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode != -2147352573)
                    {
                        throw exception;
                    }
                }
            }
        }

        private IAccessible AsIAccessible(AccessibleObject obj)
        {
            if ((obj != null) && obj.systemWrapper)
            {
                return obj.systemIAccessible;
            }
            return obj;
        }

        private object AsVariant(AccessibleObject obj)
        {
            if (obj == this)
            {
                return 0;
            }
            return this.AsIAccessible(obj);
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public virtual void DoDefaultAction()
        {
            if (this.systemIAccessible != null)
            {
                try
                {
                    this.systemIAccessible.accDoDefaultAction(0);
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode != -2147352573)
                    {
                        throw exception;
                    }
                }
            }
        }

        internal virtual string get_accKeyboardShortcutInternal(object childID)
        {
            if (this.IsClientObject)
            {
                this.ValidateChildID(ref childID);
                if (childID.Equals(0))
                {
                    return this.KeyboardShortcut;
                }
                AccessibleObject accessibleChild = this.GetAccessibleChild(childID);
                if (accessibleChild != null)
                {
                    return accessibleChild.KeyboardShortcut;
                }
            }
            if (this.systemIAccessible != null)
            {
                try
                {
                    return this.systemIAccessible.get_accKeyboardShortcut(childID);
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode != -2147352573)
                    {
                        throw exception;
                    }
                }
            }
            return null;
        }

        internal virtual string get_accNameInternal(object childID)
        {
            if (this.IsClientObject)
            {
                this.ValidateChildID(ref childID);
                if (childID.Equals(0))
                {
                    return this.Name;
                }
                AccessibleObject accessibleChild = this.GetAccessibleChild(childID);
                if (accessibleChild != null)
                {
                    return accessibleChild.Name;
                }
            }
            if (this.systemIAccessible == null)
            {
                return null;
            }
            string str = this.systemIAccessible.get_accName(childID);
            if (!this.IsClientObject || ((str != null) && (str.Length != 0)))
            {
                return str;
            }
            return this.Name;
        }

        private AccessibleObject GetAccessibleChild(object childID)
        {
            if (!childID.Equals(0))
            {
                int index = ((int) childID) - 1;
                if ((index >= 0) && (index < this.GetChildCount()))
                {
                    return this.GetChild(index);
                }
            }
            return null;
        }

        public virtual AccessibleObject GetChild(int index)
        {
            return null;
        }

        public virtual int GetChildCount()
        {
            return -1;
        }

        public virtual AccessibleObject GetFocused()
        {
            if (this.GetChildCount() >= 0)
            {
                int childCount = this.GetChildCount();
                for (int i = 0; i < childCount; i++)
                {
                    AccessibleObject child = this.GetChild(i);
                    if ((child != null) && ((child.State & AccessibleStates.Focused) != AccessibleStates.None))
                    {
                        return child;
                    }
                }
                if ((this.State & AccessibleStates.Focused) != AccessibleStates.None)
                {
                    return this;
                }
                return null;
            }
            if (this.systemIAccessible != null)
            {
                try
                {
                    return this.WrapIAccessible(this.systemIAccessible.accFocus);
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode != -2147352573)
                    {
                        throw exception;
                    }
                }
            }
            return null;
        }

        public virtual int GetHelpTopic(out string fileName)
        {
            if (this.systemIAccessible != null)
            {
                try
                {
                    int num = this.systemIAccessible.get_accHelpTopic(out fileName, 0);
                    if ((fileName != null) && (fileName.Length > 0))
                    {
                        System.Windows.Forms.IntSecurity.DemandFileIO(FileIOPermissionAccess.PathDiscovery, fileName);
                    }
                    return num;
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode != -2147352573)
                    {
                        throw exception;
                    }
                }
            }
            fileName = null;
            return -1;
        }

        public virtual AccessibleObject GetSelected()
        {
            if (this.GetChildCount() >= 0)
            {
                int childCount = this.GetChildCount();
                for (int i = 0; i < childCount; i++)
                {
                    AccessibleObject child = this.GetChild(i);
                    if ((child != null) && ((child.State & AccessibleStates.Selected) != AccessibleStates.None))
                    {
                        return child;
                    }
                }
                if ((this.State & AccessibleStates.Selected) != AccessibleStates.None)
                {
                    return this;
                }
                return null;
            }
            if (this.systemIAccessible != null)
            {
                try
                {
                    return this.WrapIAccessible(this.systemIAccessible.accSelection);
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode != -2147352573)
                    {
                        throw exception;
                    }
                }
            }
            return null;
        }

        internal virtual bool GetSysChild(AccessibleNavigation navdir, out AccessibleObject accessibleObject)
        {
            accessibleObject = null;
            return false;
        }

        internal virtual int[] GetSysChildOrder()
        {
            return null;
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        internal IAccessible GetSystemIAccessibleInternal()
        {
            return this.systemIAccessible;
        }

        public virtual AccessibleObject HitTest(int x, int y)
        {
            if (this.GetChildCount() >= 0)
            {
                int childCount = this.GetChildCount();
                for (int i = 0; i < childCount; i++)
                {
                    AccessibleObject child = this.GetChild(i);
                    if ((child != null) && child.Bounds.Contains(x, y))
                    {
                        return child;
                    }
                }
                return this;
            }
            if (this.systemIAccessible != null)
            {
                try
                {
                    return this.WrapIAccessible(this.systemIAccessible.accHitTest(x, y));
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode != -2147352573)
                    {
                        throw exception;
                    }
                }
            }
            if (this.Bounds.Contains(x, y))
            {
                return this;
            }
            return null;
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public virtual AccessibleObject Navigate(AccessibleNavigation navdir)
        {
            if (this.GetChildCount() >= 0)
            {
                switch (navdir)
                {
                    case AccessibleNavigation.Up:
                    case AccessibleNavigation.Left:
                    case AccessibleNavigation.Previous:
                        if (this.Parent.GetChildCount() <= 0)
                        {
                            break;
                        }
                        return null;

                    case AccessibleNavigation.Down:
                    case AccessibleNavigation.Right:
                    case AccessibleNavigation.Next:
                        if (this.Parent.GetChildCount() <= 0)
                        {
                            break;
                        }
                        return null;

                    case AccessibleNavigation.FirstChild:
                        return this.GetChild(0);

                    case AccessibleNavigation.LastChild:
                        return this.GetChild(this.GetChildCount() - 1);
                }
            }
            if (this.systemIAccessible != null)
            {
                try
                {
                    object retObject = null;
                    if (!this.SysNavigate((int) navdir, 0, out retObject))
                    {
                        retObject = this.systemIAccessible.accNavigate((int) navdir, 0);
                    }
                    return this.WrapIAccessible(retObject);
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode != -2147352573)
                    {
                        throw exception;
                    }
                }
            }
            return null;
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public virtual void Select(AccessibleSelection flags)
        {
            if (this.systemIAccessible != null)
            {
                try
                {
                    this.systemIAccessible.accSelect((int) flags, 0);
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode != -2147352573)
                    {
                        throw exception;
                    }
                }
            }
        }

        private bool SysNavigate(int navDir, object childID, out object retObject)
        {
            AccessibleObject obj2;
            retObject = null;
            if (!childID.Equals(0))
            {
                return false;
            }
            if (!this.GetSysChild((AccessibleNavigation) navDir, out obj2))
            {
                return false;
            }
            retObject = (obj2 == null) ? null : this.AsVariant(obj2);
            return true;
        }

        FieldInfo IReflect.GetField(string name, BindingFlags bindingAttr)
        {
            return typeof(IAccessible).GetField(name, bindingAttr);
        }

        FieldInfo[] IReflect.GetFields(BindingFlags bindingAttr)
        {
            return typeof(IAccessible).GetFields(bindingAttr);
        }

        MemberInfo[] IReflect.GetMember(string name, BindingFlags bindingAttr)
        {
            return typeof(IAccessible).GetMember(name, bindingAttr);
        }

        MemberInfo[] IReflect.GetMembers(BindingFlags bindingAttr)
        {
            return typeof(IAccessible).GetMembers(bindingAttr);
        }

        MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr)
        {
            return typeof(IAccessible).GetMethod(name, bindingAttr);
        }

        MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr, Binder binder, System.Type[] types, ParameterModifier[] modifiers)
        {
            return typeof(IAccessible).GetMethod(name, bindingAttr, binder, types, modifiers);
        }

        MethodInfo[] IReflect.GetMethods(BindingFlags bindingAttr)
        {
            return typeof(IAccessible).GetMethods(bindingAttr);
        }

        PropertyInfo[] IReflect.GetProperties(BindingFlags bindingAttr)
        {
            return typeof(IAccessible).GetProperties(bindingAttr);
        }

        PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr)
        {
            return typeof(IAccessible).GetProperty(name, bindingAttr);
        }

        PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr, Binder binder, System.Type returnType, System.Type[] types, ParameterModifier[] modifiers)
        {
            return typeof(IAccessible).GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
        }

        object IReflect.InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            if (args.Length == 0)
            {
                MemberInfo[] member = typeof(IAccessible).GetMember(name);
                if (((member != null) && (member.Length > 0)) && (member[0] is PropertyInfo))
                {
                    MethodInfo getMethod = ((PropertyInfo) member[0]).GetGetMethod();
                    if ((getMethod != null) && (getMethod.GetParameters().Length > 0))
                    {
                        args = new object[getMethod.GetParameters().Length];
                        for (int i = 0; i < args.Length; i++)
                        {
                            args[i] = 0;
                        }
                    }
                }
            }
            return typeof(IAccessible).InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
        }

        void System.Windows.Forms.UnsafeNativeMethods.IEnumVariant.Clone(System.Windows.Forms.UnsafeNativeMethods.IEnumVariant[] v)
        {
            this.EnumVariant.Clone(v);
        }

        int System.Windows.Forms.UnsafeNativeMethods.IEnumVariant.Next(int n, IntPtr rgvar, int[] ns)
        {
            return this.EnumVariant.Next(n, rgvar, ns);
        }

        void System.Windows.Forms.UnsafeNativeMethods.IEnumVariant.Reset()
        {
            this.EnumVariant.Reset();
        }

        void System.Windows.Forms.UnsafeNativeMethods.IEnumVariant.Skip(int n)
        {
            this.EnumVariant.Skip(n);
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        void System.Windows.Forms.UnsafeNativeMethods.IOleWindow.ContextSensitiveHelp(int fEnterMode)
        {
            if (this.systemIOleWindow != null)
            {
                this.systemIOleWindow.ContextSensitiveHelp(fEnterMode);
            }
            else
            {
                AccessibleObject parent = this.Parent;
                if (parent != null)
                {
                    ((System.Windows.Forms.UnsafeNativeMethods.IOleWindow) parent).ContextSensitiveHelp(fEnterMode);
                }
            }
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        int System.Windows.Forms.UnsafeNativeMethods.IOleWindow.GetWindow(out IntPtr hwnd)
        {
            if (this.systemIOleWindow != null)
            {
                return this.systemIOleWindow.GetWindow(out hwnd);
            }
            AccessibleObject parent = this.Parent;
            if (parent != null)
            {
                return ((System.Windows.Forms.UnsafeNativeMethods.IOleWindow) parent).GetWindow(out hwnd);
            }
            hwnd = IntPtr.Zero;
            return -2147467259;
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected void UseStdAccessibleObjects(IntPtr handle)
        {
            this.UseStdAccessibleObjects(handle, this.AccessibleObjectId);
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected void UseStdAccessibleObjects(IntPtr handle, int objid)
        {
            Guid refiid = new Guid("{618736E0-3C3D-11CF-810C-00AA00389B71}");
            object pAcc = null;
            System.Windows.Forms.UnsafeNativeMethods.CreateStdAccessibleObject(new HandleRef(this, handle), objid, ref refiid, ref pAcc);
            Guid guid2 = new Guid("{00020404-0000-0000-C000-000000000046}");
            object obj3 = null;
            System.Windows.Forms.UnsafeNativeMethods.CreateStdAccessibleObject(new HandleRef(this, handle), objid, ref guid2, ref obj3);
            if ((pAcc != null) || (obj3 != null))
            {
                this.systemIAccessible = (IAccessible) pAcc;
                this.systemIEnumVariant = (System.Windows.Forms.UnsafeNativeMethods.IEnumVariant) obj3;
                this.systemIOleWindow = pAcc as System.Windows.Forms.UnsafeNativeMethods.IOleWindow;
            }
        }

        internal void ValidateChildID(ref object childID)
        {
            if (childID == null)
            {
                childID = 0;
            }
            else if (childID.Equals(-2147352572))
            {
                childID = 0;
            }
            else if (!(childID is int))
            {
                childID = 0;
            }
        }

        private AccessibleObject WrapIAccessible(object iacc)
        {
            IAccessible iAcc = iacc as IAccessible;
            if (iAcc == null)
            {
                return null;
            }
            if (this.systemIAccessible == iacc)
            {
                return this;
            }
            return new AccessibleObject(iAcc);
        }

        int IAccessible.accChildCount
        {
            get
            {
                int childCount = -1;
                if (this.IsClientObject)
                {
                    childCount = this.GetChildCount();
                }
                if (childCount != -1)
                {
                    return childCount;
                }
                if (this.systemIAccessible != null)
                {
                    return this.systemIAccessible.accChildCount;
                }
                return 0;
            }
        }

        object IAccessible.accFocus
        {
            get
            {
                if (this.IsClientObject)
                {
                    AccessibleObject focused = this.GetFocused();
                    if (focused != null)
                    {
                        return this.AsVariant(focused);
                    }
                }
                if (this.systemIAccessible != null)
                {
                    try
                    {
                        return this.systemIAccessible.accFocus;
                    }
                    catch (COMException exception)
                    {
                        if (exception.ErrorCode != -2147352573)
                        {
                            throw exception;
                        }
                    }
                }
                return null;
            }
        }

        object IAccessible.accParent
        {
            get
            {
                System.Windows.Forms.IntSecurity.UnmanagedCode.Demand();
                AccessibleObject parent = this.Parent;
                if ((parent != null) && (parent == this))
                {
                    parent = null;
                }
                return this.AsIAccessible(parent);
            }
        }

        object IAccessible.accSelection
        {
            get
            {
                if (this.IsClientObject)
                {
                    AccessibleObject selected = this.GetSelected();
                    if (selected != null)
                    {
                        return this.AsVariant(selected);
                    }
                }
                if (this.systemIAccessible != null)
                {
                    try
                    {
                        return this.systemIAccessible.accSelection;
                    }
                    catch (COMException exception)
                    {
                        if (exception.ErrorCode != -2147352573)
                        {
                            throw exception;
                        }
                    }
                }
                return null;
            }
        }

        internal int AccessibleObjectId
        {
            get
            {
                return this.accObjId;
            }
            set
            {
                this.accObjId = value;
            }
        }

        public virtual Rectangle Bounds
        {
            get
            {
                if (this.systemIAccessible != null)
                {
                    int pxLeft = 0;
                    int pyTop = 0;
                    int pcxWidth = 0;
                    int pcyHeight = 0;
                    try
                    {
                        this.systemIAccessible.accLocation(out pxLeft, out pyTop, out pcxWidth, out pcyHeight, 0);
                        return new Rectangle(pxLeft, pyTop, pcxWidth, pcyHeight);
                    }
                    catch (COMException exception)
                    {
                        if (exception.ErrorCode != -2147352573)
                        {
                            throw exception;
                        }
                    }
                }
                return Rectangle.Empty;
            }
        }

        public virtual string DefaultAction
        {
            get
            {
                if (this.systemIAccessible != null)
                {
                    try
                    {
                        return this.systemIAccessible.get_accDefaultAction(0);
                    }
                    catch (COMException exception)
                    {
                        if (exception.ErrorCode != -2147352573)
                        {
                            throw exception;
                        }
                    }
                }
                return null;
            }
        }

        public virtual string Description
        {
            get
            {
                if (this.systemIAccessible != null)
                {
                    try
                    {
                        return this.systemIAccessible.get_accDescription(0);
                    }
                    catch (COMException exception)
                    {
                        if (exception.ErrorCode != -2147352573)
                        {
                            throw exception;
                        }
                    }
                }
                return null;
            }
        }

        private System.Windows.Forms.UnsafeNativeMethods.IEnumVariant EnumVariant
        {
            get
            {
                if (this.enumVariant == null)
                {
                    this.enumVariant = new EnumVariantObject(this);
                }
                return this.enumVariant;
            }
        }

        public virtual string Help
        {
            get
            {
                if (this.systemIAccessible != null)
                {
                    try
                    {
                        return this.systemIAccessible.get_accHelp(0);
                    }
                    catch (COMException exception)
                    {
                        if (exception.ErrorCode != -2147352573)
                        {
                            throw exception;
                        }
                    }
                }
                return null;
            }
        }

        internal bool IsClientObject
        {
            get
            {
                return (this.AccessibleObjectId == -4);
            }
        }

        internal bool IsNonClientObject
        {
            get
            {
                return (this.AccessibleObjectId == 0);
            }
        }

        public virtual string KeyboardShortcut
        {
            get
            {
                if (this.systemIAccessible != null)
                {
                    try
                    {
                        return this.systemIAccessible.get_accKeyboardShortcut(0);
                    }
                    catch (COMException exception)
                    {
                        if (exception.ErrorCode != -2147352573)
                        {
                            throw exception;
                        }
                    }
                }
                return null;
            }
        }

        public virtual string Name
        {
            get
            {
                if (this.systemIAccessible != null)
                {
                    try
                    {
                        return this.systemIAccessible.get_accName(0);
                    }
                    catch (COMException exception)
                    {
                        if (exception.ErrorCode != -2147352573)
                        {
                            throw exception;
                        }
                    }
                }
                return null;
            }
            set
            {
                if (this.systemIAccessible != null)
                {
                    try
                    {
                        this.systemIAccessible.set_accName(0, value);
                    }
                    catch (COMException exception)
                    {
                        if (exception.ErrorCode != -2147352573)
                        {
                            throw exception;
                        }
                    }
                }
            }
        }

        public virtual AccessibleObject Parent
        {
            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                if (this.systemIAccessible != null)
                {
                    return this.WrapIAccessible(this.systemIAccessible.accParent);
                }
                return null;
            }
        }

        public virtual AccessibleRole Role
        {
            get
            {
                if (this.systemIAccessible != null)
                {
                    return (AccessibleRole) this.systemIAccessible.get_accRole(0);
                }
                return AccessibleRole.None;
            }
        }

        public virtual AccessibleStates State
        {
            get
            {
                if (this.systemIAccessible != null)
                {
                    return (AccessibleStates) this.systemIAccessible.get_accState(0);
                }
                return AccessibleStates.None;
            }
        }

        System.Type IReflect.UnderlyingSystemType
        {
            get
            {
                return typeof(IAccessible);
            }
        }

        public virtual string Value
        {
            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                if (this.systemIAccessible != null)
                {
                    try
                    {
                        return this.systemIAccessible.get_accValue(0);
                    }
                    catch (COMException exception)
                    {
                        if (exception.ErrorCode != -2147352573)
                        {
                            throw exception;
                        }
                    }
                }
                return "";
            }
            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            set
            {
                if (this.systemIAccessible != null)
                {
                    try
                    {
                        this.systemIAccessible.set_accValue(0, value);
                    }
                    catch (COMException exception)
                    {
                        if (exception.ErrorCode != -2147352573)
                        {
                            throw exception;
                        }
                    }
                }
            }
        }

        private class EnumVariantObject : System.Windows.Forms.UnsafeNativeMethods.IEnumVariant
        {
            private int currentChild;
            private AccessibleObject owner;

            public EnumVariantObject(AccessibleObject owner)
            {
                this.owner = owner;
            }

            public EnumVariantObject(AccessibleObject owner, int currentChild)
            {
                this.owner = owner;
                this.currentChild = currentChild;
            }

            private static IntPtr GetAddressOfVariantAtIndex(IntPtr variantArrayPtr, int index)
            {
                int num = 8 + (IntPtr.Size * 2);
                return (IntPtr) (((long) variantArrayPtr) + (index * num));
            }

            private static bool GotoItem(System.Windows.Forms.UnsafeNativeMethods.IEnumVariant iev, int index, IntPtr variantPtr)
            {
                int[] pceltFetched = new int[1];
                System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                try
                {
                    iev.Reset();
                    iev.Skip(index);
                    iev.Next(1, variantPtr, pceltFetched);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                return (pceltFetched[0] == 1);
            }

            private void NextEmpty(int n, IntPtr rgvar, int[] ns)
            {
                ns[0] = 0;
            }

            private void NextFromChildCollection(int n, IntPtr rgvar, int[] ns, int childCount)
            {
                int index = 0;
                while ((index < n) && (this.currentChild < childCount))
                {
                    this.currentChild++;
                    Marshal.GetNativeVariantForObject(this.currentChild, GetAddressOfVariantAtIndex(rgvar, index));
                    index++;
                }
                ns[0] = index;
            }

            private void NextFromSystem(int n, IntPtr rgvar, int[] ns)
            {
                System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                try
                {
                    this.owner.systemIEnumVariant.Next(n, rgvar, ns);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                this.currentChild += ns[0];
            }

            private void NextFromSystemReordered(int n, IntPtr rgvar, int[] ns, int[] newOrder)
            {
                int index = 0;
                while ((index < n) && (this.currentChild < newOrder.Length))
                {
                    if (!GotoItem(this.owner.systemIEnumVariant, newOrder[this.currentChild], GetAddressOfVariantAtIndex(rgvar, index)))
                    {
                        break;
                    }
                    this.currentChild++;
                    index++;
                }
                ns[0] = index;
            }

            void System.Windows.Forms.UnsafeNativeMethods.IEnumVariant.Clone(System.Windows.Forms.UnsafeNativeMethods.IEnumVariant[] v)
            {
                v[0] = new AccessibleObject.EnumVariantObject(this.owner, this.currentChild);
            }

            int System.Windows.Forms.UnsafeNativeMethods.IEnumVariant.Next(int n, IntPtr rgvar, int[] ns)
            {
                if (this.owner.IsClientObject)
                {
                    int childCount = this.owner.GetChildCount();
                    if (childCount >= 0)
                    {
                        this.NextFromChildCollection(n, rgvar, ns, childCount);
                    }
                    else if (this.owner.systemIEnumVariant == null)
                    {
                        this.NextEmpty(n, rgvar, ns);
                    }
                    else
                    {
                        int[] sysChildOrder = this.owner.GetSysChildOrder();
                        if (sysChildOrder != null)
                        {
                            this.NextFromSystemReordered(n, rgvar, ns, sysChildOrder);
                        }
                        else
                        {
                            this.NextFromSystem(n, rgvar, ns);
                        }
                    }
                }
                else
                {
                    this.NextFromSystem(n, rgvar, ns);
                }
                if (ns[0] != n)
                {
                    return 1;
                }
                return 0;
            }

            void System.Windows.Forms.UnsafeNativeMethods.IEnumVariant.Reset()
            {
                this.currentChild = 0;
                System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                try
                {
                    if (this.owner.systemIEnumVariant != null)
                    {
                        this.owner.systemIEnumVariant.Reset();
                    }
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }

            void System.Windows.Forms.UnsafeNativeMethods.IEnumVariant.Skip(int n)
            {
                this.currentChild += n;
                System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                try
                {
                    if (this.owner.systemIEnumVariant != null)
                    {
                        this.owner.systemIEnumVariant.Skip(n);
                    }
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
        }
    }
}

