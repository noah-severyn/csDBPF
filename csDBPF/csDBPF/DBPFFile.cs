using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Linq;
using System.IO;

namespace csDBPF {
    /// <summary>
    /// Contains the header data and all entries for a DBPF file.
    /// </summary>
    /// <remarks>
    /// At a high level, a <see href="https://wiki.sc4devotion.com/index.php?title=DBPF">DBPFFile</see> ("file") is the container for the DBPF data, and takes the form of a .dat/.sc4lot/.sc4model/.sc4desc file. The main components of the file include the <see cref="Header"/>, which stores key information about the file itself, and the <see cref="ListOfTGIs"/> and <see cref="ListOfEntries"/>, which contain the data data about the contents of the file. Each DBPFFile is broken into one or more <see cref="DBPFEntry"/> ("entries" or "subfiles"), which inform what kind of data the entry stores and how it should be interpreted.<br></br>
	/// • Exemplar and Cohort type entries are composed of one or more <see cref="DBPFProperty"/> ("properties"). Each property corresponds to one of <see cref="XMLExemplarProperty"/> which are generated from the properties XML file.<br></br>
	/// • All other types of entries have their data interpreted directly from its byte array. This process varies depending on the type of entry (text, bitmap, xml, etc.).<br></br><br></br>
	/// The data for a particular entry or property will remain in its raw byte form until a DecodeEntry() or DecodeProperty() function is called to translate the byte data into a "friendly" format.
    /// </remarks>
    public class DBPFFile {
        /// <summary>
        /// Stores key information about the DBPFFile. Is the first 96 bytes of the file. 
        /// </summary>
        public DBPFHeader Header { get; private set; }

        /// <summary>
        /// Represents a file system object for this DBPF file. 
        /// </summary>
        public FileInfo File { get; }

        /// <summary>
        /// Size of all entries in this file,  in bytes.
        /// </summary>
        /// <remarks>
        /// This does not include the size allocated for the Header (96 bytes) or the Index (entry count * 20 bytes).
        /// </remarks>
        public long DataSize { get; private set; }

        /// <summary>
        /// Gets a list of all entries in this file.
        /// </summary>
        public List<DBPFEntry> ListOfEntries { get; private set; }

        /// <summary>
        /// Gets a list of all TGI sets in this file.
        /// </summary>
        /// <remarks>
        /// Can be used for quick inspection of a file's contents as no entry data is processed, meaning this information is available before entries have been decoded.
        /// </remarks>
        public List<TGI> ListOfTGIs { get; private set; }

        private List<DBPFError> _errorLog;
        /// <summary>
        /// Gets a list of issues encountered when parsing this file.
        /// </summary>
		/// <remarks>
		/// Each entry additionally stores its own error log.
		/// </remarks>
        public List<DBPFError> ErrorLog {
            get {
                var combinedLog = _errorLog;
                foreach (DBPFEntry entry in ListOfEntries) {
                    foreach (var error in entry.ErrorLog) {
                        combinedLog.Add(new DBPFError(File.Name, error.TGI, error.Message));
                    }
                }
                return combinedLog;
            }
            private set { _errorLog = value; }
        }

        /// <summary>
        /// Instantiates a DBPFFile from a file path. If the file exists, its contents are read into the new DBPFFile.
        /// </summary>
        /// <param name="filePath">File to read</param>
        public DBPFFile(string filePath) : this(new FileStream(filePath, FileMode.OpenOrCreate)) { }
        /// <summary>
        /// Instantiates a DBPFFile from a file stream. If the file exists, its contents are read into the new DBPFFile; if the file does not exist, a new DBPF file is created.
        /// </summary>
        /// <param name="stream">File to read</param>
        public DBPFFile(FileStream stream) : this() {
            File = new FileInfo(stream.Name);
            if (!File.Exists) {
                return;
            }

            bool map = false;
            BinaryReader br = new BinaryReader(stream);
            try {
                if (map) {
                    ReadAndMap();
                } else {
                    Read(br);
                }
            }
            catch (InvalidDataException) {

            }
            finally {
                br.Close();
            }
        }

        /// <summary>
        /// Instantiates a new DBPFFile from scratch. 
        /// </summary>
        public DBPFFile() {
            Header = new DBPFHeader();
            ListOfEntries = [];
            ListOfTGIs = [];
            ErrorLog = [];
        }

        /// <inheritdoc/>
        public override string ToString() {
            return $"{File.Name}: {ListOfEntries.Count} subfiles";
        }


