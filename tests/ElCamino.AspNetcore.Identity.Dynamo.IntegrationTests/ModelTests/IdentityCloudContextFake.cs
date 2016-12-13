using System;
using System.Threading.Tasks;

namespace ElCamino.AspNetCore.Identity.Dynamo.IntegrationTests.ModelTests
{
    public class IdentityCloudContextFake : IdentityCloudContext
    {
        public void DisposeTestHelper()
        {
            _disposed = true;
            Dispose(false);          
        }

        public async Task WaitTillTableCreatedTestHelper()
        {
            await WaitUntilTableCreatedAsync("TableNotFound",
                new Amazon.DynamoDBv2.Model.CreateTableResponse()
                {
                    TableDescription = new Amazon.DynamoDBv2.Model.TableDescription()
                    {
                         TableName = "TableNotFound",
                         TableStatus = new Amazon.DynamoDBv2.TableStatus("NotActive")
                    }
                });
        }

        public async Task WaitTillTableCreatedTestHelper2()
        {
            string table = "RoleTable" + Guid.NewGuid().ToString("N") ;
            var createTask = CreateTableAsync(GenerateRoleCreateTableRequest(
                table));
            createTask.Wait();

            await WaitUntilTableCreatedAsync(table,
                new Amazon.DynamoDBv2.Model.CreateTableResponse()
                {
                    TableDescription = new Amazon.DynamoDBv2.Model.TableDescription()
                    {
                        TableName = table,
                         TableStatus = new Amazon.DynamoDBv2.TableStatus("NotActive")
                    }
                });

            var deleteTask = DeleteTableAsync(table);
            deleteTask.Wait();

        }
    }
}
