// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.
using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
#if net45
using ElCamino.AspNet.Identity.Dynamo.Model;
#else
using ElCamino.AspNetCore.Identity.Dynamo.Model;
#endif
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using ElCamino.AspNet.Identity.Dynamo.Configuration;

namespace ElCamino.AspNetCore.Identity.Dynamo
{
    public class IdentityCloudContext : IdentityCloudContext<IdentityUser, IdentityRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>
    {
        public IdentityCloudContext() : base(new IdentityDynamoOptions())
        { }

        public IdentityCloudContext(IdentityDynamoOptions config) :
            base(config) { }

    }

    public class IdentityCloudContext<TUser> : IdentityCloudContext<TUser, IdentityRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim> where TUser : IdentityUser
    {
        public IdentityCloudContext() : base(new IdentityDynamoOptions())
        { }

        public IdentityCloudContext(IdentityDynamoOptions config) 
           : base(config) { }
    }

    public class IdentityCloudContext<TUser, TRole, TKey, TUserLogin, TUserRole, TUserClaim>  : DynamoDBContext
        where TUser : IdentityUser<TKey, TUserLogin, TUserRole, TUserClaim>
        where TRole : IdentityRole<TKey, TUserRole>
        where TUserLogin : IdentityUserLogin<TKey>
        where TUserRole : IdentityUserRole<TKey>
        where TUserClaim : IdentityUserClaim<TKey>
    {
        private AmazonDynamoDBClient _client;
        protected bool _disposed = false;
        private IdentityDynamoOptions _config = null;


        public AmazonDynamoDBClient Client
        {
            get { return _client; }
        }

        public string TablePrefix
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_config.TablePrefix))
                {
                    return _config.TablePrefix;
                }
                return string.Empty;
            }
        }


        public IdentityCloudContext(IdentityDynamoOptions config) : base(new AmazonDynamoDBClient())
        {
            this._config = config;
            Initialize(config);
        }

        public string FormatTableNameWithPrefix(string tableName)
        {
            return TablePrefix + tableName;
        }

        private void Initialize(IdentityDynamoOptions config)
        {
            _config = config;
            var dbConfig = new AmazonDynamoDBConfig();
            dbConfig.AuthenticationRegion = config.AuthenticationRegion;
            dbConfig.LogMetrics = config.LogMetrics ?? false;
            dbConfig.LogResponse = config.LogResponse ?? false;
            dbConfig.BufferSize = config.BufferSize;
            dbConfig.ServiceURL = config.ServiceUrl;
            _client = new AmazonDynamoDBClient(dbConfig);
        }

        public CreateTableRequest GenerateUserCreateTableRequest(string tableName, 
            string userEmailIndex,
            string userNameIndex)
        {
            return new CreateTableRequest
                {
                            TableName = tableName,
                            AttributeDefinitions = new List<AttributeDefinition>()
                    {
                      new AttributeDefinition
                      {
                        AttributeName = "Id",
                        AttributeType = "S"
                      },
                      new AttributeDefinition
                      {
                        AttributeName = "UserId",
                        AttributeType = "S"
                      },
                      new AttributeDefinition
                      {
                        AttributeName = "UserName",
                        AttributeType = "S"
                      },
                      new AttributeDefinition
                      {
                        AttributeName = "Email",
                        AttributeType = "S"
                      }
                    },
                            KeySchema = new List<KeySchemaElement>()
                    {
                      new KeySchemaElement
                      {
                        AttributeName = "UserId",
                        KeyType = KeyType.HASH
                      },
                      new KeySchemaElement
                      {
                        AttributeName = "Id",
                        KeyType = KeyType.RANGE
                      },
                    },
                            ProvisionedThroughput = new ProvisionedThroughput
                            {
                                ReadCapacityUnits = 1,
                                WriteCapacityUnits = 1
                            },
                            GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
                    {
                        new GlobalSecondaryIndex
                        {
                            IndexName = userEmailIndex,
                            KeySchema = new List<KeySchemaElement>()
                            {
                                new KeySchemaElement
                                {
                                    AttributeName = "Email",
                                    KeyType = KeyType.HASH
                                },
                                new KeySchemaElement
                                {
                                    AttributeName = "Id",
                                    KeyType = KeyType.RANGE
                                },

                            },
                            Projection = new Projection
                            {
                                 ProjectionType = new ProjectionType("ALL")
                            },
                            ProvisionedThroughput = new ProvisionedThroughput
                            {
                                ReadCapacityUnits = 1,
                                WriteCapacityUnits = 1
                            },
                        },
                        new GlobalSecondaryIndex
                        {
                            IndexName = userNameIndex,
                            KeySchema = new List<KeySchemaElement>()
                            {
                                new KeySchemaElement
                                {
                                    AttributeName = "UserName",
                                    KeyType = KeyType.HASH
                                },
                                new KeySchemaElement
                                {
                                    AttributeName = "Id",
                                    KeyType = KeyType.RANGE
                                },
                            },
                            Projection = new Projection
                            {
                                 ProjectionType = new ProjectionType("ALL")
                            },
                            ProvisionedThroughput = new ProvisionedThroughput
                            {
                                ReadCapacityUnits = 1,
                                WriteCapacityUnits = 1
                            },
                        }
                    },
                        };
        }

        public async Task CreateUserTableAsync()
        {
            await CreateTableAsync(GenerateUserCreateTableRequest(FormatTableNameWithPrefix(Constants.TableNames.UsersTable),
                Constants.SecondaryIndexNames.UserEmailIndex,
                Constants.SecondaryIndexNames.UserNameIndex));
        }

        public async Task<DeleteTableResponse> DeleteTableAsync(string tableName)
        {
            return await _client.DeleteTableAsync(new DeleteTableRequest() { TableName = tableName });
        }

        public CreateTableRequest GenerateRoleCreateTableRequest(string tableName)
        {
            return new CreateTableRequest
                    {
                        TableName = tableName,
                        AttributeDefinitions = new List<AttributeDefinition>()
                        {
                          new AttributeDefinition
                          {
                            AttributeName = "Id",
                            AttributeType = "S"
                          },
                        },
                        KeySchema = new List<KeySchemaElement>()
                        {
                          new KeySchemaElement
                          {
                            AttributeName = "Id",
                            KeyType = KeyType.HASH
                          },
                        },
                        ProvisionedThroughput = new ProvisionedThroughput
                        {
                            ReadCapacityUnits = 1,
                            WriteCapacityUnits = 1
                        },

                    };
        }
        
        public async Task CreateRoleTableAsync()
        {
            await CreateTableAsync(GenerateRoleCreateTableRequest(FormatTableNameWithPrefix(Constants.TableNames.RolesTable)));
        }

        public CreateTableRequest GenerateIndexCreateTableRequest(string tableName)
        {
            return new CreateTableRequest
                    {
                        TableName = tableName,
                        AttributeDefinitions = new List<AttributeDefinition>()
                        {
                          new AttributeDefinition
                          {
                            AttributeName = "Id",
                            AttributeType = "S"
                          },
                        },
                        KeySchema = new List<KeySchemaElement>()
                        {
                          new KeySchemaElement
                          {
                            AttributeName = "Id",
                            KeyType = KeyType.HASH
                          },
                        },
                        ProvisionedThroughput = new ProvisionedThroughput
                        {
                            ReadCapacityUnits = 1,
                            WriteCapacityUnits = 1
                        },

                    };
        }

        public async Task CreateIndexTableAsync()
        {
            await CreateTableAsync(GenerateIndexCreateTableRequest(FormatTableNameWithPrefix(Constants.TableNames.IndexTable)));
        }

        public async Task CreateTableAsync(CreateTableRequest request)
        {
            await new TaskFactory().StartNew(async () =>
            {
                var tableResponse = await _client.ListTablesAsync();
                var tableExists = tableResponse.TableNames.Any(t => t == request.TableName);
                if (!tableExists)
                {
                    var response = await _client.CreateTableAsync(request);
                    WaitTillTableCreated(request.TableName, response);
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!_disposed && disposing)
            {
                _client.Dispose();
                _disposed = true;
            }
        }


        protected async Task WaitTillTableCreated(string tableName, CreateTableResponse response)
        {
            var tableDescription = response.TableDescription;

            string status = tableDescription.TableStatus;

            Console.WriteLine(tableName + " - " + status);

            // Let us wait until table is created. Call DescribeTable.
            while (status != "ACTIVE")
            {
                System.Threading.Thread.Sleep(5000); // Wait 5 seconds.
                try
                {
                    var res = await _client.DescribeTableAsync(new DescribeTableRequest
                    {
                        TableName = tableName
                    });
                    Console.WriteLine("Table name: {0}, status: {1}", res.Table.TableName,
                                                                      res.Table.TableStatus);
                    status = res.Table.TableStatus;
                }
                // Try-catch to handle potential eventual-consistency issue.
                catch (ResourceNotFoundException) { throw; }
            }
        }

    }

}
