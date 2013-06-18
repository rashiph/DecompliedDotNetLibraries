namespace System.Messaging
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, AttributeUsage(AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false)]
    public class MessageQueuePermissionAttribute : CodeAccessSecurityAttribute
    {
        private string category;
        private string label;
        private string machineName;
        private string path;
        private MessageQueuePermissionAccess permissionAccess;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public MessageQueuePermissionAttribute(SecurityAction action) : base(action)
        {
        }

        private Exception CheckProperties()
        {
            if ((this.path != null) && (((this.machineName != null) || (this.label != null)) || (this.category != null)))
            {
                return new InvalidOperationException(Res.GetString("PermissionPathOrCriteria"));
            }
            if (((this.path == null) && (this.machineName == null)) && ((this.label == null) && (this.category == null)))
            {
                return new InvalidOperationException(Res.GetString("PermissionAllNull"));
            }
            return null;
        }

        public override IPermission CreatePermission()
        {
            if (base.Unrestricted)
            {
                return new MessageQueuePermission(PermissionState.Unrestricted);
            }
            this.CheckProperties();
            if (this.path != null)
            {
                return new MessageQueuePermission(this.PermissionAccess, this.path);
            }
            return new MessageQueuePermission(this.PermissionAccess, this.machineName, this.label, this.category);
        }

        public string Category
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.category;
            }
            set
            {
                string category = this.category;
                this.category = value;
                Exception exception = this.CheckProperties();
                if (exception != null)
                {
                    this.category = category;
                    throw exception;
                }
            }
        }

        public string Label
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.label;
            }
            set
            {
                string label = this.label;
                this.label = value;
                Exception exception = this.CheckProperties();
                if (exception != null)
                {
                    this.label = label;
                    throw exception;
                }
            }
        }

        public string MachineName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.machineName;
            }
            set
            {
                if ((value != null) && !SyntaxCheck.CheckMachineName(value))
                {
                    throw new ArgumentException(Res.GetString("InvalidProperty", new object[] { "MachineName", value }));
                }
                string machineName = this.machineName;
                this.machineName = value;
                Exception exception = this.CheckProperties();
                if (exception != null)
                {
                    this.machineName = machineName;
                    throw exception;
                }
            }
        }

        public string Path
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.path;
            }
            set
            {
                if (((value != null) && (value != "*")) && !MessageQueue.ValidatePath(value, false))
                {
                    throw new ArgumentException(Res.GetString("PathSyntax"));
                }
                string path = this.path;
                this.path = value;
                Exception exception = this.CheckProperties();
                if (exception != null)
                {
                    this.path = path;
                    throw exception;
                }
            }
        }

        public MessageQueuePermissionAccess PermissionAccess
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.permissionAccess;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.permissionAccess = value;
            }
        }
    }
}

