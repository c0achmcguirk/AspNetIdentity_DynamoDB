// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.
using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElCamino.AspNet.Identity.Dynamo.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using ElCamino.AspNet.Identity.Dynamo.Configuration;

namespace ElCamino.AspNet.Identity.Dynamo
{
    public class IdentityCloudContext : IdentityCloudContext<IdentityUser, IdentityRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>
    {
        public IdentityCloudContext() : base() { }

        [System.Obsolete("Please use the default constructor IdentityCloudContext() to load the configSection from web/app.config or " +
            "the constructor IdentityCloudContext(IdentityConfiguration config) for more options.")]
        public IdentityCloudContext(string connectionStringKey)
            : base(connectionStringKey) { }

        public IdentityCloudContext(IdentityConfiguration config) :
            base(config) { }

        public IdentityCloudContext(AmazonDynamoDBConfig config)
            : base(config) { }
    }

    public class IdentityCloudContext<TUser> : IdentityCloudContext<TUser, IdentityRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim> where TUser : IdentityUser
    {
        public IdentityCloudContext() : base()
        {
        }

        [System.Obsolete("Please use the default constructor IdentityCloudContext() to load the configSection from web/app.config or " +
            "the constructor IdentityCloudContext(IdentityConfiguration config) for more options.")]
        public IdentityCloudContext(string connectionStringKey)
            : base(connectionStringKey)
        {
        }

        public IdentityCloudContext(AmazonDynamoDBConfig config) 
        : base(config) { }

        public IdentityCloudContext(IdentityConfiguration config) 
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
        private IdentityConfiguration _config = null;


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


        public IdentityCloudContext()
            : this(IdentityConfigurationSection.GetCurrent() ??
              new IdentityConfiguration()
            {
                ServiceURL = System.Configuration.ConfigurationManager.ConnectionStrings[Constants.AppSettingsKeys.DefaultStorageConnectionStringKey].ConnectionString,
                TablePrefix = string.Empty
            }) { }

        [System.Obsolete("Please use the default constructor IdentityCloudContext() to load the configSection from web/app.config or " +
            "the constructor IdentityCloudContext(IdentityConfiguration config) for more options.")]
        public IdentityCloudContext(string connectionStringKey)
            : this(new IdentityConfiguration()
            {
                ServiceURL = System.Configuration.ConfigurationManager.ConnectionStrings[connectionStringKey] == null ?
                        connectionStringKey : System.Configuration.ConfigurationManager.ConnectionStrings[connectionStringKey].ConnectionString
            })
        {
        }

        public IdentityCloudContext(AmazonDynamoDBConfig config)
            : this(new IdentityConfiguration(config)) { }

        public IdentityCloudContext(IdentityConfiguration config)
            : base(new AmazonDynamoDBClient(config))
        {
            Initialize(config);
        }

        public string FormatTableNameWithPrefix(string tableName)
        {
            return TablePrefix + tableName;
        }

        private void Initialize(IdentityConfiguration config)
        {
            _config = config;
            _client = new AmazonDynamoDBClient(config);
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
            await new TaskFactory().StartNew(() =>
            {
                var tableRespone = _client.ListTables();

                bool tableExists = tableRespone.TableNames.Any(t => t == request.TableName);
                if (!tableExists)
                {
                    var response = _client.CreateTable(request);

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


        protected void WaitTillTableCreated(string tableName, CreateTableResponse response)
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
                    var res = _client.DescribeTable(new DescribeTableRequest
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
