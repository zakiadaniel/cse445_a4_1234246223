using System;
using System.Xml.Schema;
using System.Xml;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Xml.Linq;



/**
 * This template file is created for ASU CSE445 Distributed SW Dev Assignment 4.
 * Please do not modify or delete any existing class/variable/method names. However, you can add more variables and functions.
 * Uploading this file directly will not pass the autograder's compilation check, resulting in a grade of 0.
 * **/


namespace ConsoleApp1
{


    public class Program
    {
        public static string xmlURL = "https://zakiadaniel.github.io/cse445_a4_1234246223/Hotels.xml";
        public static string xmlErrorURL = "https://zakiadaniel.github.io/cse445_a4_1234246223/HotelsErrors.xml";
        public static string xsdURL = "https://zakiadaniel.github.io/cse445_a4_1234246223/Hotels.xsd";

        public static void Main(string[] args)
        {
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);


            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);


            result = Xml2Json(xmlURL);
            Console.WriteLine(result);
        }

        // Q2.1
        public static string Verification(string xmlUrl, string xsdUrl)
        {
            try
            {
                // Implement XML validation against XSD here
                // create a StringBuilder to collect error messages
                StringBuilder errors = new StringBuilder();
                bool hasErrors = false;

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;

                //download xsd from xsdUrl
                using (WebClient client = new WebClient())
                {
                    string xsdContent = client.DownloadString(xsdUrl);
                    using (StringReader sr = new StringReader(xsdContent))
                    {
                        XmlSchema schema = XmlSchema.Read(sr, null);
                        settings.Schemas.Add(schema);
                    }
                }

                // set up the validation event handler
                settings.ValidationEventHandler += (sender, e) =>
                {
                    hasErrors = true;
                    errors.AppendLine($"Validation Error: {e.Message}");
                    errors.AppendLine($"Severity: {e.Severity}");

                    if (e.Exception != null)
                    {
                        errors.AppendLine($"Line Number: {e.Exception.LineNumber}, Line Position: {e.Exception.LinePosition}");
                    }
                };

                using (WebClient client = new WebClient())
                {
                    string xmlContent = client.DownloadString(xmlUrl);
                    using (StringReader sr = new StringReader(xmlContent))
                    using (XmlReader reader = XmlReader.Create(xmlStringReader, settings))
                    {
                        while (reader.Read()) { }
                    }
                }

                if (hasErrors)
                {
                    return errors.ToString().TrimEnd();
                }
                else
                {
                    return "No Error";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            //return "No Error" if XML is valid. Otherwise, return the desired exception message.
        }

        public static string Xml2Json(string xmlUrl)
        {
            try
            {
                // download xml from xmlUrl
                string xmlContent;
                using (WebClient client = new WebClient())
                {
                    xmlContent = client.DownloadString(xmlUrl);
                }

                // load xml into XmlDocument
                XDocument xdoc = XDocument.Parse(xmlContent);

                // Create JSON structure
                var hotelsArray = new System.Collections.Generic.List<object>();
                foreach (var hotelElement in xdoc.Root.Elements("Hotel"))
                {
                    var hotel = new System.Collections.Generic.Dictionary<string, object>();

                    // add elements to hotel dictionary - Name
                    var nameElement = hotelElement.Element("Name");
                    if (nameElement != null)
                    {
                        hotel["Name"] = nameElement.Value;
                    }
                    // add phone numbers in an array
                    var phoneElements = hotelElement.Elements("Phone");
                    var phones = new System.Collections.Generic.List<string>();
                    foreach (var phone in phoneElements)
                    {
                        phones.Add(phone.Value);
                    }
                    if (phones.Count > 0)
                    {
                        hotel["Phone"] = phones;
                    }

                    // add address as a nested object
                    var addressElement = hotelElement.Element("Address");
                    if (addressElement != null)
                    {
                        var address = new System.Collections.Generic.Dictionary<string, string>();
                        address["Number"] = addressElement.Element("Number")?.Value ?? "";
                        address["Street"] = addressElement.Element("Street")?.Value ?? "";
                        address["City"] = addressElement.Element("City")?.Value ?? "";
                        address["State"] = addressElement.Element("State")?.Value ?? "";
                        address["Zip"] = addressElement.Element("Zip")?.Value ?? "";
                        // add nearest airport attribute
                        var nearestAirport = addressElement.Attribute("NearestAirport");
                        if (nearestAirport != null)
                        {
                            address["NearestAirport"] = nearestAirport.Value;
                        }
                        hotel["Address"] = address;
                    }

                    // add option attribute Rating
                    var ratingAttr = hotelElement.Attribute("Rating");
                    if (ratingAttr != null)
                    {
                        hotel["_Rating"] = ratingAttr.Value;
                    }

                    hotelsArray.Add(hotel);
                }

                // wrap hotels array in root object
                var jsonObject = new
                {
                    Hotels = new
                    {
                        Hotel = hotelsArray
                    }
                };
                // serialize to JSON
                string jsonText = JsonConvert.SerializeObject(jsonObject, Newtonsoft.Json.Formatting.Indented);
                try
                {
                    var testDeserialize = JsonConvert.DeserializeObject(jsonText);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: JSON may not be deserializable: {ex.Message}");
                }
                // The returned jsonText needs to be de-serializable by Newtonsoft.Json package. (JsonConvert.DeserializeXmlNode(jsonText))
                return jsonText;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
