using System.Collections.Generic;

namespace csDBPF {
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
                IsCompressed = _byteData.Length > 9 && _byteData.ReadIntoUshort(4) == 0x10FB;
				if (IsCompressed) {
					CompressedSize = (uint) _byteData.Length;
					//UncompressedSize = (uint) ByteArrayHelper.to //TODO - fix Uncompressed size setting here
				} else {
                    UncompressedSize = (uint) _byteData.Length;
                }
			}
		}

        /// <summary>
        /// Gets a list of issues encountered when parsing this entry.
        /// </summary>
		/// <remarks>
		/// For Entries, the FileName is blank as it is unknown in this item's context - it's a property of this entry's parent DBPFFile.
		/// </remarks>
        public List<DBPFError> ErrorLog { get; private set; }



		
		/// <summary>
		/// Create a new DBPFEntry object with a given TGI struct.
		/// </summary>
		/// <param name="tgi"></param>
		public DBPFEntry(TGI tgi) {
            TGI = tgi;
			ErrorLog = [];
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
			ErrorLog = [];

            //Peek at bytes 4 and 5 to determine compression status
            IsCompressed = (_byteData.Length > 9 && ByteArrayHelper.ReadBytesIntoUshort(_byteData, 4) == 0x10FB);
            IsCompressed = (_byteData.Length > 9 && _byteData.ReadIntoUshort(4) == 0x10FB);
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



        /// <inheritdoc/>
        public override string ToString() {
			return $"{TGI}, Type: {TGI.GetEntryType()}, IndexPos: {IndexPos}, Offset: {Offset}, uSize: {UncompressedSize}, Compressed: {IsCompressed}, cSize: {CompressedSize}";
        }



        /// <summary>
        /// Determine if the entry is the same kind as the specified type.
        /// </summary>
        /// <param name="known">TGI type to check against. Refer to the static fields of <see cref="DBPFTGI"/> to use as comparisons.</param>
        /// <returns><see langword="true"/> if this entry matches the specified type; otherwise <see langword="false"/></returns>
        /// <remarks>This is a shortcut equivalent to <see cref="TGI.Matches(TGI)"/>.</remarks>
        public bool MatchesEntryType(TGI known) {
			return TGI.Matches(known);
		}
        /// <summary>
        /// Determine if the entry is the same kind as any of the specified types.
        /// </summary>
		/// <param name="knowns">TGI type(s) to check against. Refer to the static fields of <see cref="DBPFTGI"/> to use as comparisons.</param>
        /// <returns><see langword="true"/> if this entry matches any of the specified types; otherwise <see langword="false"/></returns>
        public bool MatchesAnyEntryType(params TGI[] knowns) {
			foreach (TGI known in knowns) {
				if (TGI.Matches(known)) {
					return true;
				}
			}
			return false;
		}


		/// <summary>
		/// Returns whether this entry is an Exemplar or Cohort Entry
		/// </summary>
		/// <returns>TRUE if is an Exemplar or Cohort; FALSE otherwise</returns>
		public bool IsExemplar() {
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
		/// Adds the specified message to the entry's <see cref="ErrorLog"/>.
		/// </summary>
		/// <param name="message">Message to add</param>
		private protected void LogError(string message) {
			ErrorLog.Add(new DBPFError("", TGI, message));
		}

        
    }
}