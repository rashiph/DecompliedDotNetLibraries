namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    internal class GTRegexRunner10 : RegexRunner
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
            if (!RegexRunner.CharInClass(runtext[runtextpos], "\0\x0003\0\0%&"))
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
                goto Label_00EF;
            }
            runtextpos++;
            if ((runtext[runtextpos] == '%') || (runtextpos >= runtextend))
            {
                goto Label_00EF;
            }
            runtextpos++;
            if (runtext[runtextpos] != '>')
            {
                goto Label_00EF;
            }
            int start = runstack[runstackpos++];
            this.Capture(0, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 2;
        Label_00E6:
            base.runtextpos = runtextpos;
            return;
        Label_00EF:
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
                    goto Label_00EF;

                case 2:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    this.Uncapture();
                    goto Label_00EF;
            }
            runtextpos = runtrack[runtrackpos++];
            goto Label_00E6;
        }

        public override void InitTrackCount()
        {
            base.runtrackcount = 3;
        }
    }
}

