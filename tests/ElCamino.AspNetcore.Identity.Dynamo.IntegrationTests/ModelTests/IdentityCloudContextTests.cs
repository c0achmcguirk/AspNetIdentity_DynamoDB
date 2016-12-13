using Amazon.DynamoDBv2;
using ElCamino.AspNet.Identity.Dynamo.Configuration;
using ElCamino.AspNetCore.Identity.Dynamo.Model;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ElCamino.AspNetCore.Identity.Dynamo.IntegrationTests.ModelTests
{
    /// <summary>
    /// This is an integration test. You need to run DynamoDB locally on Port 8000.
    /// https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html
    /// </summary>
    [TestFixture]
    public class IdentityCloudContextTests
    {
        private IdentityDynamoOptions _iOptions;
        private AmazonDynamoDBClient _dbClient;

        [Test]
        public void Ctor_Always_Creates()
        {
            using (var sut = MakeSut())
            {
                Assert.IsInstanceOf<IIdentityCloudContext>(sut);
            }
        }

        [Test]
        public void CreateUserTableAsync_Always_Creates()
        {
            using (var sut = MakeSut())
            {
                Task.Run(() => sut.CreateRoleTableAsync());
            }
        }

        private IdentityCloudContext<IdentityUser, IdentityRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim> MakeSut()
        {
            if (_iOptions == null)
            {
                _iOptions = new IdentityDynamoOptions()
                {
                    ServiceUrl = "http://localhost:8000",
                    AuthenticationRegion = "",
                    TablePrefix = "l-"
                };
            }

            var dbConfig = new AmazonDynamoDBConfig()
            {
                ServiceURL = _iOptions.ServiceUrl,
                AuthenticationRegion = _iOptions.AuthenticationRegion,
                BufferSize = _iOptions.BufferSize,
                ConnectionLimit = _iOptions.ConnectionLimit,
                LogMetrics = _iOptions.LogMetrics ?? false,
                LogResponse = _iOptions.LogResponse ?? false
            };
            _dbClient = new AmazonDynamoDBClient(dbConfig);

            return new IdentityCloudContext(_iOptions, _dbClient);
        }
    }
}
