using csDBPF.Properties;
using System;
using System.Collections.Generic;
using System.Text;

//See: https://github.com/memo33/jDBPFX/blob/master/src/jdbpfx/DBPFEntry.java
namespace csDBPF {
	/// <summary>
	/// An abstract form of an entry item of a <see cref="DBPFFile"/>, representing an instance of a subfile that may be contained in a DBPF file.
	/// </summary>
	public class DBPFEntry {
		//------------- DBPFEntry Fields ------------- \\
		/// <summary>
		/// The <see cref="DBPFTGI"/>object representing the file type of the entry.
		/// </summary>
		private DBPFTGI _tgi;
		public DBPFTGI TGI {
			get { return _tgi; }
		}
		/// <summary>
		/// Byte position of this entry within the DBPFFile.
		/// </summary>
		private uint _offset;
		public uint Offset {
			get { return _offset; }
			//set { _offset = value; }
		}
		/// <summary>
		/// Position of this entry in relation to the other entries in the DBPFFile.
		/// </summary>
		private uint _index;
		public uint IndexPos {
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
		public uint UncompressedSize {
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
		public uint CompressedSize {
			get { return _compressedSize; }
			set { _compressedSize = value; }
		}
		/// <summary>
		/// Compression status of the entry data.
		/// </summary>
		/// <remarks>
		/// Assume TRUE until the first bytes of data can be read to determine actual compression status. 
		/// </remarks>
		private bool _isCompressed;
		public bool IsCompressed {
			get { return _isCompressed; }
			set { _isCompressed = value; }
		}
		/// <summary>
		/// Byte array of data pertaining to this entry. Depending on the value is IsCompressed, this data could be compressed or not.
		/// </summary>
		/// <remarks>
		/// The interpretation of the entry data depends on the compression status of the entry and also on the file type of the entry (known through its <see cref="TGI"/>). Always check if the data is compressed before processing.
		/// </remarks>
		private byte[] _data;
		public byte[] Data {
			get { return _data; }
			set { _data = value; }
		}



		//------------- DBPFEntry Constructors ------------- \\
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
		/// <param name="size">Compressed size of data for the entry, in bytes. Uncompressed size is also temporarily set to this to this until the data is set</param>
		/// <param name="index">Entry position in the file. 0-n</param>
		public DBPFEntry(DBPFTGI tgi, uint offset, uint size, uint index) {
			if (tgi == null) {
				_tgi = DBPFTGI.NULLTGI;
			} else {
				_tgi = tgi;
			}
			_offset = offset;
			_index = index;
			_compressedSize = size;
			//Note the properties below cannot be definitively determined until after the data is read and set - assign placeholder defaults for now
			_uncompressedSize = size;
			_isCompressed = true;
		}



		//------------- DBPFEntry Methods ------------- \\
		public override string ToString() {
			StringBuilder sb = new StringBuilder(_tgi.ToString());
			sb.AppendLine($", Type: {_tgi.Label}, IndexPos: {_index}, Offset: {_offset}, uSize: {_uncompressedSize}, Comp: {_isCompressed}, cSize: {_compressedSize} ");
			return sb.ToString();
		}

		/// <summary>
		/// Parses the byte values of the entry depending on the entry's data type.
		/// </summary>
		/// <returns>Decoded object. Object type differs depending on the entry data type.</returns>
		public object DecodeEntry() {
			switch (TGI.Label) {
				case "EXEMPLAR":
					return DecodeEntry_EXMP(_data); //return Dictionary<int, DBPFProperty>
				case "LTEXT":
					return DecodeEntry_LTEXT(_data); //return string
				default:
					return null;
			}
		}



		public Dictionary<int,DBPFProperty> DecodeEntry_EXMP() {
			return DecodeEntry_EXMP(_data);
		}


		/// <summary>
		/// Decodes the compressed data into a dictionary of one or more <see cref="DBPFProperty"/>.
		/// </summary>
		/// <param name="cData">Compressed data</param>
		/// <returns>Dictionary of <see cref="DBPFProperty"/> indexed by their order in the entry</returns>
		public static Dictionary<int, DBPFProperty> DecodeEntry_EXMP(byte[] cData) {
			byte[] dData;
			if (DBPFCompression.IsCompressed(cData)) {
				dData = DBPFCompression.Decompress(cData);
			} else {
				dData = cData;
			}

			Dictionary<int, DBPFProperty> listOfProperties = new Dictionary<int, DBPFProperty>();

			//Read cohort TGI info and determine the number of properties in this entry
			uint parentCohortTID = BitConverter.ToUInt32(dData, 8);
			uint parentCohortGID = BitConverter.ToUInt32(dData, 12);
			uint parentCohortIID = BitConverter.ToUInt32(dData, 16);
			uint propertyCount = BitConverter.ToUInt32(dData, 20);

			int pos = 24;
			DBPFProperty property;
			for (int idx = 0; idx < propertyCount; idx++) {
				property = DBPFProperty.DecodeExemplarProperty_Binary(dData, pos);
				listOfProperties.Add(idx, property);
				pos += property.ByteValues.Length + 9; //Skip 4 bytes for ID, 2 for DataType, 2 for KeyType, 1 unused byte
				if (property.KeyType == 0x80) { //Skip 4 more for NumberOfValues
					pos += 4;
				}
			}
			return listOfProperties;
		}


		/// <summary>
		/// Decodes the LTEXT string from raw data. Data is not compressed.
		/// </summary>
		/// <param name="data">Raw data of the LTEXT entry (not compressed)</param>
		/// <returns>A string</returns>
		public static string DecodeEntry_LTEXT(byte[] data) {
			int pos = 0;
			ushort numberOfChars = BitConverter.ToUInt16(data, pos);
			pos += 2;
			ushort textControlChar = ByteArrayHelper.ReadBytesIntooUshort(data, pos);
			pos += 2;
			if (textControlChar != 0x0010) {
				throw new ArgumentException("Data is not valid LTEXT format!");
			}

			StringBuilder sb = new StringBuilder();
			for (int idx = 0; idx < numberOfChars; idx++) {
				sb.Append(BitConverter.ToChar(data, pos));
				pos += 2;
			}
			return sb.ToString();
		}
	}
}
