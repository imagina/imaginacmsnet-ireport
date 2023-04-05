using Core;
using Core.Repositories;
using Idata.Data;
using Idata.Data.Entities.Ireport;
using Ireport.Repositories.Interfaces;
using OfficeOpenXml.Table.PivotTable;
using System.Xml.Serialization;

namespace Ireport.Repositories
{
    public class ReportTypeRepository : RepositoryBase<ReportType>, IReportTypeRepository
    {

        public ReportTypeRepository()
        {

        }
		
	}
}
