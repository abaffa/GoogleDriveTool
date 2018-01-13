using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;

namespace GDriveTool
{
    public class GoogleDrive
    {
        public static Google.Apis.Auth.OAuth2.UserCredential Authenticate()
        {
            var currentFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var credentialsFolder = System.IO.Path.Combine(currentFolder, "credential");

            return Authenticate(credentialsFolder);
        }

        public static Google.Apis.Auth.OAuth2.UserCredential Authenticate(string credentialsFolder)
        {
            Google.Apis.Auth.OAuth2.UserCredential credentials;

            using (var stream = new System.IO.FileStream("client_id.json", System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                // Delete credentials cache at folder debug/bin/credentials after changes here
                credentials = Google.Apis.Auth.OAuth2.GoogleWebAuthorizationBroker.AuthorizeAsync(
                    Google.Apis.Auth.OAuth2.GoogleClientSecrets.Load(stream).Secrets,
                    new[] { Google.Apis.Drive.v3.DriveService.Scope.Drive,
                    Google.Apis.Drive.v3.DriveService.Scope.DriveAppdata,
                    Google.Apis.Drive.v3.DriveService.Scope.DriveFile,
                    Google.Apis.Drive.v3.DriveService.Scope.DriveMetadata,
                    Google.Apis.Drive.v3.DriveService.Scope.DriveScripts,
                    //Google.Apis.Drive.v3.DriveService.Scope.DriveReadonly,
                    Google.Apis.Drive.v3.DriveService.Scope.DrivePhotosReadonly,
                    },
                    "user",
                    System.Threading.CancellationToken.None,
                    new Google.Apis.Util.Store.FileDataStore(credentialsFolder, true)).Result;
            }

            return credentials;
        }





        public static Google.Apis.Drive.v3.DriveService OpenService(Google.Apis.Auth.OAuth2.UserCredential credentials)
        {
            return new Google.Apis.Drive.v3.DriveService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials
            });
        }


        public static File CreateFolder(DriveService service, string newFolderName, String parentFolderId = "")
        {
            var diretorio = new Google.Apis.Drive.v3.Data.File()
            {
                Name = newFolderName,
                Parents = parentFolderId != "" ? new List<string> { parentFolderId } : null,
                MimeType = "application/vnd.google-apps.folder"
            };

            try
            {
                var request = service.Files.Create(diretorio);
                return request.Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }
            return null;
        }


        public static File CopyFile(DriveService service, String originFileId, String newFileName, String parentFolderId = "")
        {
            File newFile = new Google.Apis.Drive.v3.Data.File()
            {
                Name = newFileName,
                Parents = parentFolderId != "" ? new List<string> { parentFolderId } : null
            };

            try
            {
                var request = service.Files.Copy(newFile, originFileId);
                return request.Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }
            return null;
        }



        public static void copyAll(DriveService service, string originId, string destId)
        {
            var requestDirOrigin = service.Files.Get(originId);
            File fileDirOrigin = requestDirOrigin.Execute();

            var requestDirDest = service.Files.Get(destId);
            File fileDirDest = requestDirDest.Execute();

            List<File> lstAll = ListAll(service, destId);
            List<String> lstAllNames = lstAll.Select(p => p.Name).ToList();

            List<File> lFolders = ListFolders(service, originId);
            foreach (File f in lFolders)
            {
                if (!lstAllNames.Contains(f.Name))
                {
                    Console.WriteLine("# Creating Folder " + f.Name);
                    File newf = CreateFolder(service, f.Name, destId);
                    copyAll(service, f.Id, newf.Id);
                }
                else
                {
                    Console.WriteLine("# Folder exists " + f.Name);
                    File newf = lstAll.Where(p => p.Name == f.Name).First();
                    copyAll(service, f.Id, newf.Id);
                }
            }

            List<File> lFiles = ListFiles(service, originId);
            foreach (File f in lFiles)
            {
                if (!lstAllNames.Contains(f.Name))
                {
                    Console.WriteLine("* Create new file " + f.Name);
                    CopyFile(service, f.Id, f.Name, destId);
                }
                else
                    Console.WriteLine("# File exists " + f.Name);
            }

        }

        public static List<File> ListAll(Google.Apis.Drive.v3.DriveService service, string parentFolderId, int filesPerPage = 100)
        {
            List<File> filesList = new List<File>();

            var request = service.Files.List();

            request.Fields = "nextPageToken, files(id, parents, mimeType, name)";
            if (parentFolderId.Trim() == "")
                return null;
            request.Q = "'" + parentFolderId + "' in parents";
            request.Q += " and trashed = false";  // exclude folders in trash

            // Default 100, max 1000.
            request.PageSize = filesPerPage;
            var requestResult = request.Execute();
            var files = requestResult.Files;

            while (files != null && files.Any())
            {
                foreach (var file in files)
                {
                    filesList.Add(file);
                }

                if (requestResult.NextPageToken != null)
                {
                    request.PageToken = requestResult.NextPageToken;
                    requestResult = request.Execute();
                    files = requestResult.Files;
                }
                else
                {
                    files = null;
                }
            }

            return filesList;
        }

