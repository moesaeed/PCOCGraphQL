using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DF2023.GraphQL.Observers
{
    public class Content
    {
        public event EventHandler<ContentUpdatedEventArgs> ContentUpdated;
        private ContentObserver contentUpdatedObservable = new ContentObserver();

        public Content()
        {
            ContentUpdated += (sender, args) =>
            {
                foreach (var observer in contentUpdatedObservable.Observers)
                {
                    observer.OnNext(args);
                }
            };
        }

        public IObservable<ContentUpdatedEventArgs> ContentUpdatedObservable
        {
            get { return contentUpdatedObservable; }
        }
    }
}
