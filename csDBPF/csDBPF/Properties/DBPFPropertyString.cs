using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace csDBPF.Properties {
	public class DBPFPropertyString : DBPFProperty {
		//TODO - fill this comment here with properties.xml list
		/// <summary>
		/// Hexadecimal identifier for this property. <see cref=""/> 
		/// </summary>
		private uint _id;
		public override uint id {
			get { return _id; }
			set { _id = value; }
		}


		/// <summary>
		/// Number of reps of the data type in this property. This is always 1.
		/// </summary>
		private int _numberOfReps;
		public override int numberOfReps {
			get { return _numberOfReps; }
			set { _numberOfReps = value; }
		}


		/// <summary>
		/// The <see cref="DBPFPropertyDataType"/> for this property.
		/// </summary>
		private DBPFPropertyDataType _dataType;
		public override DBPFPropertyDataType dataType {
			get { return _dataType; }
			set {
				if (_dataType != DBPFPropertyDataType.STRING) {
					throw new ArgumentException($"Data type of {_dataType.name} provided where {DBPFPropertyDataType.STRING.name} is required.");
				}
				_dataType = value;
			}
		}


		/// <summary>
		/// The byte array of base data for the property. When this is set, <see cref="DBPFPropertyString.valuesDecoded"/> is also set to the equivalent value.
		/// </summary>
		private byte[] _values;
		public override byte[] values {
			get { return _values; }
			set {
				_values = value;
				_valuesDecoded = DBPFUtil.StringFromByteArray(value);
			}
		}


		/// <summary>
		/// When decoded, <see cref="DBPFPropertyString.values"/> returns a string. When this is set, <see cref="DBPFPropertyString.values"/> is also set to the equivalent value.
		/// </summary>
		private string _valuesDecoded;
		public override object valuesDecoded {
			get { return _valuesDecoded; }
			set {
				Type t = value.GetType();

				//If type(value) is string then directly set the decoded value
				if (t == "".GetType()) {
					_values = DBPFUtil.StringToByteArray((string) value);
					_valuesDecoded = (string) value;
					return;
				}

				//Otherwise check if value is an array of bytes
				if (!t.IsArray || t.GetElementType() != byte.MinValue.GetType()) {
					throw new ArgumentException($"An array of {t.GetElementType().Name} was provided when {Type.GetType("System.String")} is expected");
				} else {
					ArrayList result = new ArrayList(); //Use array list because we do not quite know the length
					IEnumerable e = value as IEnumerable;
					int idx = 0;
					if (e != null) {
						foreach (object item in e) {
							result.Add((byte) item);
							idx++;
						}

						_values = result.ToArray(typeof(byte)) as byte[];
						_valuesDecoded = string.Join("", result);
						return;
					}
				}
				throw new ArgumentException($"Property {this} cannot apply set the value field to type of {t}.");
			}
		}


		/// <summary>
		/// Construct a new DBPFPropertyString.
		/// </summary>
		/// <param name="dataType"></param>
		public DBPFPropertyString(DBPFPropertyDataType dataType) : base(dataType) {
			_dataType = dataType;
			_numberOfReps = 1; //For DBPFPropertyString, count always equals 1 because there is always only one string object representing the value
		}


		/// <summary>
		/// Appends a string representation of the value onto the base toString. See <see cref="DBPFProperty.ToString"/>
		/// </summary>
		/// <returns>String value of the property</returns>
		public override string ToString() {
			StringBuilder sb = new StringBuilder(base.ToString());
			sb.Append(DBPFUtil.StringFromByteArray(_values));
			return sb.ToString();
		}
	}
}
