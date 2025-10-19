using System;
using System.Text;

namespace csDBPF {
    /// <summary>
    /// An implementation of <see cref="DBPFEntry"/> for SC4 UI elements. UI elements are through a text-based file similar to XML. Legacy tags contain attributes defining the type of element, its look and behavior, etc. Legacies can have children, which contain more legacy tags defining child elements. SC4 UI files are not valid XML, but are very similar. See <see href="https://wiki.sc4devotion.com/index.php?title=UI"/>.
    /// </summary>
    public class DBPFEntryUI : DBPFEntry {
        private bool _isDecoded;

        private string _definition;
        /// <summary>
        /// XML-like text describing the structure of the UI.
        /// </summary>
        public string Definition {
            get { return _definition; }
            set { _definition = value; }
        }


        public DBPFEntryUI(TGI tgi) : base(tgi) {
        }

        public DBPFEntryUI(TGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {
        }

        public override void Decode() {
            if (_isDecoded) {
                return;
            }

            if (IsCompressed) {
                ByteData = QFS.Decompress(ByteData);
            }
            _definition = Encoding.UTF8.GetString(ByteData);

        }

        public override void Encode(bool compress = false) {
            throw new NotImplementedException();
        }
    }
}
