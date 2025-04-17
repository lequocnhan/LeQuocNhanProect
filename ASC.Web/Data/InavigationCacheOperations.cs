using ASC.Web.Navigation;

namespace ASC.Web.Data
{
    public interface InavigationCacheOperations
    {
        Task<NavigationMenu> GetNavigationCacheAsync();
        Task CreateNavigationCacheAsync();
    }
}
