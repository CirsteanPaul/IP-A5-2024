namespace IP.Project.LDAP;

using System;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using DirectoryEntry = System.DirectoryServices.DirectoryEntry;

class LDAPInstance
{
    static readonly string[] groups = { "Student", "Professor", "Admin" };

    static readonly string ldapServer = "LDAP://localhost:10389";
    static readonly string adminUserName = "uid=admin,ou=system";
    static readonly string adminPassword = "secret";
    public static void Init()
    {

    }
    public static void Main()
    {
        

        // Establish connection with LDAP server using admin credentials
        var adminDirectoryEntry = new DirectoryEntry(ldapServer, adminUserName, adminPassword, AuthenticationTypes.ServerBind);
        adminDirectoryEntry.Path = "LDAP://localhost:10389/ou=users,ou=system";

        try
        {
            // Create a new user entry
            DirectoryEntry newUser = adminDirectoryEntry.Children.Add("cn=bob", "inetOrgPerson");

            // Set user attributes
            newUser.Properties["sn"].Add("bobby");
            newUser.Properties["cn"].Add("bob");
            newUser.Properties["userPassword"].Add("password123"); // Set a password for the user

            // Save the new user entry
            newUser.CommitChanges();
            Console.WriteLine("User added successfully.");

            // Search for the added user
            DirectorySearcher searcher = new DirectorySearcher(adminDirectoryEntry)
            {
                PageSize = int.MaxValue,
                Filter = "(&(objectClass=person)(cn=bob))"
            };

            searcher.PropertiesToLoad.Add("sn");
            searcher.PropertiesToLoad.Add("cn");

            var result = searcher.FindOne();

            if (result != null)
            {
                string surname = result.Properties["sn"][0].ToString();
                string commonName = result.Properties["cn"][0].ToString();
                Console.WriteLine("Surname: " + surname);
                Console.WriteLine("Common Name: " + commonName);
            }
            else
            {
                Console.WriteLine("User not found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}

