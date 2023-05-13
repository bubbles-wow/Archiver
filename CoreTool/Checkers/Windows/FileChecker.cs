﻿using CoreTool.Archive;
using StoreLib.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoreTool.Checkers.Windows
{
    internal class FileChecker : IChecker
    {
        public async Task Check(ArchiveMeta archive)
        {
            archive.Logger.Write("Checking for missing files...");

            HttpClient httpClient = new HttpClient();

            string token = await Utils.GetMicrosoftToken("msAuthInfo.json");

            bool hasChanges = false;

            foreach (Item item in archive.GetItems())
            {
                foreach (Arch arch in item.Archs.Values)
                {
                    string outPath = Path.Join(archive.ArchiveDir, arch.FileName);
                    if (!File.Exists(outPath))
                    {
                        archive.Logger.Write($"Preparing for downloading");

                        List<string> updateIds = arch.UpdateIds.Select(guid => guid.ToString()).ToList();
                        List<string> revisionIds = new List<string>();

                        // Create the revisionId list (all 1 since MC only uses that) and then fetch the urls
                        revisionIds.AddRange(Enumerable.Repeat("1", updateIds.Count));
                        IList<Uri> Files = await FE3Handler.GetFileUrlsAsync(updateIds, revisionIds, $"<User>{token}</User>");
                        bool success = false;
                        foreach (Uri uri in Files)
                        {
                            // Check if there is a download link for the file
                            if (uri.Host == "test.com") continue;

                            try
                            {
                                archive.Logger.Write("Sucessfully generate vaild url");
                                archive.Logger.WriteWarn($"File Name: {arch.FileName}");
                                archive.Logger.WriteWarn($"URL: {uri.OriginalString}");
                                await httpClient.DownloadFileTaskAsync(uri, outPath, archive.DownloadProgressChanged);
                                Console.WriteLine();
                                archive.Logger.WriteWarn("Calculating file hashes, this may take some time");
                                arch.Hashes = new FileHashes(outPath);
                                success = true;
                                hasChanges = true;
                            }
                            catch (WebException ex)
                            {
                                // The download threw an exception so let the user know and cleanup
                                Console.WriteLine();
                                archive.Logger.WriteError($"Failed to download: {ex.Message}");
                                File.Delete(outPath);
                            }

                            if (success) break;
                        }

                        if (!success) archive.Logger.WriteError($"Failed to download from any urls");
                    }
                }
            }

            if (hasChanges)
            {
                archive.Save();
            }
        }
    }
}
