using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceStack.Html;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Utilities.TypeConverters;
using Telerik.Sitefinity.Versioning.Serialization.Serializers.SitefinityBinarySerializer;

namespace DF2023.Versioning
{
    public class BinaryHelper
    {
        public Stream binaryStream;

        private Encoding cultureEncoding;

        public readonly int BlockSize = 2000;

        private byte[] buffer;

        public BinaryHelper(Stream stream, Encoding encoding)
        {
            this.binaryStream = stream;
            this.cultureEncoding = encoding;
        }

        public void WriteJson(string json)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                this.binaryStream.Write(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                Log.Write(new Exception("-- SERIALIZE --", ex));
                throw;
            }
        }

        public object ReadJson(object graph)
        {
            JObject obj = GetPlainJson();

            if (obj == null)
                return null;

            obj.Remove("ItemDefaultUrl");
            obj.Remove("Lifecycle");
            obj.Remove("SystemUrl");
            obj.Remove("AvailableCultures");
            obj.Remove("UrlName");
            obj.Remove("ApprovalWorkflowState");

            foreach (var prop in obj.Properties())
            {
                try
                {
                    var keyType = graph.GetType()
                        .GetProperties()
                        .FirstOrDefault(p => p.Name == prop.Name);

                    if (keyType != null)
                    {
                        var valObj = prop.Value;
                        var converted = Convert.ChangeType(valObj, keyType.PropertyType);
                        ((DynamicContent)graph).SetValue(prop.Name, converted);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }

            return graph;
        }
        
        public JObject GetPlainJson()
        {
            try
            {
                this.binaryStream.Seek(0, SeekOrigin.Begin);
                byte[] result = new byte[this.binaryStream.Length];
                this.binaryStream.Read(result, 0, result.Length);
                var itemDeserialized = Encoding.UTF8.GetString(result);
                var obj = JsonConvert.DeserializeObject<JObject>(itemDeserialized);
                return obj;
            }
            catch
            {
                return null;
            }
        } 

        public void Read(byte[] buffer, int length)
        {
            this.binaryStream.Read(buffer, 0, length);
        }

        /// <summary>
        /// Reads a Byte from the serialization stream.
        /// </summary>
        public int ReadByte()
        {
            return this.binaryStream.ReadByte();
        }

        /// <summary>
        /// Reads an array of bytes from the serialization stream.
        /// </summary>
        /// <param name="count">Number of bytes to read.</param>
        /// <returns>Returns an array of bytes read from the deserialization stream.</returns>
        public byte[] ReadBytes(int count)
        {
            byte[] numArray = new byte[count];
            this.ReadBytes(numArray, 0, count);
            return numArray;
        }

        /// <summary>
        /// Reads an array of bytes from the serialization stream.
        /// </summary>
        /// <param name="bytes">Byte array to read into.</param>
        /// <param name="offset">Starting offset of <paramref>bytes</paramref>.</param>
        /// <param name="count">Number of bytes to read.</param>
        public void ReadBytes(byte[] bytes, int offset, int count)
        {
            this.binaryStream.Read(bytes, offset, count);
        }

        /// <summary>
        /// Reads a Char from the serialization stream.
        /// </summary>
        public char ReadChar()
        {
            this.buffer = new byte[2];
            this.binaryStream.Read(this.buffer, 0, 2);
            return BitConverter.ToChar(this.buffer, 0);
        }

        /// <summary>
        /// Reads culture info from the serialization stream.
        /// </summary>
        public CultureInfo ReadCultureInfo()
        {
            return null;
        }

        /// <summary>
        /// Reads a DateTime from the serialization stream.
        /// </summary>
        public DateTime ReadDateTime()
        {
            return DateTime.FromBinary(this.ReadInt64());
        }

        /// <summary>
        /// Reads a Decimal from the serialization stream.
        /// </summary>
        public decimal ReadDecimal()
        {
            return new decimal(new int[] { this.ReadInt32(), this.ReadInt32(), this.ReadInt32(), this.ReadInt32() });
        }

        /// <summary>
        /// Reads a double-precision floating point value from the serialization stream.
        /// </summary>
        public double ReadDouble()
        {
            this.buffer = new byte[8];
            this.binaryStream.Read(this.buffer, 0, 8);
            return BitConverter.ToDouble(this.buffer, 0);
        }

        /// <summary>
        /// Reads a Guid from the serialization stream.
        /// </summary>
        public Guid ReadGuid()
        {
            byte[] numArray = new byte[16];
            this.binaryStream.Read(numArray, 0, 16);
            return new Guid(numArray);
        }

        /// <summary>
        /// Reads a signed 16-bit value from the serialization stream.
        /// </summary>
        public short ReadInt16()
        {
            this.buffer = new byte[2];
            this.binaryStream.Read(this.buffer, 0, 2);
            return (short)(this.buffer[0] + (this.buffer[1] << 8));
        }

        /// <summary>
        /// Reads a signed 32-bit integer from the serialization stream.
        /// </summary>
        public int ReadInt32()
        {
            this.buffer = new byte[4];
            this.binaryStream.Read(this.buffer, 0, 4);
            return BitConverter.ToInt32(this.buffer, 0);
        }

        /// <summary>
        /// Reads a signed 64-bit value from the serialization stream.
        /// </summary>
        public long ReadInt64()
        {
            this.buffer = new byte[8];
            this.binaryStream.Read(this.buffer, 0, 8);
            return BitConverter.ToInt64(this.buffer, 0);
        }

        /// <summary>
        /// Reads a signed byte from the serialization stream.
        /// </summary>
        public sbyte ReadSByte()
        {
            return (sbyte)this.binaryStream.ReadByte();
        }

        public SerializedObjectFlags ReadSerializationFlags()
        {
            return (SerializedObjectFlags)((byte)this.binaryStream.ReadByte());
        }

        /// <summary>
        /// Reads a single-precision floating point value from the serialization stream.
        /// </summary>
        public float ReadSingle()
        {
            this.buffer = new byte[4];
            this.binaryStream.Read(this.buffer, 0, 4);
            return BitConverter.ToSingle(this.buffer, 0);
        }

        /// <summary>
        /// Reads a string from the serialization stream.
        /// </summary>
        public string ReadString()
        {
            int num = this.ReadUInt24();
            if (num == 16777215)
            {
                return null;
            }
            this.buffer = new byte[num];
            this.binaryStream.Read(this.buffer, 0, num);
            return this.cultureEncoding.GetString(this.buffer);
        }

        /// <summary>
        /// Reads a TimeSpan from the serialization stream.
        /// </summary>
        public TimeSpan ReadTimeSpan()
        {
            return new TimeSpan(this.ReadInt64());
        }

        public Type ReadType()
        {
            int num = this.ReadUInt24();
            byte[] numArray = new byte[num];
            this.Read(numArray, num);
            string str = Encoding.ASCII.GetString(numArray);
            Type type = TypeResolutionService.ResolveType(str, false);
            if (type == null)
            {
                throw new SerializationError(string.Concat("Unable to GetType object type '", str, "."));
            }
            return type;
        }

        /// <summary>
        /// Reads an unsigned 16-bit value from the serialization stream.
        /// </summary>
        public ushort ReadUInt16()
        {
            this.buffer = new byte[2];
            this.binaryStream.Read(this.buffer, 0, 2);
            return (ushort)(this.buffer[0] + (this.buffer[1] << 8));
        }

        public int ReadUInt24()
        {
            return this.ReadByte() + (this.ReadByte() << 8) + (this.ReadByte() << 16);
        }

        /// <summary>
        /// Reads an unsigned 32-bit integer from the serialization stream.
        /// </summary>
        public uint ReadUInt32()
        {
            this.buffer = new byte[4];
            this.binaryStream.Read(this.buffer, 0, 4);
            return BitConverter.ToUInt32(this.buffer, 0);
        }

        /// <summary>
        /// Reads an unsigned 64-bit value from the serialization stream.
        /// </summary>
        public ulong ReadUInt64()
        {
            this.buffer = new byte[8];
            this.binaryStream.Read(this.buffer, 0, 8);
            return BitConverter.ToUInt64(this.buffer, 0);
        }

        /// <summary>
        /// Writes a byte to the serialization stream.
        /// </summary>
        public void Write(byte val)
        {
            this.binaryStream.WriteByte(val);
        }

        /// <summary>
        /// Writes a signed byte to the serialization stream.
        /// </summary>
        public void Write(sbyte val)
        {
            this.binaryStream.WriteByte((byte)val);
        }

        /// <summary>
        /// Writes an array of bytes to the serialization stream.
        /// </summary>
        /// <param name="bytes">Array of bytes to write.</param>
        /// <param name="offset">Offset to begin writing from.</param>
        /// <param name="count">Number of bytes to write.</param>
        public void Write(byte[] bytes, int offset, int count)
        {
            this.binaryStream.Write(bytes, offset, count);
        }

        /// <summary>
        /// Writes an array of bytes to the serialization stream.
        /// </summary>
        /// <param name="bytes">Array of bytes to write.</param>
        public void Write(byte[] bytes)
        {
            this.binaryStream.Write(bytes, 0, (int)bytes.Length);
        }

        /// <summary>
        /// Writes a signed 32-bit value to the serialization stream.
        /// </summary>
        public void Write(int value)
        {
            this.binaryStream.Write(BitConverter.GetBytes(value), 0, 4);
        }

        /// <summary>
        /// Writes an unsigned 32-bit value to the serialization stream.
        /// </summary>
        public void Write(uint value)
        {
            this.binaryStream.Write(BitConverter.GetBytes(value), 0, 4);
        }

        /// <summary>
        /// Writes a string to the serialization stream.
        /// </summary>
        public void Write(string str)
        {
            if (str == null)
            {
                this.WriteUInt24(16777215);
                return;
            }
            int byteCount = this.cultureEncoding.GetByteCount(str);

            //if ((long)byteCount > -1)
            //{
            //    throw new ArgumentException("String is to long for serialization", str);
            //}

            this.WriteUInt24(byteCount);
            this.buffer = new byte[byteCount];
            this.cultureEncoding.GetBytes(str, 0, str.Length, this.buffer, 0);
            this.binaryStream.Write(this.buffer, 0, byteCount);
        }

        /// <summary>
        /// Writes a signed 16-bit value to the stream.
        /// </summary>
        public void Write(short value)
        {
            this.binaryStream.WriteByte((byte)(value & 255));
            this.binaryStream.WriteByte((byte)(value >> 8 & 255));
        }

        /// <summary>
        /// Writes an unsigned 16-bit value to the serialization stream.
        /// </summary>
        public void Write(ushort value)
        {
            this.binaryStream.WriteByte((byte)(value & 255));
            this.binaryStream.WriteByte((byte)(value >> 8 & 255));
        }

        /// <summary>
        /// Writes a signed 64-bit value to the serialization stream.
        /// </summary>
        public void Write(long value)
        {
            this.binaryStream.Write(BitConverter.GetBytes(value), 0, 8);
        }

        /// <summary>
        /// Writes an unsigned 64-bit value to the serialization stream.
        /// </summary>
        public void Write(ulong value)
        {
            this.binaryStream.Write(BitConverter.GetBytes(value), 0, 8);
        }

        /// <summary>
        /// Writes a DateTime to the serialization stream.
        /// </summary>
        public void Write(DateTime value)
        {
            this.Write(value.ToBinary());
        }

        /// <summary>
        /// Writes a TimeSpan to the serialization stream.
        /// </summary>
        public void Write(TimeSpan value)
        {
            this.Write(value.Ticks);
        }

        /// <summary>
        /// Writes a Guid to the serialization stream.
        /// </summary>
        public void Write(Guid value)
        {
            this.Write(value.ToByteArray());
        }

        /// <summary>
        /// Writes a Decimal to the serialization stream.
        /// </summary>
        public void Write(decimal value)
        {
            int[] bits = decimal.GetBits(value);
            this.Write(bits[0]);
            this.Write(bits[1]);
            this.Write(bits[2]);
            this.Write(bits[3]);
        }

        /// <summary>
        /// Writes a double-precision floating point value to the serialization stream.
        /// </summary>
        public void Write(double value)
        {
            this.binaryStream.Write(BitConverter.GetBytes(value), 0, 8);
        }

        /// <summary>
        /// Writes a single-precision floating point value to the serialization stream.
        /// </summary>
        public void Write(float value)
        {
            this.binaryStream.Write(BitConverter.GetBytes(value), 0, 4);
        }

        /// <summary>
        /// Writes a Char to the serialization stream.
        /// </summary>
        public void Write(char value)
        {
            this.binaryStream.Write(BitConverter.GetBytes(value), 0, 2);
        }

        /// <summary>
        /// Writes a CultureInfo structure to the serialization stream.
        /// </summary>
        public void WriteCultureInfo(CultureInfo info)
        {
            throw new NotImplementedException("Not Implemeneted - ");
        }

        public void WriteSerializationFlags(SerializedObjectFlags flags)
        {
            this.binaryStream.WriteByte((byte)flags);
        }

        public void WriteType(Type objectType)
        {
            Type.GetType(objectType.FullName);
            byte[] bytes = Encoding.ASCII.GetBytes(objectType.FullName);
            this.WriteUInt24((int)bytes.Length);
            this.Write(bytes);
        }

        public void WriteUInt24(int value)
        {
            this.Write((byte)(value & 255));
            this.Write((byte)(value >> 8 & 255));
            this.Write((byte)(value >> 16 & 255));
        }
    }
}