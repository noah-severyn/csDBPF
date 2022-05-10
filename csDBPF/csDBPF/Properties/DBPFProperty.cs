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
		private const string EQZB1 = "EQZB1###";
		private const string EQZT1 = "EQZT1###";
		private const string CQZB1 = "CQZB1###";
		private const string CQZT1 = "CQZT1###";
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
		private enum TextSeparators : byte {
			Colon = 0x3A, // : = 0x3A
			Comma = 0x2C, // , 0x2C
			OpeningBrace = 0x7B, // { = 0x7B
			ClosingBrace = 0x7D // } = 0x7D
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
		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.Append($"ID: {DBPFUtil.UIntToHexString(_id)}, ");
			sb.Append($"Type: {_dataType}, ");
			sb.Append($"Key: {_keyType}, ");
			sb.Append($"Reps: {_numberOfReps}, ");
			sb.Append("Values: ");
			return sb.ToString();
		}


		/// <summary>
		/// Decodes the property from raw data at the given offset.
		/// </summary>
		/// <param name="dData">Decompressed raw data</param>
		/// <param name="offset">Offset to start decoding from</param>
		/// <returns>The DBPFProperty; null if cannot be decoded</returns>
		/// <see cref="https://www.wiki.sc4devotion.com/index.php?title=EXMP"/>
		public static DBPFProperty DecodeExemplarProperty_Binary(byte[] dData, int offset = 24) {
			ValidateData(dData, 1);

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
			//for text encoding, first 8 bytes are the fileIdentifier as usual. each property is separated by the code 0x0D0A. Followed by the text name of the property.
			//for parent cohort, looks like "ParentCohort=Key:{0x00000000,0x00000000,0x00000000} ... then 0D0A
			//then number of properties, looks like PropCount=0x00000018 ... then 0D0A
			//then property hex id then :{"Exemplar Type"}=Uint32:0:{0x00000002} ... thats id:{"Exemplar Type"}=DataType:NumberOfReps:{vals}
			//if number of properties > 0 then each value is delineated with a comma
			//if data type if float32, byte values are interpreted literally, e.g., [0x38, 0x31, 0x2E, 0x35] = ["8", "1", ".", "5"] -> 81.5
			ValidateData(dData, 2);

			//The first 85 bytes are features of the entry: ParentCohort TGI and property count. When examining a specific property in the entry we are not concerned about them.
			if (offset < 85) {
				offset = 85;
			}

			offset += 2; //skip first "0x"
			uint propertyID = ByteArrayHelper.ReadTextIntoUint(dData, offset);
			offset += 8;

			//Capture the DataType
			offset += 19; //Skip over :{"Exemplar Type"}=
			int endPos = ByteArrayHelper.FindNextInstanceOf(dData, (byte) TextSeparators.Colon, offset);
			string type = ByteArrayHelper.ToAString(dData, offset, endPos-offset);
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

			//Determine number of reps
			int countOfReps =  ByteArrayHelper.ReadTextIntoByte(dData, offset)+1;
			offset++;

			//Parse the values into a byte array
			offset = ByteArrayHelper.FindNextInstanceOf(dData, (byte) TextSeparators.OpeningBrace, offset)+1;

			if (newProperty.DataType == DBPFPropertyDataType.FLOAT32) {
				////Build a string from the byte values -- in this case, the byte values are literal, e.g., [0x38, 0x31, 0x2E, 0x35] = ["8", "1", ".", "5"] -> 81.5
				//StringBuilder sb = new StringBuilder();
				//float[] newVals = new float[countOfReps];

				//if (countOfReps == 1) {
				//	endPos = ByteArrayHelper.FindNextInstanceOf(dData, (byte) TextSeparators.ClosingBrace, offset);
				//	for (int idx = offset; idx < endPos-offset; idx++) {
				//		sb.Append(BitConverter.ToChar(dData, idx));
				//	}
				//	float.TryParse(sb.ToString(), out float value);
				//	newVals[0] = value;
				//	newProperty.SetValues(ByteArrayHelper.ToByteArray(newVals));

				//} else {
				//	endPos = ByteArrayHelper.FindNextInstanceOf(dData, (byte) TextSeparators.ClosingBrace, offset);

				//	//loop over the number of reps
				//	for (int rep = 0; rep < countOfReps; rep++) {

				//		//loop over the bytes for each rep
				//		int endRepPos = ByteArrayHelper.FindNextInstanceOf(dData, (byte) TextSeparators.Comma, offset);
				//		for (int idx = offset; idx < endRepPos - offset; idx++) {
				//			sb.Append(BitConverter.ToChar(dData, idx));
				//		}
				//		float.TryParse(sb.ToString(), out float value);
				//		newVals[rep] = value;
				//	}
				//	newProperty.SetValues(ByteArrayHelper.ToByteArray(newVals));
				//}

				
			} else {
				
				Array newVals = Array.CreateInstance(newProperty.DataType.PrimitiveDataType, countOfReps+1);

				for (int rep = 0; rep < countOfReps; rep++) {
					offset += 2; //skip "0x"
					var result = ByteArrayHelper.ReadTextIntoType(dData, newProperty.DataType.PrimitiveDataType, offset);
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
						case "STRING":
							newVals.SetValue((string) result, rep); //this should just be one string not an array
							break;
						default:
							break;
					}
					offset += (newProperty.DataType.Length)*2+1;
					
				}
				newProperty.SetValues(ByteArrayHelper.ToByteArray(newVals)); //add unit tests to continue to validate this, especially with additional data types

			}
			//int pos = 0;






			//for (int idx = offset; idx < countOfReps; idx++) {


			//	Array.Copy(dData, offset, newProperty.ByteValues, pos, newProperty.DataType.Length);

			//	pos += newProperty.DataType.Length;
			//	if (newProperty.DataType != DBPFPropertyDataType.FLOAT32) {
			//		offset += newProperty.DataType.Length + 2; //skip the number and the additional "0x"
			//	} else {
			//		offset += newProperty.DataType.Length; //skip the number only
			//	}

			//}

			//As the number of reps is set automatically, all we need to do is set the value

			//endPos = ByteArrayHelper.FindNextInstanceOf(dData, (byte) TextSeparators.ClosingBrace, startPos)-1;
			//Array.Copy(dData, startPos, newProperty.ByteValues, 0, endPos - startPos);

			return newProperty;
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
		/// Check if data is compressed and if fileIdentifier is valid. Throws ArgumentException if either is not true.
		/// </summary>
		/// <param name="dData">Byte data</param>
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