        /// <summary>
        /// Reads the header and TGI list of a DBPF file.
        /// </summary>
        /// <param name="file"></param>
        private void ReadQuick(FileInfo file) {

        }



        /// <summary>
        /// Reads a DBPF file.
        /// </summary>
        /// <remarks>
        /// Use only for short-lived DBPF files for which the content does not change on disk, or does not matter if it does, or if the file is small.
        /// </remarks>
        /// <returns>A new DBPFFile object</returns>
        /// <see href="https://www.wiki.sc4devotion.com/index.php?title=DBPF#Pseudocode"/>
        private void Read(BinaryReader br) {
            try {
                // Read Header info
                Header = new DBPFHeader(br);
                DataSize = Header.IndexEntryOffset - 96;

                //Read Index info
                List<uint> offsets = [];
                List<uint> sizes = [];
                byte[] byteData;

                br.BaseStream.Seek((Header.IndexEntryOffset), SeekOrigin.Begin);
                for (int idx = 0; idx < (Header.IndexEntryCount); idx++) {
                    uint typeID = br.ReadUInt32();
                    uint groupID = br.ReadUInt32();
                    uint instanceID = br.ReadUInt32();
                    uint offset = br.ReadUInt32();
                    uint size = br.ReadUInt32();

                    ListOfTGIs.Add(new TGI(typeID, groupID, instanceID));
                    offsets.Add(offset);
                    sizes.Add(size);
                }


                //Read Entry data
                for (int idx = 0; idx < ListOfTGIs.Count; idx++) {
                    br.BaseStream.Seek(offsets[idx], SeekOrigin.Begin);
                    byteData = br.ReadBytes((int) sizes[idx]);

                    switch (ListOfTGIs[idx].GetEntryType()) {
                        case "EXMP":
                            ListOfEntries.Add(new DBPFEntryEXMP(ListOfTGIs[idx], offsets[idx], sizes[idx], (uint) idx, byteData));
                            break;
                        case "LTEXT":
                            ListOfEntries.Add(new DBPFEntryLTEXT(ListOfTGIs[idx], offsets[idx], sizes[idx], (uint) idx, byteData));
                            break;
                        case "DIR":
                            ListOfEntries.Add(new DBPFEntryDIR(offsets[idx], sizes[idx], (uint) idx, byteData));
                            break;
                        case "S3D":
                            ListOfEntries.Add(new DBPFEntryS3D(ListOfTGIs[idx], offsets[idx], sizes[idx], (uint) idx, byteData));
                            break;
                        case "FSH":
                            ListOfEntries.Add(new DBPFEntryFSH(ListOfTGIs[idx], offsets[idx], sizes[idx], (uint) idx, byteData));
                            break;
                        case "PNG":
                            ListOfEntries.Add(new DBPFEntryPNG(ListOfTGIs[idx], offsets[idx], sizes[idx], (uint) idx, byteData));
                            break;
                        case "LUA":
                            ListOfEntries.Add(new DBPFEntryLUA(ListOfTGIs[idx], offsets[idx], sizes[idx], (uint) idx, byteData));
                            break;
                        case "UI":
                            ListOfEntries.Add(new DBPFEntryUI(ListOfTGIs[idx], offsets[idx], sizes[idx], (uint) idx, byteData));
                            break;
                        case "WAV":
                            ListOfEntries.Add(new DBPFEntryWAV(ListOfTGIs[idx], offsets[idx], sizes[idx], (uint) idx, byteData));
                            break;
                        default:
                            LogIssue("Unknown TGI identifier.", ListOfTGIs[idx]);
                            ListOfEntries.Add(new DBPFEntryUnknown(ListOfTGIs[idx], offsets[idx], sizes[idx], (uint) idx, byteData));
                            break;
                    }
                }
            }

            catch {
                LogIssue("Unable to read DBPF file. Format unknown.");
            }
        }


        /// <summary>
        /// Reads a DBPF file and maps the file from disk to memory.
        /// </summary>
        /// <remarks>
        /// Serves a similar purpose to <see cref="Read"/>, but it is capable of writing large amount of entries quicker - suitable for large files and files of which the ______ methods will be called frequently.
        /// </remarks>
        private DBPFFile ReadAndMap() {
            //this.Read(this.file);
            throw new NotImplementedException();
        }

        //TODO - implement readCached https://github.com/memo33/jDBPFX/blob/master/src/jdbpfx/DBPFFile.java#L721




