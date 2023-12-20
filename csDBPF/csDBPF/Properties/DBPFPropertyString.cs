using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using csDBPF.Entries;
using static csDBPF.Entries.DBPFEntry;

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
			set { _id = value; }
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

		private bool _isTextEncoding;
		/// <summary>
		/// Specifies the encoding style (Binary or Text) of the property.
		/// </summary>
		/// <remarks>
		/// This property affects <see cref="NumberOfReps"/>. This also determines how this property will be written to file. 
		/// </remarks>
		public override bool IsTextEncoding {
			get { return _isTextEncoding; }
			set { _isTextEncoding = value; }
		}

		/// <summary>
		/// The data value stored in this property.
		/// </summary>
		private string _dataValue;




		/// <summary>
		/// Construct a new DBPFProperty with a string data type.
		/// </summary>
		/// <param name="encodingType">Encoding type: binary or text</param>
		public DBPFPropertyString(bool encodingType = EncodingType.Binary) {
			_dataType = DBPFPropertyDataType.STRING;
			_isTextEncoding = encodingType;
			_numberOfReps = 0;
		}
		/// <summary>
		/// Construct a DBPFProperty with a string data type holding a specified string.
		/// </summary>
		/// <param name="value">String to set</param>
		/// <param name="encodingType">Encoding type: binary or text</param>
		public DBPFPropertyString(string value, bool encodingType = EncodingType.Binary) {
			_dataType = DBPFPropertyDataType.STRING;
			_dataValue = value;
			_isTextEncoding = encodingType;
			if (_isTextEncoding) {
				_numberOfReps = 1;
			} else {
				_numberOfReps = _dataValue.Length;
			}
		}



		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>Returns a string that represents the current object.</returns>
		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.Append($"ID: 0x{DBPFUtil.ToHexString(_id)}, ");
			sb.Append($"Type: { _dataType}, ");
			sb.Append($"Reps: {_numberOfReps}, ");
			sb.Append($"Value: {_dataValue}");
			return sb.ToString();
		}


		/// <summary>
		/// Returns the data value stored in this property.
		/// </summary>
		/// <returns>The data value stored in this property</returns>
		public override string GetData() {
			return _dataValue;
		}
		
		
		/// <summary>
		/// Returns the value stored in this property at the given position.
		/// </summary>
		/// <param name="position">Position (or rep) to return</param>
		/// <returns>The data value at the specified position</returns>
		/// <remarks>
		/// The position parameter is ignored because type DBPFPropertyString only stores one string as its data.
		/// </remarks>
        public override object GetData(int position) {
            return _dataValue;
        }


        /// <summary>
        /// Set the data value stored in this property. Value should be of type string.
        /// </summary>
        /// <param name="value">String to set</param>
        /// <exception cref="ArgumentException">Argument to DBPFPropertyString.SetData must be string.</exception>
        public override void SetData(object value) {
			if (value is not string) {
				throw new ArgumentException($"Argument to DBPFPropertyString.SetData must be string. {value.GetType()} was provided.");
			}
			_dataValue = (string) value;
			if (_isTextEncoding) {
				_numberOfReps = 1;
			} else {
				_numberOfReps = _dataValue.Length;
			}
        }
        /// <summary>
        /// Set the values(s) stored in this property. Value should be of type string.
        /// </summary>
        /// <remarks>
        /// This implementation for string-type properties is identical to <see cref="SetData(object)"/>.
        /// </remarks>
        internal override void SetData(object value, uint countOfReps) {
            SetData(value);
        }


        /// <summary>
        /// Process the features and data values of this property into a byte array according to the set encoding type.
        /// </summary>
        /// <returns>A byte array encoding all information for this property</returns>
        public override byte[] ToRawBytes() {
			//Text Encoding
			if (_isTextEncoding) {
				StringBuilder sb = new StringBuilder();
				XMLExemplarProperty xmlprop = XMLProperties.GetXMLProperty(_id);
				sb.Append($"0x{DBPFUtil.ToHexString(_id)}:{{\"{xmlprop.Name}\"}}=String:1:{{");
				sb.Append($"\"{_dataValue}\"");
				sb.Append("}\r\n");
				return ByteArrayHelper.ToBytes(sb.ToString(), true);
			}

			//Binary Encoding
			else {
				List<byte> bytes = new List<byte>();
				bytes.AddRange(BitConverter.GetBytes(_id));
				bytes.AddRange(BitConverter.GetBytes(_dataType.IdentifyingNumber));
				bytes.AddRange(BitConverter.GetBytes((ushort) 0x80)); //String is always keyType = 0x80
				bytes.Add(0); //unused flag
				bytes.AddRange(BitConverter.GetBytes((uint) _dataValue.Length));
				bytes.AddRange(ByteArrayHelper.ToBytes(_dataValue,true));
				return bytes.ToArray();
			}
		}
	}
}
