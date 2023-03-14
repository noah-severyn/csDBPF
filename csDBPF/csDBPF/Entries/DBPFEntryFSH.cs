using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using static System.Net.WebRequestMethods;

namespace csDBPF.Entries {
    /// <summary>
	/// An implementation of <see cref="DBPFEntry"/> for FSH entries. Object data is stored in <see cref="DBPFEntry.ByteData"/> and is interpreted from the <see cref="FSHHeader.FSHDirectory"/> and <see cref="BitmapHeaders"/>.
	/// </summary>
	/// <see ref="https://www.wiki.sc4devotion.com/index.php?title=FSH"/>
    /// <seealso ref="https://www.wiki.sc4devotion.com/index.php?title=FSH_Format"/>
    public class DBPFEntryFSH : DBPFEntry {
        private readonly FSHHeader _header;
        private readonly List<FSHBitmapHeader> _bitmapHeaders;
        private bool _isCompressed;
        private bool _isDecoded;

        /// <summary>
        /// Stores key information about this FSH entry.
        /// </summary>
        public FSHHeader Header {
            get { return _header; }
        }

        /// <summary>
        /// Stores key information about each bitmap in this file.
        /// </summary>
        public List<FSHBitmapHeader> BitmapHeaders {
            get { return _bitmapHeaders; }
        }



        /// <summary>
        /// Stores key information about this FSH entry. The header is the first 16 bytes of the entry, plus the size of the directory.
        /// </summary>
        public class FSHHeader {
            private string _identifier;
            private List<FSHDirectoryItem> _fshdirectory;
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
            /// Describes the usage of these bitmaps in game.
            /// </summary>
            public FSHDirectoryID DirectoryID { get; internal set; }

            /// <summary>
            /// Describes the structure of this entry as a list of name-offset pairs of length <see cref="BitmapCount"/>.
            /// </summary>
            public List<FSHDirectoryItem> FSHDirectory {
                get { return _fshdirectory; }
                internal set { _fshdirectory = value; }
            }

            /// <summary>
            /// Create a new instance.
            /// </summary>
            internal FSHHeader() {
                _fshdirectory = new List<FSHDirectoryItem>();
            }
        }



        /// <summary>
        /// Create a new instance. Use when creating a new FSH entry.
        /// </summary>
        /// <param name="tgi"></param>
        public DBPFEntryFSH(DBPFTGI tgi) : base(tgi) {
            if (tgi is null) {
                TGI.SetTGI(DBPFTGI.FSH);
            }
            _header = new FSHHeader();
            _isCompressed = DBPFCompression.IsCompressed(ByteData);
        }

