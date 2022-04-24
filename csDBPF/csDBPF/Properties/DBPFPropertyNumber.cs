using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace csDBPF.Properties {
	class DBPFPropertyNumber : DBPFProperty {

		private uint _ID;
		/// <summary>
		/// Hexadecimal identifier for this property. <see cref="DBPFExemplarProperty"/> and <see cref="XMLProperties.AllProperties"/>. 
		/// </summary>
		public override uint ID {
			get { return _ID; }
			set { _ID = value; }
		}

		private DBPFPropertyDataType _dataType;
		/// <summary>
		/// The <see cref="DBPFPropertyDataType"/> for this property.
		/// </summary>
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
		/// <summary>
		/// The KeyType contains a value of 0x80 if the property has more than or equal to one repetition, and 0x00 if it has 0 repetitions. 0x80 is the only recorded KeyType
		/// </summary>
		public override ushort KeyType {
			get { return _keyType; }
			set { _keyType = value; }
		}

		private uint _numberOfReps;
		/// <summary>
		/// Number of repetitions of the data type in this property. The byte size of this property's <see cref="DBPFPropertyDataType"/> multiplied by this number equals the byte size of this property's values in bytes. Initialized to 1.
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
				_numberOfReps = (uint) (value.Length / _dataType.Length);
				//_valuesDecoded = DBPFUtil.StringFromByteArray(value);
			}
		}


		/// <summary>
		/// When decoded, <see cref="DBPFPropertyString.values"/> returns a string. When this is set, <see cref="values"/> is also set to the equivalent value.
		/// </summary>
		//private object _valuesDecoded;
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
		/// Construct a new DBPFPropertyInteger.
		/// </summary>
		/// <param name="dataType"></param>
		public DBPFPropertyNumber(DBPFPropertyDataType dataType) : base(dataType) {
			_dataType = dataType;
			_numberOfReps = 1;
		}


		/// <summary>
		/// Parse the byte values for this property to return an array of the property's <see cref="DBPFPropertyDataType"/>.
		/// </summary>
		/// <returns>An array <see cref="NumberOfReps"/> long of <see cref="DBPFPropertyDataType"/> numbers</returns>
		public override object DecodeValues() {
			switch (_dataType.Name) {
				case "BOOL":
					return ByteArrayHelper.ToBoolArray(_byteValues);
				case "UINT8":
					return ByteArrayHelper.ToUint8Array(_byteValues);
				case "UINT16":
					return ByteArrayHelper.ToUInt16Array(_byteValues);
				case "SINT32":
					return ByteArrayHelper.ToSInt32Array(_byteValues);
				case "FLOAT32":
					return ByteArrayHelper.ToFloat32Array(_byteValues);
				case "UINT32":
					return ByteArrayHelper.ToUInt32Array(_byteValues);
				case "SINT64":
					return ByteArrayHelper.ToSInt64Array(_byteValues);
				default:
					return null;
			}
		}


		/// <summary>
		/// Sets the value field to the provided byte array. Also sets numberOfReps to the appropriate value.
		/// </summary>
		/// <param name="newValue">Byte array</param>
		public override void SetValues(object newValue) {
			//check if newValue is an array
			Type t = newValue.GetType();
			if (!t.IsArray) {
				throw new ArgumentException("An array of numbers is expected.");
			}

			List<byte> result = new List<byte>();
			if (!(newValue is IEnumerable e)) {
				throw new ArgumentException("Unable to iterate over object!");
			} else {
				foreach (object item in e) {
					result.Add((byte) item);
				}
			}
			_byteValues = result.ToArray();

			_numberOfReps = (uint) (_byteValues.Length / _dataType.Length);
		}


		/// <summary>
		/// Appends a string representation of the value onto the base toString. See <see cref="DBPFProperty.ToString"/>
		/// </summary>
		/// <returns>String value of the property</returns>
		public override string ToString() {
			StringBuilder sb = new StringBuilder(base.ToString());
			sb.Append($"{DBPFUtil.PrintByteValues(_byteValues)}");
			//sb.Append(DecodeValues());
			return sb.ToString();
		}
	}
}
