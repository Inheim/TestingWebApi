using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TestingWebApi.Controllers
{
    [ApiController]
    [Tags("Data")]
    [Route("[controller]")]
    public class DataController : ControllerBase
    {

        private readonly ILogger<DataController> _logger;

        public DataController(ILogger<DataController> logger)
        {
            _logger = logger;
        }

        [HttpPost("PostCSV")]
        [Produces("application/json")]
        public string PostCSV(IFormFile file)
        {
            if (!file.FileName.EndsWith(".csv")) return "Wrong file extension";
            
            using var fileStream = file.OpenReadStream();

            using (var reader = new StreamReader(fileStream, System.Text.Encoding.UTF8))
            using (var csvReader = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
            {
                List<CSVData> records;
                List<Data> data = new List<Data>{ };
                int lastid = 1;
                using (ApplicationContext db = new ApplicationContext())
                {
                    List<Result> last = db.Result.OrderByDescending(d => d.id).Take(1).ToList();
                    if (last.Count > 0) lastid = last[0].id + 1;
                }
                Result result = new Result
                {
                    name = file.FileName,
                    delta = 0,
                    date = DateTime.Now,
                    averageexecutiontime = 0,
                    averagevalue = 0,
                    medianvalue = 0,
                    maxvalue = int.MinValue,
                    minvalue = int.MaxValue
                };
                DateTime maxdate = new DateTime(1999, 1, 1);
                DateTime mindate = DateTime.Now;


                try
                {
                    records = csvReader.GetRecords<CSVData>().ToList();
                } catch (CsvHelper.TypeConversion.TypeConverterException)
                {
                    return $"invalid data file: unmatching type or missing";
                }
                

                if (records.Count > 10000 || records.Count == 0) return $"invalid data file: Lines count = {records.Count}";

                foreach (CSVData d in records)
                {
                    // validating data and adding in to final AddList

                    if ((DateTime.Compare(d.date, new DateTime(2000, 1, 1))<=0 || DateTime.Compare(d.date, DateTime.Now)>0) ||
                        (d.executiontime < 0) || 
                        (d.value < 0)) return $"invalid data at {d.date} | {d.executiontime} | {d.value}";
                    d.date = d.date.ToUniversalTime();
                    data.Add(new Data { id=0, date=d.date, executiontime=d.executiontime, value=d.value, fileid = lastid });

                    if (DateTime.Compare(d.date, maxdate) > 0) maxdate = d.date;
                    if (DateTime.Compare(d.date, mindate) < 0) mindate = d.date;
                    if (DateTime.Compare(d.date, result.date) < 0) result.date = d.date;
                    if (d.value > result.maxvalue) result.maxvalue = d.value;
                    if (d.value < result.minvalue) result.minvalue = d.value;
                    result.averageexecutiontime += d.executiontime;
                    result.averagevalue += d.value;
                }
                result.delta = (int)maxdate.Subtract(mindate).TotalSeconds;
                result.averageexecutiontime /= records.Count;
                result.averagevalue /= records.Count;
                records = records.OrderBy(r => r.value).ToList();
                if (records.Count % 2 == 1) result.medianvalue = records[records.Count / 2].value;
                else result.medianvalue = (records[records.Count / 2].value + records[records.Count / 2 - 1].value) / 2;

                using (ApplicationContext db = new ApplicationContext())
                {
                    var results = db.Result.ToList();
                    if (results != null)
                    {
                        if (results.Any(res => res.name == file.FileName))
                        {
                            var toChange = results.Find(res => res.name == file.FileName);
                            db.Entry(toChange).CurrentValues.SetValues(result);
                        } else db.Result.Add(result);
                    }
                    else db.Result.Add(result);

                    db.Data.AddRange(data);
                    db.SaveChanges();
                }

            }
            return "file uploaded";
        }

        [HttpGet("SearchInResults")]
        public IEnumerable<Result> SearchInResults(DateTime maxdate, DateTime mindate, string filename = "", int averagevalmax = int.MaxValue, int averagevalmin = int.MinValue, int averageextimemax = int.MaxValue, int averageextimemin = int.MinValue)
        {
            if (maxdate == DateTime.MinValue)
            {
                maxdate = DateTime.MaxValue;
            }
            using (ApplicationContext db = new ApplicationContext())
            {
                var results = db.Result.ToList();
                
                results = results.FindAll(res => res.name.StartsWith(filename));
                results = results.FindAll(res => res.averagevalue <= averagevalmax && res.averagevalue >= averagevalmin);
                results = results.FindAll(res => res.averageexecutiontime <= averageextimemax && res.averageexecutiontime >= averageextimemin);

                return results;
            }

            
        }

        [HttpGet("GetLastTen")]
        public IEnumerable<Data> GetLastTen(string filename = "")
        {

            using (ApplicationContext db = new ApplicationContext())
            {
                List<Data> toReturn = new List<Data> { };

                int fileid;
                if (filename!="" && db.Result.Any(res => res.name.StartsWith(filename)))
                {
                    fileid = db.Result.Where(res => res.name.StartsWith(filename)).ToList()[0].id;
                    return db.Data.Where(res => res.fileid == fileid).OrderByDescending(d => d.date).Take(10).Reverse().ToList(); ;
                }

                return toReturn = db.Data.OrderByDescending(d => d.date).Take(10).Reverse().ToList();
            }

            
        }
    }
}
