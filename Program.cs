﻿using System;
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
            
            if (!Parameters.Process(args))
            {
                Parameters.Description();
            }
            else
            {
                var credentials = GoogleDrive.Authenticate();

                using (var service = GoogleDrive.OpenService(credentials))
                {
                    //string originFolderId = "xxxxxxxxxxxxxxxxx";
                    //string destFolderId =   "yyyyyyyyyyyyyyyyy";
                    //GoogleDrive.copyAll(service, originFolderId, destFolderId);

                    //List<File> lstFolders = GoogleDrive.ListFolders(service, originFolderId);
                    //List<File> lstFiles = GoogleDrive.ListFiles(service, originFolderId);
                    //List<File> lstAllItems = GoogleDrive.ListAll(service, originFolderId);

                    File f = null;
                    switch (Parameters.CurrentAction)
                    {
                        case Parameters.Actions.create_folder:
                            f = GoogleDrive.CreateFolder(service, Parameters.itemName, Parameters.destId);
                            if (f != null)
                                Console.WriteLine("New Folder ID:" + f.Id);
                            break;
                        case Parameters.Actions.copy_file:
                            f = GoogleDrive.CopyFile(service, Parameters.originId, Parameters.itemName, Parameters.destId);
                            if (f != null)
                                Console.WriteLine("New File ID:" + f.Id);
                            break;
                        case Parameters.Actions.copy_all:
                            GoogleDrive.copyAll(service, Parameters.originId, Parameters.destId);
                            break;
                        case Parameters.Actions.list_all:
                            foreach (File i in GoogleDrive.ListAll(service, Parameters.originId))
                                Console.WriteLine("{0}\t{1}\t{2}", i.Id, i.MimeType, i.Name);
                            break;
                        case Parameters.Actions.list_files:
                            foreach (File i in GoogleDrive.ListFiles(service, Parameters.originId))
                                Console.WriteLine("{0}\t{1}\t{2}", i.Id, i.MimeType, i.Name);
                            break;
                        case Parameters.Actions.list_folders:
                            foreach (File i in GoogleDrive.ListFolders(service, Parameters.originId))
                                Console.WriteLine("{0}\t{1}\t{2}", i.Id, i.MimeType, i.Name);
                            break;
                        case Parameters.Actions.search:
                            foreach (String s in GoogleDrive.SearchForFileId(service, Parameters.itemName))
                                Console.WriteLine(s);
                            break;
                        case Parameters.Actions.delete:
                            GoogleDrive.Delete(service, Parameters.originId);
                            break;
                        case Parameters.Actions.upload:
                            f = GoogleDrive.Upload(service, Parameters.localFilename, Parameters.destId);
                            if (f != null)
                                Console.WriteLine("New File ID:" + f.Id);
                            break;
                        case Parameters.Actions.resume_upload:
                            f = GoogleDrive.ResumeUpload(service, Parameters.localFilename, Parameters.destId);
                            if (f != null)
                                Console.WriteLine("New File ID:" + f.Id);
                            break;
                        case Parameters.Actions.download:
                            GoogleDrive.Download(service, Parameters.originId, Parameters.destinationFilename);
                            break;
                        case Parameters.Actions.download_folder:
                            GoogleDrive.DownloadFolder(service, Parameters.originId, Parameters.destinationFilename);
                            break;
                        default:
                            Parameters.Description();
                            break;
                    }

                }
            }
        }
    }
}
