using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Microsoft.Win32;

namespace Lada.Windows
{
    public static class SystemParametersEx
    {
        public static int       WheelScrollChars
        {
            get
            {
                if (SystemParametersEx.wheelScrollChars < 0)
                {
                    lock (SyncMouse)
                    {
                        if (SystemParametersEx.wheelScrollChars < 0)
                        {
                            if (!SystemParametersInfo (0x006C, 0, ref SystemParametersEx.wheelScrollChars, 0))
                                throw new Win32Exception ();
                        }
                    }
                }
                return SystemParametersEx.wheelScrollChars;
            }
        }

        internal static void    Initialize()
        {
        }

        static                  SystemParametersEx()
        {
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        }
        private static object   SyncMouse
        {
            get
            {
                return typeof (SystemParametersEx);
            }
        }
        private static void     OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Mouse)
            {
                lock (SyncMouse)
                {
                    SystemParametersEx.wheelScrollChars = -1;
                }
            }
        }
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport ("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SystemParametersInfo(int nAction, int nParam, ref int value, int ignore);

        private static int      wheelScrollChars = -1;
    }
}
