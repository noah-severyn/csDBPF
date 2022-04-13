using System;
using System.Collections.Generic;
using System.Text;

namespace csDBPF.Properties {
	public class DBPFPropertyDataType {
		private static readonly Dictionary<string, DBPFPropertyDataType> dataTypes = new Dictionary<string, DBPFPropertyDataType>();
		public static readonly DBPFPropertyDataType SINT32;
		public static readonly DBPFPropertyDataType FLOAT32;
		public static readonly DBPFPropertyDataType UINT32;
		public static readonly DBPFPropertyDataType BOOL;
		public static readonly DBPFPropertyDataType UINT8;
		public static readonly DBPFPropertyDataType SINT64;
		public static readonly DBPFPropertyDataType UINT16;
		public static readonly DBPFPropertyDataType STRING;

		/// <summary>
		/// Data type identifier
		/// </summary>
		private string _name;
		public string name {
			get { return _name; }
		}

		/// <summary>
		/// Numeric value encoded in the exemplar data used to identify the property data type
		/// </summary>
		private ushort _identifyingNumber;
		public ushort identifyingNumber {
			get { return _identifyingNumber; }
		}

		/// <summary>
		/// Length in bytes of the property
		/// </summary>
		private int _length;
		public int length {
			get { return _length; }
		}

		/// <summary>
		/// Returns the name of the property data type.
		/// </summary>
		/// <returns>Data type name</returns>
		public override string ToString() {
			return _name;
		}



		/// <summary>
		/// Returns the <see cref="DBPFPropertyDataType"/> from the specified value.
		/// </summary>
		/// <param name="value">Value from raw data</param>
		/// <returns>Corresponding DBPFPropertyType to the specified value; exception thrown if no result is found</returns>
		public static DBPFPropertyDataType LookupDataType(ushort value) {
			foreach (DBPFPropertyDataType type in dataTypes.Values) {
				if (type.identifyingNumber == value) {
					return type;
				}
			}
			throw new KeyNotFoundException($"Value {DBPFUtil.UIntToHexString(value, 2)} does not match a known property data type!");
		}
		public static DBPFPropertyDataType LookupDataType(string value) {
			foreach (DBPFPropertyDataType type in dataTypes.Values) {
				if (type.name.ToUpper() == value.ToUpper()) {
					return type;
				}
			}
			throw new KeyNotFoundException($"Value {value} does not match a known property data type!");
		}



		#region Static declaration and constructors
		/// <summary>
		/// This constructor only to be used internally to this class to declare known data types in the static constructor.
		/// </summary>
		/// <param name="name">Data type identifier</param>
		/// <param name="value">Numeric value encoded in the exemplar data used to identify the property data type</param>
		/// <param name="length">Length in bytes of the property</param>
		private DBPFPropertyDataType(string name, ushort value, int length) {
			_name = name;
			_identifyingNumber = value;
			_length = length;
		}
		
		static DBPFPropertyDataType() {
			SINT32 = new DBPFPropertyDataType("SINT32", 0x700, 4);
			FLOAT32 = new DBPFPropertyDataType("FLOAT32", 0x900, 4);
			UINT32 = new DBPFPropertyDataType("UINT32", 0x300, 4);
			BOOL = new DBPFPropertyDataType("BOOL", 0xB00, 1);
			UINT8 = new DBPFPropertyDataType("UINT8", 0x100, 1);
			SINT64 = new DBPFPropertyDataType("SINT64", 0x800, 8);
			UINT16 = new DBPFPropertyDataType("UINT16", 0x200, 2);
			STRING = new DBPFPropertyDataType("STRING", 0xC00, 1);

			dataTypes.Add(SINT32.ToString(), SINT32);
			dataTypes.Add(FLOAT32.ToString(), FLOAT32);
			dataTypes.Add(UINT32.ToString(), UINT32);
			dataTypes.Add(BOOL.ToString(), BOOL);
			dataTypes.Add(UINT8.ToString(), UINT8);
			dataTypes.Add(SINT64.ToString(), SINT64);
			dataTypes.Add(UINT16.ToString(), UINT16);
			dataTypes.Add(STRING.ToString(), STRING);
		}
		#endregion
	}

}
