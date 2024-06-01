using System;
using System.Collections.Generic;
using Telerik.Sitefinity.DynamicModules.Model;

namespace DF2023.Core.Custom
{
    public abstract class ContentHandler
    {
        public string TitleValue { get; set; }

        public abstract bool IsDataValid(Dictionary<string, Object> contextValue, out string errorMsg);

        public abstract void PreProcessData(Dictionary<string, Object> contextValue);

        public abstract void PostProcessData(DynamicContent item);
    }

    public class DefaultContentHandler : ContentHandler
    {
        public override void PreProcessData(Dictionary<string, object> contextValue)
        {
            // No pre-processing for unsupported types
        }

        public override void PostProcessData(DynamicContent item)
        {
            // No post-processing for unsupported types
        }

        public override bool IsDataValid(Dictionary<string, object> contextValue, out string errorMsg)
        {
            errorMsg = null;
            return true;
        }
    }
}