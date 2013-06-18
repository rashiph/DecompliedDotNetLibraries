namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Collections;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    internal class ActivityCollectionMarkupSerializer : CollectionMarkupSerializer
    {
        protected internal override void AddChild(WorkflowMarkupSerializationManager serializationManager, object obj, object childObj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (childObj == null)
            {
                throw new ArgumentNullException("childObj");
            }
            ActivityCollection activitys = obj as ActivityCollection;
            if (activitys == null)
            {
                throw new ArgumentException(SR.GetString("Error_SerializerTypeMismatch", new object[] { typeof(ActivityCollection).FullName }), "obj");
            }
            if (!(childObj is Activity))
            {
                throw new InvalidOperationException(SR.GetString("Error_ActivityCollectionSerializer", new object[] { childObj.GetType().FullName }));
            }
            CompositeActivity owner = activitys.Owner as CompositeActivity;
            if (owner != null)
            {
                if (Helpers.IsCustomActivity(owner))
                {
                    throw new InvalidOperationException(SR.GetString("Error_CanNotAddActivityInBlackBoxActivity"));
                }
                base.AddChild(serializationManager, obj, childObj);
            }
        }

        protected internal override void ClearChildren(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
        }

        protected internal override IList GetChildren(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            ActivityCollection activitys = obj as ActivityCollection;
            if (activitys == null)
            {
                throw new ArgumentException(SR.GetString("Error_SerializerTypeMismatch", new object[] { typeof(ActivityCollection).FullName }), "obj");
            }
            CompositeActivity owner = activitys.Owner as CompositeActivity;
            if ((owner != null) && Helpers.IsCustomActivity(owner))
            {
                return null;
            }
            return base.GetChildren(serializationManager, obj);
        }
    }
}

