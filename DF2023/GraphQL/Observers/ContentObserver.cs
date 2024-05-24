using System;
using System.Collections.Generic;
using Telerik.Sitefinity.Model;

namespace DF2023.GraphQL.Observers
{
    /// <summary>
    /// Contains the logic for update events routing to all subscribers.
    /// </summary>
    public class ContentObserver : IObservable<ContentUpdatedEventArgs>
    {
        public List<IObserver<ContentUpdatedEventArgs>> Observers = new List<IObserver<ContentUpdatedEventArgs>>();

        public IDisposable Subscribe(IObserver<ContentUpdatedEventArgs> observer)
        {
            if (!Observers.Contains(observer))
                Observers.Add(observer);
            return new Unsubscriber(Observers, observer);
        }
    }

    internal class Unsubscriber : IDisposable
    {
        private List<IObserver<ContentUpdatedEventArgs>> _observers;
        private IObserver<ContentUpdatedEventArgs> _observer;

        public Unsubscriber(List<IObserver<ContentUpdatedEventArgs>> observers, IObserver<ContentUpdatedEventArgs> observer)
        {
            this._observers = observers;
            this._observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }
}