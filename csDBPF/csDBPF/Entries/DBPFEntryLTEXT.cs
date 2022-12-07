using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace csDBPF.Entries {
	//https://wiki.sc4devotion.com/index.php?title=LTEXT


	public class DBPFEntryLTEXT : DBPFEntry {
		private bool _isDecoded;

		private string _text;
		/// <summary>
		/// Text string for this entry.
		/// </summary>
		public string Text {
			get { return _text; }
			set {
				_text = value;
				ByteData = EncodeEntry(value);
			}
		}



		public DBPFEntryLTEXT(DBPFTGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {
			_text = null;
			_isDecoded = false;
		}


		//TODO - previously this  Items in this file will not be exposed outside of this assembly.
		/// <summary>
		/// Decodes the LTEXT string from raw data and sets the <see cref="Text"/> property of this instance. Data must be uncompressed.
		/// </summary>
		public override void DecodeEntry() {
			if (_isDecoded) {
				return;
			}
			if (ByteData.Length < 4) {
				_text = null;
			}

			int pos = 0;
			ushort numberOfChars = BitConverter.ToUInt16(ByteData, pos);
			pos += 2;
			ushort textControlChar = ByteArrayHelper.ReadBytesIntoUshort(ByteData, pos);
			pos += 2;
			if (textControlChar != 0x0010) {
				_text = null;
			}

			StringBuilder sb = new StringBuilder();
			for (int idx = 0; idx < numberOfChars; idx++) {
				//Important to read two bytes to account for non english unicode characters
				sb.Append(BitConverter.ToInt16(ByteData, pos));
				pos += 2;
			}
			_text = sb.ToString();
			_isDecoded = true;
		}


		private static byte[] EncodeEntry(string text) {
			List<byte> bytes = new List<byte>();
			bytes.AddRange(BitConverter.GetBytes((ushort) text.Length)); //Number of characters
			bytes.AddRange(new byte[] { 0x00, 0x10 }); //Text control character
			bytes.AddRange(ByteArrayHelper.ToByteArray(text));
			return bytes.ToArray();
		}
	}
}
