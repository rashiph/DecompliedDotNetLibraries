namespace System.Web.Services.Protocols
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.All)]
    public sealed class MatchAttribute : Attribute
    {
        private int capture;
        private int group = 1;
        private bool ignoreCase;
        private string pattern;
        private int repeats = -1;

        public MatchAttribute(string pattern)
        {
            this.pattern = pattern;
        }

        public int Capture
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.capture;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.capture = value;
            }
        }

        public int Group
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.group;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.group = value;
            }
        }

        public bool IgnoreCase
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.ignoreCase;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.ignoreCase = value;
            }
        }

        public int MaxRepeats
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.repeats;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.repeats = value;
            }
        }

        public string Pattern
        {
            get
            {
                if (this.pattern != null)
                {
                    return this.pattern;
                }
                return string.Empty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.pattern = value;
            }
        }
    }
}

