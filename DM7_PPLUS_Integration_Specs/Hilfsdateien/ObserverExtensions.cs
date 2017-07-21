using System;

namespace DM7_PPLUS_Integration_Specs
{
    public static class ObserverExtensions
    {
        public static IDisposable Subscribe<T>(this IObservable<T> observable, Action<T> onNext) =>
            observable.Subscribe(new Observer<T>(onNext,ex => { throw ex; }));
    }
}