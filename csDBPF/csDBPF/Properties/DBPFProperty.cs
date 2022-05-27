using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Collections;

namespace csDBPF.Properties {

	/// <summary>
	/// An abstract class defining the structure of a Property and the methods for interfacing with it. This class is only relevant for Exemplar and Cohort type entries.
	/// </summary>
	public abstract class DBPFProperty {
		
		public enum ExemplarTypes : uint {
			T00,
			Tuning,
			Building,
			RCI,
			Developer,
			Simulator,
			Road,
			Bridge,
			MiscNetwork,
			Unknown,
			Rail,
			Highway,
			PowerLine,
			Terrain,
			Ordinance,
			FloraFauna,
			LotConfiguration,
			Foundation,
			Lighting,
			LotRetianingWall,
			Vehicle,
			Pedestrian,
			Aircraft,
			Prop,
			Construction,
			AutomataTuning,
			NetworkLot,
			Disaster,
			DataView,
			Crime,
			Audio,
			GodMode,
			MayorMode,
			TrendBar,
			GraphControl
		}
		private enum SpecialChars : byte {
			Colon = 0x3A, // :
			Comma = 0x2C, // ,
			Equal = 0x3D, // =
			OpeningBrace = 0x7B, // {
			ClosingBrace = 0x7D, // }
			Quotation = 0x22 // "
		}



		//------------- DBPFProperty Fields + Abstract Methods ------------- \\
		private readonly uint _id;
		/// <summary>
		/// The Uint32 hex identifier for this property. See <see cref="ExemplarProperty"/>.
		/// </summary>
		public abstract uint ID { get; set; }

		private readonly uint _numberOfReps;
		/// <summary>
		/// The number of repetitions of <see cref="DBPFPropertyDataType"/> this property has. This informs (in part) how many bytes to read for this property. 
		/// </summary>
		public abstract uint NumberOfReps { get; }

		private readonly DBPFPropertyDataType _dataType;
		/// <summary>
		/// The <see cref="DBPFPropertyDataType"/> for this property.
		/// </summary>
		public abstract DBPFPropertyDataType DataType { get; set; }

		private readonly ushort _keyType;
		/// <summary>
		/// The KeyType contains a value of 0x80 if the property has more than or equal to one repetition, and 0x00 if it has 0 repetitions. 0x80 is the only recorded KeyType
		/// </summary>
		public abstract ushort KeyType { get; set; }

		/// <summary>
		/// This is a byte array of the raw values in the property. Assignment of this value takes place in <see cref="DBPFPropertyString"/> or <see cref="DBPFPropertyNumber"/>.
		/// </summary>
		public abstract byte[] ByteValues { get; }

		/// <summary>
		/// Parse the byte values for this property depending on the property's <see cref="DBPFPropertyDataType"/>.
		/// </summary>
		/// <remarks>
		/// This method can return an array of a variety of numerical data types, so use something like: <code>Array.CreateInstance(DecodeValues().GetType().GetElementType(), NumberOfReps)</code>
		/// </remarks>
		public abstract object DecodeValues();

		/// <summary>
		/// Sets the value field to the provided byte array. Also sets numberOfReps to the appropriate value.
		/// </summary>
		/// <param name="newValue">Byte array if <see cref="DBPFPropertyNumber"/> or string if <see cref="DBPFPropertyString"/>.</param>
		public abstract void SetValues(byte[] newValue);




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
		/// <summary>
		/// Create a new DBPFProperty with only a <see cref="DBPFPropertyDataType"/>. All other fields are initialized to 0.
		/// </summary>
		/// <param name="dataType"></param>
		public DBPFProperty(DBPFPropertyDataType dataType) {
			_dataType = dataType;
			_id = 0;
			_numberOfReps = 0;
		}




		//------------- DBPFProperty Methods ------------- \\
		//public override string ToString() {
		//	StringBuilder sb = new StringBuilder();

		//	return sb.ToString();
		//}


