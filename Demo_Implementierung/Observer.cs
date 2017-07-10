using System;

namespace Demo_Implementierung
{
    /// <summary>
    /// Simple IObserver Implementierung für diese Demo. Im Echteinsatz bitte z.B. Reactive Extensions verwenden.
    /// </summary>
    internal class Observer<T> : IObserver<T>
    {
        private readonly Action<T> _onNext;
        private readonly Action<Exception> _onError;

        public Observer(Action<T> onNext, Action<Exception> onError)
        {
            _onNext = onNext;
            _onError = onError;
        }

        public void OnNext(T value)
        {
            _onNext(value);
        }

        public void OnError(Exception error)
        {
            _onError(error);
        }

        public void OnCompleted()
        {
            throw new NotSupportedException("OnComplete ist für "+typeof(T).Name+" nicht vorgesehen.");
        }
    }



}