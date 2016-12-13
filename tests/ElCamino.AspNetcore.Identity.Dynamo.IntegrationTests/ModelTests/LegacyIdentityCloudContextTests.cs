// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.

using Amazon.DynamoDBv2.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;

namespace ElCamino.AspNetCore.Identity.Dynamo.IntegrationTests.ModelTests
{
    [TestClass]
    public class LegacyIdentityCloudContextTests
    {
        /*
        [TestMethod]
        [TestCategory("Identity.Dynamo.Model")]
        public void IdentityCloudContextCtors()
        {
            string strValidConnection = ConfigurationManager.ConnectionStrings[AspNetCore.Identity.Dynamo.Constants.AppSettingsKeys.DefaultStorageConnectionStringKey].ConnectionString;
            var currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var section = currentConfig.Sections[IdentityConfigurationSection.Name];
            if (section == null)
            {
                currentConfig.Sections.Add(IdentityConfigurationSection.Name,
                    new IdentityConfigurationSection()
                    {
                        TablePrefix = string.Empty,
                        ServiceURL = strValidConnection,
                        BufferSize = 5 * 1024,
                        ConnectionLimit = 10,
                        LogMetrics = false,
                        LogResponse = false,
                        AuthenticationRegion = string.Empty
                    });
                currentConfig.Save(ConfigurationSaveMode.Modified);
            }
            var ic = new IdentityCloudContext();
            Assert.IsNotNull(ic, "New IdentityCloudContext is null");

            //Pass in valid connection string
            var icc = new IdentityCloudContext(strValidConnection);
            icc = new IdentityCloudContext(ElCamino.AspNet.Identity.Dynamo.Constants.AppSettingsKeys.DefaultStorageConnectionStringKey);
            icc.Dispose();

            System.Threading.Thread.Sleep(3000);

            ic = new IdentityCloudContext(new IdentityConfiguration()
            {
                TablePrefix = string.Empty,
                ServiceURL = strValidConnection
            });
            ic.Dispose();

            ic = new IdentityCloudContext(new AmazonDynamoDBConfig()
            {
                ServiceURL = strValidConnection
            });
            ic.Dispose();

            var aic = new IdentityCloudContext<IdentityUser>(new AmazonDynamoDBConfig()
                {
                    ServiceURL = strValidConnection,
                });
            aic.Dispose();

            System.Threading.Thread.Sleep(3000);

            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>(
                new IdentityCloudContext<IdentityUser>(new IdentityConfiguration()
                {
                    ServiceURL = strValidConnection,
                    TablePrefix = "My"
                })))
            {
                var task = store.CreateTablesIfNotExists();
                task.Wait();
            }

            System.Threading.Thread.Sleep(3000);

            currentConfig.Sections.Remove(IdentityConfigurationSection.Name);
            currentConfig.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(IdentityConfigurationSection.Name);

            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>(
                new IdentityCloudContext<IdentityUser>()))
            {
                var task = store.CreateTablesIfNotExists();
                task.Wait();
            }

            System.Threading.Thread.Sleep(3000);

            currentConfig.Sections.Add(IdentityConfigurationSection.Name,
                new IdentityConfigurationSection()
                {
                    TablePrefix = "Test",
                    ServiceURL = strValidConnection,
                    BufferSize = 5 * 1024,
                    ConnectionLimit = 10,
                    LogMetrics = false,
                    LogResponse = false,
                    AuthenticationRegion = string.Empty
                });
            currentConfig.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(IdentityConfigurationSection.Name);


            string strInvalidConnectionStringKey = Guid.NewGuid().ToString();

            try
            {
                ic = new IdentityCloudContext(strInvalidConnectionStringKey);
            }
            catch (System.UriFormatException) { }

            try
            {
                ic = new IdentityCloudContext(string.Empty);
            }
            catch (AmazonClientException) { }

            try
            {
                IdentityConfiguration nic = null;
                ic = new IdentityCloudContext(nic);
            }
            catch (NullReferenceException) { }

            //----------------------------------------------
            var iucc = new IdentityCloudContext<IdentityUser>();
            Assert.IsNotNull(iucc, "New IdentityCloudContext is null");

            try
            {
                iucc = new IdentityCloudContext<IdentityUser>(strInvalidConnectionStringKey);
            }
            catch (System.UriFormatException) { }

            try
            {
                iucc = new IdentityCloudContext<IdentityUser>(string.Empty);
            }
            catch (AmazonClientException) { }
            
            //------------------------------------------

            var i2 = new IdentityCloudContext<IdentityUser, IdentityRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>();
            Assert.IsNotNull(i2, "New IdentityCloudContext is null");

            i2 = new IdentityCloudContext<IdentityUser, IdentityRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>();

            try
            {
                i2 = new IdentityCloudContext<IdentityUser, IdentityRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>(strValidConnection);
            }
            catch (System.UriFormatException) { }

            try
            {
                i2 = new IdentityCloudContext<IdentityUser, IdentityRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>(string.Empty);
            }
            catch (AmazonClientException) { }
        }*/

        [TestMethod]
        [TestCategory("Identity.Dynamo.Model")]
        public async Task CreateUserTableTest()
        {
            using (var ic = new IdentityCloudContext())
            {
                string table = "UserTable123";
                await ic.CreateTableAsync(ic.GenerateUserCreateTableRequest(
                    table, "UserEmailIndex123", "UserNameIndex123"));

                Thread.Sleep(500);
                await ic.DeleteTableAsync(table);
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.Model")]
        public void CreateRoleTableTest()
        {
            using (var ic = new IdentityCloudContext())
            {
                string table = "RoleTable123";
                var createTask = ic.CreateTableAsync(ic.GenerateRoleCreateTableRequest(
                    table));
                createTask.Wait();

                Thread.Sleep(500);

                var deleteTask = ic.DeleteTableAsync(table);
                deleteTask.Wait();
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.Model")]
        public void CreateIndexTableTest()
        {
            using (var ic = new IdentityCloudContext())
            {
                string table = "IndexTable123";
                var createTask = ic.CreateTableAsync(ic.GenerateIndexCreateTableRequest(
                    table));
                createTask.Wait();

                Thread.Sleep(500);

                var deleteTask = ic.DeleteTableAsync(table);
                deleteTask.Wait();
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.Model")]
        public void IdentityCloudContextDisposeTest()
        {
            using (var ic = new IdentityCloudContextFake())
            {
                ic.DisposeTestHelper();
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.Model")]
        public async Task IdentityCloudContextWaitTillTableCreatedTest()
        {
            using (var ic = new IdentityCloudContextFake())
            {
                try
                {
                    await ic.WaitTillTableCreatedTestHelper();
                }
                catch (ResourceNotFoundException) { }

                Thread.Sleep(5000);

                ic.WaitTillTableCreatedTestHelper2();
            }
        }
    }
}
