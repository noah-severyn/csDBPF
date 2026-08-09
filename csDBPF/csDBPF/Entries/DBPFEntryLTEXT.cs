using System;
using System.Collections.Generic;
using System.Text;

namespace csDBPF {
	/// <summary>
	/// An implementation of <see cref="DBPFEntry"/> for LTEXT entries. Object data is stored in <see cref="Text"/>.
	/// </summary>
	/// <see href="https://wiki.sc4devotion.com/index.php?title=LTEXT"/>
	public class DBPFEntryLTEXT : DBPFEntry {
		private string _text;
		/// <summary>
		/// Text string for this entry.
		/// </summary>
		public string Text {
			get { return _text; }
            set {
				_text = value;
                UncompressedSize = (uint) _text.Length * 2 + 4;
            }
		}



		/// <summary>
		/// Create a new instance. Use when creating a new LTEXT entry.
		/// </summary>
		public DBPFEntryLTEXT() : base(DBPFTGI.LTEXT) {
            _text = string.Empty;
        }

        /// <summary>
        /// Create a new instance with the specified TGI. Use when creating a new LTEXT entry from scratch.
        /// </summary>
        /// <param name="tgi">TGI set to assign</param>
        public DBPFEntryLTEXT(TGI tgi) : base(tgi) {
			_text = string.Empty;
		}

        /// <summary>
        /// Create a new instance with the specified text. Use when creating a new LTEXT entry from scratch.
        /// </summary>
        /// <param name="text">Text to set</param>
        public DBPFEntryLTEXT(string text) : base(DBPFTGI.LTEXT) { 
			_text= text;
            IsCompressed = false;
            UncompressedSize = (uint) text.Length * 2 + 4;
        }

		/// <summary>
		/// Create a new instance with the specified TGI and text. Use when creating a new LTEXT entry from scratch.
		/// </summary>
		/// <param name="tgi">TGI set to assign</param>
		/// <param name="text">Text to set</param>
		public DBPFEntryLTEXT(TGI tgi, string text) : base(tgi) {
			_text = text;
			IsCompressed = false;
			UncompressedSize = (uint) text.Length * 2 + 4;
		}

		/// <summary>
		/// Create a new instance. Use when reading an existing entry from a file.
		/// </summary>
		/// <param name="tgi"><see cref="DBPFTGI"/> object representing the entry</param>
		/// <param name="offset">Offset (location) of the entry within the DBPF file</param>
		/// <param name="size">Compressed size of data for the entry, in bytes. Uncompressed size is also temporarily set to this to this until the data is set</param>
		/// <param name="index">Entry position in the file, 0-n</param>
		/// <param name="bytes">Byte data for this entry</param>
		public DBPFEntryLTEXT(TGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {
			_text = string.Empty;
		}



        /// <summary>
        /// Decodes the LTEXT string from raw data and sets the <see cref="Text"/> property of this instance.
        /// </summary>
        /// <remarks>
        /// Data must be uncompressed or garbage data is returned.
        /// </remarks>
        public override void Decode() {
			if (IsDecoded) {
				return;
			}
			if (ByteData.Length < 4) {
				_text = string.Empty;
				LogError("Data length is less than 4 bytes so no information can be read.");
			}

			if (IsCompressed) {
				ByteData = QFS.Decompress(ByteData);
			}

			int pos = 0;
			ushort numberOfChars = BitConverter.ToUInt16(ByteData, pos);
			pos += 2;
			ushort textControlChar = ByteData.ReadIntoUshort(pos, DBPF.Encoding.Binary);
			if (textControlChar != 0x0010) {
				_text = string.Empty;
				LogError("Invalid control character. Text not set.");
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
			IsDecoded = true;
		}



        /// <summary>
        /// Build and compress <see cref="DBPFEntry.ByteData"/> from the current state of this instance.
        /// </summary>
        /// <param name="compress">Whether to compress the entry</param>
        public override void Encode(bool compress = false) {
			if (TGI.GroupID == 0) { TGI.RandomizeGroup(); }
            if (TGI.InstanceID == 0) { TGI.RandomizeInstance(); }

			string text = _text ?? string.Empty;
			int byteLen = text.Length * 2;
            byte[] bytes = new byte[4 + byteLen];
			Buffer.BlockCopy(BitConverter.GetBytes((ushort) text.Length), 0, bytes, 0, 2); //Number of 2-byte characters
			bytes[2] = 0x00; //Text control characters
            bytes[3] = 0x10;
			Buffer.BlockCopy(text.ToBytes(), 0, bytes, 4, byteLen);
			
			if (compress) {
				ByteData = bytes.Compress();

                //If data could not be compressed for some reason
                if (ByteData is null) {
                    ByteData = bytes;
					IsCompressed = false;
                } else {
                    CompressedSize = (uint) ByteData.Length;
					IsCompressed = true;
                }
            } 
			
			else {
                ByteData = bytes;
                IsCompressed = false;
            }
            UncompressedSize = (uint) byteLen + 4;
        }
	}
}
