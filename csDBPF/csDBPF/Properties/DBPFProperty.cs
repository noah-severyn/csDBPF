using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Collections;
using csDBPF.Entries;

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
		/// The <see cref="DBPFPropertyDataType"/> for this property.
		/// </summary>
		public abstract DBPFPropertyDataType DataType { get; }

		/// <summary>
		/// The number of repetitions of <see cref="DBPFPropertyDataType"/> this property has. This informs (in part) how many bytes to read for this property. Initialized to 0.
		/// </summary>
		/// <remarks>
		/// Determining the count partially depends on the encoding type. For binary encoded string type: length of string. For text encoded string type: always 1. For binary encoded (all) and text encoded number types (except float): 0 reps = single value, 1 reps = multiple values but currently held to 1 value (problematic on macOS when the DataType is float), n reps = n number of values. For text encoded float type: n reps = n number of values. This property is necessary because of uneven implementation of the DataValues property in implementing types.
		/// </remarks>
		public abstract int NumberOfReps { get; }

		/// <summary>
		/// Specifies the encoding style (Binary or Text) of the property.
		/// </summary>
		/// <remarks>
		/// May affect implementation of other fields, namely <see cref="NumberOfReps"/>. Property is presented so the default value (false) will be binary encoding which we want to use most of the time.
		/// </remarks>
		public abstract bool IsTextEncoding { get; set; }

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
        public abstract byte[] ToRawBytes();
	}
}