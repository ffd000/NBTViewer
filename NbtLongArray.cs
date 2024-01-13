using System;
using System.Text;
using JetBrains.Annotations;

/// <summary>
/// fNbt doesn't have a LongArrayTag implementation and can't typically read the
/// latest Anvil region format without it.
/// </summary>
namespace fNbt
{
    /// <summary> A tag containing an array of signed longs. </summary>
    public sealed class NbtLongArray : NbtTag
    {
        static readonly long[] ZeroArray = new long[0];

        public override NbtTagType TagType
        {
            get { return NbtTagType.LongArray; }
        }

        [NotNull]
        public long[] Value
        {
            get { return longs; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                longs = value;
            }
        }

        [NotNull]
        long[] longs;

        public NbtLongArray()
            : this((string)null) { }


        public NbtLongArray([NotNull] long[] value)
            : this(null, value) { }


        public NbtLongArray([CanBeNull] string tagName)
        {
            name = tagName;
            longs = ZeroArray;
        }

        public NbtLongArray([CanBeNull] string tagName, [NotNull] long[] value)
        {
            if (value == null) throw new ArgumentNullException("value");
            name = tagName;
            longs = (long[])value.Clone();
        }


        public NbtLongArray([NotNull] NbtLongArray other)
        {
            if (other == null) throw new ArgumentNullException("other");
            name = other.name;
            longs = (long[])other.Value.Clone();
        }

        public new long this[int tagIndex]
        {
            get { return Value[tagIndex]; }
            set { Value[tagIndex] = value; }
        }


        internal override bool ReadTag(NbtBinaryReader readStream)
        {
            int length = readStream.ReadInt32();
            if (length < 0)
            {
                throw new NbtFormatException("Negative length given in TAG_Long_Array");
            }

            if (readStream.Selector != null && !readStream.Selector(this))
            {
                readStream.Skip((length * sizeof(long)));
                return false;
            }

            Value = new long[length];
            for (int i = 0; i < length; i++)
            {
                Value[i] = readStream.ReadInt64();
            }
            return true;
        }


        internal override void SkipTag(NbtBinaryReader readStream)
        {
            int length = readStream.ReadInt32();
            if (length < 0)
            {
                throw new NbtFormatException("Negative length given in TAG_Long_Array");
            }
            readStream.Skip((length * sizeof(long)));
        }

        internal override void WriteTag(NbtBinaryWriter writeStream)
        {
            writeStream.Write(NbtTagType.LongArray);
            if (Name == null) throw new NbtFormatException("Name is null");
            writeStream.Write(Name);
            WriteData(writeStream);
        }


        internal override void WriteData(NbtBinaryWriter writeStream)
        {
            writeStream.Write(Value.Length);
            for (int i = 0; i < Value.Length; i++)
            {
                writeStream.Write(Value[i]);
            }
        }


        public override object Clone()
        {
            return new NbtLongArray(this);
        }

        internal override void PrettyPrint(StringBuilder sb, string indentString, int indentLevel)
        {
            for (int i = 0; i < indentLevel; i++)
            {
                sb.Append(indentString);
            }
            sb.Append("TAG_Long_Array");
            if (!String.IsNullOrEmpty(Name))
            {
                sb.AppendFormat("(\"{0}\")", Name);
            }
            sb.AppendFormat(": [{0} longs]", longs.Length);
        }
    }
}
