using System;
using System.Collections.Generic;
using System.Text;

namespace csDBPF.Properties {
	public class DBPFPropertyDataType {
		public static readonly DBPFPropertyDataType SINT32;
		public static readonly DBPFPropertyDataType FLOAT32;
		public static readonly DBPFPropertyDataType UINT32;
		public static readonly DBPFPropertyDataType BOOL;
		public static readonly DBPFPropertyDataType UINT8;
		public static readonly DBPFPropertyDataType SINT64;
		public static readonly DBPFPropertyDataType UINT16;
		public static readonly DBPFPropertyDataType STRING;

		/// <summary>
		/// Type identifier
		/// </summary>
		private string _name;
		public string name {
			get { return _name; }
		}

		/// <summary>
		/// Numeric value encoded in the exemplar data
		/// </summary>
		private ushort _value;
		public ushort value {
			get { return _value; }
		}

		/// <summary>
		/// Length in bytes of the property
		/// </summary>
		private int _length;
		public int length {
			get { return _length; }
		}


		/// <summary>
		/// This constructor only to be used internally to this class to declare known data types in the static constructor.
		/// </summary>
		/// <param name="name">Type identifier</param>
		/// <param name="value">Numeric value encoded in the exemplar data</param>
		/// <param name="length">Length in bytes of the property</param>
		private DBPFPropertyDataType(string name, ushort value, int length) {
			_name = name;
			_value = value;
			_length = length;
		}
		

		static DBPFPropertyDataType() {
			SINT32 = new DBPFPropertyDataType("SINT32", 0x70, 4);
			FLOAT32 = new DBPFPropertyDataType("FLOAT32", 0x90, 4);
			UINT32 = new DBPFPropertyDataType("UINT32", 0x30, 4);
			BOOL = new DBPFPropertyDataType("BOOL", 0xB0, 1);
			UINT8 = new DBPFPropertyDataType("UINT8", 0x10, 1);
			SINT64 = new DBPFPropertyDataType("SINT62", 0x80, 8);
			UINT16 = new DBPFPropertyDataType("UINT16", 0x20, 2);
			STRING = new DBPFPropertyDataType("STRING", 0xC0, 1);
		}

	}

}
