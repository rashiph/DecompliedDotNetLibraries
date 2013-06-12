namespace System.Windows.Forms
{
    using System;

    internal static class MessageDecoder
    {
        private static string MsgToString(int msg)
        {
            string str;
            switch (msg)
            {
                case 0:
                    str = "WM_NULL";
                    break;

                case 1:
                    str = "WM_CREATE";
                    break;

                case 2:
                    str = "WM_DESTROY";
                    break;

                case 3:
                    str = "WM_MOVE";
                    break;

                case 5:
                    str = "WM_SIZE";
                    break;

                case 6:
                    str = "WM_ACTIVATE";
                    break;

                case 7:
                    str = "WM_SETFOCUS";
                    break;

                case 8:
                    str = "WM_KILLFOCUS";
                    break;

                case 10:
                    str = "WM_ENABLE";
                    break;

                case 11:
                    str = "WM_SETREDRAW";
                    break;

                case 12:
                    str = "WM_SETTEXT";
                    break;

                case 13:
                    str = "WM_GETTEXT";
                    break;

                case 14:
                    str = "WM_GETTEXTLENGTH";
                    break;

                case 15:
                    str = "WM_PAINT";
                    break;

                case 0x10:
                    str = "WM_CLOSE";
                    break;

                case 0x11:
                    str = "WM_QUERYENDSESSION";
                    break;

                case 0x12:
                    str = "WM_QUIT";
                    break;

                case 0x13:
                    str = "WM_QUERYOPEN";
                    break;

                case 20:
                    str = "WM_ERASEBKGND";
                    break;

                case 0x15:
                    str = "WM_SYSCOLORCHANGE";
                    break;

                case 0x16:
                    str = "WM_ENDSESSION";
                    break;

                case 0x18:
                    str = "WM_SHOWWINDOW";
                    break;

                case 0x19:
                    str = "WM_CTLCOLOR";
                    break;

                case 0x1a:
                    str = "WM_WININICHANGE";
                    break;

                case 0x1b:
                    str = "WM_DEVMODECHANGE";
                    break;

                case 0x1c:
                    str = "WM_ACTIVATEAPP";
                    break;

                case 0x1d:
                    str = "WM_FONTCHANGE";
                    break;

                case 30:
                    str = "WM_TIMECHANGE";
                    break;

                case 0x1f:
                    str = "WM_CANCELMODE";
                    break;

                case 0x20:
                    str = "WM_SETCURSOR";
                    break;

                case 0x21:
                    str = "WM_MOUSEACTIVATE";
                    break;

                case 0x22:
                    str = "WM_CHILDACTIVATE";
                    break;

                case 0x23:
                    str = "WM_QUEUESYNC";
                    break;

                case 0x24:
                    str = "WM_GETMINMAXINFO";
                    break;

                case 0x26:
                    str = "WM_PAINTICON";
                    break;

                case 0x27:
                    str = "WM_ICONERASEBKGND";
                    break;

                case 40:
                    str = "WM_NEXTDLGCTL";
                    break;

                case 0x2a:
                    str = "WM_SPOOLERSTATUS";
                    break;

                case 0x2b:
                    str = "WM_DRAWITEM";
                    break;

                case 0x2c:
                    str = "WM_MEASUREITEM";
                    break;

                case 0x2d:
                    str = "WM_DELETEITEM";
                    break;

                case 0x2e:
                    str = "WM_VKEYTOITEM";
                    break;

                case 0x2f:
                    str = "WM_CHARTOITEM";
                    break;

                case 0x30:
                    str = "WM_SETFONT";
                    break;

                case 0x31:
                    str = "WM_GETFONT";
                    break;

                case 50:
                    str = "WM_SETHOTKEY";
                    break;

                case 0x33:
                    str = "WM_GETHOTKEY";
                    break;

                case 0x37:
                    str = "WM_QUERYDRAGICON";
                    break;

                case 0x39:
                    str = "WM_COMPAREITEM";
                    break;

                case 0x3d:
                    str = "WM_GETOBJECT";
                    break;

                case 0x41:
                    str = "WM_COMPACTING";
                    break;

                case 0x44:
                    str = "WM_COMMNOTIFY";
                    break;

                case 70:
                    str = "WM_WINDOWPOSCHANGING";
                    break;

                case 0x47:
                    str = "WM_WINDOWPOSCHANGED";
                    break;

                case 0x48:
                    str = "WM_POWER";
                    break;

                case 0x4a:
                    str = "WM_COPYDATA";
                    break;

                case 0x4b:
                    str = "WM_CANCELJOURNAL";
                    break;

                case 0x4e:
                    str = "WM_NOTIFY";
                    break;

                case 80:
                    str = "WM_INPUTLANGCHANGEREQUEST";
                    break;

                case 0x51:
                    str = "WM_INPUTLANGCHANGE";
                    break;

                case 0x52:
                    str = "WM_TCARD";
                    break;

                case 0x53:
                    str = "WM_HELP";
                    break;

                case 0x54:
                    str = "WM_USERCHANGED";
                    break;

                case 0x55:
                    str = "WM_NOTIFYFORMAT";
                    break;

                case 0x7b:
                    str = "WM_CONTEXTMENU";
                    break;

                case 0x7c:
                    str = "WM_STYLECHANGING";
                    break;

                case 0x7d:
                    str = "WM_STYLECHANGED";
                    break;

                case 0x7e:
                    str = "WM_DISPLAYCHANGE";
                    break;

                case 0x7f:
                    str = "WM_GETICON";
                    break;

                case 0x80:
                    str = "WM_SETICON";
                    break;

                case 0x81:
                    str = "WM_NCCREATE";
                    break;

                case 130:
                    str = "WM_NCDESTROY";
                    break;

                case 0x83:
                    str = "WM_NCCALCSIZE";
                    break;

                case 0x84:
                    str = "WM_NCHITTEST";
                    break;

                case 0x85:
                    str = "WM_NCPAINT";
                    break;

                case 0x86:
                    str = "WM_NCACTIVATE";
                    break;

                case 0x87:
                    str = "WM_GETDLGCODE";
                    break;

                case 160:
                    str = "WM_NCMOUSEMOVE";
                    break;

                case 0xa1:
                    str = "WM_NCLBUTTONDOWN";
                    break;

                case 0xa2:
                    str = "WM_NCLBUTTONUP";
                    break;

                case 0xa3:
                    str = "WM_NCLBUTTONDBLCLK";
                    break;

                case 0xa4:
                    str = "WM_NCRBUTTONDOWN";
                    break;

                case 0xa5:
                    str = "WM_NCRBUTTONUP";
                    break;

                case 0xa6:
                    str = "WM_NCRBUTTONDBLCLK";
                    break;

                case 0xa7:
                    str = "WM_NCMBUTTONDOWN";
                    break;

                case 0xa8:
                    str = "WM_NCMBUTTONUP";
                    break;

                case 0xa9:
                    str = "WM_NCMBUTTONDBLCLK";
                    break;

                case 0x100:
                    str = "WM_KEYDOWN";
                    break;

                case 0x101:
                    str = "WM_KEYUP";
                    break;

                case 0x102:
                    str = "WM_CHAR";
                    break;

                case 0x103:
                    str = "WM_DEADCHAR";
                    break;

                case 260:
                    str = "WM_SYSKEYDOWN";
                    break;

                case 0x105:
                    str = "WM_SYSKEYUP";
                    break;

                case 0x106:
                    str = "WM_SYSCHAR";
                    break;

                case 0x107:
                    str = "WM_SYSDEADCHAR";
                    break;

                case 0x108:
                    str = "WM_KEYLAST";
                    break;

                case 0x10d:
                    str = "WM_IME_STARTCOMPOSITION";
                    break;

                case 270:
                    str = "WM_IME_ENDCOMPOSITION";
                    break;

                case 0x10f:
                    str = "WM_IME_COMPOSITION";
                    break;

                case 0x110:
                    str = "WM_INITDIALOG";
                    break;

                case 0x111:
                    str = "WM_COMMAND";
                    break;

                case 0x112:
                    str = "WM_SYSCOMMAND";
                    break;

                case 0x113:
                    str = "WM_TIMER";
                    break;

                case 0x114:
                    str = "WM_HSCROLL";
                    break;

                case 0x115:
                    str = "WM_VSCROLL";
                    break;

                case 0x116:
                    str = "WM_INITMENU";
                    break;

                case 0x117:
                    str = "WM_INITMENUPOPUP";
                    break;

                case 0x11f:
                    str = "WM_MENUSELECT";
                    break;

                case 0x120:
                    str = "WM_MENUCHAR";
                    break;

                case 0x121:
                    str = "WM_ENTERIDLE";
                    break;

                case 0x132:
                    str = "WM_CTLCOLORMSGBOX";
                    break;

                case 0x133:
                    str = "WM_CTLCOLOREDIT";
                    break;

                case 0x134:
                    str = "WM_CTLCOLORLISTBOX";
                    break;

                case 0x135:
                    str = "WM_CTLCOLORBTN";
                    break;

                case 310:
                    str = "WM_CTLCOLORDLG";
                    break;

                case 0x137:
                    str = "WM_CTLCOLORSCROLLBAR";
                    break;

                case 0x138:
                    str = "WM_CTLCOLORSTATIC";
                    break;

                case 0x200:
                    str = "WM_MOUSEMOVE";
                    break;

                case 0x201:
                    str = "WM_LBUTTONDOWN";
                    break;

                case 0x202:
                    str = "WM_LBUTTONUP";
                    break;

                case 0x203:
                    str = "WM_LBUTTONDBLCLK";
                    break;

                case 0x204:
                    str = "WM_RBUTTONDOWN";
                    break;

                case 0x205:
                    str = "WM_RBUTTONUP";
                    break;

                case 0x206:
                    str = "WM_RBUTTONDBLCLK";
                    break;

                case 0x207:
                    str = "WM_MBUTTONDOWN";
                    break;

                case 520:
                    str = "WM_MBUTTONUP";
                    break;

                case 0x209:
                    str = "WM_MBUTTONDBLCLK";
                    break;

                case 0x20a:
                    str = "WM_MOUSEWHEEL";
                    break;

                case 0x210:
                    str = "WM_PARENTNOTIFY";
                    break;

                case 0x211:
                    str = "WM_ENTERMENULOOP";
                    break;

                case 530:
                    str = "WM_EXITMENULOOP";
                    break;

                case 0x213:
                    str = "WM_NEXTMENU";
                    break;

                case 0x214:
                    str = "WM_SIZING";
                    break;

                case 0x215:
                    str = "WM_CAPTURECHANGED";
                    break;

                case 0x216:
                    str = "WM_MOVING";
                    break;

                case 0x218:
                    str = "WM_POWERBROADCAST";
                    break;

                case 0x219:
                    str = "WM_DEVICECHANGE";
                    break;

                case 0x220:
                    str = "WM_MDICREATE";
                    break;

                case 0x221:
                    str = "WM_MDIDESTROY";
                    break;

                case 0x222:
                    str = "WM_MDIACTIVATE";
                    break;

                case 0x223:
                    str = "WM_MDIRESTORE";
                    break;

                case 0x224:
                    str = "WM_MDINEXT";
                    break;

                case 0x225:
                    str = "WM_MDIMAXIMIZE";
                    break;

                case 550:
                    str = "WM_MDITILE";
                    break;

                case 0x227:
                    str = "WM_MDICASCADE";
                    break;

                case 0x228:
                    str = "WM_MDIICONARRANGE";
                    break;

                case 0x229:
                    str = "WM_MDIGETACTIVE";
                    break;

                case 560:
                    str = "WM_MDISETMENU";
                    break;

                case 0x231:
                    str = "WM_ENTERSIZEMOVE";
                    break;

                case 0x232:
                    str = "WM_EXITSIZEMOVE";
                    break;

                case 0x233:
                    str = "WM_DROPFILES";
                    break;

                case 0x234:
                    str = "WM_MDIREFRESHMENU";
                    break;

                case 0x281:
                    str = "WM_IME_SETCONTEXT";
                    break;

                case 0x282:
                    str = "WM_IME_NOTIFY";
                    break;

                case 0x283:
                    str = "WM_IME_CONTROL";
                    break;

                case 0x284:
                    str = "WM_IME_COMPOSITIONFULL";
                    break;

                case 0x285:
                    str = "WM_IME_SELECT";
                    break;

                case 0x286:
                    str = "WM_IME_CHAR";
                    break;

                case 0x290:
                    str = "WM_IME_KEYDOWN";
                    break;

                case 0x291:
                    str = "WM_IME_KEYUP";
                    break;

                case 0x2a1:
                    str = "WM_MOUSEHOVER";
                    break;

                case 0x2a3:
                    str = "WM_MOUSELEAVE";
                    break;

                case 0x300:
                    str = "WM_CUT";
                    break;

                case 0x301:
                    str = "WM_COPY";
                    break;

                case 770:
                    str = "WM_PASTE";
                    break;

                case 0x303:
                    str = "WM_CLEAR";
                    break;

                case 0x304:
                    str = "WM_UNDO";
                    break;

                case 0x305:
                    str = "WM_RENDERFORMAT";
                    break;

                case 0x306:
                    str = "WM_RENDERALLFORMATS";
                    break;

                case 0x307:
                    str = "WM_DESTROYCLIPBOARD";
                    break;

                case 0x308:
                    str = "WM_DRAWCLIPBOARD";
                    break;

                case 0x309:
                    str = "WM_PAINTCLIPBOARD";
                    break;

                case 0x30a:
                    str = "WM_VSCROLLCLIPBOARD";
                    break;

                case 0x30b:
                    str = "WM_SIZECLIPBOARD";
                    break;

                case 780:
                    str = "WM_ASKCBFORMATNAME";
                    break;

                case 0x30d:
                    str = "WM_CHANGECBCHAIN";
                    break;

                case 0x30e:
                    str = "WM_HSCROLLCLIPBOARD";
                    break;

                case 0x30f:
                    str = "WM_QUERYNEWPALETTE";
                    break;

                case 0x310:
                    str = "WM_PALETTEISCHANGING";
                    break;

                case 0x311:
                    str = "WM_PALETTECHANGED";
                    break;

                case 0x312:
                    str = "WM_HOTKEY";
                    break;

                case 0x317:
                    str = "WM_PRINT";
                    break;

                case 0x318:
                    str = "WM_PRINTCLIENT";
                    break;

                case 0x35f:
                    str = "WM_HANDHELDLAST";
                    break;

                case 0x360:
                    str = "WM_AFXFIRST";
                    break;

                case 0x358:
                    str = "WM_HANDHELDFIRST";
                    break;

                case 0x37f:
                    str = "WM_AFXLAST";
                    break;

                case 0x380:
                    str = "WM_PENWINFIRST";
                    break;

                case 0x4c8:
                    str = "EM_SETBIDIOPTIONS";
                    break;

                case 0x4c9:
                    str = "EM_GETBIDIOPTIONS";
                    break;

                case 0x4ca:
                    str = "EM_SETTYPOGRAPHYOPTIONS";
                    break;

                case 0x4cb:
                    str = "EM_GETTYPOGRAPHYOPTIONS";
                    break;

                case 0x4cc:
                    str = "EM_SETEDITSTYLE";
                    break;

                case 0x4cd:
                    str = "EM_GETEDITSTYLE";
                    break;

                case 0x8000:
                    str = "WM_APP";
                    break;

                case 0x400:
                    str = "WM_USER";
                    break;

                case 0x425:
                    str = "EM_GETLIMITTEXT";
                    break;

                case 0x426:
                    str = "EM_POSFROMCHAR";
                    break;

                case 0x427:
                    str = "EM_CHARFROMPOS";
                    break;

                case 0x431:
                    str = "EM_SCROLLCARET";
                    break;

                case 0x432:
                    str = "EM_CANPASTE";
                    break;

                case 0x433:
                    str = "EM_DISPLAYBAND";
                    break;

                case 0x434:
                    str = "EM_EXGETSEL";
                    break;

                case 0x435:
                    str = "EM_EXLIMITTEXT";
                    break;

                case 0x436:
                    str = "EM_EXLINEFROMCHAR";
                    break;

                case 0x437:
                    str = "EM_EXSETSEL";
                    break;

                case 0x438:
                    str = "EM_FINDTEXT";
                    break;

                case 0x439:
                    str = "EM_FORMATRANGE";
                    break;

                case 0x43a:
                    str = "EM_GETCHARFORMAT";
                    break;

                case 0x43b:
                    str = "EM_GETEVENTMASK";
                    break;

                case 0x43c:
                    str = "EM_GETOLEINTERFACE";
                    break;

                case 0x43d:
                    str = "EM_GETPARAFORMAT";
                    break;

                case 0x43e:
                    str = "EM_GETSELTEXT";
                    break;

                case 0x43f:
                    str = "EM_HIDESELECTION";
                    break;

                case 0x440:
                    str = "EM_PASTESPECIAL";
                    break;

                case 0x441:
                    str = "EM_REQUESTRESIZE";
                    break;

                case 0x442:
                    str = "EM_SELECTIONTYPE";
                    break;

                case 0x443:
                    str = "EM_SETBKGNDCOLOR";
                    break;

                case 0x444:
                    str = "EM_SETCHARFORMAT";
                    break;

                case 0x445:
                    str = "EM_SETEVENTMASK";
                    break;

                case 0x446:
                    str = "EM_SETOLECALLBACK";
                    break;

                case 0x447:
                    str = "EM_SETPARAFORMAT";
                    break;

                case 0x448:
                    str = "EM_SETTARGETDEVICE";
                    break;

                case 0x449:
                    str = "EM_STREAMIN";
                    break;

                case 0x44a:
                    str = "EM_STREAMOUT";
                    break;

                case 0x44b:
                    str = "EM_GETTEXTRANGE";
                    break;

                case 0x44c:
                    str = "EM_FINDWORDBREAK";
                    break;

                case 0x44d:
                    str = "EM_SETOPTIONS";
                    break;

                case 0x44e:
                    str = "EM_GETOPTIONS";
                    break;

                case 0x44f:
                    str = "EM_FINDTEXTEX";
                    break;

                case 0x450:
                    str = "EM_GETWORDBREAKPROCEX";
                    break;

                case 0x451:
                    str = "EM_SETWORDBREAKPROCEX";
                    break;

                case 0x452:
                    str = "EM_SETUNDOLIMIT";
                    break;

                case 0x454:
                    str = "EM_REDO";
                    break;

                case 0x455:
                    str = "EM_CANREDO";
                    break;

                case 0x456:
                    str = "EM_GETUNDONAME";
                    break;

                case 0x457:
                    str = "EM_GETREDONAME";
                    break;

                case 0x458:
                    str = "EM_STOPGROUPTYPING";
                    break;

                case 0x459:
                    str = "EM_SETTEXTMODE";
                    break;

                case 0x45a:
                    str = "EM_GETTEXTMODE";
                    break;

                case 0x45b:
                    str = "EM_AUTOURLDETECT";
                    break;

                case 0x45c:
                    str = "EM_GETAUTOURLDETECT";
                    break;

                case 0x45d:
                    str = "EM_SETPALETTE";
                    break;

                case 0x45e:
                    str = "EM_GETTEXTEX";
                    break;

                case 0x45f:
                    str = "EM_GETTEXTLENGTHEX";
                    break;

                case 0x464:
                    str = "EM_SETPUNCTUATION";
                    break;

                case 0x465:
                    str = "EM_GETPUNCTUATION";
                    break;

                case 0x466:
                    str = "EM_SETWORDWRAPMODE";
                    break;

                case 0x467:
                    str = "EM_GETWORDWRAPMODE";
                    break;

                case 0x468:
                    str = "EM_SETIMECOLOR";
                    break;

                case 0x469:
                    str = "EM_GETIMECOLOR";
                    break;

                case 0x46a:
                    str = "EM_SETIMEOPTIONS";
                    break;

                case 0x46b:
                    str = "EM_GETIMEOPTIONS";
                    break;

                case 0x46c:
                    str = "EM_CONVPOSITION";
                    break;

                case 0x478:
                    str = "EM_SETLANGOPTIONS";
                    break;

                case 0x479:
                    str = "EM_GETLANGOPTIONS";
                    break;

                case 0x47a:
                    str = "EM_GETIMECOMPMODE";
                    break;

                case 0x47b:
                    str = "EM_FINDTEXTW";
                    break;

                case 0x47c:
                    str = "EM_FINDTEXTEXW";
                    break;

                case 0x47d:
                    str = "EM_RECONVERSION";
                    break;

                case 0x47e:
                    str = "EM_SETIMEMODEBIAS";
                    break;

                case 0x47f:
                    str = "EM_GETIMEMODEBIAS";
                    break;

                case 0x38f:
                    str = "WM_PENWINLAST";
                    break;

                default:
                    str = null;
                    break;
            }
            if ((str != null) || ((msg & 0x2000) != 0x2000))
            {
                return str;
            }
            string str2 = MsgToString(msg - 0x2000);
            if (str2 == null)
            {
                str2 = "???";
            }
            return ("WM_REFLECT + " + str2);
        }

        private static string Parenthesize(string input)
        {
            if (input == null)
            {
                return "";
            }
            return (" (" + input + ")");
        }

        public static string ToString(Message message)
        {
            return ToString(message.HWnd, message.Msg, message.WParam, message.LParam, message.Result);
        }

        public static string ToString(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam, IntPtr result)
        {
            string str = Parenthesize(MsgToString(msg));
            string str2 = "";
            if (msg == 0x210)
            {
                str2 = Parenthesize(MsgToString(NativeMethods.Util.LOWORD(wparam)));
            }
            return ("msg=0x" + Convert.ToString(msg, 0x10) + str + " hwnd=0x" + Convert.ToString((long) hWnd, 0x10) + " wparam=0x" + Convert.ToString((long) wparam, 0x10) + " lparam=0x" + Convert.ToString((long) lparam, 0x10) + str2 + " result=0x" + Convert.ToString((long) result, 0x10));
        }
    }
}

