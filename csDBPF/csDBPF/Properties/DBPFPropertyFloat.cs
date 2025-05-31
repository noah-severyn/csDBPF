using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using csDBPF;
using static csDBPF.DBPFEntry;

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
		public override EncodingType Encoding { get; set; }

        /// <summary>
        /// List of data values which are stored in this property.
        /// </summary>
        private List<float> _dataValues;



        /// <summary>
        /// Construct a new DBPFProperty with a float data type.
        /// </summary>
        /// <param name="encodingType">Text or Binary encoding type</param>
        public DBPFPropertyFloat(EncodingType encodingType = EncodingType.Binary) {
			DataType = PropertyDataType.FLOAT32;
			Encoding = encodingType;
			NumberOfReps = 0;
		}
        /// <summary>
        /// Construct a DBPFProperty with a float data type holding a single value.
        /// </summary>
        /// <param name="value">Value of this property</param>
        /// <param name="encodingType">Text or Binary encoding type</param>
        public DBPFPropertyFloat(float value, EncodingType encodingType = EncodingType.Binary) {
			DataType = PropertyDataType.FLOAT32;
			_dataValues = [value];
			Encoding = encodingType;
			if (Encoding == EncodingType.Text) {
                NumberOfReps = 1;
            } else {
                NumberOfReps = 0;
            }
		}
        /// <summary>
        /// Construct a DBPFProperty with a float data type holding multiple values.
        /// </summary>
        /// <param name="values">Values this property holds</param>
        /// <param name="encodingType">Text or Binary encoding type</param>
        public DBPFPropertyFloat(List<float> values, EncodingType encodingType = EncodingType.Binary) {
			DataType = PropertyDataType.FLOAT32;
			_dataValues = values;
			Encoding = encodingType;
			if (Encoding == EncodingType.Text) {
				NumberOfReps = _dataValues.Count;
			} else {
                //Note that this implementation is slightly different from the specification to remove the bug on macOS for float-type properties with one value and a rep of 1
                //See: https://community.simtropolis.com/forums/topic/759206-mysterious-glitch-for-simcity-4-mac/?tab=comments#comment-1731134
                if (_dataValues.Count <= 1) {
					NumberOfReps = 0;
				} else {
					NumberOfReps = _dataValues.Count;
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



		/// <summary>
		/// Returns a list of data values which are stored in this property.
		/// </summary>
		/// <returns>List of data values which are stored in this property</returns>
		public override IEnumerable GetData() {
			return _dataValues;
        }
        /// <summary>
        /// Returns the value stored in this property at the given position.
        /// </summary>
        /// <param name="position">Position (or rep) to return</param>
        /// <returns>The data value at the specified position</returns>
        /// <remarks>
        /// If the position parameter is greater than the number of values, the last value is returned.
        /// </remarks>
        public override ValueType GetData(int position) {
            if (position < 0) {
                throw new ArgumentException("Value must be greater than or equal to 0.");
            }
            if (position >= _dataValues.Count) {
                return _dataValues[_dataValues.Count - 1];
            }
            return _dataValues[position];
        }



        /// <summary>
        /// Set the data values stored in this property. Value should be of type <![CDATA[IEnumerable<float>]]>.
        /// </summary>
        /// <param name="value">Values to set</param>
        /// <exception cref="ArgumentException">Argument to DBPFPropertyFloat.SetData must be <![CDATA[IEnumerable<float>]]>.</exception>
        public override void SetData(IEnumerable value) {
			if (value is not IEnumerable<float>) {
				throw new ArgumentException($"Argument to DBPFPropertyFloat.SetData must be IEnumerable<float>. {value.GetType()} was provided.");
			}
			_dataValues = (List<float>) value;

			if (Encoding == EncodingType.Text) {
				NumberOfReps = _dataValues.Count;
			} else {
				//Note that this implementation is slightly different from the specification to remove the bug on macOS for float-type properties with one value and a rep of 1
				if (_dataValues.Count <= 1) {
					NumberOfReps = 0;
				} else {
					NumberOfReps = _dataValues.Count;
				}
			}
        }
        /// <summary>
        /// Set the values(s) stored in this property. Value should be of type <![CDATA[List<float>]]>.
        /// </summary>
        /// <remarks>
        /// This implementation for float-type properties is identical to <see cref="SetData(IEnumerable)"/> to avoid the macOS float bug.
        /// </remarks>
        internal override void SetData(IEnumerable value, uint countOfReps) {
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
				sb.Append($"0x{DBPFUtil.ToHexString(ID)}:{{\"{xmlprop.Name}\"}}=Float32:{NumberOfReps}:{{");
				for (int idx = 0; idx < _dataValues.Count(); idx++) {
					sb.Append(_dataValues[idx]);
					if (idx != _dataValues.Count() - 1) {
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
					bytes.AddRange(BitConverter.GetBytes((uint) _dataValues.Count));
					foreach (float value in _dataValues) {
						bytes.AddRange(BitConverter.GetBytes(value));
					}
				}
				return bytes.ToArray();
			}
		}
    }
}
