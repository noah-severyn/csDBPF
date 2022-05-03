using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace csDBPF.Properties {
	public static class XMLProperties {
		private static readonly Dictionary<uint, XMLExemplarProperty> xmlProperties = new Dictionary<uint, XMLExemplarProperty>();
		public static ImmutableDictionary<uint, XMLExemplarProperty> AllProperties;
		private const string xmlPath = "C:\\Users\\Administrator\\OneDrive\\Documents\\csDBPF\\csDBPF\\csDBPF\\Properties\\new_properties.xml"; //TODO - make this path relative

		/// <summary>
		/// Static constructor to populate the list of all properties from the XML file.
		/// </summary>
		static XMLProperties() {
			LoadXMLProperties(xmlPath);
			AllProperties = xmlProperties.ToImmutableDictionary();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// The XML structure is as follows: each XML tag is an XElement (element). Each element can have one or more XAttributes (attributes) which help describe the element. See new_properties.xsd for required vs optional attributes for Properties.
		/// </remarks>
		private static void LoadXMLProperties(string xmlPath) {
			//XDocument is a great way to read XML files. Since we do not need to do anything too complicated and do not need to do any XML writing, we can go with the much simpler XElement instead which has fewer methods and properties to worry about.
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
					short.TryParse((string) TryXAttributeExists(prop, "Count"), out short s) ? s : (short?) null,
					(string) TryXAttributeExists(prop, "Default"),
					int.TryParse((string) TryXAttributeExists(prop, "MinLength"), out int i1) ? i1 : (int?) null,
					int.TryParse((string) TryXAttributeExists(prop, "MaxLength"), out int i2) ? i2 : (int?) null,
					(string) TryXAttributeExists(prop, "MinValue"),
					(string) TryXAttributeExists(prop, "MaxValue"),
					uint.TryParse((string) TryXAttributeExists(prop, "Step"), out uint u3) ? u3 : (uint?) null
				);

				xmlProperties.Add(id, exmp);
			}


			/// <summary>
			/// Generic Helper function to determine if the XAttribute exists for the given XElement and return its value if it does, or null if not.
			/// </summary>
			/// <param name="element"><see cref="XElement"/> to examine</param>
			/// <param name="xname">Name of <see cref="XAttribute"/> to look for. Represents the <see cref="XName"/>.</param>
			/// <returns><see cref="XAttribute.Value"/> if XAttribute exists; null otherwise</returns>
			static object TryXAttributeExists(XElement element, string xname) {
				XAttribute xattr = element.Attribute(xname);
				if (xattr == null) {
					return null;
				}
				return xattr.Value;
			}
		}


		/// <summary>
		/// Queries new_properties.xml and returns the exemplar property (PROPERTY) element matching the specified ID.
		/// </summary>
		/// <param name="id">Property ID to lookup</param>
		/// <returns>XElement of the specified property ID</returns>
		public static XElement GetXMLProperty(uint id) {
			XElement xml = XElement.Load(xmlPath);
			//Within XML doc, there is a single element of PROPERTIES which contain many elements PROPERTY
			string str = "0x" + DBPFUtil.UIntToHexString(id, 8).ToLower();
			IEnumerable<XElement> matchingExemplarProperty = from prop in xml.Elements("PROPERTIES").Elements("PROPERTY")
															 where prop.Attribute("ID").Value == "0x" + DBPFUtil.UIntToHexString(id, 8).ToLower()
															 select prop;
			//LINQ query returns an IEnumerable object, but because of our filter there should always only be one result so we do not have to worry about iterating over the return
			return matchingExemplarProperty.First();
		}

		//TODO - some properties have values restricted to certain things - these are the OPTION lists ... currently unimplemented
	}
}
