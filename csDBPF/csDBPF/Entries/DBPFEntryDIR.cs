using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csDBPF.Entries {

	//https://wiki.sc4devotion.com/index.php?title=DBDF

	public class DBPFEntryDIR : DBPFEntry {
		private bool _isDecoded;
		private Dictionary<DBPFTGI,uint> _compressedItems;
		/// <summary>
		/// Dictionary of TGIs to the size of the decompressed file each TGI represents, in bytes.
		/// </summary>
		public Dictionary<DBPFTGI,uint> CompressedItems {
			get { return _compressedItems; }
			private set { _compressedItems = value; }
		}



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


		//TODO Implement Update
		public void Update() {
			throw new NotImplementedException();
		}
	}
}
