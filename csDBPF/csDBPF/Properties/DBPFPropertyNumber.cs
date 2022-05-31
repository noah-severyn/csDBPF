using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace csDBPF.Properties {
	/// <summary>
	/// This class represents a numeric type property. The difference form <see cref="DBPFPropertyString"/> is how the <see cref="ByteValues"/> field is interpreted.
	/// </summary>
	public class DBPFPropertyNumber : DBPFProperty {

		//------------- DBPFPropertyNumber Fields ------------- \\
		private uint _id;
		public override uint ID {
			get { return _id; }
			set { _id = value; }
		}

		private DBPFPropertyDataType _dataType;
		public override DBPFPropertyDataType DataType {
			get { return _dataType; }
			set {
				if (_dataType == DBPFPropertyDataType.STRING) {
					throw new ArgumentException($"Data type of {_dataType.Name} provided where a numerical DBPFPropertyDataType is required.");
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





		//------------- DBPFPropertyNumber Constructor ------------- \\
		/// <summary>
		/// Construct a DBPFProperty with a numerical data type.
		/// </summary>
		/// <param name="dataType"></param>
		public DBPFPropertyNumber(DBPFPropertyDataType dataType) {
			_dataType = dataType;
			_numberOfReps = 0;
		}




		//------------- DBPFPropertyNumber Methods ------------- \\
		/// <summary>
		/// Parse the byte values for this property to return an array of the property's <see cref="DBPFPropertyDataType"/>.
		/// </summary>
		/// <returns>An array <see cref="NumberOfReps"/> long of <see cref="DBPFPropertyDataType"/> numbers</returns>
		public override void DecodeValues() {
			switch (_dataType.Name) {
				case "BOOL":
					_decodedValues = ByteArrayHelper.ToBoolArray(_byteValues);
					break;
				case "UINT8":
					_decodedValues = ByteArrayHelper.ToUint8Array(_byteValues);
					break;
				case "UINT16":
					_decodedValues = ByteArrayHelper.ToUInt16Array(_byteValues);
					break;
				case "SINT32":
					_decodedValues = ByteArrayHelper.ToSInt32Array(_byteValues);
					break;
				case "FLOAT32":
					_decodedValues = ByteArrayHelper.ToFloat32Array(_byteValues);
					break;
				case "UINT32":
					_decodedValues = ByteArrayHelper.ToUInt32Array(_byteValues);
					break;
				case "SINT64":
					_decodedValues = ByteArrayHelper.ToSInt64Array(_byteValues);
					break;
				default:
					break;
			}
		}


		/// <summary>
		/// Sets the value field to the provided byte array. Also sets numberOfReps to the appropriate value.
		/// </summary>
		/// <param name="newValue">Byte array</param>
		//public override void SetValues(byte[] newValue) {
		//	List<byte> result = new List<byte>();
		//	foreach (byte item in newValue) {
		//		result.Add(item);
		//	}
		//	_byteValues = result.ToArray();
		//	_numberOfReps = (uint) (_byteValues.Length / _dataType.Length)-1; //TODO - numberOfReps does not set to correct number when encoding is text ... see rules lined out below
		//	//if text encoding number: 0 means one rep, 1 means can hold multiple but only contains one for now (problematic on macs with floats), n means it contains that many reps
		//	//if text encoding string: always seems to be 1
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
			sb.Append($"Values: {DBPFUtil.PrintByteValues(_byteValues)}");
			return sb.ToString();
		}
	}
}
