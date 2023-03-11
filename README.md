# csDBPF
C# library for Simcity 4 DBPF files.

WIP: Dec 2021 - ?

The goal of csDBPF is to provide full functionality for reading, modifying, writing DBPF files with the aim of making batch modifications easy. It is currently being designed to be used as a base for a new generation of SC4 modding tools.

# Explanation of this Library
At a high level, a [DBPFFile](csDBPF/csDBPF/DBPFFile.cs) ("file") is the container for SC4 DBPF data. The main components of the file include the Header, ListOfTGIs, and ListOfEntries. The Header stores important information about the file itself, and the ListOfTGIs and ListOfEntries store information about subfiles. Each DBPF file is broken into one or more [DBPFEntry](csDBPF/csDBPF/DBPFEntry.cs) ("entries" or "subfiles"). For further information on these, refer to the list of entries contained in SC4D Wiki article [Filetypes](https://www.wiki.sc4devotion.com/index.php?title=List_of_File_Formats). Each entry has a type which is established form its [TGI](csDBPF/csDBPF/DBPFTGI.cs). This informs what kind of data the entry stores and how it should be interpreted.


For Exemplar and Cohort type entries, each entry is then composed of one or more [DBPFProperty](csDBPF/csDBPF/Properties/DBPFProperty.cs) ("properties"). A modified version of [new_properties.xml](https://www.sc4devotion.com/csxlex/lex_filedesc.php?lotGET=2265) is parsed to load in a long list of all of the known properties into [DBPFExemlarProperty](csDBPF/csDBPF/Properties/DBPFExemplarProperty.cs). Each property in the entry can be be associated with a user-readable format and rules can be maintained for valid values.

For all other types of entries, the data is interpreted directly from its byte array according to the entry type.

In an effort towards optimization, the data for a particular entry or property will remain in its raw byte form until a `DecodeEntry()` or `DecodeProperty()` function is called to turn the byte data into an array or list of appropriately typed objects.

Currently only parsing of DBPF files is supported. Further work needs to be done in understanding the QFS/RefPak compression algorithm before DBPF files can be modified and written.
=======
Exemplar and Cohort type entries are composed of one or more [DBPFProperty](csDBPF/csDBPF/Properties/DBPFProperty.cs) ("properties"). Using a modified version of [new_properties.xml](https://www.sc4devotion.com/csxlex/lex_filedesc.php?lotGET=2265) to load the list of all of the known properties into [DBPFExemlarProperty](csDBPF/csDBPF/Properties/DBPFExemplarProperty.cs), each property in the entry can be identified and parsed accordingly.

For other types of entries, the data is interpreted directly from its byte array. This process varies depending on the type of entry (text, bitmap, etc.).

Note that the data for a particular entry or property will remain in its raw byte form until a `DecodeEntry()` or `DecodeProperty()` function is called to translate the byte data into a human readable format.


## Code Examples
Refer to this repository's [wiki](https://github.com/noah-severyn/csDBPF/wiki) for functional code samples and an explanation of how they work.
