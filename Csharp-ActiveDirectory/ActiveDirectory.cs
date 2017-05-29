using System;
using System.DirectoryServices.AccountManagement;

namespace Csharp_ActiveDirectory
{
    public class ActiveDirectory
    {
        private PrincipalContext context;

        public PrincipalContext Context { get => context; set => context = value; }

        #region Constructors
        public ActiveDirectory(string username, string password)
        {
            try
            {
                this.Context = new PrincipalContext(ContextType.Domain, null, username, password);
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message);
            }
        }

        public ActiveDirectory(string url, string username, string password)
        {
            try
            {
                this.Context = new PrincipalContext(ContextType.Domain, url, username, password);
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message);
            }
        }
        #endregion

        public bool IsUserExisting(string username)
        {
            if (GetUser(username) == null)
                return false;
            return true;
        }

        public bool IsGroupExisting(string groupName)
        {
            if (GetGroup(groupName) == null)
                return false;
            return true;
        }

        public UserPrincipal GetUser(string username)
        {
            return UserPrincipal.FindByIdentity(this.Context, username);
        }

        public GroupPrincipal GetGroup(string groupName)
        {
            return GroupPrincipal.FindByIdentity(this.Context, groupName);
        }

        public UserPrincipal CreateUser(string username, string password, string givenName, string surname, string id)
        {
            if (!IsUserExisting(username))
            {
                UserPrincipal user = new UserPrincipal(context, username, password, true);

                user.UserPrincipalName = username;
                user.EmployeeId = id;
                user.GivenName = givenName;
                user.Surname = surname;
                user.HomeDirectory = @"C:\Users\" + username;
                user.Enabled = true;
                user.Save();

                return user;
            }
            else
            {
                return GetUser(username);
            }
        }

        public bool DeleteUser(string username)
        {
            try
            {
                UserPrincipal user = GetUser(username);

                user.Delete();
                return true;
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message);
                return false;
            }
        }

        public GroupPrincipal CreateNewGroup(string groupName, string description, bool isSecurityGroup)
        {
            try
            {
                GroupPrincipal group = new GroupPrincipal(this.Context, groupName);
                group.Description = description;
                group.IsSecurityGroup = isSecurityGroup;
                group.Save();

                return group;
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message);
                return null;
            }
        }

        public bool AddUserToGroup(string username, string groupName)
        {
            var user = GetUser(username);
            var group = GetGroup(groupName);

            if (user == null || group == null)
                return false;

            if (IsUserGroupMember(username, groupName))
            {
                throw new Exception("L'utilisateur est déjà dans le groupe.");
            }
            else
            {
                group.Members.Add(user);
                group.Save();
                return true;
            }
        }

        public bool IsUserGroupMember(string username, string groupName)
        {
            var user = GetUser(username);
            var group = GetGroup(groupName);

            if (user == null || group == null)
                return false;
            Console.WriteLine(group.ToString());
            return group.Members.Contains(user);
        }
    }
}
