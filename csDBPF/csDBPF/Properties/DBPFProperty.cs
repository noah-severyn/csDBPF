using System;
using System.Collections;

namespace csDBPF
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
        /// Determining the count partially depends on the encoding type:
        /// <list type="bullet">
        /// <item>For binary encoded string type: <c>length of string</c>. For text encoded string type: <c>always 1</c>.</item>
        /// <item>For binary encoded number types (all types) and text encoded number types (except float): <c>0</c> reps = single value, <c>1</c> rep = possible array of values that only contains a single value (problematic on macOS when the DataType is float), or <c>n</c> reps = n number of values.</item>
        /// <item>For text encoded float type: <c>n</c> reps = n number of values.</item>
        /// </list>
        /// This property is necessary because of uneven implementation of the DataValues property in implementing types.
        /// </remarks>
        public abstract int NumberOfReps { get; private protected set; }

		/// <summary>
		/// Specifies the encoding style (Binary or Text) of the property.
		/// </summary>
		/// <remarks>
		/// May affect implementation of other fields, namely <see cref="NumberOfReps"/>. Property is presented so the default value (false) will be binary encoding which we want to use most of the time.
		/// </remarks>
		public abstract DBPF.Encoding Encoding { get; set; }


        /// <summary>
        /// Returns the values(s) stored in this property.
        /// </summary>
        /// <returns>An array of string, long, or float values. This is specified via the <see cref="DataType"/> property</returns>
        [Obsolete("Use .GetTypedData instead, which returns the data as an exact cast of this items data type, instead of just long/string/float.")]
        public abstract IEnumerable GetData();

        /// <summary>
        /// Returns the value stored in this property at the given position.
        /// </summary>
        /// <returns>A single string, long, or float value.  This is specified via the <see cref="DataType"/> property</returns>
        [Obsolete("Use .GetTypedData instead, which returns the data as an exact cast of this items data type, instead of just long/string/float.")]
        public abstract object GetData(int position);

        /// <summary>
        /// Returns the value(s) stored in this property cast to the exact CLR type defined by <see cref="DataType"/>.
        /// </summary>
        /// <returns>
        /// An array whose element type matches <see cref="DataType"/>:
        /// <list type="bullet">
        /// <item><see cref="PropertyDataType.UINT8"/> → <c>[byte]</c></item>
        /// <item><see cref="PropertyDataType.UINT16"/> → <c>[ushort]</c></item>
        /// <item><see cref="PropertyDataType.UINT32"/> → <c>[uint]</c></item>
        /// <item><see cref="PropertyDataType.SINT32"/> → <c>[int]</c></item>
        /// <item><see cref="PropertyDataType.SINT64"/> → <c>[long]</c></item>
        /// <item><see cref="PropertyDataType.BOOL"/> → <c>[bool]</c></item>
        /// <item><see cref="PropertyDataType.FLOAT32"/> → <c>[float]</c></item>
        /// <item><see cref="PropertyDataType.STRING"/> → <c>[char]</c></item>
        /// </list> 
        /// </returns>
        public abstract Array GetTypedData();

        /// <summary>
        /// Returns the value stored in this property at the given position cast to the exact CLR type defined by <see cref="DataType"/>.
        /// </summary>
        /// <param name="position">Position (or rep) to return.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="position"/> is less than zero.</exception>
        /// <returns>
        /// A single value whose type matches <see cref="DataType"/>:
        /// <list type="bullet">
        /// <item><see cref="PropertyDataType.UINT8"/> → <c>byte</c></item>
        /// <item><see cref="PropertyDataType.UINT16"/> → <c>ushort</c></item>
        /// <item><see cref="PropertyDataType.UINT32"/> → <c>uint</c></item>
        /// <item><see cref="PropertyDataType.SINT32"/> → <c>int</c></item>
        /// <item><see cref="PropertyDataType.SINT64"/> → <c>long</c></item>
        /// <item><see cref="PropertyDataType.BOOL"/> → <c>bool</c></item>
        /// <item><see cref="PropertyDataType.FLOAT32"/> → <c>float</c></item>
        /// <item><see cref="PropertyDataType.STRING"/> → <c>char</c></item>
        /// </list>
        /// </returns>
        /// <remarks>If <paramref name="position"/> exceeds the number of values in this property, the last value is returned.</remarks>
        public abstract object GetTypedData(int position);


        /// <summary>
        /// Set the value(s) stored in this property. The value must match the exact CLR type defined by <see cref="DataType"/>:
        /// <list type="bullet">
        /// <item><see cref="PropertyDataType.UINT8"/> - <c>[byte]</c></item>
        /// <item><see cref="PropertyDataType.UINT16"/> - <c>[ushort]</c></item>
        /// <item><see cref="PropertyDataType.UINT32"/> - <c>[uint]</c></item>
        /// <item><see cref="PropertyDataType.SINT32"/> - <c>[int]</c></item>
        /// <item><see cref="PropertyDataType.SINT64"/> - <c>[long]</c></item>
        /// <item><see cref="PropertyDataType.BOOL"/> - <c>[bool]</c></item>
        /// <item><see cref="PropertyDataType.FLOAT32"/> - <c>[float]</c></item>
        /// <item><see cref="PropertyDataType.STRING"/> - <c>[char]</c></item>
        /// </list>
        /// </summary>
        /// <param name="value">Values to set, typed to match <see cref="DataType"/></param>
        /// <exception cref="ArgumentException">If the enumerable contains a data type that does not match <see cref="DataType"/>.</exception>
        public abstract void SetTypedData(Array value);


        /// <summary>
        /// Set the values(s) stored in this property.
        /// </summary>
        /// <param name="value">A string or set of numeric value(s)</param>
        [Obsolete("Use .GetTypedData instead, which returns the data as an exact cast of this items data type, instead of just long/string/float.")]
        public abstract void SetData(IEnumerable value);
        /// <summary>
        /// Set the values(s) stored in this property.
        /// </summary>
        /// <remarks>
		/// This override is necessary when countOfReps = 1; otherwise, if passed a list of length 1 then the number of reps would be set to 0. Figuring the byte offset for the next property will then be off by 4 because the extra 4 bytes representing the number of reps will be ignored. This is only necessary for long-type properties.
		/// </remarks>
        [Obsolete("Use .SetTypedData instead, which validates the input data is of the exact type of this data type, instead of just long/string/float.")]
        internal abstract void SetData(IEnumerable value, uint countOfReps);

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