using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace csDBPF.Entries {
	/// <summary>
	/// An implementation of <see cref="DBPFEntry"/> for Directory entries. Object data is stored in <see cref="CompressedItems"/>.
	/// </summary>
	/// <see ref="https://wiki.sc4devotion.com/index.php?title=DBDF"/>
	public class DBPFEntryDIR : DBPFEntry {
		/// <summary>
		/// Stores if this entry has been decoded yet.
		/// </summary>
		private bool _isDecoded;

		private Dictionary<DBPFTGI,uint> _compressedItems;
		/// <summary>
		/// Dictionary of TGIs to the size of the decompressed file each TGI represents, in bytes.
		/// </summary>
		public Dictionary<DBPFTGI,uint> CompressedItems {
			get { return _compressedItems; }
			private set { _compressedItems = value; }
		}



		/// <summary>
		/// Create a new instance. Use when creating a new Directory.
		/// </summary>
		public DBPFEntryDIR() : base(DBPFTGI.DIRECTORY) {

		}

		/// <summary>
		/// Create a new instance. Use when reading an existing Directy from a file.
		/// </summary>
		/// <param name="tgi"><see cref="DBPFTGI"/> object representing the entry</param>
		/// <param name="offset">Offset (location) of the entry within the DBPF file</param>
		/// <param name="size">Compressed size of data for the entry, in bytes. Uncompressed size is also temporarily set to this to this until the data is set</param>
		/// <param name="index">Entry position in the file, 0-n</param>
		/// <param name="bytes">Byte data for this entry</param>
		public DBPFEntryDIR(DBPFTGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {
			_compressedItems = new  Dictionary<DBPFTGI, uint>();
		}



		/// <summary>
		/// Sets the directory entry from raw data and sets the <see cref="CompressedItems"/> property of this instance.
		/// </summary>
		public override void DecodeEntry() {
			if (_isDecoded) {
				return;
			}

			DBPFTGI tgi;
			for (int pos = 0; pos < ByteData.Length; pos+=16) {
				tgi = new DBPFTGI(BitConverter.ToUInt16(ByteData, pos), BitConverter.ToUInt16(ByteData, pos + 4), BitConverter.ToUInt16(ByteData, pos + 8));
				_compressedItems.Add(tgi, BitConverter.ToUInt16(ByteData, pos + 12));
			}
			_isDecoded = true;
		}
	}
}