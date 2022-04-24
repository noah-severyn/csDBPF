using System;
using System.Text;
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

		/// <summary>
		/// The Uint32 hex identifier for this property. See <see cref="ExemplarProperty"/>.
		/// </summary>
		private readonly uint _id;
		public abstract uint ID { get; set; }

		/// <summary>
		/// The number of repetitions of <see cref="DBPFPropertyDataType"/> this property has. This informs how many bytes to read for this property. 
		/// </summary>
		private readonly uint _numberOfReps;
		public abstract uint NumberOfReps { get; }

		/// <summary>
		/// The <see cref="DBPFPropertyDataType"/> for this property.
		/// </summary>
		private readonly DBPFPropertyDataType _dataType;
		public abstract DBPFPropertyDataType DataType { get; set; }

		/// <summary>
		/// This is a byte array of the raw values in the property. Assignment of this value takes place in <see cref="DBPFPropertyString"/> or <see cref="DBPFPropertyNumber"/>.
		/// </summary>
		public abstract byte[] ByteValues { get; set; }

		/// <summary>
		/// Parse the byte values for this property depending on the property's <see cref="DBPFPropertyDataType"/>.
		/// </summary>
		public abstract object DecodeValues();

		/// <summary>
		/// Sets the value field to the provided byte array. Also sets numberOfReps to the appropriate value.
		/// </summary>
		/// <param name="newValue">Byte array if <see cref="DBPFPropertyNumber"/> or string if <see cref="DBPFPropertyString"/>.</param>
		public abstract void SetValues(object newValue);



		public DBPFProperty(DBPFPropertyDataType dataType) {
			_dataType = dataType;
			_id = 0x0;
			_numberOfReps = 0;
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.Append($"ID: {_id}, ");
			sb.Append($"Type: {_dataType}, ");
			sb.Append($"Reps: {_numberOfReps}, ");
			sb.AppendLine("Values: ");
			return sb.ToString();
		}

		/// <summary>
		/// Decodes the property from raw data at the given offset.
		/// </summary>
		/// <param name="dData">Decompressed raw data</param>
		/// <param name="offset">Offset to start decoding from</param>
		/// <returns>The DBPFProperty; null if cannot be decoded</returns>
		/// <see cref="https://www.wiki.sc4devotion.com/index.php?title=EXMP"/>
		public static DBPFProperty DecodeExemplarProperty(byte[] dData, int offset = 24) {
			//Read the file identifier and verify if cohort or exemplar
			string fileIdentifier = ByteArrayHelper.ToAString(dData, 0, 8);
			if (fileIdentifier != EQZB1 && fileIdentifier != EQZT1 && fileIdentifier != CQZB1 && fileIdentifier != CQZT1) {
				throw new ArgumentException("Data provided does not represent an exemplar or cohort property!");
			}

			//Read cohort TGI info and determine the number of properties in this entry
			//uint parentCohortTID = BitConverter.ToUInt32(dData, 8);
			//uint parentCohortGID = BitConverter.ToUInt32(dData, 12);
			//uint parentCohortIID = BitConverter.ToUInt32(dData, 16);
			//uint propertyCount = BitConverter.ToUInt32(dData, 20);

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

			//Examine the keyType to determine how to set the values for the new property
			if (keyType == 0x80) {
				offset += 1; //There is a 1 byte unused flag
				uint countOfReps = BitConverter.ToUInt32(dData, offset);
				offset += 4;
				byte[] newValue = new byte[countOfReps * newProperty.DataType.Length];
				for (int idx = 0; idx < newValue.Length; idx++) {
					newValue[idx] = (byte) BitConverter.ToChar(dData, offset + idx);
				}
				newProperty.ByteValues = newValue;
			}

			//keyType == 0x00 ... this is just a single value of the data type length
			else {
				offset += 1; //This one byte is number of value repetitions; seems to always be 0
				byte[] newVals = new byte[dataType.Length];
				for (int idx = 0; idx < dataType.Length; idx++) {
					newVals[idx] = (byte) BitConverter.ToChar(dData, offset + idx);
				}
				newProperty.ByteValues = newVals;
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
			return DecodeExemplarProperty(dData, offset);
		}



	}
}
