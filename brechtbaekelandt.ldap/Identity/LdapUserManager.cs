﻿using brechtbaekelandt.ldap.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using brechtbaekelandt.ldap.Extensions;
using brechtbaekelandt.ldap.Services;

namespace brechtbaekelandt.ldap.Identity
{
    public class LdapUserManager : UserManager<LdapUser>
    {
        private readonly ILdapService _ldapService;

        public LdapUserManager(
            ILdapService ldapService,
            IUserStore<LdapUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<LdapUser> passwordHasher,
            IEnumerable<IUserValidator<LdapUser>> userValidators,
            IEnumerable<IPasswordValidator<LdapUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<LdapUserManager> logger)
            : base(
                store,
                optionsAccessor,
                passwordHasher,
                userValidators,
                passwordValidators,
                keyNormalizer,
                errors,
                services,
                logger)
        {
            this._ldapService = ldapService;
        }

        public LdapUser GetAdministrator()
        {
            return this._ldapService.GetAdministrator();
        }

        /// <summary>
        /// Checks the given password agains the configured LDAP server.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public override async Task<bool> CheckPasswordAsync(LdapUser user, string password)
        {
            return this._ldapService.Authenticate(user.DistinguishedName, password);
        }

        public override Task<bool> HasPasswordAsync(LdapUser user)
        {
            return Task.FromResult(true);
        }

        public override Task<LdapUser> FindByIdAsync(string userId)
        {
            return this.FindByNameAsync(userId);
        }

        public override Task<LdapUser> FindByNameAsync(string userName)
        {
            return Task.FromResult(this._ldapService.GetUserByUserName(userName));
        }
        
        public override async Task<IdentityResult> CreateAsync(LdapUser user, string password)
        {          
            try
            {
                this._ldapService.AddUser(user, password);
            }
            catch (Exception e)
            {
                return await Task.FromResult(IdentityResult.Failed(new IdentityError() { Code = "LdapUserCreateFailed", Description = e.Message ?? "The user could not be created." }));
            }

            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> DeleteUserAsync(string distinguishedName)
        {
            try
            {
                this._ldapService.DeleteUser(distinguishedName);
            }
            catch (Exception e)
            {
                return await Task.FromResult(IdentityResult.Failed(new IdentityError() { Code = "LdapUserDeleteFailed", Description = e.Message ?? "The user could not be deleted." }));
            }

            return await Task.FromResult(IdentityResult.Success);
        }
        
        public override Task<string> GetEmailAsync(LdapUser user)
        {
            return base.GetEmailAsync(user);
        }

        public override Task<string> GetUserIdAsync(LdapUser user)
        {
            return base.GetUserIdAsync(user);
        }

        public override Task<string> GetUserNameAsync(LdapUser user)
        {
            return base.GetUserNameAsync(user);
        }

        public override Task<string> GetPhoneNumberAsync(LdapUser user)
        {
            return base.GetPhoneNumberAsync(user);
        }

        public override IQueryable<LdapUser> Users => this._ldapService.GetAllUsers().AsQueryable();
    }
}
