namespace System.Messaging
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Configuration.Install;
    using System.Messaging.Design;
    using System.Runtime;

    public class MessageQueueInstaller : ComponentInstaller
    {
        private bool authenticate;
        private short basePriority;
        private Guid category;
        private System.Messaging.EncryptionRequired encryptionRequired;
        private string label;
        private long maximumJournalSize;
        private long maximumQueueSize;
        private string multicastAddress;
        private string path;
        private AccessControlList permissions;
        private bool transactional;
        private System.Configuration.Install.UninstallAction uninstallAction;
        private bool useJournalQueue;

        public MessageQueueInstaller()
        {
            this.category = Guid.Empty;
            this.encryptionRequired = System.Messaging.EncryptionRequired.Optional;
            this.label = string.Empty;
            this.maximumJournalSize = 0xffffffffL;
            this.maximumQueueSize = 0xffffffffL;
            this.multicastAddress = string.Empty;
            this.path = string.Empty;
        }

        public MessageQueueInstaller(MessageQueue componentToCopy)
        {
            this.category = Guid.Empty;
            this.encryptionRequired = System.Messaging.EncryptionRequired.Optional;
            this.label = string.Empty;
            this.maximumJournalSize = 0xffffffffL;
            this.maximumQueueSize = 0xffffffffL;
            this.multicastAddress = string.Empty;
            this.path = string.Empty;
            this.InternalCopyFromComponent(componentToCopy);
        }

        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
            base.Context.LogMessage(System.Messaging.Res.GetString("ClearingQueue", new object[] { this.Path }));
            new MessageQueue(this.path).Purge();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public override void CopyFromComponent(IComponent component)
        {
            this.InternalCopyFromComponent(component);
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
            base.Context.LogMessage(System.Messaging.Res.GetString("CreatingQueue", new object[] { this.Path }));
            bool flag = MessageQueue.Exists(this.path);
            stateSaver["Exists"] = flag;
            MessageQueue queue = null;
            if (!flag)
            {
                queue = MessageQueue.Create(this.Path, this.Transactional);
            }
            else
            {
                queue = new MessageQueue(this.Path);
                stateSaver["Authenticate"] = queue.Authenticate;
                stateSaver["BasePriority"] = queue.BasePriority;
                stateSaver["Category"] = queue.Category;
                stateSaver["EncryptionRequired"] = queue.EncryptionRequired;
                stateSaver["Label"] = queue.Label;
                stateSaver["MaximumJournalSize"] = queue.MaximumJournalSize;
                stateSaver["MaximumQueueSize"] = queue.MaximumQueueSize;
                stateSaver["Path"] = queue.Path;
                stateSaver["Transactional"] = queue.Transactional;
                stateSaver["UseJournalQueue"] = queue.UseJournalQueue;
                if (MessageQueue.Msmq3OrNewer)
                {
                    stateSaver["MulticastAddress"] = queue.MulticastAddress;
                }
                if (queue.Transactional != this.Transactional)
                {
                    MessageQueue.Delete(this.Path);
                    queue = MessageQueue.Create(this.Path, this.Transactional);
                }
            }
            queue.Authenticate = this.Authenticate;
            queue.BasePriority = this.BasePriority;
            queue.Category = this.Category;
            queue.EncryptionRequired = this.EncryptionRequired;
            queue.Label = this.Label;
            queue.MaximumJournalSize = this.MaximumJournalSize;
            queue.MaximumQueueSize = this.MaximumQueueSize;
            queue.UseJournalQueue = this.UseJournalQueue;
            if (MessageQueue.Msmq3OrNewer)
            {
                queue.MulticastAddress = this.MulticastAddress;
            }
            if (this.permissions != null)
            {
                queue.SetPermissions(this.permissions);
            }
        }

        private void InternalCopyFromComponent(IComponent component)
        {
            MessageQueue queue = component as MessageQueue;
            if (queue == null)
            {
                throw new ArgumentException(System.Messaging.Res.GetString("NotAMessageQueue"));
            }
            if ((queue.Path == null) || (queue.Path == string.Empty))
            {
                throw new ArgumentException(System.Messaging.Res.GetString("IncompleteMQ"));
            }
            this.Path = queue.Path;
        }

        public override bool IsEquivalentInstaller(ComponentInstaller otherInstaller)
        {
            MessageQueueInstaller installer = otherInstaller as MessageQueueInstaller;
            if (installer == null)
            {
                return false;
            }
            return (installer.Path == this.Path);
        }

        private void RestoreQueue(IDictionary state)
        {
            bool flag = false;
            if ((state != null) && (state["Exists"] != null))
            {
                flag = (bool) state["Exists"];
            }
            else
            {
                return;
            }
            if (flag)
            {
                base.Context.LogMessage(System.Messaging.Res.GetString("RestoringQueue", new object[] { this.Path }));
                MessageQueue queue = null;
                if (!MessageQueue.Exists(this.Path))
                {
                    queue = MessageQueue.Create(this.Path, (bool) state["Transactional"]);
                }
                else
                {
                    queue = new MessageQueue(this.Path);
                    if (queue.Transactional != ((bool) state["Transactional"]))
                    {
                        MessageQueue.Delete(this.Path);
                        queue = MessageQueue.Create(this.Path, (bool) state["Transactional"]);
                    }
                }
                queue.Authenticate = (bool) state["Authenticate"];
                queue.BasePriority = (short) state["BasePriority"];
                queue.Category = (Guid) state["Category"];
                queue.EncryptionRequired = (System.Messaging.EncryptionRequired) state["EncryptionRequired"];
                queue.Label = (string) state["Label"];
                queue.MaximumJournalSize = (long) state["MaximumJournalSize"];
                queue.MaximumQueueSize = (long) state["MaximumQueueSize"];
                if (MessageQueue.Msmq3OrNewer)
                {
                    queue.MulticastAddress = (string) state["MulticastAddress"];
                }
                queue.UseJournalQueue = (bool) state["UseJournalQueue"];
                queue.ResetPermissions();
            }
            else
            {
                base.Context.LogMessage(System.Messaging.Res.GetString("RemovingQueue", new object[] { this.Path }));
                if (MessageQueue.Exists(this.path))
                {
                    MessageQueue.Delete(this.path);
                }
            }
        }

        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
            this.RestoreQueue(savedState);
        }

        private bool ShouldSerializeCategory()
        {
            return !this.Category.Equals(Guid.Empty);
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
            if (this.UninstallAction == System.Configuration.Install.UninstallAction.Remove)
            {
                base.Context.LogMessage(System.Messaging.Res.GetString("DeletingQueue", new object[] { this.Path }));
                if (MessageQueue.Exists(this.Path))
                {
                    MessageQueue.Delete(this.Path);
                }
            }
        }

        [DefaultValue(false)]
        public bool Authenticate
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.authenticate;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.authenticate = value;
            }
        }

        [DefaultValue(0)]
        public short BasePriority
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.basePriority;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.basePriority = value;
            }
        }

        [TypeConverter("System.ComponentModel.GuidConverter, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
        public Guid Category
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.category;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.category = value;
            }
        }

        [DefaultValue(1)]
        public System.Messaging.EncryptionRequired EncryptionRequired
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.encryptionRequired;
            }
            set
            {
                if (!ValidationUtility.ValidateEncryptionRequired(value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Messaging.EncryptionRequired));
                }
                this.encryptionRequired = value;
            }
        }

        [DefaultValue("")]
        public string Label
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.label;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.label = value;
            }
        }

        [TypeConverter(typeof(SizeConverter))]
        public long MaximumJournalSize
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.maximumJournalSize;
            }
            set
            {
                if (value < 0L)
                {
                    throw new ArgumentException(System.Messaging.Res.GetString("InvalidMaxJournalSize"));
                }
                this.maximumJournalSize = value;
            }
        }

        [TypeConverter(typeof(SizeConverter))]
        public long MaximumQueueSize
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.maximumQueueSize;
            }
            set
            {
                if (value < 0L)
                {
                    throw new ArgumentException(System.Messaging.Res.GetString("InvalidMaxQueueSize"));
                }
                this.maximumQueueSize = value;
            }
        }

        [DefaultValue("")]
        public string MulticastAddress
        {
            get
            {
                if (!MessageQueue.Msmq3OrNewer)
                {
                    throw new PlatformNotSupportedException(System.Messaging.Res.GetString("PlatformNotSupported"));
                }
                return this.multicastAddress;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (!MessageQueue.Msmq3OrNewer)
                {
                    throw new PlatformNotSupportedException(System.Messaging.Res.GetString("PlatformNotSupported"));
                }
                this.multicastAddress = value;
            }
        }

        [Editor("System.Messaging.Design.QueuePathEditor", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultValue("")]
        public string Path
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.path;
            }
            set
            {
                if (!MessageQueue.ValidatePath(value, true))
                {
                    throw new ArgumentException(System.Messaging.Res.GetString("PathSyntax"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.path = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AccessControlList Permissions
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.permissions;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.permissions = value;
            }
        }

        [DefaultValue(false)]
        public bool Transactional
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.transactional;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.transactional = value;
            }
        }

        [DefaultValue(0)]
        public System.Configuration.Install.UninstallAction UninstallAction
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.uninstallAction;
            }
            set
            {
                if (!Enum.IsDefined(typeof(System.Configuration.Install.UninstallAction), value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Configuration.Install.UninstallAction));
                }
                this.uninstallAction = value;
            }
        }

        [DefaultValue(false)]
        public bool UseJournalQueue
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.useJournalQueue;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.useJournalQueue = value;
            }
        }
    }
}

