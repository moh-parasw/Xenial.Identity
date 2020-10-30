using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;
using AutoMapper.QueryableExtensions;

using DevExpress.Xpo;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages
{
    public class PersistedGrantsModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        public PersistedGrantsModel(UnitOfWork unitOfWork)
            => this.unitOfWork = unitOfWork;

        public IQueryable<PersistentGrantOutputModel> PersistedGrants { get; set; } = new PersistentGrantOutputModel[0].AsQueryable();

        public class PersistentGrantOutputModel
        {
            public string Key { get; set; }
            public string Type { get; set; }
            public string SubjectId { get; set; }
            public string SessionId { get; set; }
            public string ClientId { get; set; }
            public string Description { get; set; }
            public DateTime CreationTime { get; set; }
            public DateTime? Expiration { get; set; }
            public DateTime? ConsumedTime { get; set; }
            public string Data { get; set; }
        }

        internal class PersistedGrantsMappingConfiguration : Profile
        {
            public PersistedGrantsMappingConfiguration()
                => CreateMap<PersistentGrantOutputModel, XpoPersistedGrant>()
                    .ReverseMap()
                ;
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<PersistedGrantsMappingConfiguration>())
                .CreateMapper();

        public void OnGet()
            => PersistedGrants = unitOfWork.Query<XpoPersistedGrant>().ProjectTo<PersistentGrantOutputModel>(Mapper.ConfigurationProvider);

    }
}
