using EIMS.Shared.Models;

namespace EIMS.Shared.Services;

public interface IOctopartService
{
    Task<List<OctopartSearchResult>> Search(string query);
    Task<List<OctopartSearchResult>> SearchByMpn(string mpn);
} 