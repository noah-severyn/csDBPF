using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace csDBPF.Properties {
	//the _value field from the abstract class is simply a byte array representing the values, each of these subclasses would interpret that byte array and push it back to the user in the best format depending on the data type???
	public class DBPFPropertyString : DBPFProperty {
		private uint _id;
		protected override uint id {
			get { return _id; }
			set { _id = value; }
		}

		private int _count;
		protected override int count {
			get { return _count; }
			set { _count = value; }
		}

		private DBPFPropertyDataType _dataType;
		protected override DBPFPropertyDataType dataType {
			get { return _dataType; }
			set {
				if (_dataType != DBPFPropertyDataType.STRING) {
					throw new ArgumentException($"Data type of {_dataType.name} provided where {DBPFPropertyDataType.STRING.name} is required.");
				}
				_dataType = value; 
			}
		}

		private byte[] _values;
		protected override byte[] values {
			get { return _values; }
			set { 
				//Set byte array values
				_values = value;

				//Set decoded string value
				_valuesDecoded = DBPFUtil.StringFromByteArray(value);
			}
		}

		private string _valuesDecoded;
		protected override object valuesDecoded {
			get { return _valuesDecoded; }
			set {
				Type t = value.GetType();

				//If type(value) is string then directly set the decoded value
				if (t == "".GetType()) {
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
						
						//Set values property to the newly set byte array
						_values = result.ToArray(typeof(byte)) as byte[];

						//Build string from array list
						_valuesDecoded = string.Join("", result);
					}
				}
			}
		}

		//blank constructor here because we are not doing anything different in the setup - the difference comes when interpreting the value property
		public DBPFPropertyString(DBPFPropertyDataType dataType) : base(dataType) { }



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
