using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace HardSoftSensorsApiGetter {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("This is a csv creator for api.nilu.no.");
            Console.WriteLine(@"Specify root folder for storing csv's: Standard: C:\nilu\");
            string root = Console.ReadLine();
            if (string.IsNullOrEmpty(root)) {
                root = @"C:\nilu\";
            }
            else if (!root.EndsWith(@"\")) root += @"\";

            Console.WriteLine("Saving to: " + root);
            Console.WriteLine("Press enter to continue");
            Console.ReadLine();

            // Sets parameters
            string api = @"https://api.nilu.no/obs/historical/";
            string options = string.Empty;
            string[] locations = { "Sverresgate", "Furulund", "Gamle Haukenes", "Gamle Lensmannsdalen", "Gamle Ås, Heistad", "Haukenes", "Lensmannsdalen", "Øyekast" };
            int fromYear = 2013;

            string[] components = { "CO", "NO", "NO2", "NOx", "O3", "PM1", "PM10", "PM2.5", "SO2" };

            // Loops through every location and component and saves results as csv file.
            foreach (string location in locations) {
                foreach (string component in components) {
                    Console.WriteLine(String.Format("Getting data for {0}, {1}", location, component));
                    string csv = string.Empty;
                    bool header = true;
                    // From a given year up to and including this year
                    for (int i = fromYear; i <= DateTime.Now.Year; i++) {
                        Console.WriteLine(String.Format("Year: {0}", i));
                        for (int j = 1; j < 12; j++) {
                            // Creates nilu api friendly options
                            string from = String.Format("{0}-{1}-{2}", i, j, 1);
                            string to = String.Format("{0}-{1}-{2}", i, j, DateTime.DaysInMonth(i, j));
                            options = String.Format(@"{0}/{1}/{2}?components={3}", from, to, location, component);

                            // Gets and adds each month to the csv string
                            csv += jsonToCSV(Get(api + options), header);
                            header = false;
                        }
                    }
                    //Saves the file
                    if (!string.IsNullOrEmpty(csv)) {
                        Directory.CreateDirectory(root + location + @"\");
                        File.WriteAllText(root + location + @"\" + location + component + ".csv", csv);
                    }
                }
            }
        }

        /// <summary>
        /// Reads the string value into the object nilu. Formats the nilu object as a csv file.
        /// </summary>
        /// <param name="jsonContent">Json string to format</param>
        /// <param name="header">Include header</param>
        /// <returns>csv formatted string</returns>
        public static string jsonToCSV(string jsonContent, bool header) {
            string s = "";
            try {
                List<nilu> datas = JsonConvert.DeserializeObject<List<nilu>>(jsonContent);
                nilu data = datas[0];
                if (header) {
                    s = string.Format("Station: {0}, Location: Latitude: {1} Longitude: {2}, Component: {3}, Unit: {4}", data.station, data.latitude, data.longitude, data.component, data.unit);
                    s += "\r\n";
                    s += "FromTime, ToTime, Value";
                }
                foreach (values v in data.values) {
                    s += "\r\n";
                    s += string.Format("{0},{1},{2}", v.fromTime, v.toTime, v.value);
                }
            }
            catch (Exception e) {

            };
            return s;
        }

        /// <summary>
        /// Gets the Json from nilu as a string
        /// </summary>
        /// <param name="link">Link to nilus api</param>
        /// <returns>Json formatted string</returns>
        private static string Get(string link) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(link);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            string html = string.Empty;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream)) {
                html = reader.ReadToEnd();
            }

            return html;
        }
    }

    /// <summary>
    /// Class for interpreting data from nilu.no
    /// </summary>
    class nilu {
        public string zone { get; set; }
        public string municipality { get; set; }
        public string area { get; set; }
        public string station { get; set; }
        public string eoi { get; set; }
        public string component { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string timestep { get; set; }
        public string unit { get; set; }
        public List<values> values { get; set; }
    }
    /// <summary>
    /// Support class for nilu class
    /// </summary>
    class values {
        public string fromTime { get; set; }
        public string toTime { get; set; }
        public string value { get; set; }
    }
}
