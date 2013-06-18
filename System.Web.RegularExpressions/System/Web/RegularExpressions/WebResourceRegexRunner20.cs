namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    internal class WebResourceRegexRunner20 : RegexRunner
    {
        public override bool FindFirstChar()
        {
            // This item is obfuscated and can not be translated.
            string runtext = base.runtext;
            int runtextend = base.runtextend;
            for (int i = base.runtextpos + 1; i < runtextend; i = 2 + i)
            {
                int num2 = runtext[i];
                if (num2 == '%')
                {
                    num2 = i;
                    if (runtext[--num2] == '<')
                    {
                        base.runtextpos = num2;
                        return true;
                    }
                }
                else
                {
                    num2 -= 0x25;
                    if (num2 <= 0x17)
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
            if (((2 > (runtextend - runtextpos)) || (runtext[runtextpos] != '<')) || (runtext[runtextpos + 1] != '%'))
            {
                goto Label_0483;
            }
            runtextpos += 2;
            int start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_00EF;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_00EF:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 2;
            }
        Label_0122:
            if (runtextpos >= runtextend)
            {
                goto Label_0483;
            }
            runtextpos++;
            if (runtext[runtextpos] != '=')
            {
                goto Label_0483;
            }
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0182;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_0182:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 3;
            }
        Label_01B5:
            if (((((13 > (runtextend - runtextpos)) || (runtext[runtextpos] != 'W')) || ((runtext[runtextpos + 1] != 'e') || (runtext[runtextpos + 2] != 'b'))) || (((runtext[runtextpos + 3] != 'R') || (runtext[runtextpos + 4] != 'e')) || ((runtext[runtextpos + 5] != 's') || (runtext[runtextpos + 6] != 'o')))) || ((((runtext[runtextpos + 7] != 'u') || (runtext[runtextpos + 8] != 'r')) || ((runtext[runtextpos + 9] != 'c') || (runtext[runtextpos + 10] != 'e'))) || ((runtext[runtextpos + 11] != '(') || (runtext[runtextpos + 12] != '"'))))
            {
                goto Label_0483;
            }
            runtextpos += 13;
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0307;
                }
                runtextpos++;
            }
            while (runtext[runtextpos] != '"');
            runtextpos--;
        Label_0307:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 4;
            }
        Label_033A:
            start = runstack[runstackpos++];
            this.Capture(1, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 5;
            if (((2 > (runtextend - runtextpos)) || (runtext[runtextpos] != '"')) || (runtext[runtextpos + 1] != ')'))
            {
                goto Label_0483;
            }
            runtextpos += 2;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_03E1;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_03E1:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 6;
            }
        Label_0414:
            if (((2 > (runtextend - runtextpos)) || (runtext[runtextpos] != '%')) || (runtext[runtextpos + 1] != '>'))
            {
                goto Label_0483;
            }
            runtextpos += 2;
            start = runstack[runstackpos++];
            this.Capture(0, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 5;
        Label_047A:
            base.runtextpos = runtextpos;
            return;
        Label_0483:
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
                    goto Label_0483;

                case 2:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 2;
                    }
                    goto Label_0122;

                case 3:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 3;
                    }
                    goto Label_01B5;

                case 4:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 4;
                    }
                    goto Label_033A;

                case 5:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    this.Uncapture();
                    goto Label_0483;

                case 6:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 6;
                    }
                    goto Label_0414;
            }
            runtextpos = runtrack[runtrackpos++];
            goto Label_047A;
        }

        public override void InitTrackCount()
        {
            base.runtrackcount = 9;
        }
    }
}

