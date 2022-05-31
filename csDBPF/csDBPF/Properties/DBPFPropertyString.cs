using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace csDBPF.Properties {
	/// <summary>
	/// This class represents a string property. The difference form <see cref="DBPFPropertyNumber"/> is how the <see cref="ByteValues"/> field is interpreted.
	/// </summary>
	public class DBPFPropertyString : DBPFProperty {

		//------------- DBPFPropertyString Fields ------------- \\		
		private uint _id;
		public override uint ID {
			get { return _id; }
			set { _id = value; }
		}

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

		private ushort _keyType;
		public override ushort KeyType {
			get { return _keyType; }
			set { _keyType = value; }
		}
		
		private uint _numberOfReps;
		public override uint NumberOfReps {
			get { return _numberOfReps; }
			set { _numberOfReps = value; }
		}

		private byte[] _byteValues;
		public override byte[] ByteValues {
			get { return _byteValues; }
			set { _byteValues = value; }
		}

		private Array _decodedValues;
		public override Array DecodedValues {
			get {
				if (_decodedValues is null) {
					throw new InvalidOperationException("This property must be decoded before it can be analyzed!");
				}
				return _decodedValues; 
			}
			set { _decodedValues = value; }
		}




		//------------- DBPFPropertyString Constructor ------------- \\		
		/// <summary>
		/// Construct a new DBPFProperty with a string data type.
		/// </summary>
		public DBPFPropertyString() {
			_dataType = DBPFPropertyDataType.STRING;
			_numberOfReps = 0;
		}




		//------------- DBPFPropertyString Methods ------------- \\		
		/// <summary>
		/// Parse the byte values for this property to return a string.
		/// </summary>
		/// <returns>An array of length 1, with the only element being the string value</returns>
		public override void DecodeValues() {
			string[] result = new string[1];
			result[0] = ByteArrayHelper.ToAString(_byteValues);
			_decodedValues = result;
		}


		/// <summary>
		/// Sets the value field to the provided string. Also sets the numberOfReps to the length of the string.
		/// </summary>
		/// <param name="newValue">String value</param>
		//public override void SetValues(byte[] newValue) {
		//	_byteValues = newValue;
		//}


		/// <summary>
		/// Appends a string representation of the value onto the base toString. See <see cref="DBPFProperty.ToString"/>
		/// </summary>
		/// <returns>String value of the property</returns>
		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.Append($"ID: 0x{DBPFUtil.UIntToHexString(_id)}, ");
			sb.Append($"Type: { _dataType}, ");
			sb.Append($"Key: {_keyType}, ");
			sb.Append($"Reps: {_numberOfReps}, ");
			sb.Append($"Values: {ByteArrayHelper.ToAString(_byteValues)}");
			return sb.ToString();
		}
	}
}
