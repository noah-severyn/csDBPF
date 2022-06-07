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
		/// <param name="cData">Compressed byte data</param>
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




		//https://wiki.sc4devotion.com/index.php?title=FSH_Format
		/// <summary>
		/// 
		/// </summary>
		/// <param name="cData">Compressed byte data</param>
		/// <returns></returns>
		internal static byte[] DecodeEntry_FSH(byte[] cData) {
			byte[] dData;
			if (DBPFCompression.IsCompressed(cData)) {
				dData = DBPFCompression.Decompress(cData);
			} else {
				dData = cData;
			}

			//analyze fsh file header
			uint identifer = BitConverter.ToUInt32(dData, 0);
			int fileSize = BitConverter.ToInt32(dData, 4);
			int entryCount = BitConverter.ToInt32(dData, 8);
			int directoryID = BitConverter.ToInt32(dData, 12);

			//analyze fsh directory
			int offset = 16;
			Dictionary<uint, int> FSHDirectory = new Dictionary<uint, int>();

			for (int entry = 0; entry < entryCount; entry++) {
				uint entryName = ByteArrayHelper.ReadBytesIntoUint(dData, offset);
				offset += 4;
				int entryOffset = BitConverter.ToInt32(dData, offset);
				offset += 4;
				FSHDirectory.Add(entryName, entryOffset);
			}

			//after the directory is built, we can look at each specific FSH entry in the file (there can be more than one)
			//parse the header

			//after header is bitmap / palette pixel / color information
			//pallets generally are 256 length arrays of 1 byte
			//bitmaps store pixel data in many ways
			//fsh images can store raw data or microsoft dxtc compressed


			//TODO - this is getting pretty complicated - it might be better to split this out into its own class along with the respective enums and FSH Header subclass. Create a new folder for filetypes (or something like that)
		}


		private class FSHHeader {
			byte RecordID;
			int BlockSize; //3 bytes only
			ushort Width;
			ushort Height;
			//ushort XCenterLoc;
			//ushort YCenterLoc;
			//ushort XOffsetLoc;
			//ushort YOffsetLoc;
			byte[] data;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <see cref="https://wiki.sc4devotion.com/index.php?title=FSH_Format#Directory_ID_Values"/>
		public enum FSH_DirectoryID {
			Building = 0x47334444, //G354 - Building Textures
			EverythingElse = 0x47323634, //G264 - Network Textures, Sim Textures, Sim heads, Sim animations, Trees, props, Base textures, Misc colors
			Animation = 0x47323636, //G266 - 3d Animation textures (e.g.the green rotating diamond in loteditor.dat)
			DispatchMarker = 0x47323930, //G290 - Dispatch marker textures
			NetworkTransportModel = 0x71333135, //G315 - Small Sim texture, Network Transport Model Textures(trains, etc.)
			UIEditor = 0x47494D58, //GIMX - UI Editor textures
			BATgen = 0x47333434 //G344 - BAT gen texture maps
		}

		public enum FSH_BitmapCode {
			EightBit_Indexed = 0x7B, //Type: 8-bit indexed, Palette: directly follows bitmap or uses global palette, Compression: none
			ThirtyTwoBit_a8r8g8b8 = 0x7D, //Type: 32-bit A8R8G8B8, Palette: none, Compression: none
			TwentyFourBit_a0r8g8b8 = 0x7F, //Type: 24-bit A0R8G8B8, Palette: none, Compression: none
			SixteenBit_a1r5g5b5 = 0x7E, //Type:	16-bit A1R5G5B5, Palette: none, Compression: none
			SixteenBit_a0r5g5b5 = 0x78, //Type:	16-bit A0R5G6B5, Palette: none, Compression: none
			SixteenBit_a4r4g4b4 = 0x6D, //Type: 16-bit A4R4G4B4, Palette: none, Compression: none
			DXT3_4x4_4bitAlpha = 0x61, //Type: DXT3 4x4 packed, 4-bit alpha, Palette: none, Compression: 4x4 grid compressed, half-byte per pixel
			DXT3_4x4_1bitAlpha = 0x60, //Type: DXT3 4x4 packed, 1-bit alpha, Palette: none, Compression: 4x4 grid compressed, half-byte per pixel
		}

		public enum FSH_PaletteCode {
			TwentyFourBitDOS = 0x22, //24-bit DOS
			TwentyFourBit = 0x24, //24-bit 
			SixteenBitNFSS = 0x29, //16-bit NFS5 
			ThirtyTwoBit = 0x2A, //32-bit 
			SixteenBit = 0x2D //16-bit
		}

		public enum FSH_TextCode {
			StandardText = 0x6F, //Standard Text file
			ArbitraryLength = 0x69, //ETXT of arbitrary length with full entry header 
			LessThan16Bytes = 0x70, //ETXT of 16 bytes or less including the header 
			PixelRegionHotspot = 0x7C //defined Pixel region Hotspot data for image.
		}
	}
}
