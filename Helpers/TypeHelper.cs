using Newtonsoft.Json.Linq;
using System.Reflection.PortableExecutable;

namespace Ireport.Helpers
{
    public static class TypeHelper
    {
        /// <summary>
        /// Replaces the given headers strings for configred ones, only valid on Ireport
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<string> MatchingReportTypeHeaders(this IEnumerable<string> data, char separator = '|', JArray? reportTypeColumns = null)
        {
            var dataList = data.ToList();

            if(reportTypeColumns != null)
            {

                var headers = dataList[0].Split('|').ToList();

                for(int i = 0; i < headers.Count; i++)
                {
                    var columnCfg = reportTypeColumns.Where(head => head["id"].ToString() == headers[i]).FirstOrDefault();

                    if (columnCfg != null)
                    {
                        headers[i] = columnCfg["title"]?.ToString() ?? headers[i];
                    }
                }

                dataList[0] = string.Join(separator, headers);

            }

            return dataList;
            
        }



    }
}
