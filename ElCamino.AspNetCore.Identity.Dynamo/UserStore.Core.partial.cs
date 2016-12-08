// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.
#if !net45
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using ElCamino.AspNetCore.Identity.Dynamo;
using ElCamino.AspNetCore.Identity.Dynamo.Helpers;
using ElCamino.AspNetCore.Identity.Dynamo.Model;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace ElCamino.AspNetCore.Identity.Dynamo
{
    public class UserStore<TUser> : UserStore<TUser, IdentityCloudContext> 
        where TUser : IdentityUser, new() 
    {
        public UserStore() : this(new IdentityCloudContext())
        { }

        public UserStore(IdentityCloudContext context) : base(context)
        { }

        //Fixing code analysis issue CA1063
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }

    public class UserStore<TUser, TContext> : UserStore<TUser, IdentityRole, TContext>, IUserStore<TUser>
        where TUser : IdentityUser, new()
        where TContext : IdentityCloudContext, new()
    {
        public UserStore(TContext context)
            : base(context)
        {
        }

        //Fixing code analysis issue CA1063
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }

    public class UserStore<TUser, TRole, TContext> : UserStore<TUser, TRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim, TContext>, IUserStore<TUser>
        where TUser : IdentityUser, new()
        where TRole : IdentityRole<string, IdentityUserRole>, new()
        where TContext : IdentityCloudContext, new()
    {
        public UserStore(TContext context)
            : base(context)
        {
        }
    }

    public class UserStore<TUser, TRole, TKey, TUserLogin, TUserRole, TUserClaim, TContext> : IUserLoginStore<TUser>
        , IUserClaimStore<TUser>
        , IUserRoleStore<TUser>
        , IUserPasswordStore<TUser>
        , IUserSecurityStampStore<TUser>
        , IQueryableUserStore<TUser>
        , IUserEmailStore<TUser>
        , IUserPhoneNumberStore<TUser>
        , IUserTwoFactorStore<TUser>
        , IUserLockoutStore<TUser>
        , IUserStore<TUser>
        , IDisposable
        where TUser : IdentityUser<TKey, TUserLogin, TUserRole, TUserClaim>, new()
        where TRole : IdentityRole<TKey, TUserRole>, new()
        where TKey : IEquatable<TKey>
        where TUserLogin : IdentityUserLogin<TKey>, new()
        where TUserRole : IdentityUserRole<TKey>, new()
        where TUserClaim : IdentityUserClaim<TKey>, new()
        where TContext : IdentityCloudContext, new()
    {
        private bool _disposed;
        private IQueryable<TUser> _users;

        private AmazonDynamoDBClient _dbClient;
        private string _tablePrefix;

        public TContext Context { get; private set; }

        public UserStore(TContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            this.Context = context;
            _dbClient = context.Client;
            _tablePrefix = context.TablePrefix;
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


        public virtual async Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            foreach (var claim in claims)
            {
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

                await Context.Client.PutItemAsync(putRequest, cancellationToken);
            }
        }

        public Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public virtual async Task AddClaimAsync(TUser user, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
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

            await Context.Client.PutItemAsync(putRequest, cancellationToken);
        }

        public virtual async Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
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

            await Context.Client.BatchWriteItemAsync(batchWriteReq, cancellationToken);
        }

        public virtual async Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException(IdentityResources.ValueCannotBeNullOrEmpty, nameof(roleName));
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
            }, cancellationToken);
        }

        public virtual async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            ((IGenerateKeys)user).GenerateKeys();

            try
            {
                await Context.SaveAsync<TUser>(user, new DynamoDBOperationConfig()
                {
                    TableNamePrefix = this.Context.TablePrefix,
                    ConsistentRead = true
                }, cancellationToken);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError() { Code = "001", Description = $"User Creation Failed. {ex.Message}, {ex.StackTrace}"}); 
            }
        }

        public virtual async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            try
            {
                BatchOperationHelper batchHelper = new BatchOperationHelper();
                batchHelper.Add(Context.FormatTableNameWithPrefix(Constants.TableNames.UsersTable),
                    CreateDeleteRequestForUser(user.UserId, user.Id));
                user.Claims.ToList()
                    .ForEach(
                        c =>
                        {
                            batchHelper.Add(Context.FormatTableNameWithPrefix(Constants.TableNames.UsersTable),
                                CreateDeleteRequestForUser(c.UserId, c.Id));
                        });
                user.Roles.ToList()
                    .ForEach(
                        r =>
                        {
                            batchHelper.Add(Context.FormatTableNameWithPrefix(Constants.TableNames.UsersTable),
                                CreateDeleteRequestForUser(r.UserId, r.Id));
                        });
                user.Logins.ToList().ForEach(l =>
                {
                    batchHelper.Add(Context.FormatTableNameWithPrefix(Constants.TableNames.UsersTable),
                        CreateDeleteRequestForUser(l.UserId, l.Id));
                    batchHelper.Add(Context.FormatTableNameWithPrefix(Constants.TableNames.IndexTable),
                        CreateDeleteRequestForIndex(l.Id));
                });

                await batchHelper.ExecuteBatchAsync(Context.Client);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError() { Code = "002", Description = $"User Delete Failed. {ex.Message} {ex.StackTrace}"});
            }
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

        public virtual async Task<TUser> FindAsync(UserLoginInfo login, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (login == null)
            {
                throw new ArgumentNullException(nameof(login));
            }

            return await Context.LoadAsync<IdentityUserIndex<string>>(login.GenerateRowKeyUserLoginInfo(),
                new DynamoDBOperationConfig()
                {
                    TableNamePrefix = Context.TablePrefix,
                    ConsistentRead = true,
                }, cancellationToken)
                .ContinueWith<Task<TUser>>(new Func<Task<IdentityUserIndex<string>>, Task<TUser>>((index) =>
                {
                    if (index.Result != null)
                    {
                        return FindByIdAsync(index.Result.UserId, cancellationToken);
                    }
                    return new TaskFactory<TUser>().StartNew(() => null, cancellationToken);
                }), cancellationToken).Unwrap();
        }

        public async Task<TUser> FindByEmailAsync(string email, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
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

        public Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loginInfo = new UserLoginInfo(loginProvider, providerKey, "");
            return FindAsync(loginInfo, cancellationToken);
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

        public virtual async Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException(IdentityResources.ValueCannotBeNullOrEmpty, nameof(userId));
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
            }, cancellationToken)
            .ContinueWith<TUser>((qResponse) => ConvertResponseToUser(qResponse.Result.Items), cancellationToken);
        }

        private IEnumerable<TUser> ConvertResponseToUsers(List<Dictionary<string, AttributeValue>> response)
        {
            ConcurrentBag<TUser> users = new ConcurrentBag<TUser>();
            var userDict = response
                .Where(c => c["Id"].S.Equals(c["UserId"].S, StringComparison.OrdinalIgnoreCase));

#if net45
            Parallel.ForEach<Dictionary<string, AttributeValue>>(userDict, (userItem) =>
#else
            foreach (var userItem in userDict)
#endif
            {
                //User
                TUser user = Context.FromDocument<TUser>(Document.FromAttributeMap(userItem));
                users.Add(MapResponseToUser(user, response));
            }

            #if net45
            );
            #endif

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

        public virtual async Task<TUser> FindByNameAsync(string userName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

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
            }, cancellationToken)
            .ContinueWith<TUser>((qResponse) => ConvertResponseToUser(qResponse.Result.Items), cancellationToken);
        }

        public Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult<int>(user.AccessFailedCount);
        }

        public virtual Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<IList<Claim>>(user.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList());
        }

        public Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult<string>(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult<bool>(user.EmailConfirmed);
        }

        public Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult<bool>(user.LockoutEnabled);
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
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
            return Task.FromResult<DateTimeOffset?>(funcDt());
        }

        public virtual Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<IList<UserLoginInfo>>((from l in user.Logins select new UserLoginInfo(l.LoginProvider, l.ProviderKey, user.UserName)).ToList<UserLoginInfo>());
        }

        public Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult<string>(user.PasswordHash);
        }

        public Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult<string>(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult<bool>(user.PhoneNumberConfirmed);
        }

        public virtual Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult<IList<string>>(user.Roles.ToList().Select(r => r.RoleName).ToList());
        }

        public Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult<string>(user.SecurityStamp);
        }

        public Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult<bool>(user.TwoFactorEnabled);
        }

        public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<bool>(user.PasswordHash != null);
        }

        public Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.AccessFailedCount++;
            return Task.FromResult<int>(user.AccessFailedCount);
        }

        public virtual Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException(IdentityResources.ValueCannotBeNullOrEmpty, nameof(roleName));
            }

            return Task.FromResult<bool>(user.Roles.Any(r=> r.Id.ToString() == KeyHelper.GenerateRowKeyIdentityUserRole(roleName)));
        }

        public Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public virtual async Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            foreach (var claim in claims)
            {
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
                    }, cancellationToken);
                }
            }
        }

        public Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public virtual async Task RemoveClaimAsync(TUser user, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
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
                }, cancellationToken);
            }
        }

        public virtual async Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException(IdentityResources.ValueCannotBeNullOrEmpty, nameof(roleName));
            }
            var item = user.Roles.FirstOrDefault<TUserRole>(r => r.Id.ToString() == KeyHelper.GenerateRowKeyIdentityUserRole(roleName));
            if (item != null)
            {
                user.Roles.Remove(item);
                await Context.DeleteAsync<TUserRole>(item, new DynamoDBOperationConfig()
                {
                    TableNamePrefix = Context.TablePrefix,
                    ConsistentRead = true,
                }, cancellationToken);
            }
        }

        public virtual async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var batchWriteReq = new BatchWriteItemRequest();
            batchWriteReq.RequestItems = new Dictionary<string, List<WriteRequest>>(10);
            var listUserwr = new List<WriteRequest>(10);
            var listIndexwr = new List<WriteRequest>(10);
            var loginInfo = new UserLoginInfo(loginProvider, providerKey, user.UserName);
            foreach (var local in (from uc in user.Logins.ToList()
                                            where uc.Id.ToString() == KeyHelper.GenerateRowKeyUserLoginInfo(loginInfo)
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
                var tresult = await Context.Client.BatchWriteItemAsync(batchWriteReq, cancellationToken);
            }
        }

        public virtual async Task RemoveLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            var batchWriteReq = new BatchWriteItemRequest();
            batchWriteReq.RequestItems = new Dictionary<string, List<WriteRequest>>(10);
            var listUserwr = new List<WriteRequest>(10);
            var listIndexwr = new List<WriteRequest>(10);
            foreach (var local in (from uc in user.Logins.ToList()
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
                var tresult = await Context.Client.BatchWriteItemAsync(batchWriteReq, cancellationToken);
            }

        }

        public Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.AccessFailedCount = 0;
            return Task.FromResult<int>(0);
        }

        public virtual Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.NormalizedEmail);
        }

        public virtual Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.NormalizedEmail = normalizedEmail;
            return Task.FromResult(0);
        }

        public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public virtual Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.UserName);
        }

        public virtual Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.UserName = userName;
            return Task.FromResult(0);
        }


        public virtual Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.NormalizedUserName);
        }

        public virtual Task SetNormalizedUserNameAsync(TUser user, string normalizedUserName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.NormalizedUserName = normalizedUserName;
            return Task.FromResult(0);
        }

        public async Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            //Only remove the email if different
            if (string.IsNullOrWhiteSpace(user.Email) || !user.Email.Equals(email?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                var itemUpdates = CreateEmailUpdateRequests(user, email);
                var tasks = new List<Task>(itemUpdates.Count);
                foreach (var updRequest in itemUpdates)
                {
                    updRequest.TableName = Context.FormatTableNameWithPrefix(Constants.TableNames.UsersTable);
                    tasks.Add(Context.Client.UpdateItemAsync(updRequest, cancellationToken));
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

        public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.EmailConfirmed = confirmed;
            return Task.FromResult<int>(0);
        }

        public Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.LockoutEnabled = enabled;
            return Task.FromResult<int>(0);
        }

        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.LockoutEndDateUtc = (lockoutEnd == DateTimeOffset.MinValue) ? null : lockoutEnd?.UtcDateTime;
            return Task.FromResult<int>(0);
        }

        public Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.PasswordHash = passwordHash;
            return Task.FromResult<int>(0);
        }

        public Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.PhoneNumber = phoneNumber;
            return Task.FromResult<int>(0);
        }

        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.PhoneNumberConfirmed = confirmed;
            return Task.FromResult<int>(0);
        }

        public Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.SecurityStamp = stamp;
            return Task.FromResult<int>(0);
        }

        public Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
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

        public virtual async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            try
            {
                //Change user name on roles, logins and claims, if different
                var itemUpdates = CreateUserNameUpdateRequests(user, user.UserName);
                List<Task> tasks = new List<Task>(itemUpdates.Count + 1);

                if (itemUpdates.Count > 0) // Only attempt username change if any differences found.
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
                }, cancellationToken));

                await Task.WhenAll(tasks);
                user.Roles.ToList().ForEach(r => r.UserName = user.UserName);
                user.Claims.ToList().ForEach(c => c.UserName = user.UserName);
                user.Logins.ToList().ForEach(l => l.UserName = user.UserName);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError() { Code = "003", Description = $"User Update Failed. {ex.Message}, {ex.StackTrace}" });
            }
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
