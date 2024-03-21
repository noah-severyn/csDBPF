using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Collections;
using csDBPF.Entries;
using static csDBPF.Entries.DBPFEntry;

namespace csDBPF.Properties
{
    /// <summary>
    /// An abstract class defining the structure of a Property and the methods for interfacing with it. This class is only relevant for Exemplar and Cohort type entries.
    /// </summary>
    public abstract partial class DBPFProperty {
		/// <summary>
		/// Hexadecimal identifier for this property. <see cref="XMLExemplarProperty"/> and <see cref="XMLProperties.AllProperties"/>. 
		/// </summary>
		public abstract uint ID { get; set; }

        /// <summary>
        /// The <see cref="PropertyDataType"/> for this property.
        /// </summary>
        public abstract PropertyDataType DataType { get; }

        /// <summary>
        /// The number of repetitions of <see cref="PropertyDataType"/> this property has. This informs (in part) how many bytes to read for this property. Initialized to 0.
        /// </summary>
        /// <remarks>
        /// Determining the count partially depends on the encoding type. For binary encoded string type: length of string. For text encoded string type: always 1. For binary encoded (all) and text encoded number types (except float): 0 reps = single value, 1 reps = multiple values but currently held to 1 value (problematic on macOS when the DataType is float), n reps = n number of values. For text encoded float type: n reps = n number of values. This property is necessary because of uneven implementation of the DataValues property in implementing types.
        /// </remarks>
        public abstract int NumberOfReps { get; private protected set; }

		/// <summary>
		/// Specifies the encoding style (Binary or Text) of the property.
		/// </summary>
		/// <remarks>
		/// May affect implementation of other fields, namely <see cref="NumberOfReps"/>. Property is presented so the default value (false) will be binary encoding which we want to use most of the time.
		/// </remarks>
		public abstract EncodingType Encoding { get; set; }

		/// <summary>
		/// Returns the values(s) stored in this property.
		/// </summary>
		public abstract object GetData();

        /// <summary>
        /// Returns the value stored in this property at the given position.
        /// </summary>
        public abstract object GetData(int position);

		/// <summary>
		/// Set the values(s) stored in this property.
		/// </summary>
		public abstract void SetData(object value);
        /// <summary>
        /// Set the values(s) stored in this property.
        /// </summary>
        /// <remarks>
		/// This override is necessary when countOfReps = 1; otherwise, if passed a list of length 1 then the number of reps would be set to 0. Figuring the byte offset for the next property will then be off by 4 because the extra 4 bytes representing the number of reps will be ignored. This is only necessary for long-type properties.
		/// </remarks>
        internal abstract void SetData(object value, uint countOfReps);

        /// <summary>
        /// Process the features and data values of this property into a byte array according to the set encoding type.
        /// </summary>
        /// <returns>A byte array encoding all information for this property</returns>
        public abstract byte[] ToBytes();



        /// <summary>
        /// This class stores information related to the possible data types for properties.
        /// </summary>
        /// <remarks>
        /// See <see href="https://www.wiki.sc4devotion.com/index.php?title=EXMP#ValueType">Value Types</see>.
        /// </remarks>
        public enum PropertyDataType {
            /// <summary>
            /// Unknown type. Equivalent to null
            /// </summary>
            UNKNOWN = 0x000,
            /// <summary>
            /// Equivalent to System.Byte
            /// </summary>
            UINT8 = 0x100,
            /// <summary>
            /// Equivalent to System.UInt16
            /// </summary>
            UINT16 = 0x200,
            /// <summary>
            /// Equivalent to System.UInt32
            /// </summary>
            UINT32 = 0x300,
            /// <summary>
            /// Equivalent to System.Int32
            /// </summary>
            SINT32 = 0x700,
            /// <summary>
            /// Equivalent to System.Int64
            /// </summary>
            SINT64 = 0x800,
            /// <summary>
            /// Equivalent to System.Double
            /// </summary>
            FLOAT32 = 0x900,
            /// <summary>
            /// Equivalent to System.Boolean
            /// </summary>
            BOOL = 0xB00,
            /// <summary>
            /// Equivalent to System.String
            /// </summary>
            STRING = 0xC00
        }

        /// <summary>
        /// Lookup the data type from a string representation.
        /// </summary>
        /// <param name="type">Property data type name</param>
        /// <returns>The corresponding PropertyDataType</returns>
        public static PropertyDataType LookupDataType(string type) {
            switch (type.ToUpper()) {
                case "UINT8":
                    return PropertyDataType.UINT8;
                case "UINT16":
                    return PropertyDataType.UINT16;
                case "UINT32":
                    return PropertyDataType.UINT32;
                case "SINT32":
                    return PropertyDataType.SINT32;
                case "SINT64":
                    return PropertyDataType.SINT64;
                case "FLOAT32":
                    return PropertyDataType.FLOAT32;
                case "BOOL":
                    return PropertyDataType.BOOL;
                case "STRING":
                    return PropertyDataType.STRING;
                default:
                    return PropertyDataType.UNKNOWN;
            }
        }
        /// <summary>
        /// Lookup the string representation of a PropertyDataType
        /// </summary>
        /// <param name="dt">Property data type</param>
        /// <returns>A string representation</returns>
        public static string LookupDataTypeName(PropertyDataType dt) {
            switch (dt) {
                case PropertyDataType.UNKNOWN:
                    return "UNKNOWN";
                case PropertyDataType.UINT8:
                    return "UINT8";
                case PropertyDataType.UINT16:
                    return "UINT16";
                case PropertyDataType.UINT32:
                    return "UINT32";
                case PropertyDataType.SINT32:
                    return "SINT32";
                case PropertyDataType.SINT64:
                    return "SINT64";
                case PropertyDataType.FLOAT32:
                    return "FLOAT32";
                case PropertyDataType.BOOL:
                    return "BOOL";
                case PropertyDataType.STRING:
                    return "STRING";
                default:
                    return string.Empty;
            }
        }
        /// <summary>
        /// Lookup the length in bytes of a PropertyDataType.
        /// </summary>
        /// <param name="dt">Property data type</param>
        /// <returns>Length in bytes</returns>
        public static int LookupDataTypeLength(PropertyDataType dt) {
            switch (dt) {
                case PropertyDataType.UINT8:
                case PropertyDataType.STRING:
                case PropertyDataType.BOOL:
                    return 1;

                case PropertyDataType.UINT16:
                    return 2;

                case PropertyDataType.UINT32:
                case PropertyDataType.SINT32:
                case PropertyDataType.FLOAT32:
                    return 4;

                case PropertyDataType.SINT64:
                    return 8;

                case PropertyDataType.UNKNOWN:
                default:
                    return 0;
            }
        }
    }
}