namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;

    [DesignerSerializer(typeof(BindMarkupExtensionSerializer), typeof(WorkflowMarkupSerializer)), ActivityValidator(typeof(ActivityBindValidator)), Browsable(true), TypeConverter(typeof(ActivityBindTypeConverter))]
    public sealed class ActivityBind : MarkupExtension, IPropertyValueProvider
    {
        [NonSerialized]
        private bool designMode;
        [NonSerialized]
        private bool dynamicUpdateMode;
        private string id;
        private string path;
        [NonSerialized]
        private object syncRoot;
        [NonSerialized]
        private IDictionary userData;

        public ActivityBind()
        {
            this.designMode = true;
            this.syncRoot = new object();
            this.id = string.Empty;
            this.path = string.Empty;
        }

        public ActivityBind(string name)
        {
            this.designMode = true;
            this.syncRoot = new object();
            this.id = string.Empty;
            this.path = string.Empty;
            this.id = name;
        }

        public ActivityBind(string name, string path)
        {
            this.designMode = true;
            this.syncRoot = new object();
            this.id = string.Empty;
            this.path = string.Empty;
            this.id = name;
            this.path = path;
        }

        private static ActivityBind GetContextBind(ActivityBind activityBind, Activity activity, out Activity contextActivity)
        {
            if (activityBind == null)
            {
                throw new ArgumentNullException("activityBind");
            }
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            BindRecursionContext context = new BindRecursionContext();
            ActivityBind bind = activityBind;
            contextActivity = activity;
            while (bind != null)
            {
                Activity activity2 = Helpers.ParseActivityForBind(contextActivity, bind.Name);
                if (activity2 == null)
                {
                    return null;
                }
                object dataContext = activity2;
                MemberInfo memberInfo = GetMemberInfo(dataContext.GetType(), bind.Path, null);
                if (memberInfo == null)
                {
                    contextActivity = activity2;
                    return bind;
                }
                if (memberInfo is FieldInfo)
                {
                    contextActivity = activity2;
                    return bind;
                }
                if ((!(memberInfo is PropertyInfo) || !((memberInfo as PropertyInfo).PropertyType == typeof(ActivityBind))) || (dataContext == null))
                {
                    return null;
                }
                object obj3 = MemberBind.GetValue(memberInfo, dataContext, bind.Path);
                if (!(obj3 is ActivityBind))
                {
                    return null;
                }
                if (context.Contains(contextActivity, bind))
                {
                    return null;
                }
                context.Add(contextActivity, bind);
                contextActivity = activity2;
                bind = obj3 as ActivityBind;
            }
            return bind;
        }

        internal static object GetDataSourceObject(Activity activity, string inputName, out string name)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (string.IsNullOrEmpty(inputName))
            {
                throw new ArgumentNullException("inputName");
            }
            return Helpers.GetDataSourceActivity(activity, inputName, out name);
        }

        internal static MemberInfo GetMemberInfo(Type dataSourceType, string path, Type targetType)
        {
            MemberInfo memberInfo = MemberBind.GetMemberInfo(dataSourceType, path);
            if (((targetType == null) || !typeof(Delegate).IsAssignableFrom(targetType)) || ((memberInfo != null) && (memberInfo is EventInfo)))
            {
                return memberInfo;
            }
            MethodInfo method = targetType.GetMethod("Invoke");
            List<Type> list = new List<Type>();
            foreach (ParameterInfo info3 in method.GetParameters())
            {
                list.Add(info3.ParameterType);
            }
            return dataSourceType.GetMethod(path, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, list.ToArray(), null);
        }

        private static object GetMemberValue(object dataSourceObject, MemberInfo memberInfo, string path, Type targetType)
        {
            if (((memberInfo is FieldInfo) || (memberInfo is PropertyInfo)) || (memberInfo is EventInfo))
            {
                return MemberBind.GetValue(memberInfo, dataSourceObject, path);
            }
            if ((targetType == null) || !(memberInfo is MethodInfo))
            {
                throw new InvalidOperationException(SR.GetString("Error_MemberNotFound"));
            }
            return Delegate.CreateDelegate(targetType, dataSourceObject, (MethodInfo) memberInfo);
        }

        internal static string GetRelativePathExpression(Activity parentActivity, Activity childActivity)
        {
            if (Helpers.GetRootActivity(childActivity) == childActivity)
            {
                return "/Self";
            }
            return parentActivity.QualifiedName;
        }

        public object GetRuntimeValue(Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            return this.InternalGetRuntimeValue(activity, null);
        }

        public object GetRuntimeValue(Activity activity, Type targetType)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }
            return this.InternalGetRuntimeValue(activity, targetType);
        }

        private object InternalGetRuntimeValue(Activity activity, Type targetType)
        {
            object runtimeValue = null;
            Activity dataSourceObject = Helpers.ParseActivityForBind(activity, this.Name);
            if (dataSourceObject != null)
            {
                MemberInfo memberInfo = GetMemberInfo(dataSourceObject.GetType(), this.Path, targetType);
                if (memberInfo != null)
                {
                    runtimeValue = GetMemberValue(dataSourceObject, memberInfo, this.Path, targetType);
                    if ((runtimeValue is ActivityBind) && (BindHelpers.GetMemberType(memberInfo) != typeof(ActivityBind)))
                    {
                        runtimeValue = ((ActivityBind) runtimeValue).GetRuntimeValue(dataSourceObject, targetType);
                    }
                    return runtimeValue;
                }
                DependencyProperty.FromName(this.Path, Helpers.GetRootActivity(activity).GetType());
            }
            return runtimeValue;
        }

        private void OnRuntimeInitialized(Activity activity)
        {
            Activity contextActivity = null;
            ActivityBind bind = GetContextBind(this, activity, out contextActivity);
            if ((bind != null) && (contextActivity != null))
            {
                Type dataSourceType = contextActivity.GetType();
                if (dataSourceType != null)
                {
                    MemberInfo info = GetMemberInfo(dataSourceType, bind.Path, null);
                    if ((info != null) && (((info is FieldInfo) || (info is PropertyInfo)) || (info is EventInfo)))
                    {
                        if (bind.UserData[UserDataKeys.BindDataSource] == null)
                        {
                            bind.UserData[UserDataKeys.BindDataSource] = new Hashtable();
                        }
                        ((Hashtable) bind.UserData[UserDataKeys.BindDataSource])[activity.QualifiedName] = info;
                        if (contextActivity != null)
                        {
                            if (bind.UserData[UserDataKeys.BindDataContextActivity] == null)
                            {
                                bind.UserData[UserDataKeys.BindDataContextActivity] = new Hashtable();
                            }
                            ((Hashtable) bind.UserData[UserDataKeys.BindDataContextActivity])[activity.QualifiedName] = contextActivity.QualifiedName;
                        }
                    }
                }
            }
        }

        public override object ProvideValue(IServiceProvider provider)
        {
            return this;
        }

        internal void SetContext(Activity activity)
        {
            this.designMode = false;
            this.OnRuntimeInitialized(activity);
        }

        public void SetRuntimeValue(Activity activity, object value)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            Activity dataSourceObject = Helpers.ParseActivityForBind(activity, this.Name);
            if (dataSourceObject != null)
            {
                MemberInfo memberInfo = GetMemberInfo(dataSourceObject.GetType(), this.Path, null);
                if (memberInfo != null)
                {
                    ActivityBind bind = GetMemberValue(dataSourceObject, memberInfo, this.Path, null) as ActivityBind;
                    if (bind != null)
                    {
                        bind.SetRuntimeValue(dataSourceObject, value);
                    }
                    else
                    {
                        MemberBind.SetValue(dataSourceObject, this.Path, value);
                    }
                }
            }
        }

        ICollection IPropertyValueProvider.GetPropertyValues(ITypeDescriptorContext context)
        {
            ArrayList list = new ArrayList();
            if ((string.Equals(context.PropertyDescriptor.Name, "Path", StringComparison.Ordinal) && !string.IsNullOrEmpty(this.Name)) && (context.PropertyDescriptor is ActivityBindPathPropertyDescriptor))
            {
                ITypeDescriptorContext outerPropertyContext = ((ActivityBindPathPropertyDescriptor) context.PropertyDescriptor).OuterPropertyContext;
                if (outerPropertyContext == null)
                {
                    return list;
                }
                Activity component = PropertyDescriptorUtils.GetComponent(outerPropertyContext) as Activity;
                if (component == null)
                {
                    return list;
                }
                Activity activity2 = Helpers.ParseActivityForBind(component, this.Name);
                if (activity2 == null)
                {
                    return list;
                }
                foreach (MemberInfo info in ActivityBindPropertyDescriptor.GetBindableMembers(activity2, outerPropertyContext))
                {
                    list.Add(info.Name);
                }
            }
            return list;
        }

        public override string ToString()
        {
            Activity context = this.UserData[UserDataKeys.BindDataContextActivity] as Activity;
            if (context == null)
            {
                return base.ToString();
            }
            string qualifiedName = string.Empty;
            if (!string.IsNullOrEmpty(this.Name))
            {
                qualifiedName = Helpers.ParseActivityForBind(context, this.Name).QualifiedName;
            }
            if (!string.IsNullOrEmpty(this.Path))
            {
                string path = this.Path;
                int length = path.IndexOfAny(new char[] { '.', '/', '[' });
                path = (length != -1) ? path.Substring(0, length) : path;
                qualifiedName = qualifiedName + (!string.IsNullOrEmpty(qualifiedName) ? ("." + path) : path);
            }
            return qualifiedName;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        private bool DesignMode
        {
            get
            {
                return (this.designMode && !this.dynamicUpdateMode);
            }
        }

        internal bool DynamicUpdateMode
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dynamicUpdateMode;
            }
            set
            {
                this.dynamicUpdateMode = false;
            }
        }

        [ConstructorArgument("name"), DefaultValue(""), SRDescription("ActivityBindIDDescription")]
        public string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.id;
            }
            set
            {
                if (!this.DesignMode)
                {
                    throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
                }
                this.id = value;
            }
        }

        [DefaultValue(""), SRDescription("ActivityBindPathDescription"), TypeConverter(typeof(ActivityBindPathTypeConverter))]
        public string Path
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.path;
            }
            set
            {
                if (!this.DesignMode)
                {
                    throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
                }
                this.path = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDictionary UserData
        {
            get
            {
                if (this.userData == null)
                {
                    lock (this.syncRoot)
                    {
                        if (this.userData == null)
                        {
                            this.userData = Hashtable.Synchronized(new Hashtable());
                        }
                    }
                }
                return this.userData;
            }
        }
    }
}

