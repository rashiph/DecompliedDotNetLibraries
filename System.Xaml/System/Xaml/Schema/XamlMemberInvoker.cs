namespace System.Xaml.Schema
{
    using System;
    using System.Reflection;
    using System.Security;
    using System.Xaml;

    public class XamlMemberInvoker
    {
        private XamlMember _member;
        private NullableReference<MethodInfo> _shouldSerializeMethod;
        private static XamlMemberInvoker s_Directive;
        private static object[] s_emptyObjectArray = new object[0];
        private static XamlMemberInvoker s_Unknown;

        protected XamlMemberInvoker()
        {
        }

        public XamlMemberInvoker(XamlMember member)
        {
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }
            this._member = member;
        }

        public virtual object GetValue(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            this.ThrowIfUnknown();
            if (this.UnderlyingGetter == null)
            {
                throw new NotSupportedException(System.Xaml.SR.Get("CantGetWriteonlyProperty", new object[] { this._member }));
            }
            return this.GetValueSafeCritical(instance);
        }

        [SecuritySafeCritical]
        private object GetValueSafeCritical(object instance)
        {
            if (this.UnderlyingGetter.IsStatic)
            {
                return SafeReflectionInvoker.InvokeMethod(this.UnderlyingGetter, null, new object[] { instance });
            }
            return SafeReflectionInvoker.InvokeMethod(this.UnderlyingGetter, instance, s_emptyObjectArray);
        }

        [SecuritySafeCritical]
        private static bool IsSystemXamlNonPublic(ref ThreeValuedBool methodIsSystemXamlNonPublic, MethodInfo method)
        {
            if (methodIsSystemXamlNonPublic == ThreeValuedBool.NotSet)
            {
                bool flag = SafeReflectionInvoker.IsSystemXamlNonPublic(method);
                methodIsSystemXamlNonPublic = flag ? ThreeValuedBool.True : ThreeValuedBool.False;
            }
            return (methodIsSystemXamlNonPublic == ThreeValuedBool.True);
        }

        public virtual void SetValue(object instance, object value)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            this.ThrowIfUnknown();
            if (this.UnderlyingSetter == null)
            {
                throw new NotSupportedException(System.Xaml.SR.Get("CantSetReadonlyProperty", new object[] { this._member }));
            }
            this.SetValueSafeCritical(instance, value);
        }

        [SecuritySafeCritical]
        private void SetValueSafeCritical(object instance, object value)
        {
            if (this.UnderlyingSetter.IsStatic)
            {
                SafeReflectionInvoker.InvokeMethod(this.UnderlyingSetter, null, new object[] { instance, value });
            }
            else
            {
                SafeReflectionInvoker.InvokeMethod(this.UnderlyingSetter, instance, new object[] { value });
            }
        }

        public virtual ShouldSerializeResult ShouldSerializeValue(object instance)
        {
            bool flag;
            if (this.IsUnknown)
            {
                return ShouldSerializeResult.Default;
            }
            if (!this._shouldSerializeMethod.IsSet)
            {
                Type[] emptyTypes;
                Type declaringType = this._member.UnderlyingMember.DeclaringType;
                string name = "ShouldSerialize" + this._member.Name;
                BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
                if (this._member.IsAttachable)
                {
                    emptyTypes = new Type[] { this._member.TargetType.UnderlyingType ?? typeof(object) };
                }
                else
                {
                    bindingAttr |= BindingFlags.Instance;
                    emptyTypes = Type.EmptyTypes;
                }
                this._shouldSerializeMethod.Value = declaringType.GetMethod(name, bindingAttr, null, emptyTypes, null);
            }
            MethodInfo info = this._shouldSerializeMethod.Value;
            if (info == null)
            {
                return ShouldSerializeResult.Default;
            }
            if (this._member.IsAttachable)
            {
                flag = (bool) info.Invoke(null, new object[] { instance });
            }
            else
            {
                flag = (bool) info.Invoke(instance, null);
            }
            if (!flag)
            {
                return ShouldSerializeResult.False;
            }
            return ShouldSerializeResult.True;
        }

        private void ThrowIfUnknown()
        {
            if (this.IsUnknown)
            {
                throw new NotSupportedException(System.Xaml.SR.Get("NotSupportedOnUnknownMember"));
            }
        }

        internal static XamlMemberInvoker DirectiveInvoker
        {
            get
            {
                if (s_Directive == null)
                {
                    s_Directive = new DirectiveMemberInvoker();
                }
                return s_Directive;
            }
        }

        private bool IsUnknown
        {
            get
            {
                if (this._member != null)
                {
                    return (this._member.UnderlyingMember == null);
                }
                return true;
            }
        }

        public MethodInfo UnderlyingGetter
        {
            get
            {
                if (!this.IsUnknown)
                {
                    return this._member.Getter;
                }
                return null;
            }
        }

        public MethodInfo UnderlyingSetter
        {
            get
            {
                if (!this.IsUnknown)
                {
                    return this._member.Setter;
                }
                return null;
            }
        }

        public static XamlMemberInvoker UnknownInvoker
        {
            get
            {
                if (s_Unknown == null)
                {
                    s_Unknown = new XamlMemberInvoker();
                }
                return s_Unknown;
            }
        }

        private class DirectiveMemberInvoker : XamlMemberInvoker
        {
            public override object GetValue(object instance)
            {
                throw new NotSupportedException(System.Xaml.SR.Get("NotSupportedOnDirective"));
            }

            public override void SetValue(object instance, object value)
            {
                throw new NotSupportedException(System.Xaml.SR.Get("NotSupportedOnDirective"));
            }
        }
    }
}

