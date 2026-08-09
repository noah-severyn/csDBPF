using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace csDBPF {
	/// <summary>
	/// Represents a property storing float value(s).
	/// </summary>
	public class DBPFPropertyFloat : DBPFProperty {
		/// <summary>
		/// Hexadecimal identifier for this property. <see cref="XMLExemplarProperty"/> and <see cref="XMLProperties.AllProperties"/>. 
		/// </summary>
		public override uint ID { get; set; }

        /// <summary>
        /// The data type for this property.
        /// </summary>
        public override PropertyDataType DataType { get; }

           /// <summary>
        /// The number of repetitions of <see cref="DataType"/> this property has. This informs (in part) how many bytes to read for this property. Initialized to 0.
        /// </summary>
        /// <remarks>
        /// Determining the count partially depends on the encoding type. For binary encoded float-type properties: 0 reps = single value, 1 reps = multiple values but currently held to 1 value (problematic on macOS), n reps = n number of values. For text encoded float-type properties: n reps = n number of values.
        /// </remarks>
        public override int NumberOfReps { get; private protected set;  }

		/// <summary>
		/// Specifies the encoding style (Binary or Text) of the property.
		/// </summary>
		/// <remarks>
		/// This property affects <see cref="NumberOfReps"/>. This also determines how this property will be written to file. 
		/// </remarks>
		public override DBPF.Encoding Encoding { get; set; }

        /// <summary>
        /// List of data values which are stored in this property.
        /// </summary>
        private float[] _dataValues;



        /// <summary>
        /// Construct a new DBPFProperty with a float data type.
        /// </summary>
        /// <param name="encoding">Text or Binary encoding type</param>
        public DBPFPropertyFloat(DBPF.Encoding encoding = DBPF.Encoding.Binary) {
			DataType = PropertyDataType.FLOAT32;
			_dataValues = [];
			Encoding = encoding;
			NumberOfReps = 0;
		}
        /// <summary>
        /// Construct a DBPFProperty with a float data type holding a single value.
        /// </summary>
        /// <param name="value">Value of this property</param>
        /// <param name="encoding">Text or Binary encoding type</param>
        public DBPFPropertyFloat(float value, DBPF.Encoding encoding = DBPF.Encoding.Binary) {
			DataType = PropertyDataType.FLOAT32;
			_dataValues = [value];
			Encoding = encoding;
			if (Encoding == DBPF.Encoding.Text) {
                NumberOfReps = 1;
            } else {
                NumberOfReps = 0;
            }
		}
        /// <summary>
        /// Construct a DBPFProperty with a float data type holding multiple values.
        /// </summary>
        /// <param name="values">Values this property holds</param>
        /// <param name="encoding">Text or Binary encoding type</param>
        public DBPFPropertyFloat(float[] values, DBPF.Encoding encoding = DBPF.Encoding.Binary) {
			DataType = PropertyDataType.FLOAT32;
			_dataValues = values;
			Encoding = encoding;
			if (Encoding == DBPF.Encoding.Text) {
				NumberOfReps = _dataValues.Length;
			} else {
                //Note that this implementation is slightly different from the specification to remove the bug on macOS for float-type properties with one value and a rep of 1
                //See: https://community.simtropolis.com/forums/topic/759206-mysterious-glitch-for-simcity-4-mac/?tab=comments#comment-1731134
                if (_dataValues.Length <= 1) {
					NumberOfReps = 0;
				} else {
					NumberOfReps = _dataValues.Length;
				}
			}
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
        public override IEnumerable GetData() {
			return _dataValues;
        }
        /// <inheritdoc/>
        [Obsolete("Use .GetTypedData instead, which returns the data as an exact cast of this items data type, instead of just long/string/float.")]
        public override ValueType GetData(int position) {
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
            return _dataValues;
        }       
        
        /// <inheritdoc/>
        public override object GetTypedData(int position) {
            if (position < 0) {
                throw new ArgumentException("Value must be greater than or equal to 0.");
            }
            return position >= _dataValues.Length ? _dataValues[_dataValues.Length - 1] : _dataValues[position];
        }



        /// <inheritdoc/>
        [Obsolete("Use .SetTypedData instead, which validates the input data is of the exact type of this data type, instead of just long/string/float.")]
        public override void SetData(IEnumerable value) {
            SetTypedData((float[]) value);
        }
        /// <inheritdoc/>
        [Obsolete("Use .SetTypedData instead, which validates the input data is of the exact type of this data type, instead of just long/string/float.")]
        internal override void SetData(IEnumerable value, uint countOfReps) {
			SetData(value);
        }


        /// <inheritdoc/>
        public override void SetTypedData(Array value) {
            if (value is not float[]) {
                throw new ArgumentException($"Argument to DBPFPropertyFloat.SetData must be float[]. {value.GetType()} was provided.");
            }
            _dataValues = (float[]) value;

            if (Encoding == DBPF.Encoding.Text) {
                NumberOfReps = _dataValues.Length;
            } else {
                //Note that this implementation is slightly different from the specification to remove the bug on macOS for float-type properties with one value and a rep of 1
                if (_dataValues.Length <= 1) {
                    NumberOfReps = 0;
                } else {
                    NumberOfReps = _dataValues.Length;
                }
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
				sb.Append($"0x{DBPFUtil.ToHexString(ID)}:{{\"{xmlprop.Name}\"}}=Float32:{NumberOfReps}:{{");
				for (int idx = 0; idx < _dataValues.Length; idx++) {
					sb.Append(_dataValues[idx]);
					if (idx != _dataValues.Length - 1) {
						sb.Append(',');
					}
				}
				sb.Append("}}\r\n");
				return ByteArrayHelper.ToBytes(sb.ToString(), true);
			} else {
				List<byte> bytes = new List<byte>();
				bytes.AddRange(BitConverter.GetBytes(ID));
				bytes.AddRange(BitConverter.GetBytes((ushort) DataType));
				if (NumberOfReps == 0) { //keyType = 0x00
					bytes.AddRange(BitConverter.GetBytes((ushort) 0x00)); //keyType
					bytes.Add(0); //Number of value repetitions. (Seems to be always 0.)
					bytes.AddRange(BitConverter.GetBytes(_dataValues[0]));

				} else { // keyType = 0x80
					bytes.AddRange(BitConverter.GetBytes((ushort) 0x80)); //keyType
					bytes.Add(0); //unused flag
					bytes.AddRange(BitConverter.GetBytes((uint) _dataValues.Length));
					foreach (float value in _dataValues) {
						bytes.AddRange(BitConverter.GetBytes(value));
					}
				}
				return bytes.ToArray();
			}
		}
    }
}
