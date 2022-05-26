using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace csDBPF.Properties {
	class DBPFPropertyNumber : DBPFProperty {




		//------------- DBPFPropertyNumber Fields ------------- \\
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
		/// Number of repetitions of the data type in this property. The byte size of this property's <see cref="DBPFPropertyDataType"/> multiplied by this number equals the byte size of this property's values in bytes. Initialized to 1. This is set automatically when <see cref="SetValues(byte[])"/> is called on the member.
		/// </summary>
		public override uint NumberOfReps {
			get { return _numberOfReps; }
		}

		private byte[] _byteValues;
		/// <summary>
		/// The byte array of base data for the property. This is set by calling <see cref="SetValues(byte[])"/> on the member.
		/// </summary>
		public override byte[] ByteValues {
			get { return _byteValues; }
		}




		//------------- DBPFPropertyNumber Constructor ------------- \\
		/// <summary>
		/// Construct a new DBPFPropertyInteger.
		/// </summary>
		/// <param name="dataType"></param>
		public DBPFPropertyNumber(DBPFPropertyDataType dataType) : base(dataType) {
			_dataType = dataType;
			_numberOfReps = 1;
		}




		//------------- DBPFPropertyNumber Methods ------------- \\
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
		public override void SetValues(byte[] newValue) {
			List<byte> result = new List<byte>();
			foreach (byte item in newValue) {
				result.Add(item);
			}
			_byteValues = result.ToArray();
			_numberOfReps = (uint) (_byteValues.Length / _dataType.Length)-1; //TODO - numberOfReps does not set to correct number when encoding is text ... see rules lined out below
			//if text encoding number: 0 means one rep, 1 means can hold multiple but only contains one for now (problematic on macs with floats), n means it contains that many reps
			//if text encoding string: always seems to be 1
		}


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
