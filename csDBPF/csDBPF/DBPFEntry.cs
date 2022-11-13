using csDBPF.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace csDBPF {
	/// <summary>
	/// An abstract form of an entry item of a <see cref="DBPFFile"/>, representing an instance of a subfile that may be contained in a DBPF file. The data for each entry is not parsed or decoded until <see cref="DecodeEntry"/> is called to decompress and set the actual entry data.
	/// </summary>
	public partial class DBPFEntry {
		private const string EQZB = "EQZB";
		private const string EQZT = "EQZT";
		private const string CQZB = "CQZB";
		private const string CQZT = "CQZT";



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

		private byte[] _byteData;
		/// <summary>
		/// Byte array of raw (not decoded) data pertaining to this entry. Depending on the value is IsCompressed, this data could be compressed.
		/// </summary>
		/// <remarks>
		/// The interpretation of the entry data depends on the compression status of the entry and also on the file type of the entry (known through its <see cref="TGI"/>). Always check if the data is compressed before processing.
		/// </remarks>
		public byte[] ByteData {
			get { return _byteData; }
			set { _byteData = value; }
		}

		private List<DBPFProperty> _listOfProperties;
		/// <summary>
		/// Dictionary of one or more <see cref="DBPFProperty"/> associated with this entry.
		/// </summary>
		/// <remarks>
		/// Relevant for only Exemplar and Cohort type entries. This is null for all other type entries. Use <see cref="DecodedData"/> for other type entries.
		/// </remarks>
		public List<DBPFProperty> ListOfProperties {
			get { return _listOfProperties; }
			set { _listOfProperties = value; }
		}

		private byte[] _decodedData;
		/// <summary>
		/// Byte array of decoded data for this entry. This is always uncompressed.
		/// </summary>
		/// <remarks>
		/// Relevant for all type entries except Exemplars and Cohorts. This is null for those type entries. Use <see cref="ListOfProperties"/> for Exemplars and Cohorts.
		/// </remarks>
		public byte[] DecodedData {
			get { return _decodedData; }
			set { _decodedData = value; }
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
			_listOfProperties = null;
			_decodedData = null;
		}



		//------------- DBPFEntry Methods ------------- \\
		public override string ToString() {
			StringBuilder sb = new StringBuilder(_tgi.ToString());
			sb.AppendLine($", Type: {_tgi.Label}, IndexPos: {_index}, Offset: {_offset}, uSize: {_uncompressedSize}, Comp: {_isCompressed}, cSize: {_compressedSize} ");
			return sb.ToString();
		}


		/// <summary>
		/// Parses the byte values of the entry depending on the entry's data type. This exposes the list of properties (what properties are present) but does not expose the contents of those properties. If Exemplar or Cohort this sets <see cref="ListOfProperties"/>, otherwise <see cref="DecodedData"/> is set.
		/// </summary>
		/// <remarks>
		/// This is the default function and should be used in most circumstances when there is already data associated with the entry, e.g. when reading from a file.
		/// </remarks>
		public void DecodeEntry() {
			//TODO - should have something in here to check if there is actually any data associated with the entry besides its header, and if not dont parse or mark for deletion or something else. example an exemplar entry with no properties - byte size = 24 = size of header
			//although be careful with blank LTEXT - those should exist still
			switch (TGI.Label) {
				case "EXEMPLAR":
				case "COHORT":
					_listOfProperties = DecodeEntry_EXMP(_byteData);
					break;
				case "LTEXT":
					_decodedData = DecodeEntry_LTEXT(_byteData);
					break;
				default:
					break;
			}
		}
		/// <summary>
		/// Parses the byte values of the entry depending on the entry's data type. This exposes the list of properties (what properties are present) but does not expose the contents of those properties. If Exemplar or Cohort this sets <see cref="ListOfProperties"/>, otherwise <see cref="DecodedData"/> is set.
		/// </summary>
		/// <param name="data">Byte data to decode</param>
		/// <remarks>
		/// This alternative can be used when creating entries that do not already have data associated with them.
		/// </remarks>
		public void DecodeEntry(byte[] data) {
			switch (TGI.Label) {
				case "EXEMPLAR":
				case "COHORT":
					_listOfProperties = DecodeEntry_EXMP(data);
					break;
				case "LTEXT":
					_decodedData = DecodeEntry_LTEXT(data);
					break;
				default:
					break;
			}
		}



		/// <summary>
		/// Decode all properties in the entry. Only valid for Exemplar/Cohort type entries.
		/// </summary>
		public void DecodeAllProperties() {
			if (!(_tgi.MatchesKnownTGI(DBPFTGI.COHORT) || _tgi.MatchesKnownTGI(DBPFTGI.EXEMPLAR))) {
				throw new InvalidOperationException("This function can only be called on Exemplar and Cohort type entries!");
			}

			foreach (DBPFProperty property in _listOfProperties) {
				property.DecodeValues();
			}
		}



		/// <summary>
		/// Determine if the entry is the same entry type as the specifified one
		/// </summary>
		/// <param name="known"><see cref="DBPFTGI"/> to compare against</param>
		/// <returns>TRUE if this Entry matches the specified; FALSE otherwise.</returns>
		/// <remarks>This is a shortcut to accessing DBPFEntry.TGI.MatchesKnownEntryType instead.</remarks>
		public bool MatchesKnownEntryType(DBPFTGI known) {
			return _tgi.MatchesKnownTGI(known);
		}



		/// <summary>
		/// Lookup and return a property from a list of properties in the entry.
		/// </summary>
		/// <param name="idToGet">Property ID to find</param>
		/// <param name="properties">Dictionary of properties to search</param>
		/// <returns>DBPFProperty of the match if found; null otherwise</returns>
		public DBPFProperty GetProperty(uint idToGet) {
			if (_listOfProperties is null) {
				throw new InvalidOperationException("This entry must be decoded before it can be analyzed!");
			}

			foreach (DBPFProperty property in _listOfProperties) {
				if (property.ID == idToGet) {
					return property;
				}
			}
			return null;
		}
		/// <summary>
		/// Lookup and return a property from a list of properties in the entry.
		/// </summary>
		/// <param name="name">Name of property</param>
		/// <returns>DBPFProperty of the match if found; null otherwise</returns>
		/// <remarks>
		/// Lookup name is case insensitive and ignores spaces (the XML properties can be inconsistently named).
		/// </remarks>
		public DBPFProperty GetProperty(string name) {
			uint id = XMLProperties.LookupPropertyID(name);
			return GetProperty(id);
		}



		/// <summary>
		/// Gets the Exemplar Type (0x00 - 0x2B) of the property. See <see cref="DBPFProperty.ExemplarTypes"/> for the full list.
		/// </summary>
		/// <returns>Exemplar Type if found; -1 if entry is not Exemplar or Cohort; -2 if "ExemplarType" property is not found</returns>
		public int GetExemplarType() {
			if (!(_tgi.MatchesKnownTGI(DBPFTGI.EXEMPLAR) || _tgi.MatchesKnownTGI(DBPFTGI.COHORT))) {
				return -1;
			}
			DBPFProperty property = GetProperty(0x00000010);

			if (property is null) {
				return -2;
			}

			Array propertyType = Array.CreateInstance(property.DataType.PrimitiveDataType, property.NumberOfReps); //Create new array to hold the values
			property.DecodeValues();
			propertyType = property.DecodedValues; //Set the values from the decoded property
			//return unchecked((int) propertyType.GetValue(0)); //We know exemplar type can only hold one value, so grab the first one.... BUT HOW?????????
			return Convert.ToInt32(propertyType.GetValue(0));
		}




		//------------- DBPFEntry Static Methods ------------- \\



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
			string fileIdentifier = ByteArrayHelper.ToAString(dData, 0, 4);
			switch (checkType) {
				case 1:
					if (fileIdentifier != EQZB && fileIdentifier != CQZB) {
						throw new ArgumentException("Data provided does not represent an exemplar or cohort property, or is not in binary format!");
					}
					return 1;
				case 2:
					if (fileIdentifier != EQZT && fileIdentifier != CQZT) {
						throw new ArgumentException("Data provided does not represent an exemplar or cohort property, or is not in text format!");
					}
					return 2;
				case 3:
					if (fileIdentifier != EQZB && fileIdentifier != EQZT && fileIdentifier != CQZB && fileIdentifier != CQZT) {
						throw new ArgumentException("Data provided does not represent an exemplar or cohort property.");
					}
					if (fileIdentifier == EQZB || fileIdentifier == CQZB) {
						return 1;
					} else {
						return 2;
					}
				default:
					return 0;
			}
		}




		/// <summary>
		/// Returns the encoding type of the property (Binary or Text).
		/// </summary>
		/// <param name="dData">Byte data for a property</param>
		/// <returns>1 if Binary encoding, 2 if Text encoding</returns>
		public static int GetEncodingType(byte[] dData) {
			return ValidateData(dData, 3);
		}







	}
}
