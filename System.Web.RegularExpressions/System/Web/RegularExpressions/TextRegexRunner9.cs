namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    internal class TextRegexRunner9 : RegexRunner
    {
        public override bool FindFirstChar()
        {
            if (base.runtextpos > base.runtextstart)
            {
                base.runtextpos = base.runtextend;
                return false;
            }
            return true;
        }

        public override void Go()
        {
            int num5;
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
            if ((runtextpos != base.runtextstart) || (1 > (runtextend - runtextpos)))
            {
                goto Label_015A;
            }
            runtextpos++;
            int start = 1;
            do
            {
                start--;
                if (runtext[runtextpos - start] == '<')
                {
                    goto Label_015A;
                }
            }
            while (start > 0);
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_00EE;
                }
                runtextpos++;
            }
            while (runtext[runtextpos] != '<');
            runtextpos--;
        Label_00EE:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 2;
            }
        Label_0121:
            start = runstack[runstackpos++];
            this.Capture(0, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 3;
        Label_0151:
            base.runtextpos = runtextpos;
            return;
        Label_015A:
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
                    goto Label_015A;

                case 2:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 2;
                    }
                    goto Label_0121;

                case 3:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    this.Uncapture();
                    goto Label_015A;
            }
            runtextpos = runtrack[runtrackpos++];
            goto Label_0151;
        }

        public override void InitTrackCount()
        {
            base.runtrackcount = 4;
        }
    }
}

