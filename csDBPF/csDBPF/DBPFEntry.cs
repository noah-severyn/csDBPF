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
		private const string EQZB1 = "EQZB1###";
		private const string EQZT1 = "EQZT1###";
		private const string CQZB1 = "CQZB1###";
		private const string CQZT1 = "CQZT1###";



		//------------- DBPFEntry Fields ------------- \\
		private DBPFTGI _tgi;
		/// <summary>
		/// The <see cref="DBPFTGI"/>object representing the file type of the entry.
		/// </summary>
		public DBPFTGI TGI {
			get { return _tgi; }
		}

		private uint _offset;
		/// <summary>
		/// Byte position of this entry within the <see cref="DBPFFile"/>.
		/// </summary>
		public uint Offset {
			get { return _offset; }
		}

		private uint _index;
		/// <summary>
		/// Position of this entry in relation to the other entries in the <see cref="DBPFFile"/>, 0-n.
		/// </summary>
		public uint IndexPos {
			get { return _index; }
		}

		private uint _uncompressedSize;
		/// <summary>
		/// Uncompressed size of the entry data, in bytes.
		/// </summary>
		/// <remarks>
		/// Initially all data is assumed to be compressed until the first bytes of data can be read to determine actual compression status.
		/// </remarks>
		public uint UncompressedSize {
			get { return _uncompressedSize; }
			set { _uncompressedSize = value; }
		}

		private uint _compressedSize;
		/// <summary>
		/// Compressed size of the entry data, in bytes.
		/// </summary>
		/// <remarks>
		/// Initially all data is assumed to be compressed until the first bytes of data can be read to determine actual compression status.
		/// </remarks>
		public uint CompressedSize {
			get { return _compressedSize; }
			set { _compressedSize = value; }
		}

		private bool _isCompressed;
		/// <summary>
		/// Compression status of this entry.
		/// </summary>
		/// <remarks>
		/// Assume TRUE until the first bytes of data can be read to determine actual compression status. 
		/// </remarks>
		public bool IsCompressed {
			get { return _isCompressed; }
			set { _isCompressed = value; }
		}

		private byte[] _data;
		/// <summary>
		/// Byte array of data pertaining to this entry. Depending on the value is IsCompressed, this data could be compressed or not.
		/// </summary>
		/// <remarks>
		/// The interpretation of the entry data depends on the compression status of the entry and also on the file type of the entry (known through its <see cref="TGI"/>). Always check if the data is compressed before processing.
		/// </remarks>
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
		/// <param name="index">Entry position in the file, 0-n</param>
		public DBPFEntry(DBPFTGI tgi, uint offset, uint size, uint index) {
			if (tgi == null) {
				_tgi = DBPFTGI.NULLTGI;
			} else {
				_tgi = tgi;
			}
			_offset = offset;
			_index = index;
			_compressedSize = size;
			//The following properties cannot be definitively determined until after the data is read and set, so assume for now
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
					return DecodeEntry_EXMP(_data); //returns Dictionary<int, DBPFProperty>
				case "LTEXT":
					return DecodeEntry_LTEXT(_data); //returns string
				default:
					return null;
			}
		}


		//TODO - this should be internal? or not exist?. Access through .DecodeEntry only
		public Dictionary<int, DBPFProperty> DecodeEntry_EXMP() {
			return DecodeEntry_EXMP(_data);
		}


		/// <summary>
		/// Lookup and return the target property from a list of properties.
		/// </summary>
		/// <param name="idToGet">Property ID to find</param>
		/// <param name="properties">Dictionary of properties to search</param>
		/// <returns>DBPFProperty of the match if found; null otherwise</returns>
		public DBPFProperty GetProperty(uint idToGet, Dictionary<int, DBPFProperty> properties) {
			foreach (DBPFProperty property in properties.Values) {
				if (property.ID == idToGet) {
					return property;
				}
			}
			return null;
		}


		public DBPFProperty GetProperty(string name, Dictionary<int, DBPFProperty> properties) {
			//TODO - implement GetProperty(string name, Dictionary<int, DBPFProperty> properties)
			//XMLProperties.AllProperties.
			return null;
		}




		//------------- DBPFEntry Static Methods ------------- \\
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
			uint parentCohortTID;
			uint parentCohortGID;
			uint parentCohortIID;
			uint propertyCount;
			int pos; //Offset position in dData. Initialized to the starting position of the properties after the header data
			switch (GetEncodingType(dData)) {
				case 1: //Binary encoding
					parentCohortTID = BitConverter.ToUInt32(dData, 8);
					parentCohortGID = BitConverter.ToUInt32(dData, 12);
					parentCohortIID = BitConverter.ToUInt32(dData, 16);
					propertyCount = BitConverter.ToUInt32(dData, 20);
					pos = 24;
					break;
				case 2: //Text encoding
					parentCohortTID = ByteArrayHelper.ReadTextIntoUint(dData, 30);
					parentCohortGID = ByteArrayHelper.ReadTextIntoUint(dData, 41);
					parentCohortIID = ByteArrayHelper.ReadTextIntoUint(dData, 52);
					propertyCount = ByteArrayHelper.ReadTextIntoUint(dData, 75);
					pos = 85;
					break;
				default:
					propertyCount = 0;
					pos = 0;
					break;
			}

			//Create the Property
			DBPFProperty property;
			for (int idx = 0; idx < propertyCount; idx++) {
				property = DBPFProperty.DecodeExemplarProperty(dData, pos);
				listOfProperties.Add(idx, property);

				//Determine which bytes to skip to get to the start of the next property
				switch (GetEncodingType(dData)) {
					case 1: //Binary encoding
						pos += property.ByteValues.Length + 9; //Additionally skip the 4 bytes for ID, 2 for DataType, 2 for KeyType, 1 unused byte
						if (property.KeyType == 0x80) { //Skip 4 more for NumberOfValues
							pos += 4;
						}
						break;
					case 2: //Text encoding
						pos = ByteArrayHelper.FindNextInstanceOf(dData, 0x0A, pos) + 1;
						break;
				}
			}

			return listOfProperties;
		}


		/// <summary>
		/// Check if data is compressed and if fileIdentifier is valid. Throws ArgumentException if either is not true.
		/// </summary>
		/// <param name="dData">Decompressed byte data</param>
		/// <param name="checkType">1 for Binary, 2 for Text, 3 for Either</param>
		/// <returns>1 if Binary encoding, 2 if Text encoding</returns>
		private static int ValidateData(byte[] dData, int checkType) {
			if (DBPFCompression.IsCompressed(dData)) {
				throw new ArgumentException("Data cannot be compressed!");
			}
			string fileIdentifier = ByteArrayHelper.ToAString(dData, 0, 8);
			switch (checkType) {
				case 1:
					if (fileIdentifier != EQZB1 && fileIdentifier != CQZB1) {
						throw new ArgumentException("Data provided does not represent an exemplar or cohort property, or is not in binary format!");
					}
					return 1;
				case 2:
					if (fileIdentifier != EQZT1 && fileIdentifier != CQZT1) {
						throw new ArgumentException("Data provided does not represent an exemplar or cohort property, or is not in text format!");
					}
					return 2;
				case 3:
					if (fileIdentifier != EQZB1 && fileIdentifier != EQZT1 && fileIdentifier != CQZB1 && fileIdentifier != CQZT1) {
						throw new ArgumentException("Data provided does not represent an exemplar or cohort property.");
					}
					if (fileIdentifier == EQZB1 || fileIdentifier == CQZB1) {
						return 1;
					} else {
						return 2;
					}
				default:
					return 0;
			}
		}




		//handy to have a function to get the exemplar type
		/// <summary>
		/// Gets the Exemplar Type (0x00 - 0x2B) of the property. See <see cref="https://www.wiki.sc4devotion.com/index.php?title=Exemplar"/>
		/// </summary>
		/// <param name="dData"></param>
		/// <returns></returns>
		public int GetExemplarType(byte[] dData) {
			if (!TGI.MatchesKnownTGI(DBPFTGI.EXEMPLAR)) {
				return 0;
			}



			return 0;
		}



		/// <summary>
		/// Returns the encoding type of the property (Binary or Text).
		/// </summary>
		/// <param name="dData">Byte data for a property</param>
		/// <returns>1 if Binary encoding, 2 if Text encoding</returns>
		public static int GetEncodingType(byte[] dData) {
			return ValidateData(dData, 3);
		}



		//TODO - this should be internal? or not exist?. Access through .DecodeEntry only
		/// <summary>
		/// Decodes the LTEXT string from raw data. Data is not compressed.
		/// </summary>
		/// <param name="data">Raw data of the LTEXT entry (not compressed)</param>
		/// <returns>A string</returns>
		/// 
		public static string DecodeEntry_LTEXT(byte[] data) {
			int pos = 0;
			ushort numberOfChars = BitConverter.ToUInt16(data, pos);
			pos += 2;
			ushort textControlChar = ByteArrayHelper.ReadBytesIntoUshort(data, pos);
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
