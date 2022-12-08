using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using csDBPF.Properties;
using static System.Net.WebRequestMethods;

namespace csDBPF.Entries {
	/// <summary>
	/// An implementation of <see cref="DBPFEntry"/> for Exemplar and Cohort entries. Object data is stored in <see cref="ListOfProperties"/>.
	/// </summary>
	/// <see ref="https://wiki.sc4devotion.com/index.php?title=EXMP"/>
	public class DBPFEntryEXMP : DBPFEntry {
		/// <summary>
		/// Stores if this entry has been decoded yet.
		/// </summary>
		private bool _isDecoded;

		private List<DBPFProperty> _listOfProperties;
		/// <summary>
		/// List of one or more <see cref="DBPFProperty"/> associated with this entry.
		/// </summary>
		public List<DBPFProperty> ListOfProperties {
			get { return _listOfProperties; }
			set { _listOfProperties = value; }
		}

		private DBPFTGI _parentCohort;
		/// <summary>
		/// TGI set representing the Parent Cohort for this exemplar.
		/// </summary>
		/// <see ref="https://www.wiki.sc4devotion.com/index.php?title=Cohort"/>
		public DBPFTGI ParentCohort {
			get { return _parentCohort; }
			set { _parentCohort = value; }
		}



		/// <summary>
		/// Create a new instance. Use when creating new exemplars.
		/// </summary>
		/// <param name="tgi">TGI set to assign</param>
		public DBPFEntryEXMP(DBPFTGI tgi) : base(tgi) {
			if (tgi is null) {
				TGI.SetTGI(DBPFTGI.EXEMPLAR);
			}
		}

