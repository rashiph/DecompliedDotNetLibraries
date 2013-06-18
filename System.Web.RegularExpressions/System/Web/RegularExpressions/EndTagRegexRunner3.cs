namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    internal class EndTagRegexRunner3 : RegexRunner
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
            if (((runtextpos != base.runtextstart) || (2 > (runtextend - runtextpos))) || ((runtext[runtextpos] != '<') || (runtext[runtextpos + 1] != '/')))
            {
                goto Label_0284;
            }
            runtextpos += 2;
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (1 > (runtextend - runtextpos))
            {
                goto Label_0284;
            }
            runtextpos++;
            int start = 1;
            do
            {
                start--;
                if (!RegexRunner.CharInClass(runtext[runtextpos - start], "\0\x0004\t./:;\0\x0002\x0004\x0005\x0003\x0001\t\x0013\0"))
                {
                    goto Label_0284;
                }
            }
            while (start > 0);
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0155;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\x0004\t./:;\0\x0002\x0004\x0005\x0003\x0001\t\x0013\0"));
            runtextpos--;
        Label_0155:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 2;
            }
        Label_0188:
            start = runstack[runstackpos++];
            this.Capture(1, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 3;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_01F9;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_01F9:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 4;
            }
        Label_022C:
            if (runtextpos >= runtextend)
            {
                goto Label_0284;
            }
            runtextpos++;
            if (runtext[runtextpos] != '>')
            {
                goto Label_0284;
            }
            start = runstack[runstackpos++];
            this.Capture(0, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 3;
        Label_027B:
            base.runtextpos = runtextpos;
            return;
        Label_0284:
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
                    goto Label_0284;

                case 2:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 2;
                    }
                    goto Label_0188;

                case 3:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    this.Uncapture();
                    goto Label_0284;

                case 4:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 4;
                    }
                    goto Label_022C;
            }
            runtextpos = runtrack[runtrackpos++];
            goto Label_027B;
        }

        public override void InitTrackCount()
        {
            base.runtrackcount = 7;
        }
    }
}

