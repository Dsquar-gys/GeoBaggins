using System.Threading.Tasks;
using Refit;

namespace GeoBaggins.Models;

public interface IGeoBagginsApi
{
    [Post("/api/location")]
    Task<string> CheckLocation([Body]LocationDto location);
}