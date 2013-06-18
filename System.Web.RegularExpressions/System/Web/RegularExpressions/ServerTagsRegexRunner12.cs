namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    internal class ServerTagsRegexRunner12 : RegexRunner
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
            int num4;
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
            if (((2 <= (runtextend - runtextpos)) && (runtext[runtextpos] == '<')) && (runtext[runtextpos + 1] == '%'))
            {
                runtextpos += 2;
                runstack[--runstackpos] = base.runtrack.Length - runtrackpos;
                runstack[--runstackpos] = this.Crawlpos();
                runtrack[--runtrackpos] = 2;
                runtrack[--runtrackpos] = runtextpos;
                runtrack[--runtrackpos] = 3;
                if (runtextpos < runtextend)
                {
                    runtextpos++;
                    if (RegexRunner.CharInClass(runtext[runtextpos], "\0\x0002\0#%"))
                    {
                        runtrackpos = base.runtrack.Length - runstack[runstackpos++];
                        int num1 = runstack[runstackpos++];
                        if (num1 != this.Crawlpos())
                        {
                            do
                            {
                                this.Uncapture();
                            }
                            while (num1 != this.Crawlpos());
                        }
                    }
                }
            }
            goto Label_037E;
        Label_0211:
            if (num5 > num4)
            {
                runtrack[--runtrackpos] = (num5 - num4) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 5;
            }
        Label_0244:
            num4 = runstack[runstackpos++];
            this.Capture(2, num4, runtextpos);
            runtrack[--runtrackpos] = num4;
            runtrack[--runtrackpos] = 6;
            if (runtextpos >= runtextend)
            {
                goto Label_037E;
            }
            runtextpos++;
            if (runtext[runtextpos] != '%')
            {
                goto Label_037E;
            }
            num4 = runstack[runstackpos++];
            this.Capture(1, num4, runtextpos);
            runtrack[--runtrackpos] = num4;
            runtrack[--runtrackpos] = 6;
        Label_02C3:
            if (num4 != -1)
            {
                runtrack[--runtrackpos] = num4;
            }
            else
            {
                runtrack[--runtrackpos] = runtextpos;
            }
            if ((num4 = runstack[runstackpos++]) != runtextpos)
            {
                runtrack[--runtrackpos] = runtextpos;
                runtrack[--runtrackpos] = 7;
            }
            else
            {
                runstack[--runstackpos] = num4;
                runtrack[--runtrackpos] = 8;
            }
            if (runtextpos >= runtextend)
            {
                goto Label_037E;
            }
            runtextpos++;
            if (runtext[runtextpos] != '>')
            {
                goto Label_037E;
            }
            num4 = runstack[runstackpos++];
            this.Capture(0, num4, runtextpos);
            runtrack[--runtrackpos] = num4;
            runtrack[--runtrackpos] = 6;
        Label_0375:
            base.runtextpos = runtextpos;
            return;
        Label_037E:
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
                    goto Label_037E;

                case 2:
                    runstackpos += 2;
                    goto Label_037E;

                case 3:
                    runtextpos = runtrack[runtrackpos++];
                    num4 = runstack[runstackpos++];
                    runtrackpos = base.runtrack.Length - runstack[runstackpos++];
                    runtrack[--runtrackpos] = num4;
                    runtrack[--runtrackpos] = 4;
                    runstack[--runstackpos] = -1;
                    runtrack[--runtrackpos] = 1;
                    goto Label_02C3;

                case 4:
                {
                    int num10 = runtrack[runtrackpos++];
                    if (num10 != this.Crawlpos())
                    {
                        do
                        {
                            this.Uncapture();
                        }
                        while (num10 != this.Crawlpos());
                    }
                    goto Label_037E;
                }
                case 5:
                    runtextpos = runtrack[runtrackpos++];
                    num4 = runtrack[runtrackpos++];
                    if (num4 > 0)
                    {
                        runtrack[--runtrackpos] = num4 - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 5;
                    }
                    goto Label_0244;

                case 6:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    this.Uncapture();
                    goto Label_037E;

                case 7:
                    runtextpos = runtrack[runtrackpos++];
                    runstack[--runstackpos] = runtextpos;
                    runtrack[--runtrackpos] = 8;
                    if ((runtrackpos > 0x38) && (runstackpos > 0x2a))
                    {
                        runstack[--runstackpos] = runtextpos;
                        runtrack[--runtrackpos] = 1;
                        runstack[--runstackpos] = runtextpos;
                        runtrack[--runtrackpos] = 1;
                        num4 = (num5 = runtextend - runtextpos) + 1;
                        do
                        {
                            if (--num4 <= 0)
                            {
                                goto Label_0211;
                            }
                            runtextpos++;
                        }
                        while (runtext[runtextpos] != '%');
                        runtextpos--;
                        goto Label_0211;
                    }
                    runtrack[--runtrackpos] = 9;
                    goto Label_037E;

                case 8:
                    runstack[runstackpos] = runtrack[runtrackpos++];
                    goto Label_037E;
            }
            runtextpos = runtrack[runtrackpos++];
            goto Label_0375;
        }

        public override void InitTrackCount()
        {
            base.runtrackcount = 14;
        }
    }
}

