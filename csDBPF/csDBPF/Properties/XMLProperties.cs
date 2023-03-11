using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace csDBPF.Properties {
	/// <summary>
	/// Static class for interfacing with new_properties.xml file through <see cref="XMLExemplarProperty"/> objects.
	/// </summary>
	public static class XMLProperties {
		//TODO - make new_properties.xml path relative
		private const string xmlPath = "C:\\source\\repos\\csDBPF\\csDBPF\\csDBPF\\Properties\\new_properties.xml";

		private static readonly List<XMLExemplarProperty> _allProperties = new List<XMLExemplarProperty>();
		/// <summary>
		/// List of all properties included in new_properties.xml.
		/// </summary>
		public static List<XMLExemplarProperty> AllProperties { 
			get { return _allProperties; } 
		}



		/// <summary>
		/// Static constructor to populate the list of all properties from the XML file into <see cref="AllProperties"/>.
		/// </summary>
		/// <remarks>
		/// The XML structure is as follows: each XML tag is an XElement (element). Each element can have one or more XAttributes (attributes) which help describe the element. See new_properties.xsd for required vs optional attributes for Properties.
		/// </remarks>
		static XMLProperties() {
			//Use the much simpler XElement instead of XDocument which has fewer methods and properties to worry about.
			XElement xml = XElement.Load(xmlPath);
			IEnumerable<XElement> exemplarProperties = from prop in xml.Elements("PROPERTIES").Elements("PROPERTY")
													   select prop;

			foreach (XElement prop in exemplarProperties) {
				uint id = Convert.ToUInt32(prop.Attribute("ID").Value, 16);

				//For the rest of the attributes, we do not know which ones will exist for a property; directly setting them from the constructor results in a null reference exception. Examine then set each one individually.
				//https://stackoverflow.com/a/44929328 This is kind of hideous, but we are first checking if the XAttribute exists. If it does a string value is returned; null is returned if it does not exist. Throw this returned value into the TryParse. Since we have to deal with the possibility of the value being null and TryParse can only out non-nullable values, we need to check the result of TryParse whether it was successful or not. If it was successful we just return the out value, if it was not successful we finally just return null.
				XMLExemplarProperty exmp = new XMLExemplarProperty(
					id,
					prop.Attribute("Name").Value,
					DBPFPropertyDataType.LookupDataType(prop.Attribute("Type").Value),
					prop.Attribute("ShowAsHex").Value == "Y",
					short.TryParse((string) TryXAttributeExists(prop, "Count"), out short s) ? s : null,
					(string) TryXAttributeExists(prop, "Default"),
					int.TryParse((string) TryXAttributeExists(prop, "MinLength"), out int i1) ? i1 : null,
					int.TryParse((string) TryXAttributeExists(prop, "MaxLength"), out int i2) ? i2 : null,
					(string) TryXAttributeExists(prop, "MinValue"),
					(string) TryXAttributeExists(prop, "MaxValue"),
					uint.TryParse((string) TryXAttributeExists(prop, "Step"), out uint u3) ? u3 : null
				);
				_allProperties.Add(exmp);
			}

		}



		/// <summary>
		/// Generic helper function to determine if the XAttribute exists for the given XElement and return its value if it does, or null if not.
		/// </summary>
		/// <param name="element"><see cref="XElement"/> to examine</param>
		/// <param name="xname">Name of <see cref="XAttribute"/> to look for. Represents the <see cref="XName"/>.</param>
		/// <returns><see cref="XAttribute.Value"/> if XAttribute exists; null otherwise</returns>
		private static object TryXAttributeExists(XElement element, string xname) {
			XAttribute xattr = element.Attribute(xname);
			if (xattr == null) {
				return null;
			}
			return xattr.Value;
		}



		/// <summary>
		/// Queries new_properties.xml and returns the exemplar property (PROPERTY) element matching the specified ID.
		/// </summary>
		/// <param name="id">Property ID to lookup</param>
		/// <returns>A matching XMLExemplarProperty</returns>
		public static XMLExemplarProperty GetXMLProperty(uint id) {
			IEnumerable<XMLExemplarProperty> matchingPropery = from prop in AllProperties
															   where prop.ID == id
															   select prop;
			//Because of our filter there should always only be one result so we do not have to worry about iterating over the return
			return matchingPropery.First();
		}
		/// <summary>
		/// Queries new_properties.xml and returns the exemplar property (PROPERTY) element matching the specified Name.
		/// </summary>
		/// <param name="name">Name to lookup</param>
		/// <returns>A matching XMLExemplarProperty</returns>
		/// <remarks>
		/// Due to inconsistencies in how the Name field is saved in the xml file, spaces are ignored and the lowercase string is compared.
		/// </remarks>
		public static XMLExemplarProperty GetXMLProperty(string name) {
			IEnumerable<XMLExemplarProperty> matchingPropery = from prop in AllProperties
															   where prop.Name.ToLower().Replace(" ","") == name.ToLower().Replace(" ", "")
															   select prop;
			//Because of our filter there should always only be one result so we do not have to worry about iterating over the return
			return matchingPropery.First();
		}



		/// <summary>
		/// Look up a given property and return its identifier.
		/// </summary>
		/// <param name="nameToFind">Property name</param>
		/// <remarks>Match is *not* case sensitive and will disregard spaces because some property names have spaces and others are camel cased.</remarks>
		/// <returns>Property ID if found; 0 otherwise</returns>
		public static uint GetPropertyID(string nameToFind) {
			foreach (XMLExemplarProperty property in AllProperties) {
				if (property.Name.ToLower().Replace(" ", "") == nameToFind.ToLower().Replace(" ", "")) {
					return property.ID;
				}
			}
			return 0;
		}



		//TODO - some properties have values restricted to certain things - these are the OPTION lists ... currently unimplemented
	}
}
