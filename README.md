# Backup Manager 2.0

Backup Manager 2.0 is a simple tool to do some backups. Simple backups that just copy what files you want to a different location, perhaps even a different drive. Its build on a command in the Windows CMD, called RoboCopy, all of its copying parameters can be changed. A list of the parameters can be found by either going to a CMD window and type "robocopy /?" or by going to the website here: https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/robocopy

![Error Loading Image](https://k2wdjg.db.files.1drv.com/y4mMBsc0zQKGFe6T3xaro_ObgwM5SyhV4lKpwDiLd3p6dDy8xog-uSW_S1_oYGCh_aWaeg7wL5jc9Zf92JkONWVtqnaP1U_wzFwVvHOj2fUUOpTWyqL_GxUe33Egq-tjzGccRjidsAXvMBCUdhqcaC-BadA8q_OJoB2N9A50OXPjA4-Ouc0kdlbuWE9W-leEFmIdBDwW7QHF6apQWyD2dQPow/sample.PNG?psid=1)

This is an image of what the program looks like. The buttons **Start Backup** and **Stop Backup** says what they do, right below them there is the button **Add Folder** from wich you can add folders to be copied. Then there are some checkboxes, **Run at Startup** will make sure that the program starts when your computer does, if you want to do a daily backup. **Close when done** automatically closes the program when the backup have finished. **Debug Mode** Shows all the files thats being copied in a different window. **Start When Open** automatically starts the backup list when the program starts. The last textbox is the copying parameters for robocopy. These can be changed accordingly to what changes you would want. There is a .txt file with the program called **ExcludeFolders.txt** here you can put in folder paths or folder names that you dont want to be backed up. It can look simply like this:

    \Application Data\
    C:\Users\Kristian\AppData\Local\Temp
    
And thats it! Hope you enjoy it!
