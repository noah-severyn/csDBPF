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
		public override uint id {
			get { return _id; }
			set { _id = value; }
		}


		/// <summary>
		/// Number of repetitions of the data type in this property. Describes the number of chars in the string (length of string).
		/// </summary>
		private uint _numberOfReps;
		public override uint numberOfReps {
			get { return _numberOfReps; }
		}


		/// <summary>
		/// The <see cref="DBPFPropertyDataType"/> for this property.
		/// </summary>
		private DBPFPropertyDataType _dataType;
		public override DBPFPropertyDataType dataType {
			get { return _dataType; }
			set {
				if (_dataType != DBPFPropertyDataType.STRING) {
					throw new ArgumentException($"Data type of {_dataType.name} provided where {DBPFPropertyDataType.STRING.name} is required.");
				}
				_dataType = value;
			}
		}


		/// <summary>
		/// The byte array of base data for the property. When this is set, <see cref="valuesDecoded"/> is also set to the equivalent value.
		/// </summary>
		private byte[] _values;
		public override byte[] values {
			get { return _values; }
			set {
				_numberOfReps = (uint) value.Length;
				_values = value;
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
			_numberOfReps = 1; //For DBPFPropertyString, count always equals 1 because there is always only one string object representing the value
		}


		public override object DecodeValues() {
			return DBPFUtil.StringFromByteArray(_values);
		}

		public override void SetValues(object newValue) {
			Type t = newValue.GetType();
			if (t != "".GetType()) {
				throw new ArgumentException($"Property {this} cannot apply set the value field to type of {t}. Must be a string.");
			} else {
				_numberOfReps = (uint) ((string) newValue).Length;
				_values = DBPFUtil.StringToByteArray((string) newValue);
			}
		}

		/// <summary>
		/// Appends a string representation of the value onto the base toString. See <see cref="DBPFProperty.ToString"/>
		/// </summary>
		/// <returns>String value of the property</returns>
		public override string ToString() {
			StringBuilder sb = new StringBuilder(base.ToString());
			sb.Append(DBPFUtil.StringFromByteArray(_values));
			return sb.ToString();
		}
	}
}
