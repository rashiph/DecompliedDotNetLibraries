namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.CodeDom;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    public interface IMemberCreationService
    {
        void CreateEvent(string className, string eventName, Type eventType, AttributeInfo[] attributes, bool emitDependencyProperty);
        void CreateField(string className, string fieldName, Type fieldType, Type[] genericParameterTypes, MemberAttributes attributes, CodeSnippetExpression initializationExpression, bool overwriteExisting);
        void CreateProperty(string className, string propertyName, Type propertyType, AttributeInfo[] attributes, bool emitDependencyProperty, bool isMetaProperty, bool isAttached, Type ownerType, bool isReadOnly);
        void RemoveEvent(string className, string eventName, Type eventType);
        void RemoveProperty(string className, string propertyName, Type propertyType);
        void ShowCode();
        void ShowCode(Activity activity, string methodName, Type delegateType);
        void UpdateBaseType(string className, Type baseType);
        void UpdateEvent(string className, string oldEventName, Type oldEventType, string newEventName, Type newEventType, AttributeInfo[] attributes, bool emitDependencyProperty, bool isMetaProperty);
        void UpdateProperty(string className, string oldPropertyName, Type oldPropertyType, string newPropertyName, Type newPropertyType, AttributeInfo[] attributes, bool emitDependencyProperty, bool isMetaProperty);
        void UpdateTypeName(string oldClassName, string newClassName);
    }
}

