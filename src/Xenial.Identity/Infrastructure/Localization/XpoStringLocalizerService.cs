using DevExpress.Xpo;

using Xenial.Identity.Models;

namespace Xenial.Identity.Infrastructure.Localization;

public record XpoStringLocalizerService(UnitOfWork UnitOfWork, XpoStringLocalizer StringLocalizer)
{
    public async Task Refresh()
    {
        var items = await UnitOfWork.Query<XpoLocalization>()
            .Where(m => !string.IsNullOrEmpty(m.Key))
            .Select(m => new KeyValuePair<string, string>(m.Key, m.Value))
            .ToArrayAsync();
        StringLocalizer.UpdateDictionary(items);
    }
}
