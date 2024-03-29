﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csDBPF.Entries {
    /// <summary>
    /// Generic entry to encapsulate as yet unimplemented entry types or unknown entry types.
    /// </summary>
    internal class DBPFEntryUnknown : DBPFEntry {
        public DBPFEntryUnknown(TGI tgi) : base(tgi) {
        }

        public DBPFEntryUnknown(TGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {
        }

        public override void DecodeEntry() {}

        public override void ToBytes() {
            throw new NotImplementedException();
        }
    }
}
