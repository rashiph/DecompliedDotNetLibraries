namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.ComponentModel.Design.Serialization;
    using System.IO;
    using System.Text;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Xml;

    [DefaultSerializationProvider(typeof(ActivityMarkupSerializationProvider))]
    public class ActivityMarkupSerializer : WorkflowMarkupSerializer
    {
        public static readonly DependencyProperty EndColumnProperty = DependencyProperty.RegisterAttached("EndColumn", typeof(int), typeof(ActivityMarkupSerializer), new PropertyMetadata(-1, new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        public static readonly DependencyProperty EndLineProperty = DependencyProperty.RegisterAttached("EndLine", typeof(int), typeof(ActivityMarkupSerializer), new PropertyMetadata(-1, new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        private const int minusOne = -1;
        public static readonly DependencyProperty StartColumnProperty = DependencyProperty.RegisterAttached("StartColumn", typeof(int), typeof(ActivityMarkupSerializer), new PropertyMetadata(-1, new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        public static readonly DependencyProperty StartLineProperty = DependencyProperty.RegisterAttached("StartLine", typeof(int), typeof(ActivityMarkupSerializer), new PropertyMetadata(-1, new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));

        protected override object CreateInstance(WorkflowMarkupSerializationManager serializationManager, Type type)
        {
            XmlReader reader = serializationManager.WorkflowMarkupStack[typeof(XmlReader)] as XmlReader;
            if (reader == null)
            {
                return null;
            }
            object obj2 = base.CreateInstance(serializationManager, type);
            if (((obj2 is Activity) && (serializationManager.Context[typeof(Activity)] == null)) && (serializationManager.Context[typeof(WorkflowCompilerParameters)] != null))
            {
                (obj2 as Activity).UserData[UserDataKeys.CustomActivity] = false;
            }
            WorkflowMarkupSourceAttribute[] customAttributes = (WorkflowMarkupSourceAttribute[]) type.GetCustomAttributes(typeof(WorkflowMarkupSourceAttribute), false);
            if (((obj2 is CompositeActivity) && (customAttributes.Length > 0)) && (type.Assembly == serializationManager.LocalAssembly))
            {
                object obj3 = null;
                using (XmlReader reader2 = XmlReader.Create(customAttributes[0].FileName))
                {
                    obj3 = base.Deserialize(serializationManager, reader2);
                }
                ReplaceChildActivities(obj2 as CompositeActivity, obj3 as CompositeActivity);
            }
            if (obj2 is Activity)
            {
                int num6;
                int num = (reader is IXmlLineInfo) ? ((IXmlLineInfo) reader).LineNumber : 1;
                int num2 = (reader is IXmlLineInfo) ? ((IXmlLineInfo) reader).LinePosition : 1;
                int num3 = num - 1;
                int num4 = num2 - 1;
                bool flag = false;
                while (reader.MoveToNextAttribute())
                {
                    flag = true;
                }
                int num5 = num - 1;
                if (flag)
                {
                    reader.ReadAttributeValue();
                    num6 = num2 + reader.Value.Length;
                }
                else
                {
                    num6 = (num2 + reader.Name.Length) - 1;
                }
                reader.MoveToElement();
                Activity activity = (Activity) obj2;
                activity.SetValue(StartLineProperty, num3);
                activity.SetValue(StartColumnProperty, num4);
                activity.SetValue(EndLineProperty, num5);
                activity.SetValue(EndColumnProperty, num6);
            }
            return obj2;
        }

        protected override void OnAfterSerialize(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            Activity activity = obj as Activity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(Activity).FullName }), "obj");
            }
            XmlWriter writer = serializationManager.WorkflowMarkupStack[typeof(XmlWriter)] as XmlWriter;
            if (writer != null)
            {
                StringWriter writer2 = serializationManager.WorkflowMarkupStack[typeof(StringWriter)] as StringWriter;
                if (writer2 != null)
                {
                    string str = writer2.ToString();
                    int startIndex = (int) activity.GetValue(EndColumnProperty);
                    int num2 = str.IndexOf(writer2.NewLine, startIndex, StringComparison.Ordinal);
                    if (num2 == -1)
                    {
                        activity.SetValue(EndColumnProperty, (str.Length - startIndex) - 1);
                    }
                    else
                    {
                        activity.SetValue(EndColumnProperty, num2 - startIndex);
                    }
                }
                CodeTypeMemberCollection members = activity.GetValue(WorkflowMarkupSerializer.XCodeProperty) as CodeTypeMemberCollection;
                if (members != null)
                {
                    foreach (CodeSnippetTypeMember member in members)
                    {
                        if (member.Text != null)
                        {
                            writer.WriteStartElement("x", "Code", "http://schemas.microsoft.com/winfx/2006/xaml");
                            int writerDepth = serializationManager.WriterDepth;
                            StringBuilder builder = new StringBuilder();
                            if (member.UserData.Contains(UserDataKeys.CodeSegment_New))
                            {
                                builder.AppendLine();
                                foreach (string str2 in member.Text.Trim().Split(new string[] { "\r\n" }, StringSplitOptions.None))
                                {
                                    builder.Append(writer.Settings.IndentChars);
                                    builder.Append(str2);
                                    builder.AppendLine();
                                }
                                builder.Append(writer.Settings.IndentChars);
                            }
                            else
                            {
                                builder.Append(member.Text);
                            }
                            writer.WriteCData(builder.ToString());
                            writer.WriteEndElement();
                        }
                    }
                }
            }
        }

        protected override void OnBeforeSerialize(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            Activity activity = obj as Activity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(Activity).FullName }), "obj");
            }
            XmlWriter writer = serializationManager.WorkflowMarkupStack[typeof(XmlWriter)] as XmlWriter;
            if (writer != null)
            {
                StringWriter writer2 = serializationManager.WorkflowMarkupStack[typeof(StringWriter)] as StringWriter;
                if (writer2 != null)
                {
                    writer.Flush();
                    string str = writer2.ToString();
                    int num = 0;
                    int startIndex = 0;
                    string newLine = writer2.NewLine;
                    int length = newLine.Length;
                    while (true)
                    {
                        int num4 = str.IndexOf(newLine, startIndex, StringComparison.Ordinal);
                        if (num4 == -1)
                        {
                            break;
                        }
                        startIndex = num4 + length;
                        num++;
                    }
                    activity.SetValue(StartLineProperty, num);
                    activity.SetValue(EndLineProperty, num);
                    activity.SetValue(EndColumnProperty, startIndex);
                    activity.SetValue(StartColumnProperty, (str.IndexOf('<', startIndex) - startIndex) + 1);
                }
                string str3 = activity.GetValue(WorkflowMarkupSerializer.XClassProperty) as string;
                if (str3 != null)
                {
                    writer.WriteAttributeString("x", "Class", "http://schemas.microsoft.com/winfx/2006/xaml", str3);
                }
            }
        }

        internal static void ReplaceChildActivities(CompositeActivity instanceActivity, CompositeActivity xomlActivity)
        {
            ArrayList list = new ArrayList();
            foreach (Activity activity in xomlActivity.Activities)
            {
                list.Add(activity);
            }
            try
            {
                instanceActivity.CanModifyActivities = true;
                xomlActivity.CanModifyActivities = true;
                instanceActivity.Activities.Clear();
                xomlActivity.Activities.Clear();
                foreach (Activity activity2 in list)
                {
                    instanceActivity.Activities.Add(activity2);
                }
            }
            finally
            {
                instanceActivity.CanModifyActivities = false;
                xomlActivity.CanModifyActivities = false;
            }
            if (!instanceActivity.UserData.Contains(UserDataKeys.CustomActivity))
            {
                instanceActivity.UserData[UserDataKeys.CustomActivity] = instanceActivity.Activities.Count > 0;
            }
        }
    }
}

