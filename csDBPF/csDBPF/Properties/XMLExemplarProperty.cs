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
		private readonly uint _id;
		/// <summary>
		/// Property ID. Required.
		/// </summary>
		public uint ID {
			get { return _id; }
		}
		private readonly string _name;
		/// <summary>
		/// Property name. Required.
		/// </summary>
		public string Name {
			get { return _name; }
		}
		private readonly DBPFPropertyDataType _type;
		/// <summary>
		/// Property data type. Required.
		/// </summary>
		public DBPFPropertyDataType Type {
			get { return _type; }
		}
		private readonly bool _showAsHex;
		/// <summary>
		/// Status if this property should be shown as a hexadecimal number. Required.
		/// </summary>
		public bool ShowAsHex {
			get { return _showAsHex; }
		}

		//These parameters are optional
		//<xs:attribute name = "Count" type="xs:short" use="optional" />
		//<xs:attribute name = "Default" type="xs:string" use="optional" />
		//<xs:attribute name = "MinLength" type="xs:string" use="optional" />
		//<xs:attribute name = "MaxLength" type="xs:string" use="optional" />
		//<xs:attribute name = "MinValue" type="xs:string" use="optional" />
		//<xs:attribute name = "MaxValue" type="xs:string" use="optional" />
		//<xs:attribute name = "Step" type="xs:string" use="optional" />
		private readonly short? _count;
		/// <summary>
		/// Count of values required for this property. Optional.
		/// </summary>
		public short? Count {
			get { return _count; }
		}
		private readonly List<string> _defaultValue;
		/// <summary>
		/// Defatule value(s) for this property. Optional.
		/// </summary>
		public List<string> DefaultValue {
			get { return _defaultValue; }
		}
		private int? _minLength;
		/// <summary>
		/// Minimum number of values for this property. Optional.
		/// </summary>
		public int? MinLength {
			get { return _minLength; }
			set { _minLength = value; }
		}
		private readonly int? _maxLength;
		/// <summary>
		/// Maximum number of values for this property. Optional.
		/// </summary>
		public int? MaxLength {
			get { return _maxLength; }
		}
		private readonly string _minValue;
		/// <summary>
		/// Minimum value for each value of this property. Optional
		/// </summary>
		public string MinValue {
			get { return _minValue; }
		}
		private readonly string _maxValue;
		/// <summary>
		/// Maximum value for each value of this property. Optional
		/// </summary>
		public string MaxValue {
			get { return _maxValue; }
		}
		private readonly uint? _step;
		public uint? Step {
			get { return _step; }
		}



		/// <summary>
		/// Create a DBPFExemplarProperty, setting all required parameters to null. All optional parameters are null.
		/// </summary>
		internal XMLExemplarProperty() {
			_id = 0;
			_name = null;
			_type = null;
			_showAsHex = true;
		}
		/// <summary>
		/// Create a DBPFExemplarProperty, setting all required parameters to the specified values. All optional parameters are null.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="showAsHex"></param>
		private XMLExemplarProperty(uint id, string name, DBPFPropertyDataType type, bool showAsHex) {
			_id = id;
			_name = name;
			_type = type;
			_showAsHex = showAsHex;
		}
		/// <summary>
		/// Create a DBPFExemplarProperty, setting all required and optional parameters to the specified values.
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
		/// <param name="maxValue">Maximum allowed value<</param>
		/// <param name="step"></param>
		internal XMLExemplarProperty(uint id, string name, DBPFPropertyDataType type, bool showAsHex, short? count = null, string defaultValue = null, int? minLength = null, int? maxLength = null, string minValue = null, string maxValue = null, uint? step = null) {
			_id = id;
			_name = name;
			_type = type;
			_showAsHex = showAsHex;
			_count = count;
			//TODO - figure exactly what the Count property means, especially for negative numbers
			//if count is not present, it is assumed to be one.
			//if it is specified, it must be whatever that number is
			//if count is -1, can have any number of values
			//if count is -2, paired list of values, any length
			//if count is < -3, unsure? can have any number of reps up to that number? e.g. -8 can have 1-8 values?
			if (defaultValue == null) {
				_defaultValue = null;
			} else {
				_defaultValue = new List<string>(defaultValue.Split(" "));
			}

			_minLength = minLength;
			_maxLength = maxLength;
			_minValue = minValue;
			_maxValue = maxValue;
			_step = step;
		}



		/// <summary>
		/// Returns a string that represents the current instance.
		/// </summary>
		/// <returns>A string that represents the current instance</returns>
		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.Append($"ID: {_id}, ");
			sb.Append($"Name: {_name}, ");
			sb.Append($"Type: {_type}, ");
			sb.Append($"ShowAsHex: {_showAsHex}, ");
			sb.Append($"Count: {_count}, ");
			sb.Append($"Default: {_defaultValue}, ");
			sb.Append($"MinLen: {_minLength}, ");
			sb.Append($"MaxLen: {_maxLength}, ");
			sb.Append($"MinVal: {_minValue}, ");
			sb.Append($"MaxVal: {_maxValue}, ");
			sb.Append($"Step: {_step}, ");
			return sb.ToString();
		}
	}
}
