using Core;
using Core.Interfaces;
using Idata.Data.Entities.Ireport;
using Idata.Data.Entities.Ireport;

namespace Ireport.Repositories.Interfaces
{
    public interface IReportRepository : IRepositoryBase<Report>
    {
        public Task<dynamic> GetReportJson(UrlRequestBase request);
		public Task<dynamic> GetReportFile(UrlRequestBase request, BodyRequestBase bodyRequest);
	}
}
