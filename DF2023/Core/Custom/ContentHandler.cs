using System;
using System.Collections.Generic;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Model;

namespace DF2023.Core.Custom
{
    public abstract class ContentHandler
    {
        public string TitleValue { get; set; }

        public abstract bool IsDataValid(Dictionary<string, Object> contextValue, out string errorMsg);

        public abstract void PreProcessData(Dictionary<string, Object> contextValue);

        public abstract void PostProcessData(DynamicContent item, Dictionary<string, Object> contextValue = null);

        public abstract void DuringProcessData(DynamicContent item, Dictionary<string, Object> contextValue);
        public abstract void PostProcessRelateItem(DynamicContent item, string normalizedFieldName, IDataItem toRelate);
    }

    public class DefaultContentHandler : ContentHandler
    {
        public override void PreProcessData(Dictionary<string, object> contextValue)
        {
            // No pre-processing for unsupported types
        }

        public override void PostProcessData(DynamicContent item, Dictionary<string, Object> contextValue = null)
        {
            // No post-processing for unsupported types
        }

        public override bool IsDataValid(Dictionary<string, object> contextValue, out string errorMsg)
        {
            errorMsg = null;
            return true;
        }

        public override void DuringProcessData(DynamicContent item, Dictionary<string, object> contextValue)
        {
            // No During-Processing for unsupported types
        }

        public override void PostProcessRelateItem(DynamicContent item, string normalizedFieldName, IDataItem toRelate)
        {
            // No PostProcessRelateItem for unsupported types
        }
    }
}