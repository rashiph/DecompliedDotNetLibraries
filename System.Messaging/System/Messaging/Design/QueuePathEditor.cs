namespace System.Messaging.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Messaging;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public class QueuePathEditor : UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                IWindowsFormsEditorService service = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
                if (service == null)
                {
                    return value;
                }
                QueuePathDialog dialog = new QueuePathDialog(provider);
                MessageQueue queue = null;
                if (value is MessageQueue)
                {
                    queue = (MessageQueue) value;
                }
                else if (value is string)
                {
                    queue = new MessageQueue((string) value);
                }
                else if (value != null)
                {
                    return value;
                }
                if (queue != null)
                {
                    dialog.SelectQueue(queue);
                }
                IDesignerHost host = (IDesignerHost) provider.GetService(typeof(IDesignerHost));
                DesignerTransaction transaction = null;
                if (host != null)
                {
                    transaction = host.CreateTransaction();
                }
                try
                {
                    if ((context != null) && !context.OnComponentChanging())
                    {
                        return value;
                    }
                    if ((service.ShowDialog(dialog) != DialogResult.OK) || !(dialog.Path != string.Empty))
                    {
                        return value;
                    }
                    if ((context.Instance is MessageQueue) || (context.Instance is MessageQueueInstaller))
                    {
                        value = dialog.Path;
                    }
                    else
                    {
                        value = MessageQueueConverter.GetFromCache(dialog.Path);
                        if (value == null)
                        {
                            value = new MessageQueue(dialog.Path);
                            MessageQueueConverter.AddToCache((MessageQueue) value);
                            if (context != null)
                            {
                                context.Container.Add((IComponent) value);
                            }
                        }
                    }
                    context.OnComponentChanged();
                }
                finally
                {
                    if (transaction != null)
                    {
                        transaction.Commit();
                    }
                }
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}

