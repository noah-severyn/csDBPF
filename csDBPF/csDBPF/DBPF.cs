using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csDBPF {

    /// <summary>
    /// Details an error encountered when parsing a DBPF file.
    /// </summary>
    public struct DBPFError(string fileName, TGI tgi, string message) {
        /// <summary>
        /// Name of the current DBPF file
        /// </summary>
        public string FileName = fileName;
        /// <summary>
        /// TGI of the current subfile
        /// </summary>
        public TGI TGI = tgi;
        /// <summary>
        /// Error message
        /// </summary>
        public string Message = message;
    }


}
