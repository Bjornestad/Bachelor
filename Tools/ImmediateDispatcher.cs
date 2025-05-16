using System;
using Avalonia.Threading;

namespace Bachelor.Tools
{
    public class ImmediateDispatcher : IDispatcher
    {
        public bool CheckAccess() => true;

        public void VerifyAccess() { }

        public void Post(Action action, DispatcherPriority priority)
        {
            action();
        }

        public void Post(Action action)
        {
            action();
        }

        public DispatcherOperation PostWithResult(Action action, DispatcherPriority priority)
        {
            action();
            return null;
        }

        public DispatcherOperation PostWithResult(Action action)
        {
            return PostWithResult(action, DispatcherPriority.Normal);
        }
    }
}