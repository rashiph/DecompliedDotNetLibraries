namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    internal class TagRegexRunner1 : RegexRunner
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
            if ((runtextpos != base.runtextstart) || (runtextpos >= runtextend))
            {
                goto Label_0EE8;
            }
            runtextpos++;
            if (runtext[runtextpos] != '<')
            {
                goto Label_0EE8;
            }
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (1 > (runtextend - runtextpos))
            {
                goto Label_0EE8;
            }
            runtextpos++;
            int start = 1;
            do
            {
                start--;
                if (!RegexRunner.CharInClass(runtext[runtextpos - start], "\0\x0004\t./:;\0\x0002\x0004\x0005\x0003\x0001\t\x0013\0"))
                {
                    goto Label_0EE8;
                }
            }
            while (start > 0);
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_013E;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\x0004\t./:;\0\x0002\x0004\x0005\x0003\x0001\t\x0013\0"));
            runtextpos--;
        Label_013E:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 2;
            }
        Label_0171:
            start = runstack[runstackpos++];
            this.Capture(3, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 3;
            runstack[--runstackpos] = -1;
            runtrack[--runtrackpos] = 1;
            goto Label_0C7F;
        Label_01BE:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (1 > (runtextend - runtextpos))
            {
                goto Label_0EE8;
            }
            runtextpos++;
            start = 1;
            do
            {
                start--;
                if (!RegexRunner.CharInClass(runtext[runtextpos - start], "\0\0\x0001d"))
                {
                    goto Label_0EE8;
                }
            }
            while (start > 0);
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0258;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_0258:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 4;
            }
        Label_028B:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (runtextpos >= runtextend)
            {
                goto Label_0EE8;
            }
            runtextpos++;
            if (!RegexRunner.CharInClass(runtext[runtextpos], "\0\0\t\0\x0002\x0004\x0005\x0003\x0001\t\x0013\0"))
            {
                goto Label_0EE8;
            }
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_030B;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\x0004\t-.:;\0\x0002\x0004\x0005\x0003\x0001\t\x0013\0"));
            runtextpos--;
        Label_030B:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 5;
            }
        Label_033E:
            start = runstack[runstackpos++];
            this.Capture(4, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 3;
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            runtrack[--runtrackpos] = runtextpos;
            runtrack[--runtrackpos] = 6;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_03DF;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_03DF:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 7;
            }
        Label_0412:
            if (runtextpos >= runtextend)
            {
                goto Label_0EE8;
            }
            runtextpos++;
            if (runtext[runtextpos] != '=')
            {
                goto Label_0EE8;
            }
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0472;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_0472:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 8;
            }
        Label_04A5:
            if (runtextpos >= runtextend)
            {
                goto Label_0EE8;
            }
            runtextpos++;
            if (runtext[runtextpos] != '"')
            {
                goto Label_0EE8;
            }
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_050F;
                }
                runtextpos++;
            }
            while (runtext[runtextpos] != '"');
            runtextpos--;
        Label_050F:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 9;
            }
        Label_0542:
            start = runstack[runstackpos++];
            this.Capture(5, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 3;
            if (runtextpos >= runtextend)
            {
                goto Label_0EE8;
            }
            runtextpos++;
            if (runtext[runtextpos] != '"')
            {
                goto Label_0EE8;
            }
            goto Label_0C1F;
        Label_05EF:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 11;
            }
        Label_0622:
            if (runtextpos >= runtextend)
            {
                goto Label_0EE8;
            }
            runtextpos++;
            if (runtext[runtextpos] != '=')
            {
                goto Label_0EE8;
            }
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0682;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_0682:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 12;
            }
        Label_06B5:
            if (runtextpos >= runtextend)
            {
                goto Label_0EE8;
            }
            runtextpos++;
            if (runtext[runtextpos] != '\'')
            {
                goto Label_0EE8;
            }
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_071F;
                }
                runtextpos++;
            }
            while (runtext[runtextpos] != '\'');
            runtextpos--;
        Label_071F:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 13;
            }
        Label_0752:
            start = runstack[runstackpos++];
            this.Capture(5, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 3;
            if (runtextpos >= runtextend)
            {
                goto Label_0EE8;
            }
            runtextpos++;
            if (runtext[runtextpos] != '\'')
            {
                goto Label_0EE8;
            }
            goto Label_0C1F;
        Label_07FF:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 15;
            }
        Label_0832:
            if (runtextpos >= runtextend)
            {
                goto Label_0EE8;
            }
            runtextpos++;
            if (runtext[runtextpos] != '=')
            {
                goto Label_0EE8;
            }
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0892;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_0892:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 0x10;
            }
        Label_08C5:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (((3 > (runtextend - runtextpos)) || (runtext[runtextpos] != '<')) || ((runtext[runtextpos + 1] != '%') || (runtext[runtextpos + 2] != '#')))
            {
                goto Label_0EE8;
            }
            runtextpos += 3;
            start = runtextend - runtextpos;
            if (start > 0)
            {
                runtrack[--runtrackpos] = start - 1;
                runtrack[--runtrackpos] = runtextpos;
                runtrack[--runtrackpos] = 0x11;
            }
        Label_0959:
            if (((2 > (runtextend - runtextpos)) || (runtext[runtextpos] != '%')) || (runtext[runtextpos + 1] != '>'))
            {
                goto Label_0EE8;
            }
            runtextpos += 2;
            start = runstack[runstackpos++];
            this.Capture(5, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 3;
            goto Label_0C1F;
        Label_0A1D:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 0x13;
            }
        Label_0A50:
            if (runtextpos >= runtextend)
            {
                goto Label_0EE8;
            }
            runtextpos++;
            if (runtext[runtextpos] != '=')
            {
                goto Label_0EE8;
            }
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0AB0;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_0AB0:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 20;
            }
        Label_0AE3:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0B3C;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\x0001\b\x0001\"#'(/0=?d"));
            runtextpos--;
        Label_0B3C:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 0x15;
            }
        Label_0B6F:
            start = runstack[runstackpos++];
            this.Capture(5, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 3;
            goto Label_0C1F;
        Label_0BEF:
            start = runstack[runstackpos++];
            this.Capture(5, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 3;
        Label_0C1F:
            start = runstack[runstackpos++];
            this.Capture(2, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 3;
            start = runstack[runstackpos++];
            this.Capture(1, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 3;
        Label_0C7F:
            runtrack[--runtrackpos] = start;
            if ((start = runstack[runstackpos++]) != runtextpos)
            {
                runtrack[--runtrackpos] = runtextpos;
                runstack[--runstackpos] = runtextpos;
                runtrack[--runtrackpos] = 0x17;
                if ((runtrackpos > 0xd4) && (runstackpos > 0x9f))
                {
                    goto Label_01BE;
                }
                runtrack[--runtrackpos] = 0x18;
                goto Label_0EE8;
            }
            runtrack[--runtrackpos] = 0x19;
        Label_0CF2:
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0D33;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_0D33:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 0x1a;
            }
        Label_0D66:
            runstack[--runstackpos] = -1;
            runstack[--runstackpos] = 0;
            runtrack[--runtrackpos] = 0x1b;
            goto Label_0DF6;
        Label_0D8F:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (runtextpos >= runtextend)
            {
                goto Label_0EE8;
            }
            runtextpos++;
            if (runtext[runtextpos] != '/')
            {
                goto Label_0EE8;
            }
            start = runstack[runstackpos++];
            this.Capture(6, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 3;
        Label_0DF6:
            start = runstack[runstackpos++];
            runtrack[--runtrackpos] = num5;
            if ((((num5 = runstack[runstackpos++]) != runtextpos) || (start < 0)) && (start < 1))
            {
                runstack[--runstackpos] = runtextpos;
                runstack[--runstackpos] = start + 1;
                runtrack[--runtrackpos] = 0x1c;
                if ((runtrackpos > 0xd4) && (runstackpos > 0x9f))
                {
                    goto Label_0D8F;
                }
                runtrack[--runtrackpos] = 0x1d;
                goto Label_0EE8;
            }
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 30;
        Label_0E90:
            if (runtextpos >= runtextend)
            {
                goto Label_0EE8;
            }
            runtextpos++;
            if (runtext[runtextpos] != '>')
            {
                goto Label_0EE8;
            }
            start = runstack[runstackpos++];
            this.Capture(0, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 3;
        Label_0EDF:
            base.runtextpos = runtextpos;
            return;
        Label_0EE8:
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
                    goto Label_0EE8;

                case 2:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 2;
                    }
                    goto Label_0171;

                case 3:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    this.Uncapture();
                    goto Label_0EE8;

                case 4:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 4;
                    }
                    goto Label_028B;

                case 5:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 5;
                    }
                    goto Label_033E;

                case 6:
                    runtextpos = runtrack[runtrackpos++];
                    runtrack[--runtrackpos] = runtextpos;
                    runtrack[--runtrackpos] = 10;
                    start = (num5 = runtextend - runtextpos) + 1;
                    do
                    {
                        if (--start <= 0)
                        {
                            goto Label_05EF;
                        }
                        runtextpos++;
                    }
                    while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
                    runtextpos--;
                    goto Label_05EF;

                case 7:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 7;
                    }
                    goto Label_0412;

                case 8:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 8;
                    }
                    goto Label_04A5;

                case 9:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 9;
                    }
                    goto Label_0542;

                case 10:
                    runtextpos = runtrack[runtrackpos++];
                    runtrack[--runtrackpos] = runtextpos;
                    runtrack[--runtrackpos] = 14;
                    start = (num5 = runtextend - runtextpos) + 1;
                    do
                    {
                        if (--start <= 0)
                        {
                            goto Label_07FF;
                        }
                        runtextpos++;
                    }
                    while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
                    runtextpos--;
                    goto Label_07FF;

                case 11:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 11;
                    }
                    goto Label_0622;

                case 12:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 12;
                    }
                    goto Label_06B5;

                case 13:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 13;
                    }
                    goto Label_0752;

                case 14:
                    runtextpos = runtrack[runtrackpos++];
                    runtrack[--runtrackpos] = runtextpos;
                    runtrack[--runtrackpos] = 0x12;
                    start = (num5 = runtextend - runtextpos) + 1;
                    do
                    {
                        if (--start <= 0)
                        {
                            goto Label_0A1D;
                        }
                        runtextpos++;
                    }
                    while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
                    runtextpos--;
                    goto Label_0A1D;

                case 15:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 15;
                    }
                    goto Label_0832;

                case 0x10:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 0x10;
                    }
                    goto Label_08C5;

                case 0x11:
                    runtextpos = runtrack[runtrackpos++];
                    num5 = runtrack[runtrackpos++];
                    runtextpos++;
                    if (!RegexRunner.CharInClass(runtext[runtextpos], "\0\x0001\0\0"))
                    {
                        goto Label_0EE8;
                    }
                    if (num5 > 0)
                    {
                        runtrack[--runtrackpos] = num5 - 1;
                        runtrack[--runtrackpos] = runtextpos;
                        runtrack[--runtrackpos] = 0x11;
                    }
                    goto Label_0959;

                case 0x12:
                    runtextpos = runtrack[runtrackpos++];
                    runstack[--runstackpos] = runtextpos;
                    runtrack[--runtrackpos] = 1;
                    start = runtextend - runtextpos;
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos;
                        runtrack[--runtrackpos] = 0x16;
                    }
                    goto Label_0BEF;

                case 0x13:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 0x13;
                    }
                    goto Label_0A50;

                case 20:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 20;
                    }
                    goto Label_0AE3;

                case 0x15:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 0x15;
                    }
                    goto Label_0B6F;

                case 0x16:
                    runtextpos = runtrack[runtrackpos++];
                    num5 = runtrack[runtrackpos++];
                    runtextpos++;
                    if (!RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"))
                    {
                        goto Label_0EE8;
                    }
                    if (num5 > 0)
                    {
                        runtrack[--runtrackpos] = num5 - 1;
                        runtrack[--runtrackpos] = runtextpos;
                        runtrack[--runtrackpos] = 0x16;
                    }
                    goto Label_0BEF;

                case 0x17:
                {
                    runtextpos = runtrack[runtrackpos++];
                    int num1 = runstack[runstackpos++];
                    runtrack[--runtrackpos] = 0x19;
                    goto Label_0CF2;
                }
                case 0x18:
                    goto Label_01BE;

                case 0x19:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    goto Label_0EE8;

                case 0x1a:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 0x1a;
                    }
                    goto Label_0D66;

                case 0x1b:
                    runstackpos += 2;
                    goto Label_0EE8;

                case 0x1c:
                    start = runstack[runstackpos++] - 1;
                    if (start < 0)
                    {
                        runstack[runstackpos] = runtrack[runtrackpos++];
                        runstack[--runstackpos] = start;
                        goto Label_0EE8;
                    }
                    runtextpos = runstack[runstackpos++];
                    runtrack[--runtrackpos] = start;
                    runtrack[--runtrackpos] = 30;
                    goto Label_0E90;

                case 0x1d:
                    goto Label_0D8F;

                case 30:
                    start = runtrack[runtrackpos++];
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    runstack[--runstackpos] = start;
                    goto Label_0EE8;
            }
            runtextpos = runtrack[runtrackpos++];
            goto Label_0EDF;
        }

        public override void InitTrackCount()
        {
            base.runtrackcount = 0x35;
        }
    }
}

