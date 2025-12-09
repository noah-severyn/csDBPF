using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csDBPF {
    public static class DBPF {

        /// <summary>
        /// Specifies the encoding type for an entry or a property.
        /// </summary>
        public enum Encoding {
            /// <summary>
            /// Entry/property is encoded in binary format.
            /// </summary>
            Binary,
            /// <summary>
            /// Entry/property is encoded in text format.
            /// </summary>
            Text
        }
    }
}
