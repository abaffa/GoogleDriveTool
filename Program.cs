using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;

namespace GDriveTool
{
    class Program
    {

        
        static void Main(string[] args)
        {
            // C#
            var credentials = GoogleDrive.Authenticate();

            using (var service = GoogleDrive.OpenService(credentials))
            {
                string originFolderId = "xxxxxxxxxxxxxxxxx";
                string destFolderId =   "yyyyyyyyyyyyyyyyy";
                GoogleDrive.copyAll(service, originFolderId, destFolderId);

                //List<File> lstFolders = GoogleDrive.ListFolders(service, originFolderId);
                //List<File> lstFiles = GoogleDrive.ListFiles(service, originFolderId);
                //List<File> lstAllItems = GoogleDrive.ListAll(service, originFolderId);

            }
        }
    }
}
