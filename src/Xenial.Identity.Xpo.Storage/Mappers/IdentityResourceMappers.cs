using AutoMapper;

using DevExpress.Xpo;

using IdentityServer4.Models;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Xpo.Storage.Mappers
{
    /// <summary>
    /// Extension methods to map to/from entity/model for identity resources.
    /// </summary>
    public static class IdentityResourceMappers
    {
        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<IdentityResourceMapperProfile>())
                .CreateMapper();

        /// <summary>
        /// Maps an entity to a model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static IdentityResource ToModel(this XpoIdentityResource entity)
            => entity == null ? null : Mapper.Map<IdentityResource>(entity);

        /// <summary>
        /// Maps a model to an entity.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public static XpoIdentityResource ToEntity(this IdentityResource model, Session session)
            => model == null ? null : Mapper.Map<XpoIdentityResource>(model, opts => opts.ConstructServicesUsing(t => session.GetClassInfo(t).CreateObject(session)));
    }
}
