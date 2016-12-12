using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElCamino.AspNetCore.Identity.Dynamo;

namespace ElCamino.AspNet.Identity.Dynamo.Tests.ModelTests
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
            await WaitTillTableCreated("TableNotFound",
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

            await WaitTillTableCreated(table,
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
