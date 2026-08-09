using System;
using System.Text;

namespace csDBPF {
    /// <summary>
    /// An implementation of <see cref="DBPFEntry"/> for SC4 UI elements. UI elements are through a text-based file similar to XML. Legacy tags contain attributes defining the type of element, its look and behavior, etc. Legacies can have children, which contain more legacy tags defining child elements. SC4 UI files are not valid XML, but are very similar. See <see href="https://wiki.sc4devotion.com/index.php?title=UI"/>.
    /// </summary>
    public class DBPFEntryUI : DBPFEntry {
        private string _definition;
        /// <summary>
        /// XML-like text describing the structure of the UI.
        /// </summary>
        public string Definition {
            get { return _definition; }
            set { _definition = value; }
        }

        /// <summary>
		/// Create a new instance. Use when creating a new exemplar.
		/// </summary>
		/// <param name="tgi">TGI set to assign</param>
        public DBPFEntryUI(TGI tgi) : base(tgi) {
            _definition = string.Empty;
        }

        /// <summary>
		/// Create a new instance. Use when reading an existing image from a file.
		/// </summary>
		/// <param name="tgi">TGI object representing the entry</param>
		/// <param name="offset">Offset (location) of the entry within the DBPF file</param>
		/// <param name="size">Compressed size of data for the entry, in bytes. Uncompressed size is also temporarily set to this to this until the data is set</param>
		/// <param name="index">Entry position in the file, 0-n</param>
		/// <param name="bytes">Byte data for this entry</param>
        public DBPFEntryUI(TGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {
            _definition = string.Empty;
        }

        /// <inheritdoc/>
        public override void Decode() {
            if (IsDecoded) {
                return;
            }

            if (IsCompressed) {
                ByteData = QFS.Decompress(ByteData);
            }
            _definition = Encoding.UTF8.GetString(ByteData);
            IsDecoded = true;
        }

        /// <inheritdoc/>
        public override void Encode(bool compress = false) {
            throw new NotImplementedException();
        }
    }
}
