using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Auth.OAuth2;

namespace GDriveTool
{
    public class GoogleDrive
    {
        private static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        public static UserCredential Authenticate()
        {
            var currentFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var credentialsFolder = System.IO.Path.Combine(currentFolder, "credential");

            return Authenticate(credentialsFolder);
        }

        public static UserCredential Authenticate(string credentialsFolder)
        {
            UserCredential credentials;

            using (var stream = new System.IO.FileStream("client_id.json", System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                // Delete credentials cache at folder debug/bin/credentials after changes here
                credentials = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { DriveService.Scope.Drive,
                    DriveService.Scope.DriveAppdata,
                    DriveService.Scope.DriveFile,
                    DriveService.Scope.DriveMetadata,
                    DriveService.Scope.DriveScripts,
                    //DriveService.Scope.DriveReadonly,
                    DriveService.Scope.DrivePhotosReadonly,
                    },
                    "user",
                    System.Threading.CancellationToken.None,
                    new Google.Apis.Util.Store.FileDataStore(credentialsFolder, true)).Result;
            }

            return credentials;
        }


        
        public static DriveService OpenService(UserCredential credentials)
        {
            return new DriveService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials
            });
        }


        // -cf  <new foldername> <optional destinationFolderId> Create a Folder
        public static File CreateFolder(DriveService service, string newFolderName, String parentFolderId = "")
        {
            var diretorio = new File()
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


        // -cp <originId> <new filename> <optional destinationFolderId> Copy a file
        public static File CopyFile(DriveService service, String originFileId, String newFileName, String parentFolderId = "")
        {
            File newFile = new File()
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



        // -cpall <originId> <destId> Copy all files and folders recursively
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

        // -l <parentFolderId> List items of a location
        public static List<File> ListAll(DriveService service, string parentFolderId, int filesPerPage = 100)
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

        // -lf <parentFolderId> List files of a location
        public static List<File> ListFiles(DriveService service, string parentFolderId, int filesPerPage = 100)
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

        // -ld <parentFolderId> List folders of a location
        public static List<File> ListFolders(DriveService service, string parentFolderId, int filesPerPage = 100)
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


        // -s <name> <optional includeTrash True/Fase> Search a File
        public static string[] SearchForFileId(DriveService service, string name, bool includeTrash = false)
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

        // -del <fileId> Delete a file/Folder
        public static void Delete(DriveService service, string fileId)
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


        // -u <filePath> <optional parentFolderId> Upload a File
        public static File Upload(DriveService service, string filepath, String parentFolderId = "")
        {
            var newFile = new File()
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

        // -ru <filePath> <fileId> Resume Upload
        public static File ResumeUpload(DriveService service, string filepath, string fileId)
        {
            var newFile = new File();
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


        // -d <fileId> <destinationFilename> Download File
        public static void Download(DriveService service, string fileId, string localDestinationFilename)
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

        public static void DownloadFolder(DriveService service, string originId, string folder)
        {
            var requestDirOrigin = service.Files.Get(originId);
            File fileDirOrigin = requestDirOrigin.Execute();


            List<String> fileEntries = new System.Collections.Generic.List<String>(System.IO.Directory.GetFiles(folder));

            // Recurse into subdirectories of this directory.
            List<String> subdirectoryEntries = new System.Collections.Generic.List<String>(System.IO.Directory.GetDirectories(folder));

            List<File> lFolders = ListFolders(service, originId);
            foreach (File f in lFolders)
            {
                if (!subdirectoryEntries.Contains(f.Name))
                {
                    Console.WriteLine("# Creating Folder " + f.Name);
                    String newFolder = folder + (IsLinux ? "/" : "\\") + f.Name;
                    System.IO.Directory.CreateDirectory(newFolder);
                    DownloadFolder(service, f.Id, newFolder);
                }
                else
                {
                    Console.WriteLine("# Folder exists " + f.Name);
                    String newFolder = folder + (IsLinux ? "/" : "\\") + f.Name;
                    DownloadFolder(service, f.Id, newFolder);
                }
            }

            List<File> lFiles = ListFiles(service, originId);
            foreach (File f in lFiles)
            {
                if (!fileEntries.Contains(f.Name))
                {
                    Console.WriteLine("* Create new file " + f.Name);

                    String newFilename = folder + (IsLinux ? "/" : "\\") + f.Name;
                    Download(service, f.Id, newFilename);
                }
                else
                {
                    Console.WriteLine("# File exists " + f.Name);
                }
            }

        }

    }
}
