﻿using DF2023.Core.Constants;

namespace DF2023.Core.Custom
{
    public static class ContentHandlerFactory
    {
        public static ContentHandler GetHandler(string contentType)
        {
            switch (contentType)
            {
                case Delegation.DelegationDynamicTypeName:
                    return new DelegationManager();

                case Guest.GuestDynamicTypeName:
                    return new GuestManager();

                default:
                    return new DefaultContentHandler();
            }
        }
    }
}