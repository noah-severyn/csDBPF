using csDBPF.Properties;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.WebRequestMethods;

namespace csDBPF.Entries {
    /// <summary>
    /// An abstract form of an entry item, representing an instance of a subfile that may be contained in a <see cref="DBPFFile"/>. The data for each entry is not parsed or decoded until <see cref="Decode"/> is called to decompress and set the actual entry data.
    /// </summary>
    /// <see href="https://www.wiki.sc4devotion.com/index.php?title=List_of_File_Formats"/>
    public abstract class DBPFEntry {
		/// <summary>
		/// The TGI set representing the file type of the entry.
		/// </summary>
		public TGI TGI { get; }

		/// <summary>H
		/// Byte position of this entry within the <see cref="DBPFFile"/>.
		/// </summary>
		public uint Offset { get; internal set; }

		/// <summary>
		/// Position of this entry in relation to the other entries in the <see cref="DBPFFile"/>, 0-n.
		/// </summary>
		public uint IndexPos { get; internal set; }

        /// <summary>
        /// Uncompressed size of the entry data, in bytes.
        /// </summary>
        /// <remarks>
        /// This may be null if ByteData is null.
        /// </remarks>
        public uint UncompressedSize { get; protected set; }

		/// <summary>
		/// Compressed size of the entry data, in bytes.
		/// </summary>
		/// <remarks>
		/// This may be null if ByteData is null or if ByteData has been updated and not yet re-compressed.
		/// </remarks>
		public uint CompressedSize { get; protected set; }

        /// <summary>
        /// Get the current compression state of <see cref="ByteData"/>. May be null if the compression state is unknown.
        /// </summary>
        public bool IsCompressed { get;	private protected set; }

		private byte[] _byteData;
        /// <summary>
        /// Byte array of raw data pertaining to this entry. This may or may not be compressed.
        /// </summary>
        /// <remarks>
        /// The interpretation of the entry data depends on the compression status and the entry type (known through its <see cref="TGI"/>). Always check if the data is compressed before processing.
        /// </remarks>
		public byte[] ByteData { get {
				return _byteData;
			}

			protected set {
                _byteData = value;
                //Peek at bytes 4 and 5 to determine compression status
                IsCompressed = _byteData.Length > 9 && ByteArrayHelper.ReadBytesIntoUshort(_byteData, 4) == 0x10FB;
				if (IsCompressed) {
					CompressedSize = (uint) _byteData.Length;
					//UncompressedSize = (uint) ByteArrayHelper.to //TODO - fix Uncompressed size setting here
				} else {
                    UncompressedSize = (uint) _byteData.Length;
                }
			}
		}

        /// <summary>
        /// Comma delineated list of issues encountered when loading this entry.
        /// </summary>
		/// <remarks>
		/// This is a multi line string in the format of  <see cref="TGI.ToString"/> followed by the message. Format is: FileName, Type, Group, Instance, TGIType, TGISubtype, Message. For items logged at the entry level, FileName is left blank as it is unknown (it's a property of this entry's <see cref="DBPFFile.File"/>).
		/// </remarks>
        internal StringBuilder IssueLog { get; private set; }



		
		/// <summary>
		/// Create a new DBPFEntry object with a given TGI struct.
		/// </summary>
		/// <param name="tgi"></param>
		public DBPFEntry(TGI tgi) {
            TGI = tgi;
            IssueLog = new StringBuilder();
        }

		
        /// <summary>
        /// Create a new DBPFEntry object.
        /// </summary>
        /// <param name="tgi"><see cref="TGI"/> object representing the entry</param>
        /// <param name="offset">Offset (location) of the entry within the DBPF file</param>
        /// <param name="size">Compressed size of data for the entry, in bytes. Uncompressed size is also temporarily set to this to this until the data is set</param>
        /// <param name="index">Entry position in the file, 0-n</param>
        /// <param name="bytes">Byte data for this entry</param>
        public DBPFEntry(TGI tgi, uint offset, uint size, uint index, byte[] bytes) {
			TGI = tgi;
            Offset = offset;
            IndexPos = index;
            CompressedSize = size;
            _byteData = bytes;
            IssueLog = new StringBuilder();

            //Peek at bytes 4 and 5 to determine compression status
            IsCompressed = (_byteData.Length > 9 && ByteArrayHelper.ReadBytesIntoUshort(_byteData, 4) == 0x10FB);
            //Peek at the first 9 bytes of this data to determine its compression characteristics
            if (IsCompressed) {
                UncompressedSize = (uint) ((bytes[6] << 16) | (bytes[7] << 8) | bytes[8]);
            } else {
                UncompressedSize = 0;
            }
        }



        /// <summary>
        /// Decompresses the data (if necessary) and sets the entry's data object from <see cref="ByteData"/> according to the specific entry's type.
        /// </summary>
        public abstract void Decode();

        /// <summary>
        /// Builds <see cref="ByteData"/> with the current state of the entry's data object. The encoding can be either text or binary according to <see cref="EncodingType"/>.
        /// </summary>
		/// <param name="compress">Whether to compress the ByteData. Default is FALSE</param>
        public abstract void Encode(bool compress = false);



		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>Returns a string that represents the current object.</returns>
		public override string ToString() {
			return $"{TGI}, Type: {TGI.GetEntryType()}, IndexPos: {IndexPos}, Offset: {Offset}, uSize: {UncompressedSize}, Compressed: {IsCompressed}, cSize: {CompressedSize}";
        }



		/// <summary>
		/// Determine if the entry is the same entry type as the specified one.
		/// </summary>
		/// <param name="known"><see cref="DBPFTGI"/> to compare against</param>
		/// <returns>TRUE if this Entry matches the specified; FALSE otherwise.</returns>
		/// <remarks>This is a shortcut equivalent to <see cref="TGI.Matches(TGI)"/>.</remarks>
		public bool MatchesEntryType(TGI known) {
			return TGI.Matches(known);
		}


		/// <summary>
		/// Returns whether this entry is an Exemplar or Cohort Entry
		/// </summary>
		/// <returns>TRUE if is an Exemplar or Cohort; FALSE otherwise</returns>
		public bool IsEXMP() {
			byte[] data;
			if (IsCompressed) {
				data = QFS.Decompress(_byteData[0..16]);
			} else {
				data = _byteData[0..16];
			}
			string fileIdentifier = ByteArrayHelper.ToAString(data, 0, 4);
			return fileIdentifier == "EQZB" || fileIdentifier == "EQZT" || fileIdentifier == "CQZB" || fileIdentifier == "CQZT";
		}

		/// <summary>
		/// Return either the Compressed or Uncompressed size depending on if this entry is compressed or not.
		/// </summary>
		/// <returns>The size in bytes</returns>
		public uint GetSize() {
			if (IsCompressed) {
				return CompressedSize;
			} else {
				return UncompressedSize;
			}
		}

		/// <summary>
		/// Adds the specified message to the entry's <see cref="IssueLog"/>.
		/// </summary>
		/// <param name="message">Message to add</param>
		private protected void LogMessage(string message) {
			IssueLog.AppendLine("," + TGI.ToString().Replace(" ", "") + "," + message);
		}


		/// <summary>
		/// Specifies the encoding type for an entry or properties.
		/// </summary>
		public enum EncodingType {
			/// <summary>
			/// Entry/property is encoded in binary format.
			/// </summary>
			Binary,
			/// <summary>
			/// Entry/property is encoded in text format.
			/// </summary>
			Text
		}
	}
}