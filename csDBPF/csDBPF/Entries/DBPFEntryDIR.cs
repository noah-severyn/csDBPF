using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csDBPF.Entries {

	//https://wiki.sc4devotion.com/index.php?title=DBDF

	internal class DBPFEntryDIR : DBPFEntry {




		//https://www.wiki.sc4devotion.com/index.php?title=DBDF
		public DBPFEntryDIR(DBPFTGI tgi, uint offset, uint size, uint index, byte[] bytes) : base(tgi, offset, size, index, bytes) {
		}

		public override void DecodeEntry() {
			throw new NotImplementedException();
		}


		//TODO Implement Update
		public void Update() {
			throw new NotImplementedException();
		}
	}
}
