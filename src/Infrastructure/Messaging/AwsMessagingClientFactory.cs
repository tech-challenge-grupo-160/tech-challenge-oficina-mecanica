using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.Credentials;
using Amazon.SQS;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Messaging;

internal static class AwsMessagingClientFactory
{
    public static IAmazonSQS CreateSqs(MessagingOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ServiceUrl))
        {
            var config = new AmazonSQSConfig
            {
                ServiceURL = options.ServiceUrl,
                AuthenticationRegion = options.Region
            };
            var credentials = DefaultAWSCredentialsIdentityResolver.GetCredentials(config);
            return new AmazonSQSClient(credentials, config);
        }

        return new AmazonSQSClient(RegionEndpoint.GetBySystemName(options.Region));
    }
}
