using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.S3;
using System.Configuration;

namespace _301428777_GuertaLigo__Lab_2
{
    internal class AWSHelper
    {
        private static AmazonDynamoDBClient _dynamoClient;
        private static IAmazonS3 _s3Client;

        public static AmazonDynamoDBClient GetDynamoDBClient()
        {
            if (_dynamoClient != null)
                return _dynamoClient;

            string accessKey = ConfigurationManager.AppSettings["AWSAccessKey"];
            string secretKey = ConfigurationManager.AppSettings["AWSSecretKey"];
            var credentials = new BasicAWSCredentials(accessKey, secretKey);

            _dynamoClient = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
            return _dynamoClient;
        }

        public static IAmazonS3 GetS3Client()
        {
            if (_s3Client != null)
                return _s3Client;

            string accessKey = ConfigurationManager.AppSettings["AWSAccessKey"];
            string secretKey = ConfigurationManager.AppSettings["AWSSecretKey"];
            var credentials = new BasicAWSCredentials(accessKey, secretKey);

            _s3Client = new AmazonS3Client(credentials, RegionEndpoint.USEast1);
            return _s3Client;
        }
    }
}
