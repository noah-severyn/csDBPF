using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csDBPF.Entries {
    internal class DBPFEntryXML : DBPFEntry {
        public DBPFEntryXML(TGI tgi) : base(tgi) {
        }
        public DBPFEntryXML(TGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {
        }

        public override void Decode() {
            throw new NotImplementedException();
        }

        public override void Encode(bool compress = false) {
            throw new NotImplementedException();
        }
    }
}
