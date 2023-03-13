using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using csDBPF.Properties;

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
		/// <summary>
		/// Stores if this entry is encoded as binary or text.
		/// </summary>
		private bool _isTextEncoding;

		private SortedList<uint, DBPFProperty> _listOfProperties;
		/// <summary>
		/// List of one or more <see cref="DBPFProperty"/> associated with this entry. Sorted by <see cref="DBPFProperty.ID"/>.
		/// </summary>
		public SortedList<uint, DBPFProperty> ListOfProperties {
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

		private bool _isCohort;
		/// <summary>
		/// Determine if the file is Exemplar or Cohort.
		/// </summary>
		public bool IsCohort {
			get { return _isCohort; }
			set { _isCohort = value; }
		}




		/// <summary>
		/// Create a new instance. Use when creating new exemplars.
		/// </summary>
		/// <param name="tgi">TGI set to assign</param>
		public DBPFEntryEXMP(DBPFTGI tgi) : base(tgi) {
			if (tgi is null) {
				TGI.SetTGI(DBPFTGI.EXEMPLAR);
			}
			_listOfProperties = new SortedList<uint, DBPFProperty>();
			_parentCohort = new DBPFTGI(0, 0, 0);
		}

		//TODO add constructor for only listofprop, tgi+listofprop, tgi+parent, tgi+listofprop+parent

		/// <summary>
		/// Create a new instance. Use when reading existing exemplars from a file.
		/// </summary>
		/// <param name="tgi"><see cref="DBPFTGI"/> object representing the entry</param>
		/// <param name="offset">Offset (location) of the entry within the DBPF file</param>
		/// <param name="size">Compressed size of data for the entry, in bytes. Uncompressed size is also temporarily set to this to this until the data is set</param>
		/// <param name="index">Entry position in the file, 0-n</param>
		/// <param name="bytes">Byte data for this entry</param>
		public DBPFEntryEXMP(DBPFTGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {
			_listOfProperties = new SortedList<uint, DBPFProperty>();
			_parentCohort = new DBPFTGI(0,0,0);

			if (bytes[0] == 0x43) { //"C"
				_isCohort = true;
			}
		}


		/// <summary>
		/// Uncompresses the exemplar/cohort instance and sets <see cref="ListOfProperties"/> as one or more <see cref="DBPFProperty"/> from a byte sequence.
		/// </summary>
		/// <remarks>
		/// Use when reading from a file.
		/// </remarks>
		public override void DecodeEntry() {
			if (_isDecoded) return;
			_isTextEncoding = IsTextEncoding();

			byte[] dData;
			if (DBPFCompression.IsCompressed(ByteData)) {
				dData = DBPFCompression.Decompress(ByteData);
			} else {
				dData = ByteData;
			}

			//Read cohort TGI info and determine the number of properties in this entry
			uint parentCohortTID;
			uint parentCohortGID;
			uint parentCohortIID;
			uint propertyCount;
			int pos; //Offset position in dData. Initialized to the starting position of the properties after the header data
			if (!_isTextEncoding) {
				parentCohortTID = BitConverter.ToUInt32(dData, 8);
				parentCohortGID = BitConverter.ToUInt32(dData, 12);
				parentCohortIID = BitConverter.ToUInt32(dData, 16);
				_parentCohort.SetTGI(parentCohortTID, parentCohortGID, parentCohortIID);
				propertyCount = BitConverter.ToUInt32(dData, 20);
				pos = 24;
			} else {
				parentCohortTID = ByteArrayHelper.ReadTextToUint(dData, 30);
				parentCohortGID = ByteArrayHelper.ReadTextToUint(dData, 41);
				parentCohortIID = ByteArrayHelper.ReadTextToUint(dData, 52);
				_parentCohort.SetTGI(parentCohortTID, parentCohortGID, parentCohortIID);
				propertyCount = ByteArrayHelper.ReadTextToUint(dData, 75);
				pos = 85;
			}

			if (propertyCount == 0) {
				LogMessage("Entry contains 0 properties.");
				return;
			}

			//Create the Property
			DBPFProperty property;
			for (int idx = 0; idx < propertyCount; idx++) {
				property = DecodeProperty(pos);
				if (property is null) {
					LogMessage($"Property #{idx} could not be decoded.");
					return;
				}

				//Can be an error if this property has duplicate entries which some files do have - skip any subsequent properties with same ID
				try {
                    _listOfProperties.Add(property.ID, property);
                } catch {
					LogMessage($"Property 0x{DBPFUtil.ToHexString(property.ID)} is duplicated.");
				}

				//Determine which bytes to skip to get to the start of the next property
				if (!_isTextEncoding) {
					if (property.NumberOfReps == 0) {
						pos += (property.DataType.Length * (property.NumberOfReps+1)) + 9; //Additionally skip the 4 bytes for ID, 2 for DataType, 2 for KeyType, 1 unused byte
					} else {
						pos += (property.DataType.Length * (property.NumberOfReps)) + 9; //Additionally skip the 4 bytes for ID, 2 for DataType, 2 for KeyType, 1 unused byte
					}
					
					if (property.NumberOfReps > 0) { //Skip 4 more for NumberOfValues
						pos += 4;
					}
				} else {
					pos = ByteArrayHelper.FindNextInstanceOf(dData, 0x0A, pos) + 1;
				}
			}

			//Lastly check to make sure we have ExemplarType (0x10) and ExemplarName (0x20) properties
			if (GetExemplarType() == -1) {
				LogMessage("Missing property Exemplar Type.");
			}
            if (GetExemplarName() == null) {
                LogMessage("Missing property Exemplar Name.");
            }
        }



		/// <summary>
		/// Decodes either Binary or Text encoded data, and returns the property.
		/// </summary>
		/// <param name="offset">Offset (location) to start reading from</param>
		/// <returns>A <see cref="DBPFProperty"/></returns>
		private DBPFProperty DecodeProperty(int offset = 0) {
			if (!_isTextEncoding) {
				return DecodeProperty_Binary(offset);
			} else {
				return DecodeProperty_Text(offset);
			}
		}
		/// <summary>
		/// Decodes the property from raw binary data at the given offset.
		/// </summary>
		/// <param name="offset">Offset to start decoding from</param>
		/// <returns>The DBPFProperty; null if it cannot be decoded</returns>
		private DBPFProperty DecodeProperty_Binary(int offset = 24) {
			byte[] dData = ByteData;

			//Check for exemplars with no properties. Later checks against dData.Length to account for extraneous bytes on the end of dData that do not correspond to valid properties of the property
			if (dData.Length <= 24) return null;

			//The first 24 bytes are features of the entry: ParentCohort TGI and property count. When examining a specific property in the entry we are not concerned about them.
			if (offset < 24) {
				offset = 24;
			}

			//Get the property ID
			if (offset + 4 > dData.Length) {
                LogMessage($"Offset of {offset} does not contain enough data to hold a property. Unable to decode property.");
                return null; 
			}
			uint propertyID = BitConverter.ToUInt32(dData, offset);
			offset += 4;

			//Get the data value type
			ushort valueType = BitConverter.ToUInt16(dData, offset);
			DBPFPropertyDataType dataType = DBPFPropertyDataType.LookupDataType(valueType);
			if (dataType is null) {
				LogMessage($"Property 0x{DBPFUtil.ToHexString(propertyID)} has invalid data type. Unable to decode property.");
                return null;
            }
			offset += 2;

			//Get the property keyType
			if (offset + 2 > dData.Length) {
                LogMessage($"Property 0x{DBPFUtil.ToHexString(propertyID)} has invalid key type. Unable to decode property.");
                return null; 
			}
			ushort keyType = BitConverter.ToUInt16(dData, offset);
			offset += 2;

			//Examine the keyType to determine how to set the values for the new property
			object dataValues;
			uint countOfReps;
			//keyType == 0x80 ... this is one or more repetitions of the data type (2+ values of the data type)
			if (keyType == 0x80) {
				offset += 1; //There is a 1 byte unused flag
				countOfReps = BitConverter.ToUInt32(dData, offset);
				offset += 4;
				if (dataType == DBPFPropertyDataType.STRING) {
					dataValues = ByteArrayHelper.ToAString(dData, offset, (int) countOfReps);
				}
				else if (dataType == DBPFPropertyDataType.FLOAT32) {
					dataValues = new List<float>();
					for (int idx = 0; idx < countOfReps; idx++) {
						((List<float>) dataValues).Add(BitConverter.ToSingle(dData,offset));
						offset += 4;
					}
				} 
				else {
					dataValues = new List<long>();
					byte[] oneVal = new byte[8];
					for (int idx = 0; idx < countOfReps; idx++) {
						Array.Copy(dData, offset, oneVal, 0, dataType.Length);
						((List<long>) dataValues).Add(BitConverter.ToInt64(oneVal));
						offset += dataType.Length;
					}
				}
			}

			//keyType == 0x00 ... this is 0 repetitions (just a single value of the data type)
			else {
				countOfReps = 0;
				offset += 1; //This one byte is number of value repetitions; seems to always be 0
				byte[] byteVals = new byte[8];
				Array.Copy(dData, offset, byteVals, 0, dataType.Length);
				if (dataType == DBPFPropertyDataType.STRING) {
					dataValues = ByteArrayHelper.ToAString(dData, offset, 1);
				} else if (dataType == DBPFPropertyDataType.FLOAT32) {
					dataValues = new List<float> { BitConverter.ToSingle(byteVals) };
				} else {
					dataValues = new List<long> { BitConverter.ToInt64(byteVals) };
                }
				
			}

			//Create new decoded property then set ID and DataValues
			DBPFProperty newProperty;
			if (dataType == DBPFPropertyDataType.STRING) {
				newProperty = new DBPFPropertyString();
			} else if (dataType == DBPFPropertyDataType.FLOAT32) {
				if (countOfReps == 1 && ((List<float>) dataValues).Count == 1) {
					LogMessage($"Property 0x{DBPFUtil.ToHexString(propertyID)} contains a potential macOS TE bug.");
                }
                newProperty = new DBPFPropertyFloat();
            } else {
				newProperty = new DBPFPropertyLong(dataType);
            }
			newProperty.ID = propertyID;
			newProperty.IsTextEncoding = EncodingType.Binary;
            newProperty.SetData(dataValues, countOfReps);
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
		/// <param name="offset">Offset to start decoding from</param>
		/// <returns>The DBPFProperty; null if cannot be decoded</returns>
		private DBPFProperty DecodeProperty_Text(int offset = 85) {
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
			byte[] dData = ByteData;
			if (offset < 85) {
				offset = 85;
			}

			//Capture the Property ID
			offset += 2; //skip first "0x"
			uint propertyID = ByteArrayHelper.ReadTextToUint(dData, offset);
			offset += 8;

			//Capture the DataType
			//Skip over the property name `:{"Exemplar Type"}=` or `:{"Bulldoze Cost"}=` are examples
			offset = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.Equal, offset) + 1;
			int endPos = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.Colon, offset); //this represents the ending position (offset) of whatever we are looking for
			string type = ByteArrayHelper.ToAString(dData, offset, endPos - offset);
			offset = endPos + 1;
			DBPFPropertyDataType dataType = DBPFPropertyDataType.LookupDataType(type);

			//Determine number of reps; Problem if countOfReps = 0, then the loop below will not execute. If one value, the loop should run just once. Be careful with the difference between the "number of values" and "number of repetitions".
			endPos = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.Colon, offset);
			int countOfReps = ByteArrayHelper.ReadTextToInt(dData, offset, endPos - offset);
			int countOfValues;
			if (countOfReps == 0) {
				countOfValues = 1;
			} else {
				countOfValues = countOfReps;
			}

			//Parse the text values into a byte array and set the property values equal to the array. Algorithm differs depending on if the data type is float, string, or other number.
			offset = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.OpeningBrace, offset) + 1;
			object dataValues;

			if (dataType == DBPFPropertyDataType.FLOAT32) {
				dataValues = new List<float>();
				float value;

				if (countOfReps == 1) {
					endPos = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.ClosingBrace, offset);
					value = ByteArrayHelper.ReadTextToFloat(dData, offset, endPos - offset);
					((List<float>) dataValues).Add(value);
				} 
				else {
					for (int rep = 0; rep < countOfReps; rep++) {
						int endRepPos;
						if (rep != countOfReps - 1) {//get all except last rep (aka reps appended by a comma)
							endRepPos = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.Comma, offset);
						} else { //get the last rep in the list (rep appended by a closing bracket
							endRepPos = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.ClosingBrace, offset);
						}

						//Precision of floats is ~6-9 digits so this number may be rounded or truncated
						value = ByteArrayHelper.ReadTextToFloat(dData, offset, endRepPos - offset);
						((List<float>) dataValues).Add(value);
						offset = endRepPos + 1;
					}
				}
			} 
			
			else if (dataType == DBPFPropertyDataType.STRING) {
				//strings are encoded with quotes, so we start one position after and end one position sooner to avoid incorporating them into the decoded string
				endPos = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.ClosingBrace, offset) - 2;
				string result = ByteArrayHelper.ToAString(dData, offset+1, endPos - offset);
				dataValues = result;
			} 
			
			else {
				dataValues = new List<long>();
				for (int rep = 0; rep < countOfValues; rep++) {
					offset += 2; //skip "0x"
					long result = ByteArrayHelper.ReadTextToLong(dData, offset, dataType.Length * 2);
					((List<long>) dataValues).Add(result);
					offset += (dataType.Length * 2) + 1; //skip comma
				}
			}

			//Create new decoded property then set ID and DataValues
			DBPFProperty newProperty;
			if (dataType == DBPFPropertyDataType.STRING) {
				newProperty = new DBPFPropertyString();
			} else if (dataType == DBPFPropertyDataType.FLOAT32) {
                if (countOfReps == 1 && ((List<float>) dataValues).Count == 1) {
                    LogMessage($"Property 0x{DBPFUtil.ToHexString(propertyID)} contains a potential macOS TE bug.");
                }
                newProperty = new DBPFPropertyFloat();
			} else {
				newProperty = new DBPFPropertyLong(dataType);
			}
			newProperty.ID = propertyID;
			newProperty.IsTextEncoding = EncodingType.Text;
			newProperty.SetData(dataValues);
			return newProperty;
		}



		/// <summary>
		/// Returns the encoding type of this entry.
		/// </summary>
		/// <returns>TRUE if text encoding; FALSE otherwise</returns>
		/// <remarks>This will decompress the data if it is not already uncompressed.</remarks>
		public bool IsTextEncoding() {
			if (IsCompressedNow) {
				ByteData = DBPFCompression.Decompress(ByteData);
				IsCompressedNow = false;
			}

			string fileIdentifier = ByteArrayHelper.ToAString(ByteData, 0, 4);
			return fileIdentifier.Substring(fileIdentifier.Length - 1) == "T";
		}



		/// <summary>
		/// Gets the Exemplar Type (0x00 - 0x2B) of the property. See <see cref="DBPFProperty.ExemplarTypes"/> for the full list.
		/// </summary>
		/// <returns>Exemplar Type if found; -1 if ExemplarType (0x00000010) property is not found</returns>
		public int GetExemplarType() {
			DBPFProperty property = GetProperty(0x00000010);
			if (property is null) {
				return -1;
			}

			//We know exemplar type can only hold one value, so grab the first one
			List<long> dataValues = (List<long>) property.GetData();
			return Convert.ToInt32(dataValues[0]);
		}



        /// <summary>
        /// Gets the Exemplar Name of the property.
        /// </summary>
        /// <returns>ExemplarName if found; null if ExemplarName (0x00000020) property is not found</returns>
        public string? GetExemplarName() {
            DBPFProperty property = GetProperty(0x00000020);
            if (property is null) {
                return null;
            }

            //We know exemplar type can only hold one value, so grab the first one
            string dataValue = (string) property.GetData();
            return dataValue;
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
			_listOfProperties.TryGetValue(idToGet, out DBPFProperty property);
			return property;
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
			uint id = XMLProperties.GetPropertyID(name);
			return GetProperty(id);
		}



		/// <summary>
		/// Add a property to this exemplar/cohort. If the property already exists no action is taken.
		/// </summary>
		/// <param name="prop">Property to add</param>
		public void AddProperty(DBPFProperty prop) {
			try {
				_listOfProperties.Add(prop.ID, prop);
			}
			catch (ArgumentException) {

			}
		}



		/// <summary>
		/// Update a property in this exemplar/cohort
		/// </summary>
		/// <param name="prop">Property to update</param>
		public void UpdateProperty(DBPFProperty prop) {
			try {
				_listOfProperties[prop.ID] = prop;
			}
			catch (KeyNotFoundException) {

			}

		}



		/// <summary>
		/// Update a property if it exists or add if it is not found.
		/// </summary>
		/// <param name="prop">Property to add or update</param>
		public void AddOrUpdateProperty(DBPFProperty prop) {
			try {
				_listOfProperties[prop.ID] = prop;
			}
			catch (KeyNotFoundException) {
				_listOfProperties.Add(prop.ID, prop);
			}
		}



		/// <summary>
		/// Remove a property to this exemplar/cohort. No action is taken if the property is not found.
		/// </summary>
		/// <param name="id">Property ID to remove</param>
		public void RemoveProperty(uint id) {
			try {
				_listOfProperties.Remove(id);
			}
			catch (KeyNotFoundException) {

			}
		}



		/// <summary>
		/// Removes all the properties from this cohort/exemplar.
		/// </summary>
		public void RemoveAllProperties() {
			_listOfProperties.Clear();
		}



		/// <summary>
		/// Build <see cref="DBPFEntry.ByteData"/> from the current state of this instance.
		/// </summary>
		public override void ToBytes() {
			//If not decoded then assumed no changes have been made to the entry → decompressed size and compressed size are unchanged
			if (!_isDecoded) return;

			string id;
			if (_isCohort) {
				id = "C";
			} else {
				id = "E";
			}

			//Text Encoding
			if (_isTextEncoding) {
				id += "QZT1###";

				StringBuilder sb = new StringBuilder();
				sb.Append(id+"\r\n");
				sb.Append($"ParentCohort=Key:{{0x{DBPFUtil.ToHexString(_parentCohort.TypeID.Value)},0x{DBPFUtil.ToHexString(_parentCohort.GroupID.Value)},0x{DBPFUtil.ToHexString(_parentCohort.InstanceID.Value)}}}\r\n");
				sb.Append($"PropCount=0x{DBPFUtil.ToHexString(_listOfProperties.Count)}\r\n");
				foreach (DBPFProperty prop in _listOfProperties.Values) {
					sb.Append(prop.ToRawBytes());
				}
				ByteData = ByteArrayHelper.ToBytes(sb.ToString(), true);
				UncompressedSize = (uint) ByteData.Length;
				IsCompressed = false;
			}

			//Binary Encoding
			else {
				id += "QZB1###";

				List<byte> bytes = new List<byte>();
				bytes.AddRange(ByteArrayHelper.ToBytes(id,true));
				bytes.AddRange(BitConverter.GetBytes(_parentCohort.TypeID.Value));
				bytes.AddRange(BitConverter.GetBytes(_parentCohort.GroupID.Value));
				bytes.AddRange(BitConverter.GetBytes(_parentCohort.InstanceID.Value));
				bytes.AddRange(BitConverter.GetBytes(_listOfProperties.Count));
				foreach (DBPFProperty prop in _listOfProperties.Values) {
					bytes.AddRange(prop.ToRawBytes());
				}
				ByteData = bytes.ToArray();
				UncompressedSize = (uint) ByteData.Length;
				IsCompressed= true;
			}
		}
	}
}
