using System;
using System.Collections.Generic;

namespace csDBPF {
    /// <summary>
    /// An implementation of <see cref="DBPFEntry"/> for Directory entries. Object data is stored in <see cref="CompressedItems"/>.
    /// </summary>
    /// <see href="https://wiki.sc4devotion.com/index.php?title=DBDF"/>
    public class DBPFEntryDIR : DBPFEntry {

        private List<DBDFItem> _compressedItems;
        /// <summary>
        /// List of <see cref="DBDFItem"/> representing the TGI set and the decompressed byte size of each subfile in this file.
        /// </summary>
        public List<DBDFItem> CompressedItems {
            get { return _compressedItems; }
            private set { _compressedItems = value; }
        }


        /// <summary>
        /// Struct which represents one item in the DBDF directory list. Each item is a reference to a compressed entry in the DBPF file.
        /// </summary>
        public readonly struct DBDFItem {
            /// <summary>
            /// Item Type ID.
            /// </summary>
            public uint TID { get; }
            /// <summary>
            /// Item Group ID.
            /// </summary>
            public uint GID { get; }
            /// <summary>
            /// Item Instance ID.
            /// </summary>
            public uint IID { get; }
            /// <summary>
            /// Item size in bytes.
            /// </summary>
            public uint Size { get; }

            internal DBDFItem(uint tid, uint gid, uint iid, uint size) {
                TID = tid;
                GID = gid;
                IID = iid;
                Size = size;
            }
        }



        /// <summary>
        /// Create a new instance. Use when creating a new Directory.
        /// </summary>
        public DBPFEntryDIR() : base(DBPFTGI.DIRECTORY) {
            _compressedItems = [];
            IsCompressed = false; //DIR files are never compressed
        }

        /// <summary>
        /// Create a new instance. Use when reading an existing directory from a file.
        /// </summary>
        /// <param name="offset">Offset (location) of the entry within the DBPF file</param>
        /// <param name="size">Compressed size of data for the entry, in bytes. Uncompressed size is also temporarily set to this to this until the data is set</param>
        /// <param name="index">Entry position in the file, 0-n</param>
        /// <param name="bytes">Byte data for this entry</param>
        /// <remarks>Directory subfiles are special in that their TGI is *always* the same, so providing TGI as an argument is unnecessary.</remarks>
        public DBPFEntryDIR(uint offset, uint size, uint index, byte[] bytes) : base(DBPFTGI.DIRECTORY, offset, size, index, bytes) {
            _compressedItems = [];
        }



        /// <summary>
        /// Sets the directory entry from raw data and sets the <see cref="CompressedItems"/> property of this instance.
        /// </summary>
        public override void Decode() {
            if (IsDecoded) {
                return;
            }

            for (int pos = 0; pos < ByteData.Length; pos += 16) {
                _compressedItems.Add(new DBDFItem(BitConverter.ToUInt32(ByteData, pos), BitConverter.ToUInt32(ByteData, pos + 4), BitConverter.ToUInt32(ByteData, pos + 8), BitConverter.ToUInt32(ByteData, pos + 12)));
            }
            IsDecoded = true;
        }



        /// <summary>
        /// Builds the <see cref="CompressedItems"/> list with all compressed entries.
        /// </summary>
        /// <param name="entries">List of entries</param>
        internal void Build(List<DBPFEntry> entries) {
            _compressedItems.Clear();

            foreach (DBPFEntry entry in entries) {
                if (entry.IsCompressed) {
                    _compressedItems.Add(new DBDFItem((uint) entry.TGI.TypeID, (uint) entry.TGI.GroupID, (uint) entry.TGI.InstanceID, entry.UncompressedSize));
                }
            }
        }



        /// <summary>
        /// Build <see cref="DBPFEntry.ByteData"/> from the current state of this instance.
        /// </summary>
		/// <param name="compress">Note this has no effect as DIR entries always remain uncompressed</param>
        public override void Encode(bool compress = false) {
            byte[] bytes = new byte[_compressedItems.Count * 16];
            int pos = 0;

            foreach (DBDFItem item in _compressedItems) {
                Buffer.BlockCopy(BitConverter.GetBytes(item.TID), 0, bytes, pos, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(item.GID), 0, bytes, pos + 4, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(item.IID), 0, bytes, pos + 8, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(item.Size), 0, bytes, pos + 12, 4);
                pos += 16;
            }
            ByteData = bytes;
        }
    }
}