namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    internal class BrowserCapsRefRegexRunner23 : RegexRunner
    {
        public override bool FindFirstChar()
        {
            // This item is obfuscated and can not be translated.
            string runtext = base.runtext;
            int runtextend = base.runtextend;
            for (int i = base.runtextpos + 1; i < runtextend; i = 2 + i)
            {
                int num2 = runtext[i];
                if (num2 == '{')
                {
                    num2 = i;
                    if (runtext[--num2] == '$')
                    {
                        base.runtextpos = num2;
                        return true;
                    }
                }
                else
                {
                    num2 -= 0x24;
                    if (num2 <= 0x57)
                    {
                    }
                }
            }
            base.runtextpos = base.runtextend;
            return false;
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
            if (((2 > (runtextend - runtextpos)) || (runtext[runtextpos] != '$')) || (runtext[runtextpos + 1] != '{'))
            {
                goto Label_0203;
            }
            runtextpos += 2;
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (1 > (runtextend - runtextpos))
            {
                goto Label_0203;
            }
            runtextpos++;
            int start = 1;
            do
            {
                start--;
                if (!RegexRunner.CharInClass(runtext[runtextpos - start], "\0\0\t\0\x0002\x0004\x0005\x0003\x0001\t\x0013\0"))
                {
                    goto Label_0203;
                }
            }
            while (start > 0);
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0148;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\t\0\x0002\x0004\x0005\x0003\x0001\t\x0013\0"));
            runtextpos--;
        Label_0148:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 2;
            }
        Label_017B:
            start = runstack[runstackpos++];
            this.Capture(1, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 3;
            if (runtextpos >= runtextend)
            {
                goto Label_0203;
            }
            runtextpos++;
            if (runtext[runtextpos] != '}')
            {
                goto Label_0203;
            }
            start = runstack[runstackpos++];
            this.Capture(0, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 3;
        Label_01FA:
            base.runtextpos = runtextpos;
            return;
        Label_0203:
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
                    goto Label_0203;

                case 2:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 2;
                    }
                    goto Label_017B;

                case 3:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    this.Uncapture();
                    goto Label_0203;
            }
            runtextpos = runtrack[runtrackpos++];
            goto Label_01FA;
        }

        public override void InitTrackCount()
        {
            base.runtrackcount = 6;
        }
    }
}

