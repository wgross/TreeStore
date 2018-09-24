using System;
using System.Windows.Threading;

namespace Kosmograph.Desktop.Graph
{
    public static class DispatcherObjectExtensions
    {
        public static void InvokeInUiThread<T>(this T dispatcherObject, Action action) where T : DispatcherObject
        {
            if (dispatcherObject.Dispatcher.CheckAccess())
                action();
            else
                dispatcherObject.Dispatcher.Invoke(action);
        }

        public static R InvokeInUiThread<T, R>(this T dispatcherObject, Func<R> action) where T : DispatcherObject
        {
            if (dispatcherObject.Dispatcher.CheckAccess())
                return action();
            else
                return dispatcherObject.Dispatcher.Invoke(action);
        }
    }
}