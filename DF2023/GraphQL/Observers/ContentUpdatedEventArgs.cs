using Telerik.Sitefinity.Model;

namespace DF2023.GraphQL.Observers
{
    public class ContentUpdatedEventArgs
    {
        public string ItemType { get; set; }
        public string ItemProvider { get; set; }
        public string MasterId { get; set; }
        public IDataItem DataItem { get; set; }

    }
}
