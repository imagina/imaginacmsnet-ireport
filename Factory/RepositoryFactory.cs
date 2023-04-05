using Core.Entities;
using Core.Interfaces;
using Core.Repositories;
using Idata.Data;
using Idata.Data.Entities.Ireport;
using Idata.Data.Entities.Ramp;
using Idata.Entities.Core;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Ramp.Data;
using System;

namespace Ireport.Factory
{
    public static class RepositoryFactory
    {

        public static dynamic GetRepository(string namespaceName)
        {
            switch (namespaceName)
            {
                case "Idata.Data.Entities.WorkOrder":
                    IdataContext idataContext = new IdataContext();
                    IRepositoryBase<WorkOrder> repository = new RepositoryBase<WorkOrder>();
                    repository.Initialize(idataContext);
                    return repository;

                default: return null;
            }

        }

        public static IRepositoryBase<EntityBase> GetRepositoryCase(string namespaceName)
        {
            switch (namespaceName)
            {
                case "Idata.Data.Entities.WorkOrder":
                    IdataContext idataContext = new IdataContext();
                    IRepositoryBase<WorkOrder> repository = new RepositoryBase<WorkOrder>();
                    repository.Initialize(idataContext);
                    return (IRepositoryBase<EntityBase>)repository;

                default: return null;
            }

        }
    }
}
