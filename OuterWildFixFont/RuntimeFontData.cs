using System;
using System.IO;
using System.Text;

namespace OuterWildFixFont
{
    // Unity's legacy Font cannot import a loose TTF in this player. Supply its
    // serialized font-data field instead. The embedded template has no font data.
    internal static class RuntimeFontData
    {
        internal static byte[] BuildBundle(byte[] ttf)
        {
            var metrics = ReadMetrics(ttf);
            using (var resource = typeof(RuntimeFontData).Assembly.GetManifestResourceStream(
                       "OuterWildFixFont.FontTemplate"))
            {
                if (resource == null) throw new InvalidOperationException("Font template is missing.");
                using (var reader = new BinaryReader(resource))
                {
                    if (reader.ReadUInt32() != 0x4646574F || reader.ReadInt32() != 1)
                        throw new InvalidDataException("Unsupported font template.");
                    int insert = reader.ReadInt32();
                    int sizeOffset = reader.ReadInt32();
                    int objectEnd = reader.ReadInt32();
                    var metricOffsets = new int[4];
                    for (int i = 0; i < metricOffsets.Length; i++) metricOffsets[i] = reader.ReadInt32();
                    var template = reader.ReadBytes(checked((int)(resource.Length - resource.Position)));
                    int paddedFontSize = checked((ttf.Length + 3) & ~3);
                    int delta = paddedFontSize - 16;
                    int end = checked(objectEnd + delta);
                    var serialized = new byte[checked((end + 7) & ~7)];
                    Buffer.BlockCopy(template, 0, serialized, 0, insert);
                    Buffer.BlockCopy(ttf, 0, serialized, insert, ttf.Length);
                    Buffer.BlockCopy(template, insert + 16, serialized, insert + paddedFontSize,
                        objectEnd - insert - 16);
                    PutInt32(serialized, insert - 4, ttf.Length);
                    PutInt32(serialized, sizeOffset, BitConverter.ToInt32(template, sizeOffset) + delta);
                    PutBigEndian(serialized, 4, serialized.Length, 4);
                    for (int i = 0; i < metrics.Length; i++)
                    {
                        int position = metricOffsets[i];
                        if (position >= insert + 16) position += delta;
                        Buffer.BlockCopy(BitConverter.GetBytes(metrics[i]), 0, serialized, position, 4);
                    }
                    return WrapBundle(serialized);
                }
            }
        }

        private static float[] ReadMetrics(byte[] font)
        {
            if (font.Length < 12 || font.Length > 64 * 1024 * 1024)
                throw new InvalidDataException("Invalid font file size.");
            int signature = BigInt32(font, 0);
            if (signature != 0x00010000 && signature != 0x4F54544F)
                throw new InvalidDataException("Expected a single-face TTF or OTF file.");
            int head = -1, hhea = -1;
            int count = BigUInt16(font, 4);
            if (12L + count * 16L > font.Length) throw new InvalidDataException("Truncated font directory.");
            for (int i = 0; i < count; i++)
            {
                int record = 12 + i * 16;
                int offset = BigInt32(font, record + 8);
                int length = BigInt32(font, record + 12);
                if (offset < 0 || length < 0 || (long)offset + length > font.Length)
                    throw new InvalidDataException("Font table is outside the file.");
                int tag = BigInt32(font, record);
                if (tag == 0x68656164 && length >= 54) head = offset;
                if (tag == 0x68686561 && length >= 36) hhea = offset;
            }
            if (head < 0 || hhea < 0) throw new InvalidDataException("Font metrics are missing.");
            int units = BigUInt16(font, head + 18);
            if (units < 16 || units > 16384) throw new InvalidDataException("Invalid font units per em.");
            float scale = 48f / units;
            float ascent = (short)BigUInt16(font, hhea + 4) * scale;
            float descent = (short)BigUInt16(font, hhea + 6) * scale;
            float gap = Math.Max(0, (int)(short)BigUInt16(font, hhea + 8)) * scale;
            if (ascent <= descent) throw new InvalidDataException("Invalid font ascender/descender.");
            return new[] { 48f, ascent - descent + gap, ascent, descent };
        }

        private static byte[] WrapBundle(byte[] serialized)
        {
            using (var blocks = new MemoryStream())
            using (var output = new MemoryStream())
            {
                blocks.Write(new byte[16], 0, 16); // No compressed-data hash.
                WriteBigEndian(blocks, 1, 4); // One uncompressed data block.
                WriteBigEndian(blocks, serialized.Length, 4);
                WriteBigEndian(blocks, serialized.Length, 4);
                WriteBigEndian(blocks, 0, 2);
                WriteBigEndian(blocks, 1, 4); // One serialized file.
                WriteBigEndian(blocks, 0, 8);
                WriteBigEndian(blocks, serialized.Length, 8);
                WriteBigEndian(blocks, 4, 4);
                WriteString(blocks, "CAB-OuterWildFixFont-RuntimeFont");

                WriteString(output, "UnityFS");
                WriteBigEndian(output, 7, 4);
                WriteString(output, "5.x.x");
                WriteString(output, "2019.4.39f1c1");
                long sizePosition = output.Position;
                WriteBigEndian(output, 0, 8);
                WriteBigEndian(output, blocks.Length, 4);
                WriteBigEndian(output, blocks.Length, 4);
                WriteBigEndian(output, 0x40, 4);
                while ((output.Position & 15) != 0) output.WriteByte(0);
                blocks.Position = 0;
                blocks.CopyTo(output);
                output.Write(serialized, 0, serialized.Length);
                long size = output.Length;
                output.Position = sizePosition;
                WriteBigEndian(output, size, 8);
                return output.ToArray();
            }
        }

        private static int BigUInt16(byte[] bytes, int position) => (bytes[position] << 8) | bytes[position + 1];
        private static int BigInt32(byte[] bytes, int position) =>
            (BigUInt16(bytes, position) << 16) | BigUInt16(bytes, position + 2);

        private static void PutInt32(byte[] bytes, int offset, int value) =>
            Buffer.BlockCopy(BitConverter.GetBytes(value), 0, bytes, offset, 4);

        private static void PutBigEndian(byte[] bytes, int offset, long value, int count)
        {
            for (int i = count - 1; i >= 0; i--)
            {
                bytes[offset + i] = (byte)value;
                value >>= 8;
            }
        }

        private static void WriteBigEndian(Stream stream, long value, int count)
        {
            var bytes = new byte[count];
            PutBigEndian(bytes, 0, value, count);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static void WriteString(Stream stream, string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
            stream.WriteByte(0);
        }
    }
}
