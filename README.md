# csDBPF
C# library for Simcity 4 DBPF files.

WIP: Dec 2021 - ?

# Explanation of this Library
At a high level, a [DBPFFile](csDBPF/csDBPF/DBPFFile.cs) ("file") is the container for SC4 DBPF data. The main components of the file include the Header, ListOfTGIs, and ListOfEntries. Each file is broken into one or more [DBPFEntry](csDBPF/csDBPF/DBPFEntry.cs) ("entries" or "subfiles"). For further information on these, refer to the list of entries contained in that link or at SC4DWiki [Filetypes](https://www.wiki.sc4devotion.com/index.php?title=List_of_File_Formats).

For Exemplar and Cohort type entries, each entry is then composed of one or more [DBPFProperty](csDBPF/csDBPF/Properties/DBPFProperty.cs) ("properties"). A modified version of [new_properties.xml](https://www.sc4devotion.com/csxlex/lex_filedesc.php?lotGET=2265) is parsed to load in a long list of all of the known properties into [DBPFExemlarProperty](csDBPF/csDBPF/Properties/DBPFExemplarProperty.cs). Each property in the entry can be be associated with a user-readable format and rules can be maintained for valid values.

For all other types of entries, the data is interpreted directly from its byte array according to the entry type.

In an effort towards optimization, the data for a particular entry or property will remain in its raw byte form until a `DecodeEntry()` or `DecodeProperty()` function is called to turn the byte data into an array or list of appropriately typed objects.

Currently only parsing of DBPF files is supported. Further work needs to be done in understanding the QFS/RefPak compression algorithm before DBPF files can be modified and written. Stay tuned.


## Code Examples
Refer to the [wiki](https://github.com/noah-severyn/csDBPF/wiki) for functional code samples and an explanation of how they work.


## Other DBPF Libraries:

- Wiki: https://www.wiki.sc4devotion.com/index.php?title=DBPF
- scala: https://github.com/memo33/scdbpf
- java: https://github.com/memo33/jDBPFX (derived from: java https://sourceforge.net/projects/sc4dbpf4j/)
- go: https://github.com/marcboudreau/godbpf
- python: https://github.com/fbstj/dbpf (also stale C#, c, ... branches are here in varying states of completion, but most are far from complete)
- php: https://www.wiki.sc4devotion.com/index.php?title=DBPF_Source_Code
- js: https://github.com/sebamarynissen/sc4/blob/master/lib/dbpf.js
