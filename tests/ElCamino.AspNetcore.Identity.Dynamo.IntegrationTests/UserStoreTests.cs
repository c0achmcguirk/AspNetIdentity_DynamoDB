// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace ElCamino.AspNetCore.Identity.Dynamo.IntegrationTests
{
    [TestClass]
    public partial class UserStoreTests
    {
        #region Static and Const Members
        public static string DefaultUserPassword;
        //private static IdentityUser User = null;
        private static bool tablesCreated = false;
        private static List<string> NoCreateUserTests =
            new List<string>() { 
                "AddRemoveUserLogin",
                "AddUserLogin",
                "ChangeUserName",
                "CreateUser",
                "DeleteUser",
                "ThrowIfDisposed",
                "UpdateApplicationUser",
                "UpdateUser",
                "UserStoreCtors",
                "AccessFailedCount",
                "EmailConfirmed",
                "EmailNone",
                "PhoneNumberConfirmed",
                "SecurityStamp",
                "UsersProperty",
                "FindUsersByEmail",
                "Email"
                };

        #endregion

        private TestContext testContextInstance;
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }


        /*
        #region Test Initialization
        [TestInitialize]
        public void Initialize()
        {
            DefaultUserPassword = Guid.NewGuid().ToString();

            //--Changes to speed up tests that don't require a new user, sharing a static user
            //--Also limiting table creation to once per test run
            if (!tablesCreated)
            {
                using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
                {
                    var taskCreateTables = store.CreateTablesIfNotExists();
                    taskCreateTables.Wait();
                }

                tablesCreated = true;
            }

            if (User == null &&
                !NoCreateUserTests.Any(t => t == TestContext.TestName))
            {
                //CreateUser();
            }
            //--
        }
        #endregion
        */

        private void WriteLineObject<t>(t obj) where t : class
        {
            TestContext.WriteLine(typeof(t).Name);
            string strLine = obj == null ? "Null" : Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            TestContext.WriteLine("{0}", strLine);
        }

        private Claim GenAdminClaim()
        {
            return new Claim(Constants.AccountClaimTypes.AccountTestAdminClaim, Guid.NewGuid().ToString());
        }

        private Claim GenAdminClaimEmptyValue()
        {
            return new Claim(Constants.AccountClaimTypes.AccountTestAdminClaim, string.Empty);
        }

        private Claim GenUserClaim()
        {
            return new Claim(Constants.AccountClaimTypes.AccountTestUserClaim, Guid.NewGuid().ToString());
        }

        /*
        private UserLoginInfo GenGoogleLogin()
        {
            return new UserLoginInfo(Constants.LoginProviders.GoogleProvider.LoginProvider,
                         Constants.LoginProviders.GoogleProvider.ProviderKey);
        }
        */

            /*
        private IdentityUser GenTestUser()
        {
            Guid id = Guid.NewGuid();
            IdentityUser user = new IdentityUser()
            {
                Email = id.ToString() + "@live.com",
                UserName = id.ToString("N"),
                LockoutEnabled = false,
                LockoutEndDateUtc = null,
                PhoneNumber = "555-555-5555",
                TwoFactorEnabled = false,
            };

            return user;
        }
        */

            /*
        [TestMethod]
        [TestCategory("Identity.Dynamo.UserStore")]
        public void UserStoreCtors()
        {
            try
            {
                new UserStore<IdentityUser>(null);
            }
            catch (ArgumentException) { }
        }
        */


        /*
        [TestMethod]
        [TestCategory("Identity.Dynamo.UserStore")]
        public void CreateUser()
        {
            User = CreateTestUser();
            WriteLineObject<IdentityUser>(User);
        }

        private IdentityUser CreateTestUser(bool createPassword = true, bool createEmail = true
            , string emailAddress = null)
        {
            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>(new IdentityCloudContext()))
            {
                var options = new IdentityOptions();
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store, options, )) //new UserManager<IdentityUser>(store))
                {
                    var user = GenTestUser();
                    if (!createEmail)
                    {
                        user.Email = null;
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(emailAddress))
                        {
                            user.Email = emailAddress;
                        }
                    }
                    var taskUser = createPassword ?
                        manager.CreateAsync(user, DefaultUserPassword) :
                        manager.CreateAsync(user);
                    taskUser.Wait();
                    Assert.IsTrue(taskUser.Result.Succeeded, string.Concat(taskUser.Result.Errors));

                    for (int i = 0; i < 5; i++)
                    {
                        AddUserClaimHelper(user, GenAdminClaim());
                        AddUserLoginHelper(user, GenGoogleLogin());
                        AddUserRoleHelper(user, string.Format("{0}_{1}", Constants.AccountRoles.AccountTestUserRole, Guid.NewGuid().ToString("N")));
                    }

                    try
                    {
                        var task = store.CreateAsync(null);
                        task.Wait();
                    }
                    catch (AggregateException agg)
                    {
                        agg.ValidateAggregateException<ArgumentException>();
                    }

                    var getUserTask = manager.FindByIdAsync(user.Id);
                    getUserTask.Wait();
                    return getUserTask.Result;
                }
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.UserStore")]
        public void DeleteUser()
        {
            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    var user = GenTestUser();

                    var taskUser = manager.CreateAsync(user, DefaultUserPassword);
                    taskUser.Wait();
                    Assert.IsTrue(taskUser.Result.Succeeded, string.Concat(taskUser.Result.Errors));


                    for (int i = 0; i < 10; i++)
                    {
                        AddUserClaimHelper(user, GenAdminClaim());
                        AddUserLoginHelper(user, GenGoogleLogin());
                        AddUserRoleHelper(user, string.Format("{0}_{1}", Constants.AccountRoles.AccountTestUserRole, Guid.NewGuid().ToString("N")));
                    }

                    var findUserTask2 = manager.FindByIdAsync(user.Id);
                    findUserTask2.Wait();
                    user = findUserTask2.Result;
                    WriteLineObject<IdentityUser>(user);


                    DateTime start = DateTime.UtcNow;
                    var taskUserDel = manager.DeleteAsync(user);
                    taskUserDel.Wait();
                    Assert.IsTrue(taskUserDel.Result.Succeeded, string.Concat(taskUser.Result.Errors));
                    TestContext.WriteLine("DeleteAsync: {0} seconds", (DateTime.UtcNow - start).TotalSeconds);

                    Thread.Sleep(1000);

                    var findUserTask = manager.FindByIdAsync(user.Id);
                    findUserTask.Wait();
                    Assert.IsNull(findUserTask.Result, "Found user Id, user not deleted.");

                    var batchGet = store.Context.CreateBatchGet<IdentityUserIndex>(
                        new Amazon.DynamoDBv2.DataModel.DynamoDBOperationConfig()
                        {
                            TableNamePrefix = store.Context.TablePrefix
                        });
                    batchGet.ConsistentRead = true;
                    user.Logins.ToList().ForEach(l => batchGet.AddKey(l.Id));
                    var batchGetTask = batchGet.ExecuteAsync();
                    batchGetTask.Wait();
                    Assert.IsFalse(batchGet.Results.Any(), "Login index records not deleted.");

                    try
                    {
                        var task = store.DeleteAsync(null);
                        task.Wait();
                    }
                    catch (AggregateException agg)
                    {
                        agg.ValidateAggregateException<ArgumentException>();
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.UserStore")]
        public void UpdateUser()
        {
            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    var user = GenTestUser();
                    WriteLineObject<IdentityUser>(user);
                    var taskUser = manager.CreateAsync(user, DefaultUserPassword);
                    taskUser.Wait();
                    Assert.IsTrue(taskUser.Result.Succeeded, string.Concat(taskUser.Result.Errors));

                    var taskUserUpdate = manager.UpdateAsync(user);
                    taskUserUpdate.Wait();
                    Assert.IsTrue(taskUserUpdate.Result.Succeeded, string.Concat(taskUserUpdate.Result.Errors));

                    try
                    {
                        store.UpdateAsync(null);
                    }
                    catch (ArgumentException) { }
                }
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.UserStore")]
        public void ChangeUserName()
        {
            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    var firstUser = CreateTestUser();
                    TestContext.WriteLine("{0}", "Original User");
                    WriteLineObject<IdentityUser>(firstUser);
                    string originalPlainUserName = firstUser.UserName;
                    string originalUserId = firstUser.Id;
                    string userNameChange = Guid.NewGuid().ToString("N");
                    firstUser.UserName = userNameChange;

                    DateTime start = DateTime.UtcNow;
                    var taskUserUpdate = manager.UpdateAsync(firstUser);
                    taskUserUpdate.Wait();
                    TestContext.WriteLine("UpdateAsync(ChangeUserName): {0} seconds", (DateTime.UtcNow - start).TotalSeconds);

                    Assert.IsTrue(taskUserUpdate.Result.Succeeded, string.Concat(taskUserUpdate.Result.Errors));

                    var taskUserChanged = manager.FindByNameAsync(userNameChange);
                    taskUserChanged.Wait();
                    var changedUser = taskUserChanged.Result;

                    TestContext.WriteLine("{0}", "Changed User");
                    WriteLineObject<IdentityUser>(changedUser);

                    Assert.IsNotNull(changedUser, "User not found by new username.");
                    Assert.IsFalse(originalPlainUserName.Equals(changedUser.UserName, StringComparison.OrdinalIgnoreCase), "UserName property not updated.");

                    Assert.AreEqual<int>(firstUser.Roles.Count, changedUser.Roles.Count, "Roles count are not equal");
                    Assert.IsTrue(changedUser.Roles.All(r => r.UserId == changedUser.Id.ToString()), "Roles partition keys are not equal to the new user id");

                    Assert.AreEqual<int>(firstUser.Claims.Count, changedUser.Claims.Count, "Claims count are not equal");
                    Assert.IsTrue(changedUser.Claims.All(r => r.UserId == changedUser.Id.ToString()), "Claims partition keys are not equal to the new user id");

                    Assert.AreEqual<int>(firstUser.Logins.Count, changedUser.Logins.Count, "Logins count are not equal");
                    Assert.IsTrue(changedUser.Logins.All(r => r.UserId == changedUser.Id.ToString()), "Logins partition keys are not equal to the new user id");

                    Assert.AreEqual<string>(originalUserId, changedUser.Id, "User Ids are not the same.");
                    Assert.AreNotEqual<string>(originalPlainUserName, changedUser.UserName, "UserNames are the same.");

                    //Check email
                    var taskFindEmail = manager.FindByEmailAsync(changedUser.Email);
                    taskFindEmail.Wait();
                    Assert.IsNotNull(taskFindEmail.Result, "User not found by new email.");

                    //Check logins
                    foreach (var log in taskFindEmail.Result.Logins)
                    {
                        var taskFindLogin = manager.FindAsync(new UserLoginInfo(log.LoginProvider, log.ProviderKey));
                        taskFindLogin.Wait();
                        Assert.IsNotNull(taskFindLogin.Result, "User not found by login.");
                        Assert.AreNotEqual<string>(originalPlainUserName, taskFindLogin.Result.UserName, "Login user id not changed");
                    }

                    try
                    {
                        store.UpdateAsync(null);
                    }
                    catch (ArgumentException) { }
                }
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.UserStore")]
        public void FindUserByEmail()
        {
            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    var user = User;
                    WriteLineObject<IdentityUser>(user);

                    DateTime start = DateTime.UtcNow;
                    var findUserTask = manager.FindByEmailAsync(user.Email);
                    findUserTask.Wait();
                    TestContext.WriteLine("FindByEmailAsync: {0} seconds", (DateTime.UtcNow - start).TotalSeconds);

                    Assert.AreEqual<string>(user.Email, findUserTask.Result.Email, "Found user email not equal");

                    CreateTestUser(true, true, user.Email);

                    start = DateTime.UtcNow;
                    findUserTask = manager.FindByEmailAsync(user.Email);
                    findUserTask.Wait();
                    TestContext.WriteLine("FindByEmailAsync: {0} seconds", (DateTime.UtcNow - start).TotalSeconds);
                    Assert.IsNotNull(findUserTask.Result, "User should be not null.");

                    //Negative cases
                    string bogusEmail = "234567@aldskjfalsdfj43344.com";
                    var findUserTask2 = manager.FindByEmailAsync(bogusEmail);
                    findUserTask2.Wait();
                    Assert.IsNull(findUserTask2.Result, "User should be null.");

                    try
                    {
                        store.FindByEmailAsync(null);
                    }
                    catch (ArgumentException) { }

                }
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.UserStore")]
        public void FindUsersByEmail()
        {
            string strEmail = Guid.NewGuid().ToString() + "@live.com";

            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    List<IdentityUser> listCreated = new List<IdentityUser>();
                    for (int i = 0; i < 11; i++)
                    {
                        listCreated.Add(CreateTestUser(true, true, strEmail));
                    }

                    DateTime start = DateTime.UtcNow;
                    TestContext.WriteLine("FindAllByEmailAsync: {0}", strEmail);

                    var findUserTask = store.FindAllByEmailAsync(strEmail);
                    findUserTask.Wait();
                    TestContext.WriteLine("FindAllByEmailAsync: {0} seconds", (DateTime.UtcNow - start).TotalSeconds);
                    TestContext.WriteLine("Users Found: {0}", findUserTask.Result.Count());
                    Assert.AreEqual<int>(listCreated.Count, findUserTask.Result.Count(), "Found users by email not equal");

                    //Change email and check results
                    string strEmailChanged = Guid.NewGuid().ToString() + "@live.com";
                    var userToChange = listCreated.Last();
                    manager.SetEmailAsync(userToChange.Id, strEmailChanged).Wait();

                    var findUserChanged = manager.FindByEmailAsync(strEmailChanged);
                    findUserChanged.Wait();
                    Assert.AreEqual<string>(userToChange.Id, findUserChanged.Result.Id, "Found user by email not equal");
                    Assert.AreNotEqual<string>(userToChange.Email, findUserChanged.Result.Email, "Found user by email not changed");


                    //Make sure changed user doesn't show up in previous query
                    start = DateTime.UtcNow;

                    findUserTask = store.FindAllByEmailAsync(strEmail);
                    findUserTask.Wait();
                    TestContext.WriteLine("FindAllByEmailAsync: {0} seconds", (DateTime.UtcNow - start).TotalSeconds);
                    TestContext.WriteLine("Users Found: {0}", findUserTask.Result.Count());
                    Assert.AreEqual<int>(listCreated.Count - 1, findUserTask.Result.Count(), "Found users by email not equal");

                    //negative tests
                    try
                    {
                        store.FindAllByEmailAsync(string.Empty);
                    }
                    catch (ArgumentException) { }

                }
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.UserStore")]
        public void FindUserById()
        {
            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    var user = User;
                    DateTime start = DateTime.UtcNow;
                    var findUserTask = manager.FindByIdAsync(user.Id);
                    findUserTask.Wait();
                    TestContext.WriteLine("FindByIdAsync: {0} seconds", (DateTime.UtcNow - start).TotalSeconds);

                    Assert.AreEqual<string>(user.Id, findUserTask.Result.Id, "Found user Id not equal");

                    try
                    {
                        store.FindByIdAsync(null);
                    }
                    catch (ArgumentNullException) { }
                    try
                    {
                        store.FindByIdAsync(string.Empty);
                    }
                    catch (ArgumentException) { }
                }
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.UserStore")]
        public void FindUserByName()
        {
            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    var user = User;
                    WriteLineObject<IdentityUser>(user);
                    DateTime start = DateTime.UtcNow;
                    var findUserTask = manager.FindByNameAsync(user.UserName);
                    findUserTask.Wait();
                    TestContext.WriteLine("FindByNameAsync: {0} seconds", (DateTime.UtcNow - start).TotalSeconds);

                    Assert.AreEqual<string>(user.UserName, findUserTask.Result.UserName, "Found user UserName not equal");
                }
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.UserStore")]
        public void AddUserLogin()
        {
            var user = CreateTestUser(false);
            WriteLineObject<IdentityUser>(user);
            AddUserLoginHelper(user, GenGoogleLogin());
        }

        public void AddUserLoginHelper(IdentityUser user, UserLoginInfo loginInfo)
        {
            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    var userAddLoginTask = manager.AddLoginAsync(user.Id, loginInfo);
                    userAddLoginTask.Wait();
                    Assert.IsTrue(userAddLoginTask.Result.Succeeded, string.Concat(userAddLoginTask.Result.Errors));

                    var loginGetTask = manager.GetLoginsAsync(user.Id);
                    loginGetTask.Wait();
                    Assert.IsTrue(loginGetTask.Result
                        .Any(log => log.LoginProvider == loginInfo.LoginProvider
                            & log.ProviderKey == loginInfo.ProviderKey), "LoginInfo not found: GetLoginsAsync");

                    var loginGetTask2 = manager.FindAsync(loginGetTask.Result.First());
                    loginGetTask2.Wait();
                    Assert.IsNotNull(loginGetTask2.Result, "LoginInfo not found: FindAsync");

                }
            }
        }


        [TestMethod]
        [TestCategory("Identity.Dynamo.UserStore")]
        public void AddRemoveUserLogin()
        {
            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    var user = GenTestUser();
                    WriteLineObject<IdentityUser>(user);
                    var taskUser = manager.CreateAsync(user, DefaultUserPassword);
                    taskUser.Wait();
                    Assert.IsTrue(taskUser.Result.Succeeded, string.Concat(taskUser.Result.Errors));

                    var loginInfo = GenGoogleLogin();
                    var userAddLoginTask = manager.AddLoginAsync(user.Id, loginInfo);
                    userAddLoginTask.Wait();
                    Assert.IsTrue(userAddLoginTask.Result.Succeeded, string.Concat(userAddLoginTask.Result.Errors));

                    var loginGetTask = manager.GetLoginsAsync(user.Id);
                    loginGetTask.Wait();
                    Assert.IsTrue(loginGetTask.Result
                        .Any(log => log.LoginProvider == loginInfo.LoginProvider
                            & log.ProviderKey == loginInfo.ProviderKey), "LoginInfo not found: GetLoginsAsync");

                    var loginGetTask2 = manager.FindAsync(loginGetTask.Result.First());
                    loginGetTask2.Wait();
                    Assert.IsNotNull(loginGetTask2.Result, "LoginInfo not found: FindAsync");

                    var loginGetTaskNeg = manager.FindAsync(new UserLoginInfo(Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N")));
                    loginGetTaskNeg.Wait();
                    Assert.IsNull(loginGetTaskNeg.Result, "LoginInfo found: FindAsync");


                    var userRemoveLoginTaskNeg1 = manager.RemoveLoginAsync(user.Id, new UserLoginInfo(string.Empty, loginInfo.ProviderKey));
                    userRemoveLoginTaskNeg1.Wait();

                    var userRemoveLoginTaskNeg2 = manager.RemoveLoginAsync(user.Id, new UserLoginInfo(loginInfo.LoginProvider, string.Empty));
                    userRemoveLoginTaskNeg2.Wait();

                    var userRemoveLoginTask = manager.RemoveLoginAsync(user.Id, loginInfo);
                    userRemoveLoginTask.Wait();
                    Assert.IsTrue(userRemoveLoginTask.Result.Succeeded, string.Concat(userRemoveLoginTask.Result.Errors));
                    var loginGetTask3 = manager.GetLoginsAsync(user.Id);
                    loginGetTask3.Wait();
                    Assert.IsTrue(!loginGetTask3.Result.Any(), "LoginInfo not removed");

                    //Negative cases

                    var loginFindNeg = manager.FindAsync(new UserLoginInfo("asdfasdf", "http://4343443dfaksjfaf"));
                    loginFindNeg.Wait();
                    Assert.IsNull(loginFindNeg.Result, "LoginInfo found: FindAsync");

                    try
                    {
                        store.AddLoginAsync(null, loginInfo);
                    }
                    catch (ArgumentException) { }

                    try
                    {
                        store.AddLoginAsync(user, null);
                    }
                    catch (ArgumentException) { }

                    try
                    {
                        store.RemoveLoginAsync(null, loginInfo);
                    }
                    catch (ArgumentException) { }

                    try
                    {
                        store.RemoveLoginAsync(user, null);
                    }
                    catch (ArgumentException) { }

                    try
                    {
                        store.FindAsync(null);
                    }
                    catch (ArgumentNullException) { }

                    try
                    {
                        store.GetLoginsAsync(null);
                    }
                    catch (ArgumentException) { }
                }
            }
        }
        */

        /*
        [TestMethod]
        [TestCategory("Identity.Dynamo.UserStore")]
        public void AddUserRole()
        {
            string strUserRole = string.Format("{0}_{1}", Constants.AccountRoles.AccountTestUserRole, Guid.NewGuid().ToString("N"));
            WriteLineObject<IdentityUser>(User);
            AddUserRoleHelper(User, strUserRole);
        }

        public void AddUserRoleHelper(IdentityUser user, string roleName)
        {
            using (RoleStore<IdentityRole> rstore = new RoleStore<IdentityRole>())
            {
                var userRole = rstore.FindByNameAsync(roleName);
                userRole.Wait();

                if (userRole.Result == null)
                {
                    var taskUser = rstore.CreateAsync(new IdentityRole(roleName));
                    taskUser.Wait();
                }
            }

            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    var userRoleTask = manager.AddToRoleAsync(user.Id, roleName);
                    userRoleTask.Wait();
                    Assert.IsTrue(userRoleTask.Result.Succeeded, string.Concat(userRoleTask.Result.Errors));

                    var roles2Task = manager.IsInRoleAsync(user.Id, roleName);
                    roles2Task.Wait();
                    Assert.IsTrue(roles2Task.Result, "Role not found");

                }
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.UserStore")]
        public void AddRemoveUserRole()
        {
            string roleName = string.Format("{0}_{1}", Constants.AccountRoles.AccountTestAdminRole, Guid.NewGuid().ToString("N"));

            using (RoleStore<IdentityRole> rstore = new RoleStore<IdentityRole>())
            {
                var taskAdmin = rstore.CreateAsync(new IdentityRole(roleName));
                taskAdmin.Wait();
                var adminRole = rstore.FindByNameAsync(roleName);
                adminRole.Wait();
            }

            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    var user = User;
                    WriteLineObject<IdentityUser>(user);
                    var userRoleTask = manager.AddToRoleAsync(user.Id, roleName);
                    userRoleTask.Wait();
                    Assert.IsTrue(userRoleTask.Result.Succeeded, string.Concat(userRoleTask.Result.Errors));

                    var rolesTask = manager.GetRolesAsync(user.Id);
                    rolesTask.Wait();
                    Assert.IsTrue(rolesTask.Result.Contains(roleName), "Role not found");

                    var roles2Task = manager.IsInRoleAsync(user.Id, roleName);
                    roles2Task.Wait();
                    Assert.IsTrue(roles2Task.Result, "Role not found");

                    var userRemoveTask = manager.RemoveFromRoleAsync(user.Id, roleName);
                    userRemoveTask.Wait();
                    var rolesTask2 = manager.GetRolesAsync(user.Id);
                    rolesTask2.Wait();
                    Assert.IsFalse(rolesTask2.Result.Contains(roleName), "Role not removed.");

                    try
                    {
                        store.AddToRoleAsync(null, roleName);
                    }
                    catch (ArgumentException) { }

                    try
                    {
                        store.AddToRoleAsync(user, null);
                    }
                    catch (ArgumentException) { }

                    try
                    {
                        store.AddToRoleAsync(user, Guid.NewGuid().ToString());
                    }
                    catch (ArgumentException) { }

                    try
                    {
                        store.RemoveFromRoleAsync(null, roleName);
                    }
                    catch (ArgumentException) { }

                    try
                    {
                        store.RemoveFromRoleAsync(user, null);
                    }
                    catch (ArgumentException) { }

                    try
                    {
                        store.GetRolesAsync(null);
                    }
                    catch (ArgumentException) { }

                }
            }
        }
        */

            /*
        [TestMethod]
        [TestCategory("Identity.Dynamo.UserStore")]
        public void IsUserInRole()
        {
            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    var user = User;
                    WriteLineObject<IdentityUser>(user);
                    string roleName = string.Format("{0}_{1}", Constants.AccountRoles.AccountTestUserRole, Guid.NewGuid().ToString("N"));

                    AddUserRoleHelper(user, roleName);

                    var roles2Task = manager.IsInRoleAsync(user.Id, roleName);
                    roles2Task.Wait();
                    Assert.IsTrue(roles2Task.Result, "Role not found");


                    var rolesTsk3 = store.IsInRoleAsync(user, Guid.NewGuid().ToString());
                    rolesTsk3.Wait();
                    Assert.IsFalse(rolesTsk3.Result, "Role should not be found");

                    try
                    {
                        store.IsInRoleAsync(null, roleName);
                    }
                    catch (ArgumentException) { }

                    try
                    {
                        store.IsInRoleAsync(user, null);
                    }
                    catch (ArgumentException) { }
                }
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.UserStore")]
        public void AddUserClaim()
        {
            WriteLineObject<IdentityUser>(User);
            AddUserClaimHelper(User, GenUserClaim());
        }

        private void AddUserClaimHelper(IdentityUser user, Claim claim)
        {
            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>(new IdentityCloudContext<IdentityUser>()))
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    var userClaimTask = manager.AddClaimAsync(user.Id, claim);
                    userClaimTask.Wait();
                    Assert.IsTrue(userClaimTask.Result.Succeeded, string.Concat(userClaimTask.Result.Errors));
                    var claimsTask = manager.GetClaimsAsync(user.Id);
                    claimsTask.Wait();
                    Assert.IsTrue(claimsTask.Result.Any(c => c.Value == claim.Value & c.ValueType == claim.ValueType), "Claim not found");
                }
            }

        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.UserStore")]
        public void AddRemoveUserClaim()
        {
            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>(new IdentityCloudContext<IdentityUser>()))
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    var user = User;
                    WriteLineObject<IdentityUser>(user);
                    Claim claim = GenAdminClaim();
                    var userClaimTask = manager.AddClaimAsync(user.Id, claim);
                    userClaimTask.Wait();
                    Assert.IsTrue(userClaimTask.Result.Succeeded, string.Concat(userClaimTask.Result.Errors));
                    var claimsTask = manager.GetClaimsAsync(user.Id);
                    claimsTask.Wait();
                    Assert.IsTrue(claimsTask.Result.Any(c => c.Value == claim.Value & c.ValueType == claim.ValueType), "Claim not found");


                    var userRemoveClaimTask = manager.RemoveClaimAsync(user.Id, claim);
                    userRemoveClaimTask.Wait();
                    Assert.IsTrue(userClaimTask.Result.Succeeded, string.Concat(userClaimTask.Result.Errors));
                    var claimsTask2 = manager.GetClaimsAsync(user.Id);
                    claimsTask2.Wait();
                    Assert.IsTrue(!claimsTask2.Result.Any(c => c.Value == claim.Value & c.ValueType == claim.ValueType), "Claim not removed");

                    //adding test for removing an empty claim
                    Claim claimEmpty = GenAdminClaimEmptyValue();
                    var userClaimTask2 = manager.AddClaimAsync(user.Id, claimEmpty);
                    userClaimTask2.Wait();

                    var userRemoveClaimTask2 = manager.RemoveClaimAsync(user.Id, claimEmpty);
                    userRemoveClaimTask2.Wait();
                    Assert.IsTrue(userClaimTask2.Result.Succeeded, string.Concat(userClaimTask2.Result.Errors));

                    try
                    {
                        store.AddClaimAsync(null, claim);
                    }
                    catch (ArgumentException) { }

                    try
                    {
                        store.AddClaimAsync(user, null);
                    }
                    catch (ArgumentException) { }

                    try
                    {
                        store.RemoveClaimAsync(null, claim);
                    }
                    catch (ArgumentException) { }

                    try
                    {
                        store.RemoveClaimAsync(user, null);
                    }
                    catch (ArgumentException) { }

                    try
                    {
                        store.RemoveClaimAsync(user, new Claim(string.Empty, Guid.NewGuid().ToString()));
                    }
                    catch (ArgumentException) { }

                    try
                    {
                        store.RemoveClaimAsync(user, new Claim(claim.Type, null));
                    }
                    catch (ArgumentException) { }

                    try
                    {
                        store.GetClaimsAsync(null);
                    }
                    catch (ArgumentException) { }
                }
            }
        }
        */

        /*
        [TestMethod]
        [TestCategory("Identity.Dynamo.UserStore")]
        public void ThrowIfDisposed()
        {
            UserStore<IdentityUser> store = new UserStore<IdentityUser>();
            store.Dispose();
            GC.Collect();
            try
            {
                var task = store.DeleteAsync(null);
            }
            catch (AggregateException agg)
            {
                agg.ValidateAggregateException<ArgumentException>();
            }
        }
        */

    }
}
