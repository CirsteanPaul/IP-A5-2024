namespace IP.Project.LDAP;

using System;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using DirectoryEntry = System.DirectoryServices.DirectoryEntry;

public class LDAPManager
{
    static readonly string ldapServer = "LDAP://localhost:10389";
    static readonly string adminUserName = "uid=admin,ou=system";
    static readonly string adminPassword = "secret";
    
    public static void CreateUser(string username, string password)
    {
        var adminDirectoryEntry = new DirectoryEntry(ldapServer, adminUserName, adminPassword, AuthenticationTypes.ServerBind)
        {
            Path = ldapServer + "/ou=users,ou=system"
        };

        try
        {
            // Create a new user entry
            DirectoryEntry newUser = adminDirectoryEntry.Children.Add("cn=" + username, "inetOrgPerson");

            // Set user attributes
            newUser.Properties["sn"].Add("bobby");
            //newUser.Properties["cn"].Add("bob");
            newUser.Properties["userPassword"].Add(password); // Set a password for the user

            // Save the new user entry
            newUser.CommitChanges();
            Console.WriteLine("User added successfully.");

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public static bool VerifyUser(string username, string password)
    {
        try
        {
            var userDirPath = ldapServer + "/cn=" + username + ",ou=users,ou=system";
            var userDirectoryEntry = new DirectoryEntry(ldapServer, username, password, AuthenticationTypes.ServerBind);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }
}
