namespace System.Messaging
{
    using System;
    using System.ComponentModel;
    using System.Runtime;

    [Serializable]
    public class MessageQueuePermissionEntry
    {
        private string category;
        private string label;
        private string machineName;
        private string path;
        private MessageQueuePermissionAccess permissionAccess;

        public MessageQueuePermissionEntry(MessageQueuePermissionAccess permissionAccess, string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if ((path != "*") && !MessageQueue.ValidatePath(path, false))
            {
                throw new ArgumentException(Res.GetString("PathSyntax"));
            }
            this.path = path;
            this.permissionAccess = permissionAccess;
        }

        public MessageQueuePermissionEntry(MessageQueuePermissionAccess permissionAccess, string machineName, string label, string category)
        {
            if (((machineName == null) && (label == null)) && (category == null))
            {
                throw new ArgumentNullException("machineName");
            }
            if ((machineName != null) && !SyntaxCheck.CheckMachineName(machineName))
            {
                throw new ArgumentException(Res.GetString("InvalidParameter", new object[] { "MachineName", machineName }));
            }
            this.permissionAccess = permissionAccess;
            this.machineName = machineName;
            this.label = label;
            this.category = category;
        }

        public string Category
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.category;
            }
        }

        public string Label
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.label;
            }
        }

        public string MachineName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.machineName;
            }
        }

        public string Path
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.path;
            }
        }

        public MessageQueuePermissionAccess PermissionAccess
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.permissionAccess;
            }
        }
    }
}

