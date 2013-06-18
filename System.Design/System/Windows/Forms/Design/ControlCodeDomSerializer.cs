namespace System.Windows.Forms.Design
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;
    using System.Windows.Forms;

    internal class ControlCodeDomSerializer : CodeDomSerializer
    {
        public override object Deserialize(IDesignerSerializationManager manager, object codeObject)
        {
            if ((manager == null) || (codeObject == null))
            {
                throw new ArgumentNullException((manager == null) ? "manager" : "codeObject");
            }
            IContainer service = (IContainer) manager.GetService(typeof(IContainer));
            ArrayList list = null;
            if (service != null)
            {
                list = new ArrayList(service.Components.Count);
                foreach (IComponent component in service.Components)
                {
                    Control control = component as Control;
                    if (control != null)
                    {
                        control.SuspendLayout();
                        list.Add(control);
                    }
                }
            }
            object obj2 = null;
            try
            {
                CodeDomSerializer serializer = (CodeDomSerializer) manager.GetSerializer(typeof(Component), typeof(CodeDomSerializer));
                if (serializer == null)
                {
                    return null;
                }
                obj2 = serializer.Deserialize(manager, codeObject);
            }
            finally
            {
                if (list != null)
                {
                    foreach (Control control2 in list)
                    {
                        control2.ResumeLayout(false);
                    }
                }
            }
            return obj2;
        }

        private bool HasAutoSizedChildren(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control.AutoSize)
                {
                    return true;
                }
            }
            return false;
        }

        private bool HasMixedInheritedChildren(Control parent)
        {
            bool flag = false;
            bool flag2 = false;
            foreach (Control control in parent.Controls)
            {
                InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(control)[typeof(InheritanceAttribute)];
                if ((attribute != null) && (attribute.InheritanceLevel != InheritanceLevel.NotInherited))
                {
                    flag = true;
                }
                else
                {
                    flag2 = true;
                }
                if (flag && flag2)
                {
                    return true;
                }
            }
            return false;
        }

        protected virtual bool HasSitedNonReadonlyChildren(Control parent)
        {
            if (parent.HasChildren)
            {
                foreach (Control control in parent.Controls)
                {
                    if ((control.Site != null) && control.Site.DesignMode)
                    {
                        InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(control)[typeof(InheritanceAttribute)];
                        if ((attribute != null) && (attribute.InheritanceLevel != InheritanceLevel.InheritedReadOnly))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override object Serialize(IDesignerSerializationManager manager, object value)
        {
            if ((manager == null) || (value == null))
            {
                throw new ArgumentNullException((manager == null) ? "manager" : "value");
            }
            CodeDomSerializer serializer = (CodeDomSerializer) manager.GetSerializer(typeof(Component), typeof(CodeDomSerializer));
            if (serializer == null)
            {
                return null;
            }
            object obj2 = serializer.Serialize(manager, value);
            InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(value)[typeof(InheritanceAttribute)];
            InheritanceLevel notInherited = InheritanceLevel.NotInherited;
            if (attribute != null)
            {
                notInherited = attribute.InheritanceLevel;
            }
            if (notInherited != InheritanceLevel.InheritedReadOnly)
            {
                IDesignerHost service = (IDesignerHost) manager.GetService(typeof(IDesignerHost));
                if (service != null)
                {
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(service.RootComponent)["Localizable"];
                    if (((descriptor != null) && (descriptor.PropertyType == typeof(bool))) && ((bool) descriptor.GetValue(service.RootComponent)))
                    {
                        this.SerializeControlHierarchy(manager, service, value);
                    }
                }
                CodeStatementCollection statements = obj2 as CodeStatementCollection;
                if (statements == null)
                {
                    return obj2;
                }
                Control parent = (Control) value;
                if (((service != null) && (parent == service.RootComponent)) || this.HasSitedNonReadonlyChildren(parent))
                {
                    this.SerializeSuspendLayout(manager, statements, value);
                    this.SerializeResumeLayout(manager, statements, value);
                    ControlDesigner designer = service.GetDesigner(parent) as ControlDesigner;
                    if (this.HasAutoSizedChildren(parent) || ((designer != null) && designer.SerializePerformLayout))
                    {
                        this.SerializePerformLayout(manager, statements, value);
                    }
                }
                if (this.HasMixedInheritedChildren(parent))
                {
                    this.SerializeZOrder(manager, statements, parent);
                }
            }
            return obj2;
        }

        private void SerializeControlHierarchy(IDesignerSerializationManager manager, IDesignerHost host, object value)
        {
            Control control = value as Control;
            if (control != null)
            {
                string str;
                IMultitargetHelperService service = host.GetService(typeof(IMultitargetHelperService)) as IMultitargetHelperService;
                if (control == host.RootComponent)
                {
                    str = "$this";
                    foreach (IComponent component in host.Container.Components)
                    {
                        if (!(component is Control) && !TypeDescriptor.GetAttributes(component).Contains(InheritanceAttribute.InheritedReadOnly))
                        {
                            string name = manager.GetName(component);
                            string str3 = (service == null) ? component.GetType().AssemblyQualifiedName : service.GetAssemblyQualifiedName(component.GetType());
                            if (name != null)
                            {
                                base.SerializeResourceInvariant(manager, ">>" + name + ".Name", name);
                                base.SerializeResourceInvariant(manager, ">>" + name + ".Type", str3);
                            }
                        }
                    }
                }
                else
                {
                    str = manager.GetName(value);
                    if (str == null)
                    {
                        return;
                    }
                }
                base.SerializeResourceInvariant(manager, ">>" + str + ".Name", manager.GetName(value));
                base.SerializeResourceInvariant(manager, ">>" + str + ".Type", (service == null) ? control.GetType().AssemblyQualifiedName : service.GetAssemblyQualifiedName(control.GetType()));
                Control parent = control.Parent;
                if ((parent != null) && (parent.Site != null))
                {
                    string str4;
                    if (parent == host.RootComponent)
                    {
                        str4 = "$this";
                    }
                    else
                    {
                        str4 = manager.GetName(parent);
                    }
                    if (str4 != null)
                    {
                        base.SerializeResourceInvariant(manager, ">>" + str + ".Parent", str4);
                    }
                    for (int i = 0; i < parent.Controls.Count; i++)
                    {
                        if (parent.Controls[i] == control)
                        {
                            base.SerializeResourceInvariant(manager, ">>" + str + ".ZOrder", i.ToString(CultureInfo.InvariantCulture));
                            return;
                        }
                    }
                }
            }
        }

        private void SerializeMethodInvocation(IDesignerSerializationManager manager, CodeStatementCollection statements, object control, string methodName, CodeExpressionCollection parameters, System.Type[] paramTypes, StatementOrdering ordering)
        {
            using (CodeDomSerializerBase.TraceScope("ControlCodeDomSerializer::SerializeMethodInvocation(" + methodName + ")"))
            {
                manager.GetName(control);
                paramTypes = ToTargetTypes(control, paramTypes);
                if (TypeDescriptor.GetReflectionType(control).GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, null, paramTypes, null) != null)
                {
                    CodeMethodReferenceExpression expression2 = new CodeMethodReferenceExpression(base.SerializeToExpression(manager, control), methodName);
                    CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                        Method = expression2
                    };
                    if (parameters != null)
                    {
                        expression.Parameters.AddRange(parameters);
                    }
                    CodeExpressionStatement statement = new CodeExpressionStatement(expression);
                    switch (ordering)
                    {
                        case StatementOrdering.Prepend:
                            statement.UserData["statement-ordering"] = "begin";
                            break;

                        case StatementOrdering.Append:
                            statement.UserData["statement-ordering"] = "end";
                            break;
                    }
                    statements.Add(statement);
                }
            }
        }

        private void SerializePerformLayout(IDesignerSerializationManager manager, CodeStatementCollection statements, object control)
        {
            this.SerializeMethodInvocation(manager, statements, control, "PerformLayout", null, new System.Type[0], StatementOrdering.Append);
        }

        private void SerializeResumeLayout(IDesignerSerializationManager manager, CodeStatementCollection statements, object control)
        {
            CodeExpressionCollection parameters = new CodeExpressionCollection();
            parameters.Add(new CodePrimitiveExpression(false));
            System.Type[] paramTypes = new System.Type[] { typeof(bool) };
            this.SerializeMethodInvocation(manager, statements, control, "ResumeLayout", parameters, paramTypes, StatementOrdering.Append);
        }

        private void SerializeSuspendLayout(IDesignerSerializationManager manager, CodeStatementCollection statements, object control)
        {
            this.SerializeMethodInvocation(manager, statements, control, "SuspendLayout", null, new System.Type[0], StatementOrdering.Prepend);
        }

        private void SerializeZOrder(IDesignerSerializationManager manager, CodeStatementCollection statements, Control control)
        {
            using (CodeDomSerializerBase.TraceScope("ControlCodeDomSerializer::SerializeZOrder()"))
            {
                for (int i = control.Controls.Count - 1; i >= 0; i--)
                {
                    Control component = control.Controls[i];
                    if ((component.Site != null) && (component.Site.Container == control.Site.Container))
                    {
                        InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(component)[typeof(InheritanceAttribute)];
                        if (attribute.InheritanceLevel != InheritanceLevel.InheritedReadOnly)
                        {
                            CodeExpression targetObject = new CodePropertyReferenceExpression(base.SerializeToExpression(manager, control), "Controls");
                            CodeMethodReferenceExpression expression2 = new CodeMethodReferenceExpression(targetObject, "SetChildIndex");
                            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                                Method = expression2
                            };
                            CodeExpression expression4 = base.SerializeToExpression(manager, component);
                            expression.Parameters.Add(expression4);
                            expression.Parameters.Add(base.SerializeToExpression(manager, 0));
                            CodeExpressionStatement statement = new CodeExpressionStatement(expression);
                            statements.Add(statement);
                        }
                    }
                }
            }
        }

        private static System.Type ToTargetType(object context, System.Type runtimeType)
        {
            return TypeDescriptor.GetProvider(context).GetReflectionType(runtimeType);
        }

        private static System.Type[] ToTargetTypes(object context, System.Type[] runtimeTypes)
        {
            System.Type[] typeArray = new System.Type[runtimeTypes.Length];
            for (int i = 0; i < runtimeTypes.Length; i++)
            {
                typeArray[i] = ToTargetType(context, runtimeTypes[i]);
            }
            return typeArray;
        }

        private enum StatementOrdering
        {
            Prepend,
            Append
        }
    }
}

