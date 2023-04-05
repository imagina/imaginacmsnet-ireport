using Core;
using Core.Repositories;
using Ireport.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using System.Xml.Serialization;
using Idata.Data.Entities.Ireport;
using TypeSupport.Extensions;
using System.Linq.Dynamic.Core;
using Idata.Data;
using Idata.Data.Entities.Iprofile;

namespace Ireport.Repositories
{
    public class FolderRepository : RepositoryBase<Folder>, IFolderRepository
    {
		public FolderRepository()
        {
        }
        public override void CustomFilters(ref IQueryable<Folder> query, ref UrlRequestBase? requestBase)
        {
            query = query.OrderBy(rep => rep.position);

			if (!requestBase.currentContextUser.HasAccess("ireport.reports.index-all"))
			{
				var userRoles = (List<Role>)requestBase.currentContextUser.roles;

				var user = (User)requestBase.currentContextUser;

				query = query.Include("roles");
				query = query.Include("users");
           
				//Get the folders by users assigned and also get by roles of the current user roles

				query = query.Where(f => f.roles.Count() == 0 || f.roles.Any(r => userRoles.Contains(r)));
				query = query.Where(f => f.users.Count() == 0 || f.users.Contains(user));


			}
		}

        public override async Task SyncRelations(object? input, dynamic relations, dynamic dataContext)
        {
            //First clear current relations 
            var model = (Folder?)input;

            //List for storing relations given in front.
            List<long?> relationIds;


           

            //new dbContext is necesary for avoid strick id checking in previous context
            //Already implements IDisposable so when finish invoke gets disposed
            using (var db = new IdataContext())
            {
                //get the model again
                var internalModel = await db.Folders.Where(u => u.id == model.id).FirstOrDefaultAsync();

                //Asign relations
                if (relations.ContainsKey("reports"))
                {
                    //clear all current relations
                    model.reports?.Clear();

                    await dataContext.SaveChangesAsync(CancellationToken.None);

                    relationIds = relations["reports"];

                    internalModel.reports = db.Reports.Where(dep => relationIds.Contains(dep.id)).ToList();
                    internalModel.total_reports = internalModel.reports != null ? internalModel.reports.Count() : null;
                }
				//Asign relations
				if (relations.ContainsKey("users"))
				{
                    //clear all current relations

                    model.users?.Clear();

                    await dataContext.SaveChangesAsync(CancellationToken.None);

                    relationIds = relations["users"];

					internalModel.users = db.Users.Where(dep => relationIds.Contains(dep.id)).ToList();
				}

				if (relations.ContainsKey("roles"))
				{
                    //clear all current relations

                    model.roles?.Clear();

                    await dataContext.SaveChangesAsync(CancellationToken.None);

                    relationIds = relations["roles"];

					internalModel.roles = db.Roles.Where(dep => relationIds.Contains(dep.id)).ToList();
				}
				//save the changes to the model
				await db.SaveChangesAsync(CancellationToken.None);

            }



        }



		
	}
}
