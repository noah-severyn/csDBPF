using System;
using System.Collections.Generic;
using csDBPF;

namespace csDBPF {
    /// <summary>
    /// A set of extension methods for working with DBPF items
    /// </summary>
    public static class DBPFFileExtensions {

        /// <summary>
        /// Decode all entries in the given list
        /// </summary>
        /// <param name="entries">List of entries to decode</param>
        public static void DecodeEntries(this IEnumerable<DBPFEntry> entries) {
            foreach (DBPFEntry entry in entries) {
                entry.Decode();
            }
        }

    }
}
