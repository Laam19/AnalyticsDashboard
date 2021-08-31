using ChoETL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using OfficeOpenXml;
using SimpleAnalyticsDashbord.Models;
using SimpleAnalyticsDashbord.Services.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;


namespace SimpleAnalyticsDashbord.Services
{
    public class AnalyticsService
    {
        private readonly IMongoCollection<ParentChildClass> _analytics2;
        private ILogger<AnalyticsService> logger;

        public AnalyticsService(ILogger<AnalyticsService> logger, IAnalyticsDatabaseSettings settings)
        {
            this.logger = logger;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);


            _analytics2 = database.GetCollection<ParentChildClass>(settings.AnalyticsCollectionName);
        }
        public void ConvertAndMergeModel(string filename)
        {

            var modelBuilder = new ModelBuilder();
            var data = modelBuilder.ConvertAndMergeModel(filename);
            _analytics2.InsertMany(data);
        }


        public List<ParentChildClass> Get()
        {
            var list = _analytics2.Find(jsonData => true).ToList();
            return list;
        }





        public List<ParentChildClass> GetDataByRange(string parent, string child, DateTime? startdate, DateTime? enddate)
        {
            var rangeData = _analytics2.Find(x => x.ParentCatagory == parent && x.MiddleCatagory == child && x.DateTime >= startdate && x.DateTime <= enddate).ToList();

            return rangeData;
        }

        private int GetTotalAndroid(string parent, string child, DateTime? startdate, DateTime? enddate)
        {
            var rangeData = _analytics2.Find(x => x.ParentCatagory == parent && x.MiddleCatagory == child && x.DateTime >= startdate && x.DateTime <= enddate).ToList();
            int android = 0;
            foreach (var data in rangeData)
            {
                if (data.ChildCatagory.Device == "Android Users")
                {
                    android += data.ChildCatagory.Value;
                }
            }
            return android;
        }
        public Dictionary<string, int> GetAndroid(string parent, DateTime? startdate, DateTime? enddate)
        {

            Dictionary<string, int> disk = new Dictionary<string, int>();
            var list = GetChild(parent);
            foreach (var data in list)
            {
                int value = GetTotalAndroid(parent, data, startdate, enddate);
                disk.Add(data, value);
            }
            return disk;
        }
        public Dictionary<string, int> GetIos(string parent, DateTime? startdate, DateTime? enddate)
        {

            Dictionary<string, int> disk = new Dictionary<string, int>();
            var list = GetChild(parent);
            foreach (var data in list)
            {
                int value = GetTotalIos(parent, data, startdate, enddate);
                disk.Add(data, value);
            }
            return disk;
        }
        private int GetTotalIos(string parent, string child, DateTime? startdate, DateTime? enddate)
        {
            var rangeData = _analytics2.Find(x => x.ParentCatagory == parent && x.MiddleCatagory == child && x.DateTime >= startdate && x.DateTime <= enddate).ToList();
            var ios = 0;
            foreach (var data in rangeData)
            {
                if (data.ChildCatagory.Device == "iOS Users")
                {
                    ios += data.ChildCatagory.Value;
                }
            }

            return ios;
        }


        //public void Remove(DateTime datetime) =>
        //    _analytics2.DeleteOne(dtomodel => dtomodel.DateTime == datetime);

        public void Remove()
        {

            var list = _analytics2.Find(jsonData => true).ToList();

            foreach (var data in list)
            {

                _analytics2.DeleteOne(x => x.ParentCatagory == data.ParentCatagory);

            }
        }
        private List<ParentChildClass> GetDataByRangeForExel(DateTime? startdate, DateTime? enddate)
        {
            var rangeData = _analytics2.Find(x => x.DateTime >= startdate && x.DateTime <= enddate).ToList();

            return rangeData;
        }

        public List<string> GetParent()
        {
            //int count = 0;
            var list = _analytics2.Find(jsonData => true).ToList();
            List<string> ParentList = new List<string>();
            foreach (var data in list)
            {


                if (!ParentList.Contains(data.ParentCatagory))
                {
                    ParentList.Add(data.ParentCatagory);

                }

            }
            return ParentList;
        }



