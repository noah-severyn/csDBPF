using System;
using System.Collections.Generic;
using System.Text;
using csDBPF.Properties;

namespace csDBPF.Properties {

	/// <summary>
	/// An abstract class that defines the structure 
	/// </summary>
	public abstract class DBPFProperty {
		private const ulong EQZB1 = 0x45515A4231232323; //EQZB1####
		private const ulong EQZT1 = 0x45515A5431232323; //EQZT1####
		private const ulong CQZB1 = 0x43515A4231232323; //CQZB1####
		private const ulong CQZT1 = 0x43515A5431232323; //CQZT1####

		private uint _id;
		protected abstract uint id { get; set; }

		private int _count;
		protected abstract int count { get; set; }

		private DBPFPropertyDataType _dataType;
		protected abstract DBPFPropertyDataType dataType { get; set; }

		/// <summary>
		/// This is a byte array of the raw values in the property.
		/// </summary>
		private byte[] _values;
		protected abstract byte[] values { get; set; }

		/// <summary>
		/// This is the decoded (interpreted) values based on the implementing class type. With the exception of string, will take the form of an array of the implementing class's type (int or float).
		/// </summary>
		protected abstract object valuesDecoded { get; set; }


		protected DBPFProperty(DBPFPropertyDataType dataType) {
			_dataType = dataType;
			_id = 0x0;
			_count = 0;
			_values = null;
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.Append($"ID: {_id}, ");
			sb.Append($"Type: {_dataType}, ");
			sb.Append($"Reps: {_count}, ");
			sb.AppendLine("Values: ");
			//TODO - interpret that byte array and push it back to the user in the best format depending on the data type???
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
			ulong fileIdentifier = DBPFUtil.ReverseBytes(BitConverter.ToUInt64(dData, 0));
			if (fileIdentifier != EQZB1 && fileIdentifier != EQZT1 && fileIdentifier != CQZB1 && fileIdentifier != CQZT1) {
				throw new ArgumentException("Data provided does not represent an exemplar or cohort property!");
			}

			//Read cohort TGI info and determine the number of properties in this entry
			//uint parentCohortTID = BitConverter.ToUInt32(dData, 8);
			//uint parentCohortGID = BitConverter.ToUInt32(dData, 12);
			//uint parentCohortIID = BitConverter.ToUInt32(dData, 16);
			//uint propertyCount = BitConverter.ToUInt32(dData, 20);

			//Read the property's numeric value (0x0000 0000)
			uint propertyID = BitConverter.ToUInt32(dData, offset);
			offset += 4;

			//Read and return the data value type
			ushort valueType = DBPFUtil.ReverseBytes(BitConverter.ToUInt16(dData, offset));
			DBPFPropertyDataType dataType = DBPFPropertyDataType.LookupDataType(valueType);
			offset += 2;

			//Read the property keyType
			//Because we are just reading the value to a byte array, it effectively does not matter whether the key type is 0x00 or 0x80 - the values are read the same way and the processing to parse the values is pushed off to later to the specific type classes.
			ushort keyType = BitConverter.ToUInt16(dData, offset);
			offset += 2;

			//Create new decoded property and set id and dataType
			DBPFProperty newProperty = null;
			if (dataType.name == "STRING") {
				newProperty = new DBPFPropertyString(dataType);
			} else if (dataType.name == "FLOAT32") {
				newProperty = new DBPFPropertyFloat(dataType);
			} else {
				newProperty = new DBPFPropertyInteger(dataType);
			}
			newProperty.id = propertyID;

			//Examine the keyType to determine how to set the values for the new property
			if (keyType == 0x80) {
				offset += 1; //Theres a 1 byte unused flag
				uint numberOfBytes = BitConverter.ToUInt32(dData, offset);
				offset += 4;
				byte[] newVals = new byte[dataType.length];
				for (int idx = 0; idx < numberOfBytes; idx++) {
					newVals[idx] = (byte) BitConverter.ToChar(dData, offset + idx);
				}
			}

			//keyType == 0x00 ... this is just a single value of the data type length
			else {
				offset += 1; //This one byte is number of value repetitions; seems to always be 0
				byte[] newVals = new byte[dataType.length];
				for (int idx = 0; idx < dataType.length; idx++) {
					newVals[idx] = (byte) BitConverter.ToChar(dData, offset + idx);
				}
				//newProperty.value = newVals; //TODO uncomment !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			}
			return newProperty;
		}


		//TODO - make one function DecodeProperty which calls a bunch of private specialized functions depending on the entry TGI knownType


		public static DBPFProperty DecodeCohortProperty(byte[] dData, int offset = 0) {
			return DecodeExemplarProperty(dData, offset);
		}

	}
}
