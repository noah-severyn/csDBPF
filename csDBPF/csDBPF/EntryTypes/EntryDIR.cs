using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csDBPF.EntryTypes {
	internal class EntryDIR : DBPFEntry {

		//https://www.wiki.sc4devotion.com/index.php?title=DBDF
		public EntryDIR(DBPFTGI tgi, uint offset, uint size, uint index) : base(tgi, offset, size, index) {
		}
	}
}
