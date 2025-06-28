using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Advanced;
//This implementation is based off of:
// - https://github.com/sebamarynissen/sc4/blob/main/src/core/fsh.ts
// - https://github.com/sebamarynissen/sc4/blob/main/src/core/bitmap-decompression.ts
// - https://github.com/mafaca/Dxt/blob/master/Dxt/DxtDecoder.cs

namespace csDBPF {
    /// <summary>
	/// An implementation of <see cref="DBPFEntry"/> for FSH entries.
	/// </summary>
	/// <see href="https://www.wiki.sc4devotion.com/index.php?title=FSH"/>
    /// <seealso href="https://www.wiki.sc4devotion.com/index.php?title=FSH_Format"/>
    public class DBPFEntryFSH : DBPFEntry {
        /// <summary>
        /// A FSH file may contain one or more entries, though commonly this is limited to one.
        /// </summary>
        public List<FSHEntry> Entries { get; private set; }
        /// <summary>
        /// An image created from the first entry, if it exists.
        /// </summary>
        public Image<Rgba32> Image => Entries.Count > 0 ? Entries[0].Image : throw new InvalidOperationException("No image entries available");


        /// <summary>
        /// Create a new instance. Use when creating a new FSH entry.
        /// </summary>
        /// <param name="tgi"></param>
        public DBPFEntryFSH(TGI tgi) : base(tgi) {
            Entries = [];
        }
        /// <summary>
		/// Create a new instance. Use when reading an existing FSH entry from a file.
		/// </summary>
		/// <param name="tgi"><see cref="DBPFTGI"/> object representing the entry</param>
		/// <param name="offset">Offset (location) of the entry within the DBPF file</param>
		/// <param name="size">Compressed size of data for the entry, in bytes. Uncompressed size is also temporarily set to this to this until the data is set</param>
		/// <param name="index">Entry position in the file, 0-n</param>
		/// <param name="bytes">Byte data for this entry</param>
        public DBPFEntryFSH(TGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {
            Entries = [];
        }



        /// <summary>
        /// Decompresses this entry and sets bitmap data from byte data.
        /// </summary>
        public override void Decode() {
            byte[] dData;
            if (QFS.IsCompressed(ByteData)) {
                dData = QFS.Decompress(ByteData);
            } else {
                dData = ByteData;
            }

            MemoryStream ms = new MemoryStream(dData);
            BinaryReader br = new BinaryReader(ms);

            //Analyze the FSH file header
            string identifier = br.ReadString(4);
            if (identifier != "SHPI") {
                throw new InvalidDataException("Invalid FSH file");
            }
            int fileSize = br.ReadInt32();
            int bitmapCount = br.ReadInt32();
            string dirId = br.ReadString(4);


            //Fill the FSH Directory. This tells us the location in the file of each bitmap.
            var directory = new List<FSHDirectoryItem>();
            for (int idx = 0; idx < bitmapCount; idx++) {
                string name = br.ReadString(4);
                int offset = br.ReadInt32();
                directory.Add(new FSHDirectoryItem(name, offset));
            }

            //After the directory is built, look at the header information each bitmap in the file
            foreach (FSHDirectoryItem di in directory) {
                BinaryReader streamData = br.GetStreamAt(di.Offset);
                FSHEntry entry = new FSHEntry(di.Name);
                entry.Parse(streamData);
                Entries.Add(entry);
            }
        }


        /// <summary>
		/// Build <see cref="DBPFEntry.ByteData"/> from the current state of this instance.
		/// </summary>
		/// <param name="compress">Whether to compress the ByteData. Default is FALSE</param>
        public override void Encode(bool compress = false) {
            throw new NotImplementedException();
        }



        /// <summary>
        /// Stores the name and offset of each bitmap item in this subfile.
        /// </summary>
        /// <see href="https://www.wiki.sc4devotion.com/index.php?title=FSH_Format#FSH_Directory"/>
        /// <param name="name">4 byte name of this item</param>
        /// <param name="offset">Offset of the entry in the file</param>
        private readonly struct FSHDirectoryItem(string name, int offset) {
            /// <summary>
            /// The entry name sometimes has significance. When searching for a global palette for 8-bit bitmaps, the directory entry name for the gobal palette will always <c>!pal</c>. Once the <c>!pal</c> directory entry has been found, the global palette can be extracted and used for any bitmaps that use 8-bit indexed color. If no global palette is found, FSH decoders should look for a local palette directly following the indexed bitmap. If no palette is found, then no palette will be created or associated with the bitmap. Most tools simply ignore missing palettes and save the bitmap with an empty palette with all indexes set to black.
            /// </summary>
            public readonly string Name = name;
            /// <summary>
            /// Offset of the entry in the file.
            /// </summary>
            public readonly int Offset = offset;
        }




        /// <summary>
        /// Defines the bitmap type stored in this file.
        /// </summary>
        /// <see href="https://www.wiki.sc4devotion.com/index.php?title=FSH_Format#Bitmap_or_Palette_Data"/>
        public enum BitmapType {
            /// <summary>
            /// Type: 8-bit indexed<br/>Palette: directly follows bitmap or uses global palette<br/>Compression: none
            /// </summary>
            EightBit = 0x7B,
            /// <summary>
            /// Type: 32-bit A8 R8 G8 B8<br/>Palette: none<br/>Compression: none
            /// </summary>
            ThirtyTwoBit = 0x7D,
            /// <summary>
            /// Type: 24-bit A0 R8 G8 B8<br/>Palette: none<br/>Compression: none
            /// </summary>
            TwentyFourBit = 0x7F,
            /// <summary>
            /// Type: 16-bit A1 R5 G5 B5<br/>Palette: none<br/>Compression: none
            /// </summary>
            SixteenBitAlpha5 = 0x7E,
            /// <summary>
            /// Type: 16-bit A0 R5 G6 B5<br/>Palette: none<br/>Compression: none
            /// </summary>
            SixteenBit = 0x78,
            /// <summary>
            /// Type: 16-bit A4 R4 G4 B4<br/>Palette: none<br/>Compression: none
            /// </summary>
            SixteenBitAlpha4 = 0x6D,
            /// <summary>
            /// Type: DXT3 4x4 packed, 4-bit alpha<br/>Palette: none<br/>Compression: 4x4 grid compressed, half-byte per pixel
            /// </summary>
            DXT3 = 0x61,
            /// <summary>
            /// Type: DXT1 4x4 packed, 1-bit alpha<br/>Palette: none<br/>Compression: 4x4 grid compressed, half-byte per pixel
            /// </summary>
            DXT1 = 0x60
        }



        

        


        
        /// <summary>
        /// Holds information about the bitmap data.
        /// </summary>
        /// <param name="name">Name of this entry, as read from <see cref="FSHDirectoryItem.Name"/></param>
        public class FSHEntry(string? name) {
            /// <summary>
            /// Entry name. See <see cref="FSHDirectoryItem.Name"/>.
            /// </summary>
            public string Name { get; set; } = name ?? "0000";
            /// <summary>
            /// Record identifier.
            /// </summary>
            public int Id { get; set; } = 0x00;
            /// <summary>
            /// Size of the block including this header, only used if the file contains an attachment or embedded mipmaps; it is zero otherwise.<br></br><br></br>
            /// - For single images this is usually: width x height + 10h(hex).<br></br>
            /// - For images with embedded mipmaps, this is the total size of the original image, plus all mipmaps, plus the header.<br></br><br></br>
            /// In either case, it may include additional data as a binary attachment with unknown format.
            /// </summary>
            public int Size { get; set; }
            /// <summary>
            /// Bitmap width in pixels.
            /// </summary>
            public ushort Width { get; set; }
            /// <summary>
            /// Bitmap height in pixels.
            /// </summary>
            public ushort Height { get; set; }
            /// <summary>
            /// [X, Y] coordinate for the center of the image. Not used in SC4.
            /// </summary>
            private ushort[] Center { get; set; } = new ushort[2];
            /// <summary>
            /// [X, Y] position to display the image from the [left, top]. Not used in SC4.
            /// </summary>
            private ushort[] Offset { get; set; } = new ushort[2];
            /// <summary>
            /// List of embedded mipmaps in this file, if any.
            /// </summary>
            public List<FSHImageData> Mipmaps { get; private set; } = [];
            /// <summary>
            /// An image created from the first mipmap, if it exists.
            /// </summary>
            public Image<Rgba32> Image => Mipmaps.Count > 0 ? Mipmaps[0].ToImage() : throw new InvalidOperationException("No mipmaps available");
            

            /// <summary>
            /// Specifies how this bitmap's pixel and color information is saved, and if it is compressed.
            /// </summary>
            public BitmapType Code => (BitmapType) (Id & 0x7F);

            internal void Parse(BinaryReader br) {
                Id = br.ReadByte();
                Size =  (br.ReadByte() + (br.ReadByte() << 8) + (br.ReadByte() << 16));
                Width = br.ReadUInt16();
                Height = br.ReadUInt16();
                Center[0] = br.ReadUInt16();
                Center[1] = br.ReadUInt16();
                ushort ox = br.ReadUInt16();
                ushort oy = br.ReadUInt16();
                Offset[0] = (ushort) (ox & 0xFFFFFF);
                Offset[1] = (ushort) (oy & 0xFFFFFF);

                double sizeFactor = GetSizeFactor(Code);
                byte[] data = br.ReadBytes((int) (Width * Height * sizeFactor));
                var image = new FSHImageData(Code, Width, Height, data);

                Mipmaps = [image];

                int numMipMaps = oy >> 24;
                for (int i = 0; i < numMipMaps; i++) {
                    int factor = 1 << (i + 1);
                    int mipWidth = Width / factor;
                    int mipHeight = Height / factor;
                    byte[] mipData = br.ReadBytes((int) (mipWidth * mipHeight * sizeFactor));
                    var mipmap = new FSHImageData(Code, mipWidth, mipHeight, mipData);
                    Mipmaps.Add(mipmap);
                }
            }
        }

        private static double GetSizeFactor(BitmapType code) {
            switch (code) {
                case BitmapType.EightBit: return 1;
                case BitmapType.ThirtyTwoBit: return 4;
                case BitmapType.TwentyFourBit: return 3;
                case BitmapType.DXT1: return 0.5;
                case BitmapType.DXT3: return 1;
                default:
                    break;
            }
            throw new Exception($"Unknown FSH format 0x${code}");
        }

        /// <summary>
        /// Contains the raw, encoded and potentially compressed image data. This is the entry point for actually getting the raw bitmap.
        /// </summary>
        /// <param name="code">Specifies the type of compression</param>
        /// <param name="width">Bitmap width in pixels</param>
        /// <param name="height">Bitmap height in pixels</param>
        /// <param name="data">Compressed bitmap data</param>
        public class FSHImageData(BitmapType code, int width, int height, byte[] data) {
            /// <summary>
            /// Specifies the type of compression for <see cref="Data"/>
            /// </summary>
            public BitmapType Code { get; private set; } = code;
            /// <summary>
            /// Bitmap width in pixels.
            /// </summary>
            public int Width { get; private set; } = width;
            /// <summary>
            /// Bitmap height in pixels.
            /// </summary>
            public int Height { get; private set; } = height;
            /// <summary>
            /// Compressed bitmap data.
            /// </summary>
            public byte[] Data { get; private set; } = data;
            /// <summary>
            /// Decompressed bitmap data.
            /// </summary>
            public byte[] Bitmap { get; private set; } = [];

            /// <summary>
            /// Decompresses bitmap data
            /// </summary>
            /// <exception cref="InvalidOperationException">Data is null and cannot be decompressed.</exception>
            /// <exception cref="NotSupportedException">Unknown bitmap format</exception>
            public void Decompress() {
                if (Bitmap.Length > 0) return;
                if (Data.Length == 0) throw new InvalidOperationException("Data is null and cannot be decompressed.");

                switch (Code) {
                    case BitmapType.EightBit: Bitmap = Decompress8Bit(Data); return;
                    case BitmapType.ThirtyTwoBit: Bitmap = Decompress32Bit(Data); return;
                    case BitmapType.TwentyFourBit: Bitmap = Decompress24Bit(Data); return;
                    case BitmapType.SixteenBitAlpha5: Bitmap = Decompress1555(Data); return;
                    case BitmapType.SixteenBit: Bitmap = Decompress0565(Data); return;
                    case BitmapType.SixteenBitAlpha4: Bitmap = Decompress444(Data); return;
                    case BitmapType.DXT3: Bitmap = DecompressDXT3(Data, Width, Height); return;
                    case BitmapType.DXT1: Bitmap = DecompressDXT1(Data, Width, Height); return;
                    default:
                        throw new NotSupportedException($"Unknown bitmap format 0x{Code:X}");
                }
            }

            /// <summary>
            /// Converts the decompressed bitmap data to a useable image format, decompressing if necessary.
            /// </summary>
            /// <returns>An ImageSharp <see cref="Image{Rgba32}"/> image</returns>
            public Image<Rgba32> ToImage() {
                if (Bitmap.Length == 0) {
                    Decompress();
                }
                
                var image = new Image<Rgba32>(Width, Height);
                int pixelIndex = 0;

                image.ProcessPixelRows(accessor => {
                    for (int y = 0; y < Height; y++) {
                        Span<Rgba32> row = accessor.GetRowSpan(y);
                        for (int x = 0; x < Width; x++) {
                            byte r = Bitmap[pixelIndex++];
                            byte g = Bitmap[pixelIndex++];
                            byte b = Bitmap[pixelIndex++];
                            byte a = Bitmap[pixelIndex++];
                            //Channel order for DXT compressed data is BGRA instead of RGBA
                            if (Code == BitmapType.DXT1 || Code == BitmapType.DXT3) {
                                row[x] = new Rgba32(b, g, r, a); 
                            } else {
                                row[x] = new Rgba32(r, g, b, a);
                            }
                            
                        }
                    }
                });
                return image;
            }


            /// <summary>
            /// Decompresses an 8-bit encoded bitmap. Note that for now we don't use color palettes and just assume grayscale. That's probably what SimCity 4 uses them for anyway.
            /// </summary>
            private static byte[] Decompress8Bit(byte[] data) {
                byte[] output = new byte[data.Length * 4];
                for (byte i = 0; i < data.Length; i++) {
                    byte value = data[i];
                    int j = 4 * i;
                    output[j + 2] = output[j + 1] = output[j] = value;
                    output[j + 3] = 0xff;
                }
                return output;
            }

            /// <summary>
            /// Decompresses a 32-bit encoded bitmap - which actually is no decompressing at all. The only thing that changes is the order because alpha goes first here.
            /// </summary>
            private static byte[] Decompress32Bit(byte[] data) {
                byte[] output = new byte[data.Length];
                for (int i = 0; i < data.Length; i += 4) {
                    byte a = data[i];
                    byte r = data[i + 1];
                    byte g = data[i + 2];
                    byte b = data[i + 3];
                    output[i] = r;
                    output[i + 1] = g;
                    output[i + 2] = b;
                    output[i + 3] = a;
                }
                return output;
            }

            /// <summary>
            /// Decompresses a 24-bit encoded bitmap, meaning a bitmap without alpha channel.
            /// </summary>
            private static byte[] Decompress24Bit(byte[] data) {
                int colors = data.Length / 3;
                byte[] output = new byte[4 * colors];
                for (int i = 0; i < colors; i++) {
                    int sourceIndex = 3 * i;
                    int outputIndex = 4 * i;
                    byte r = data[sourceIndex];
                    byte g = data[sourceIndex + 1];
                    byte b = data[sourceIndex + 2];
                    output[outputIndex] = r;
                    output[outputIndex + 1] = g;
                    output[outputIndex + 2] = b;
                    output[outputIndex + 3] = 0xff;
                }
                return output;
            }

            /// <summary>
            /// Decompress a 16-bit bitmap of the A1R5G5B5 format.
            /// </summary>
            private static byte[] Decompress1555(byte[] data) {
                int colors = data.Length / 2;
                byte[] output = new byte[4 * colors];
                for (int i = 0; i < colors; i++) {
                    int sourceIndex = 2 * i;
                    int number = (data[sourceIndex] << 8) | data[sourceIndex + 1];
                    RGBA rgba = Unpack1555(number);
                    int outputIndex = 4 * i;
                    output[outputIndex] = (byte) rgba.R;
                    output[outputIndex + 1] = (byte) rgba.G;
                    output[outputIndex + 2] = (byte) rgba.B;
                    output[outputIndex + 3] = (byte) rgba.A;
                }
                return output;
            }

            /// <summary>
            /// Decompress a 16-bit bitmap of the A0R5G5B5 format.
            /// </summary>
            private static byte[] Decompress0565(byte[] data) {
                int colors = data.Length / 2;
                byte[] output = new byte[4 * colors];
                for (int i = 0; i < colors; i++) {
                    int sourceIndex = 2 * i;
                    int number = (data[sourceIndex] << 8) | data[sourceIndex + 1];
                    RGBA rgba = Unpack565(number);
                    int outputIndex = 4 * i;
                    output[outputIndex] = (byte) rgba.R;
                    output[outputIndex + 1] = (byte) rgba.G;
                    output[outputIndex + 2] = (byte) rgba.B;
                    output[outputIndex + 3] = (byte) rgba.A;
                }
                return output;
            }

            /// <summary>
            /// Decompress a 16-bit bitmap of the A4R4G4B4 format.
            /// </summary>
            private static byte[] Decompress444(byte[] data) {
                int colors = data.Length / 2;
                byte[] output = new byte[4 * colors];
                for (int i = 0; i < colors; i++) {
                    int sourceIndex = 2 * i;
                    int number = (data[sourceIndex] << 8) | data[sourceIndex + 1];
                    RGBA rgba = Unpack4444(number);
                    int outputIndex = 4 * i;
                    output[outputIndex] = (byte) rgba.R;
                    output[outputIndex + 1] = (byte) rgba.G;
                    output[outputIndex + 2] = (byte) rgba.B;
                    output[outputIndex + 3] = (byte) rgba.A;
                }
                return output;
            }

            private struct RGBA(int r, int g, int b, int a = 255) {
                public int R = r;
                public int G = g;
                public int B = b;
                public int A = a;

                /// <inheritdoc/>
                public override readonly string ToString() {
                    return $"RGBA({R}, {G}, {B}, {A})";
                }
            }

            

            private static RGBA Unpack565(int rgb565) {
                int r = ((rgb565 >> 11) & 0b11111) * (255 / 31);
                int g = ((rgb565 >> 5) & 0b111111) * (255 / 63);
                int b = (rgb565 & 0b11111) * (255 / 31);
                return new RGBA(r, g, b, 0);
            }

            private static RGBA Unpack1555(int rgb1555) {
                int a = ((rgb1555 >> 15) & 0b1) * 255;
                int r = ((rgb1555 >> 10) & 0b11111) * (255 / 31);
                int g = ((rgb1555 >> 5) & 0b11111) * (255 / 31);
                int b = (rgb1555 & 0b11111) * (255 / 31);
                return new RGBA(r, g, b, a);
            }

            private static RGBA Unpack4444(int rgb444) {
                int a = ((rgb444 >> 12) & 0b1111) * (255 / 15);
                int r = ((rgb444 >> 8) & 0b1111) * (255 / 15);
                int g = ((rgb444 >> 4) & 0b1111) * (255 / 15);
                int b = (rgb444 & 0b1111) * (255 / 15);
                return new RGBA(r, g, b, a);
            }

            
            /// <summary>
            /// Decompresses an image compressed in the DXT1 format to a bitmap (a byte[] of rgba values).
            /// </summary>
            private static byte[] DecompressDXT1(byte[] input, int width, int height) {
                byte[] output = new byte[width * height * 4];
                int offset = 0;
                int bcw = (width + 3) / 4;
                int bch = (height + 3) / 4;
                int clen_last = (width + 3) % 4 + 1;
                uint[] buffer = new uint[16];
                int[] colors = new int[4];
                for (int t = 0; t < bch; t++) {
                    for (int s = 0; s < bcw; s++, offset += 8) {
                        int q0 = input[offset + 0] | input[offset + 1] << 8;
                        int q1 = input[offset + 2] | input[offset + 3] << 8;
                        RGBA c0 = IntToRgb565(q0);
                        RGBA c1 = IntToRgb565(q1);
                        colors[0] = GetColorInt(c0.R, c0.G, c0.B, 255);
                        colors[1] = GetColorInt(c1.R, c1.G, c1.B, 255);
                        if (q0 > q1) {
                            colors[2] = GetColorInt((c0.R * 2 + c1.R) / 3, (c0.G * 2 + c1.G) / 3, (c0.B * 2 + c1.B) / 3, 255);
                            colors[3] = GetColorInt((c0.R + c1.R * 2) / 3, (c0.G + c1.G * 2) / 3, (c0.B + c1.B * 2) / 3, 255);
                        } else {
                            colors[2] = GetColorInt((c0.R + c1.R) / 2, (c0.G + c1.G) / 2, (c0.B + c1.B) / 2, 255);
                        }

                        uint d = BitConverter.ToUInt32(input, offset + 4);
                        for (int i = 0; i < 16; i++, d >>= 2) {
                            buffer[i] = unchecked((uint) colors[d & 3]);
                        }

                        int clen = (s < bcw - 1 ? 4 : clen_last) * 4;
                        for (int i = 0, y = t * 4; i < 4 && y < height; i++, y++) {
                            Buffer.BlockCopy(buffer, i * 4 * 4, output, (y * width + s * 4) * 4, clen);
                        }
                    }
                }
                return output;
            }


            /// <summary>
            /// Decompresses an image compressed in the DXT3 format to a bitmap (a byte[] of rgba values).
            /// </summary>
            private static byte[] DecompressDXT3(byte[] dxtData, int width, int height) {
                byte[] rgbaData = new byte[width * height * 4];
                int offset = 0;
                int bcw = (width + 3) / 4;
                int bch = (height + 3) / 4;
                int clen_last = (width + 3) % 4 + 1;
                uint[] buffer = new uint[16];
                int[] colors = new int[4];
                int[] alphas = new int[16];
                for (int t = 0; t < bch; t++) {
                    for (int s = 0; s < bcw; s++, offset += 16) {
                        for (int i = 0; i < 4; i++) {
                            int alpha = dxtData[offset + i * 2] | dxtData[offset + i * 2 + 1] << 8;
                            alphas[i * 4 + 0] = (((alpha >> 0) & 0xF) * 0x11) << 24;
                            alphas[i * 4 + 1] = (((alpha >> 4) & 0xF) * 0x11) << 24;
                            alphas[i * 4 + 2] = (((alpha >> 8) & 0xF) * 0x11) << 24;
                            alphas[i * 4 + 3] = (((alpha >> 12) & 0xF) * 0x11) << 24;
                        }

                        int color0 = dxtData[offset + 8] | dxtData[offset + 9] << 8;
                        int color1 = dxtData[offset + 10] | dxtData[offset + 11] << 8;
                        RGBA c0 = IntToRgb565(color0);
                        RGBA c1 = IntToRgb565(color1);
                        colors[0] = GetColorInt(c0.R, c0.G, c0.B, 0);
                        colors[1] = GetColorInt(c1.R, c1.G, c1.B, 0);
                        if (color0 > color1) {
                            colors[2] = GetColorInt((c0.R * 2 + c1.R) / 3, (c0.G * 2 + c1.G) / 3, (c0.B * 2 + c1.B) / 3, 0);
                            colors[3] = GetColorInt((c0.R + c1.R * 2) / 3, (c0.G + c1.G * 2) / 3, (c0.B + c1.B * 2) / 3, 0);
                        } else {
                            colors[2] = GetColorInt((c0.R + c1.R) / 2, (c0.G + c1.G) / 2, (c0.B + c1.B) / 2, 0);
                        }

                        uint d = BitConverter.ToUInt32(dxtData, offset + 12);
                        for (int i = 0; i < 16; i++, d >>= 2) {
                            buffer[i] = unchecked((uint) (colors[d & 3] | alphas[i]));
                        }

                        int clen = (s < bcw - 1 ? 4 : clen_last) * 4;
                        for (int i = 0, y = t * 4; i < 4 && y < height; i++, y++) {
                            Buffer.BlockCopy(buffer, i * 4 * 4, rgbaData, (y * width + s * 4) * 4, clen);
                        }
                    }
                }
                return rgbaData;
            }

            private static RGBA IntToRgb565(int c) {
                int r = (c & 0xf800) >> 8;
                int g = (c & 0x07e0) >> 3;
                int b = (c & 0x001f) << 3;
                r |= r >> 5;
                g |= g >> 6;
                b |= b >> 5;

                return new RGBA(r, g, b, 0);
            }

            private static int GetColorInt(int r, int g, int b, int a) {
                return r << 16 | g << 8 | b | a << 24;
            }
        }
    }
}