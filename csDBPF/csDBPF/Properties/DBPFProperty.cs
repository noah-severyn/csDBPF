using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Collections;
using csDBPF.Entries;

namespace csDBPF.Properties
{
    /// <summary>
    /// An abstract class defining the structure of a Property and the methods for interfacing with it. This class is only relevant for Exemplar and Cohort type entries.
    /// </summary>
    public abstract partial class DBPFProperty {
		/// <summary>
		/// Hexadecimal identifier for this property. <see cref="XMLExemplarProperty"/> and <see cref="XMLProperties.AllProperties"/>. 
		/// </summary>
		public abstract uint ID { get; internal set; }

		/// <summary>
		/// The <see cref="DBPFPropertyDataType"/> for this property.
		/// </summary>
		public abstract DBPFPropertyDataType DataType { get; }

		/// <summary>
		/// The number of repetitions of <see cref="DBPFPropertyDataType"/> this property has. This informs (in part) how many bytes to read for this property. Initialized to 0.
		/// </summary>
		/// <remarks>
		/// Determining the count partially depends on the encoding type. For binary encoded string type: length of string. For text encoded string type: always 1. For binary encoded (all) and text encoded number types (except float): 0 reps = single value, 1 reps = multiple values but currently held to 1 value (problematic on macOS when the DataType is float), n reps = n number of values. For text encoded float type: n reps = n number of values. This property is necessary because of uneven implementation of the DataValues property in implementing types.
		/// </remarks>
		public abstract int NumberOfReps { get; }

		/// <summary>
		/// Return the values(s) stored in this property.
		/// </summary>
		public abstract object GetDataValues();

		/// <summary>
		/// Set the values(s) stored in this property.
		/// </summary>
		public abstract void SetDataValues(object value);

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public abstract byte[] ToRawBytes();




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
	}
}
