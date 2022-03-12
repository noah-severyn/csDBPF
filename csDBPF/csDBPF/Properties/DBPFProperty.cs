using System;
using System.Collections.Generic;
using System.Text;
using csDBPF.Properties;

namespace csDBPF.Properties {
	public abstract class DBPFProperty {
		private uint _id;
		protected uint id {
			get { return _id; }
			set { _id = value; }
		}

		private int _count;
		protected int count {
			get { return _count; }
			set { _count = value; }
		}

		private DBPFPropertyDataType _dataType;
		protected DBPFPropertyDataType dataType {
			get { return _dataType; }
			set { _dataType = value; }
		}

		//TODO - figure out what this is and what it is used for
		protected Object _values;

		protected DBPFProperty(DBPFPropertyDataType dataType) {
			_dataType = dataType;
			_id = 0;
			_count = 0;
			_values = new Object();
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.Append($"ID: {_id}, ");
			sb.Append($"Type: {_dataType}, ");
			sb.Append($"Reps: {_count}, ");
			//TODO - add representation of _values here
			return sb.ToString();
		}



	}
}
