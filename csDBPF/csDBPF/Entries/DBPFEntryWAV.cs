using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csDBPF.Entries {
    internal class DBPFEntryWAV : DBPFEntry {
        public DBPFEntryWAV(DBPFTGI tgi) : base(tgi) {
        }

        public DBPFEntryWAV(DBPFTGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {
        }
        public DBPFEntryWAV(TGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {
        }

        public override void DecodeEntry() {
            throw new NotImplementedException();
        }

        public override void ToBytes() {
            throw new NotImplementedException();
        }
    }
}
