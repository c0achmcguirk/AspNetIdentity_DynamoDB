// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.

namespace ElCamino.AspNetCore.Identity.Dynamo.IntegrationTests
{
    public partial class LegacyUserStoreTests
    {

        /*
        [TestMethod]
        [TestCategory("Identity.Dynamo.UserStore.Properties")]
        public void AccessFailedCount()
        {
            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    manager.MaxFailedAccessAttemptsBeforeLockout = 2;

                    var user = CreateTestUser();
                    var taskUser = manager.GetAccessFailedCountAsync(user.Id);
                    taskUser.Wait();
                    Assert.AreEqual<int>(user.AccessFailedCount, taskUser.Result, "AccessFailedCount not equal");

                    var taskAccessFailed = manager.AccessFailedAsync(user.Id);
                    taskAccessFailed.Wait();
                    Assert.IsTrue(taskAccessFailed.Result.Succeeded, string.Concat(taskAccessFailed.Result.Errors));

                    user = manager.FindById(user.Id);
                    var taskAccessReset = manager.ResetAccessFailedCountAsync(user.Id);
                    taskAccessReset.Wait();
                    Assert.IsTrue(taskAccessReset.Result.Succeeded, string.Concat(taskAccessReset.Result.Errors));

                    try
                    {
                        var task = store.GetAccessFailedCountAsync(null);
                        task.Wait();
                    }
                    catch (Exception ex)
                    {
                        Assert.IsNotNull(ex, "Argument exception not raised");
                    }

                    try
                    {
                        var task = store.IncrementAccessFailedCountAsync(null);
                        task.Wait();
                    }
                    catch (Exception ex)
                    {
                        Assert.IsNotNull(ex, "Argument exception not raised");
                    }

                    try
                    {
                        var task = store.ResetAccessFailedCountAsync(null);
                        task.Wait();
                    }
                    catch (Exception ex)
                    {
                        Assert.IsNotNull(ex, "Argument exception not raised");
                    }

                }
            }
        }

        private void SetValidateEmail(UserManager<IdentityUser> manager,
            UserStore<IdentityUser> store,
            IdentityUser user,
            string strNewEmail)
        {
            string originalEmail = user.Email;
            var taskUserSet = manager.SetEmailAsync(user.Id, strNewEmail);
            taskUserSet.Wait();
            Assert.IsTrue(taskUserSet.Result.Succeeded, string.Concat(taskUserSet.Result.Errors));

            var taskUser = manager.GetEmailAsync(user.Id);
            taskUser.Wait();
            if (!string.IsNullOrWhiteSpace(strNewEmail))
            {
                Assert.AreEqual<string>(strNewEmail, taskUser.Result, "GetEmailAsync: Email not equal");
            }
            else
            {
                Assert.IsNull(taskUser.Result, "GetEmailAsync: Email not null");
            }
            if (!string.IsNullOrWhiteSpace(strNewEmail))
            {
                TestContext.WriteLine("New email: {0}", strNewEmail);
                var taskFind = manager.FindByEmailAsync(strNewEmail);
                taskFind.Wait();
                WriteLineObject<IdentityUser>(taskFind.Result);
                Assert.AreEqual<string>(strNewEmail, taskFind.Result.Email, "FindByEmailAsync: Email not equal");
            }
            
            //Should not find old by old email.
            if (!string.IsNullOrWhiteSpace(originalEmail))
            {
                TestContext.WriteLine("Original email: {0}", originalEmail);
                var taskFind = manager.FindByEmailAsync(originalEmail);
                taskFind.Wait();
                if (taskFind.Result != null)
                {
                    WriteLineObject<IdentityUser>(taskFind.Result);
                }
                Assert.IsNull(taskFind.Result, "FindByEmailAsync: Old email should not yield a find result.");
            }

        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.UserStore.Properties")]
        public void EmailNone()
        {
            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    var user = CreateTestUser(false, false);
                    string strNewEmail = string.Format("{0}@hotmail.com", Guid.NewGuid().ToString("N"));
                    SetValidateEmail(manager, store, user, strNewEmail);

                    SetValidateEmail(manager, store, user, string.Empty);

                }
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.UserStore.Properties")]
        public void Email()
        {
            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    var user = CreateTestUser(true, true, string.Format("{0}@mydomain.com", Guid.NewGuid().ToString("N")));

                    string strNewEmail = string.Format("{0}@gmail.com", Guid.NewGuid().ToString("N"));
                    SetValidateEmail(manager, store, user, strNewEmail);

                    try
                    {
                        var task = store.GetEmailAsync(null);
                        task.Wait();
                    }
                    catch (Exception ex)
                    {
                        Assert.IsNotNull(ex, "Argument exception not raised");
                    }

                    try
                    {
                        var task = store.SetEmailAsync(null, strNewEmail);
                        task.Wait();
                    }
                    catch (Exception ex)
                    {
                        Assert.IsNotNull(ex, "Argument exception not raised");
                    }

                    try
                    {
                        var task = store.SetEmailAsync(user, null);
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
        [TestCategory("Identity.Dynamo.UserStore.Properties")]
        public void EmailConfirmed()
        {
            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    manager.UserTokenProvider = new EmailTokenProvider<IdentityUser>();
                    var user = CreateTestUser();

                    var taskUserSet = manager.GenerateEmailConfirmationTokenAsync(user.Id);
                    taskUserSet.Wait();
                    Assert.IsFalse(string.IsNullOrWhiteSpace(taskUserSet.Result), "GenerateEmailConfirmationToken failed.");
                    string token = taskUserSet.Result;

                    var taskConfirm = manager.ConfirmEmailAsync(user.Id, token);
                    taskConfirm.Wait();
                    Assert.IsTrue(taskConfirm.Result.Succeeded, string.Concat(taskConfirm.Result.Errors));

                    user = manager.FindByEmail(user.Email);
                    var taskConfirmGet = store.GetEmailConfirmedAsync(user);
                    taskConfirmGet.Wait();
                    Assert.IsTrue(taskConfirmGet.Result, "Email not confirmed");

                    try
                    {
                        var task = store.SetEmailConfirmedAsync(null, true);
                        task.Wait();
                    }
                    catch (Exception ex)
                    {
                        Assert.IsNotNull(ex, "Argument exception not raised");
                    }

                    try
                    {
                        var task = store.GetEmailConfirmedAsync(null);
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
        [TestCategory("Identity.Dynamo.UserStore.Properties")]
        public void LockoutEnabled()
        {
            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    manager.UserTokenProvider = new EmailTokenProvider<IdentityUser>();

                    var user = User;

                    var taskLockoutSet = manager.SetLockoutEnabledAsync(user.Id, true);
                    taskLockoutSet.Wait();
                    Assert.IsTrue(taskLockoutSet.Result.Succeeded, string.Concat(taskLockoutSet.Result.Errors));

                    DateTimeOffset offSet = new DateTimeOffset(DateTime.UtcNow.AddMinutes(3));
                    var taskDateSet = manager.SetLockoutEndDateAsync(user.Id, offSet);
                    taskDateSet.Wait();
                    Assert.IsTrue(taskDateSet.Result.Succeeded, string.Concat(taskDateSet.Result.Errors));

                    var taskEnabledGet = manager.GetLockoutEnabledAsync(user.Id);
                    taskEnabledGet.Wait();
                    Assert.IsTrue(taskEnabledGet.Result, "Lockout not true");

                    var taskDateGet = manager.GetLockoutEndDateAsync(user.Id);
                    taskDateGet.Wait();
                    //The DateTimeOffSet is off by ~ 2ticks
                    Assert.AreEqual(offSet.Date.ToShortDateString(), taskDateGet.Result.Date.ToShortDateString(), "Lockout date incorrect");

                    DateTime tmpDate = DateTime.UtcNow.AddDays(1);
                    user.LockoutEndDateUtc = tmpDate;
                    var taskGet = store.GetLockoutEndDateAsync(user);
                    taskGet.Wait();
                    Assert.AreEqual<DateTimeOffset>(new DateTimeOffset(tmpDate), taskGet.Result, "LockoutEndDate not set");

                    user.LockoutEndDateUtc = null;
                    var taskGet2 = store.GetLockoutEndDateAsync(user);
                    taskGet2.Wait();
                    Assert.AreEqual<DateTimeOffset>(new DateTimeOffset(), taskGet2.Result, "LockoutEndDate not set");

                    var minOffSet = DateTimeOffset.MinValue;
                    var taskSet2 = store.SetLockoutEndDateAsync(user, minOffSet);
                    taskSet2.Wait();
                    Assert.IsNull(user.LockoutEndDateUtc, "LockoutEndDate not null");


                    try
                    {
                        store.GetLockoutEnabledAsync(null);
                    }
                    catch (ArgumentException) { }


                    try
                    {
                        store.GetLockoutEndDateAsync(null);
                    }
                    catch (ArgumentException) { }

                    try
                    {
                        store.SetLockoutEndDateAsync(null, offSet);
                    }
                    catch (ArgumentException) { }

                    try
                    {
                        store.SetLockoutEnabledAsync(null, false);
                    }
                    catch (ArgumentException) { }
                }
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.UserStore.Properties")]
        public void PhoneNumber()
        {
            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    var user = User;

                    string strNewPhoneNumber = "542-887-3434";
                    var taskPhoneNumberSet = manager.SetPhoneNumberAsync(user.Id, strNewPhoneNumber);
                    taskPhoneNumberSet.Wait();
                    Assert.IsTrue(taskPhoneNumberSet.Result.Succeeded, string.Concat(taskPhoneNumberSet.Result.Errors));

                    var taskUser = manager.GetPhoneNumberAsync(user.Id);
                    taskUser.Wait();
                    Assert.AreEqual<string>(strNewPhoneNumber, taskUser.Result, "PhoneNumber not equal");

                    try
                    {
                        var task = store.GetPhoneNumberAsync(null);
                        task.Wait();
                    }
                    catch (Exception ex)
                    {
                        Assert.IsNotNull(ex, "Argument exception not raised");
                    }

                    try
                    {
                        var task = store.SetPhoneNumberAsync(null, strNewPhoneNumber);
                        task.Wait();
                    }
                    catch (Exception ex)
                    {
                        Assert.IsNotNull(ex, "Argument exception not raised");
                    }

                    try
                    {
                        var task = store.SetPhoneNumberAsync(user, null);
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
        [TestCategory("Identity.Dynamo.UserStore.Properties")]
        public void PhoneNumberConfirmed()
        {
            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    manager.UserTokenProvider = new PhoneNumberTokenProvider<IdentityUser>();
                    var user = CreateTestUser();
                    string strNewPhoneNumber = "425-555-1111";
                    var taskUserSet = manager.GenerateChangePhoneNumberTokenAsync(user.Id, strNewPhoneNumber);
                    taskUserSet.Wait();
                    Assert.IsFalse(string.IsNullOrWhiteSpace(taskUserSet.Result), "GeneratePhoneConfirmationToken failed.");
                    string token = taskUserSet.Result;

                    var taskConfirm = manager.ChangePhoneNumberAsync(user.Id, strNewPhoneNumber, token);
                    taskConfirm.Wait();
                    Assert.IsTrue(taskConfirm.Result.Succeeded, string.Concat(taskConfirm.Result.Errors));

                    user = manager.FindByEmail(user.Email);
                    var taskConfirmGet = store.GetPhoneNumberConfirmedAsync(user);
                    taskConfirmGet.Wait();
                    Assert.IsTrue(taskConfirmGet.Result, "Phone not confirmed");

                    try
                    {
                        var task = store.SetPhoneNumberConfirmedAsync(null, true);
                        task.Wait();
                    }
                    catch (Exception ex)
                    {
                        Assert.IsNotNull(ex, "Argument exception not raised");
                    }

                    try
                    {
                        var task = store.GetPhoneNumberConfirmedAsync(null);
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
        [TestCategory("Identity.Dynamo.UserStore.Properties")]
        public void TwoFactorEnabled()
        {
            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    var user = User;

                    bool twoFactorEnabled = true;
                    var taskTwoFactorEnabledSet = manager.SetTwoFactorEnabledAsync(user.Id, twoFactorEnabled);
                    taskTwoFactorEnabledSet.Wait();
                    Assert.IsTrue(taskTwoFactorEnabledSet.Result.Succeeded, string.Concat(taskTwoFactorEnabledSet.Result.Errors));

                    var taskUser = manager.GetTwoFactorEnabledAsync(user.Id);
                    taskUser.Wait();
                    Assert.AreEqual<bool>(twoFactorEnabled, taskUser.Result, "TwoFactorEnabled not true");

                    try
                    {
                        var task = store.GetTwoFactorEnabledAsync(null);
                        task.Wait();
                    }
                    catch (Exception ex)
                    {
                        Assert.IsNotNull(ex, "Argument exception not raised");
                    }

                    try
                    {
                        var task = store.SetTwoFactorEnabledAsync(null, twoFactorEnabled);
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
        [TestCategory("Identity.Dynamo.UserStore.Properties")]
        public void PasswordHash()
        {
            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    var user = User;
                    string passwordPlain = Guid.NewGuid().ToString("N");
                    string passwordHash = manager.PasswordHasher.HashPassword(passwordPlain);

                    var taskUserSet = store.SetPasswordHashAsync(user, passwordHash);
                    taskUserSet.Wait();

                    var taskHasHash = manager.HasPasswordAsync(user.Id);
                    taskHasHash.Wait();
                    Assert.IsTrue(taskHasHash.Result, "PasswordHash not set");

                    var taskUser = store.GetPasswordHashAsync(user);
                    taskUser.Wait();
                    Assert.AreEqual<string>(passwordHash, taskUser.Result, "PasswordHash not equal");
                    user.PasswordHash = passwordHash;
                    try
                    {
                        var task = store.GetPasswordHashAsync(null);
                        task.Wait();
                    }
                    catch (Exception ex)
                    {
                        Assert.IsNotNull(ex, "Argument exception not raised");
                    }

                    try
                    {
                        var task = store.HasPasswordAsync(null);
                        task.Wait();
                    }
                    catch (Exception ex)
                    {
                        Assert.IsNotNull(ex, "Argument exception not raised");
                    }

                    try
                    {
                        var task = store.SetPasswordHashAsync(null, passwordHash);
                        task.Wait();
                    }
                    catch (Exception ex)
                    {
                        Assert.IsNotNull(ex, "Argument exception not raised");
                    }

                    try
                    {
                        var task = store.SetPasswordHashAsync(user, null);
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
        [TestCategory("Identity.Dynamo.UserStore.Properties")]
        public void UsersProperty()
        {
            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
            {
                Assert.IsNull(store.Users, "Users Property is not null");
            }
        }

        [TestMethod]
        [TestCategory("Identity.Dynamo.UserStore.Properties")]
        public void SecurityStamp()
        {
            using (UserStore<IdentityUser> store = new UserStore<IdentityUser>())
            {
                using (UserManager<IdentityUser> manager = new UserManager<IdentityUser>(store))
                {
                    var user = CreateTestUser();

                    var taskUser = manager.GetSecurityStampAsync(user.Id);
                    taskUser.Wait();
                    Assert.AreEqual<string>(user.SecurityStamp, taskUser.Result, "SecurityStamp not equal");

                    string strNewSecurityStamp = Guid.NewGuid().ToString("N");
                    var taskUserSet = store.SetSecurityStampAsync(user, strNewSecurityStamp);
                    taskUserSet.Wait();

                    try
                    {
                        var task = store.GetSecurityStampAsync(null);
                        task.Wait();
                    }
                    catch (Exception ex)
                    {
                        Assert.IsNotNull(ex, "Argument exception not raised");
                    }

                    try
                    {
                        var task = store.SetSecurityStampAsync(null, strNewSecurityStamp);
                        task.Wait();
                    }
                    catch (Exception ex)
                    {
                        Assert.IsNotNull(ex, "Argument exception not raised");
                    }

                    try
                    {
                        var task = store.SetSecurityStampAsync(user, null);
                        task.Wait();
                    }
                    catch (Exception ex)
                    {
                        Assert.IsNotNull(ex, "Argument exception not raised");
                    }
                }
            }
        }
        */
    }
}
