using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace csDBPF.Properties {
	public class DBPFPropertyString : DBPFProperty {
		
		private uint _id;
		/// <summary>
		/// Hexadecimal identifier for this property. <see cref="XMLExemplarProperty"/> and <see cref="XMLProperties.AllProperties"/>. 
		/// </summary>
		public override uint ID {
			get { return _id; }
			set { _id = value; }
		}

		private DBPFPropertyDataType _dataType;
		/// <summary>
		/// The <see cref="DBPFPropertyDataType"/> for this property.
		/// </summary>
		public override DBPFPropertyDataType DataType {
			get { return _dataType; }
			set {
				if (_dataType != DBPFPropertyDataType.STRING) {
					throw new ArgumentException($"Data type of {_dataType.Name} provided where {DBPFPropertyDataType.STRING.Name} is required.");
				}
				_dataType = value;
			}
		}

		private ushort _keyType;
		/// <summary>
		/// The KeyType contains a value of 0x80 if the property has more than or equal to one repetition, and 0x00 if it has 0 repetitions. 0x80 is the only recorded KeyType
		/// </summary>
		public override ushort KeyType {
			get { return _keyType; }
			set { _keyType = value; }
		}
		
		private uint _numberOfReps;
		/// <summary>
		/// Number of repetitions of the data type in this property. Describes the number of chars in the string (length of string).
		/// </summary>
		public override uint NumberOfReps {
			get { return _numberOfReps; }
		}

		private byte[] _byteValues;
		/// <summary>
		/// The byte array of base data for the property. When this is set, <see cref="valuesDecoded"/> is also set to the equivalent value.
		/// </summary>
		public override byte[] ByteValues {
			get { return _byteValues; }
			set {
				_byteValues = value;
				_numberOfReps = (uint) value.Length;
			}
		}


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
