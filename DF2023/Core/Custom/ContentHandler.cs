using System;
using System.Collections.Generic;
using Telerik.Sitefinity.DynamicModules.Model;

namespace DF2023.Core.Custom
{
    public abstract class ContentHandler
    {
        public abstract bool IsDataValid(Dictionary<string, Object> contextValue);

        public abstract void PreProcessData(Dictionary<string, Object> contextValue);

        public abstract void PostProcessData(DynamicContent item);
    }
}