namespace System.Activities.Debugger
{
    using System;
    using System.Activities;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Xaml;
    using System.Xml;

    [DebuggerNonUserCode]
    public static class SourceLocationProvider
    {
        public static void CollectMapping(Activity rootActivity1, Activity rootActivity2, Dictionary<object, SourceLocation> mapping, string path)
        {
            Activity activity = (rootActivity1.RootActivity != null) ? rootActivity1.RootActivity : rootActivity1;
            if (!activity.IsRuntimeReady)
            {
                IList<ValidationError> validationErrors = null;
                ActivityUtilities.CacheRootMetadata(activity, new ActivityLocationReferenceEnvironment(), ProcessActivityTreeOptions.ValidationOptions, null, ref validationErrors);
            }
            Activity activity2 = (rootActivity2.RootActivity != null) ? rootActivity2.RootActivity : rootActivity2;
            if (!activity2.IsRuntimeReady)
            {
                IList<ValidationError> list2 = null;
                ActivityUtilities.CacheRootMetadata(activity2, new ActivityLocationReferenceEnvironment(), ProcessActivityTreeOptions.ValidationOptions, null, ref list2);
            }
            Queue<KeyValuePair<Activity, Activity>> queue = new Queue<KeyValuePair<Activity, Activity>>();
            queue.Enqueue(new KeyValuePair<Activity, Activity>(rootActivity1, rootActivity2));
            System.Collections.Generic.HashSet<Activity> set = new System.Collections.Generic.HashSet<Activity>();
            while (queue.Count > 0)
            {
                SourceLocation location;
                KeyValuePair<Activity, Activity> pair = queue.Dequeue();
                Activity key = pair.Key;
                Activity activity4 = pair.Value;
                set.Add(key);
                if (TryGetSourceLocation(activity4, path, out location))
                {
                    mapping.Add(key, location);
                }
                else if (!(activity4 is IExpressionContainer) && !(activity4 is IValueSerializableExpression))
                {
                    Debugger.Log(2, "Workflow", "WorkflowDebugger: Does not have corresponding Xaml node for: " + activity4.DisplayName + "\n");
                }
                if ((!(key is IExpressionContainer) && !(activity4 is IExpressionContainer)) && (!(key is IValueSerializableExpression) && !(activity4 is IValueSerializableExpression)))
                {
                    IEnumerator<Activity> enumerator = WorkflowInspectionServices.GetActivities(key).GetEnumerator();
                    IEnumerator<Activity> enumerator2 = WorkflowInspectionServices.GetActivities(activity4).GetEnumerator();
                    bool flag = enumerator.MoveNext();
                    bool flag2 = enumerator2.MoveNext();
                    while (flag && flag2)
                    {
                        if (!set.Contains(enumerator.Current))
                        {
                            if (enumerator.Current.GetType() != enumerator2.Current.GetType())
                            {
                                Debugger.Log(2, "Workflow", "Unmatched type: " + enumerator.Current.GetType().FullName + " vs " + enumerator2.Current.GetType().FullName + "\n");
                            }
                            queue.Enqueue(new KeyValuePair<Activity, Activity>(enumerator.Current, enumerator2.Current));
                        }
                        flag = enumerator.MoveNext();
                        flag2 = enumerator2.MoveNext();
                    }
                    if (flag || flag2)
                    {
                        Debugger.Log(2, "Workflow", "Unmatched number of children\n");
                    }
                }
            }
        }

        private static object Deserialize(byte[] buffer, Assembly localAssembly)
        {
            object obj2;
            using (XmlReader reader = XmlReader.Create(new MemoryStream(buffer)))
            {
                XamlXmlReaderSettings settings = new XamlXmlReaderSettings {
                    LocalAssembly = localAssembly,
                    ProvideLineInfo = true
                };
                using (XamlXmlReader reader2 = new XamlXmlReader(reader, settings))
                {
                    using (XamlDebuggerXmlReader reader3 = new XamlDebuggerXmlReader(reader2, new StreamReader(new MemoryStream(buffer))))
                    {
                        using (XamlReader reader4 = ActivityXamlServices.CreateBuilderReader(reader3))
                        {
                            obj2 = XamlServices.Load(reader4);
                        }
                    }
                }
            }
            return obj2;
        }

