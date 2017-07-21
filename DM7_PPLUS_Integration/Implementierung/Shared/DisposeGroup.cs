using System;
using System.Collections.Generic;

namespace DM7_PPLUS_Integration.Implementierung.Shared
{
    sealed class DisposeGroup : IDisposable
    {
        private Stack<IDisposable> _disposeables = new Stack<IDisposable>();

        public void Dispose()
        {
            var dispose = _disposeables;
            _disposeables = new Stack<IDisposable>();

            while (dispose.Count > 0) dispose.Pop().Dispose();
        }

        public DisposeGroup With(IDisposable disposable)
        {
            if (disposable!=null) _disposeables.Push(disposable);
            return this;
        }

        public DisposeGroup With(Action onDispose)
        {
            return With(new DisposeAction(onDispose));
        }
    }

    static class DisposeGroupExtensions
    {
        public static T WithDisposeGroup<T>(this T disposable, DisposeGroup disposegroup) where T : IDisposable
        {
            disposegroup.With(disposable);
            return disposable;
        }
    }

    public abstract class DisposeGroupMember : IDisposable
    {
        private readonly IDisposable _disposegroup;

        protected DisposeGroupMember(IDisposable disposegroup)
        {
            _disposegroup = disposegroup;
            if (disposegroup is DisposeGroup)
            {
                ((DisposeGroup) disposegroup).With(this);
            }
        }

        public void Dispose()
        {
            _disposegroup.Dispose();
        }
    }

    sealed class DisposeAction : IDisposable
    {
        private Action _onDispose;

        public DisposeAction(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            var dispose = _onDispose;
            _onDispose = null;
            if (dispose!=null) dispose();
        }
    }
}