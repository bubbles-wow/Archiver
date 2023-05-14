using GooglePlayApi;
using GooglePlayApi.Helpers;
using GooglePlayApi.Models;
using GooglePlayApi.Popup;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MicrosoftAuth;
using MicrosoftAuth.Models.Token;
using MSAuth.Popup;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Text.RegularExpressions;

namespace CoreTool
{
    internal class Utils
    {
        public static readonly Task CompletedTask = Task.FromResult(false);

        public static readonly Log GenericLogger = new Log("Util");

        private static readonly JsonSerializerOptions serializeOptions = new JsonSerializerOptions { Converters = { new CultureInfoJsonConverter() } };

        public static string GetVersionFromName(string name)
        {
            string fileName = Path.GetFileNameWithoutExtension(name);
            string extension = Path.GetExtension(name).ToLower();
            if (extension == ".appx")
            {
                return GetVersionFromNameAppx(fileName);
            }
            else if (extension == ".appxbundle" || extension == ".msixbundle" || extension == ".msix")
            {
                string version = fileName.Split("_")[1];
                
                //special in edu version
                if (name.IndexOf("Edu") > 0)
                {
                    string shortVer = name.Split("_")[1];
                    string[] versionPart = shortVer.Split(".");
                    if (versionPart[3].ToLower() == "appxbundle" || versionPart[3] == "00")
                        return $"{versionPart[0]}.{versionPart[1]}.{versionPart[2]}.0";
                    return $"{versionPart[0]}.{versionPart[1]}.{versionPart[2]}.{versionPart[3]}";
                }
                /*if (version.Split(".").Length > 3)
                {
                    version = Regex.Replace(version, "\\.00?$", "");
                }*/

                return version;
            }
            else if (extension == ".apk")
            {
                return fileName.Split("-")[1];
            }
            else
            {
                return "unknown";
            }
        }

        private static string GetVersionFromNameAppx(string name)
        {
            string rawVer = name.Split("_")[1];
            string[] verParts = rawVer.Split('.');
            //edu solution
            if (name.IndexOf("Edu") > 0)
            {
                if (verParts[0] == "0")
                {
                    if (verParts[1].Length == 3)
                        return $"{verParts[0]}.{verParts[1].Substring(0, 2)}.{verParts[1].Substring(2, 1)}.0";
                    if (verParts[1].Length == 4)
                        return $"{verParts[0]}.{verParts[1].Substring(0, 2)}.{verParts[1].Substring(2, 2)}.0";
                    return $"{verParts[0]}.{verParts[1]}.{verParts[2]}.0";
                }
                else
                {
                    if (verParts[1] == "12")
                    {
                        if (verParts[2] == "0") return "1.12.0.0";
                        if (verParts[2] == "301") return "1.12.3.1";
                        if (verParts[2] == "501") return "1.12.5.0";
                        if (verParts[2] == "601") return "1.12.60.0";
                    }
                    if (verParts[2].Length > 2)
                    {
                        if (verParts[2].Length == 3)
                            return $"{verParts[0]}.{verParts[1]}.{verParts[2].Substring(0, 2)}.{verParts[2].Substring(2,1)}";
                        if (verParts[2].Length == 4)
                        {
                            if (verParts[2].Substring(2, 2) == "00")
                                return $"{verParts[0]}.{verParts[1]}.{verParts[2].Substring(0, 2)}.0";
                            else
                                return $"{verParts[0]}.{verParts[1]}.{verParts[2].Substring(0, 2)}.{verParts[2].Substring(2, 2)}";
                        }
                    }
                    else
                        return $"{verParts[0]}.{verParts[1]}.{verParts[2]}.{verParts[3]}";
                }
            }

            // Check if we are a pre-v1 version as they have a different format
            if (verParts[0] == "0")
            {
                string lastBit = verParts[1].Substring(2).TrimStart('0');
                string firstBit = verParts[1].Substring(0, 2);

                if (lastBit == "")
                {
                    lastBit = "0";
                }

                return $"{verParts[0]}.{firstBit}.{lastBit}.{verParts[2]}";
            }
            else
            {
                verParts[2] = verParts[2].PadLeft(2, '0');
                string lastBit = verParts[2].Substring(verParts[2].Length - 2).TrimStart('0');
                string firstBit = verParts[2].Substring(0, verParts[2].Length - 2);

                if (firstBit == "")
                {
                    firstBit = "0";
                }

                if (lastBit == "")
                {
                    lastBit = "0";
                }

                return $"{verParts[0]}.{verParts[1]}.{firstBit}.{lastBit}";
            }
        }

