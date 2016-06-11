using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace Lada.Windows
{
    public class UserPreferenceChangedEventManager : WeakEventManager
    {
        public static void      AddListener(IWeakEventListener listener)
        {
            if (listener == null)
                throw new ArgumentNullException ("listener");
            CurrentManager.ProtectedAddListener (null, listener);
        }
        public static void      RemoveListener(IWeakEventListener listener)
        {
            if (listener == null)
                throw new ArgumentNullException ("listener");
            CurrentManager.ProtectedRemoveListener (null, listener);
        }

        protected override void StartListening(object source)
        {
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        }
        protected override void StopListening(object source)
        {
            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
        }

        private void            OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            base.DeliverEvent (null, e);
        }

        private static UserPreferenceChangedEventManager CurrentManager
        {
            get
            {
                if (UserPreferenceChangedEventManager.currentManager == null)
                {
                    lock (typeof (UserPreferenceChangedEventManager))
                    {
                        if (UserPreferenceChangedEventManager.currentManager == null)
                        {
                            SystemParametersEx.Initialize ();
                            var managerType = typeof (UserPreferenceChangedEventManager);
                            UserPreferenceChangedEventManager.currentManager = (UserPreferenceChangedEventManager) WeakEventManager.GetCurrentManager (managerType);
                            if (UserPreferenceChangedEventManager.currentManager == null)
                            {
                                UserPreferenceChangedEventManager.currentManager = new UserPreferenceChangedEventManager ();
                                WeakEventManager.SetCurrentManager (managerType, UserPreferenceChangedEventManager.currentManager);
                            }
                        }
                    }
                }
                return UserPreferenceChangedEventManager.currentManager;
            }
        }
        private static UserPreferenceChangedEventManager currentManager;
    }
}
