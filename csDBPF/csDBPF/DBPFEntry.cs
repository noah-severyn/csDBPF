using System;
using System.Collections.Generic;
using System.Text;

//See: https://github.com/memo33/jDBPFX/blob/master/src/jdbpfx/DBPFEntry.java
namespace csDBPF {
	/// <summary>
	/// An abstract form of an entry item of a <see cref="DBPFFile"/>, representing an instance of a subfile that may be contained in a DBPF file.
	/// </summary>
	public class DBPFEntry {
		private DBPFTGI _tgi;
		public DBPFTGI TGI {
			get { return _tgi; }
			//set {
			//	if (value == null) {
			//		throw new Exception("Null TGI");
			//	} else {
			//		_tgi = value;
			//	}
			//}
		}

		private uint _offset;
		public uint offset {
			get { return _offset; }
			//set { _offset = value; }
		}

		private uint _index;
		public uint indexPos {
			get { return _index; }
			//set { _index = value; }
		}


		/// <summary>
		/// Uncompressed size of the entry data, in bytes.
		/// </summary>
		/// <remarks>
		/// Initially all data is assumed to be compressed until the first bytes of data can be read to determine actual compression status.
		/// </remarks>
		private uint _uncompressedSize;
		public uint uncompressedSize {
			get { return _uncompressedSize; }
			set { _uncompressedSize = value; }
		}

		/// <summary>
		/// Compressed size of the entry data, in bytes.
		/// </summary>
		/// <remarks>
		/// Initially all data is assumed to be compressed until the first bytes of data can be read to determine actual compression status.
		/// </remarks>
		private uint _compressedSize;
		public uint compressedSize {
			get { return _compressedSize; }
			set { _compressedSize = value; }
		}


		private bool _isCompressed;
		public bool isCompressed {
			get { return _isCompressed; }
			set { isCompressed = value; }
		}

		private byte[] _data;
		public byte[] data {
			get { return _data; }
			set { _data = value; }
		}





		// Constructor
		/// <summary>
		/// Create a new DBPFEntry object.
		/// </summary>
		/// <param name="tgi"><see cref="DBPFTGI"/> object representing the entry</param>
		public DBPFEntry(DBPFTGI tgi) {
			_tgi = tgi;
		}

		/// <summary>
		/// Create a new DBPFEntry object.
		/// </summary>
		/// <param name="tgi"><see cref="DBPFTGI"/> object representing the entry</param>
		/// <param name="offset">Offset (location) of the entry within the DBPF file</param>
		/// <param name="size">Size of data for the entry, in bytes. Set both compressed and uncompressed size to this until the data is set to make a determination</param>
		/// <param name="index">Entry position in the file. 0-n</param>
		public DBPFEntry(DBPFTGI tgi, uint offset, uint size, uint index) {
			if (tgi == null) {
				//throw new Exception("Null TGI");
				_tgi = DBPFTGI.NULLTGI;
			} else {
				_tgi = tgi;
			}
			_offset = offset;
			_uncompressedSize = size;
			_isCompressed = true;
			_compressedSize = size;
			_index = index;
			//Note: for other properties, we cannot know them until after the data is read and set
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder(_tgi.ToString());
			sb.AppendLine($", IndexPos: {_index}, Offset: {_offset}, uSize: {_uncompressedSize}, Compr: {_isCompressed}, cSize: {_compressedSize} ");
			return sb.ToString();
		}


	}
}
