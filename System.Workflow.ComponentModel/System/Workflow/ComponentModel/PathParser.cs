namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    internal sealed class PathParser
    {
        private List<SourceValueInfo> al = new List<SourceValueInfo>();
        private DrillIn drillIn;
        private static List<SourceValueInfo> EmptyInfo = new List<SourceValueInfo>(1);
        private string error = string.Empty;
        private int index;
        private const char NullChar = '\0';
        private int pathLength;
        private string pathValue;
        private static string SpecialChars = ".[]";
        private State state;

        private void AddIndexer()
        {
            int index = this.index;
            int num2 = 1;
            while (num2 > 0)
            {
                if (this.index >= this.pathLength)
                {
                    return;
                }
                if (this.pathValue[this.index] == '[')
                {
                    num2++;
                }
                else if (this.pathValue[this.index] == ']')
                {
                    num2--;
                }
                this.index++;
            }
            string n = this.pathValue.Substring(index, (this.index - index) - 1).Trim();
            SourceValueInfo item = new SourceValueInfo(SourceValueType.Indexer, this.drillIn, n);
            this.al.Add(item);
            this.StartNewLevel();
        }

        private void AddProperty()
        {
            int index = this.index;
            while ((this.index < this.pathLength) && (SpecialChars.IndexOf(this.pathValue[this.index]) < 0))
            {
                this.index++;
            }
            string n = this.pathValue.Substring(index, this.index - index).Trim();
            SourceValueInfo item = new SourceValueInfo(SourceValueType.Property, this.drillIn, n);
            this.al.Add(item);
            this.StartNewLevel();
        }

        internal List<SourceValueInfo> Parse(string path, bool returnResultBeforeError)
        {
            this.pathValue = (path != null) ? path.Trim() : string.Empty;
            this.pathLength = this.pathValue.Length;
            this.index = 0;
            this.drillIn = DrillIn.IfNeeded;
            this.al.Clear();
            this.error = null;
            this.state = State.Init;
            if ((this.pathLength > 0) && (this.pathValue[0] == '.'))
            {
                SourceValueInfo item = new SourceValueInfo(SourceValueType.Property, this.drillIn, string.Empty);
                this.al.Add(item);
            }
            while (this.state != State.Done)
            {
                bool flag;
                char ch = (this.index < this.pathLength) ? this.pathValue[this.index] : '\0';
                switch (this.state)
                {
                    case State.Init:
                        switch (ch)
                        {
                            case ']':
                                goto Label_010C;
                        }
                        goto Label_015C;

                    case State.Prop:
                        flag = false;
                        switch (ch)
                        {
                            case '\0':
                                goto Label_018C;

                            case '.':
                                goto Label_017F;

                            case '[':
                                goto Label_0188;
                        }
                        goto Label_019C;

                    default:
                    {
                        continue;
                    }
                }
                this.state = State.Prop;
                continue;
            Label_010C:;
                this.error = string.Concat(new object[] { "path[", this.index, "] = ", ch });
                if (!returnResultBeforeError)
                {
                    return EmptyInfo;
                }
                return this.al;
            Label_015C:
                this.AddProperty();
                continue;
            Label_017F:
                this.drillIn = DrillIn.Never;
                goto Label_01EC;
            Label_0188:
                flag = true;
                goto Label_01EC;
            Label_018C:
                this.index--;
                goto Label_01EC;
            Label_019C:;
                this.error = string.Concat(new object[] { "path[", this.index, "] = ", ch });
                if (!returnResultBeforeError)
                {
                    return EmptyInfo;
                }
                return this.al;
            Label_01EC:
                this.index++;
                if (flag)
                {
                    this.AddIndexer();
                }
                else
                {
                    this.AddProperty();
                }
            }
            if ((this.error != null) && !returnResultBeforeError)
            {
                return EmptyInfo;
            }
            return this.al;
        }

        private void StartNewLevel()
        {
            if (this.index >= this.pathLength)
            {
                this.state = State.Done;
            }
            this.drillIn = DrillIn.Never;
        }

        internal string Error
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.error;
            }
        }

        private enum State
        {
            Init,
            Prop,
            Done
        }
    }
}

