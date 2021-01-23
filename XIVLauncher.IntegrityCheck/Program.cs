﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json;
using XIVLauncher.PatchInstaller;

namespace XIVLauncher.Game
{
    public static class IntegrityCheck
    {
        private const string INTEGRITY_CHECK_BASE_URL = "https://goaaats.github.io/ffxiv/tools/launcher/integrity/";

        public class IntegrityCheckResult
        {
            public Dictionary<string, string> Hashes { get; set; }
            public string GameVersion { get; set; }
            public string LastGameVersion { get; set; }
        }

        public class IntegrityCheckProgress
        {
            public string CurrentFile { get; set; }
        }

        public enum CompareResult
        {
            Valid,
            Invalid,
            NoServer
        }

        public static void Main() {
            string gamePath;
            gamePath= Console.ReadLine();
            GenHashAsync(new DirectoryInfo(gamePath), null);
        }

        public static async void GenHashAsync(DirectoryInfo gamePath, IProgress<IntegrityCheckProgress> progress) {
            var localIntegrity = await RunIntegrityCheckAsync(gamePath, progress);
            //foreach (var hashEntry in localIntegrity.Hashes)
            //    Console.WriteLine($"{hashEntry.Key}     {hashEntry.Value}");
            var json = JsonConvert.SerializeObject(localIntegrity,Formatting.Indented);
            string fp = $"{localIntegrity.GameVersion}.json";
            File.WriteAllText(fp,json);
        }


        public static async Task<(CompareResult compareResult, string report, IntegrityCheckResult remoteIntegrity)>
            CompareIntegrityAsync(IProgress<IntegrityCheckProgress> progress, DirectoryInfo gamePath) {
            IntegrityCheckResult remoteIntegrity;

            try {
                remoteIntegrity = DownloadIntegrityCheckForVersion(Repository.Ffxiv.GetVer(gamePath));
            }
            catch (WebException) {
                return (CompareResult.NoServer, null, null);
            }

            var localIntegrity = await RunIntegrityCheckAsync(gamePath, progress);

            var report = "";
            foreach (var hashEntry in remoteIntegrity.Hashes)
                if (localIntegrity.Hashes.Any(h => h.Key == hashEntry.Key)) {
                    if (localIntegrity.Hashes.First(h => h.Key == hashEntry.Key).Value != hashEntry.Value)
                        report += $"Mismatch: {hashEntry.Key}\n";
                }
                else {
                    Debug.WriteLine("File not found in local integrity: " + hashEntry.Key);
                }

            return (string.IsNullOrEmpty(report) ? CompareResult.Valid : CompareResult.Invalid, report,
                remoteIntegrity);
        }

        private static IntegrityCheckResult DownloadIntegrityCheckForVersion(string gameVersion) {
            using (var client = new WebClient()) {
                return JsonConvert.DeserializeObject<IntegrityCheckResult>(
                    client.DownloadString(INTEGRITY_CHECK_BASE_URL + gameVersion + ".json"));
            }
        }

        public static async Task<IntegrityCheckResult> RunIntegrityCheckAsync(DirectoryInfo gamePath,
            IProgress<IntegrityCheckProgress> progress) {
            var hashes = new Dictionary<string, string>();

            using (var sha1 = new SHA1Managed()) {
                CheckDirectory(gamePath, sha1, gamePath.FullName, ref hashes, progress);
            }

            return new IntegrityCheckResult {
                GameVersion = Repository.Ffxiv.GetVer(gamePath),
                Hashes = hashes
            };
        }

        private static void CheckDirectory(DirectoryInfo directory, SHA1Managed sha1, string rootDirectory,
            ref Dictionary<string, string> results, IProgress<IntegrityCheckProgress> progress) {
            foreach (var file in directory.GetFiles()) {
                var relativePath = file.FullName.Substring(rootDirectory.Length);

                if (!relativePath.StartsWith("\\game"))
                    continue;

                if (relativePath.StartsWith("\\game\\My Games"))
                    continue;

                try {
                    using (var stream =
                        new BufferedStream(file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite), 1200000)) {
                        var hash = sha1.ComputeHash(stream);

                        results.Add(relativePath, BitConverter.ToString(hash).Replace('-', ' '));
                        Console.WriteLine(relativePath);
                        Console.WriteLine(BitConverter.ToString(hash).Replace('-', ' '));
                        progress?.Report(new IntegrityCheckProgress {
                            CurrentFile = relativePath
                        });
                    }
                }
                catch (IOException) {
                    // Ignore
                }
            }

            foreach (var dir in directory.GetDirectories())
                CheckDirectory(dir, sha1, rootDirectory, ref results, progress);
        }
    }
}