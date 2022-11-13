using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Collections;

namespace csDBPF.Properties {
	/// <summary>
	/// An abstract class defining the structure of a Property and the methods for interfacing with it. This class is only relevant for Exemplar and Cohort type entries. The data for the property is not parsed or decoded until <see cref="DecodeValues"/> is called to set the actual entry data.
	/// </summary>
	public abstract partial class DBPFProperty {
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


		//------------- DBPFProperty Fields + Abstract Methods ------------- \\
		/// <summary>
		/// Hexadecimal identifier for this property. <see cref="XMLExemplarProperty"/> and <see cref="XMLProperties.AllProperties"/>. 
		/// </summary>
		public abstract uint ID { get; set; }

		/// <summary>
		/// The <see cref="DBPFPropertyDataType"/> for this property.
		/// </summary>
		public abstract DBPFPropertyDataType DataType { get; set; }

		/// <summary>
		/// The KeyType contains a value of 0x80 if the property has more than or equal to one repetition, and 0x00 if it has 0 repetitions. 0x80 is the only recorded KeyType
		/// </summary>
		public abstract ushort KeyType { get; set; }

		/// <summary>
		/// The number of repetitions of <see cref="DBPFPropertyDataType"/> this property has. This informs (in part) how many bytes to read for this property. Initialized to 0.
		/// </summary>
		/// <remarks>
		/// Determining the count partially depends on the encoding type. For binary encoded string type: length of string. For text encoded string type: always 1. For binary encoded (all) and text encoded number types (except float): 0 reps = single value, 1 reps = multiple values but currently held to 1 value (problematic on macOS when the DataType is float), n reps = n number of values. For text encoded float type: n reps = n number of values.
		/// </remarks>
		public abstract uint NumberOfReps { get; set; }

		/// <summary>
		/// This is a byte array of the raw values in the property. Assignment of this value takes place in <see cref="DBPFPropertyString"/> or <see cref="DBPFPropertyNumber"/>.
		/// </summary>
		public abstract byte[] ByteValues { get; set; }

		/// <summary>
		/// This Array of type <see cref="DataType"/> holds the decoded values for this property. It is only set after <see cref="DecodeValues"/> is called on the member.
		/// </summary>
		/// <remarks>
		/// For <see cref="DBPFPropertyString"/> this will always be an array of length 1 with the only value equal to the string value. For <see cref="DBPFPropertyNumber"/> this can be an array of length 1 to <see cref="NumberOfReps"/>.
		/// </remarks>
		/// <example>
		/// To use,
		/// <code>
		/// Array values = Array.CreateInstance(property.DataType.PrimitiveDataType, property.NumberOfReps);
		/// values = property.DecodedValues;
		/// </code>
		/// </example>
		public abstract Array DecodedValues { get; set; }

		/// <summary>
		/// Parse the byte values for this property to set <see cref="DecodedValues"/>.
		/// </summary>
		public abstract void DecodeValues();

		/// <summary>
		/// Sets the value field to the provided byte array. Also sets numberOfReps to the appropriate value.
		/// </summary>
		/// <param name="newValue">Byte array if <see cref="DBPFPropertyNumber"/> or string if <see cref="DBPFPropertyString"/>.</param>
		//public abstract void SetValues(byte[] newValue);




		//------------- DBPFProperty Constructors ------------- \\
		///// <summary>
		///// Create a new DBPFProperty by attempting a lookup against <see cref="XMLProperties.AllProperties"/> and initializing fields as appropriate. If no match is found, fields are initialized to null or 0, respectively.
		///// </summary>
		///// <param name="id">Property identifier, used as the lookup</param>
		//public DBPFProperty(uint id) {
		//	_id = id;

		//	//Try to find a matching property from the XML file to initialize the fields; otherwise set to 0
		//	bool lookupFound = XMLProperties.AllProperties.TryGetValue(id, out XMLExemplarProperty xmlProperty);
		//	if (lookupFound) {
		//		_dataType = xmlProperty.Type;
		//		_numberOfReps = 0;
		//	} else {
		//		_dataType = null;
		//		_numberOfReps = 0;
		//	}
		//}




		//------------- DBPFProperty Methods ------------- \\



		/// <summary>
		/// Decodes either Binary or Text encoded data, and returns the property.
		/// </summary>
		/// <param name="dData">Byte array of decompressed data</param>
		/// <param name="offset">Offset (location) to start reading from</param>
		/// <returns>A <see cref="DBPFProperty"/></returns>
		public static DBPFProperty DecodeProperty(byte[] dData, int offset = 0) {
			switch (DBPFEntry.GetEncodingType(dData)) {
				case 1: //Binary encoding
					return DecodeProperty_Binary(dData, offset);
				case 2: //Text encoding
					return DecodeProperty_Text(dData, offset);
				default:
					return null;
			}
		}



		/// <summary>
		/// Decodes the property from raw binary data at the given offset.
		/// </summary>
		/// <param name="dData">Decompressed binary data</param>
		/// <param name="offset">Offset to start decoding from</param>
		/// <returns>The DBPFProperty; null if it cannot be decoded</returns>
		/// <see cref="https://www.wiki.sc4devotion.com/index.php?title=EXMP"/>
		private static DBPFProperty DecodeProperty_Binary(byte[] dData, int offset = 24) {
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
		/// Decodes the property from raw text data at the given offset.
		/// </summary>
		/// <param name="dData">Decompressed text data</param>
		/// <param name="offset">Offset to start decoding from</param>
		/// <returns>The DBPFProperty; null if cannot be decoded</returns>
		private static DBPFProperty DecodeProperty_Text(byte[] dData, int offset = 85) {
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
			offset = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.Equal, offset)+1;
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
				} 
				else {
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
			} 
			
			else if (newProperty.DataType == DBPFPropertyDataType.STRING) {
				//strings are encoded with quotes, so we start one position after and end one position sooner to avoid incorporating them into the decoded string
				endPos = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.ClosingBrace, offset)-2;
				//string result = ByteArrayHelper.ToAString(dData, offset, endPos - offset);
				byte[] newVals = new byte[endPos - offset];
				Array.Copy(dData, offset+1, newVals, 0, endPos - offset);
				newProperty.NumberOfReps = 1;
				newProperty.ByteValues = newVals;
				//newProperty.SetValues(result2);
			}

			else {
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
				//newProperty.SetValues(ByteArrayHelper.ToByteArray(newVals)); //add unit tests to continue to validate this, especially with additional data types
			}
			return newProperty;
		}



		/// <summary>
		/// Decodes the property from raw data at the given offset.
		/// </summary>
		/// <param name="dData">Decompressed raw data</param>
		/// <param name="offset">Offset to start decoding from</param>
		/// <returns>The DBPFProperty; null if cannot be decoded</returns>
		public static DBPFProperty DecodeCohortProperty(byte[] dData, int offset = 0) {
			//return DecodeExemplarProperty(dData, offset);
			throw new NotImplementedException();
		}
	}
}
