using System.Globalization;
using System.IO;
using Telerik.Sitefinity.Versioning.Serialization.Interfaces;

namespace DF2023.Versioning
{
    public class DFVersionFormatter : ILocalizableSitefinityFormatter, ISitefinityFormatter
    {
        private CultureInfo cultureToPersist;

        private bool persistSpecificCulture;

        public bool PersistSpecificCulture
        {
            get
            {
                return persistSpecificCulture;
            }
            set
            {
                persistSpecificCulture = value;
            }
        }

        public string CultureNameToPersist
        {
            set
            {
                if (value != null)
                {
                    cultureToPersist = new CultureInfo(value);
                }
                else
                {
                    cultureToPersist = null;
                }
            }
        }
        public void Deserialize(byte[] serializedObject, object graph)
        {
            using (MemoryStream serializationStream = new MemoryStream(serializedObject))
            {
                Deserialize(serializationStream, ref graph);
            }
        }

        public void Deserialize(byte[] serializedObject, ref object graph)
        {
            using (MemoryStream serializationStream = new MemoryStream(serializedObject))
            {
                Deserialize(serializationStream, ref graph);
            }
        }

        public void Deserialize(Stream serializationStream, ref object graph)
        {
            MyBinarySerializer serializer = new MyBinarySerializer();

            if (this.persistSpecificCulture)
            {
                serializer.PersistSpecificCulture = this.cultureToPersist;
            }

            byte[] bytes = new byte[serializationStream.Length];
            serializationStream.Read(bytes, 0, bytes.Length);
            serializer.Deserialize(bytes, ref graph);
        }

        public void Deserialize(Stream serializationStream, object graph)
        {
            Deserialize(serializationStream, ref graph);
        }

        public byte[] Serialize(object graph)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                Serialize(memoryStream, graph);
                return memoryStream.ToArray();
            }
        }

        public void Serialize(Stream serializationStream, object graph)
        {
            MyBinarySerializer serializer = new MyBinarySerializer();
            if (this.persistSpecificCulture)
            {
                serializer.PersistSpecificCulture = this.cultureToPersist;
            }
            serializer.Serialize(serializationStream, graph);
        }
    }
}