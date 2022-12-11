using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csDBPF.Properties {
	/// <summary>
	/// Represents a property storing float value(s).
	/// </summary>
	public class DBPFPropertyFloat : DBPFProperty {
		private uint _id;
		/// <summary>
		/// Hexadecimal identifier for this property. <see cref="XMLExemplarProperty"/> and <see cref="XMLProperties.AllProperties"/>. 
		/// </summary>
		public override uint ID {
			get { return _id; }
			internal set { _id = value; }
		}

		private readonly DBPFPropertyDataType _dataType;
		/// <summary>
		/// The <see cref="DBPFPropertyDataType"/> for this property.
		/// </summary>
		public override DBPFPropertyDataType DataType {
			get { return _dataType; }
		}

		private int _numberOfReps;
		/// <summary>
		/// The number of repetitions of <see cref="DBPFPropertyDataType"/> this property has. This informs (in part) how many bytes to read for this property. Initialized to 0.
		/// </summary>
		/// <remarks>
		/// Determining the count partially depends on the encoding type. For binary encoded float-type properties: 0 reps = single value, 1 reps = multiple values but currently held to 1 value (problematic on macOS when the DataType is float), n reps = n number of values. For text encoded float-type properties: n reps = n number of values.
		/// </remarks>
		public override int NumberOfReps {
			get { return _numberOfReps; }
		}

		private List<float> _dataValues;



		/// <summary>
		/// Construct a new DBPFProperty with a float data type.
		/// </summary>
		public DBPFPropertyFloat() {
			_dataType = DBPFPropertyDataType.FLOAT32;
			_numberOfReps = 0;
		}
		/// <summary>
		/// Construct a DBPFProperty with a float data type holding a single value.
		/// </summary>
		/// <param name="value">Value of this property</param>
		public DBPFPropertyFloat(float value) {
			_dataType = DBPFPropertyDataType.FLOAT32;
			_dataValues = new List<float> { value };
			_numberOfReps = _dataValues.Count;
		}
		/// <summary>
		/// Construct a DBPFProperty with a float data type holding multiple values.
		/// </summary>
		/// <param name="values">Values this property holds</param>
		/// <exception cref="ArgumentException">DBPFPropertyNumber cannot contain float or string data.</exception>
		public DBPFPropertyFloat(List<float> values) {
			_dataType = DBPFPropertyDataType.FLOAT32;
			_dataValues = values;
			_numberOfReps = _dataValues.Count;
		}



		/// <summary>
		/// Returns a list of data values which are stored in this property.
		/// </summary>
		/// <returns>List of data values which are stored in this property</returns>
		public override List<float> GetDataValues() {
			return _dataValues;
		}



		/// <summary>
		/// Set the data values stored in this property.
		/// </summary>
		/// <param name="value">Values to set</param>
		/// <exception cref="ArgumentException">Argument to DBPFPropertyFloat.SetDataValues must be List&lt;float&gt;.</exception>
		public override void SetDataValues(object value) {
			if (value is not List<float>) {
				throw new ArgumentException($"Argument to DBPFPropertyFloat.SetDataValues must be List<float>. {value.GetType()} was provided.");
			}
			_dataValues = (List<float>) value;
			if (_dataValues.Count <= 1) {
				_numberOfReps = 0;
			} else {
				_numberOfReps = _dataValues.Count;
			}
		}


		public override byte[] ToRawBytes() {
			throw new NotImplementedException();
		}
	}
}
