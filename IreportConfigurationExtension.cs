using Ireport.Repositories.Interfaces;
using Ireport.Repositories;
using Ireport.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;
using System.Xml.Serialization;
using Idata.Data;

namespace Ireport
{
    public static class IreportServiceProvider
    {


        public static WebApplicationBuilder? Boot(WebApplicationBuilder? builder)
        {
            //TODO Implement controllerBase to avoid basic crud redundant code
            builder.Services.AddControllers().ConfigureApplicationPartManager(o =>
            {
                o.ApplicationParts.Add(new AssemblyPart(typeof(IreportServiceProvider).Assembly));
            });


            builder.Services.AddScoped(typeof(IReportTypeRepository), typeof(ReportTypeRepository));
            builder.Services.AddScoped(typeof(IReportRepository), typeof(ReportRepository));
            builder.Services.AddScoped(typeof(IFolderRepository), typeof(FolderRepository));
            return builder;

        }
    }
}
