using AutoMapper;

using DevExpress.Xpo;

using Duende.IdentityServer.Models;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Xpo.Storage.Mappers
{
    /// <summary>
    /// Extension methods to map to/from entity/model for clients.
    /// </summary>
    public static class ClientMappers
    {
        internal static IMapper Mapper { get; }
            = new MapperConfiguration(cfg => cfg.AddProfile<ClientMapperProfile>())
                .CreateMapper();

        /// <summary>
        /// Maps an entity to a model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static Client ToModel(this XpoClient entity)
            => Mapper.Map<Client>(entity);

        /// <summary>
        /// Maps a model to an entity.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public static XpoClient ToEntity(this Client model, Session session)
            => Mapper.Map<XpoClient>(model, opts => opts.ConstructServicesUsing(t => session.GetClassInfo(t).CreateObject(session)));
    }
}
