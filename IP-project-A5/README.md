**First install**

- git clone https://github.com/CirsteanPaul/IP-A5-2024.git or use github desktop or any tool to work with git.
- Be sure to be logged on the github account which you provided.
- You need to have installed SQL server and SSMS.
- Enter SSMS and in the connect popup write "localhost"
- Now create a database called "test" or whatever and get the connection string from it.
- That connection string should be added in "appsettings.json" in the Database section.
- Try to run the app from visual studio. The migration should happen on start and the tables in the database should be created.
- Try to run some queries in the browser(Swagger).
- If anything goes wrong in these steps search the Holy Google.

**Next steps**

- After the first iteration all this template will be deleted.
- When you start to work for your tasks please create a new database and connect to it. Don't run the app again until you delete the Migrations folder.
- If you don't delete it when you start the application all the tables from this example will be created on your DB.
- Please create a branch on which you will work. (eg. feature/samba-implement-create).(command git checkout -b new_name). 
- Get familiar with git and branches because after the first iteration you will not to be able to commit on the main branch, we will be able to work only on a separate branch.
- Get familiar with the command: git add, git commit, git push, git merge, git checkout.
- Another aspect is that when you want to merge your work on the main branch at you will need to create a PR(Pull request).
- After you create it ping your teammates to look on your code.
- Each pull request should be reviewed by at least one member of the team.

**Scenario**
- Let's say X has done a future which I need to start my work. This work is on **main** branch.
- I should be on main branch. Use the command git pull. This command will take all the changes my college did and put it in my machine.
- Now I can create a new branch git checkout -b new-branch.
- And now my work can begin.
- After I finished the feature I will use git add .(. means all files).
- git commit (with a descriptive message).
- git push to put all my changes on github.
- Finally I can create a pull request on github and wait until someone reviews my code.