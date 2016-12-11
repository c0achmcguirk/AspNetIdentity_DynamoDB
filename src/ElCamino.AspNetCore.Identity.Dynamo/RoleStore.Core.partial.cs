// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.
#if !net45
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using ElCamino.AspNetCore.Identity.Dynamo.Helpers;
using ElCamino.AspNetCore.Identity.Dynamo.Model;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


#if net45
namespace ElCamino.AspNet.Identity.Dynamo
#else
namespace ElCamino.AspNetCore.Identity.Dynamo
#endif
{
    public class RoleStore<TRole> : RoleStore<TRole, IdentityCloudContext> 
        where TRole : IdentityRole, new()
    {
        public RoleStore() : this(new IdentityCloudContext())
        {
        }

        public RoleStore(IdentityCloudContext context) : base(context) { }
    }

    public class RoleStore<TRole, TContext> : RoleStore<TRole, string, IdentityUserRole, TContext> 
        , IRoleStore<TRole>
        where TRole : Model.IdentityRole, new()
        where TContext : IdentityCloudContext, new()
    {
        
        public RoleStore(TContext context)
            : base(context)
        { }

        //Fixing code analysis issue CA1063
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }

    public class RoleStore<TRole, TKey, TUserRole, TContext> : 
        IRoleStore<TRole>, IDisposable
        where TRole : IdentityRole<TKey, TUserRole>, new()
        where TUserRole : IdentityUserRole<TKey>, new()
        where TContext : IdentityCloudContext, new()

    {
        private bool _disposed;

        public RoleStore(IdentityCloudContext<IdentityUser, IdentityRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            this.Context = context;
        }

        public async Task CreateTableIfNotExistsAsync()
        {
            await Context.CreateRoleTableAsync();
        }

        public virtual async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            try
            {
                ((IGenerateKeys) role).GenerateKeys();
                await Context.SaveAsync<TRole>(role, new DynamoDBOperationConfig()
                {
                    TableNamePrefix = Context.TablePrefix,
                    ConsistentRead = true,
                });

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError() { Code = "004", Description = $"Role Creation Failed. {ex.Message}, {ex.StackTrace}" });
            }
        }

        public virtual async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            try {
                await Context.DeleteAsync<TRole>(role, new DynamoDBOperationConfig()
                {
                    TableNamePrefix = Context.TablePrefix,
                    ConsistentRead = true,
                }, cancellationToken);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError() { Code = "006", Description = $"Role Delete Failed. {ex.Message}, {ex.StackTrace}" });
            }
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
                if (Context != null)
                {
                    Context.Dispose();
                }
                _disposed = true;
                Context = null;
            }
        }

        public async Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            return await FindIdAsync(roleId, cancellationToken);
        }

        public async Task<TRole> FindByNameAsync(string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            return await FindIdAsync(KeyHelper.GenerateRowKeyIdentityRole(roleName), cancellationToken);
        }

        public Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            return Task.FromResult(role.Id.ToString());
        }
        public Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            return Task.FromResult(role.Name);
        }


        public Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            role.Name = roleName;
            return Task.FromResult(0);
        }

        public Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            return Task.FromResult(role.NormalizedName);
        }

        public virtual Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            role.NormalizedName = normalizedName;
            return Task.FromResult(0);
        }

        private Task<TRole> FindIdAsync(string roleId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Context.LoadAsync<TRole>(roleId, new DynamoDBOperationConfig()
                {
                    TableNamePrefix = Context.TablePrefix,
                    ConsistentRead = true,
                }, cancellationToken);
        }

        private void ThrowIfDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
        }

        public virtual async Task<IdentityResult> UpdateAsync(TRole role,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            try
            {
                var batchWrite = Context.CreateBatchWrite<TRole>(new DynamoDBOperationConfig()
                {
                    TableNamePrefix = Context.TablePrefix,
                    ConsistentRead = true,
                });

                IGenerateKeys g = role as IGenerateKeys;
                if (!g.PeekRowKey().Equals(role.Id.ToString(), StringComparison.Ordinal))
                {
                    batchWrite.AddDeleteKey(role.Id.ToString());
                }
                g.GenerateKeys();
                batchWrite.AddPutItem(role);
                await Context.ExecuteBatchWriteAsync(new BatchWrite[] {batchWrite}, cancellationToken);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError() { Code = "005", Description = $"Role Update Failed. {ex.Message}, {ex.StackTrace}" });
            }
        }

        public IdentityCloudContext<IdentityUser, IdentityRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim> Context { get; private set; }

        public IQueryable<TRole> Roles
        {
            get
            {
                var tableName = Context.FormatTableNameWithPrefix(Constants.TableNames.RolesTable);
                //var attributes = Constants.
                var result = Context.Client.ScanAsync(new ScanRequest(tableName)).Result;
                var retObj = new List<TRole>();

                foreach (var item in result.Items)
                {
                    if (item.Values != null)
                    {
                        var query = item.AsQueryable();
                        var id = (from r in query where r.Key == "Id" select r.Value.S).FirstOrDefault();
                        var name = (from r in query where r.Key == "Name" select r.Value.S).FirstOrDefault();

                        if (id != null && name != null)
                        {
                            var role = new IdentityRole(name);
                            role.Id = id;

                            retObj.Add(role as TRole);
                        }
                    }
                }

                return retObj.AsQueryable();
            }
        }

    }
}
#endif