		/// <summary>
		/// Create a new instance. Use when reading existing exemplars from a file.
		/// </summary>
		/// <param name="tgi"><see cref="DBPFTGI"/> object representing the entry</param>
		/// <param name="offset">Offset (location) of the entry within the DBPF file</param>
		/// <param name="size">Compressed size of data for the entry, in bytes. Uncompressed size is also temporarily set to this to this until the data is set</param>
		/// <param name="index">Entry position in the file, 0-n</param>
		/// <param name="bytes">Byte data for this entry</param>
		public DBPFEntryEXMP(DBPFTGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {
			_listOfProperties = new List<DBPFProperty>();
		}


		/// <summary>
		/// Decodes the compressed data into a dictionary of one or more <see cref="DBPFProperty"/>.
		/// </summary>
		/// <returns>Dictionary of <see cref="DBPFProperty"/> indexed by their order in the entry</returns>
		public override void DecodeEntry() {
			if (_isDecoded) {
				return;
			}

			byte[] cData = ByteData;
			byte[] dData;
			if (DBPFCompression.IsCompressed(cData)) {
				dData = DBPFCompression.Decompress(cData);
			} else {
				dData = cData;
			}

			List<DBPFProperty> listOfProperties = new List<DBPFProperty>();

			//Read cohort TGI info and determine the number of properties in this entry
			uint parentCohortTID;
			uint parentCohortGID;
			uint parentCohortIID;
			uint propertyCount;
			int pos; //Offset position in dData. Initialized to the starting position of the properties after the header data
			if (IsBinaryEncoding()) {
				parentCohortTID = BitConverter.ToUInt32(dData, 8);
				parentCohortGID = BitConverter.ToUInt32(dData, 12);
				parentCohortIID = BitConverter.ToUInt32(dData, 16);
				_parentCohort.SetTGI(parentCohortTID,parentCohortGID,parentCohortIID);
				propertyCount = BitConverter.ToUInt32(dData, 20);
				pos = 24;
			} else {
				parentCohortTID = ByteArrayHelper.ReadTextIntoUint(dData, 30);
				parentCohortGID = ByteArrayHelper.ReadTextIntoUint(dData, 41);
				parentCohortIID = ByteArrayHelper.ReadTextIntoUint(dData, 52);
				_parentCohort.SetTGI(parentCohortTID, parentCohortGID, parentCohortIID);
				propertyCount = ByteArrayHelper.ReadTextIntoUint(dData, 75);
				pos = 85;
			}

			//Create the Property
			DBPFProperty property;
			for (int idx = 0; idx < propertyCount; idx++) {
				property = DecodeProperty(pos);
				listOfProperties.Add(property);

				//Determine which bytes to skip to get to the start of the next property
				if (IsBinaryEncoding()) {
					pos += property.ByteValues.Length + 9; //Additionally skip the 4 bytes for ID, 2 for DataType, 2 for KeyType, 1 unused byte
					if (property.KeyType == 0x80) { //Skip 4 more for NumberOfValues
						pos += 4;
					}
				} else {
					pos = ByteArrayHelper.FindNextInstanceOf(dData, 0x0A, pos) + 1;
				}
			}

			_listOfProperties = listOfProperties;
		}



		/// <summary>
		/// Decodes either Binary or Text encoded data, and returns the property.
		/// </summary>
		/// <param name="offset">Offset (location) to start reading from</param>
		/// <returns>A <see cref="DBPFProperty"/></returns>
		internal DBPFProperty DecodeProperty(int offset = 0) {
			if (IsBinaryEncoding()) {
				return DecodeProperty_Binary(ByteData, offset);
			} else {
				return DecodeProperty_Text(ByteData, offset);
			}
		}



		/// <summary>
		/// Decodes the property from raw binary data at the given offset.
		/// </summary>
		/// <param name="dData">Decompressed binary data</param>
		/// <param name="offset">Offset to start decoding from</param>
		/// <returns>The DBPFProperty; null if it cannot be decoded</returns>
		internal static DBPFProperty DecodeProperty_Binary(byte[] dData, int offset = 24) {
			//Check for exemplars with no properties. Later checks against dData.Length to account for extraneous bytes on the end of dData that do not correspond to valid properties of the property
			if (dData.Length <= 24) return null;

			//The first 24 bytes are features of the entry: ParentCohort TGI and property count. When examining a specific property in the entry we are not concerned about them.
			if (offset < 24) {
				offset = 24;
			}

			//Get the property ID
			if (offset + 4 > dData.Length) return null;
			uint propertyID = BitConverter.ToUInt32(dData, offset);
			offset += 4;

			//Get the data value type
			ushort valueType = BitConverter.ToUInt16(dData, offset);
			DBPFPropertyDataType dataType = DBPFPropertyDataType.LookupDataType(valueType);
			if (dataType is null) return null;
			offset += 2;

			//Get the property keyType
			if (offset + 2 > dData.Length) return null;
			ushort keyType = BitConverter.ToUInt16(dData, offset);
			offset += 2;

			//Create new decoded property then set id and dataType
			DBPFProperty newProperty;
			if (dataType.Name == "STRING") {
				newProperty = new DBPFPropertyString();
			} else {
				newProperty = new DBPFPropertyNumber(dataType);
			}
			newProperty.ID = propertyID;
			newProperty.KeyType = keyType;

			//Examine the keyType to determine how to set the values for the new property
			//keyType == 0x80 ... this is one or more repetitions of the data type
			if (keyType == 0x80) {
				offset += 1; //There is a 1 byte unused flag
				uint countOfReps = BitConverter.ToUInt32(dData, offset);
				offset += 4;
				byte[] newValue = new byte[countOfReps * newProperty.DataType.Length];
				for (int idx = 0; idx < newValue.Length; idx++) {
					newValue[idx] = dData[offset + idx];
				}
				newProperty.NumberOfReps = countOfReps;
				newProperty.ByteValues = newValue;
				//newProperty.SetValues(newValue);
			}

			//keyType == 0x00 ... this is just a single value of the data type length
			else {
				offset += 1; //This one byte is number of value repetitions; seems to always be 0
				byte[] newVals = new byte[dataType.Length];
				for (int idx = 0; idx < dataType.Length; idx++) {
					newVals[idx] = dData[offset + idx];
				}
				newProperty.NumberOfReps = 0;
				newProperty.ByteValues = newVals;
				//newProperty.SetValues(newVals);
			}
			return newProperty;
		}



		/// <summary>
		/// Enumeration for special delimiting characters when reading text encoded properties.
		/// </summary>
		private enum SpecialChars : byte {
			Colon = 0x3A, // :
			Comma = 0x2C, // ,
			Equal = 0x3D, // =
			OpeningBrace = 0x7B, // {
			ClosingBrace = 0x7D, // }
			Quotation = 0x22 // "
		}

		/// <summary>
		/// Decodes the property from raw text data at the given offset.
		/// </summary>
		/// <param name="dData">Decompressed text data</param>
		/// <param name="offset">Offset to start decoding from</param>
		/// <returns>The DBPFProperty; null if cannot be decoded</returns>
		internal static DBPFProperty DecodeProperty_Text(byte[] dData, int offset = 85) {
			//The sequence 0D0A (i.e. {0x0D, 0x0A}) separates each piece of entry header information and each property

			//The first 8 bytes are the fileIdentifier, as usual (EQZT1### etc)
			//Next two bytes for the delimiter 0D0A
			//Parent Cohort is the text `ParentCohort=Key:{0x00000000,0x00000000,0x00000000}`
			//Next two bytes for the delimiter 0D0A
			//Number of properties is the text `PropCount=0x00000000`
			//Next two bytes for the delimiter 0D0A

			//Now comes the list of properties.
			//For each property hex ID preceded with 0x (for 10 bytes) then the format `:{"TextExemplarName"}=DataTypeName:NumberOfReps:{rep0,rep1,rep2}`
			//An example is `:{"Exemplar Type"}=Uint32:0:{0x00000002}`
			//If number of properties > 0 then rep list is comma separated, and for all but Float32 each rep is preceded with 0x
			//If data type if Float32, byte values are interpreted literally, e.g., {0x38, 0x31, 0x2E, 0x35} = {"8", "1", ".", "5"} -> 81.5

			//The first 85 bytes are features of the entry: ParentCohort TGI and property count. When examining a specific property in the entry we are not concerned about them.
			if (offset < 85) {
				offset = 85;
			}

			//Capture the Property ID
			offset += 2; //skip first "0x"
			uint propertyID = ByteArrayHelper.ReadTextIntoUint(dData, offset);
			offset += 8;

			//Capture the DataType
			//Skip over the property name `:{"Exemplar Type"}=` or `:{"Bulldoze Cost"}=` are examples
			offset = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.Equal, offset) + 1;
			int endPos = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.Colon, offset); //this represents the ending position (offset) of whatever we are looking for
			string type = ByteArrayHelper.ToAString(dData, offset, endPos - offset);
			offset = endPos + 1;
			DBPFPropertyDataType dataType = DBPFPropertyDataType.LookupDataType(type);

