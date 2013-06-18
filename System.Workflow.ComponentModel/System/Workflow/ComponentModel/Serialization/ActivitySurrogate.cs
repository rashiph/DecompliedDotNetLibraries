namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Xml;

    internal sealed class ActivitySurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            if (Activity.ContextIdToActivityMap == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_ActivitySaveLoadNotCalled"));
            }
            Activity activity = (Activity) obj;
            bool isSurrounding = false;
            bool flag2 = this.IsDanglingActivity(activity, out isSurrounding);
            if (isSurrounding)
            {
                if (activity.ContextActivity != null)
                {
                    info.AddValue("cid", activity.ContextId);
                }
                info.AddValue("id", activity.DottedPath);
                info.SetType(typeof(ActivityRef));
            }
            else if (flag2)
            {
                info.AddValue("id", activity.Name);
                info.AddValue("type", activity.GetType());
                info.SetType(typeof(DanglingActivityRef));
            }
            else
            {
                info.AddValue("id", activity.DottedPath);
                string[] names = null;
                MemberInfo[] serializableMembers = FormatterServicesNoSerializableCheck.GetSerializableMembers(obj.GetType(), out names);
                object[] objectData = FormatterServices.GetObjectData(obj, serializableMembers);
                if ((objectData == null) || (objectData.Length != 2))
                {
                    info.AddValue("memberNames", names);
                    info.AddValue("memberDatas", objectData);
                }
                else
                {
                    IDictionary<DependencyProperty, object> dictionary = (IDictionary<DependencyProperty, object>) objectData[0];
                    if ((dictionary != null) && (dictionary.Count > 0))
                    {
                        foreach (KeyValuePair<DependencyProperty, object> pair in dictionary)
                        {
                            if ((pair.Key != null) && !pair.Key.DefaultMetadata.IsNonSerialized)
                            {
                                info.AddValue("memberData", objectData[0]);
                                break;
                            }
                        }
                    }
                    if (objectData[1] != null)
                    {
                        info.AddValue("disposed", objectData[1]);
                    }
                }
                if ((obj is Activity) && (((Activity) obj).Parent == null))
                {
                    string str = activity.GetValue(Activity.WorkflowXamlMarkupProperty) as string;
                    if (!string.IsNullOrEmpty(str))
                    {
                        info.AddValue("workflowMarkup", str);
                        string str2 = activity.GetValue(Activity.WorkflowRulesMarkupProperty) as string;
                        if (!string.IsNullOrEmpty(str2))
                        {
                            info.AddValue("rulesMarkup", str2);
                        }
                    }
                    else
                    {
                        info.AddValue("type", activity.GetType());
                    }
                    Activity activity2 = (Activity) activity.GetValue(Activity.WorkflowDefinitionProperty);
                    if (activity2 != null)
                    {
                        ArrayList list = (ArrayList) activity2.GetValue(WorkflowChanges.WorkflowChangeActionsProperty);
                        if (list != null)
                        {
                            Guid guid = (Guid) activity2.GetValue(WorkflowChanges.WorkflowChangeVersionProperty);
                            info.AddValue("workflowChangeVersion", guid);
                            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
                            {
                                using (XmlWriter writer2 = Helpers.CreateXmlWriter(writer))
                                {
                                    new WorkflowMarkupSerializer().Serialize(writer2, list);
                                    info.AddValue("workflowChanges", writer.ToString());
                                }
                            }
                        }
                    }
                }
                info.SetType(typeof(ActivitySerializedRef));
            }
        }

        private bool IsDanglingActivity(Activity activity, out bool isSurrounding)
        {
            isSurrounding = false;
            bool flag = false;
            do
            {
                if (Activity.ActivityRoots.Contains(activity))
                {
                    flag = false;
                    break;
                }
                if (activity.Parent == null)
                {
                    flag = ((Activity) Activity.ActivityRoots[0]).RootActivity != activity;
                    break;
                }
                if (!activity.Parent.Activities.Contains(activity))
                {
                    IList<Activity> list = null;
                    if (activity.Parent.ContextActivity != null)
                    {
                        list = (IList<Activity>) activity.Parent.ContextActivity.GetValue(Activity.ActiveExecutionContextsProperty);
                    }
                    if ((list == null) || !list.Contains(activity))
                    {
                        flag = true;
                        break;
                    }
                }
                activity = activity.Parent;
            }
            while (activity != null);
            isSurrounding = !flag && !Activity.ActivityRoots.Contains(activity);
            return flag;
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return null;
        }

        [Serializable]
        private sealed class ActivityRef : IObjectReference
        {
            [OptionalField]
            private int cid;
            private string id = string.Empty;

            object IObjectReference.GetRealObject(StreamingContext context)
            {
                if (Activity.ContextIdToActivityMap == null)
                {
                    throw new InvalidOperationException(SR.GetString("Error_ActivitySaveLoadNotCalled"));
                }
                Activity activity = (Activity) Activity.ContextIdToActivityMap[this.cid];
                return activity.TraverseDottedPathFromRoot(this.id);
            }
        }

        [Serializable]
        private sealed class ActivitySerializedRef : IObjectReference, IDeserializationCallback
        {
            [NonSerialized]
            private Activity cachedActivity;
            [NonSerialized]
            private Activity cachedDefinitionActivity;
            [OptionalField]
            private EventHandler disposed;
            private string id = string.Empty;
            [NonSerialized]
            private int lastPosition;
            [OptionalField]
            private object memberData;
            [OptionalField]
            private object[] memberDatas;
            [OptionalField]
            private string[] memberNames;
            [OptionalField]
            private string rulesMarkup;
            [OptionalField]
            private Type type;
            [OptionalField]
            private string workflowChanges;
            [OptionalField]
            private Guid workflowChangeVersion = Guid.Empty;
            [OptionalField]
            private string workflowMarkup;

            private int Position(string name)
            {
                if ((this.memberNames.Length > 0) && this.memberNames[this.lastPosition].Equals(name))
                {
                    return this.lastPosition;
                }
                if ((++this.lastPosition < this.memberNames.Length) && this.memberNames[this.lastPosition].Equals(name))
                {
                    return this.lastPosition;
                }
                for (int i = 0; i < this.memberNames.Length; i++)
                {
                    if (this.memberNames[i].Equals(name))
                    {
                        this.lastPosition = i;
                        return this.lastPosition;
                    }
                }
                this.lastPosition = 0;
                return -1;
            }

            void IDeserializationCallback.OnDeserialization(object sender)
            {
                if (this.cachedActivity != null)
                {
                    bool flag = false;
                    string[] names = null;
                    MemberInfo[] serializableMembers = FormatterServicesNoSerializableCheck.GetSerializableMembers(this.cachedActivity.GetType(), out names);
                    if (serializableMembers.Length == 2)
                    {
                        if ((this.memberData != null) && (this.disposed != null))
                        {
                            FormatterServices.PopulateObjectMembers(this.cachedActivity, serializableMembers, new object[] { this.memberData, this.disposed });
                            flag = true;
                        }
                        else if (this.memberData != null)
                        {
                            FormatterServices.PopulateObjectMembers(this.cachedActivity, new MemberInfo[] { serializableMembers[0] }, new object[] { this.memberData });
                            flag = true;
                        }
                        else if (this.disposed != null)
                        {
                            FormatterServices.PopulateObjectMembers(this.cachedActivity, new MemberInfo[] { serializableMembers[1] }, new object[] { this.disposed });
                            flag = true;
                        }
                    }
                    if (!flag && (this.memberDatas != null))
                    {
                        object[] data = new object[serializableMembers.Length];
                        for (int i = 0; i < names.Length; i++)
                        {
                            data[i] = this.memberDatas[this.Position(names[i])];
                        }
                        FormatterServices.PopulateObjectMembers(this.cachedActivity, serializableMembers, data);
                    }
                    this.cachedActivity.FixUpMetaProperties(this.cachedDefinitionActivity);
                    this.cachedActivity = null;
                }
            }

            object IObjectReference.GetRealObject(StreamingContext context)
            {
                if (Activity.DefinitionActivity == null)
                {
                    if ((this.type == null) && string.IsNullOrEmpty(this.workflowMarkup))
                    {
                        return null;
                    }
                    Activity root = null;
                    bool createNew = this.workflowChanges != null;
                    root = Activity.OnResolveActivityDefinition(this.type, this.workflowMarkup, this.rulesMarkup, createNew, !createNew, null);
                    if (root == null)
                    {
                        throw new NullReferenceException(SR.GetString("Error_InvalidRootForWorkflowChanges"));
                    }
                    if (createNew)
                    {
                        ArrayList list = Activity.OnResolveWorkflowChangeActions(this.workflowChanges, root);
                        foreach (WorkflowChangeAction action in list)
                        {
                            action.ApplyTo(root);
                        }
                        root.SetValue(WorkflowChanges.WorkflowChangeActionsProperty, list);
                        root.SetValue(WorkflowChanges.WorkflowChangeVersionProperty, this.workflowChangeVersion);
                        ((IDependencyObjectAccessor) root).InitializeDefinitionForRuntime(null);
                    }
                    Activity.DefinitionActivity = root;
                }
                if (this.cachedActivity == null)
                {
                    this.cachedDefinitionActivity = Activity.DefinitionActivity.TraverseDottedPathFromRoot(this.id);
                    this.cachedActivity = (Activity) FormatterServices.GetUninitializedObject(this.cachedDefinitionActivity.GetType());
                }
                return this.cachedActivity;
            }
        }

        [Serializable]
        private class DanglingActivityRef : IObjectReference
        {
            [NonSerialized]
            private Activity activity;
            private string id = string.Empty;
            private Type type;

            object IObjectReference.GetRealObject(StreamingContext context)
            {
                if (this.activity == null)
                {
                    this.activity = (Activity) Activator.CreateInstance(this.type);
                    this.activity.Name = this.id;
                }
                return this.activity;
            }
        }
    }
}

