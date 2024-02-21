using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csDBPF {
	internal class DBPF_Test {

		static void Main(string[] args) {
			string path = "C:\\Users\\Administrator\\Documents\\SimCity 4\\Plugins\\new.dat";
			string parksaura = "C:\\Users\\Administrator\\Documents\\SimCity 4\\Plugins\\z_DataView - Parks Aura.dat";
			DBPFFile newdat = new DBPFFile(parksaura);
			newdat.SaveAs(path);
		}
	}
}
