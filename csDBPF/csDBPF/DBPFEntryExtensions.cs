using System;
using System.Collections.Generic;

namespace csDBPF {
    /// <summary>
    /// Extension methods for working with lists of <see cref="DBPFEntry"/> objects.
    /// </summary>
    public static class DBPFEntryExtensions {

        /// <summary>
        /// Decode all entries in the given list
        /// </summary>
        /// <param name="entries">List of entries to decode</param>
        public static void DecodeEntries(this IEnumerable<DBPFEntry> entries) {
            foreach (DBPFEntry entry in entries) {
                entry.Decode();
            }
        }
        /// <summary>
        /// Encode all entries in the given list
        /// </summary>
        /// <param name="entries">List of entries to encode</param>
        public static void EncodeEntries(this IEnumerable<DBPFEntry> entries) {
            foreach (DBPFEntry entry in entries) {
                entry.Encode();
            }
        }
    }
}
