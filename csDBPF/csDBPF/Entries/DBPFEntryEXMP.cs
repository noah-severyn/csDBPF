using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using csDBPF.Properties;

namespace csDBPF.Entries {
	internal class DBPFEntryEXMP : DBPFEntry {

		private List<DBPFProperty> _listOfProperties;
		/// <summary>
		/// List of one or more <see cref="DBPFProperty"/> associated with this entry.
		/// </summary>
		public List<DBPFProperty> ListOfProperties {
			get { return _listOfProperties; }
			set { _listOfProperties = value; }
		}

		public DBPFTGI ParentCohort { get; set; }


		public DBPFEntryEXMP(DBPFTGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {
			_listOfProperties = null;
		}


		/// <summary>
		/// Decodes the compressed data into a dictionary of one or more <see cref="DBPFProperty"/>.
		/// </summary>
		/// <returns>Dictionary of <see cref="DBPFProperty"/> indexed by their order in the entry</returns>
		public override void DecodeEntry() {
			byte[] cData = base.ByteData;
			byte[] dData;
			if (DBPFCompression.IsCompressed(cData)) {
				dData = DBPFCompression.Decompress(cData);
			} else {
				dData = cData;
			}

			List<DBPFProperty> listOfProperties = new List<DBPFProperty>();

			//Read cohort TGI info and determine the number of properties in this entry
			uint parentCohortTID;
			uint parentCohortGID;
			uint parentCohortIID;
			uint propertyCount;
			int pos; //Offset position in dData. Initialized to the starting position of the properties after the header data
			if (IsBinaryEncoding()) {
				parentCohortTID = BitConverter.ToUInt32(dData, 8);
				parentCohortGID = BitConverter.ToUInt32(dData, 12);
				parentCohortIID = BitConverter.ToUInt32(dData, 16);
				propertyCount = BitConverter.ToUInt32(dData, 20);
				pos = 24;
			} else {
				parentCohortTID = ByteArrayHelper.ReadTextIntoUint(dData, 30);
				parentCohortGID = ByteArrayHelper.ReadTextIntoUint(dData, 41);
				parentCohortIID = ByteArrayHelper.ReadTextIntoUint(dData, 52);
				propertyCount = ByteArrayHelper.ReadTextIntoUint(dData, 75);
				pos = 85;
			}

			//Create the Property
			DBPFProperty property;
			for (int idx = 0; idx < propertyCount; idx++) {
				property = DBPFProperty.DecodeProperty(dData, pos);
				listOfProperties.Add(property);

				//Determine which bytes to skip to get to the start of the next property
				if (IsBinaryEncoding()) {
					pos += property.ByteValues.Length + 9; //Additionally skip the 4 bytes for ID, 2 for DataType, 2 for KeyType, 1 unused byte
					if (property.KeyType == 0x80) { //Skip 4 more for NumberOfValues
						pos += 4;
					}
				} else {
					pos = ByteArrayHelper.FindNextInstanceOf(dData, 0x0A, pos) + 1;
				}
			}

			_listOfProperties = listOfProperties;
		}



		/// <summary>
		/// Returns the encoding type of this entry.
		/// </summary>
		/// <returns>TRUE if binary encoding; FALSE otherwise</returns>
		/// <remarks>This will decompress the data if it is not already uncompressed.</remarks>
		public bool IsBinaryEncoding() {
			if (IsCompressedNow) {
				ByteData = DBPFCompression.Decompress(ByteData);
				IsCompressedNow = false;
			}

			string fileIdentifier = ByteArrayHelper.ToAString(ByteData, 0, 4);
			return fileIdentifier.Substring(fileIdentifier.Length - 1) == "B";
		}



		/// <summary>
		/// Decode all properties in the entry. Only valid for Exemplar/Cohort type entries.
		/// </summary>
		public void DecodeAllProperties() {
			if (!(TGI.MatchesKnownTGI(DBPFTGI.COHORT) || TGI.MatchesKnownTGI(DBPFTGI.EXEMPLAR))) {
				throw new InvalidOperationException("This function can only be called on Exemplar and Cohort type entries!");
			}

			foreach (DBPFProperty property in _listOfProperties) {
				property.DecodeValues();
			}
		}



		/// <summary>
		/// Gets the Exemplar Type (0x00 - 0x2B) of the property. See <see cref="DBPFProperty.ExemplarTypes"/> for the full list.
		/// </summary>
		/// <returns>Exemplar Type if found; -1 if entry is not Exemplar or Cohort; -2 if "ExemplarType" property is not found</returns>
		public int GetExemplarType() {
			if (!(TGI.MatchesKnownTGI(DBPFTGI.EXEMPLAR) || TGI.MatchesKnownTGI(DBPFTGI.COHORT))) {
				return -1;
			}
			DBPFProperty property = GetProperty(0x00000010);

			if (property is null) {
				return -2;
			}

			//TODO - WTF IS THIS SYNTAX? GROSS.
			Array propertyType = Array.CreateInstance(property.DataType.PrimitiveDataType, property.NumberOfReps); //Create new array to hold the values
			property.DecodeValues();
			propertyType = property.DecodedValues; //Set the values from the decoded property
												   //return unchecked((int) propertyType.GetValue(0)); 
												   //TODO We know exemplar type can only hold one value, so grab the first one.... BUT HOW?????????
			return Convert.ToInt32(propertyType.GetValue(0));
		}



		/// <summary>
		/// Lookup and return a property from a list of properties in the entry.
		/// </summary>
		/// <param name="idToGet">Property ID to find</param>
		/// <returns>DBPFProperty of the match if found; null otherwise</returns>
		public DBPFProperty GetProperty(uint idToGet) {
			if (_listOfProperties is null) {
				throw new InvalidOperationException("This entry must be decoded before it can be analyzed!");
			}

			foreach (DBPFProperty property in _listOfProperties) {
				if (property.ID == idToGet) {
					return property;
				}
			}
			return null;
		}
		/// <summary>
		/// Lookup and return a property from a list of properties in the entry.
		/// </summary>
		/// <param name="name">Name of property</param>
		/// <returns>DBPFProperty of the match if found; null otherwise</returns>
		/// <remarks>
		/// Lookup name is case insensitive and ignores spaces (the XML properties can be inconsistently named).
		/// </remarks>
		public DBPFProperty GetProperty(string name) {
			uint id = XMLProperties.LookupPropertyID(name);
			return GetProperty(id);
		}

		//TODO - Implement AddProperty
		//Add a property to this exemplar/cohort. This method returns false if the property is null or if another property with the same id already exists.
		public bool AddProperty(DBPFProperty prop) {
			throw new NotImplementedException();
		}

		//TODO - Implement UpdateProperty
		//Update a property to this exemplar/cohort. This method returns false if the property is null or if another property with the same id does not already exists.
		public bool UpdateProperty(DBPFProperty prop) {
			throw new NotImplementedException();
		}

		//TODO - Implement AddOrUpdateProperty
		//Add or Update a property to this exemplar/cohort. This method returns false if the property is null.
		public bool AddOrUpdateProperty(DBPFProperty prop) {
			throw new NotImplementedException();
		}

		//TODO - Implement RemoveProperty
		//Remove a property to this exemplar/cohort. This method returns false if the property is null.
		public bool RemoveProperty(uint id) {
			throw new NotImplementedException();
		}

		//TODO - Implement RemoveAllProperties
		//Removes all the properties from this cohort/exemplar.
		public bool RemoveAllProperties() {
			throw new NotImplementedException();
		}

	}
}
