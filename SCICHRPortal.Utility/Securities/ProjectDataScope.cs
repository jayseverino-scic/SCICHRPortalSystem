using SCICHRPortal.Data.DTOs;

namespace SCICHRPortal.Utility.Securities;

public sealed class ProjectDataScope
{
    // Background jobs have no browser identity. HTTP middleware replaces this
    // before an endpoint executes, using the service's current database decision.
    public ProjectDataAccess Access { get; set; } = new(false);
}
