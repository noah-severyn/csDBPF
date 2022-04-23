using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace csDBPF.Properties {
	public class DBPFPropertyString : DBPFProperty {
		//TODO - fill this comment here with properties.xml list
		/// <summary>
		/// Hexadecimal identifier for this property. <see cref=""/> 
		/// </summary>
		private uint _id;
		public override uint ID {
			get { return _id; }
			set { _id = value; }
		}


		/// <summary>
		/// Number of repetitions of the data type in this property. Describes the number of chars in the string (length of string).
		/// </summary>
		private uint _numberOfReps;
		public override uint NumberOfReps {
			get { return _numberOfReps; }
		}


		/// <summary>
		/// The <see cref="DBPFPropertyDataType"/> for this property.
		/// </summary>
		private DBPFPropertyDataType _dataType;
		public override DBPFPropertyDataType DataType {
			get { return _dataType; }
			set {
				if (_dataType != DBPFPropertyDataType.STRING) {
					throw new ArgumentException($"Data type of {_dataType.Name} provided where {DBPFPropertyDataType.STRING.Name} is required.");
				}
				_dataType = value;
			}
		}


		/// <summary>
		/// The byte array of base data for the property. When this is set, <see cref="valuesDecoded"/> is also set to the equivalent value.
		/// </summary>
		private byte[] _byteValues;
		public override byte[] ByteValues {
			get { return _byteValues; }
			set {
				_byteValues = value;
				_numberOfReps = (uint) value.Length;
				//_valuesDecoded = DBPFUtil.StringFromByteArray(value);
			}
		}


		/// <summary>
		/// When decoded, <see cref="DBPFPropertyString.values"/> returns a string. When this is set, <see cref="values"/> is also set to the equivalent value.
		/// </summary>
		//private string _valuesDecoded;
		//public override object valuesDecoded {
		//	get { return _valuesDecoded; }
		//	set {
		//		Type t = value.GetType();

		//		//If type(value) is string then directly set the decoded value
		//		if (t != "".GetType()) {
		//			throw new ArgumentException($"Property {this} cannot apply set the value field to type of {t}.");
		//		} else {
		//			_numberOfReps = (uint) ((string) value).Length;
		//			_values = DBPFUtil.StringToByteArray((string) value);
		//			_valuesDecoded = (string) value;
		//		}
		//	}
		//}


		/// <summary>
		/// Construct a new DBPFPropertyString.
		/// </summary>
		/// <param name="dataType"></param>
		public DBPFPropertyString(DBPFPropertyDataType dataType) : base(dataType) {
			_dataType = dataType;
			_numberOfReps = 1;
		}


		/// <summary>
		/// Parse the byte values for this property to return a string.
		/// </summary>
		/// <returns>A string</returns>
		public override object DecodeValues() {
			return ByteArrayHelper.ToAString(_byteValues);
		}


		/// <summary>
		/// Sets the value field to the provided string. Also sets the numberOfReps to the length of the string.
		/// </summary>
		/// <param name="newValue">String value</param>
		public override void SetValues(object newValue) {
			Type t = newValue.GetType();
			if (t != "".GetType()) {
				throw new ArgumentException($"Property {this} cannot apply set the value field to type of {t}. Must be a string.");
			} else {
				_byteValues = ByteArrayHelper.ToByteArray((string) newValue);
				_numberOfReps = (uint) ((string) newValue).Length;
			}
		}

		/// <summary>
		/// Appends a string representation of the value onto the base toString. See <see cref="DBPFProperty.ToString"/>
		/// </summary>
		/// <returns>String value of the property</returns>
		public override string ToString() {
			StringBuilder sb = new StringBuilder(base.ToString());
			sb.Append(ByteArrayHelper.ToAString(_byteValues));
			return sb.ToString();
		}
	}
}
