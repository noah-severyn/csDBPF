using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csDBPF.Entries {
    internal class DBPFEntryWAV : DBPFEntry {
        public DBPFEntryWAV(TGI tgi) : base(tgi) {
        }

        public DBPFEntryWAV(TGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {
        }

        public override void Decode() {
            throw new NotImplementedException();
        }

        public override void Encode(bool compress = false) {
            throw new NotImplementedException();
        }
    }
}
