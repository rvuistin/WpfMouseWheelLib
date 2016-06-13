using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace Lada.Windows.Input
{
    public interface IMouseWheelController
    {
        DependencyObject    Element
        {
            get;
        }
        IInputElement       InputElement
        {
            get;
        }
        IInputElement       ExitElement
        {
            get;
        }
        void                AddClient(IMouseWheelClient client);
    }

    public partial class MouseWheelController : IMouseWheelController, IDisposable
    {

        public static readonly RoutedEvent PreviewMouseWheelInputEvent = EventManager.RegisterRoutedEvent("PreviewMouseWheelInput", RoutingStrategy.Tunnel, typeof(MouseWheelInputEventHandler), typeof(IInputElement));
        public static readonly RoutedEvent        MouseWheelInputEvent = EventManager.RegisterRoutedEvent(       "MouseWheelInput", RoutingStrategy.Bubble, typeof(MouseWheelInputEventHandler), typeof(IInputElement));


        public MouseWheelController(IInputLevelElement inputLevelElement)
        {
            if (inputLevelElement == null)
                throw new ArgumentNullException ("inputLevelElement");
            _inputLevelElement = inputLevelElement;

            _presentationSource = PresentationSource.FromDependencyObject (inputLevelElement.Proxied) as HwndSource;
            HookMouseHorizontalWheelEvent (_presentationSource);

            InputLevelElement.PreviewMouseWheel += OnPreviewMouseWheel;
            InputLevelElement.AddHandler (PreviewMouseWheelInputEvent, new MouseWheelInputEventHandler (OnPreviewInput));
            InputLevelElement.AddHandler (MouseWheelInputEvent, new MouseWheelInputEventHandler (OnInput));
        }


        public DependencyObject     Element
        {
            get
            {
                return _inputLevelElement.Proxied;
            }
        }
        public IInputElement        InputElement
        {
            get
            {
                return Element as IInputElement;
            }
        }
        public IInputElement        ExitElement
        {
            get
            {
                if (_exitElement == null)
                {
                    _exitElement = Element.GetVisualAncestors ().OfType<IInputElement> ().FirstOrDefault ();
                }
                return _exitElement;
            }
        }
        public IInputLevelElement   InputLevelElement
        {
            get
            {
                return _inputLevelElement;
            }
        }
        public void                 AddClient(IMouseWheelClient client)
        {
            _clients.Add (client);
        }
        public virtual void         Dispose()
        {
            UnhookMouseHorizontalWheelEvent (_presentationSource);
            InputLevelElement.PreviewMouseWheel -= OnPreviewMouseWheel;
            InputLevelElement.RemoveHandler (PreviewMouseWheelInputEvent, new MouseWheelInputEventHandler (OnPreviewInput));
            InputLevelElement.RemoveHandler (MouseWheelInputEvent, new MouseWheelInputEventHandler (OnInput));
            Unload ();
        }

        protected void              Unload()
        {
            foreach (var client in _clients)
            {
                client.Unload ();
            }
        }

        private IInputElement       GetOriginalSource(MouseWheelEventArgs e)
        {
            if (e.OriginalSource is ContentElement)
            {
                var ie = Element as IInputElement;
                var pt = e.GetPosition (ie);
                if (ie is Visual)
                {
                    var result = VisualTreeHelper.HitTest (ie as Visual, pt);
                    return GetOriginalSource (result.VisualHit);
                }
                return null;
            }
            else
            {
                return GetOriginalSource (e.OriginalSource);
            }
        }
        private IInputElement       GetOriginalSource(object originalSource)
        {
            if (originalSource is IInputElement)
            {
                return originalSource as IInputElement;
            }
            else if (originalSource is DependencyObject)
            {
                return (originalSource as DependencyObject).GetVisualAncestors ().OfType<IInputElement> ().FirstOrDefault ();
            }
            return null;
        }
        private void                OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var originalSource = GetOriginalSource (e);
            var ie = new MouseWheelInputEventArgs (this, e.MouseDevice.GetWheel (), e.Timestamp, e.Delta, Orientation.Vertical);

            OnPreviewMouseWheel (originalSource, ie);

            e.Handled = true;
        }
        private void                OnPreviewMouseWheel(IInputElement originalSource, MouseWheelInputEventArgs ie)
        {
            // Update wheel motion info
            var info = ie.Wheel.PreTransmit (ie.Timestamp, ie.Delta);

            // 1. Tunneling event
            // Clients and behaviors use this tunneling event to update the wheel transfer
            // case by dynamically creating / retrieving motion shafts.
            ie.RoutedEvent = PreviewMouseWheelInputEvent;
            originalSource.RaiseEvent (ie);

            // In cooperation with clients and behaviors, if inputEventArgs.Handled is set to true,
            // the controller lets the underlying mouse wheel tunneling event continue its route.
            if (ie.Handled)
                return;

            // Fill motion reservoir
            ie.Wheel.Transmit (info, ie.Delta, null);
            // 2. Bubbling event
            // Clients consume the motion here
            ie.RoutedEvent = MouseWheelInputEvent;
            originalSource.RaiseEvent (ie);
            // 3. Remaining motion is processed here
            ie.EndCommand ();
        }
        private void                OnPreviewInput(object sender, MouseWheelInputEventArgs e)
        {
            Debug.Assert (sender == Element);
            var client = _clients.FirstOrDefault (c => c.IsActive (e));
            if (client != null)
                client.OnPreviewInput (sender, e);
        }
        private void                OnInput(object sender, MouseWheelInputEventArgs e)
        {
            Debug.Assert (sender == Element);
            e.Controller = this;
            var client = _clients.FirstOrDefault (c => c.IsActive (e));
            if (client != null)
            {
                _exitElement = client.ExitElement;
                client.OnInput (sender, e);
            }
        }
        private void                HookMouseHorizontalWheelEvent(HwndSource source)
        {
            if (source != null && !_hwndSourceHooks.Contains (source.Handle))
            {
                _hwndSourceHooks.Add (source.Handle);
                source.AddHook (MouseHorizontalWheelHook);
            }
        }
        private void                UnhookMouseHorizontalWheelEvent(HwndSource source)
        {
            if (source != null && _hwndSourceHooks.Contains (source.Handle))
            {
                _hwndSourceHooks.Remove (source.Handle);
                source.RemoveHook (MouseHorizontalWheelHook);
            }
        }
        private IntPtr              MouseHorizontalWheelHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_MOUSEHWHEEL:
                    {
                        var timestamp = Environment.TickCount;
                        var originalSource = Mouse.DirectlyOver;
                        var delta = (short)unchecked((uint)(long)wParam >> 16);
                        var ie = new MouseWheelInputEventArgs(this, MouseWheel.Current, timestamp, -delta, Orientation.Horizontal);
                        OnPreviewMouseWheel(originalSource, ie);
                        break;
                    }
            }
            return IntPtr.Zero;
        }

        private static readonly HashSet<IntPtr>     _hwndSourceHooks = new HashSet<IntPtr>();
        private readonly List<IMouseWheelClient>    _clients = new List<IMouseWheelClient>();
        private readonly IInputLevelElement         _inputLevelElement;
        private IInputElement                       _exitElement;
        private ClientType                          _clientType;
        private HwndSource                          _presentationSource;

        private const int WM_MOUSEHWHEEL = 0x020E;
        [Flags]
        private enum ClientType
        {
            Patch = 0x01,
            Adapter = 0x02
        }
    }

    public partial class MouseWheelController
    {
        static MouseWheelController()
        {
            InitializePatchClientFactories ();
        }

        internal static void                                                        BeginEnsurePatchController(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            BeginEnsureController (element, e, ClientType.Patch);
        }
        internal static void                                                        BeginEnsureMapController(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            BeginEnsureController (element, e, ClientType.Adapter);
        }

        private static void                                                         BeginEnsureController(DependencyObject element, DependencyPropertyChangedEventArgs e, ClientType clientType)
        {
            var inputLevelElement = Windows.InputLevelElement.FromElement (element);
            if (inputLevelElement == null)
                return;

            element.Dispatcher.BeginInvoke (
              new PropertyChangedCallback ((element1, e1) => EnsureController (inputLevelElement, element1, clientType)),
              DispatcherPriority.Loaded, element, e);
        }
        private static void                                                         EnsureController(IInputLevelElement inputLevelElement, DependencyObject element, ClientType clientType)
        {
            var clientFactories = GetClientFactories (inputLevelElement, element, clientType).ToArray ();
            if (clientFactories.Length == 0)
                return;
            IMouseWheelController controller;
            if (_controllers.TryGetValue (element, out controller))
            {
                if (0 == ((controller as MouseWheelController)._clientType & clientType))
                {
                    foreach (var clientFactory in clientFactories)
                    {
                        controller.AddClient (clientFactory (controller));
                    }
                    (controller as MouseWheelController)._clientType |= clientType;
                }
            }
            else
            {
                var controllerFactory = GetControllerFactory (inputLevelElement);
                _controllers[element] = controller = controllerFactory (element);
                (controller as MouseWheelController)._clientType |= clientType;
                foreach (var clientFactory in clientFactories)
                {
                    controller.AddClient (clientFactory (controller));
                }
            }
        }
        private static Func<DependencyObject, IMouseWheelController>                GetControllerFactory(IInputLevelElement inputLevelElement)
        {
            if (inputLevelElement is IFrameworkLevelElement)
            {
                return s => new MouseWheelFrameworkLevelController (inputLevelElement as IFrameworkLevelElement);
            }
            else
            {
                return s => new MouseWheelController (inputLevelElement);
            }
        }
        private static void                                                         InitializePatchClientFactories()
        {
            _patchClientFactories.Add (typeof (ScrollViewer), new Func<IMouseWheelController, IMouseWheelClient>[]
            {
                controller => new MouseWheelScrollClient(controller, Orientation.Vertical),
                controller => new MouseWheelScrollClient(controller, Orientation.Horizontal),
            });
            _patchClientFactories.Add (typeof (FlowDocumentPageViewer), new Func<IMouseWheelController, IMouseWheelClient>[]
            {
                controller => new MouseWheelFlowDocumentPageViewerScrollClient(controller),
                controller => new MouseWheelZoomClient(controller),
            });
            _patchClientFactories.Add (typeof (FlowDocumentScrollViewer), new Func<IMouseWheelController, IMouseWheelClient>[]
            {
                controller => new MouseWheelZoomClient(controller),
            });
        }
        private static IEnumerable<Func<IMouseWheelController, IMouseWheelClient>>  GetClientFactories(IInputLevelElement inputLevelElement, DependencyObject element, ClientType clientType)
        {
            if (0 != (clientType & ClientType.Patch))
            {
                var elementType = element.GetType ();
                var key = _patchClientFactories.Keys.FirstOrDefault (t => t.IsAssignableFrom (elementType));
                if (key != null)
                {
                    foreach (var factory in _patchClientFactories[key])
                    {
                        yield return factory;
                    }
                }
            }
            if (0 != (clientType & ClientType.Adapter))
            {
                yield return _adaptationClientFactory;
            }
        }

        private static readonly Func<IMouseWheelController, IMouseWheelClient>      _adaptationClientFactory = controller => new MouseWheelAdaptationClient(controller);
        private static readonly Dictionary<DependencyObject, IMouseWheelController> _controllers = new Dictionary<DependencyObject, IMouseWheelController>();
        private static readonly Dictionary<Type, IEnumerable<Func<IMouseWheelController, IMouseWheelClient>>> _patchClientFactories = new Dictionary<Type, IEnumerable<Func<IMouseWheelController, IMouseWheelClient>>>();
    }
}
