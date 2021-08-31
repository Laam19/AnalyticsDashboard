using ChoETL;
using Newtonsoft.Json;
using SimpleAnalyticsDashbord.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SimpleAnalyticsDashbord.Services.Services
{
    public class ModelBuilder
    {
        public List<ParentChildClass> ConvertAndMergeModel(string filename)
        {

            var location = Directory.GetCurrentDirectory();

            string csv = location + "\\Upload\\files\\" + filename;

            return BuildModel(csv);

        }
        private List<ParentChildClass> BuildModel(string path)
        {
            var json = CSVToJSONString(path);

            if (json == null)
            {
                // Throw Exception
            }
            List<ParentChildClass> parentChildClasses = new List<ParentChildClass>();

            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                string currentDateTime = "";
                string propertyName = "";
                bool isDateTimeMonth = false;
                while (reader.Read())
                {

                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        if (reader.Value.ToString().Equals("DateTime"))
                        {
                            propertyName = reader.Value.ToString();
                            isDateTimeMonth = true;
                        }

                        propertyName = reader.Value.ToString();

                    }
                    else if (reader.TokenType == JsonToken.String)
                    {

                        if (isDateTimeMonth == true)
                        {
                            currentDateTime = reader.Value.ToString();
                            if (currentDateTime.Contains(" "))
                            {
                                string[] splitDate = currentDateTime.Split(" ");
                                currentDateTime = splitDate[0].Trim();
                                string[] splitDate2 = currentDateTime.Split("\"");
                                currentDateTime = splitDate2[1].Trim();


                            }

                            isDateTimeMonth = false;
                        }
                        else
                        {
                            // Split and Assign
                            ParentChildClass parentChildClass = new ParentChildClass();
                            int value = reader.Value.CastTo(Int32.MaxValue);
                            ParentChildKey parentChildKey = splitToParentChildClass(propertyName);
                            parentChildClass.DateTime = Convert.ToDateTime(currentDateTime);
                            parentChildClass.ParentCatagory = parentChildKey.ParentKey;
                            parentChildClass.MiddleCatagory = parentChildKey.ChildKey;
                            parentChildClass.ChildCatagory = new ChildClass(parentChildKey.Device, value);
                            parentChildClasses.Add(parentChildClass);


                        }
                    }
                }
            }


            return parentChildClasses;

        }

        private ParentChildKey splitToParentChildClass(string fullPropertyName)
        {
            ParentChildKey parentChildKey = new ParentChildKey();
            string[] family = fullPropertyName.Split("-");
            for (int i = 0; i < family.Length; i++)
            {
                if (family[i].Trim().Equals("Name"))
                {
                    continue;
                }
                if (i == 0)
                {
                    parentChildKey.ParentKey = family[i].Trim();
                }
                if (i == 1)
                {
                    parentChildKey.Device = family[i].Trim();
                }
                else if (i == family.Length - 1)
                {
                    parentChildKey.ChildKey = family[i].Trim();
                }
            }

            if (parentChildKey.ChildKey == null)
                parentChildKey.ChildKey = "All";

            return parentChildKey;
        }

        private string CSVToJSONString(string csv)
        {
            StringBuilder jsonOutput = new StringBuilder();
            try
            {

                using (var csvReader = new ChoCSVReader(csv).WithFirstLineHeader())
                {
                    using (var JsonWriter = new ChoJSONWriter(new StringWriter(jsonOutput)))
                    {
                        JsonWriter.Write(csvReader);
                    }

                }

                var json = jsonOutput.ToString();
                return json;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

    }
}
