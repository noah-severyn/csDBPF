using System;

namespace csDBPF {
    internal class DBPFEntryS3D : DBPFEntry {
        public DBPFEntryS3D(TGI tgi) : base(tgi) {
        }

        public DBPFEntryS3D(TGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {
        }

        public override void Decode() {
            throw new NotImplementedException();
        }

        public override void Encode(bool compress = false) {
            throw new NotImplementedException();
        }
    }
}