        private void LogIssue(string message, TGI tgi = new TGI()) {
            ErrorLog.Add(new DBPFError(File.Name, tgi, message));
        }



        /// <summary>
        /// Decompresses (if necessary) and decodes all entries in the file.
        /// </summary>
		/// <remarks>
		/// For more information, see <see cref="DBPFEntry.Decode()"/> and the specific implementations for each entry type.
		/// </remarks>
        public void DecodeAllEntries() {
            foreach (DBPFEntry entry in ListOfEntries) {
                entry.Decode();
            }
        }


        public void EncodeAllEntries() {
            foreach (DBPFEntry entry in ListOfEntries) {
                entry.Encode();
            }
        }



        /// <summary>
        /// Return the nth entry in the file by index. 0-based index.
        /// </summary>
        /// <param name="index">Index position in file.</param>
        /// <returns>The nth DBPFEntry</returns>
        public DBPFEntry GetEntry(int index) {
            return ListOfEntries[index];
        }
        /// <summary>
        /// Return the first entry matching the specified Instance ID.
        /// </summary>
        /// <param name="instance">IID to search for</param>
        /// <returns>A matching DBPFEntry</returns>
        public DBPFEntry GetEntry(uint instance) {
            return ListOfEntries.Find(i => i.TGI.InstanceID == instance);
        }
        /// <summary>
        /// Return the first entry matching the specified TGI.
        /// </summary>
        /// <param name="TGI">TGI set to search for</param>
        /// <returns>A matching DBPFEntry</returns>
        public DBPFEntry GetEntry(TGI TGI) {
            return ListOfEntries.Find(entry => entry.TGI.Matches(TGI));
        }



        /// <summary>
        /// Saves the current instance to disk using the <see cref="File"/> property.
        /// </summary>
        public void Save() {
            SaveAs(File.FullName);
        }
        /// <summary>
        /// Saves the current instance to disk at the specified path.
        /// <param name="filePath">File to save as</param>
        /// </summary>
        public void SaveAs(string filePath) {
            FileInfo file;
            if (filePath is null) {
                file = File;
            } else {
                file = new FileInfo(filePath);
            }

            //Update and write Header
            if (Header.Identifier is null) {
                Header = new DBPFHeader();
            }
            Header.Update(this);
            using FileStream fs = new(file.FullName, FileMode.Create);
            fs.Write(ByteArrayHelper.ToBytes(Header.Identifier, true));
            fs.Write(BitConverter.GetBytes(Header.MajorVersion));
            fs.Write(BitConverter.GetBytes(Header.MinorVersion));
            fs.Write(new byte[12]); //12 bytes are unused
            fs.Write(BitConverter.GetBytes(Header.DateCreated));
            fs.Write(BitConverter.GetBytes(Header.DateModified));
            fs.Write(BitConverter.GetBytes(Header.IndexMajorVersion));
            fs.Write(BitConverter.GetBytes(Header.IndexEntryCount));
            fs.Write(BitConverter.GetBytes(Header.IndexEntryOffset));
            fs.Write(BitConverter.GetBytes(Header.IndexSize));
            fs.Write(new byte[48]);

            //Write all entries
            RebuildDirectory();
            foreach (DBPFEntry entry in ListOfEntries) {
                fs.Write(entry.ByteData);
            }

            //Write Index
            UpdateIndex();
            long pos = fs.Position;
            foreach (DBPFEntry entry in ListOfEntries) {
                fs.Write(BitConverter.GetBytes(entry.TGI.TypeID));
                fs.Write(BitConverter.GetBytes(entry.TGI.GroupID));
                fs.Write(BitConverter.GetBytes(entry.TGI.InstanceID));
                fs.Write(BitConverter.GetBytes(entry.Offset));
                if (entry.IsCompressed) {
                    fs.Write(BitConverter.GetBytes(entry.CompressedSize));
                } else {
                    fs.Write(BitConverter.GetBytes(entry.UncompressedSize));
                }

            }
        }


        /// <summary>
        /// Updates the position number and offset of each entry based on the current size and position of each entry.
        /// </summary>
        private void UpdateIndex() {
            uint offset = 96;
            ListOfEntries[0].Offset = 96;
            if (ListOfEntries.Count == 1) {
                return;
            }

            for (int idx = 1; idx < ListOfEntries.Count; idx++) {
                offset += ListOfEntries[idx - 1].GetSize();
                ListOfEntries[idx].Offset = offset;
                ListOfEntries[idx].IndexPos = (uint) idx;
            }
        }



