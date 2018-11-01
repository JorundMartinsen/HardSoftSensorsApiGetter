using CsvHelper;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Net;

namespace HardSoftSensorsApiGetter {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("This is a csv creator for api.nilu.no.");
            string api = @"https://api.nilu.no/obs/historical/";
            string options = string.Empty;
            string[] locations = { "Sverresgate", "Furulund", "Gamle Haukenes", "Gamle Lensmannsdalen", "Gamle Ås, Heistad", "Haukenes", "Lensmannsdalen", "Øyekast" };
            int fromYear = 2013;

            string[] components = { "CO", "NO", "NO2", "NOx", "O3", "PM1", "PM10", "PM2.5", "SO2" };

            foreach (string location in locations) {
                foreach (string component in components) {
                    Console.WriteLine(String.Format("Getting data for {0}, {1}", location, component));
                    string csv = string.Empty;
                    bool header = true;
                    for (int i = fromYear; i < 2019; i++) {
                        Console.WriteLine(String.Format("Year: {0}", i));
                        for (int j = 1; j < 12; j++) {
                            string from = String.Format("{0}-{1}-{2}", i, j, 1);
                            string to = String.Format("{0}-{1}-{2}", i, j, DateTime.DaysInMonth(i, j));
                            options = String.Format(@"{0}/{1}/{2}?components={3}", from, to, location, component);

                            csv += jsonToCSV(Get(api + options), ",", header);
                            header = false;
                        }
                    }
                    Directory.CreateDirectory(@"D:\" + location + @"\");
                    File.WriteAllText(@"D:\" + location + @"\" + location + component + ".csv", csv);
                }
            }
        }
        public static DataTable jsonStringToTable(string jsonContent) {
            DataTable dt = JsonConvert.DeserializeObject<DataTable>(jsonContent);
            return dt;
        }
        public static string jsonToCSV(string jsonContent, string delimiter, bool header) {
            StringWriter csvString = new StringWriter();
            using (var csv = new CsvWriter(csvString)) {
                csv.Configuration.Delimiter = delimiter;

                using (var dt = jsonStringToTable(jsonContent)) {
                    if (header) {
                        foreach (DataColumn column in dt.Columns) {
                            csv.WriteField(column.ColumnName);
                        }
                        csv.NextRecord();
                    }

                    for (int i = 1; i < dt.Columns.Count; i++) {
                            csv.WriteField(dt.Rows[0][i]);
                        }
                    csv.NextRecord();
                }

            }
            return csvString.ToString();
        }
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
}
