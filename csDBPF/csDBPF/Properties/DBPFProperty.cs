using System;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace csDBPF.Properties {

	/// <summary>
	/// An abstract class defining the structure of a Property and the methods for interfacing with it.
	/// </summary>
	public abstract class DBPFProperty {
		private const string EQZB1 = "EQZB1###";
		private const string EQZT1 = "EQZT1###";
		private const string CQZB1 = "CQZB1###";
		private const string CQZT1 = "CQZT1###";

		private uint _id;
		public abstract uint id { get; set; }

		private uint _numberOfReps;
		public abstract uint numberOfReps { get; }

		private DBPFPropertyDataType _dataType;
		public abstract DBPFPropertyDataType dataType { get; set; }

		/// <summary>
		/// This is a byte array of the raw values in the property.
		/// </summary>
		private byte[] _byteValues;
		public abstract byte[] byteValues { get; set; }

		/// <summary>
		/// This is the decoded (interpreted) values based on the implementing class type. With the exception of string, will take the form of an array of the implementing class's type (int or float).
		/// </summary>
		//public abstract object valuesDecoded { get; set; }

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
			_byteValues = null;
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
			if (dataType.name == "STRING") {
				newProperty = new DBPFPropertyString(dataType);
			} else {
				newProperty = new DBPFPropertyNumber(dataType);
			}
			newProperty.id = propertyID;

			//Examine the keyType to determine how to set the values for the new property
			if (keyType == 0x80) {
				offset += 1; //There is a 1 byte unused flag
				uint countOfReps = BitConverter.ToUInt32(dData, offset);
				offset += 4;
				byte[] newValue = new byte[countOfReps * newProperty.dataType.length];
				for (int idx = 0; idx < newValue.Length; idx++) {
					newValue[idx] = (byte) BitConverter.ToChar(dData, offset + idx);
				}
				newProperty.byteValues = newValue;
			}

			//keyType == 0x00 ... this is just a single value of the data type length
			else {
				offset += 1; //This one byte is number of value repetitions; seems to always be 0
				byte[] newVals = new byte[dataType.length];
				for (int idx = 0; idx < dataType.length; idx++) {
					newVals[idx] = (byte) BitConverter.ToChar(dData, offset + idx);
				}
				newProperty.byteValues = newVals;
			}
			return newProperty;
		}


		//TODO - make one function DecodeProperty which calls a bunch of private specialized functions depending on the entry TGI knownType


		public static DBPFProperty DecodeCohortProperty(byte[] dData, int offset = 0) {
			return DecodeExemplarProperty(dData, offset);
		}


		/// <summary>
		/// Queries new_properties.xml and returns the exemplar property (PROPERTY) element matching the specified ID.
		/// </summary>
		/// <param name="id">Property ID to lookup</param>
		/// <returns>XElement of the specified property ID</returns>
		public static XElement GetXMLProperty(uint id) {
			//TODO - figure out if it is quicker to routinely query only what we need from the xml doc one thing at a time or load the whole doc into memory and then just grab the parts we need from that


			XElement xml = XElement.Load("C:\\Users\\Administrator\\OneDrive\\Documents\\csDBPF\\csDBPF\\csDBPF\\Properties\\new_properties.xml");
			//Within XML doc, there is a single element of PROPERTIES which contain many elements PROPERTY
			string str = "0x" + DBPFUtil.UIntToHexString(id, 8).ToLower();
			IEnumerable<XElement> matchingExemplarProperty = from prop in xml.Elements("PROPERTIES").Elements("PROPERTY")
															 where prop.Attribute("ID").Value == "0x" + DBPFUtil.UIntToHexString(id, 8).ToLower()
															 select prop;
			//LINQ query returns an IEnumerable object, but because of our filter there should always only be one result so we do not have to worry about iterating over the return
			return matchingExemplarProperty.First();



			//foreach (var prop in matchingExemplarProperty) {
			//	Console.WriteLine(prop.Value);
			//	string idd = prop.Attribute("ID").Value;
			//	string name = prop.Attribute("Name").Value;
			//	string type = prop.Attribute("Type").Value;
			//}
			//Example XML format in new_properties.xml
			//<PROPERTY Name="Name" ID="0x00000000" Type="Uint32" Default="0x00000000" ShowAsHex="Y" >
			//		 < HELP >
			//			 A GZGUID that identifies a class interface
			//		 </HELP>
			//</PROPERTY>

			//list of possible fields for a property:
			//name, id, type, default, showashex, minlength, maxlength, count, minvalue, maxvalue
			//some properties have values restricted to certain things - these are the OPTION lists ... currently unimplemented
		}

	}
}
