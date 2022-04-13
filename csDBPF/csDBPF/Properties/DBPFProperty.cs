using System;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace csDBPF.Properties {

	/// <summary>
	/// An abstract class defining the structure of a Property and the methods for interfacing with it.
	/// </summary>
	public abstract class DBPFProperty {
		private const string EQZB1 = "EQZB1###";
		private const string EQZT1 = "EQZT1###";
		private const string CQZB1 = "CQZB1###";
		private const string CQZT1 = "CQZT1###";
		private static readonly Dictionary<uint, ExemplarProperty> AllProperties = new Dictionary<uint, ExemplarProperty>();

		private readonly uint _id;
		public abstract uint ID { get; set; }

		private readonly uint _numberOfReps;
		public abstract uint NumberOfReps { get; }

		private readonly DBPFPropertyDataType _dataType;
		public abstract DBPFPropertyDataType DataType { get; set; }

		/// <summary>
		/// This is a byte array of the raw values in the property. Assignment of this value takes place in <see cref="DBPFPropertyString"/> or <see cref="DBPFPropertyNumber"/>.
		/// </summary>
		public abstract byte[] ByteValues { get; set; }

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
			newProperty.ID = propertyID;

			//Examine the keyType to determine how to set the values for the new property
			if (keyType == 0x80) {
				offset += 1; //There is a 1 byte unused flag
				uint countOfReps = BitConverter.ToUInt32(dData, offset);
				offset += 4;
				byte[] newValue = new byte[countOfReps * newProperty.DataType.length];
				for (int idx = 0; idx < newValue.Length; idx++) {
					newValue[idx] = (byte) BitConverter.ToChar(dData, offset + idx);
				}
				newProperty.ByteValues = newValue;
			}

			//keyType == 0x00 ... this is just a single value of the data type length
			else {
				offset += 1; //This one byte is number of value repetitions; seems to always be 0
				byte[] newVals = new byte[dataType.length];
				for (int idx = 0; idx < dataType.length; idx++) {
					newVals[idx] = (byte) BitConverter.ToChar(dData, offset + idx);
				}
				newProperty.ByteValues = newVals;
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

			//some properties have values restricted to certain things - these are the OPTION lists ... currently unimplemented
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// The XML structure is as follows: each XML tag is an XElement (element). Each element can have one or more XAttributes (attributes) which help describe the element. See new_properties.xsd for required vs optional attributes for Properties.
		/// </remarks>
		public static void LoadXMLProperties() {
			//XDocument is a great way to read XML files. Since we do not need to do anything too complicated and do not need to do any xml writing, we can go with the much simpler XElement instead which has fewer methods and properties to worry about.
			XElement xml = XElement.Load("C:\\Users\\Administrator\\OneDrive\\Documents\\csDBPF\\csDBPF\\csDBPF\\Properties\\new_properties.xml");
			IEnumerable<XElement> exemplarProperties = from prop in xml.Elements("PROPERTIES").Elements("PROPERTY")
													   select prop;

			foreach (XElement prop in exemplarProperties) {
				uint id = Convert.ToUInt32(prop.Attribute("ID").Value, 16);
				//string nm = prop.Attribute("Name").Value;
				//DBPFPropertyDataType dt = DBPFPropertyDataType.LookupDataType(prop.Attribute("Type").Value);
				//bool hx = prop.Attribute("ShowAsHex").Value == "Y";

				//For the rest of the attributes, we do not know which ones will exist for a property; directly setting them from the constructor results in a null reference exception. Examine then set each one individually.
				//https://stackoverflow.com/a/44929328 This is kind of hideous, but we are first checking if the XAttribute exists. If it does a string value is returned; null is returned if it does not exist. Throw this returned value into the TryParse. Since we have to deal with the possibility of the value being null and TryParse can only out non-nullable values, we need to check the result of TryParse whether it was successful or not. If it was successful we just return the out value, if it was not successful we finally just return null.
				//short? ct = short.TryParse((string) TryXAttributeExists(prop, "Count"), out short s) ? s : (short?) null;
				//string df = (string) TryXAttributeExists(prop, "DefaultValue");
				//int? ml = int.TryParse((string) TryXAttributeExists(prop, "MinLength"), out int i1) ? i1 : (int?) null;
				//int? xl = int.TryParse((string) TryXAttributeExists(prop, "MaxLength"), out int i2) ? i2 : (int?) null;
				//uint? mv = uint.TryParse((string) TryXAttributeExists(prop, "MinValue"), out uint u1) ? u1 : (uint?) null;
				//uint? xv = uint.TryParse((string) TryXAttributeExists(prop, "MaxValue"), out uint u2) ? u2 : (uint?) null;
				//uint? st = uint.TryParse((string) TryXAttributeExists(prop, "Step"), out uint u3) ? u3 : (uint?) null;

				ExemplarProperty exmp = new ExemplarProperty(
					id,
					prop.Attribute("Name").Value,
					DBPFPropertyDataType.LookupDataType(prop.Attribute("Type").Value),
					prop.Attribute("ShowAsHex").Value == "Y",
					short.TryParse((string) TryXAttributeExists(prop, "Count"), out short s) ? s : (short?) null,
					(string) TryXAttributeExists(prop, "DefaultValue"),
					int.TryParse((string) TryXAttributeExists(prop, "MinLength"), out int i1) ? i1 : (int?) null,
					int.TryParse((string) TryXAttributeExists(prop, "MaxLength"), out int i2) ? i2 : (int?) null,
					uint.TryParse((string) TryXAttributeExists(prop, "MinValue"), out uint u1) ? u1 : (uint?) null,
					uint.TryParse((string) TryXAttributeExists(prop, "MaxValue"), out uint u2) ? u2 : (uint?) null,
					uint.TryParse((string) TryXAttributeExists(prop, "Step"), out uint u3) ? u3 : (uint?) null
				);

				AllProperties.Add(id, exmp);
			}

		}


		/// <summary>
		/// Generic Helper function to determine if the XAttribute exists for the given XElement and return its value if it does, or null if not.
		/// </summary>
		/// <param name="element"><see cref="XElement"/> to examine</param>
		/// <param name="xname">Name of <see cref="XAttribute"/> to look for. Represents the <see cref="XName"/>.</param>
		/// <returns><see cref="XAttribute.Value"/> if XAttribute exists; null otherwise</returns>
		private static object TryXAttributeExists(XElement element, string xname) {
			XAttribute xattr = element.Attribute(xname);
			if (xattr == null) {
				return null;
			}
			return xattr.Value;
		}


		private class ExemplarProperty {
			//These parameters are required
			//<xs:attribute name = "ID" type="xs:string" use="required" />
			//<xs:attribute name = "Name" type="xs:string" use="required" />
			//<xs:attribute name = "Type" type="xs:string" use="required" />
			//<xs:attribute name = "ShowAsHex" type="xs:string" use="required" />
			private uint _id;
			protected uint id {
				get { return _id; }
			}
			private string _name;
			protected string name {
				get { return _name; }
			}
			private DBPFPropertyDataType _type;
			protected DBPFPropertyDataType type {
				get { return _type; }
			}
			private bool _showAsHex;
			protected bool showAsHex {
				get { return _showAsHex; }
			}

			//These parameters are optional
			//<xs:attribute name = "Count" type="xs:short" use="optional" />
			//<xs:attribute name = "Default" type="xs:string" use="optional" />
			//<xs:attribute name = "MinLength" type="xs:string" use="optional" />
			//<xs:attribute name = "MaxLength" type="xs:string" use="optional" />
			//<xs:attribute name = "MinValue" type="xs:string" use="optional" />
			//<xs:attribute name = "MaxValue" type="xs:string" use="optional" />
			//<xs:attribute name = "Step" type="xs:string" use="optional" />
			private short? _count;
			protected short? count {
				get { return _count; }
			}
			private string _defaultValue;
			protected string defaultValue {
				get { return _defaultValue; }
			}
			private int? _minLength;
			protected int? minLength {
				get { return _minLength; }
				set { _minLength = value; }
			}
			private int? _maxLength;
			protected int? maxLength {
				get { return _maxLength; }
			}
			private uint? _minValue;
			protected uint? minValue {
				get { return _minValue; }
			}
			private uint? _maxValue;
			protected uint? maxValue {
				get { return _maxValue; }
			}
			private uint? _step;
			protected uint? step {
				get { return _step; }
			}

			public ExemplarProperty() {
				_id = 0;
				_name = null;
				_type = null;
				_showAsHex = true;
			}

			public ExemplarProperty(uint id, string name, DBPFPropertyDataType type, bool showAsHex) {
				_id = 0;
				_name = null;
				_type = null;
				_showAsHex = true;
			}

			public ExemplarProperty(uint id, string name, DBPFPropertyDataType type, bool showAsHex, short? count = null, string defaultValue = null, int? minLength = null, int? maxLength = null, uint? minValue = null, uint? maxValue = null, uint? step = null) {
				_id = id;
				_name = name;
				_type = type;
				_showAsHex = showAsHex;
				_count = count;
				_defaultValue = defaultValue;
				_minLength = minLength;
				_maxLength = maxLength;
				_minValue = minValue;
				_maxValue = maxValue;
				_step = step;
			}
		}
	}
}
