namespace DominoAllFivesService
{
    public partial class AuthenticationServiceClient
    {
        public AuthenticationServiceClient(string endpointConfigurationName)
            : base(endpointConfigurationName) { }
    }

    public partial class RankingRemoteServiceClient
    {
        public RankingRemoteServiceClient(string endpointConfigurationName)
            : base(endpointConfigurationName) { }
    }
}