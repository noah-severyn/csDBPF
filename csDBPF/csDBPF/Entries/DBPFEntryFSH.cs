using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;

namespace csDBPF.Entries {
    /// <summary>
	/// An implementation of <see cref="DBPFEntry"/> for FSH entries. Object data is stored in <see cref="Text"/>.
	/// </summary>
	/// <see ref="https://www.wiki.sc4devotion.com/index.php?title=FSH"/>
    /// <seealso ref="https://www.wiki.sc4devotion.com/index.php?title=FSH_Format"/>
    internal class DBPFEntryFSH : DBPFEntry {

        private List<BitmapItem> _bitmapItems;
        private FSHHeader _header;
        private bool _isCompressed;
        private bool _isDirty;
        private bool _saveGlobalPalette;
        private bool _isDecoded;

        public DBPFEntryFSH(DBPFTGI tgi) : base(tgi) {
            if (tgi is null) {
                TGI.SetTGI(DBPFTGI.FSH);
            }

            _bitmapItems = new List<BitmapItem>();
            _header = new FSHHeader();
        }

        public DBPFEntryFSH(DBPFTGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {
            _bitmapItems = new List<BitmapItem>();
            _header = new FSHHeader();
        }

        /// <summary>
        /// Stores key information about this FSH entry. The header is the first 16 bytes of the entry, plus the size of the directory.
        /// </summary>
        public class FSHHeader {
            private string _identifier;
            private List<FSHDirEntry> _fshdirectory;
            /// <summary>
            /// FSH File identifier. Should always be "SHPI".
            /// </summary>
            public string Identifier {
                get { return _identifier; }
                internal set {
                    string identifierFSH = "SHPI";
                    if (value.CompareTo(identifierFSH) != 0) {
                        throw new InvalidDataException("Invalid FSH header format.");
                    } else {
                        _identifier = value;
                    }
                }
            }

            /// <summary>
            /// File size in bytes of this entry.
            /// </summary>
            /// <remarks>This is equivalent to the QFS compressed size of this entry.</remarks>
            public int FileSize { get; internal set; }

            /// <summary>
            /// Number of bitmaps in this entry.
            /// </summary>
            public int BitmapCount { get; internal set; }

            /// <summary>
            /// Stores the usage of this entry.
            /// </summary>
            public FSHDirectoryID DirectoryID { get; internal set; }

            /// <summary>
            /// Defines the structure of this entry as a list of name-offset pairs.
            /// </summary>
            public List<FSHDirEntry> FSHDirectory {
                get { return _fshdirectory; }
                internal set { _fshdirectory = value; }
            }

            /// <summary>
            /// Instantiate a new FSHHeader.
            /// </summary>
            internal FSHHeader() {
                _fshdirectory = new List<FSHDirEntry>();
            }
        }


        /// <summary>
        /// Stores the name and offset of each bitmap entry in this file.
        /// </summary>
        public readonly struct FSHDirEntry {
            private readonly byte[] _name;
            private readonly int _offset;
            public byte[] Name { get { return _name; } }
            public int Offset { get { return _offset; } }

            public FSHDirEntry(byte[] name, int offset) {
                _name = name;
                _offset = offset;
            }
        }



        public class BitmapItem {
            public byte[] rawData { get; set; }

            /// <summary>
            /// Color (base) bitmap.
            /// </summary>
            public Image Color { get; set; }
            /// <summary>
            /// Alpha (transparency) bitmap.
            /// </summary>
            public Image Alpha { get; set; }
            /// <summary>
            /// Defines the bitmap type of this item.
            /// </summary>
            public FSHBitmapType BitmapType { get; set; }
            public string[] Comments { get; set; } //TODO - what's this for???
            public bool IsCompressed { get; set; } //TODO - is this QFS or DXT compression reference?
            public Color[] Palette { get; set; }
        }

        public override void DecodeEntry() {
            if (_isDecoded) return;

            byte[] dData;
            if (DBPFCompression.IsCompressed(ByteData)) {
                dData = DBPFCompression.Decompress(ByteData);
            } else {
                dData = ByteData;
            }

            //Analyze the FSH file header
            _header.Identifier = ByteArrayHelper.ToAString(dData, 0, 4);
            _header.FileSize = BitConverter.ToInt32(dData, 4);
            _header.BitmapCount = BitConverter.ToInt32(dData, 8);
            _header.DirectoryID = (FSHDirectoryID) BitConverter.ToInt32(dData, 12);

            //Fill the FSH Directory
            int offset = 16;
            byte[] entryName = new byte[4];
            for (int entry = 0; entry < _header.BitmapCount; entry++) {
                Array.Copy(dData, offset, entryName, 0, 4);
                offset += 4;
                int entryOffset = BitConverter.ToInt32(dData, offset);
                offset += 4;
                _header.FSHDirectory.Add(new FSHDirEntry(entryName, entryOffset));
            }

            //After the Directory is built, look at each specific FSH entry in the file
            //offset += 8; //Skip "Buy ERTS" ... not sure what this is but it appears in every FSH file. Doesn't always appear though (not present with lot textures)

            ////pallets generally are 256 length arrays of 1 byte
            ////bitmaps store pixel data in many ways
            ////FSH images can store raw data or Microsoft DXTC compressed
            for (int idx = 0; idx < _header.BitmapCount; idx++) {
                //Before the palette or bitmap data is an entry header
                
                offset = _header.FSHDirectory[idx].Offset;
                int code = BitConverter.ToInt32(dData, offset);
                short width = BitConverter.ToInt16(dData, offset + 4);
                short height = BitConverter.ToInt16(dData, offset + 6);
                short[] misc = new short[4];
                Array.Copy(dData, offset + 8, misc, 0, 4);
                FSHEntryHeader fshentryheader = new FSHEntryHeader(code, width, height, misc);

                FSHBitmapType bitmapcode = (FSHBitmapType) (fshentryheader.BlockSize & 0x7F);

                //bool isvalidcode = Enum.IsDefined<FSHBitmapType>(bitmapcode);
                //if (!isvalidcode) return;
                
                //AColor[] colorArray = new AColor[0];
                //int[,] numArray1 = new int[fshentryheader.Height, fshentryheader.Width];
                //int[,] numArray2 = new int[fshentryheader.Height, fshentryheader.Width];
                //switch (bitmapcode) {
                //    case FSHBitmapType.DXT1: //0x60 = 96
                //        //var format = new SixLabors.ImageSharp.PixelFormats.Argb32;
                //        for (int row = 0; row < numArray2.GetLength(0); row++) {
                //            for (int col = 0; col < numArray2.GetLength(1); col++) {
                //                numArray2[row, col] = -1;
                //            }
                //        }
                //        byte[] target = new byte[12 * fshentryheader.Width / 4];
                //        for (int row = fshentryheader.Height/4 -1; row >=0; row--) {
                //            for (int col = 7; col >= 4; col--) {

                //            }
                //        }

                //        break;
                //    case FSHBitmapType.DXT3: //0x61 = 97
                //        format = 0;
                //        break;
                //    case FSHBitmapType.SixteenBit4x4: //0x6D = 109
                //        format = 0;
                //        break;
                //    case FSHBitmapType.SixteenBit: //0x78 = 120
                //        format = 0;
                //        break;
                //    case FSHBitmapType.EightBit: //0x7B = 123
                //        format = 0;
                //        break;
                //    case FSHBitmapType.ThirtyTwoBit: //0x7D = 125
                //        format = 0;
                //        break;
                //    case FSHBitmapType.SixteenBitAlpha: //0x7E = 126
                //        format = 0;
                //        break;
                //    case FSHBitmapType.TwentyFourBit: //0x7F = 127
                //        format = 0;
                //        break;
                //    default:
                //        isvalidcode = false;
                //        break;
                //}
            }
        }


        public override void ToBytes() {
            throw new NotImplementedException();
        }

        

        private readonly struct FSHEntryHeader {
            private readonly int _blocksize;
            private readonly short _width;
            private readonly short _height;
            private readonly short[] _misc;

            public int BlockSize { get { return _blocksize; } }
            public short Width { get { return _width; } }
            public short Height { get { return _height; } }
            /// <summary>
            /// Holds center coordinates and axis position offsets for the bitmap. These are not used row SC4.
            /// </summary>
            public short[] Misc { get { return _misc; } }

            public FSHEntryHeader(int blockSize, short width, short height, short[] misc) {
                _blocksize = blockSize;
                _width = width;
                _height = height;
                _misc = misc;
            }
        }

        /// <summary>
		/// Valid options for the DirectoryID value in the FSHHeader.
		/// </summary>
		public enum FSHDirectoryID {
            /// <summary>
            /// G354 - Building Textures
            /// </summary>
            Building = 0x47334444,
            /// <summary>
            /// G264 - Network Textures, Sim Textures, Sim heads, Sim animations, Trees, props, Lot textures, Misc colors
            /// </summary>
            EverythingElse = 0x47323634,
            /// <summary>
            /// G266 - 3d Animation textures (e.g.the green rotating diamond in loteditor.dat)
            /// </summary>
            Animation = 0x47323636,
            /// <summary>
            /// //G290 - Dispatch marker textures
            /// </summary>
            DispatchMarker = 0x47323930,
            /// <summary>
            /// G315 - Small Sim texture, Network Transport Model Textures(trains, etc.)
            /// </summary>
            NetworkTransportModel = 0x71333135,
            /// <summary>
            /// GIMX - UI Editor textures
            /// </summary>
            UIEditor = 0x47494D58,
            /// <summary>
            /// G344 - BAT gen texture maps
            /// </summary>
            BATgen = 0x47333434
        }

        /// <summary>
        /// Defines the bitmap type stored in this file.
        /// </summary>
        public enum FSHBitmapType {
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
            SixteenBitAlpha = 0x7E,
            /// <summary>
            /// Type: 16-bit A0 R5 G6 B5<br/>Palette: none<br/>Compression: none
            /// </summary>
            SixteenBit = 0x78,
            /// <summary>
            /// Type: 16-bit A4 R4 G4 B4<br/>Palette: none<br/>Compression: none
            /// </summary>
            SixteenBit4x4 = 0x6D,
            /// <summary>
            /// Type: DXT3 4x4 packed, 4-bit alpha<br/>Palette: none<br/>Compression: 4x4 grid compressed, half-byte per pixel
            /// </summary>
            DXT3 = 0x61,
            /// <summary>
            /// Type: DXT1 4x4 packed, 1-bit alpha<br/>Palette: none<br/>Compression: 4x4 grid compressed, half-byte per pixel
            /// </summary>
            DXT1 = 0x60
        }

        public enum FSHPaletteCode {
            TwentyFourBitDOS = 0x22, //24-bit DOS
            TwentyFourBit = 0x24, //24-bit 
            SixteenBitNFSS = 0x29, //16-bit NFS5 
            ThirtyTwoBit = 0x2A, //32-bit 
            SixteenBit = 0x2D //16-bit
        }

        public enum FSHTextCode {
            StandardText = 0x6F, //Standard Text file
            ArbitraryLength = 0x69, //ETXT of arbitrary length with full entry header 
            LessThan16Bytes = 0x70, //ETXT of 16 bytes or less including the header 
            PixelRegionHotspot = 0x7C //defined Pixel region Hotspot data for image.
        }








        //TODO - move this to an external repository: csFSH (or something)
        //TODO - refactor BitmapItem to remove the Image dependency. Figure out a way to separate the two




        //Explanation: https://www.fsdeveloper.com/wiki/index.php?title=DXT_compression_explained
        //Wikipedia  : https://en.wikipedia.org/wiki/S3_Texture_Compression



        //! https://github.com/nominom/bcnencoder.net
        //? https://github.com/BraveSirAndrew/ManagedSquish/blob/master/ManagedSquish/Squish.cs
        //? https://github.com/castano/nvidia-texture-tools/tree/master/src/nvimage
        //(java script) https://github.com/kchapelier/decode-dxt


        /// <summary>
        /// Represents a 4x4 pixel block of a bitmap.
        /// </summary>
        class Block {
            /// <summary>
            /// Stores 16x <see cref="Argb32"/> pixels making up a 4x4 block, numbered left to right, top to bottom. 
            /// </summary>
            public Argb32[] Pixels = new Argb32[16];
        }


        public static Argb32[] DXTDecompress(int width, int height, byte[] blob, FSHBitmapType bitmapType) {
            //partially based off of: https://github.com/Easimer/cs-dxt1/blob/master/DXT1Decompressor.cs
            //DXT1 aka Block Compression 1 aka BC1: Compressed block is 64bits. 16 bit for each color0 and color1. 32bits left: 16x 2-bit identifiers of each color
            //DXT2 aka Block Compression 2 aka BC2: Compressed block is 128bit. Contains 2x DXT1 blocks. First is alpha channel data, second is color data.

            int bytes = width * 3 * 4 * height;

            Argb32[] decompressed = new Argb32[bytes];
            byte[] byteBlock = new byte[8];

            //row and col represent the position of a 4x4 block in the greater image
            for (int row = 0; row < height / 4; row++) {
                for (int col = 0; col < width / 4; col++) {
                    Array.Copy(blob, row * 4 + col, byteBlock, 0, 8);
                    Block block = DecompressBlock(byteBlock, bitmapType);

                    Array.Copy(decompressed, row * 4 + col, block.Pixels, 0, 16);

                    //int blockoffset = row * 4 * width * 3 + col * 4 * 3;
                    //for (int py = 0; py < 4; py++) {
                    //    for (int px = 0; px < 4; px++) {
                    //        int pxoffset = blockoffset + px * 3;
                    //        decompressed[pxoffset + 0] = block.Pixels[py * 4 + px].R;
                    //        decompressed[pxoffset + 1] = block.Pixels[py * 4 + px].G;
                    //        decompressed[pxoffset + 2] = block.Pixels[py * 4 + px].B;
                    //    }
                    //}

                    //Image<Argb32> output2 = Image.LoadPixelData<Argb32>(pixels, width, height);
                }
            }
            return decompressed;
        }


        /// <summary>
        /// Decompress a 64 bit block of data into a block of pixels.
        /// </summary>
        /// <param name="blob">Raw data to process</param>
        /// <param name="bitmapType">Compression type of the blob: DXT1 or DXT3</param>
        /// <returns>A decompressed <see cref="Block"/> of pixels</returns>
        /// <exception cref="ArgumentException">If block is incorrect length</exception>
        /// /// <exception cref="ArgumentException">Bitmap type is not DXT1 or DXT3</exception>
        private static Block DecompressBlock(byte[] blob, FSHBitmapType bitmapType) {
            if (blob.Length != 8) {
                throw new ArgumentException("Block must be 64 bits (8 bytes) in length.");
            }
            if (bitmapType > FSHBitmapType.DXT3) {
                throw new ArgumentException("Only valid for DXT1 and DXT3 bitmap types.");
            }

            //Read color0 and color1 straight from the block. Colors stored in R5 G6 B5 format.
            ushort c0 = (ushort) (blob[1] << 8 | blob[0]);
            ushort c1 = (ushort) (blob[3] << 8 | blob[2]);
            byte c0_r = (byte) ((c0 & 0b1111100000000000) >> 11);
            byte c0_g = (byte) ((c0 & 0b0000011111100000) >> 5);
            byte c0_b = ((byte) (c1 & 0b0000000000011111));
            byte c1_r = (byte) ((c1 & 0b1111100000000000) >> 11);
            byte c1_g = (byte) ((c1 & 0b0000011111100000) >> 5);
            byte c1_b = ((byte) (c1 & 0b0000000000011111));

            //Interpolate color2 and color3. DXT3 always uses the 4 color pattern.
            byte c2_r, c2_g, c2_b;
            byte c3_r, c3_g, c3_b;
            if ((c0 >= c1) || (bitmapType == FSHBitmapType.DXT3)) {
                c2_r = (byte) ((2.0f * c0_r + c1_r) / 3.0f);
                c2_g = (byte) ((2.0f * c0_g + c1_g) / 3.0f);
                c2_b = (byte) ((2.0f * c0_b + c1_b) / 3.0f);
                c3_r = (byte) ((c0_r + 2.0f * c1_r) / 3.0f);
                c3_g = (byte) ((c0_g + 2.0f * c1_g) / 3.0f);
                c3_b = (byte) ((c0_b + 2.0f * c1_b) / 3.0f);
            } else {
                c2_r = (byte) ((c0_r + c1_r) / 2.0f);
                c2_g = (byte) ((c0_g + c1_g) / 2.0f);
                c2_b = (byte) ((c0_b + c1_b) / 2.0f);
                c3_r = c3_g = c3_b = 0; //c3 is black
            }

            Argb32[] colors = new Argb32[] {
                new Argb32(c0_r, c0_g, c0_b),
                new Argb32(c1_r, c1_g, c1_b),
                new Argb32(c2_r, c2_g, c2_b),
                new Argb32(c3_r, c3_g, c3_b)
            };

            //Build a pixel array based on the color codes in the remaining 32 bits.
            int offset = 32;
            Block result = new Block();
            for (int by = 0; by < 4; by++) {
                for (int bx = 0; bx < 4; bx++) {
                    byte code = (byte) ((blob[offset + by] << (bx * 2)) & 3);
                    result.Pixels[by * 4 + bx] = colors[code];
                }
            }
            return result;

        }



        /// <summary>
        /// Blends the color/base bitmap with the alpha bitmap to create a transparent bitmap.
        /// </summary>
        /// <returns></returns>
        ///<remarks>Originally decompiled from BlendBmp.dll from <see ref="https://community.simtropolis.com/files/file/35279-fsh-converter-tool/">FSH Converter Tool</see>. Updated for cross platform interoperability.</remarks>
        public Image Blend() {
            Image blendedBmp = null;
            if (Color != null && Alpha != null) {
                blendedBmp = new Bitmap(AColor.Width, AColor.Height, PixelFormat.Format32bppArgb);
            }

            Bitmap alphaBmp = null;
            Bitmap colorBmp = null;
            if (Color != null && Alpha != null) {
                colorBmp = new Bitmap(Color);
                alphaBmp = new Bitmap(Alpha);
            }

            BitmapData colorBmpData = colorBmp.LockBits(new Rectangle(0, 0, blendedBmp.Width, blendedBmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData alphaBmpData = alphaBmp.LockBits(new Rectangle(0, 0, blendedBmp.Width, blendedBmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData blendedBmpData = blendedBmp.LockBits(new Rectangle(0, 0, blendedBmp.Width, blendedBmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            IntPtr scan0_1 = blendedBmpData.Scan0;
            byte* scan0_2 = (byte*) (void*) colorBmpData.Scan0;
            byte* scan0_3 = (byte*) (void*) alphaBmpData.Scan0;
            byte* numPtr = (byte*) (void*) scan0_1;
            int numBlended = blendedBmpData.Stride - blendedBmp.Width * 4;
            int numColor = colorBmpData.Stride - blendedBmp.Width * 4;
            int numAlpha = alphaBmpData.Stride - blendedBmp.Width * 4;
            for (int index1 = 0; index1 < blendedBmp.Height; ++index1) {
                for (int index2 = 0; index2 < blendedBmp.Width; ++index2) {
                    numPtr[3] = *scan0_3;
                    *numPtr = *scan0_2;
                    numPtr[1] = scan0_2[1];
                    numPtr[2] = scan0_2[2];
                    numPtr += 4;
                    scan0_2 += 4;
                    scan0_3 += 4;
                }
                numPtr += numBlended;
                scan0_2 += numColor;
                scan0_3 += numAlpha;
            }
            colorBmp.UnlockBits(colorBmpData);
            alphaBmp.UnlockBits(alphaBmpData);
            blendedBmp.UnlockBits(blendedBmpData);
            return blendedBmp;
        }
    }
}
