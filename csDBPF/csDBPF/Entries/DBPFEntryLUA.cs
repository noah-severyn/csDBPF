using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csDBPF {
    internal class DBPFEntryLUA : DBPFEntry {
        public DBPFEntryLUA(TGI tgi) : base(tgi) {
        }

        public DBPFEntryLUA(TGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {
        }

        public override void Decode() {
            
        }

        public override void Encode(bool compress = false) {
            
        }
    }
}
