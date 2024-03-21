using System;
using System.Collections.Generic;
using System.Text;

namespace csDBPF.Properties {
	/// <summary>
	/// Represents a property stored in the new_properties.xml file.
	/// </summary>
	/// <remarks>
	/// This class is always returned as a result of a query to new_properties.xml amd does not ever need to be user instantiated.
	/// </remarks>
	public class XMLExemplarProperty {
		//These parameters are required
		//<xs:attribute name = "ID" type="xs:string" use="required" />
		//<xs:attribute name = "Name" type="xs:string" use="required" />
		//<xs:attribute name = "Type" type="xs:string" use="required" />
		//<xs:attribute name = "ShowAsHex" type="xs:string" use="required" />
		/// <summary>
		/// Property ID. Required.
		/// </summary>
		public uint ID { get; private set; }
		/// <summary>
		/// Property name. Required.
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// Property data type. Required.
		/// </summary>
		public DBPFProperty.PropertyDataType DataType { get; private set; }
		/// <summary>
		/// Whether this property should be shown as a hexadecimal number. Required.
		/// </summary>
		public bool ShowAsHex { get; private set; }

        //These parameters are optional
        //<xs:attribute name = "Count" type="xs:short" use="optional" />
        //<xs:attribute name = "Default" type="xs:string" use="optional" />
        //<xs:attribute name = "MinLength" type="xs:string" use="optional" />
        //<xs:attribute name = "MaxLength" type="xs:string" use="optional" />
        //<xs:attribute name = "MinValue" type="xs:string" use="optional" />
        //<xs:attribute name = "MaxValue" type="xs:string" use="optional" />
        //<xs:attribute name = "Step" type="xs:string" use="optional" />
		/// <summary>
		/// Count of values required for this property. Optional.
		/// </summary>
		public short? Count { get; private set; }
        /// <summary>
        /// Default value(s) for this property. Optional.
        /// </summary>
        public List<string> DefaultValue { get; private set; }
		/// <summary>
		/// Minimum number of values for this property. Optional.
		/// </summary>
		public int? MinLength { get; private set; }
        /// <summary>
        /// Maximum number of values for this property. Optional.
        /// </summary>
        public int? MaxLength { get; private set; }
        /// <summary>
        /// Minimum value for each value of this property. Optional
        /// </summary>
        public string MinValue { get; private set; }
        /// <summary>
        /// Maximum value for each value of this property. Optional
        /// </summary>
        public string MaxValue { get; private set; }
        /// <summary>
        /// Each value is incremented by this number. Optional.
        /// </summary>
        public uint? Step { get; private set; }



        /// <summary>
        /// Create a DBPFExemplarProperty with all required arguments set to null and all optional arguments set to null.
        /// </summary>
        internal XMLExemplarProperty() {
			ID = 0;
			Name = null;
			DataType = DBPFProperty.PropertyDataType.UNKNOWN;
			ShowAsHex = true;
		}
        /// <summary>
        /// Create a DBPFExemplarProperty with the specified required arguments and all optional arguments set to null.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="showAsHex"></param>
        internal XMLExemplarProperty(uint id, string name, DBPFProperty.PropertyDataType type, bool showAsHex) {
			ID = id;
			Name = name;
			DataType = type;
			ShowAsHex = showAsHex;
		}
        /// <summary>
        /// Create a DBPFExemplarProperty with the specified required arguments and the specified optional arguments.
        /// </summary>
        /// <param name="id">Property ID</param>
        /// <param name="name">Property Name</param>
        /// <param name="type">Property data type</param>
        /// <param name="showAsHex">Is property represented as hexadecimal</param>
        /// <param name="count">Count of reps</param>
        /// <param name="defaultValue">Default Value</param>
        /// <param name="minLength">Minimum number of properties</param>
        /// <param name="maxLength">Maximum number of properties</param>
        /// <param name="minValue">Minimum allowed value</param>
        /// <param name="maxValue">Maximum allowed value</param>
        /// <param name="step"></param>
        internal XMLExemplarProperty(uint id, string name, DBPFProperty.PropertyDataType type, bool showAsHex, short? count = null, string defaultValue = null, int? minLength = null, int? maxLength = null, string minValue = null, string maxValue = null, uint? step = null) {
			ID = id;
			Name = name;
			DataType = type;
			ShowAsHex = showAsHex;
			Count = count;
			//TODO - figure exactly what the Count property means, especially for negative numbers
			//if count is not present, it is assumed to be one.
			//if it is specified, it must be whatever that number is
			//if count is -1, can have any number of values
			//if count is -2, paired list of values, any length
			//if count is < -3, unsure? can have any number of reps up to that number? e.g. -8 can have 1-8 values?
			if (defaultValue == null) {
				DefaultValue = null;
			} else {
				DefaultValue = new List<string>(defaultValue.Split(" "));
			}

			MinLength = minLength;
			MaxLength = maxLength;
			MinValue = minValue;
			MaxValue = maxValue;
			Step = step;
		}



		/// <summary>
		/// Returns a string that represents the current instance.
		/// </summary>
		/// <returns>A string that represents the current instance</returns>
		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.Append($"ID: {ID}, ");
			sb.Append($"Name: {Name}, ");
			sb.Append($"Type: {DataType}, ");
			sb.Append($"ShowAsHex: {ShowAsHex}, ");
			sb.Append($"Count: {Count}, ");
			sb.Append($"Default: {DefaultValue}, ");
			sb.Append($"MinLen: {MinLength}, ");
			sb.Append($"MaxLen: {MaxLength}, ");
			sb.Append($"MinVal: {MinValue}, ");
			sb.Append($"MaxVal: {MaxValue}, ");
			sb.Append($"Step: {Step}, ");
			return sb.ToString();
		}
	}
}
