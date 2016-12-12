// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using ElCamino.AspNetCore.Identity.Dynamo;
using ElCamino.AspNetCore.Identity.Dynamo.Model;
using Microsoft.AspNetCore.Identity;

namespace ElCamino.AspNet.Identity.Dynamo.Tests
{
    [TestClass]
    public class RoleStoreTests
    {
        private static IdentityRole CurrentRole;
        [TestInitialize]
        public void Initialize()
        {
            using (RoleStore<IdentityRole> store = new RoleStore<IdentityRole>())
            {
                var taskCreateTables = store.CreateTableIfNotExistsAsync();
                taskCreateTables.Wait();
            }
            // !!!
            //CreateRole();
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.RoleStore")]
        public void RoleStoreCtors()
        {
            try
            {
                new RoleStore<IdentityRole>(null);
            }
            catch (ArgumentException) { }
        }

        /*
        [TestMethod]
        [TestCategory("Identity.Dynamo.RoleStore")]
        public void CreateRole()
        {    
            using (RoleStore<IdentityRole> store = new RoleStore<IdentityRole>())
            {
                using (RoleManager<IdentityRole> manager = new RoleManager<IdentityRole>(store))
                {
                    string roleNew = string.Format("TestRole_{0}", Guid.NewGuid());
                    var role = new IdentityRole(roleNew);
                    var createTask = manager.CreateAsync(role);
                    createTask.Wait();
                    CurrentRole = role;

                    try
                    {
                        var task = store.CreateAsync(null);
                        task.Wait();
                    }
                    catch (Exception ex)
                    {
                        Assert.IsNotNull(ex, "Argument exception not raised");
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.RoleStore")]
        public void ThrowIfDisposed()
        {
            using (RoleStore<IdentityRole> store = new RoleStore<IdentityRole>())
            {
                RoleManager<IdentityRole> manager = new RoleManager<IdentityRole>(store);
                manager.Dispose();

                try
                {
                    var task = store.DeleteAsync(null);
                }
                catch (ArgumentException) { }
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.RoleStore")]
        public void UpdateRole()
        {
            using (RoleStore<IdentityRole> store = new RoleStore<IdentityRole>())
            {
                using (RoleManager<IdentityRole> manager = new RoleManager<IdentityRole>(store))
                {
                    string roleNew = string.Format("TestRole_{0}", Guid.NewGuid());

                    var role = new IdentityRole(roleNew);
                    var createTask = manager.CreateAsync(role);
                    createTask.Wait();

                    role.Name = Guid.NewGuid() + role.Name;
                    var updateTask = manager.UpdateAsync(role);
                    updateTask.Wait();

                    var findTask = manager.FindByIdAsync(role.Id);

                    Assert.IsNotNull(findTask.Result, "Find Role Result is null");
                    Assert.AreEqual<string>(role.Id, findTask.Result.Id, "RowKeys don't match.");
                    Assert.AreNotEqual<string>(roleNew, findTask.Result.Name, "Name not updated.");

                    try
                    {
                        var task = store.UpdateAsync(null);
                        task.Wait();
                    }
                    catch (Exception ex)
                    {
                        Assert.IsNotNull(ex, "Argument exception not raised");
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.RoleStore")]
        public void UpdateRole2()
        {
            using (RoleStore<IdentityRole> store = new RoleStore<IdentityRole>())
            {
                using (RoleManager<IdentityRole> manager = new RoleManager<IdentityRole>(store))
                {
                    string roleNew = string.Format("{0}_TestRole", Guid.NewGuid());

                    var role = new IdentityRole(roleNew);
                    var createTask = manager.CreateAsync(role);
                    createTask.Wait();

                    role.Name = role.Name + Guid.NewGuid();
                    var updateTask = manager.UpdateAsync(role);
                    updateTask.Wait();

                    var findTask = manager.FindByIdAsync(role.Id);

                    Assert.IsNotNull(findTask.Result, "Find Role Result is null");
                    Assert.AreEqual<string>(role.Id, findTask.Result.Id, "RowKeys don't match.");
                    Assert.AreNotEqual<string>(roleNew, findTask.Result.Name, "Name not updated.");
                }
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.RoleStore")]
        public void DeleteRole()
        {
            using (RoleStore<IdentityRole> store = new RoleStore<IdentityRole>())
            {
                using (RoleManager<IdentityRole> manager = new RoleManager<IdentityRole>(store))
                {
                    string roleNew = string.Format("TestRole_{0}", Guid.NewGuid());
                    var role = new IdentityRole(roleNew);
                    var createTask = manager.CreateAsync(role);
                    createTask.Wait();

                    var delTask = manager.DeleteAsync(role);
                    delTask.Wait();

                    var findTask = manager.FindByIdAsync(role.Id);
                    Assert.IsNull(findTask.Result, "Role not deleted ");

                    try
                    {
                        var task = store.DeleteAsync(null);
                        task.Wait();
                    }
                    catch (Exception ex)
                    {
                        Assert.IsNotNull(ex, "Argument exception not raised");
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.RoleStore")]
        public void RolesGetter()
        {
            using (RoleStore<IdentityRole> store = new RoleStore<IdentityRole>())
            {
                string roleNew = string.Format("TestRole_{0}", Guid.NewGuid());
                IdentityRole role;

                using (RoleManager<IdentityRole> manager = new RoleManager<IdentityRole>(store))
                {
                    role = new IdentityRole(roleNew);
                    var createTask = manager.CreateAsync(role);
                    createTask.Wait();

                    var findTask = manager.FindByIdAsync(role.Id);
                    Assert.IsNotNull(findTask.Result, "Role wasn't created");

                    var roles = store.Roles;
                    Assert.IsNotNull(roles);

                    var foundRole = from r in roles where r.Name == roleNew select r;
                    Assert.IsNotNull(foundRole.FirstOrDefault());

                    try
                    {
                        var task = store.DeleteAsync(null);
                        task.Wait();
                    }
                    catch (Exception ex)
                    {
                        Assert.IsNotNull(ex, "Argument exception not raised");
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.RoleStore")]
        public void FindRoleById()
        {
            using (RoleStore<IdentityRole> store = new RoleStore<IdentityRole>())
            {
                using (RoleManager<IdentityRole> manager = new RoleManager<IdentityRole>(store))
                {
                    var findTask = manager.FindByIdAsync(CurrentRole.Id);
                    Assert.IsNotNull(findTask.Result, "Find Role Result is null");
                    Assert.AreEqual<string>(CurrentRole.Id, findTask.Result.Id, "RowKeys don't match.");
                }
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.RoleStore")]
        public void FindRoleByName()
        {
            using (RoleStore<IdentityRole> store = new RoleStore<IdentityRole>())
            {
                using (RoleManager<IdentityRole> manager = new RoleManager<IdentityRole>(store))
                {

                    var findTask = manager.FindByNameAsync(CurrentRole.Name);
                    Assert.IsNotNull(findTask.Result, "Find Role Result is null");
                    Assert.AreEqual<string>(CurrentRole.Name, findTask.Result.Name, "Role names don't match.");
                }
            }
        }
        */

    }
}
