using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csDBPF.Entries {
    internal class DBPFEntryUI : DBPFEntry {
        public DBPFEntryUI(DBPFTGI tgi) : base(tgi) {
        }

        public DBPFEntryUI(DBPFTGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {
        }
        public DBPFEntryUI(TGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {
        }

        public override void DecodeEntry() {
            throw new NotImplementedException();
        }

        public override void ToBytes() {
            throw new NotImplementedException();
        }
    }
}
