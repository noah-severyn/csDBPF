using System;
using System.Collections.Generic;
using System.Text;
using csDBPF.Properties;

namespace csDBPF.Properties {

	/// <summary>
	/// An abstract class that defines the structure 
	/// </summary>
	public abstract class DBPFProperty {
		private uint _id;
		protected uint id {
			get { return _id; }
			set { _id = value; }
		}

		private int _count;
		protected int count {
			get { return _count; }
			set { _count = value; }
		}

		private DBPFPropertyDataType _dataType;
		protected DBPFPropertyDataType dataType {
			get { return _dataType; }
			set { _dataType = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// This is purposely vague as other classes that implement this class like XXXXX and YYYY and ZZZZZ implement their own data types
		/// </remarks>
		private object _values;
		public object value {
			get { return _values; }
			set { _values = value; }
		}



		protected DBPFProperty(DBPFPropertyDataType dataType) {
			_dataType = dataType;
			_id = 0;
			_count = 0;
			_values = new object();
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.Append($"ID: {_id}, ");
			sb.Append($"Type: {_dataType}, ");
			sb.Append($"Reps: {_count}, ");
			sb.AppendLine("Values: ");
			object[] vals = (object[]) _values;
			foreach (object val in vals) {
				sb.Append($"{val}, ");
			}
			return sb.ToString();
		}

		/// <summary>
		/// Decodes the property from raw data at the given offset.
		/// </summary>
		/// <param name="dData">Decompressed raw data</param>
		/// <param name="offset">Offset to start decoding from</param>
		/// <returns>The DBPFProperty; null if cannot be decoded</returns>
		public static DBPFProperty DecodeExemplarProperty(byte[] dData, int offset = 24) {
			//See: https://www.wiki.sc4devotion.com/index.php?title=EXMP

			//Read the property's numeric value (0x0000 0000)
			uint propertyID = BitConverter.ToUInt32(dData, offset);
			offset += 4;

			//Read and return the data value type
			ushort valueType = DBPFUtil.ReverseBytes(BitConverter.ToUInt16(dData, offset));
			DBPFPropertyDataType dataType = DBPFPropertyDataType.LookupDataType(valueType);
			offset += 2;

			//Read the property keyType
			ushort keyType = DBPFUtil.ReverseBytes(BitConverter.ToUInt16(dData, offset));
			offset += 2;

			//Examine the keyType

			DBPFProperty property = null;
			
			if (keyType == 0x80) {
				offset += 1; //Theres a 1 byte unused flag
				uint numberOfRepititions = BitConverter.ToUInt32(dData, offset);
				offset += 4;
			} else {
				offset += 1; //This one byte is number of value repetitions; seems to always be 0
			}

			return null;
		}
		
	}
}
