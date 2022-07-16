using AutoMapper;

using DevExpress.Xpo;

using Duende.IdentityServer.Models;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Xpo.Storage.Mappers
{
    /// <summary>
    /// Extension methods to map to/from entity/model for scopes.
    /// </summary>
    public static class ScopeMappers
    {
        internal static IMapper Mapper { get; } = new MapperConfiguration(cfg => cfg.AddProfile<ScopeMapperProfile>())
                .CreateMapper();

        /// <summary>
        /// Maps an entity to a model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static ApiScope ToModel(this XpoApiScope entity)
            => entity == null ? null : Mapper.Map<ApiScope>(entity);

        /// <summary>
        /// Maps a model to an entity.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public static XpoApiScope ToEntity(this ApiScope model, Session session)
            => model == null ? null : Mapper.Map<XpoApiScope>(model, opt => opt.ConstructServicesUsing(t => session.GetClassInfo(t).CreateObject(session)));
    }
}
