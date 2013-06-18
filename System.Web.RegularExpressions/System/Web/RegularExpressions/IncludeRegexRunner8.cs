namespace System.Web.RegularExpressions
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    internal class IncludeRegexRunner8 : RegexRunner
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
            int num1;
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
            if ((((runtextpos != base.runtextstart) || (4 > (runtextend - runtextpos))) || ((runtext[runtextpos] != '<') || (runtext[runtextpos + 1] != '!'))) || ((runtext[runtextpos + 2] != '-') || (runtext[runtextpos + 3] != '-')))
            {
                goto Label_0730;
            }
            runtextpos += 4;
            int start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0122;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_0122:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 2;
            }
        Label_0155:
            if (runtextpos >= runtextend)
            {
                goto Label_0730;
            }
            runtextpos++;
            if (((((runtext[runtextpos] != '#') || (7 > (runtextend - runtextpos))) || ((char.ToLower(runtext[runtextpos], CultureInfo.CurrentCulture) != 'i') || (char.ToLower(runtext[runtextpos + 1], CultureInfo.CurrentCulture) != 'n'))) || (((char.ToLower(runtext[runtextpos + 2], CultureInfo.CurrentCulture) != 'c') || (char.ToLower(runtext[runtextpos + 3], CultureInfo.CurrentCulture) != 'l')) || ((char.ToLower(runtext[runtextpos + 4], CultureInfo.CurrentCulture) != 'u') || (char.ToLower(runtext[runtextpos + 5], CultureInfo.CurrentCulture) != 'd')))) || (char.ToLower(runtext[runtextpos + 6], CultureInfo.CurrentCulture) != 'e'))
            {
                goto Label_0730;
            }
            runtextpos += 7;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0290;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_0290:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 3;
            }
        Label_02C3:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (1 > (runtextend - runtextpos))
            {
                goto Label_0730;
            }
            runtextpos++;
            start = 1;
            do
            {
                start--;
                if (!RegexRunner.CharInClass(runtext[runtextpos - start], "\0\0\t\0\x0002\x0004\x0005\x0003\x0001\t\x0013\0"))
                {
                    goto Label_0730;
                }
            }
            while (start > 0);
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_035D;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\t\0\x0002\x0004\x0005\x0003\x0001\t\x0013\0"));
            runtextpos--;
        Label_035D:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 4;
            }
        Label_0390:
            start = runstack[runstackpos++];
            this.Capture(1, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 5;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0401;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_0401:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 6;
            }
        Label_0434:
            if (runtextpos >= runtextend)
            {
                goto Label_0730;
            }
            runtextpos++;
            if (runtext[runtextpos] != '=')
            {
                goto Label_0730;
            }
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0494;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_0494:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 7;
            }
        Label_04C7:
            num1 = runtextend - runtextpos;
            if (num1 >= 1)
            {
            }
            start = (num5 = 1) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0510;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\x0004\0\"#'("));
            runtextpos--;
        Label_0510:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 8;
            }
        Label_0543:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            start = runtextend - runtextpos;
            if (start > 0)
            {
                runtrack[--runtrackpos] = start - 1;
                runtrack[--runtrackpos] = runtextpos;
                runtrack[--runtrackpos] = 9;
            }
        Label_058E:
            start = runstack[runstackpos++];
            this.Capture(2, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 5;
            int num10 = runtextend - runtextpos;
            if (num10 >= 1)
            {
            }
            start = (num5 = 1) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0607;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\x0004\0\"#'("));
            runtextpos--;
        Label_0607:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 10;
            }
        Label_063A:
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_067B;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_067B:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 11;
            }
        Label_06AE:
            if (((3 > (runtextend - runtextpos)) || (runtext[runtextpos] != '-')) || ((runtext[runtextpos + 1] != '-') || (runtext[runtextpos + 2] != '>')))
            {
                goto Label_0730;
            }
            runtextpos += 3;
            start = runstack[runstackpos++];
            this.Capture(0, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 5;
        Label_0727:
            base.runtextpos = runtextpos;
            return;
        Label_0730:
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
                    goto Label_0730;

                case 2:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 2;
                    }
                    goto Label_0155;

                case 3:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 3;
                    }
                    goto Label_02C3;

                case 4:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 4;
                    }
                    goto Label_0390;

                case 5:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    this.Uncapture();
                    goto Label_0730;

                case 6:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 6;
                    }
                    goto Label_0434;

                case 7:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 7;
                    }
                    goto Label_04C7;

                case 8:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 8;
                    }
                    goto Label_0543;

                case 9:
                    runtextpos = runtrack[runtrackpos++];
                    num5 = runtrack[runtrackpos++];
                    runtextpos++;
                    if (!RegexRunner.CharInClass(runtext[runtextpos], "\x0001\x0004\0\"#'("))
                    {
                        goto Label_0730;
                    }
                    if (num5 > 0)
                    {
                        runtrack[--runtrackpos] = num5 - 1;
                        runtrack[--runtrackpos] = runtextpos;
                        runtrack[--runtrackpos] = 9;
                    }
                    goto Label_058E;

                case 10:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 10;
                    }
                    goto Label_063A;

                case 11:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 11;
                    }
                    goto Label_06AE;
            }
            runtextpos = runtrack[runtrackpos++];
            goto Label_0727;
        }

        public override void InitTrackCount()
        {
            base.runtrackcount = 0x10;
        }
    }
}

