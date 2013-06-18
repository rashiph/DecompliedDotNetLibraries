namespace System.Web.RegularExpressions
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    internal class EvalExpressionRegexRunner22 : RegexRunner
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
            if (!RegexRunner.CharInClass(char.ToLower(runtext[runtextpos], CultureInfo.InvariantCulture), "\0\x0002\x0001efd"))
            {
            }
            runtextpos--;
            base.runtextpos = runtextpos;
            return (num3 > 0);
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
            if ((runtextpos > runtextbeg) && (runtext[runtextpos - 1] != '\n'))
            {
                goto Label_03E9;
            }
            int start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_00DC;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(char.ToLower(runtext[runtextpos], CultureInfo.InvariantCulture), "\0\0\x0001d"));
            runtextpos--;
        Label_00DC:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 2;
            }
        Label_010F:
            if ((((4 > (runtextend - runtextpos)) || (char.ToLower(runtext[runtextpos], CultureInfo.InvariantCulture) != 'e')) || ((char.ToLower(runtext[runtextpos + 1], CultureInfo.InvariantCulture) != 'v') || (char.ToLower(runtext[runtextpos + 2], CultureInfo.InvariantCulture) != 'a'))) || (char.ToLower(runtext[runtextpos + 3], CultureInfo.InvariantCulture) != 'l'))
            {
                goto Label_03E9;
            }
            runtextpos += 4;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_01DE;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(char.ToLower(runtext[runtextpos], CultureInfo.InvariantCulture), "\0\0\x0001d"));
            runtextpos--;
        Label_01DE:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 3;
            }
        Label_0211:
            if (runtextpos >= runtextend)
            {
                goto Label_03E9;
            }
            runtextpos++;
            if (char.ToLower(runtext[runtextpos], CultureInfo.InvariantCulture) != '(')
            {
                goto Label_03E9;
            }
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_029D;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(char.ToLower(runtext[runtextpos], CultureInfo.InvariantCulture), "\0\x0001\0\0"));
            runtextpos--;
        Label_029D:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 4;
            }
        Label_02D0:
            start = runstack[runstackpos++];
            this.Capture(1, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 5;
            if (runtextpos >= runtextend)
            {
                goto Label_03E9;
            }
            runtextpos++;
            if (char.ToLower(runtext[runtextpos], CultureInfo.InvariantCulture) != ')')
            {
                goto Label_03E9;
            }
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0374;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(char.ToLower(runtext[runtextpos], CultureInfo.InvariantCulture), "\0\0\x0001d"));
            runtextpos--;
        Label_0374:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 6;
            }
        Label_03A7:
            if (runtextpos < runtextend)
            {
                goto Label_03E9;
            }
            start = runstack[runstackpos++];
            this.Capture(0, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 5;
        Label_03E0:
            base.runtextpos = runtextpos;
            return;
        Label_03E9:
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
                    goto Label_03E9;

                case 2:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 2;
                    }
                    goto Label_010F;

                case 3:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 3;
                    }
                    goto Label_0211;

                case 4:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 4;
                    }
                    goto Label_02D0;

                case 5:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    this.Uncapture();
                    goto Label_03E9;

                case 6:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 6;
                    }
                    goto Label_03A7;
            }
            runtextpos = runtrack[runtrackpos++];
            goto Label_03E0;
        }

        public override void InitTrackCount()
        {
            base.runtrackcount = 9;
        }
    }
}