        public static string GetArchFromName(string name)
        {
            string fileName = Path.GetFileNameWithoutExtension(name);
            string extension = Path.GetExtension(name).ToLower();
            if (extension.StartsWith(".appx") || extension.StartsWith(".msix"))
            {
                //special edu version file name
                if (name.IndexOf("Edu") > 0)
                {
                    if (extension == ".appx")
                    {
                        if (name.IndexOf("Publish") > 0)
                        {
                            string archcache = fileName.Split("_")[2];
                            return archcache.Split(".")[0];
                        }
                    }
                    else
                        return "neutral";
                }
                return fileName.Split("_")[2];
            }
            else if (extension == ".apk")
            {
                return fileName.Substring(name.IndexOf('-', name.IndexOf('-') + 1) + 1);
            }
            else
            {
                return "unknown";
            }
        }

        // https://stackoverflow.com/a/10789196/5299903
        public static Task<(int ExitCode, string Output)> RunProcessAsync(string fileName, string arguments = "", string workingDir = "./")
        {
            var tcs = new TaskCompletionSource<(int ExitCode, string Output)>();
            string output = "";

            var process = new Process
            {
                StartInfo = {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = workingDir,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
            };

            process.OutputDataReceived += (sender, args) => output += args.Data;

            process.Exited += (sender, args) =>
            {
                tcs.SetResult((ExitCode: process.ExitCode, Output: output));
                process.Dispose();
            };

            process.Start();
            process.BeginOutputReadLine();

            return tcs.Task;
        }

        /// <summary>
        /// Re-implementation of the native StrCmpLogicalW from https://stackoverflow.com/a/5641272/5299903
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int StrCmpLogicalW(string x, string y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int lx = x.Length, ly = y.Length;

            for (int mx = 0, my = 0; mx < lx && my < ly; mx++, my++)
            {
                if (char.IsDigit(x[mx]) && char.IsDigit(y[my]))
                {
                    long vx = 0, vy = 0;

                    for (; mx < lx && char.IsDigit(x[mx]); mx++)
                        vx = vx * 10 + x[mx] - '0';

                    for (; my < ly && char.IsDigit(y[my]); my++)
                        vy = vy * 10 + y[my] - '0';

                    if (vx != vy)
                        return vx > vy ? 1 : -1;
                }

                if (mx < lx && my < ly && x[mx] != y[my])
                    return x[mx] > y[my] ? 1 : -1;
            }

            return lx - ly;
        }

        public static async Task<AuthData> GetGooglePlayAuthData(string cacheFile, string deviceProperties = "octopus.properties")
        {
            AuthData authData;

            if (!File.Exists(cacheFile))
            {
                (string Email, string OAuthToken) authResponse = AuthPopupForm.GetOAuthToken();
                authData = await AuthHelper.Build(authResponse.Email, authResponse.OAuthToken, deviceProperties);
            }
            else
            {
                // Get cached auth
                using (FileStream readStream = File.OpenRead(cacheFile))
                    authData = await JsonSerializer.DeserializeAsync<AuthData>(readStream, serializeOptions);

                // Re-aquire tokens
                // TODO: Check if they are still valid
                authData = await AuthHelper.Build(authData.Email, authData.AasToken, deviceProperties);
            }

            // Save latest auth to cache file
            try
            {
                using (FileStream writeStream = File.Create(cacheFile))
                    await JsonSerializer.SerializeAsync(writeStream, authData, serializeOptions);
            }
            catch (JsonException ex)
            {
                GenericLogger.WriteError(ex.Message);
            }

            return authData;
        }

        public static async Task<string> GetMicrosoftToken(string cacheFile, string scope = "service::dcat.update.microsoft.com::MBI_SSL")
        {
            MicrosoftAccount account = null;

            if (File.Exists(cacheFile))
            {
                await using FileStream readStream = File.OpenRead(cacheFile);
                MicrosoftAccount cachedAccount = await JsonSerializer.DeserializeAsync<MicrosoftAccount>(readStream, serializeOptions);

                if (!cachedAccount.DaToken.IsExpired())
                    account = cachedAccount;
            }

            if (account == null)
            {
                string token = OAuthPopup.GetAuthToken();
                account = MicrosoftAccount.FromOAuthResponse(token);
            }

            BaseToken requestedToken = await account.RequestToken("{28520974-CE92-4F36-A219-3F255AF7E61E}", new SecureScope($"scope={scope}", "TOKEN_BROKER"));

            GenericLogger.Write($"Microsoft: Received token for scope {scope}.");

            try
            {
                await using FileStream writeStream = File.Create(cacheFile);
                await JsonSerializer.SerializeAsync(writeStream, account, serializeOptions);
            }
            catch (JsonException ex)
            {
                GenericLogger.WriteError(ex.Message);
            }

            return Convert.ToBase64String(Encoding.Unicode.GetBytes(requestedToken.Token));
        }
    }
}