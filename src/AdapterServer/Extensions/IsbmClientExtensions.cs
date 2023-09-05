using System.Net.Security;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using Isbm2Client.Service;
using Isbm2RestClient.Client;
using RestApi = Isbm2RestClient.Api;

namespace AdapterServer.Extensions;

public static class IsbmClientExtensions
{
    public static void AddIsbmRestClient(this IServiceCollection services, IConfigurationSection isbmSection,
        RemoteCertificateValidationCallback? sslValdiationCallback = null)
    {
        var config = isbmSection.Get<ClientConfig>();
        var restApiConfig = new Configuration()
        {
            BasePath = config.EndPoint,
            ServerCertificateValidationCallback = sslValdiationCallback
        };

        services.AddScoped<RestApi.IChannelManagementApi>(x => new RestApi.ChannelManagementApi(restApiConfig));
        services.AddScoped<RestApi.IConsumerRequestServiceApi>(x => new RestApi.ConsumerRequestServiceApi(restApiConfig));
        services.AddScoped<RestApi.IProviderRequestServiceApi>(x => new RestApi.ProviderRequestServiceApi(restApiConfig));
        services.AddScoped<RestApi.IConsumerPublicationServiceApi>(x => new RestApi.ConsumerPublicationServiceApi(restApiConfig));
        services.AddScoped<RestApi.IProviderPublicationServiceApi>(x => new RestApi.ProviderPublicationServiceApi(restApiConfig));

        services.AddScoped<IChannelManagement, RestChannelManagement>();
        services.AddScoped<IProviderRequest, RestProviderRequest>();
        services.AddScoped<IConsumerRequest, RestConsumerRequest>();
        services.AddScoped<IProviderPublication, RestProviderPublication>();
        services.AddScoped<IConsumerPublication, RestConsumerPublication>();
    }
}
