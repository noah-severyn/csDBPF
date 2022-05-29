using System;
using System.Collections.Generic;
using System.Text;

namespace csDBPF.Properties {
	public class XMLExemplarProperty {

		//------------- XMLExemplarProperty Fields ------------- \\

		//These parameters are required
		//<xs:attribute name = "ID" type="xs:string" use="required" />
		//<xs:attribute name = "Name" type="xs:string" use="required" />
		//<xs:attribute name = "Type" type="xs:string" use="required" />
		//<xs:attribute name = "ShowAsHex" type="xs:string" use="required" />
		private readonly uint _id;
		public uint ID {
			get { return _id; }
		}
		private readonly string _name;
		public string Name {
			get { return _name; }
		}
		private readonly DBPFPropertyDataType _type;
		public DBPFPropertyDataType Type {
			get { return _type; }
		}
		private readonly bool _showAsHex;
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
		public short? Count {
			get { return _count; }
		}
		private readonly List<string> _defaultValue;
		public List<string> DefaultValue {
			get { return _defaultValue; }
		}
		private int? _minLength;
		public int? MinLength {
			get { return _minLength; }
			set { _minLength = value; }
		}
		private readonly int? _maxLength;
		public int? MaxLength {
			get { return _maxLength; }
		}
		private readonly string _minValue;
		public string MinValue {
			get { return _minValue; }
		}
		private readonly string _maxValue;
		public string MaxValue {
			get { return _maxValue; }
		}
		private readonly uint? _step;
		public uint? Step {
			get { return _step; }
		}




		//------------- XMLExemplarProperty Constructors ------------- \\
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
		internal XMLExemplarProperty(uint id, string name, DBPFPropertyDataType type, bool showAsHex) {
			_id = id;
			_name = name;
			_type = type;
			_showAsHex = showAsHex;
		}
		/// <summary>
		/// Create a DBPFExemplarProperty, setting all required and optional parameters to the specified values.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="showAsHex"></param>
		/// <param name="count"></param>
		/// <param name="defaultValue"></param>
		/// <param name="minLength"></param>
		/// <param name="maxLength"></param>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>
		/// <param name="step"></param>
		public XMLExemplarProperty(uint id, string name, DBPFPropertyDataType type, bool showAsHex, short? count = null, string defaultValue = null, int? minLength = null, int? maxLength = null, string minValue = null, string maxValue = null, uint? step = null) {
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




		//------------- XMLExemplarProperty Methods ------------- \\
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
