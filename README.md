# csDBPF
C# library for Simcity 4 DBPF files.

WIP: Dec 2021 - ?

The goal of csDBPF is to provide full functionality for reading, modifying, writing DBPF files with the aim of making batch modifications easy. It is currently being designed to be used as a base for a new generation of SC4 modding tools.

# Explanation of this Library
At a high level, a [DBPFFile](csDBPF/csDBPF/DBPFFile.cs) ("file") is the container for SC4 DBPF data. The main components of the file include the Header, ListOfTGIs, and ListOfEntries. Each file is broken into one or more [DBPFEntry](csDBPF/csDBPF/DBPFEntry.cs) ("entries" or "subfiles"). For further information on these, refer to the list of entries contained in that link or at SC4DWiki [Filetypes](https://www.wiki.sc4devotion.com/index.php?title=List_of_File_Formats).

For Exemplar and Cohort type entries, each entry is then composed of one or more [DBPFProperty](csDBPF/csDBPF/Properties/DBPFProperty.cs) ("properties"). Using a modified version of [new_properties.xml](https://www.sc4devotion.com/csxlex/lex_filedesc.php?lotGET=2265) to load in a long list of all of the known properties into [DBPFExemlarProperty](csDBPF/csDBPF/Properties/DBPFExemplarProperty.cs), each property in the entry can be identified and parsed accordingly.

For other types of entries, the data is interpreted directly from its byte array.

Note that the data for a particular entry or property will remain in its raw byte form until a `DecodeEntry()` or `DecodeProperty()` function is called to turn the byte data into a useable format.


## Code Examples
Refer to the [wiki](https://github.com/noah-severyn/csDBPF/wiki) for functional code samples and an explanation of how they work.


## Other DBPF Libraries:

- [Wiki Explanation](https://www.wiki.sc4devotion.com/index.php?title=DBPF)
- [scdbpf (Scala)](https://github.com/memo33/scdbpf)
- [jDBPFX (Java)](https://github.com/memo33/jDBPFX). Is derived from: [sc4dbpf4j: java](https://sourceforge.net/projects/sc4dbpf4j/)
- [godbpf (Go)](https://github.com/marcboudreau/godbpf)
- [dbpf (Python)](https://github.com/fbstj/dbpf) (also stale C#, c, ... branches; most are far from complete)
- [DBPF_Source_Code (PHP)](https://www.wiki.sc4devotion.com/index.php?title=DBPF_Source_Code)
- [sc4 (Javascript)](https://github.com/sebamarynissen/sc4)
- [DBPFSharp (C#)](https://github.com/0xC0000054/DBPFSharp)
