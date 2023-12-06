using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace csDBPF.Entries {
	/// <summary>
	/// An implementation of <see cref="DBPFEntry"/> for LTEXT entries. Object data is stored in <see cref="Text"/>.
	/// </summary>
	/// <see href="https://wiki.sc4devotion.com/index.php?title=LTEXT"/>
	public class DBPFEntryLTEXT : DBPFEntry {
		/// <summary>
		/// Stores if this entry has been decoded yet.
		/// </summary>
		private bool _isDecoded;

		private string _text;
		/// <summary>
		/// Text string for this entry.
		/// </summary>
		public string Text {
			get { return _text; }
			set {
				_text = value;
			}
		}



		/// <summary>
		/// Create a new instance. Use when creating a new LTEXT entry.
		/// </summary>
		public DBPFEntryLTEXT() : base(DBPFTGI.LTEXT) { }

		/// <summary>
		/// Create a new instance with the specified text. Use when creating a new LTEXT entry from scratch.
		/// </summary>
		/// <param name="text">Text to set</param>
		public DBPFEntryLTEXT(string text) : base(DBPFTGI.LTEXT) { 
			_text= text;
		}

		/// <summary>
		/// Create a new instance with the specified TGI. Use when creating a new LTEXT entry from scratch.
		/// </summary>
		/// <param name="tgi">TGI set to assign</param>
		public DBPFEntryLTEXT(DBPFTGI tgi) : base(tgi) {
			if (tgi is null) {
				TGI.SetTGI(DBPFTGI.LTEXT);
			}
		}

		/// <summary>
		/// Create a new instance with the specified TGI and text. Use when creating a new LTEXT entry from scratch.
		/// </summary>
		/// <param name="tgi">TGI set to assign</param>
		/// <param name="text">Text to set</param>
		public DBPFEntryLTEXT(DBPFTGI tgi, string text) : base(tgi) {
			if (tgi is null) {
				TGI.SetTGI(DBPFTGI.LTEXT);
			}
			_text = text;
		}

		/// <summary>
		/// Create a new instance. Use when reading an existing entry from a file.
		/// </summary>
		/// <param name="tgi"><see cref="DBPFTGI"/> object representing the entry</param>
		/// <param name="offset">Offset (location) of the entry within the DBPF file</param>
		/// <param name="size">Compressed size of data for the entry, in bytes. Uncompressed size is also temporarily set to this to this until the data is set</param>
		/// <param name="index">Entry position in the file, 0-n</param>
		/// <param name="bytes">Byte data for this entry</param>
		public DBPFEntryLTEXT(DBPFTGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {
			_text = null;
			_isDecoded = false;
		}
        /// <summary>
        /// Create a new instance. Use when reading an existing entry from a file.
        /// </summary>
        /// <param name="tgi"><see cref="TGI"/> object representing the entry</param>
        /// <param name="offset">Offset (location) of the entry within the DBPF file</param>
        /// <param name="size">Compressed size of data for the entry, in bytes. Uncompressed size is also temporarily set to this to this until the data is set</param>
        /// <param name="index">Entry position in the file, 0-n</param>
        /// <param name="bytes">Byte data for this entry</param>
        public DBPFEntryLTEXT(TGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {
            _text = null;
            _isDecoded = false;
        }



        /// <summary>
        /// Decodes the LTEXT string from raw data and sets the <see cref="Text"/> property of this instance.
        /// </summary>
        /// <remarks>
        /// Data must be uncompressed or garbage data is returned.
        /// </remarks>
        public override void DecodeEntry() {
			if (_isDecoded) {
				return;
			}
			if (ByteData.Length < 4) {
				_text = null;
				LogMessage("Data length is less than 4 bytes so information can be read.");
			}

			int pos = 0;
			ushort numberOfChars = BitConverter.ToUInt16(ByteData, pos);
			pos += 2;
			ushort textControlChar = ByteArrayHelper.ReadBytesIntoUshort(ByteData, pos);
			if (textControlChar != 0x0010) {
				_text = null;
				LogMessage("Invalid control character. Text not set.");
				return;
			}
			pos += 2;

			StringBuilder sb = new StringBuilder();
			for (int idx = 0; idx < numberOfChars; idx++) {
				//Important to read two bytes to account for non English Unicode characters
				int twoBytes = BitConverter.ToInt16(ByteData, pos);
				sb.Append(Convert.ToChar(twoBytes));
				pos += 2;
			}
			_text = sb.ToString();
			_isDecoded = true;
		}



		/// <summary>
		/// Build <see cref="DBPFEntry.ByteData"/> from the current state of this instance.
		/// </summary>
		public override void ToBytes() {
			List<byte> bytes = new List<byte>();
			if (_text is null) {
				bytes.AddRange(BitConverter.GetBytes((ushort) 0)); //Number of characters
			} else {
				bytes.AddRange(BitConverter.GetBytes((ushort) _text.Length)); //Number of characters
			}
			bytes.AddRange(new byte[] { 0x00, 0x10 }); //Text control character
			bytes.AddRange(ByteArrayHelper.ToBytes(_text, false));
			ByteData = bytes.ToArray();
		}
	}
}
