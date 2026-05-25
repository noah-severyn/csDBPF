using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static csDBPF.DBPFProperty;

namespace csDBPF {
	/// <summary>
	/// Represents a property storing integer-based value(s).
	/// </summary>
	/// <remarks>
	/// All numbers are stored internally as long (equal to largest used DBPFPropertyDataType of SINT64). The actual underlying data type is defined by the <see cref="PropertyDataType"/>.
	/// </remarks>
	public class DBPFPropertyLong : DBPFProperty {
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
        /// 0 reps = single value; n reps = n number of values.
        /// </remarks>
        public override int NumberOfReps { get; private protected set; }

		/// <summary>
		/// Specifies the encoding style (Binary or Text) of the property.
		/// </summary>
		/// <remarks>
		/// This only determines how this property will be written to file. No properties of this instance are affected by this. 
		/// </remarks>
		public override DBPF.Encoding Encoding { get; set; }

		/// <summary>
		/// List of data values which are stored in this property.
		/// </summary>
		private long[] _dataValues;



        /// <summary>
        /// Construct a DBPFProperty with a numerical data type.
        /// </summary>
        /// <param name="dataType">Data type of this property</param>
        /// <param name="encoding">Text or Binary encoding type</param>
        /// <exception cref="ArgumentException">DBPFPropertyNumber cannot contain float or string data.</exception>
        public DBPFPropertyLong(PropertyDataType dataType, DBPF.Encoding encoding = DBPF.Encoding.Binary) {
			if (dataType == PropertyDataType.FLOAT32 || dataType == PropertyDataType.STRING) {
				throw new ArgumentException("DBPFPropertyNumber cannot contain float or string data.");
			}
			DataType = dataType;
			_dataValues = [];
			Encoding = encoding;
			NumberOfReps = 0;
		}
        /// <summary>
        /// Construct a DBPFProperty with a numerical data type holding a single value.
        /// </summary>
        /// <param name="dataType">Data type of this property</param>
        /// <param name="value">Value of this property</param>
        /// <param name="encoding">Text or Binary encoding type</param>
        /// <exception cref="ArgumentException">DBPFPropertyNumber cannot contain float or string data.</exception>
        public DBPFPropertyLong(PropertyDataType dataType, long value, DBPF.Encoding encoding = DBPF.Encoding.Binary) {
			if (dataType == PropertyDataType.FLOAT32 || dataType == PropertyDataType.STRING) {
				throw new ArgumentException("DBPFPropertyNumber cannot contain float or string data.");
			}
			DataType = dataType;
			_dataValues = [value];
			Encoding = encoding;
			NumberOfReps = 0;
		}
        /// <summary>
        /// Construct a DBPFProperty with a numerical data type holding multiple values.
        /// </summary>
        /// <param name="dataType">Data type of this property</param>
        /// <param name="values">Values this property holds</param>
        /// <param name="encoding">Text or Binary encoding type</param>
        /// <exception cref="ArgumentException">DBPFPropertyNumber cannot contain float or string data.</exception>
        public DBPFPropertyLong(PropertyDataType dataType, Array values, DBPF.Encoding encoding = DBPF.Encoding.Binary) {
			if (dataType == PropertyDataType.FLOAT32 || dataType == PropertyDataType.STRING) {
				throw new ArgumentException("DBPFPropertyNumber cannot contain float or string data.");
			}
			DataType = dataType;
            _dataValues = [];
			SetTypedData(values);
			Encoding = encoding;
        }



        /// <inheritdoc/>
        public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.Append($"ID: 0x{DBPFUtil.ToHexString(ID)}, ");
			sb.Append($"Type: {DataType}, ");
			sb.Append($"Reps: {NumberOfReps}, ");
			sb.Append($"Values: {_dataValues.ToString()}");
			return sb.ToString();
		}



        /// <inheritdoc/>
        [Obsolete("Use .GetTypedData instead, which returns the data as an exact cast of this items data type, instead of just long/string/float.")]
        public override long[] GetData() {
			return _dataValues;
		}
        /// <inheritdoc/>
        [Obsolete("Use .GetTypedData instead, which returns the data as an exact cast of this items data type, instead of just long/string/float.")]
        public override object GetData(int position) {
			if (position < 0) {
				throw new ArgumentException("Value must be greater than or equal to 0.");
			}
			if (position >= _dataValues.Length) {
				return _dataValues[_dataValues.Length - 1];
			}
			return _dataValues[position];
        }

        /// <inheritdoc/>
        public override Array GetTypedData() {
            switch (DataType) {
                case PropertyDataType.UINT8:
                    return _dataValues.Select(Convert.ToByte).ToArray();
                case PropertyDataType.UINT16:
                    return _dataValues.Select(Convert.ToUInt16).ToArray();
                case PropertyDataType.UINT32:
                    return _dataValues.Select(Convert.ToUInt32).ToArray();
                case PropertyDataType.SINT32:
                    return _dataValues.Select(Convert.ToInt32).ToArray();
                case PropertyDataType.BOOL:
                    return _dataValues.Select(Convert.ToBoolean).ToArray();
                default: // SINT64
					return _dataValues.ToArray();
            }
        }

