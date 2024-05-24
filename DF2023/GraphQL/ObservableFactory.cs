using GraphQL;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DF2023.GraphQL
{
    public class ObservableFactory
    {
        private static Dictionary<string, List<ISubject<object>>> _subjects = new Dictionary<string, List<ISubject<object>>>();

        public static IObservable<object> GetObservable(IResolveFieldContext context, string typeName)
        {
            if (context != null)
            {
                var subFields = context.SubFields;
                var args = context.Arguments;
            }

            if (!_subjects.TryGetValue(typeName, out var subjects))
            {
                subjects = new List<ISubject<object>>();
                _subjects[typeName] = subjects;
            }

            var subject = new ReplaySubject<object>();
            subjects.Add(subject);

            return subject.AsObservable();
        }

        public static void PublishUpdate(string typeName, object update, string Action)
        {
            if (_subjects.TryGetValue(typeName, out var subjects))
            {
                foreach (var subject in subjects)
                {
                    try
                    {
                        subject.OnNext(new Dictionary<string, object>()
                        {
                            { "Item", update },
                            { "Action", Action  }
                        });
                    }
                    catch
                    {
                        // Handle exceptions as needed
                    }
                }
            }
        }
    }
}