
using APIBase.Models.POS;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace APIBase.Helpers
{
    public static class NumberSeriesHelper
    {
        public static string get_new_id_for_insert(string number_series_id, POSContext _context, int? timeout = 3, DateTime? date = null)
        {
            //var ns = _context.NumberSeries.Find(ns_id);
            //var new_last_used = (ns.LastUsed is null ? int.Parse(ns.Start) : int.Parse(ns.LastUsed) + ns.Increment).ToString().PadLeft(ns.Start.Length, '0');
            //var new_ns = ns.Alpha + new_last_used;

            //ns.LastUsed = new_last_used;

            //return new new_number_series_result(new_ns, ns);

            var retrycounter = 0;
            var retryupto = DateTime.Now;



            var nsl = _context.NumberSeriesLines.Where(x => x.NumberSeriesId == number_series_id &&
                 x.StatusId == "st-active" && (date != null ? x.StartDate < date : true))
                .OrderByDescending(x => x.StartDate).First();




        tryagain:
            //get ns line
            if (retrycounter > 0)
            {
                _context.Entry(nsl).Reload();
            }

            //cehck if nsl is null after first load or after retry
            if (nsl == null)
            {
                return null;
            }
            //generate new_numeric_id and alpha numeric  id
            var new_num_id = (nsl.LastUsed is null ? int.Parse(nsl.Start) : int.Parse(nsl.LastUsed) + nsl.Increment).ToString().PadLeft(nsl.Start.Length, '0');
            var new_id = nsl.Alpha + new_num_id;

            //set last_used to new id
            nsl.LastUsed = new_num_id;

            try
            {
                _context.NumberSeriesLines.Update(nsl);
                _context.SaveChanges();
                return new_id;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var exceptionEntry = ex.Entries.Single();
                var databaseEntry = exceptionEntry.GetDatabaseValues();
                if (databaseEntry == null)
                {
                    return null;
                }
                else
                {
                    if (DateTime.Now.Subtract(retryupto).Seconds < timeout)
                    {
                        retrycounter++;
                        goto tryagain;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public static string get_new_id_for_offline(string number_series_id, POSContext _context, int? timeout = 3, DateTime? date = null)
        {

            var nsl = _context.NumberSeriesLines.Where(x => x.NumberSeriesId == number_series_id &&
                 x.StatusId == "st-active" && (date != null ? x.StartDate < date : true))
                .OrderByDescending(x => x.StartDate).First();

            //cehck if nsl is null after first load or after retry
            if (nsl == null)
            {
                return null;
            }
            //generate new_numeric_id and alpha numeric  id
            var new_num_id = (nsl.LastUsed is null ? int.Parse(nsl.Start) : int.Parse(nsl.LastUsed) + nsl.Increment).ToString().PadLeft(nsl.Start.Length, '0');
            var new_id = nsl.Alpha + new_num_id;

            return new_id;
        }
    }
}