        public static List<File> ListFiles(Google.Apis.Drive.v3.DriveService service, string parentFolderId, int filesPerPage = 100)
        {
            List<File> filesList = new List<File>();

            var request = service.Files.List();

            request.Fields = "nextPageToken, files(id, parents, mimeType, name)";
            if (parentFolderId.Trim() == "")
                return null;
            request.Q = "'" + parentFolderId + "' in parents";
            request.Q += " and trashed = false";  // exclude folders in trash

            // Default 100, max 1000.
            request.PageSize = filesPerPage;
            var requestResult = request.Execute();
            var files = requestResult.Files;

            while (files != null && files.Any())
            {
                foreach (var file in files)
                {
                    if (file.MimeType != "application/vnd.google-apps.folder")
                        filesList.Add(file);
                }

                if (requestResult.NextPageToken != null)
                {
                    request.PageToken = requestResult.NextPageToken;
                    requestResult = request.Execute();
                    files = requestResult.Files;
                }
                else
                {
                    files = null;
                }
            }

            return filesList;
        }
        public static List<File> ListFolders(Google.Apis.Drive.v3.DriveService service, string parentFolderId, int filesPerPage = 100)
        {
            List<File> folderList = new List<File>();

            var request = service.Files.List();

            request.Fields = "nextPageToken, files(id, parents, mimeType, name)";
            if (parentFolderId.Trim() == "")
                return null;
            request.Q = "'" + parentFolderId + "' in parents";
            request.Q += " and trashed = false";  // exclude folders in trash

            // Default 100, max 1000.
            request.PageSize = filesPerPage;
            var requestResult = request.Execute();
            var files = requestResult.Files;

            while (files != null && files.Any())
            {
                foreach (var file in files)
                {
                    if (file.MimeType == "application/vnd.google-apps.folder")
                        folderList.Add(file);
                }

                if (requestResult.NextPageToken != null)
                {
                    request.PageToken = requestResult.NextPageToken;
                    requestResult = request.Execute();
                    files = requestResult.Files;
                }
                else
                {
                    files = null;
                }
            }

            return folderList;
        }


        public static string[] SearchForFileId(Google.Apis.Drive.v3.DriveService service, string name, bool includeTrash = false)
        {
            var ret = new List<string>();

            var request = service.Files.List();
            request.Q = string.Format("name = '{0}'", name);
            if (!includeTrash)
            {
                request.Q += " and trashed = false";
            }
            request.Fields = "files(id)";
            var requestResult = request.Execute();
            var files = requestResult.Files;

            if (files != null && files.Any())
            {
                foreach (var file in files)
                {
                    ret.Add(file.Id);
                }
            }

            return ret.ToArray();
        }


        public static void Delete(Google.Apis.Drive.v3.DriveService service, string fileId)
        {

            try
            {
                var request = service.Files.Delete(fileId);
                request.Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }
        }


        public static File Upload(Google.Apis.Drive.v3.DriveService service, string filepath, String parentFolderId = "")
        {
            var newFile = new Google.Apis.Drive.v3.Data.File()
            {
                Name = System.IO.Path.GetFileName(filepath),
                Parents = parentFolderId != "" ? new List<string> { parentFolderId } : null,
                MimeType = MimeTypes.MimeTypeMap.GetMimeType(System.IO.Path.GetExtension(filepath))
            };

            try
            {
                using (var stream = new System.IO.FileStream(filepath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    var request = service.Files.Create(newFile, stream, newFile.MimeType);
                    request.Fields = "id";
                    request.Upload();
                    return request.ResponseBody;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }

            return null;
        }

        public static File ResumeUpload(Google.Apis.Drive.v3.DriveService service, string filepath, string fileId)
        {
            var newFile = new Google.Apis.Drive.v3.Data.File();
            newFile.Name = System.IO.Path.GetFileName(filepath);
            newFile.MimeType = MimeTypes.MimeTypeMap.GetMimeType(System.IO.Path.GetExtension(filepath));
            try
            {
                using (var stream = new System.IO.FileStream(filepath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    var request = service.Files.Update(newFile, fileId, stream, newFile.MimeType);
                    request.Fields = "id";
                    request.Upload();
                    return request.ResponseBody;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }

            return null;
        }


        public static void Download(Google.Apis.Drive.v3.DriveService service, string fileId, string localDestinationFilename)
        {

            try
            {
                var request = service.Files.Get(fileId);
                using (var stream = new System.IO.FileStream(localDestinationFilename, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    request.Download(stream);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }

        }

    }
}