        public List<string> GetChild(string _parent)
        {
            var list = _analytics2.Find(jsonData => true).ToList();
            List<string> ChildList = new List<string>();
            foreach (var data in list)
            {
                if (data.ParentCatagory == _parent && !ChildList.Contains(data.MiddleCatagory))
                {
                    ChildList.Add(data.MiddleCatagory);
                }
            }
            return ChildList;
        }
        public FileResult ReadFile(DateTime? startdate, DateTime? enddate)
        {
            List<string> monthList = new List<string>();
            List<ParentChildClass> RangeData = GetDataByRangeForExel(startdate, enddate);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var package = new ExcelPackage(new FileInfo("Upload\\Template\\NMEF_FINAL_TEMPLATE.xlsx"));
            //var package = new ExcelPackage(new FileInfo(@"C:\Users\nmefdev\Documents\NMEF_FINAL_TEMPLATE.xlsx"));


            List<string> cellList = new List<string>()
            {
                "AP","AQ","AD","AE","AA","AB","AG","AH","AM","AN","AJ","AK","AY","AZ","AV","AW","O","P","R","S","U","V","X","Y",
                "L","M","BB","BC","BE","BF","BH","BI","BK","BL","BN","BO","BQ","BR","BW","BX","BT","BU","F","G","I","J","C","D",

            };

            List<string> ParentList = new List<string>()
            {
                "Employees","PictorialIndex","Total Daily Play Time","Total Sessions Today","Vehicle Used","DAU","Accessories","Augmented Reality","Dealer Locator",
                "In Case of Emergency","Quick Reference Guide","Tutorial","Warning Light","Warning Message","Selected English","Selected Arabic","New Users"
            };
            var WorkSheetTemplate = package.Workbook.Worksheets[1];

            for (int k = startdate.Value.Month; k <= enddate.Value.Month; k++)
            {
                DateTimeFormatInfo mfi = new DateTimeFormatInfo();
                string strMonthName = mfi.GetMonthName(k).ToString();
                var sheet = package.Workbook.Worksheets.Add(strMonthName, WorkSheetTemplate);
                sheet.Cells["A4"].Value = strMonthName;
                monthList.Add(strMonthName);


                List<ParentChildClass> currentMonthData = RangeData.Where(r => r.DateTime.Month == k).ToList();
                for (int xx = 0; xx < DateTime.DaysInMonth(2021, k); xx++)
                {
                    sheet.Cells[$"B{4 + xx}"].Value = new DateTime(2021, k, xx + 1);
                }

                //var parentCount = 0;
                var cellNum = 0;

                foreach (var Data in ParentList)
                {


                    var childList = GetChild(Data);

                    if (Data == "Vehicle Used")
                    {
                        childList = new List<string> { "NISSAN MAXIMA", "NISSAN ALTIMA", "NISSAN KICKS", "NISSAN PATROL" };

                    }
                    if (Data == "PictorialIndex")
                    {
                        childList = new List<string> { "Front", "NIM", "Side", "Rear", "Interior" };

                    }
                    var childcount = childList.Count();
                    for (int j = 0; j < childList.Count; j++)
                    {
                        if (childList[j] == "Unique Users") continue;
                        var cell = 0;
                        foreach (var data in currentMonthData)
                        {

                            //ParentChild.Add(Data, GetChild(Data));




                            if (data.ParentCatagory == Data && data.MiddleCatagory == childList[j] && data.ChildCatagory.Device == "Android Users")
                            {



                                sheet.Cells[cellList[cellNum] + $"{cell + 4}"].Value = data.ChildCatagory.Value;
                                cell++;


                            }



                        }
                        cellNum++;
                        cell = 0;
                        foreach (var data in currentMonthData)
                        {


                            if (data.ParentCatagory == Data && data.MiddleCatagory == childList[j] && data.ChildCatagory.Device == "iOS Users")
                            {



                                sheet.Cells[cellList[cellNum] + $"{cell + 4}"].Value = data.ChildCatagory.Value;
                                cell++;


                            }



                        }
                        cellNum++;
                    }


                }

            }

            // generating cumulative data
            var WorkSheetTemplate1 = package.Workbook.Worksheets[0];
            var WorkSheetTemplate2 = package.Workbook.Worksheets[3];
            //get cumulative data cell list
            List<string> cumulativeDataCell = new List<string>
            {
                "C","D","F","G","I","J","L","M", "O", "P", "R", "S", "U", "V", "X", "Y",
                "AA","AB","AD","AE","AG","AH","AJ","AK", "AM", "AN", "AP", "AQ", "AS", "AT","AV", "AW", "AY", "AZ",
                "BB","BC","BE","BF","BH","BI","BK","BL", "BN", "BO", "BQ", "BR", "BT", "BU", "BW", "BX"

            };
            // WorkSheetTemplate1.Cells["C6"].Formula = WorkSheetTemplate2.Cells["=SUM(C4: C34)"].Address;
            // WorkSheetTemplate1.Cells[$"C{6}"].Formula = = January!C35;
            // WorkSheetTemplate1.Cells["C6"].Formula = "=SUM("+  WorkSheetTemplate2.Cells[C4:C34]);
            //WorkSheetTemplate1.Cells["C6"].Formula = $"= January!C35";
            var cumulativeStartCellNum = 6;
            foreach (var month in monthList)
            {
                foreach (var cumulativeDataset in cumulativeDataCell)
                {
                    WorkSheetTemplate1.Cells[$"{cumulativeDataset}{cumulativeStartCellNum}"].Formula = $"= {month}!{cumulativeDataset}35";
                }
                cumulativeStartCellNum++;
            }

            //insert data to the month cell

            string filePath = "Upload\\Template\\UpdatedTemplate.xlsx";
            string filename = "UpdatedTemplate.xlsx";
            var newFile = new FileInfo(filePath);

            package.SaveAs(newFile);

            //FileContentResult result = new FileContentResult(System.IO.File.ReadAllBytes(filePath),
            //"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            //{
            //    FileDownloadName = "otherfile.xlsx"
            //};
            //return result;




            var bytes = System.IO.File.ReadAllBytes(filePath);

            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";


            var fileContentResult = new FileContentResult(bytes, contentType)
            {
                FileDownloadName = filename
            };

            return fileContentResult;
        }




    }
}