        /// <summary>
        /// Add an entry to this file.
        /// </summary>
        /// <param name="entry">Entry to add</param>
        public void AddEntry(DBPFEntry entry) {
            entry.IndexPos = (uint) (ListOfEntries.Count + 1);
            entry.Offset = (uint) DataSize + 96;

            ListOfEntries.Add(entry);
            ListOfTGIs.Add(entry.TGI);

            //Non decoded entries (or newly created entries that have not been encoded yet) will not have ByteData set
            if (entry.ByteData is null) {
                DataSize += entry.GetSize();
            } else {
                DataSize += entry.ByteData.LongLength;
            }

        }

        /// <summary>
        /// Add multiple entries to this file.
        /// </summary>
        /// <param name="entries">Entries to add</param>
        public void AddEntries(IEnumerable<DBPFEntry> entries) {
            foreach (DBPFEntry entry in entries) {
                AddEntry(entry);
            }
        }

        /// <summary>
        /// Add the entry to this file if a matching TGI is not found, otherwise update the corresponding entry.
        /// </summary>
        /// <param name="entry">Entry to add</param>
        public void AddOrUpdateEntry(DBPFEntry entry) {
            if (ListOfEntries.Any(e => e.TGI.Matches(entry.TGI))) {
                UpdateEntry(entry);
            } else {
                AddEntry(entry);
            }
        }

        /// <summary>
        /// Add each entry to this file if a matching TGI is not found, otherwise update the corresponding entry.
        /// </summary>
        /// <param name="entries">Entries to add</param>
        public void AddOrUpdateEntries(IEnumerable<DBPFEntry> entries) {
            foreach (DBPFEntry entry in entries) {
                AddOrUpdateEntry(entry);
            }
        }

        /// <summary>
        /// Remove the entry matching the specified TGI from this file.
        /// </summary>
        /// <param name="tgi">Entry TGI to remove</param>
        /// <remarks>
        /// If more than one entry matches the given TGI then no entries are removed.
        /// </remarks>
        public void RemoveEntry(TGI tgi) {
            int matches = ListOfTGIs.Count(x => x.Equals(tgi));
            if (matches != 1) {
                return;
            }

            int index = ListOfTGIs.IndexOf(tgi);
            RemoveEntry(index);
        }

        /// <summary>
        /// Remove the entry at the specified position from this file.
        /// </summary>
        /// <param name="position">Entry position to remove</param>
        public void RemoveEntry(int position) {
            ListOfEntries.RemoveAt(position);
            ListOfTGIs.RemoveAt(position);
            DataSize -= ListOfEntries[position].ByteData.LongLength;
        }


        public void RemoveEntries(TGI tgi) {
            //TODO - implement RemoveEntries for all matching TGIs
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears all entries from this file.
        /// </summary>
        public void RemoveAllEntries() {
            ListOfEntries.Clear();
            ListOfTGIs.Clear();
            DataSize = 0;
        }

        /// <summary>
        /// Builds the Directory subfile if any entries in this file are compressed.
        /// </summary>
        public void RebuildDirectory() {
            if (ListOfEntries.Any(entry => entry.IsCompressed == true)) {
                DBPFEntryDIR dir = new DBPFEntryDIR();
                dir.Build(ListOfEntries);
            }
        }

        /// <summary>
        /// Returns the count of entries in this file.
        /// </summary>
        /// <returns>The count of entries</returns>
        public int CountEntries() {
            return ListOfEntries.Count;
        }


        /// <summary>
        /// Returns the count of TGIs in this file.
        /// </summary>
        /// <returns>The count of TGI sets</returns>
        /// <remarks>
        /// This may be used in lieu of <see cref="CountEntries()"/> for more performant operation if no entry data is required.
        /// </remarks>
        public int CountTGIs() {
            return ListOfTGIs.Count;
        }

        public void UpdateAllEntries(IEnumerable<DBPFEntry> entries) {
            //TODO - implement Update for a list of entries
            //any entries with matching TGIs will be overwritten (first occurrence only) and any new ones will be added - skip DIR files
            throw new NotImplementedException();
        }

        public void UpdateEntry(DBPFEntry entry) {
            //TODO - implement Update for a TGI
            //throw new NotImplementedException();
        }
        public void UpdateEntry(int index) {
            //TODO - implement Update for a position
            throw new NotImplementedException();
        }

        public void SortEntries() {
            //_listOfEntries = _listOfEntries.OrderBy(e => e.TGI);
            //_listOfTGIs.OrderBy(_);
        }
    }
}