using System;
using System.Collections.Generic;
using System.Text;
using csDBPF.Properties;

namespace csDBPF {
	/// <summary>
	/// This is the second half of the class where all of the static decoding functions are stored. It's split up purely for organizational purposes to make reading the code easier. Items in this file will not be exposed outside of this assembly.
	/// </summary>
	public partial class DBPFEntry {
		/// <summary>
		/// Decodes the compressed data into a dictionary of one or more <see cref="DBPFProperty"/>.
		/// </summary>
		/// <param name="cData">Compressed data</param>
		/// <returns>Dictionary of <see cref="DBPFProperty"/> indexed by their order in the entry</returns>
		internal static Dictionary<int, DBPFProperty> DecodeEntry_EXMP(byte[] cData) {
			byte[] dData;
			if (DBPFCompression.IsCompressed(cData)) {
				dData = DBPFCompression.Decompress(cData);
			} else {
				dData = cData;
			}

			Dictionary<int, DBPFProperty> listOfProperties = new Dictionary<int, DBPFProperty>();

			//Read cohort TGI info and determine the number of properties in this entry
			uint parentCohortTID;
			uint parentCohortGID;
			uint parentCohortIID;
			uint propertyCount;
			int pos; //Offset position in dData. Initialized to the starting position of the properties after the header data
			switch (GetEncodingType(dData)) {
				case 1: //Binary encoding
					parentCohortTID = BitConverter.ToUInt32(dData, 8);
					parentCohortGID = BitConverter.ToUInt32(dData, 12);
					parentCohortIID = BitConverter.ToUInt32(dData, 16);
					propertyCount = BitConverter.ToUInt32(dData, 20);
					pos = 24;
					break;
				case 2: //Text encoding
					parentCohortTID = ByteArrayHelper.ReadTextIntoUint(dData, 30);
					parentCohortGID = ByteArrayHelper.ReadTextIntoUint(dData, 41);
					parentCohortIID = ByteArrayHelper.ReadTextIntoUint(dData, 52);
					propertyCount = ByteArrayHelper.ReadTextIntoUint(dData, 75);
					pos = 85;
					break;
				default:
					propertyCount = 0;
					pos = 0;
					break;
			}

			//Create the Property
			DBPFProperty property;
			for (int idx = 0; idx < propertyCount; idx++) {
				property = DBPFProperty.DecodeProperty(dData, pos);
				listOfProperties.Add(idx, property);

				//Determine which bytes to skip to get to the start of the next property
				switch (GetEncodingType(dData)) {
					case 1: //Binary encoding
						pos += property.ByteValues.Length + 9; //Additionally skip the 4 bytes for ID, 2 for DataType, 2 for KeyType, 1 unused byte
						if (property.KeyType == 0x80) { //Skip 4 more for NumberOfValues
							pos += 4;
						}
						break;
					case 2: //Text encoding
						pos = ByteArrayHelper.FindNextInstanceOf(dData, 0x0A, pos) + 1;
						break;
				}
			}

			return listOfProperties;
		}


		/// <summary>
		/// Decodes the LTEXT string from raw data. Data is not compressed.
		/// </summary>
		/// <param name="data">Raw data of the LTEXT entry (not compressed)</param>
		/// <returns>A string</returns>
		internal static byte[] DecodeEntry_LTEXT(byte[] data) {
			int pos = 0;
			ushort numberOfChars = BitConverter.ToUInt16(data, pos);
			pos += 2;
			ushort textControlChar = ByteArrayHelper.ReadBytesIntoUshort(data, pos);
			pos += 2;
			if (textControlChar != 0x0010) {
				throw new ArgumentException("Data is not valid LTEXT format!");
			}

			StringBuilder sb = new StringBuilder();
			for (int idx = 0; idx < numberOfChars; idx++) {
				sb.Append(BitConverter.ToChar(data, pos));
				pos += 2;
			}
			return ByteArrayHelper.ToByteArray(sb.ToString());
		}




	}
}
