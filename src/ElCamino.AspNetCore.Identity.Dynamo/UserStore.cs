// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.
#if net45
using ElCamino.AspNet.Identity.Dynamo.Model;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ElCamino.AspNet.Identity.Dynamo.Helpers;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using System.Threading;
using System.Collections.Concurrent;

namespace ElCamino.AspNet.Identity.Dynamo
{
    public class UserStore<TUser> : UserStore<TUser, IdentityRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>, IUserStore<TUser>, IUserStore<TUser, string> where TUser : IdentityUser, new()
    {
        public UserStore()
            : this(new IdentityCloudContext<TUser>())
        {
           
        }

        public UserStore(IdentityCloudContext<TUser> context)
            : base(context)
        {
        }

        //Fixing code analysis issue CA1063
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }

    public class UserStore<TUser, TRole, TKey, TUserLogin, TUserRole, TUserClaim> : IUserLoginStore<TUser, TKey>
        , IUserClaimStore<TUser, TKey>
        , IUserRoleStore<TUser, TKey>, IUserPasswordStore<TUser, TKey>
        , IUserSecurityStampStore<TUser, TKey>, IQueryableUserStore<TUser, TKey>
        , IUserEmailStore<TUser, TKey>, IUserPhoneNumberStore<TUser, TKey>
        , IUserTwoFactorStore<TUser, TKey>
        , IUserLockoutStore<TUser, TKey>
        , IUserStore<TUser, TKey>
        , IDisposable
        where TUser : IdentityUser<TKey, TUserLogin, TUserRole, TUserClaim>, new()
        where TRole : IdentityRole<TKey, TUserRole>, new()
        where TKey : IEquatable<TKey>
        where TUserLogin : IdentityUserLogin<TKey>, new()
        where TUserRole : IdentityUserRole<TKey>, new()
        where TUserClaim : IdentityUserClaim<TKey>, new()
    {
        private bool _disposed;
        private IQueryable<TUser> _users;


        public UserStore(IdentityCloudContext<TUser, TRole, TKey, TUserLogin, TUserRole, TUserClaim> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            this.Context = context;
        }

        public async Task CreateTablesIfNotExists()
        {
            await Task.WhenAll(new Task[]
            { 
                Context.CreateUserTableAsync(),
                Context.CreateIndexTableAsync(),
                Context.CreateRoleTableAsync()
            });
        }

        public virtual async Task AddClaimAsync(TUser user, Claim claim)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }


            TUserClaim item = Activator.CreateInstance<TUserClaim>();
            item.UserId = user.UserId;
            item.ClaimType = claim.Type;
            item.ClaimValue = claim.Value;
            item.UserName = user.UserName;
            item.Email = user.Email;
            ((IGenerateKeys)item).GenerateKeys();


            user.Claims.Add(item);

            var putRequest = new PutItemRequest()
            {
                TableName = Context.FormatTableNameWithPrefix(Constants.TableNames.UsersTable),
                Item = Context.ToDocument<TUserClaim>(item).ToAttributeMap(),
            };

