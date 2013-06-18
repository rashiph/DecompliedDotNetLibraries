namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Reflection;

    internal sealed class EventMemberCodeDomSerializer : MemberCodeDomSerializer
    {
        private static EventMemberCodeDomSerializer _default;
        private static CodeThisReferenceExpression _thisRef = new CodeThisReferenceExpression();

        public override void Serialize(IDesignerSerializationManager manager, object value, MemberDescriptor descriptor, CodeStatementCollection statements)
        {
            EventDescriptor e = descriptor as EventDescriptor;
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (e == null)
            {
                throw new ArgumentNullException("descriptor");
            }
            if (statements == null)
            {
                throw new ArgumentNullException("statements");
            }
            try
            {
                IEventBindingService service = (IEventBindingService) manager.GetService(typeof(IEventBindingService));
                if (service != null)
                {
                    string methodName = (string) service.GetEventProperty(e).GetValue(value);
                    if (methodName != null)
                    {
                        CodeExpression targetObject = base.SerializeToExpression(manager, value);
                        if (targetObject != null)
                        {
                            CodeTypeReference delegateType = new CodeTypeReference(e.EventType);
                            CodeDelegateCreateExpression listener = new CodeDelegateCreateExpression(delegateType, _thisRef, methodName);
                            CodeEventReferenceExpression eventRef = new CodeEventReferenceExpression(targetObject, e.Name);
                            CodeAttachEventStatement statement = new CodeAttachEventStatement(eventRef, listener);
                            statement.UserData[typeof(Delegate)] = e.EventType;
                            statements.Add(statement);
                        }
                    }
                }
            }
            catch (Exception innerException)
            {
                if (innerException is TargetInvocationException)
                {
                    innerException = innerException.InnerException;
                }
                manager.ReportError(System.Design.SR.GetString("SerializerPropertyGenFailed", new object[] { e.Name, innerException.Message }));
            }
        }

        public override bool ShouldSerialize(IDesignerSerializationManager manager, object value, MemberDescriptor descriptor)
        {
            return true;
        }

        internal static EventMemberCodeDomSerializer Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new EventMemberCodeDomSerializer();
                }
                return _default;
            }
        }
    }
}

