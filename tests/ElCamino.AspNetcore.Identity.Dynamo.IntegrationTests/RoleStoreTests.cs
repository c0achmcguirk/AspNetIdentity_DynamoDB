using Amazon.DynamoDBv2;
using ElCamino.AspNet.Identity.Dynamo.Configuration;
using ElCamino.AspNetCore.Identity.Dynamo.Model;
using NUnit.Framework;

namespace ElCamino.AspNetCore.Identity.Dynamo.IntegrationTests
{
    [TestFixture]
    public class RoleStoreTests
    {
        private IdentityCloudContext _icContext;
        private IdentityDynamoOptions _iOptions;
        private AmazonDynamoDBClient _dbClient;

        [Test]
        public void Ctor_Always_Creates()
        {
            var sut = MakeSut();
            Assert.IsNotNull(sut);
            Assert.IsInstanceOf<RoleStore<IdentityRole>>(sut);
        }



        private RoleStore<IdentityRole> MakeSut()
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

            if (_dbClient == null)
            {
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
            }

            if (_icContext == null)
            {
                _icContext = new IdentityCloudContext(_iOptions, _dbClient);
            }

            return new RoleStore<IdentityRole>(_icContext);
        }
    }
}
