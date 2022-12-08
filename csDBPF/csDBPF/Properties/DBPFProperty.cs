using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Collections;
using csDBPF.Entries;

namespace csDBPF.Properties
{
    /// <summary>
    /// An abstract class defining the structure of a Property and the methods for interfacing with it. This class is only relevant for Exemplar and Cohort type entries. The data for the property is not parsed or decoded until <see cref="DecodeValues"/> is called to set the actual entry data.
    /// </summary>
    public abstract partial class DBPFProperty {
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
		public abstract Array DecodedValues { get; set; } //TODO - setting DecodedValues like this is mega whack

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

		//For internal unit testing only
		//TODO fix up all the unit tests that require this
		public static DBPFProperty DecodeProperty(byte[] array, int offset = 0) {
			DBPFEntryEXMP exmp = new DBPFEntryEXMP(DBPFTGI.EXEMPLAR, 0, 0, 0, array);

			if (exmp.IsBinaryEncoding()) {
				return DBPFEntryEXMP.DecodeProperty_Binary(exmp.ByteData, offset);
			} else {
				return DBPFEntryEXMP.DecodeProperty_Text(exmp.ByteData, offset);
			}
		}
	}
}
