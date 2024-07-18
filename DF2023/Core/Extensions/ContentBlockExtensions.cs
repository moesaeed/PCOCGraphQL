using System.Linq;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Modules.GenericContent;

namespace DF2023.Core.Extensions
{
    public static class ContentBlockExtensions
    {
        public static ContentItem GetContentItemByTitle(string title)
        {
            ContentManager manager = ContentManager.GetManager();
            ContentItem contentItem = null;
            using (new ElevatedModeRegion(manager))
            {
                contentItem = manager.GetContent().Where(cI => (cI.Title != null && cI.Title == title && cI.Status == ContentLifecycleStatus.Live)).FirstOrDefault();
            }

            return contentItem;
        }
    }
}