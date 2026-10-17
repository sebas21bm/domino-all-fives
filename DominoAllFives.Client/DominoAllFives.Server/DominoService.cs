using System.ServiceModel;

namespace DominoAllFives.Server
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerCall,
        IncludeExceptionDetailInFaults = false)]
    public partial class DominoService
    {
    }
}