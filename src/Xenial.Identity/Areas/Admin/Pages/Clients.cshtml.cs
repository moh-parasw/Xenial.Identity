using AutoMapper;
using AutoMapper.QueryableExtensions;

using DevExpress.Xpo;

using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Areas.Admin.Pages
{
    public class ClientsModel : PageModel
    {
        private readonly UnitOfWork unitOfWork;
        public ClientsModel(UnitOfWork unitOfWork)
            => this.unitOfWork = unitOfWork;

        public IQueryable<ClientsOutputModel> Clients { get; set; } = new ClientsOutputModel[0].AsQueryable();

        public class ClientsOutputModel
        {
            public string Id { get; set; }
            public string ClientId { get; set; }
            public string ClientName { get; set; }
        }

        internal class ClientsMappingConfiguration : Profile
        {
            public ClientsMappingConfiguration()
                => CreateMap<ClientsOutputModel, XpoClient>()
                    .ReverseMap()
                ;
        }

        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<ClientsMappingConfiguration>())
                .CreateMapper();

        public void OnGet()
            => Clients = unitOfWork.Query<XpoClient>().ProjectTo<ClientsOutputModel>(Mapper.ConfigurationProvider);

    }
}
