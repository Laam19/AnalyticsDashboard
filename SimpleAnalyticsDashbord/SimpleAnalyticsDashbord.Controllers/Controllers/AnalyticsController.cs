using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleAnalyticsDashbord.Models;
using SimpleAnalyticsDashbord.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AnalyticsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {

        private readonly AnalyticsService _analyticsService;


        public AnalyticsController(AnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;


        }


        [HttpGet("datetime", Name = "GetDataByRange")]
        public ActionResult<ParentChildClass> GetDataByRange(string parent, string child, DateTime startdatetime, DateTime enddatetime)
        {
            var data = _analyticsService.GetDataByRange(parent, child, startdatetime, enddatetime);
            if (data == null)
            {
                return NotFound();
            }
            return new JsonResult(data);
        }

        [HttpGet("TotalAndroid", Name = "GetAndroid")]
        public ActionResult<Dictionary<string, int>> GetAndroid(string parent, DateTime? startdate, DateTime? enddate)
        {
            Dictionary<string, int> disk = new Dictionary<string, int>();
            disk = _analyticsService.GetAndroid(parent, startdate, enddate);
            if (disk == null)
            {
                return NotFound();
            }
            return new JsonResult(disk);
        }
        [HttpGet("TotalIos", Name = "GetIos")]
        public ActionResult<Dictionary<string, int>> GetIos(string parent, DateTime? startdate, DateTime? enddate)
        {
            Dictionary<string, int> disk = new Dictionary<string, int>();
            disk = _analyticsService.GetIos(parent, startdate, enddate);
            if (disk == null)
            {
                return NotFound();
            }
            return new JsonResult(disk);
        }


        [HttpGet]
        public ActionResult<List<ParentChildClass>> Get()
        {
            var list = _analyticsService.Get();


            return new JsonResult(list);
        }

        [HttpPost("upload", Name = "upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile(
         IFormFile file,
         CancellationToken cancellationToken)
        {
            if (CheckIfExcelFile(file))
            {

                await WriteFile(file);
            }
            else
            {
                return BadRequest(new { message = "Invalid file extension" });
            }

            return Ok();
        }
        private bool CheckIfExcelFile(IFormFile file)
        {
            var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
            return (extension == ".csv");
        }

        private async Task<bool> WriteFile(IFormFile file)
        {
            bool isSaveSuccess = false;
            string fileName;
            try
            {
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                fileName = file.FileName;
                var pathBuilt = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Upload\\files");

                if (!Directory.Exists(pathBuilt))
                {
                    Directory.CreateDirectory(pathBuilt);
                }

                var path = Path.Combine(Directory.GetCurrentDirectory(), "Upload\\files",
                   fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                isSaveSuccess = true;
                _analyticsService.ConvertAndMergeModel(fileName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }


            return isSaveSuccess;
        }

        [HttpGet("read")]

        public ActionResult ReadFile(DateTime startdatetime, DateTime enddatetime)
        {
            var res = _analyticsService.ReadFile(startdatetime, enddatetime);
            return res;
        }

        [HttpDelete("Delete")]
        public IActionResult Delete()
        {
            var data = _analyticsService.Get();
            if (data == null)
            {
                return NotFound();
            }
            _analyticsService.Remove();

            return NoContent();
        }


        [HttpGet("GetParents")]
        public ActionResult<List<string>> GetParent()
        {
            var list = _analyticsService.GetParent();


            return new JsonResult(list);
        }


        [HttpGet("GetChilds")]
        public ActionResult<List<string>> GetChild(string parent)
        {
            var list = _analyticsService.GetChild(parent);


            return new JsonResult(list);
        }
    }
}