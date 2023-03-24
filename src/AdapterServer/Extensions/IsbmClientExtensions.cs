using Isbm2Client.Interface;
using Isbm2Client.Model;
using Isbm2Client.Service;
using RestApi = Isbm2RestClient.Api;

namespace AdapterServer.Extensions;

public static class IsbmClientExtensions
{
    public static void AddIsbmRestClient( this IServiceCollection services, IConfigurationSection isbmSection )
    {
        var config = isbmSection.Get<ClientConfig>();

        services.AddScoped<RestApi.IChannelManagementApi>(x => new RestApi.ChannelManagementApi(config.EndPoint));
        services.AddScoped<RestApi.IConsumerRequestServiceApi>(x => new RestApi.ConsumerRequestServiceApi(config.EndPoint));
        services.AddScoped<RestApi.IProviderRequestServiceApi>(x => new RestApi.ProviderRequestServiceApi(config.EndPoint));
        services.AddScoped<RestApi.IConsumerPublicationServiceApi>(x => new RestApi.ConsumerPublicationServiceApi(config.EndPoint));
        services.AddScoped<RestApi.IProviderPublicationServiceApi>(x => new RestApi.ProviderPublicationServiceApi(config.EndPoint));

        services.AddScoped<IChannelManagement, RestChannelManagement>();
        services.AddScoped<IProviderRequest, RestProviderRequest>();
        services.AddScoped<IConsumerRequest, RestConsumerRequest>();
        services.AddScoped<IProviderPublication, RestProviderPublication>();
        services.AddScoped<IConsumerPublication, RestConsumerPublication>();
    }
}