		/// <summary>
		/// Decodes the property from raw data at the given offset.
		/// </summary>
		/// <param name="dData">Decompressed raw data</param>
		/// <param name="offset">Offset to start decoding from</param>
		/// <returns>The DBPFProperty; null if cannot be decoded</returns>
		/// <see cref="https://www.wiki.sc4devotion.com/index.php?title=EXMP"/>
		public static DBPFProperty DecodeExemplarProperty_Binary(byte[] dData, int offset = 24) {
			//The first 24 bytes are features of the entry: ParentCohort TGI and property count. When examining a specific property in the entry we are not concerned about them.
			if (offset < 24) {
				offset = 24;
			}

			//Get the property's numeric value (0x0000 0000)
			uint propertyID = BitConverter.ToUInt32(dData, offset);
			offset += 4;

			//Get the data value type
			ushort valueType = BitConverter.ToUInt16(dData, offset);
			DBPFPropertyDataType dataType = DBPFPropertyDataType.LookupDataType(valueType);
			offset += 2;

			//Get the property keyType
			ushort keyType = BitConverter.ToUInt16(dData, offset);
			offset += 2;

			//Create new decoded property then set id and dataType
			DBPFProperty newProperty;
			if (dataType.Name == "STRING") {
				newProperty = new DBPFPropertyString(dataType);
			} else {
				newProperty = new DBPFPropertyNumber(dataType);
			}
			newProperty.ID = propertyID;
			newProperty.KeyType = keyType;

			//Examine the keyType to determine how to set the values for the new property
			if (keyType == 0x80) {
				offset += 1; //There is a 1 byte unused flag
				uint countOfReps = BitConverter.ToUInt32(dData, offset);
				offset += 4;
				byte[] newValue = new byte[countOfReps * newProperty.DataType.Length];
				for (int idx = 0; idx < newValue.Length; idx++) {
					newValue[idx] = dData[offset + idx];
				}
				newProperty.SetValues(newValue);
			}

			//keyType == 0x00 ... this is just a single value of the data type length
			else {
				offset += 1; //This one byte is number of value repetitions; seems to always be 0
				byte[] newVals = new byte[dataType.Length];
				for (int idx = 0; idx < dataType.Length; idx++) {
					newVals[idx] = dData[offset + idx];
				}
				newProperty.SetValues(newVals);
			}
			return newProperty;
		}


		public static DBPFProperty DecodeExemplarProperty_Text(byte[] dData, int offset = 85) {
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
			//Skip over the property name :{"Exemplar Type"}= or :{"Bulldoze Cost"}= are examples
			offset = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.Equal, offset)+1;
			int endPos = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.Colon, offset); //represents the ending position (offset) of whatever we are looking for
			string type = ByteArrayHelper.ToAString(dData, offset, endPos - offset);
			offset = endPos + 1;
			DBPFPropertyDataType dataType = DBPFPropertyDataType.LookupDataType(type);

			//Create new decoded property then set dataType and id
			DBPFProperty newProperty;
			if (dataType.Name == "STRING") {
				newProperty = new DBPFPropertyString(dataType);
			} else {
				newProperty = new DBPFPropertyNumber(dataType);
			}
			newProperty.ID = propertyID;

			//Determine number of reps; reps = number of repetitions = number of values + 1 (e.g. one value -> 0 reps; 4 values -> 3 reps)
			endPos = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.Colon, offset);
			int countOfReps = ByteArrayHelper.ReadTextIntoANumber(dData, offset, endPos - offset);

			//Parse the values into a byte array and set the property values equal to the array
			offset = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.OpeningBrace, offset) + 1;
			if (newProperty.DataType == DBPFPropertyDataType.FLOAT32) {
				float[] newVals = new float[countOfReps];

				if (countOfReps == 1) {
					endPos = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.ClosingBrace, offset);
					float value = (float) ByteArrayHelper.ReadTextIntoType(dData, newProperty.DataType.PrimitiveDataType, offset, endPos - offset);
					newVals[0] = value;
					newProperty.SetValues(ByteArrayHelper.ToByteArray(newVals));
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
					newProperty.SetValues(ByteArrayHelper.ToByteArray(newVals));
				}
			} 
			
			else if (newProperty.DataType == DBPFPropertyDataType.STRING) {
				//strings are encoded with quotes, so we start one position after and end one position sooner to avoid incorporating them into the decoded string
				endPos = ByteArrayHelper.FindNextInstanceOf(dData, (byte) SpecialChars.ClosingBrace, offset)-2;
				//string result = ByteArrayHelper.ToAString(dData, offset, endPos - offset);
				byte[] result2 = new byte[endPos - offset];
				Array.Copy(dData, offset+1, result2, 0, endPos - offset);
				newProperty.SetValues(result2);
			}

			else {
				Array newVals = Array.CreateInstance(newProperty.DataType.PrimitiveDataType, countOfReps + 1);

				for (int rep = 0; rep < countOfReps+1; rep++) {
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
				newProperty.SetValues(ByteArrayHelper.ToByteArray(newVals)); //add unit tests to continue to validate this, especially with additional data types
			}
			return newProperty;
		}



		/// <summary>
		/// Decodes either Binary or Text encoded data, and returns the property.
		/// </summary>
		/// <param name="dData">Byte array of decompressed data</param>
		/// <param name="offset">Offset (location) to start reading from</param>
		/// <returns>A <see cref="DBPFProperty"/></returns>
		public static DBPFProperty DecodeExemplarProperty(byte[] dData, int offset = 0) {
			switch (DBPFEntry.GetEncodingType(dData)) {
				case 1: //Binary encoding
					return DecodeExemplarProperty_Binary(dData, offset);
				case 2: //Text encoding
					return DecodeExemplarProperty_Text(dData, offset);
				default:
					return null;
			}
		}

		///// <summary>
		///// Gets the Exemplar Type (0x00 - 0x2B) of the property. See <see cref="https://www.wiki.sc4devotion.com/index.php?title=Exemplar"/>
		///// </summary>
		///// <param name="dData"></param>
		///// <returns></returns>
		//public static uint GetExemplarType(byte[] dData) {
		//	ValidateData(dData,1);
		//	return BitConverter.ToUInt32(dData, 33);
		//}


		


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
