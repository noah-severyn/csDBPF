﻿using System;
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

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// This is purposely vague as other classes that implement this class like XXXXX and YYYY and ZZZZZ implement their own data types
		/// </remarks>
		private byte[] _values;
		protected override byte[] value {
			get { return _values; }
			set { _values = value; }
		}


		//blank constructor here because we are not doing anything different in the setup - the difference comes when interpreting the value property
		public DBPFPropertyString(DBPFPropertyDataType dataType) : base(dataType) {}

		

		/// <summary>
		/// Appends a string representation of the value onto the base toString. See <see cref="DBPFProperty.ToString"/>
		/// </summary>
		/// <returns>String value of the property</returns>
		public override string ToString() {
			StringBuilder sb = new StringBuilder(base.ToString());
			sb.Append(DBPFUtil.CharsFromByteArray(_values));
			return sb.ToString();
		}

	}
}