        /// <summary>
		/// Create a new instance. Use when reading an existing FSH entry from a file.
		/// </summary>
		/// <param name="tgi"><see cref="DBPFTGI"/> object representing the entry</param>
		/// <param name="offset">Offset (location) of the entry within the DBPF file</param>
		/// <param name="size">Compressed size of data for the entry, in bytes. Uncompressed size is also temporarily set to this to this until the data is set</param>
		/// <param name="index">Entry position in the file, 0-n</param>
		/// <param name="bytes">Byte data for this entry</param>
        public DBPFEntryFSH(DBPFTGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {
            _header = new FSHHeader();
            _isCompressed = DBPFCompression.IsCompressed(ByteData);
        }



        /// <summary>
        /// Decompresses this entry and sets <see cref="FSHHeader.FSHDirectory"/> and <see cref="BitmapHeaders"/> from byte data. These provide a template for how to process the <see cref="DBPFEntry.ByteData"/>
        /// </summary>
        public override void DecodeEntry() {
            if (_isDecoded) return;

            byte[] dData;
            if (_isCompressed) {
                dData = DBPFCompression.Decompress(ByteData);
                _isCompressed = false;
            } else {
                dData = ByteData;
            }

            //Analyze the FSH file header
            _header.Identifier = ByteArrayHelper.ToAString(dData, 0, 4);
            _header.FileSize = BitConverter.ToInt32(dData, 4);
            _header.BitmapCount = BitConverter.ToInt32(dData, 8);
            _header.DirectoryID = (FSHDirectoryID) BitConverter.ToInt32(dData, 12);

            //Fill the FSH Directory. This tells us the location in the file of each bitmap.
            int offset = 16;
            byte[] entryName = new byte[4];
            for (int entry = 0; entry < _header.BitmapCount; entry++) {
                Array.Copy(dData, offset, entryName, 0, 4);
                offset += 4;
                int entryOffset = BitConverter.ToInt32(dData, offset);
                offset += 4;
                _header.FSHDirectory.Add(new FSHDirectoryItem(entryName, entryOffset));
            }

            //After the directory is built, look at the header information each bitmap in the file
            for (int idx = 0; idx < _header.BitmapCount; idx++) {
                offset = _header.FSHDirectory[idx].Offset;
                int code = BitConverter.ToInt32(dData, offset);
                short width = BitConverter.ToInt16(dData, offset + 4);
                short height = BitConverter.ToInt16(dData, offset + 6);
                short[] misc = new short[4];
                Array.Copy(dData, offset + 8, misc, 0, 4);
                _bitmapHeaders.Add(new FSHBitmapHeader(code, width, height, misc));
            }
            _isDecoded = true;
        }


        /// <summary>
		/// Build <see cref="DBPFEntry.ByteData"/> from the current state of this instance.
		/// </summary>
        public override void ToBytes() {
            throw new NotImplementedException();
        }



        /// <summary>
        /// Stores the name and offset of each bitmap item in this subfile.
        /// </summary>
        /// <see ref="https://www.wiki.sc4devotion.com/index.php?title=FSH_Format#FSH_Directory"/>
        public readonly struct FSHDirectoryItem {
            private readonly byte[] _name;
            private readonly int _offset;

            /// <summary>
            /// Item name.
            /// </summary>
            public byte[] Name { get { return _name; } }
            /// <summary>
            /// Byte offset of this item, from the start of this DBPFEntryFSH
            /// </summary>
            public int Offset { get { return _offset; } }

            /// <summary>
            /// Instantiate a new FSHDirectoryItem.
            /// </summary>
            /// <param name="name">4 byte name of this item</param>
            /// <param name="offset">Byte offset of this item, from the start of this DBPFEntryFSH</param>
            public FSHDirectoryItem(byte[] name, int offset) {
                if (name.Length != 4) {
                    throw new ArgumentException("Name must be 4 bytes in length.");
                }
                _name = name;
                _offset = offset;
            }
        }

        /// <summary>
        /// Stores key information about each bitmap in this file.
        /// </summary>
        /// <see ref="https://www.wiki.sc4devotion.com/index.php?title=FSH_Format#FSH_Entry_Header"/>
        public readonly struct FSHBitmapHeader {
            private readonly int _blocksize;
            private readonly short _width;
            private readonly short _height;
            private readonly short[] _misc;

            /// <summary>
            /// Size of this block including this header. 
            /// </summary>
            /// <remarks>This is actually a combination of 2 values: least significant byte is the Record ID (logically ANDed by 0x7f for the <see cref="FSHBitmapType"/>), and the remaining 3 bytes are the size of this block including the header. The block size is only used if the file contains an attachment or embedded mipmaps. It is zero otherwise. For single images this is usually: width x height + 0x10. For images with embedded mipmaps, this is the total size of the original image, plus all mipmaps, plus the header. In either case, it may include additional data as a binary attachment with unknown format.</remarks>
            public int BlockSize { get { return _blocksize; } }
            /// <summary>
            /// Pixel width of this bitmap.
            /// </summary>
            public short Width { get { return _width; } }
            /// <summary>
            /// Pixel height of this bitmap.
            /// </summary>
            public short Height { get { return _height; } }
            /// <summary>
            /// Holds center coordinates and axis position offsets for the bitmap. These are not used in SC4.
            /// </summary>
            public short[] Misc { get { return _misc; } }
            /// <summary>
            /// Create a new instance.
            /// </summary>
            /// <param name="blockSize">Size of this block including this header (see <see cref="FSHBitmapHeader.BlockSize"/>)</param>
            /// <param name="width">Pixel width of this bitmap</param>
            /// <param name="height">Pixel height of this bitmap</param>
            /// <param name="misc">Holds center coordinates and axis position offsets for the bitmap. These are not used in SC4</param>
            internal FSHBitmapHeader(int blockSize, short width, short height, short[] misc) {
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
        /// <see ref="https://www.wiki.sc4devotion.com/index.php?title=FSH_Format#Bitmap_or_Palette_Data"/>
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

        /// <summary>
        /// Specifies the color palette for <see cref="FSHBitmapType.EightBit"/> type bitmaps.
        /// </summary>
        /// <see ref="https://www.wiki.sc4devotion.com/index.php?title=FSH_Format#Palette_codes"/>
        public enum FSHPaletteCode {
            /// <summary>
            /// 24-bit DOS
            /// </summary>
            TwentyFourBitDOS = 0x22,
            /// <summary>
            /// 24-bit
            /// </summary>
            TwentyFourBit = 0x24,
            /// <summary>
            /// 16-bit NFS5
            /// </summary>
            SixteenBitNFSS = 0x29,
            /// <summary>
            /// 32-bit 
            /// </summary>
            ThirtyTwoBit = 0x2A,
            /// <summary>
            /// 16-bit
            /// </summary>
            SixteenBit = 0x2D
        }


        public enum FSHTextCode {
            /// <summary>
            /// Standard Text file
            /// </summary>
            StandardText = 0x6F,
            /// <summary>
            /// ETXT of arbitrary length with full entry header
            /// </summary>
            ArbitraryLength = 0x69,
            /// <summary>
            /// ETXT of 16 bytes or less including the header 
            /// </summary>
            LessThan16Bytes = 0x70,
            /// <summary>
            /// Defined Pixel region Hotspot data for image.
            /// </summary>
            PixelRegionHotspot = 0x7C
        }
    }
}