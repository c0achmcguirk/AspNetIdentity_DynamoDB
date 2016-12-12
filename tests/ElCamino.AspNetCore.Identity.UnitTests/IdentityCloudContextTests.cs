using System.Linq;
using Amazon.DynamoDBv2;
using ElCamino.AspNet.Identity.Dynamo.Configuration;
using ElCamino.AspNetCore.Identity.Dynamo;
using FakeItEasy;
using NUnit.Framework;

namespace ElCamino.AspNetCore.Identity.UnitTests
{
    [TestFixture]
    public class IdentityCloudContextTests
    {
        protected IAmazonDynamoDB _fakeAmazonDynamoDbClient = A.Fake<IAmazonDynamoDB>();

        [Test]
        public void Ctor_Always_Creates()
        {
            var sut = (TestableIdentityCloudContext)MakeSut();
            Assert.IsNotNull(sut);
            Assert.IsTrue(sut.SetUpClientCalled);
        }

        [Test]
        public void FormatTableNameWithPrefix_TableNameAndPrefixDefined_ReturnsBothAppended()
        {
            var options = new IdentityDynamoOptions {TablePrefix = "SomePrefix"};
            var sut = (TestableIdentityCloudContext) MakeSut(options);
            var tableName = sut.FormatTableNameWithPrefix("SomeTable");
            Assert.AreEqual("SomePrefixSomeTable", tableName);
        }

        [Test]
        public void GenerateUserCreateTableRequest_HasUserEmailIndex_UserEmailIndexIsSetUp()
        {
            var sut = MakeSut();
            var ctRequest = sut.GenerateUserCreateTableRequest("SomeTable", "UserEmailIndex", "UserNameIndex");
            var emailIndexAttribute = (from i in ctRequest.GlobalSecondaryIndexes where i.IndexName == "UserEmailIndex" select i)
                    .SingleOrDefault();
            Assert.IsNotNull(emailIndexAttribute);
            Assert.AreEqual("UserEmailIndex", emailIndexAttribute.IndexName);
        }

        [Test]
        public void GenerateRoleCreateTableRequest_TableNameIsPassedIn_RequestHasTableNameDefined()
        {
            var sut = MakeSut();
            var ctRequest = sut.GenerateRoleCreateTableRequest("SomeTable");
            Assert.AreEqual("SomeTable", ctRequest.TableName);
        }

        [Test]
        public void GenerateIndexCreateTableRequest_TableNamePassedIn_TableNameInRequest()
        {
            var sut = MakeSut();
            var ctRequest = sut.GenerateIndexCreateTableRequest("SomeTable");
            Assert.AreEqual("SomeTable", ctRequest.TableName);
        }

        private IIdentityCloudContext MakeSut(IdentityDynamoOptions options = null)
        {
            if(options == null)
            {
                options = new IdentityDynamoOptions(); 
            }
            return new TestableIdentityCloudContext(options, _fakeAmazonDynamoDbClient);
        }
    }

    internal class TestableIdentityCloudContext : IdentityCloudContext
    {
        public IdentityDynamoOptions Options { get; private set; }
        public IAmazonDynamoDB Client { get; private set; }

        public bool SetUpClientCalled = false;

        public TestableIdentityCloudContext(IdentityDynamoOptions options, IAmazonDynamoDB dbClient) : base(options, dbClient)
        {
            Client = dbClient;
            Options = options;
        }

        protected override void SetUpClient(AmazonDynamoDBConfig dbConfig)
        {
            SetUpClientCalled = true;
        }
    }
}