        /// <inheritdoc/>
        public override object GetTypedData(int position) {
            if (position < 0) {
                throw new ArgumentException("Value must be greater than or equal to 0.");
            }
            long value = position >= _dataValues.Length ? _dataValues[_dataValues.Length - 1] : _dataValues[position];
            switch (DataType) {
                case PropertyDataType.UINT8:
                    return Convert.ToByte(value);
                case PropertyDataType.UINT16:
                    return Convert.ToUInt16(value);
                case PropertyDataType.UINT32:
                    return Convert.ToUInt32(value);
                case PropertyDataType.SINT32:
                    return Convert.ToInt32(value);
                case PropertyDataType.BOOL:
                    return Convert.ToBoolean(value);
                default: // SINT64
                    return value;
            }
        }



        /// <inheritdoc/>
        [Obsolete("Use .SetTypedData instead, which validates the input data is of the exact type of this data type, instead of just long/string/float.")]
        public override void SetData(IEnumerable value) {
            if (value is not IEnumerable<long>) {
                throw new ArgumentException($"Argument to DBPFPropertyNumber.SetData must be IEnumerable<long>. {value.GetType()} was provided.");
            }
            _dataValues = [.. (IEnumerable<long>) value];
            if (_dataValues.Length <= 1) {
                NumberOfReps = 0;
            } else {
                NumberOfReps = _dataValues.Length;
            }
        }
        /// <inheritdoc/>
        [Obsolete("Use .SetTypedData instead, which validates the input data is of the exact type of this data type, instead of just long/string/float.")]
        internal override void SetData(IEnumerable value, uint countOfReps) {
            if (value is not List<long>) {
                throw new ArgumentException($"Argument to DBPFPropertyNumber.SetData must be List<long>. {value.GetType()} was provided.");
            }

            _dataValues = ((List<long>) value).ToArray();
			NumberOfReps = (int) countOfReps;
        }




        /// <inheritdoc/>
        public override void SetTypedData(Array value) {
            bool valid = value is IEnumerable<long> || DataType switch {
                PropertyDataType.UINT8 => value is byte[],
                PropertyDataType.UINT16 => value is ushort[],
                PropertyDataType.UINT32 => value is uint[],
                PropertyDataType.SINT32 => value is int[],
                PropertyDataType.BOOL => value is bool[],
                _ => false,
            };
            if (!valid) {
                throw new ArgumentException($"DataType {DataType} does not match the provided type {value.GetType()}.");
            }

            long[] converted = new long[value.Length];
            int idx = 0;
            foreach (object v in value) {
                converted[idx++] = Convert.ToInt64(v);
            }

            _dataValues = converted;
            if (_dataValues.Length <= 1) {
                NumberOfReps = 0;
            } else {
                NumberOfReps = _dataValues.Length;
            }
        }



        /// <summary>
        /// Process the features and data values of this property into a byte array according to the set encoding type.
        /// </summary>
        /// <returns>A byte array encoding all information for this property</returns>
        public override byte[] ToBytes() {
			if (Encoding == DBPF.Encoding.Text) {
				StringBuilder sb = new StringBuilder();
				XMLExemplarProperty xmlprop = XMLProperties.GetXMLProperty(ID);
				sb.Append($"0x{DBPFUtil.ToHexString(ID)}:{{\"{xmlprop.Name}\"}}={LookupDataTypeName(DataType)}:{NumberOfReps}:{{");
				for (int idx = 0; idx < _dataValues.Length; idx++) {
					sb.Append($"0x{DBPFUtil.ToHexString(_dataValues[idx], LookupDataTypeLength(DataType) * 2)}");
					if (idx != _dataValues.Length - 1) {
						sb.Append(',');
					}
				}
				sb.Append("}}\r\n");
				return ByteArrayHelper.ToBytes(sb.ToString(), true);
			} else {
				List<byte> bytes = [];
				bytes.AddRange(BitConverter.GetBytes(ID));
				bytes.AddRange(BitConverter.GetBytes((ushort) DataType));
				if (NumberOfReps == 0) { //keyType = 0x00
					bytes.AddRange(BitConverter.GetBytes((ushort) 0x00)); //keyType
					bytes.Add(0); //Number of value repetitions. (Seems to be always 0.)
					bytes.AddRange(ByteArrayHelper.ToBytes(_dataValues[0], LookupDataTypeLength(DataType)));

				} else { // keyType = 0x80
					bytes.AddRange(BitConverter.GetBytes((ushort) 0x80)); //keyType
					bytes.Add(0); //unused flag
					bytes.AddRange(BitConverter.GetBytes((uint) _dataValues.Length));
					foreach (long value in _dataValues) {
						bytes.AddRange(ByteArrayHelper.ToBytes(value, LookupDataTypeLength(DataType)));
					}
				}
				return bytes.ToArray();
			}
		}
	}
}
