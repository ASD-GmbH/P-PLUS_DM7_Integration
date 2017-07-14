using System;
using System.Collections.Generic;

namespace DM7_PPLUS_Integration.Implementierung
{
    internal class Subject<T> : IObservable<T>
    {
        private readonly List<IObserver<T>> _observers = new List<IObserver<T>>();

        public void Next(T t)
        {
            foreach (var observer in _observers)
            {
                observer.OnNext(t);
            }
        }

        public void Error(Exception ex)
        {
            foreach (var observer in _observers)
            {
                observer.OnError(ex);
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
            return new Unsubscriber(() =>
            {
                if (_observers.Contains(observer)) _observers.Remove(observer);
            });
        }

        private class Unsubscriber : IDisposable
        {
            private Action _action;

            public Unsubscriber(Action action)
            {
                _action = action;
            }

            public void Dispose()
            {
                var action = _action;
                _action = null;
                action?.Invoke();
            }
        }

        public void Completed()
        {
            foreach (var observer in _observers)
            {
                observer.OnCompleted();
            }
        }
    }
}