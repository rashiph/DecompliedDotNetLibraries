namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Workflow.ComponentModel.Compiler;

    internal abstract class MemberBind : BindBase
    {
        private string name;

        protected MemberBind()
        {
            this.name = string.Empty;
        }

        protected MemberBind(string name)
        {
            this.name = string.Empty;
            this.name = name;
        }

        internal static MemberInfo GetMemberInfo(Type srcType, string path)
        {
            PathWalker walker;
            if (srcType == null)
            {
                throw new ArgumentNullException("srcType");
            }
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(SR.GetString("Error_EmptyPathValue"), "path");
            }
            Type rootType = srcType;
            MemberInfo memberInfo = null;
            if (new PathWalker { MemberFound = (EventHandler<PathMemberInfoEventArgs>) Delegate.Combine(walker.MemberFound, delegate (object sender, PathMemberInfoEventArgs eventArgs) {
                memberInfo = eventArgs.MemberInfo;
                if (eventArgs.MemberKind == PathMemberKind.Event)
                {
                    eventArgs.Action = PathWalkAction.Stop;
                }
            }) }.TryWalkPropertyPath(rootType, path))
            {
                return memberInfo;
            }
            return null;
        }

        internal static object GetValue(MemberInfo memberInfo, object dataContext, string path)
        {
            PathWalker walker;
            if (memberInfo == null)
            {
                throw new ArgumentNullException("memberInfo");
            }
            if (dataContext == null)
            {
                throw new ArgumentNullException("dataContext");
            }
            if (path == null)
            {
                path = string.Empty;
            }
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            object targetObject = dataContext;
            Type memberType = dataContext.GetType();
            if (!new PathWalker { MemberFound = (EventHandler<PathMemberInfoEventArgs>) Delegate.Combine(walker.MemberFound, delegate (object sender, PathMemberInfoEventArgs eventArgs) {
                if (targetObject == null)
                {
                    eventArgs.Action = PathWalkAction.Cancel;
                    return;
                }
                switch (eventArgs.MemberKind)
                {
                    case PathMemberKind.Field:
                        memberType = (eventArgs.MemberInfo as FieldInfo).FieldType;
                        targetObject = (eventArgs.MemberInfo as FieldInfo).GetValue(targetObject);
                        goto Label_01CC;

                    case PathMemberKind.Event:
                    {
                        EventInfo memberInfo = eventArgs.MemberInfo as EventInfo;
                        memberType = memberInfo.EventHandlerType;
                        DependencyObject obj1 = targetObject as DependencyObject;
                        DependencyProperty dependencyProperty = DependencyProperty.FromName(memberInfo.Name, obj1.GetType());
                        if ((dependencyProperty == null) || (obj1 == null))
                        {
                            targetObject = null;
                            break;
                        }
                        if (!obj1.IsBindingSet(dependencyProperty))
                        {
                            targetObject = obj1.GetHandler(dependencyProperty);
                            break;
                        }
                        targetObject = obj1.GetBinding(dependencyProperty);
                        break;
                    }
                    case PathMemberKind.Property:
                        memberType = (eventArgs.MemberInfo as PropertyInfo).PropertyType;
                        if ((eventArgs.MemberInfo as PropertyInfo).CanRead)
                        {
                            targetObject = (eventArgs.MemberInfo as PropertyInfo).GetValue(targetObject, null);
                            goto Label_01CC;
                        }
                        eventArgs.Action = PathWalkAction.Cancel;
                        return;

                    case PathMemberKind.IndexedProperty:
                        memberType = (eventArgs.MemberInfo as PropertyInfo).PropertyType;
                        if ((eventArgs.MemberInfo as PropertyInfo).CanRead)
                        {
                            targetObject = (eventArgs.MemberInfo as PropertyInfo).GetValue(targetObject, eventArgs.IndexParameters);
                            goto Label_01CC;
                        }
                        eventArgs.Action = PathWalkAction.Cancel;
                        return;

                    case PathMemberKind.Index:
                        memberType = (eventArgs.MemberInfo as PropertyInfo).PropertyType;
                        targetObject = (eventArgs.MemberInfo as PropertyInfo).GetValue(targetObject, BindingFlags.GetProperty, null, eventArgs.IndexParameters, CultureInfo.InvariantCulture);
                        goto Label_01CC;

                    default:
                        goto Label_01CC;
                }
                eventArgs.Action = PathWalkAction.Stop;
            Label_01CC:
                if (targetObject != null)
                {
                    return;
                }
                if (!eventArgs.LastMemberInThePath)
                {
                    throw new InvalidOperationException(SR.GetString("Error_BindPathNullValue", new object[] { eventArgs.Path }));
                }
                eventArgs.Action = PathWalkAction.Cancel;
            }) }.TryWalkPropertyPath(memberType, path))
            {
                return null;
            }
            if (targetObject == dataContext)
            {
                return null;
            }
            return targetObject;
        }

        private static bool SafeType(IList<AuthorizedType> authorizedTypes, Type referenceType)
        {
            bool flag = false;
            foreach (AuthorizedType type in authorizedTypes)
            {
                if (type.RegularExpression.IsMatch(referenceType.AssemblyQualifiedName))
                {
                    flag = string.Compare(bool.TrueString, type.Authorized, StringComparison.OrdinalIgnoreCase) == 0;
                    if (!flag)
                    {
                        return false;
                    }
                }
            }
            return flag;
        }

        internal static void SetValue(object dataContext, string path, object value)
        {
            PathWalker walker;
            if (dataContext == null)
            {
                throw new ArgumentNullException("dataContext");
            }
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }
            object parentObj = null;
            object obj = dataContext;
            object[] args = null;
            MemberInfo memberInfo = null;
            if (new PathWalker { MemberFound = (EventHandler<PathMemberInfoEventArgs>) Delegate.Combine(walker.MemberFound, delegate (object sender, PathMemberInfoEventArgs eventArgs) {
                if (obj == null)
                {
                    eventArgs.Action = PathWalkAction.Cancel;
                }
                else
                {
                    parentObj = obj;
                    memberInfo = eventArgs.MemberInfo;
                    switch (eventArgs.MemberKind)
                    {
                        case PathMemberKind.Field:
                            obj = (eventArgs.MemberInfo as FieldInfo).GetValue(parentObj);
                            args = null;
                            return;

                        case PathMemberKind.Event:
                            eventArgs.Action = PathWalkAction.Cancel;
                            return;

                        case PathMemberKind.Property:
                            obj = (eventArgs.MemberInfo as PropertyInfo).GetValue(parentObj, null);
                            args = null;
                            return;

                        case PathMemberKind.IndexedProperty:
                        case PathMemberKind.Index:
                            obj = (eventArgs.MemberInfo as PropertyInfo).GetValue(parentObj, eventArgs.IndexParameters);
                            args = eventArgs.IndexParameters;
                            return;
                    }
                }
            }) }.TryWalkPropertyPath(dataContext.GetType(), path))
            {
                if (memberInfo is FieldInfo)
                {
                    (memberInfo as FieldInfo).SetValue(parentObj, value);
                }
                else if (memberInfo is PropertyInfo)
                {
                    if (!(memberInfo as PropertyInfo).CanWrite)
                    {
                        throw new InvalidOperationException(SR.GetString("Error_ReadOnlyField", new object[] { memberInfo.Name }));
                    }
                    (memberInfo as PropertyInfo).SetValue(parentObj, value, args);
                }
            }
        }

        internal static ValidationError ValidateTypesInPath(Type srcType, string path)
        {
            PathWalker walker;
            ValidationError error = null;
            if (srcType == null)
            {
                throw new ArgumentNullException("srcType");
            }
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(SR.GetString("Error_EmptyPathValue"), "path");
            }
            IList<AuthorizedType> authorizedTypes = WorkflowCompilationContext.Current.GetAuthorizedTypes();
            if (authorizedTypes == null)
            {
                return new ValidationError(SR.GetString("Error_ConfigFileMissingOrInvalid"), 0x178);
            }
            Type rootType = srcType;
            MemberInfo memberInfo = null;
            new PathWalker { MemberFound = (EventHandler<PathMemberInfoEventArgs>) Delegate.Combine(walker.MemberFound, delegate (object sender, PathMemberInfoEventArgs eventArgs) {
                Type fieldType = null;
                memberInfo = eventArgs.MemberInfo;
                if (memberInfo is FieldInfo)
                {
                    fieldType = ((FieldInfo) memberInfo).FieldType;
                }
                if (memberInfo is PropertyInfo)
                {
                    fieldType = ((PropertyInfo) memberInfo).PropertyType;
                }
                if ((fieldType != null) && !SafeType(authorizedTypes, fieldType))
                {
                    error = new ValidationError(SR.GetString("Error_TypeNotAuthorized", new object[] { fieldType }), 0x16b);
                    eventArgs.Action = PathWalkAction.Stop;
                }
            }) }.TryWalkPropertyPath(rootType, path);
            return error;
        }

        [DefaultValue("")]
        public string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
        }
    }
}