			//Create new decoded property then set dataType and id
			DBPFProperty newProperty;
			if (dataType.Name == "STRING") {
				newProperty = new DBPFPropertyString();
			} else {
				newProperty = new DBPFPropertyNumber(dataType);
			}
			newProperty.ID = propertyID;

			//Determine number of reps; see note on the field at the top for what this actually means
			endPos = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.Colon, offset);
			int countOfReps = ByteArrayHelper.ReadTextIntoANumber(dData, offset, endPos - offset);
			int countOfValues; //Problem if countOfReps = 0, then the loop below will not execute. If one value, the loop should run just once. Be careful with the difference between the "number of values" and "number of repetitions" difference.
			if (countOfReps == 0) {
				countOfValues = 1;
			} else {
				countOfValues = countOfReps;
			}

			//Parse the text values into a byte array and set the property values equal to the array. Algorithm differs depending on if the data type is float, string, or other number.
			offset = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.OpeningBrace, offset) + 1;
			if (newProperty.DataType == DBPFPropertyDataType.FLOAT32) {
				float[] newVals = new float[countOfReps];
				if (countOfReps == 1) {
					endPos = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.ClosingBrace, offset);
					float value = (float) ByteArrayHelper.ReadTextIntoType(dData, newProperty.DataType.PrimitiveDataType, offset, endPos - offset);
					newVals[0] = value;
					newProperty.NumberOfReps = (uint) countOfReps;
					newProperty.ByteValues = ByteArrayHelper.ToByteArray(newVals);
					//newProperty.SetValues(ByteArrayHelper.ToByteArray(newVals));
				} else {
					for (int rep = 0; rep < countOfReps; rep++) {
						int endRepPos;
						if (rep != countOfReps - 1) {//get all except last rep (aka reps appended by a comma)
							endRepPos = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.Comma, offset);
						} else { //get the last rep in the list (rep appended by a closing bracket
							endRepPos = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.ClosingBrace, offset);
						}

						float value = (float) ByteArrayHelper.ReadTextIntoType(dData, newProperty.DataType.PrimitiveDataType, offset, endRepPos - offset);
						newVals[rep] = value;
						offset = endRepPos + 1;
					}
					newProperty.NumberOfReps = (uint) countOfReps;
					newProperty.ByteValues = ByteArrayHelper.ToByteArray(newVals);
					//newProperty.SetValues(ByteArrayHelper.ToByteArray(newVals));
				}
			} else if (newProperty.DataType == DBPFPropertyDataType.STRING) {
				//strings are encoded with quotes, so we start one position after and end one position sooner to avoid incorporating them into the decoded string
				endPos = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.ClosingBrace, offset) - 2;
				//string result = ByteArrayHelper.ToAString(dData, offset, endPos - offset);
				byte[] newVals = new byte[endPos - offset];
				Array.Copy(dData, offset + 1, newVals, 0, endPos - offset);
				newProperty.NumberOfReps = 1;
				newProperty.ByteValues = newVals;
				//newProperty.SetValues(result2);
			} else {
				Array newVals = Array.CreateInstance(newProperty.DataType.PrimitiveDataType, countOfValues);
				for (int rep = 0; rep < countOfValues; rep++) {
					offset += 2; //skip "0x"
					var result = ByteArrayHelper.ReadTextIntoType(dData, newProperty.DataType.PrimitiveDataType, offset, (newProperty.DataType.Length) * 2);
					switch (newProperty.DataType.Name) {
						case "SINT32":
							newVals.SetValue((int) result, rep);
							break;
						case "UINT32":
							newVals.SetValue((uint) result, rep);
							break;
						case "BOOL":
							newVals.SetValue((bool) result, rep);
							break;
						case "UINT8":
							newVals.SetValue((byte) result, rep);
							break;
						case "SINT64":
							newVals.SetValue((long) result, rep);
							break;
						case "UINT16":
							newVals.SetValue((ushort) result, rep);
							break;
						default:
							break;
					}
					offset += (newProperty.DataType.Length) * 2 + 1;
				}
				newProperty.ByteValues = ByteArrayHelper.ToByteArray(newVals);
				newProperty.NumberOfReps = (uint) countOfReps;
			}
			return newProperty;
		}



		/// <summary>
		/// Decodes the property from raw data at the given offset.
		/// </summary>
		/// <param name="dData">Decompressed raw data</param>
		/// <param name="offset">Offset to start decoding from</param>
		/// <returns>The DBPFProperty; null if cannot be decoded</returns>
		public DBPFProperty DecodeCohortProperty(byte[] dData, int offset = 0) {
			throw new NotImplementedException();
			//TODO - Implement DecodeCohortProperty? ... what was this supposed to do?
		}



		/// <summary>
		/// Returns the encoding type of this entry.
		/// </summary>
		/// <returns>TRUE if binary encoding; FALSE otherwise</returns>
		/// <remarks>This will decompress the data if it is not already uncompressed.</remarks>
		public bool IsBinaryEncoding() {
			if (IsCompressedNow) {
				ByteData = DBPFCompression.Decompress(ByteData);
				IsCompressedNow = false;
			}

			string fileIdentifier = ByteArrayHelper.ToAString(ByteData, 0, 4);
			return fileIdentifier.Substring(fileIdentifier.Length - 1) == "B";
		}



		/// <summary>
		/// Decode all properties in the entry. Only valid for Exemplar/Cohort type entries.
		/// </summary>
		public void DecodeAllProperties() {
			if (!(TGI.MatchesKnownTGI(DBPFTGI.COHORT) || TGI.MatchesKnownTGI(DBPFTGI.EXEMPLAR))) {
				throw new InvalidOperationException("This function can only be called on Exemplar and Cohort type entries!");
			}

			foreach (DBPFProperty property in _listOfProperties) {
				property.DecodeValues();
			}
		}



		/// <summary>
		/// Gets the Exemplar Type (0x00 - 0x2B) of the property. See <see cref="DBPFProperty.ExemplarTypes"/> for the full list.
		/// </summary>
		/// <returns>Exemplar Type if found; -1 if entry is not Exemplar or Cohort; -2 if "ExemplarType" property is not found</returns>
		public int GetExemplarType() {
			if (!(TGI.MatchesKnownTGI(DBPFTGI.EXEMPLAR) || TGI.MatchesKnownTGI(DBPFTGI.COHORT))) {
				return -1;
			}
			DBPFProperty property = GetProperty(0x00000010);

			if (property is null) {
				return -2;
			}

			//TODO - WTF IS THIS SYNTAX? GROSS.
			Array propertyType = Array.CreateInstance(property.DataType.PrimitiveDataType, property.NumberOfReps); //Create new array to hold the values
			property.DecodeValues();
			propertyType = property.DecodedValues; //Set the values from the decoded property
												   //return unchecked((int) propertyType.GetValue(0)); 
												   //TODO We know exemplar type can only hold one value, so grab the first one.... BUT HOW?????????
			return Convert.ToInt32(propertyType.GetValue(0));
		}



		/// <summary>
		/// Lookup and return a property from a list of properties in the entry.
		/// </summary>
		/// <param name="idToGet">Property ID to find</param>
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

		//TODO - Implement AddProperty
		//Add a property to this exemplar/cohort. This method returns false if the property is null or if another property with the same id already exists.
		public bool AddProperty(DBPFProperty prop) {
			throw new NotImplementedException();
		}

		//TODO - Implement UpdateProperty
		//Update a property to this exemplar/cohort. This method returns false if the property is null or if another property with the same id does not already exists.
		public bool UpdateProperty(DBPFProperty prop) {
			throw new NotImplementedException();
		}

		//TODO - Implement AddOrUpdateProperty
		//Add or Update a property to this exemplar/cohort. This method returns false if the property is null.
		public bool AddOrUpdateProperty(DBPFProperty prop) {
			throw new NotImplementedException();
		}

		//TODO - Implement RemoveProperty
		//Remove a property to this exemplar/cohort. This method returns false if the property is null.
		public bool RemoveProperty(uint id) {
			throw new NotImplementedException();
		}

		//TODO - Implement RemoveAllProperties
		//Removes all the properties from this cohort/exemplar.
		public bool RemoveAllProperties() {
			throw new NotImplementedException();
		}

	}
}
