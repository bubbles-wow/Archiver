using CoreTool.Archive;
using StoreLib.Models;
using StoreLib.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CoreTool.Loaders.Windows
{
    internal class StoreLoader : ILoader
    {
        private string packageId;
        private string packageName;
        private bool hasBeta;
        private bool authBetaQuery;
        private string scope;

        public StoreLoader(string packageId, string packageName, bool hasBeta = true, bool authBetaQuery = false, string scope = "service::dcat.update.microsoft.com::MBI_SSL")
        {
            this.packageId = packageId;
            this.packageName = packageName;
            this.hasBeta = hasBeta;
            this.authBetaQuery = authBetaQuery;
            this.scope = scope;
        }

        public async Task Load(ArchiveMeta archive)
        {
            // Create the dcat handler in production mode
            DisplayCatalogHandler dcathandler = DisplayCatalogHandler.ProductionConfig();

            // Create a packages var for debugging
            IList<PackageInstance> packages;
            string releaseVer = "";
            string uids = "";
            archive.Logger.Write("Loading release...");

            // Grab the packages for the release
            await dcathandler.QueryDCATAsync(this.packageId);
            if (dcathandler.Result == DisplayCatalogResult.Found)
            {
                packages = await dcathandler.GetPackagesForProductAsync();
                foreach (PackageInstance package in packages)
                {
                    if (!package.PackageMoniker.StartsWith(packageName + "_")) continue;
                    int platformTarget = package.ApplicabilityBlob.ContentTargetPlatforms[0].PlatformTarget;
                    
                    if (platformTarget !=0
                        && platformTarget != 3) continue;

                    uids = package.UpdateId;

                    //Automatically convert WSA file suffix name to msixbundle
                    string fullPackageName;
                    if (package.PackageMoniker.IndexOf("WindowsSubsystemForAndroid") > 0)
                    fullPackageName = package.PackageMoniker + ".Msixbundle";
                    else fullPackageName = package.PackageMoniker + (platformTarget == 0 ? ".Appx" : ".AppxBundle");
                    
                    //string fullPackageName = package.PackageMoniker + (platformTarget == 0 ? ".Appx" : ".AppxBundle");
                    
                    // Create the meta and store it
                    Item item = new Item(Utils.GetVersionFromName(fullPackageName));
                    item.Archs[Utils.GetArchFromName(fullPackageName)] = new Arch(fullPackageName, new List<string>() { Guid.Parse(package.UpdateId).ToString() });
                    if (archive.AddOrUpdate(item, true)) archive.Logger.Write($"New version registered: {Utils.GetVersionFromName(fullPackageName)}");

                    //output download url without downloading
                    archive.Logger.WriteWarn($"File Name: {fullPackageName}");
                    archive.Logger.WriteWarn($"URL: {package.PackageUri.OriginalString}");
                    archive.Logger.WriteWarn($"UpdateId: {package.UpdateId}");

                    releaseVer = Utils.GetVersionFromName(fullPackageName);
                }
            }

            if (!hasBeta) return;

            //set re-login flag
            a: 

            // Make sure we have a token, if not don't bother checking for betas
            string token = await Utils.GetMicrosoftToken("msAuthInfo.json", scope);
            if (token == "")
            {
                archive.Logger.WriteError("You need re-login to fetch beta vesion.");
                if (File.Exists(Path.GetFullPath("msAuthInfo.json")))
                    File.Delete(Path.GetFullPath("msAuthInfo.json"));
                goto a;
            }
            else
            {
                archive.Logger.Write("Loading beta...");

                // Grab the packages for the beta using auth
                string authentication = "";
                if (authBetaQuery)
                {
                    authentication = "WLID1.0=" + Encoding.Unicode.GetString(Convert.FromBase64String(token));
                }
                await dcathandler.QueryDCATAsync(this.packageId, IdentiferType.ProductID, authentication);
                if (dcathandler.Result == DisplayCatalogResult.Found)
                {
                    packages = await dcathandler.GetPackagesForProductAsync($"<User>{await Utils.GetMicrosoftToken("msAuthInfo.json")}</User>");
                    bool flag = true;
                    foreach (PackageInstance package in packages)
                    {
                        if (!package.PackageMoniker.StartsWith(packageName + "_")) continue;
                        
                        int platformTarget = package.ApplicabilityBlob.ContentTargetPlatforms[0].PlatformTarget;
                        if (platformTarget != 0
                            && platformTarget != 3) continue;
                        
                        //Automatically convert WSA file suffix name to msixbundle
                        string fullPackageName;
                        if (package.PackageMoniker.IndexOf("WindowsSubsystemForAndroid") > 0)
                        fullPackageName = package.PackageMoniker + ".Msixbundle";
                        else fullPackageName = package.PackageMoniker + (platformTarget == 0 ? ".Appx" : ".AppxBundle");

                        //string fullPackageName = package.PackageMoniker + (platformTarget == 0 ? ".Appx" : ".AppxBundle");

                        //check if token is invaild by wsa updateid
                        if (package.UpdateId == uids && package.PackageMoniker.IndexOf("WindowsSubsystemForAndroid") > 0)
                        {
                            archive.Logger.WriteError($"You need re-login to fetch beta vesion.");
                            if (File.Exists(Path.GetFullPath("msAuthInfo.json")))
                                File.Delete(Path.GetFullPath("msAuthInfo.json"));
                            goto a;
                        }

                        // Check we haven't got a release version in the beta request
                        if (Utils.GetVersionFromName(fullPackageName) == releaseVer && flag)
                        {
                            flag = false;
                            archive.Logger.WriteError($"There is currently no beta version available.");
                            archive.Logger.WriteWarn($"Current version: {Utils.GetVersionFromName(fullPackageName)}");
                        }

                        // Create the meta and store it
                        Item item = new Item(Utils.GetVersionFromName(fullPackageName));
                        item.Archs[Utils.GetArchFromName(fullPackageName)] = new Arch(fullPackageName, new List<string>() { Guid.Parse(package.UpdateId).ToString() });
                        if (archive.AddOrUpdate(item, true)) archive.Logger.WriteWarn($"New version registered: {Utils.GetVersionFromName(fullPackageName)}");

                        //output download url without downloading
                        
                        archive.Logger.WriteWarn($"File Name: {fullPackageName}");
                        archive.Logger.WriteWarn($"URL: {package.PackageUri.OriginalString}");
                        archive.Logger.WriteWarn($"UpdateId: {package.UpdateId}");
                    }
                }
            }
        }
    }
}
