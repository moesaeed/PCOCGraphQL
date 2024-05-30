using System;

namespace DF2023.Core.Custom
{
    public static class ContentHandlerFactory
    {
        public static ContentHandler GetHandler(string contentType)
        {
            switch (contentType.ToLower())
            {
                case "article":
                    return new DelegationManager();

                default:
                    throw new NotSupportedException($"Content type '{contentType}' is not supported.");
            }
        }
    }
}