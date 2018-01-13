using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDriveTool
{
    public static class Parameters
    {

        public static String originId { get; set; }
        public static String destId { get; set; }
        public static String parentFolderId { get; set; }
        public static String itemName { get; set; }
        public static String destinationFilename { get; set; }
        public static String localFilename { get; set; }

        public static Actions CurrentAction { get; set; }

        public enum Actions
        {
            none,
            create_folder,
            copy_file,
            copy_all,
            list_all,
            list_files,
            list_folders,
            search,
            delete,
            upload,
            resume_upload,
            download

        }


        public static void Description()
        {

            Console.WriteLine("+ Usage GDriveTool.exe <parameters>");
            Console.WriteLine();
            Console.WriteLine("  Paramenters:");
            Console.WriteLine("  -cf <new foldername> <optional destinationFolderId>\t\tCreate a Folder");
            Console.WriteLine("  -cp <originId> <new filename> <optional destinationFolderId>\tCopy a file");
            Console.WriteLine("  -cpall <originId> <destId>\t\t\t\t\tCopy all files and folders recursively");
            Console.WriteLine("  -l <parentFolderId root/id>\t\t\t\t\tList items of a location");
            Console.WriteLine("  -lf <parentFolderId root/id>\t\t\t\t\tList files of a location");
            Console.WriteLine("  -ld <parentFolderId root/id>\t\t\t\t\tList folders of a location");
            Console.WriteLine("  -s <name> <optional includeTrash True/Fase>\t\t\tSearch a File");
            Console.WriteLine("  -del <fileId>\t\t\t\t\t\t\tDelete a file/Folder");
            Console.WriteLine("  -u <filePath> <optional parentFolderId>\t\t\tUpload a File");
            Console.WriteLine("  -ru <filePath> <fileId>\t\t\t\t\tResume Upload");
            Console.WriteLine("  -d <fileId> <destinationFilename>\t\t\t\tDownload File");
            Console.WriteLine();
        }
        public static bool Process(string[] args)
        {
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-cf" && i + 1 < args.Length)
                    {
                        itemName = args[i + 1];
                        if (i + 2 < args.Length)
                            destId = args[i + 2];

                        CurrentAction = Actions.create_folder;
                        return true;
                    }
                    else if (args[i] == "-cp" && i + 2 < args.Length)
                    {
                        originId = args[i + 1];
                        itemName = args[i + 2];
                        if (i + 3 < args.Length)
                            destId = args[i + 3];

                        CurrentAction = Actions.copy_file;
                        return true;
                    }
                    else if (args[i] == "-cpall" && i + 2 < args.Length)
                    {
                        originId = args[i + 1];
                        destId = args[i + 2];
                        CurrentAction = Actions.copy_all;
                        return true;
                    }

                    else if (args[i] == "-l" && i + 1 < args.Length)
                    {
                        originId = args[i + 1];
                        CurrentAction = Actions.list_all;
                        return true;
                    }
                    else if (args[i] == "-lf" && i + 1 < args.Length)
                    {
                        originId = args[i + 1];
                        CurrentAction = Actions.list_files;
                        return true;
                    }
                    else if (args[i] == "-ld" && i + 1 < args.Length)
                    {
                        originId = args[i + 1];
                        CurrentAction = Actions.list_folders;
                        return true;
                    }
                    else if (args[i] == "-s" && i + 1 < args.Length)
                    {
                        itemName = args[i + 1];
                        CurrentAction = Actions.search;
                        return true;
                    }
                    else if (args[i] == "-del" && i + 1 < args.Length)
                    {
                        originId = args[i + 1];
                        CurrentAction = Actions.search;
                        return true;
                    }
                    else if (args[i] == "-u" && i + 1 < args.Length)
                    {
                        localFilename = args[i + 1];
                        if (i + 2 < args.Length)
                            destId = args[i + 2];
                        CurrentAction = Actions.upload;
                        return true;
                    }
                    else if (args[i] == "-ru" && i + 1 < args.Length)
                    {
                        localFilename = args[i + 1];
                        if (i + 2 < args.Length)
                            destId = args[i + 2];
                        CurrentAction = Actions.resume_upload;
                        return true;
                    }
                    else if (args[i] == "-d" && i + 2 < args.Length)
                    {
                        originId = args[i + 1];
                        destinationFilename = args[i + 2];
                        CurrentAction = Actions.download;
                        return true;
                    }

                }
            }
            return false;
        }
    }

}
