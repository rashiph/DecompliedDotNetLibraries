namespace System.Workflow.ComponentModel
{
    using System;
    using System.Reflection;
    using System.Runtime;

    internal class PathMemberInfoEventArgs : EventArgs
    {
        private PathWalkAction action;
        private object[] indexParameters;
        private bool lastMemberInThePath;
        private System.Reflection.MemberInfo memberInfo;
        private PathMemberKind memberKind;
        private Type parentType;
        private string path;

        public PathMemberInfoEventArgs(string path, Type parentType, System.Reflection.MemberInfo memberInfo, PathMemberKind memberKind, bool lastMemberInThePath)
        {
            this.indexParameters = new object[0];
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }
            if (parentType == null)
            {
                throw new ArgumentNullException("parentType");
            }
            if (memberInfo == null)
            {
                throw new ArgumentNullException("memberInfo");
            }
            this.path = path;
            this.parentType = parentType;
            this.memberInfo = memberInfo;
            this.memberKind = memberKind;
            this.lastMemberInThePath = lastMemberInThePath;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public PathMemberInfoEventArgs(string path, Type parentType, System.Reflection.MemberInfo memberInfo, PathMemberKind memberKind, bool lastMemberInThePath, object[] indexParameters) : this(path, parentType, memberInfo, memberKind, lastMemberInThePath)
        {
            this.indexParameters = indexParameters;
        }

        public PathWalkAction Action
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.action;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.action = value;
            }
        }

        public object[] IndexParameters
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.indexParameters;
            }
        }

        public bool LastMemberInThePath
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.lastMemberInThePath;
            }
        }

        public System.Reflection.MemberInfo MemberInfo
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.memberInfo;
            }
        }

        public PathMemberKind MemberKind
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.memberKind;
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
    }
}

