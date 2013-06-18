namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    internal class NonWordRegexRunner21 : RegexRunner
    {
        public override bool FindFirstChar()
        {
            int runtextpos = base.runtextpos;
            string runtext = base.runtext;
            int num3 = base.runtextend - runtextpos;
            if (num3 <= 0)
            {
                return false;
            }
            num3--;
            runtextpos++;
            if (!RegexRunner.CharInClass(runtext[runtextpos], "\x0001\0\t\0\x0002\x0004\x0005\x0003\x0001\t\x0013\0"))
            {
            }
            runtextpos--;
            base.runtextpos = runtextpos;
            return (num3 > 0);
        }

        public override void Go()
        {
            string runtext = base.runtext;
            int runtextstart = base.runtextstart;
            int runtextbeg = base.runtextbeg;
            int runtextend = base.runtextend;
            int runtextpos = base.runtextpos;
            int[] runtrack = base.runtrack;
            int runtrackpos = base.runtrackpos;
            int[] runstack = base.runstack;
            int runstackpos = base.runstackpos;
            runtrack[--runtrackpos] = runtextpos;
            runtrack[--runtrackpos] = 0;
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (runtextpos >= runtextend)
            {
                goto Label_00D8;
            }
            runtextpos++;
            if (!RegexRunner.CharInClass(runtext[runtextpos], "\x0001\0\t\0\x0002\x0004\x0005\x0003\x0001\t\x0013\0"))
            {
                goto Label_00D8;
            }
            int start = runstack[runstackpos++];
            this.Capture(0, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 2;
        Label_00CF:
            base.runtextpos = runtextpos;
            return;
        Label_00D8:
            base.runtrackpos = runtrackpos;
            base.runstackpos = runstackpos;
            this.EnsureStorage();
            runtrackpos = base.runtrackpos;
            runstackpos = base.runstackpos;
            runtrack = base.runtrack;
            runstack = base.runstack;
            switch (runtrack[runtrackpos++])
            {
                case 1:
                    runstackpos++;
                    goto Label_00D8;

                case 2:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    this.Uncapture();
                    goto Label_00D8;
            }
            runtextpos = runtrack[runtrackpos++];
            goto Label_00CF;
        }

        public override void InitTrackCount()
        {
            base.runtrackcount = 3;
        }
    }
}