        internal static Dictionary<object, SourceLocation> GetSourceLocations(Activity rootActivity, out string sourcePath, out bool isTemporaryFile)
        {
            sourcePath = XamlDebuggerXmlReader.GetFileName(rootActivity) as string;
            isTemporaryFile = false;
            Assembly localAssembly = rootActivity.GetType().Assembly;
            if (rootActivity.Parent != null)
            {
                localAssembly = rootActivity.Parent.GetType().Assembly;
            }
            if ((rootActivity.Children != null) && (rootActivity.Children.Count > 0))
            {
                Activity instance = rootActivity.Children[0];
                string fileName = XamlDebuggerXmlReader.GetFileName(instance) as string;
                if (!string.IsNullOrEmpty(fileName))
                {
                    rootActivity = instance;
                    sourcePath = fileName;
                }
            }
            if (!string.IsNullOrEmpty(sourcePath))
            {
                SourceLocation location;
                Activity workflowRoot;
                if (TryGetSourceLocation(rootActivity, sourcePath, out location))
                {
                    workflowRoot = rootActivity;
                }
                else
                {
                    FileInfo info = new FileInfo(sourcePath);
                    byte[] buffer = new byte[info.Length];
                    using (FileStream stream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
                    {
                        stream.Read(buffer, 0, buffer.Length);
                    }
                    object obj2 = Deserialize(buffer, localAssembly);
                    IDebuggableWorkflowTree tree = obj2 as IDebuggableWorkflowTree;
                    if (tree != null)
                    {
                        workflowRoot = tree.GetWorkflowRoot();
                    }
                    else
                    {
                        workflowRoot = obj2 as Activity;
                    }
                }
                Dictionary<object, SourceLocation> mapping = new Dictionary<object, SourceLocation>();
                if (workflowRoot != null)
                {
                    CollectMapping(rootActivity, workflowRoot, mapping, sourcePath);
                }
                return mapping;
            }
            string tempFileName = Path.GetTempFileName();
            sourcePath = Path.ChangeExtension(tempFileName, ".xaml");
            File.Move(tempFileName, sourcePath);
            isTemporaryFile = true;
            return PublishAndCollectMapping(rootActivity, sourcePath);
        }

        private static Dictionary<object, SourceLocation> PublishAndCollectMapping(Activity activity, string path)
        {
            Dictionary<object, SourceLocation> mapping = new Dictionary<object, SourceLocation>();
            using (MemoryStream stream = new MemoryStream())
            {
                Serialize(stream, activity);
                stream.Position = 0L;
                Activity activity2 = Deserialize(stream.GetBuffer(), activity.GetType().Assembly) as Activity;
                using (StreamWriter writer = new StreamWriter(path))
                {
                    stream.Position = 0L;
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        writer.Write(reader.ReadToEnd());
                    }
                }
                CollectMapping(activity, activity2, mapping, path);
            }
            return mapping;
        }

        private static void Serialize(Stream writeStream, Activity activity)
        {
            XmlWriterSettings settings = new XmlWriterSettings {
                Indent = true,
                IndentChars = "    "
            };
            using (XmlWriter writer = XmlWriter.Create(writeStream, settings))
            {
                XamlServices.Save(writer, activity);
                writer.Flush();
            }
        }

        private static bool TryGetSourceLocation(object obj, string path, out SourceLocation sourceLocation)
        {
            int num;
            int num2;
            int num3;
            int num4;
            sourceLocation = null;
            if (((AttachablePropertyServices.TryGetProperty<int>(obj, XamlDebuggerXmlReader.StartLineName, out num) && AttachablePropertyServices.TryGetProperty<int>(obj, XamlDebuggerXmlReader.StartColumnName, out num2)) && (AttachablePropertyServices.TryGetProperty<int>(obj, XamlDebuggerXmlReader.EndLineName, out num3) && AttachablePropertyServices.TryGetProperty<int>(obj, XamlDebuggerXmlReader.EndColumnName, out num4))) && SourceLocation.IsValidRange(num, num2, num3, num4))
            {
                sourceLocation = new SourceLocation(path, num, num2, num3, num4);
                return true;
            }
            return false;
        }
    }
}

