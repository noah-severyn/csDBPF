using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace csDBPF.Properties {
	/// <summary>
	/// Represents a property storing a string value.
	/// </summary>
	public class DBPFPropertyString : DBPFProperty {	
		private uint _id;
		/// <summary>
		/// Hexadecimal identifier for this property. <see cref="XMLExemplarProperty"/> and <see cref="XMLProperties.AllProperties"/>. 
		/// </summary>
		public override uint ID {
			get { return _id; }
			internal set { _id = value; }
		}

		private readonly DBPFPropertyDataType _dataType;
		/// <summary>
		/// The <see cref="DBPFPropertyDataType"/> for this property.
		/// </summary>
		public override DBPFPropertyDataType DataType {
			get { return _dataType; }
		}
		
		private int _numberOfReps;
		/// <summary>
		/// The number of repetitions of <see cref="DBPFPropertyDataType"/> this property has. This informs (in part) how many bytes to read for this property. Initialized to 0.
		/// </summary>
		/// <remarks>
		/// Determining the count partially depends on the encoding type. For binary encoded string-type properties: length of string. For text encoded string-type properties: always 1.
		/// </remarks>
		public override int NumberOfReps {
			get { return _numberOfReps; }
		}

		private string _dataValue;
		/// <summary>
		/// List of data values which are stored in this property.
		/// </summary>


	
		/// <summary>
		/// Construct a new DBPFProperty with a string data type.
		/// </summary>
		public DBPFPropertyString() {
			_dataType = DBPFPropertyDataType.STRING;
			_numberOfReps = 0;
		}
		/// <summary>
		/// Construct a DBPFProperty with a string data type holding a specified string.
		/// </summary>
		/// <param name="value"></param>
		public DBPFPropertyString(string value) {
			_dataType = DBPFPropertyDataType.STRING;
			_dataValue = value;
			_numberOfReps = _dataValue.Length;
		}



		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>Returns a string that represents the current object.</returns>
		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.Append($"ID: 0x{DBPFUtil.UIntToHexString(_id)}, ");
			sb.Append($"Type: { _dataType}, ");
			sb.Append($"Reps: {_numberOfReps}, ");
			sb.Append($"Value: {_dataValue}");
			return sb.ToString();
		}


		/// <summary>
		/// Returns the data value stored in this property.
		/// </summary>
		/// <returns>The data value stored in this property</returns>
		public override string GetDataValues() {
			return _dataValue;
		}

		/// <summary>
		/// Set the data value stored in this property.
		/// </summary>
		/// <param name="value">String to set</param>
		/// <exception cref="ArgumentException">Argument to DBPFPropertyString.SetDataValues must be string.</exception>
		public override void SetDataValues(object value) {
			if (value is not string) {
				throw new ArgumentException($"Argument to DBPFPropertyString.SetDataValues must be string. {value.GetType()} was provided.");
			}
			_dataValue = (string) value;
			_numberOfReps = _dataValue.Length;
		}



		public override byte[] ToRawBytes() {
			throw new NotImplementedException();
		}
	}
}