            await Context.Client.PutItemAsync(putRequest);
                    
        }

        public virtual async Task AddLoginAsync(TUser user, UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            TUserLogin item = Activator.CreateInstance<TUserLogin>();
            item.UserId = user.Id;
            item.ProviderKey = login.ProviderKey;
            item.LoginProvider = login.LoginProvider;
            item.Email = user.Email;
            item.UserName = user.UserName;
            var t = item as IGenerateKeys;
            ((IGenerateKeys)item).GenerateKeys();

            user.Logins.Add(item);
            IdentityUserIndex index = new IdentityUserIndex();
            index.Id = item.Id.ToString();
            index.UserId = item.UserId.ToString();

            BatchWriteItemRequest batchWriteReq = new BatchWriteItemRequest();
            batchWriteReq.RequestItems = new Dictionary<string, List<WriteRequest>>(10);
            List<WriteRequest> listUserwr = new List<WriteRequest>(10);
            List<WriteRequest> listIndexwr = new List<WriteRequest>(10);

            var indexwr = new WriteRequest();
            indexwr.PutRequest = new PutRequest();
            indexwr.PutRequest.Item = Context.ToDocument<IdentityUserIndex>(index).ToAttributeMap();
            listIndexwr.Add(indexwr);

            var userwr = new WriteRequest();
            userwr.PutRequest = new PutRequest();
            userwr.PutRequest.Item = Context.ToDocument<TUserLogin>(item).ToAttributeMap();
            listUserwr.Add(userwr);

            batchWriteReq.RequestItems.Add(Context.FormatTableNameWithPrefix(Constants.TableNames.UsersTable), listUserwr);
            batchWriteReq.RequestItems.Add(Context.FormatTableNameWithPrefix(Constants.TableNames.IndexTable), listIndexwr);

            await Context.Client.BatchWriteItemAsync(batchWriteReq);
        }

        public virtual async Task AddToRoleAsync(TUser user, string roleName)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException(IdentityResources.ValueCannotBeNullOrEmpty, "roleName");
            }

            TRole roleT = Activator.CreateInstance<TRole>();
            roleT.Name = roleName;
            ((IGenerateKeys)roleT).GenerateKeys();

            TUserRole userToRole = Activator.CreateInstance<TUserRole>();
            userToRole.UserId = user.Id;
            userToRole.RoleId = roleT.Id;
            userToRole.RoleName = roleT.Name;
            userToRole.Email = user.Email;
            userToRole.UserName = user.UserName;
            TUserRole item = userToRole;

            ((IGenerateKeys)item).GenerateKeys();

            user.Roles.Add(item);
            roleT.Users.Add(item);

            await Context.SaveAsync<TUserRole>(userToRole, new DynamoDBOperationConfig()
            {
                TableNamePrefix = this.Context.TablePrefix,
                ConsistentRead = true,
            });

        }

        public async virtual Task CreateAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            ((IGenerateKeys)user).GenerateKeys();

            await Context.SaveAsync<TUser>(user, new DynamoDBOperationConfig()
                {
                    TableNamePrefix = this.Context.TablePrefix,
                    ConsistentRead = true
                });

        }

        public async virtual Task DeleteAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            BatchOperationHelper batchHelper = new BatchOperationHelper();
            batchHelper.Add(Context.FormatTableNameWithPrefix(Constants.TableNames.UsersTable), CreateDeleteRequestForUser(user.UserId, user.Id));
            user.Claims.ToList().ForEach(c => { batchHelper.Add(Context.FormatTableNameWithPrefix(Constants.TableNames.UsersTable), CreateDeleteRequestForUser(c.UserId, c.Id)); });
            user.Roles.ToList().ForEach(r => { batchHelper.Add(Context.FormatTableNameWithPrefix(Constants.TableNames.UsersTable), CreateDeleteRequestForUser(r.UserId, r.Id)); });
            user.Logins.ToList().ForEach(l => 
            { 
                batchHelper.Add(Context.FormatTableNameWithPrefix(Constants.TableNames.UsersTable), CreateDeleteRequestForUser(l.UserId, l.Id));
                batchHelper.Add(Context.FormatTableNameWithPrefix(Constants.TableNames.IndexTable), CreateDeleteRequestForIndex(l.Id));
            });

            await batchHelper.ExecuteBatchAsync(Context.Client);

        }

        private WriteRequest CreateDeleteRequestForUser(TKey UserId, TKey Id)
        {
            var wr = new WriteRequest();
            wr.DeleteRequest = new DeleteRequest();
            wr.DeleteRequest.Key = new Dictionary<string, AttributeValue>(2);
            wr.DeleteRequest.Key.Add("UserId", new AttributeValue() { S = UserId.ToString() });
            wr.DeleteRequest.Key.Add("Id", new AttributeValue() { S = Id.ToString() });
            return wr;
        }

        private WriteRequest CreateDeleteRequestForIndex(TKey id)
        {
            var iwr = new WriteRequest();
            iwr.DeleteRequest = new DeleteRequest();
            iwr.DeleteRequest.Key = new Dictionary<string, AttributeValue>(1);
            iwr.DeleteRequest.Key.Add("Id", new AttributeValue() { S = id.ToString() });
            return iwr;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (this.Context != null)
                {
                    this.Context.Dispose();
                }
                _disposed = true;
                Context = null;
            }
        }

        public async virtual Task<TUser> FindAsync(UserLoginInfo login)
        {
            ThrowIfDisposed();

            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            return await Context.LoadAsync<IdentityUserIndex<TKey>>(login.GenerateRowKeyUserLoginInfo(),
                new DynamoDBOperationConfig()
                {
                    TableNamePrefix = Context.TablePrefix,
                    ConsistentRead = true,
                })
                .ContinueWith<Task<TUser>>(new Func<Task<IdentityUserIndex<TKey>>, Task<TUser>>((index) =>
                {
                    if (index.Result != null)
                    {
                        return FindByIdAsync(index.Result.UserId);
                    }
                    return new TaskFactory<TUser>().StartNew(() => null);

                })).Unwrap();

        }

        public async Task<TUser> FindByEmailAsync(string email)
        {
            this.ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException(IdentityResources.ValueCannotBeNullOrEmpty, "email");
            }

            return await Context.Client.QueryAsync(new QueryRequest()
            {
                TableName = Context.FormatTableNameWithPrefix(Constants.TableNames.UsersTable),
                IndexName = Constants.SecondaryIndexNames.UserEmailIndex,
                KeyConditions = new Dictionary<string, Condition>()
                    { 
                        {"Email", new Condition()
                            { 
                                ComparisonOperator = ComparisonOperator.EQ,
                                AttributeValueList = new List<AttributeValue>() { new AttributeValue() { S = email }}
                            }
                        }
                }
            })
            .ContinueWith<TUser>(new Func<Task<QueryResponse>, TUser>((qResponse) =>
            {
                return ConvertResponseToUser(qResponse.Result.Items);
            })
            );
            

        }

        public async Task<IEnumerable<TUser>> FindAllByEmailAsync(string email)
        {
            this.ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException(IdentityResources.ValueCannotBeNullOrEmpty, "email");
            }

            return await Context.Client.QueryAsync(new QueryRequest()
            {
                TableName = Context.FormatTableNameWithPrefix(Constants.TableNames.UsersTable),
                IndexName = Constants.SecondaryIndexNames.UserEmailIndex,
                KeyConditions = new Dictionary<string, Condition>()
                    { 
                        {"Email", new Condition()
                            { 
                                ComparisonOperator = ComparisonOperator.EQ,
                                AttributeValueList = new List<AttributeValue>() { new AttributeValue() { S = email }}
                            }
                        }
                }
            })
            .ContinueWith<IEnumerable<TUser>>(new Func<Task<QueryResponse>, IEnumerable<TUser>>((qResponse) =>
            {
                return ConvertResponseToUsers(qResponse.Result.Items);
            })
            );


        }


        public virtual async Task<TUser> FindByIdAsync(TKey userId)
        {
            this.ThrowIfDisposed();

            if (userId == null)
            {
                throw new ArgumentNullException("userId");
            }
            if (string.IsNullOrWhiteSpace(userId.ToString()))
            {
                throw new ArgumentException(IdentityResources.ValueCannotBeNullOrEmpty, "userId");
            }


            return await Context.Client.QueryAsync(new QueryRequest()
            {
                ConsistentRead = true,
                TableName = Context.FormatTableNameWithPrefix(Constants.TableNames.UsersTable),
                KeyConditions = new Dictionary<string, Condition>()
                { 
                    {"UserId", new Condition()
                        { 
                            ComparisonOperator = ComparisonOperator.EQ,
                            AttributeValueList = new List<AttributeValue>() { new AttributeValue() { S = userId.ToString() }}
                        }
                    }
                }
            })
            .ContinueWith<TUser>(new Func<Task<QueryResponse>, TUser>((qResponse) =>
            {
                return ConvertResponseToUser(qResponse.Result.Items);
            }));
           
        }

        private IEnumerable<TUser> ConvertResponseToUsers(List<Dictionary<string, AttributeValue>> response)
        {
            ConcurrentBag<TUser> users = new ConcurrentBag<TUser>();
            var userDict = response
                .Where(c => c["Id"].S.Equals(c["UserId"].S, StringComparison.OrdinalIgnoreCase));

            Parallel.ForEach<Dictionary<string, AttributeValue>>(userDict, (userItem) =>
            {
                //User
                TUser user = Context.FromDocument<TUser>(Document.FromAttributeMap(userItem));
                users.Add(MapResponseToUser(user, response));
            });
            return users;
        }

        private TUser ConvertResponseToUser(List<Dictionary<string, AttributeValue>> response)
        {
            //Fixes issue where OAuth user has not created a local account, not finding the PasswordHash field.
            var userDict = response
                .FirstOrDefault(c => c["Id"].S.Equals(c["UserId"].S, StringComparison.OrdinalIgnoreCase)); 

            if (userDict != null)
            {
                //User
                TUser user = Context.FromDocument<TUser>(Document.FromAttributeMap(userDict));
                return MapResponseToUser(user, response);
            }
            return null;
        }

        private TUser MapResponseToUser(TUser user, List<Dictionary<string, AttributeValue>> response)
        {
            //Claims
            response
                .Where(c => c.ContainsKey("ClaimType")
                    && c.ContainsKey("UserId")
                    && c["UserId"].S.Equals(user.UserId.ToString(), StringComparison.OrdinalIgnoreCase))
                .Select(c => Context.FromDocument<TUserClaim>(Document.FromAttributeMap(c)))
                .ToList()
                .ForEach(uc => { user.Claims.Add(uc); });

            //Logins
            response
                .Where(c => c.ContainsKey("LoginProvider")
                    && c.ContainsKey("UserId")
                    && c["UserId"].S.Equals(user.UserId.ToString(), StringComparison.OrdinalIgnoreCase))
                .Select(c => Context.FromDocument<TUserLogin>(Document.FromAttributeMap(c)))
                .ToList()
                .ForEach(l => { user.Logins.Add(l); });

            //Roles
            response
                .Where(c => c.ContainsKey("RoleName")
                    && c.ContainsKey("UserId")
                    && c["UserId"].S.Equals(user.UserId.ToString(), StringComparison.OrdinalIgnoreCase))
                .Select(c => Context.FromDocument<TUserRole>(Document.FromAttributeMap(c)))
                .ToList()
                .ForEach(r => { user.Roles.Add(r); });


            return user;
        }

        public virtual async Task<TUser> FindByNameAsync(string userName)
        {
            this.ThrowIfDisposed();

            return await Context.Client.QueryAsync(new QueryRequest()
            {
                TableName = Context.FormatTableNameWithPrefix(Constants.TableNames.UsersTable),
                IndexName = Constants.SecondaryIndexNames.UserNameIndex,
                KeyConditions = new Dictionary<string, Condition>()
                { 
                    {"UserName", new Condition()
                        { 
                            ComparisonOperator = ComparisonOperator.EQ,
                            AttributeValueList = new List<AttributeValue>() { new AttributeValue() { S = userName }}
                        }
                    }
                }
            })
            .ContinueWith<TUser>(new Func<Task<QueryResponse>, TUser>((qResponse) =>
            {
                return ConvertResponseToUser(qResponse.Result.Items);
            }));

        }

        public Task<int> GetAccessFailedCountAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<int>(user.AccessFailedCount);
        }

        public virtual Task<IList<Claim>> GetClaimsAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<IList<Claim>>(user.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList());
        }

        public Task<string> GetEmailAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<string>(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<bool>(user.EmailConfirmed);
        }

        public Task<bool> GetLockoutEnabledAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<bool>(user.LockoutEnabled);
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            Func<DateTimeOffset> funcDt = () =>
                {
                    if(user.LockoutEndDateUtc.HasValue)
                    {
                        if(user.LockoutEndDateUtc.Value.Kind != DateTimeKind.Utc)
                        {
                            return new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value.ToUniversalTime(), DateTimeKind.Utc));
                        }
                        return new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc));

                    }
                    return new DateTimeOffset();
                };
            return Task.FromResult<DateTimeOffset>(funcDt());
        }

        public virtual Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<IList<UserLoginInfo>>((from l in user.Logins select new UserLoginInfo(l.LoginProvider, l.ProviderKey)).ToList<UserLoginInfo>());
        }

        public Task<string> GetPasswordHashAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<string>(user.PasswordHash);
        }

        public Task<string> GetPhoneNumberAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<string>(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<bool>(user.PhoneNumberConfirmed);
        }

        public virtual Task<IList<string>> GetRolesAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return Task.FromResult<IList<string>>(user.Roles.ToList().Select(r => r.RoleName).ToList());
        }

        public Task<string> GetSecurityStampAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<string>(user.SecurityStamp);
        }

        public Task<bool> GetTwoFactorEnabledAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<bool>(user.TwoFactorEnabled);
        }

        public Task<bool> HasPasswordAsync(TUser user)
        {
            return Task.FromResult<bool>(user.PasswordHash != null);
        }

        public Task<int> IncrementAccessFailedCountAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.AccessFailedCount++;
            return Task.FromResult<int>(user.AccessFailedCount);
        }

        public virtual Task<bool> IsInRoleAsync(TUser user, string roleName)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException(IdentityResources.ValueCannotBeNullOrEmpty, "roleName");
            }

            return Task.FromResult<bool>(user.Roles.Any(r=> r.Id.ToString() == KeyHelper.GenerateRowKeyIdentityUserRole(roleName)));
        }

        public virtual async Task RemoveClaimAsync(TUser user, Claim claim)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }

            if (string.IsNullOrWhiteSpace(claim.Type))
            {
                throw new ArgumentException(IdentityResources.ValueCannotBeNullOrEmpty, "claim.Type");
            }

            // Claim ctor doesn't allow Claim.Value to be null. Need to allow string.empty.

                   
            TUserClaim local = (from uc in user.Claims
                                where uc.Id.ToString() == KeyHelper.GenerateRowKeyIdentityUserClaim(claim.Type, claim.Value)
                                select uc).FirstOrDefault();
            if(local != null)
            {
                user.Claims.Remove(local);
                await Context.DeleteAsync<TUserClaim>(local, new DynamoDBOperationConfig()
                {
                    TableNamePrefix = Context.TablePrefix,
                    ConsistentRead = true,
                });
            }
        }

        public virtual async Task RemoveFromRoleAsync(TUser user, string roleName)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException(IdentityResources.ValueCannotBeNullOrEmpty, "roleName");
            }
            TUserRole item = user.Roles.FirstOrDefault<TUserRole>(r => r.Id.ToString() == KeyHelper.GenerateRowKeyIdentityUserRole(roleName));
            if (item != null)
            {
                user.Roles.Remove(item);
                await Context.DeleteAsync<TUserRole>(item, new DynamoDBOperationConfig()
                {
                    TableNamePrefix = Context.TablePrefix,
                    ConsistentRead = true,
                });
            }
        }

        public virtual async Task RemoveLoginAsync(TUser user, UserLoginInfo login)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            BatchWriteItemRequest batchWriteReq = new BatchWriteItemRequest();
            batchWriteReq.RequestItems = new Dictionary<string, List<WriteRequest>>(10);
            List<WriteRequest> listUserwr = new List<WriteRequest>(10);
            List<WriteRequest> listIndexwr = new List<WriteRequest>(10);
            foreach (TUserLogin local in (from uc in user.Logins.ToList()
                                            where uc.Id.ToString() == KeyHelper.GenerateRowKeyUserLoginInfo(login)
                                            select uc))
            {
                var wr = CreateDeleteRequestForUser(local.UserId, local.Id);
                user.Logins.Remove(local);
                listUserwr.Add(wr);

                var iwr = CreateDeleteRequestForIndex(local.Id);
                listIndexwr.Add(iwr);
            }
            batchWriteReq.RequestItems.Add(Context.FormatTableNameWithPrefix(Constants.TableNames.UsersTable), listUserwr);
            batchWriteReq.RequestItems.Add(Context.FormatTableNameWithPrefix(Constants.TableNames.IndexTable), listIndexwr);

            if (listUserwr.Count > 0)
            {
                var tresult = await Context.Client.BatchWriteItemAsync(batchWriteReq);
            }

        }

        public Task ResetAccessFailedCountAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.AccessFailedCount = 0;
            return Task.FromResult<int>(0);
        }

        public async Task SetEmailAsync(TUser user, string email)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            //Only remove the email if different
            if (string.IsNullOrWhiteSpace(user.Email) ||
                !user.Email.Equals(email?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                var itemUpdates = CreateEmailUpdateRequests(user, email);
                List<Task> tasks = new List<Task>(itemUpdates.Count);
                foreach (var updRequest in itemUpdates)
                {
                    updRequest.TableName = Context.FormatTableNameWithPrefix(Constants.TableNames.UsersTable);
                    tasks.Add(Context.Client.UpdateItemAsync(updRequest));
                }
                await Task.WhenAll(tasks.ToArray());
            }
            user.Email = email;
        }

        private List<UpdateItemRequest> CreateEmailUpdateRequests(TUser user, string emailNew)
        {
            List<UpdateItemRequest> list = new List<UpdateItemRequest>(200);
            list.Add(CreateEmailUpdateRequest(new Dictionary<string,AttributeValue>() 
            { 
                { "UserId", new AttributeValue() { S = user.UserId.ToString() } }, 
                { "Id", new AttributeValue(){S = user.Id.ToString()} } 
            }, emailNew));
            user.Roles.ToList().ForEach(r =>
            {
                list.Add(CreateEmailUpdateRequest(new Dictionary<string, AttributeValue>() 
                { 
                    { "UserId", new AttributeValue() { S = r.UserId.ToString() } }, 
                    { "Id", new AttributeValue(){S = r.Id.ToString()} } 
                }, emailNew));
            });
            user.Claims.ToList().ForEach(c =>
            {
                list.Add(CreateEmailUpdateRequest(new Dictionary<string, AttributeValue>() 
                { 
                    { "UserId", new AttributeValue() { S = c.UserId.ToString() } }, 
                    { "Id", new AttributeValue(){S = c.Id.ToString()} } 
                }, emailNew));
            });
            user.Logins.ToList().ForEach(l =>
            {
                list.Add(CreateEmailUpdateRequest(new Dictionary<string, AttributeValue>() 
                { 
                    { "UserId", new AttributeValue() { S = l.UserId.ToString() } }, 
                    { "Id", new AttributeValue(){S = l.Id.ToString()} } 
                }, emailNew));
            });

            return list;
        }

        private UpdateItemRequest CreateEmailUpdateRequest(Dictionary<string,AttributeValue> key, string emailNew)
        {
            var userwr = new UpdateItemRequest();
            userwr.AttributeUpdates = new Dictionary<string, AttributeValueUpdate>();
            if (string.IsNullOrWhiteSpace(emailNew))
            {
                userwr.AttributeUpdates.Add("Email", new AttributeValueUpdate() 
                { Action = Amazon.DynamoDBv2.AttributeAction.DELETE });
            }
            else
            {
                userwr.AttributeUpdates.Add("Email", new AttributeValueUpdate()
                {
                    Action = Amazon.DynamoDBv2.AttributeAction.PUT,
                    Value = new AttributeValue() { S = emailNew }
                });
            }
            userwr.Key = key;
            return userwr;
        }

        public Task SetEmailConfirmedAsync(TUser user, bool confirmed)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.EmailConfirmed = confirmed;
            return Task.FromResult<int>(0);
        }

        public Task SetLockoutEnabledAsync(TUser user, bool enabled)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.LockoutEnabled = enabled;
            return Task.FromResult<int>(0);
        }

        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset lockoutEnd)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.LockoutEndDateUtc = (lockoutEnd == DateTimeOffset.MinValue) ? null : new DateTime?(lockoutEnd.UtcDateTime);
            return Task.FromResult<int>(0);
        }

        public Task SetPasswordHashAsync(TUser user, string passwordHash)
        {
            this.ThrowIfDisposed();
            
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.PasswordHash = passwordHash;
            return Task.FromResult<int>(0);
        }

        public Task SetPhoneNumberAsync(TUser user, string phoneNumber)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.PhoneNumber = phoneNumber;
            return Task.FromResult<int>(0);
        }

        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.PhoneNumberConfirmed = confirmed;
            return Task.FromResult<int>(0);
        }

        public Task SetSecurityStampAsync(TUser user, string stamp)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.SecurityStamp = stamp;
            return Task.FromResult<int>(0);
        }

        public Task SetTwoFactorEnabledAsync(TUser user, bool enabled)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.TwoFactorEnabled = enabled;
            return Task.FromResult<int>(0);
        }

        private void ThrowIfDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
        }

        public async virtual Task UpdateAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            //Change user name on roles, logins and claims, if different
            var itemUpdates = CreateUserNameUpdateRequests(user, user.UserName);
            List<Task> tasks = new List<Task>(itemUpdates.Count + 1);

            if (itemUpdates.Count > 0) //Only attempt username change if any differences found.
            {
                foreach (var updRequest in itemUpdates)
                {
                    updRequest.TableName = Context.FormatTableNameWithPrefix(Constants.TableNames.UsersTable);
                    tasks.Add(Context.Client.UpdateItemAsync(updRequest));
                }
            }
            tasks.Add(Context.SaveAsync<TUser>(user, new DynamoDBOperationConfig()
            {
                TableNamePrefix = this.Context.TablePrefix,
                ConsistentRead = true
            }));

            await Task.WhenAll(tasks);
            user.Roles.ToList().ForEach(r => r.UserName = user.UserName);
            user.Claims.ToList().ForEach(c => c.UserName = user.UserName);
            user.Logins.ToList().ForEach(l => l.UserName = user.UserName);
        }

        /// <summary>
        /// Create updates for any roles, claims and/or logins that don't have the username passed in.
        /// </summary>
        /// <param name="user">User containing updates.</param>
        /// <param name="userNameNew">The 'new' username to check against. </param>
        /// <returns></returns>
        private List<UpdateItemRequest> CreateUserNameUpdateRequests(TUser user, string userNameNew)
        {
            List<UpdateItemRequest> list = new List<UpdateItemRequest>(200);
            user.Roles.Where(x => !x.UserName.Equals(userNameNew, StringComparison.OrdinalIgnoreCase)).ToList().ForEach(r =>
            {
                list.Add(CreateUserNameUpdateRequest(new Dictionary<string, AttributeValue>() 
                { 
                    { "UserId", new AttributeValue() { S = r.UserId.ToString() } }, 
                    { "Id", new AttributeValue(){S = r.Id.ToString()} } 
                }, userNameNew));
            });
            user.Claims.Where(x => !x.UserName.Equals(userNameNew, StringComparison.OrdinalIgnoreCase)).ToList().ForEach(c =>
            {
                list.Add(CreateUserNameUpdateRequest(new Dictionary<string, AttributeValue>() 
                { 
                    { "UserId", new AttributeValue() { S = c.UserId.ToString() } }, 
                    { "Id", new AttributeValue(){S = c.Id.ToString()} } 
                }, userNameNew));
            });
            user.Logins.Where(x => !x.UserName.Equals(userNameNew, StringComparison.OrdinalIgnoreCase)).ToList().ForEach(l =>
            {
                list.Add(CreateUserNameUpdateRequest(new Dictionary<string, AttributeValue>() 
                { 
                    { "UserId", new AttributeValue() { S = l.UserId.ToString() } }, 
                    { "Id", new AttributeValue(){S = l.Id.ToString()} } 
                }, userNameNew));
            });

            return list;
        }

        private UpdateItemRequest CreateUserNameUpdateRequest(Dictionary<string, AttributeValue> key, string userNameNew)
        {
            var userwr = new UpdateItemRequest();
            userwr.AttributeUpdates = new Dictionary<string, AttributeValueUpdate>();
            userwr.AttributeUpdates.Add("UserName", new AttributeValueUpdate()
            {
                Action = Amazon.DynamoDBv2.AttributeAction.PUT,
                Value = new AttributeValue() { S = userNameNew }
            });
            userwr.Key = key;
            return userwr;
        }


        public IdentityCloudContext<TUser, TRole, TKey, TUserLogin, TUserRole, TUserClaim> Context { get; private set; }


        public IQueryable<TUser> Users
        {
            get
            {
                ThrowIfDisposed();
                return _users;
            }
        }
    }
}
#endif
