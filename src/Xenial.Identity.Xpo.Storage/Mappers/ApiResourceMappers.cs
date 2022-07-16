using AutoMapper;

using DevExpress.Xpo;

using Duende.IdentityServer.Models;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Xpo.Storage.Mappers
{
    /// <summary>
    /// Extension methods to map to/from entity/model for API resources.
    /// </summary>
    public static class ApiResourceMappers
    {
        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<ApiResourceMapperProfile>())
                .CreateMapper();

        /// <summary>
        /// Maps an entity to a model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static ApiResource ToModel(this XpoApiResource entity)
            => entity == null ? null : Mapper.Map<ApiResource>(entity);

        /// <summary>
        /// Maps a model to an entity.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public static XpoApiResource ToEntity(this ApiResource model, Session session)
            => model == null ? null : Mapper.Map<XpoApiResource>(model, opt => opt.ConstructServicesUsing(t => session.GetClassInfo(t).CreateObject(session)));
    }
}
