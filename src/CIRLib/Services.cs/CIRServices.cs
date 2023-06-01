using ObjModels = CIRLib.ObjectModel.Models;
using System.Security.Claims;
using CIRLib.Persistence;
using CIRLib.ObjectModel;
using Factory = CIRLib.Persistence.CIRLibContextFactory;

namespace CIRServices;
public class CIRServices
{
    CIRLibContext Context;

    public CIRServices()
    {
        Context = new CIRLibContextFactory().CreateDbContext(new ClaimsPrincipal()).Result;
    }

    public List<ObjModels.Entry> GetEquivalentEntriesResponse(string RegistryId, string CategoryId, string CategorySourceId,
        string EntryIdInSource, string EntrySourceId)
    {
        //This func will be invoked after retrieving the params from the xml i guess.
        return new List<ObjModels.Entry>();
    }

}