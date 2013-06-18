namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;
    using System.Workflow.Runtime;
    using System.Xml;

    public class DefaultWorkflowLoaderService : WorkflowLoaderService
    {
        protected internal override Activity CreateInstance(Type workflowType)
        {
            if (workflowType == null)
            {
                throw new ArgumentNullException("workflowType");
            }
            if (!typeof(Activity).IsAssignableFrom(workflowType))
            {
                throw new ArgumentException(ExecutionStringManager.TypeMustImplementRootActivity);
            }
            if (workflowType.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new ArgumentException(ExecutionStringManager.TypeMustHavePublicDefaultConstructor);
            }
            return (Activator.CreateInstance(workflowType) as Activity);
        }

        protected internal override Activity CreateInstance(XmlReader workflowDefinitionReader, XmlReader rulesReader)
        {
            if (workflowDefinitionReader == null)
            {
                throw new ArgumentNullException("workflowDefinitionReader");
            }
            Activity activity = null;
            ValidationErrorCollection errors = new ValidationErrorCollection();
            ServiceContainer container = new ServiceContainer();
            ITypeProvider service = base.Runtime.GetService<ITypeProvider>();
            if (service != null)
            {
                container.AddService(typeof(ITypeProvider), service);
            }
            DesignerSerializationManager manager = new DesignerSerializationManager(container);
            try
            {
                using (manager.CreateSession())
                {
                    WorkflowMarkupSerializationManager serializationManager = new WorkflowMarkupSerializationManager(manager);
                    activity = new WorkflowMarkupSerializer().Deserialize(serializationManager, workflowDefinitionReader) as Activity;
                    if ((activity != null) && (rulesReader != null))
                    {
                        object obj2 = new WorkflowMarkupSerializer().Deserialize(serializationManager, rulesReader);
                        activity.SetValue(ConditionTypeConverter.DeclarativeConditionDynamicProp, obj2);
                    }
                    foreach (object obj3 in manager.Errors)
                    {
                        if (obj3 is WorkflowMarkupSerializationException)
                        {
                            errors.Add(new ValidationError(((WorkflowMarkupSerializationException) obj3).Message, 0x15b));
                        }
                        else
                        {
                            errors.Add(new ValidationError(obj3.ToString(), 0x15b));
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                errors.Add(new ValidationError(exception.Message, 0x15b));
            }
            if (errors.HasErrors)
            {
                throw new WorkflowValidationFailedException(ExecutionStringManager.WorkflowValidationFailure, errors);
            }
            return activity;
        }
    }
}

