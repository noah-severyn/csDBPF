using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using static csDBPF.Entries.DBPFEntry;
using static csDBPF.Properties.DBPFProperty;

namespace csDBPF.Properties {
	/// <summary>
	/// Represents a property storing a string value.
	/// </summary>
	public class DBPFPropertyString : DBPFProperty {	
		/// <summary>
		/// Hexadecimal identifier for this property. <see cref="XMLExemplarProperty"/> and <see cref="XMLProperties.AllProperties"/>. 
		/// </summary>
		public override uint ID { get; set; }

        /// <summary>
        /// The <see cref="PropertyDataType"/> for this property.
        /// </summary>
        public override PropertyDataType DataType { get; }

        /// <summary>
        /// The number of repetitions of <see cref="PropertyDataType"/> this property has. This informs (in part) how many bytes to read for this property. Initialized to 0.
        /// </summary>
        /// <remarks>
        /// Determining the count partially depends on the encoding type. For binary encoded string-type properties: length of string. For text encoded string-type properties: always 1.
        /// </remarks>
        public override int NumberOfReps { get; private protected set; }

		/// <summary>
		/// Specifies the encoding style (Binary or Text) of the property.
		/// </summary>
		/// <remarks>
		/// This property affects <see cref="NumberOfReps"/>. This also determines how this property will be written to file. 
		/// </remarks>
		public override EncodingType Encoding { get; set; }

        /// <summary>
        /// The data value stored in this property.
        /// </summary>
        private string _dataValue;




        /// <summary>
        /// Construct a new DBPFProperty with a string data type.
        /// </summary>
        /// <param name="encodingType">Text or Binary encoding type</param>
        public DBPFPropertyString(EncodingType encodingType = EncodingType.Binary) {
			DataType = PropertyDataType.STRING;
			Encoding = encodingType;
			NumberOfReps = 0;
		}
		/// <summary>
		/// Construct a DBPFProperty with a string data type holding a specified string.
		/// </summary>
		/// <param name="value">String to set</param>
		/// <param name="encodingType">Text or Binary encoding type</param>
		public DBPFPropertyString(string value, EncodingType encodingType = EncodingType.Binary) {
			DataType = PropertyDataType.STRING;
			_dataValue = value;
			Encoding = encodingType;
			if (Encoding == EncodingType.Text) {
				NumberOfReps = 1;
			} else {
				NumberOfReps = _dataValue.Length;
			}
		}



		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>Returns a string that represents the current object.</returns>
		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.Append($"ID: 0x{DBPFUtil.ToHexString(ID)}, ");
			sb.Append($"Type: {DataType}, ");
			sb.Append($"Reps: {NumberOfReps}, ");
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
			if (Encoding == EncodingType.Text) {
                NumberOfReps = 1;
			} else {
                NumberOfReps = _dataValue.Length;
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
        public override byte[] ToBytes() {
			if (Encoding == EncodingType.Text) {
				StringBuilder sb = new StringBuilder();
				XMLExemplarProperty xmlprop = XMLProperties.GetXMLProperty(ID);
				sb.Append($"0x{DBPFUtil.ToHexString(ID)}:{{\"{xmlprop.Name}\"}}=String:1:{{");
				sb.Append($"\"{_dataValue}\"");
				sb.Append("}\r\n");
				return ByteArrayHelper.ToBytes(sb.ToString(), true);
			} else {
				List<byte> bytes = new List<byte>();
				bytes.AddRange(BitConverter.GetBytes(ID));
				bytes.AddRange(BitConverter.GetBytes((ushort) DataType));
				bytes.AddRange(BitConverter.GetBytes((ushort) 0x80)); //String is always keyType = 0x80
				bytes.Add(0); //unused flag
				bytes.AddRange(BitConverter.GetBytes((uint) _dataValue.Length));
				bytes.AddRange(ByteArrayHelper.ToBytes(_dataValue,true));
				return bytes.ToArray();
			}
		}
	}
}
