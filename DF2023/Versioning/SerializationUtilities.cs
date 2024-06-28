using System;

namespace DF2023.Versioning
{
    public class SerializationUtilities
    {
        public static object GetInstance(Type objectType)
        {
            object empty;
            try
            {
                if (objectType == typeof(Type))
                {
                    empty = null;
                }
                else if (objectType != typeof(string))
                {
                    empty = (!objectType.IsArray ? Activator.CreateInstance(objectType) : Activator.CreateInstance(objectType, new object[] { 0 }));
                }
                else
                {
                    empty = string.Empty;
                }
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                if (!objectType.IsValueType)
                {
                    throw new ArgumentException(string.Concat("Unable to create instance of type : ", objectType.FullName), "objectType", exception);
                }
                empty = null;
            }
            return empty;
        }
    }
}