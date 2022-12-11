using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace csDBPF.Properties {
	/// <summary>
	/// Represents a property storing integer-based value(s).
	/// </summary>
	/// <remarks>
	/// All numbers are stored internally as long (equal to largest used DBPFPropertyDataType of SINT64). The actual underlying data type is defined by the <see cref="DBPFPropertyDataType"/>.
	/// </remarks>
	public class DBPFPropertyNumber : DBPFProperty {
		private uint _id;
		/// <summary>
		/// Hexadecimal identifier for this property. <see cref="XMLExemplarProperty"/> and <see cref="XMLProperties.AllProperties"/>. 
		/// </summary>
		public override uint ID {
			get { return _id; }
			internal set { _id = value; }
		}

		private DBPFPropertyDataType _dataType;
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
		/// 0 reps = single value; n reps = n number of values.
		/// </remarks>
		public override int NumberOfReps {
			get { return _numberOfReps; }
		}

		private List<long> _dataValues;



		/// <summary>
		/// Construct a DBPFProperty with a numerical data type.
		/// </summary>
		/// <param name="dataType">Data type of this property</param>
		/// <exception cref="ArgumentException">DBPFPropertyNumber cannot contain float or string data.</exception>
		public DBPFPropertyNumber(DBPFPropertyDataType dataType) {
			if (dataType == DBPFPropertyDataType.FLOAT32 || dataType == DBPFPropertyDataType.STRING) {
				throw new ArgumentException("DBPFPropertyNumber cannot contain float or string data.");
			}
			_dataType = dataType;
			_dataValues = null;
			_numberOfReps = 0;
		}
		/// <summary>
		/// Construct a DBPFProperty with a numerical data type holding a single value.
		/// </summary>
		/// <param name="dataType">Data type of this property</param>
		/// <param name="value">Value of this property</param>
		/// <exception cref="ArgumentException">DBPFPropertyNumber cannot contain float or string data.</exception>
		public DBPFPropertyNumber(DBPFPropertyDataType dataType, long value) {
			if (dataType == DBPFPropertyDataType.FLOAT32 || dataType == DBPFPropertyDataType.STRING) {
				throw new ArgumentException("DBPFPropertyNumber cannot contain float or string data.");
			}
			_dataType = dataType;
			_dataValues = new List<long> { value };
			_numberOfReps = _dataValues.Count;
		}
		/// <summary>
		/// Construct a DBPFProperty with a numerical data type holding multiple values.
		/// </summary>
		/// <param name="dataType">Data type of this property</param>
		/// <param name="values">Values this property holds</param>
		/// <exception cref="ArgumentException">DBPFPropertyNumber cannot contain float or string data.</exception>
		public DBPFPropertyNumber(DBPFPropertyDataType dataType, List<long> values) {
			if (dataType == DBPFPropertyDataType.FLOAT32 || dataType == DBPFPropertyDataType.STRING) {
				throw new ArgumentException("DBPFPropertyNumber cannot contain float or string data.");
			}
			_dataType = dataType;
			_dataValues = values;
			_numberOfReps = _dataValues.Count;
		}



		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>Returns a string that represents the current object.</returns>
		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.Append($"ID: 0x{DBPFUtil.UIntToHexString(_id)}, ");
			sb.Append($"Type: { _dataType}, ");
			sb.Append($"Reps: {_numberOfReps}, ");
			sb.Append($"Values: {_dataValues.ToString()}");
			return sb.ToString();
		}



		/// <summary>
		/// Returns a list of data values which are stored in this property.
		/// </summary>
		/// <returns>List of data values which are stored in this property</returns>
		public override List<long> GetDataValues() {
			return _dataValues;
		}



		/// <summary>
		/// Set the data values stored in this property.
		/// </summary>
		/// <param name="value">Values to set</param>
		/// <exception cref="ArgumentException">Argument to DBPFPropertyNumber.SetDataValues must be List&lt;long&gt;.</exception>
		public override void SetDataValues(object value) {
			if (value is not List<long>) {
				throw new ArgumentException($"Argument to DBPFPropertyNumber.SetDataValues must be List<long>. {value.GetType()} was provided.");
			}
			_dataValues= (List<long>)value;
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
