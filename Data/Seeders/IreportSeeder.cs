
using Idata.Data;
using Ireport.Data.Seeders;
using System.Xml.Serialization;

namespace Ramp.Data.Seeders
{
    public class IreportSeeder
    {

        public static void Seed(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                


                var context = serviceScope.ServiceProvider.GetService<IdataContext>();

                context.Database.EnsureCreated();

				IreportModuleSeeder.Seed(applicationBuilder);


				context.SaveChanges();


                // RampProductsSeeder.Seed(applicationBuilder);
            }
        }


    }
}
