using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

using Autofac;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using CKAN.DLC;
using CKAN.Games.KerbalSpaceProgram.GameVersionProviders;
using CKAN.Games.KerbalSpaceProgram.DLC;
using CKAN.Versioning;

namespace CKAN.Games.KerbalSpaceProgram
{
    public class KerbalSpaceProgram : IGame
    {
        public string ShortName => "KSP";
        public DateTime FirstReleaseDate => new DateTime(2011, 6, 24);

        public bool GameInFolder(DirectoryInfo where)
            => InstanceAnchorFiles.Any(f => File.Exists(Path.Combine(where.FullName, f)))
                && Directory.Exists(Path.Combine(where.FullName, "GameData"));

        /// <summary>
        /// Finds the Steam KSP path. Returns null if the folder cannot be located.
        /// </summary>
        /// <returns>The KSP path.</returns>
        public string SteamPath()
        {
            // Attempt to get the Steam path.
            string steamPath = CKANPathUtils.SteamPath();

            if (steamPath == null)
            {
                return null;
            }

            // Default steam library
            string installPath = KSPDirectory(steamPath);
            if (installPath != null)
            {
                return installPath;
            }

            // Attempt to find through config file
            string configPath = Path.Combine(steamPath, "config", "config.vdf");
            if (File.Exists(configPath))
            {
                log.InfoFormat("Found Steam config file at {0}", configPath);
                StreamReader reader = new StreamReader(configPath);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Found Steam library
                    if (line.Contains("BaseInstallFolder"))
                    {
                        // This assumes config file is valid, we just skip it if it looks funny.
                        string[] split_line = line.Split('"');

                        if (split_line.Length > 3)
                        {
                            log.DebugFormat("Found a Steam Libary Location at {0}", split_line[3]);

                            installPath = KSPDirectory(split_line[3]);
                            if (installPath != null)
                            {
                                log.InfoFormat("Found a KSP install at {0}", installPath);
                                return installPath;
                            }
                        }
                    }
                }
            }

            // Could not locate the folder.
            return null;
        }

        /// <summary>
        /// Get the default non-Steam path to KSP on macOS
        /// </summary>
        /// <returns>
        /// "/Applications/Kerbal Space Program" if it exists and we're on a Mac, else null
        /// </returns>
        public string MacPath()
        {
            if (Platform.IsMac)
            {
                string installPath = Path.Combine(
                    // This is "/Applications" in Mono on Mac
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "Kerbal Space Program"
                );
                return Directory.Exists(installPath) ? installPath : null;
            }
            return null;
        }

        public string PrimaryModDirectoryRelative => "GameData";
        public string[] AlternateModDirectoriesRelative => new string[] { };

        public string PrimaryModDirectory(GameInstance inst)
            => CKANPathUtils.NormalizePath(
                Path.Combine(inst.GameDir(), PrimaryModDirectoryRelative));

        public string[] StockFolders => new string[]
        {
            "GameData/Squad",
            "GameData/SquadExpansion",
            "KSP_Data",
            "KSP_x64_Data",
            "KSPLauncher_Data",
            "Launcher_Data",
            "MonoBleedingEdge",
            "PDLauncher",
        };

        public string[] ReservedPaths => new string[]
        {
            "GameData", "Ships", "Missions"
        };

        public string[] CreateableDirs => new string[]
        {
            "GameData", "Tutorial", "Scenarios", "Missions", "Ships/Script"
        };

        public string[] AutoRemovableDirs => new string[]
        {
            "@thumbs"
        };

        /// <summary>
        /// Checks the path against a list of reserved game directories
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsReservedDirectory(GameInstance inst, string path)
            => path == inst.GameDir() || path == inst.CkanDir()
            || path == PrimaryModDirectory(inst)
            || path == Missions(inst)
            || path == Scenarios(inst) || path == Tutorial(inst)
            || path == Ships(inst)     || path == ShipsThumbs(inst)
            || path == ShipsVab(inst)  || path == ShipsThumbsVAB(inst)
            || path == ShipsSph(inst)  || path == ShipsThumbsSPH(inst)
            || path == ShipsScript(inst);

        public bool AllowInstallationIn(string name, out string path)
            => allowedFolders.TryGetValue(name, out path);

        public void RebuildSubdirectories(string absGameRoot)
        {
            string[][] FoldersToCheck = {
                new string[] { "Ships"                   },
                new string[] { "Ships", "VAB"            },
                new string[] { "Ships", "SPH"            },
                new string[] { "Ships", "@thumbs"        },
                new string[] { "Ships", "@thumbs", "VAB" },
                new string[] { "Ships", "@thumbs", "SPH" },
                new string[] { "saves"                   },
                new string[] { "saves", "scenarios"      },
                new string[] { "saves", "training"       },
            };
            foreach (string[] sRelativePath in FoldersToCheck)
            {
                string sAbsolutePath = Path.Combine(absGameRoot, Path.Combine(sRelativePath));
                if (!Directory.Exists(sAbsolutePath))
                {
                    Directory.CreateDirectory(sAbsolutePath);
                }
            }
        }

        public string DefaultCommandLine(string path)
            => Platform.IsMac
                ? "./KSP.app/Contents/MacOS/KSP"
                : string.Format(Platform.IsUnix ? "./{0} -single-instance"
                                                : "{0} -single-instance",
                                InstanceAnchorFiles.FirstOrDefault(f =>
                                    File.Exists(Path.Combine(path, f)))
                                ?? InstanceAnchorFiles.First());

        public string[] AdjustCommandLine(string[] args, GameVersion installedVersion)
        {
            // -single-instance crashes KSP 1.8 to KSP 1.11 on Linux
            // https://issuetracker.unity3d.com/issues/linux-segmentation-fault-when-running-a-built-project-with-single-instance-argument
            if (Platform.IsUnix)
            {
                var brokenVersionRange = new GameVersionRange(
                    new GameVersion(1, 8),
                    new GameVersion(1, 11)
                );
                args = filterCmdLineArgs(args, installedVersion, brokenVersionRange, "-single-instance");
            }
            return args;
        }

        public IDlcDetector[] DlcDetectors => new IDlcDetector[]
        {
            new BreakingGroundDlcDetector(),
            new MakingHistoryDlcDetector(),
        };

        public void RefreshVersions()
        {
            ServiceLocator.Container.Resolve<IKspBuildMap>().Refresh();
            versions = null;
        }

        private List<GameVersion> versions;

        private readonly object versionMutex = new object();

        public List<GameVersion> KnownVersions
        {
            get
            {
                if (versions == null)
                {
                    lock (versionMutex)
                    {
                        if (versions == null)
                        {
                            // There's a lot of duplicate real versions with different build IDs,
                            // skip all those extra checks when we use these
                            versions = ServiceLocator.Container
                                                     .Resolve<IKspBuildMap>()
                                                     .KnownVersions
                                                     .Select(v => v.WithoutBuild)
                                                     .Distinct()
                                                     .OrderBy(v => v)
                                                     .ToList();
                        }
                    }
                }
                return versions;
            }
        }

        public GameVersion[] EmbeddedGameVersions
            => JsonConvert.DeserializeObject<JBuilds>(
                new StreamReader(Assembly.GetExecutingAssembly()
                                         .GetManifestResourceStream("CKAN.builds-ksp.json"))
                    .ReadToEnd())
                .Builds
                .Select(b => GameVersion.Parse(b.Value))
                .ToArray();

        public GameVersion[] ParseBuildsJson(JToken json)
            => json.ToObject<JBuilds>()
                   .Builds
                   .Select(b => GameVersion.Parse(b.Value))
                   .ToArray();

        public GameVersion DetectVersion(DirectoryInfo where)
            => ServiceLocator.Container
                .ResolveKeyed<IGameVersionProvider>(GameVersionSource.BuildId)
                .TryGetVersion(where.FullName, out GameVersion verFromId)
                    ? verFromId
                    : ServiceLocator.Container
                        .ResolveKeyed<IGameVersionProvider>(GameVersionSource.Readme)
                        .TryGetVersion(where.FullName, out GameVersion verFromReadme)
                            ? verFromReadme
                            : null;

        public string CompatibleVersionsFile => "compatible_ksp_versions.json";

        public string[] InstanceAnchorFiles =>
            // KSP.app is a directory :(
              Platform.IsMac     ? new string[] { "buildID64.txt", "buildID.txt" }
            : Platform.IsUnix    ? new string[] { "KSP.x86_64",    "KSP.x86" }
            :                      new string[] { "KSP_x64.exe",   "KSP.exe" };

        public Uri DefaultRepositoryURL => new Uri("https://github.com/KSP-CKAN/CKAN-meta/archive/master.tar.gz");

        public Uri RepositoryListURL => new Uri("https://raw.githubusercontent.com/KSP-CKAN/CKAN-meta/master/repositories.json");

        public Uri MetadataBugtrackerURL => new Uri("https://github.com/KSP-CKAN/NetKAN/issues/new/choose");

        private static string Missions(GameInstance inst)
            => CKANPathUtils.NormalizePath(Path.Combine(inst.GameDir(), "Missions"));

        private static string Ships(GameInstance inst)
            => CKANPathUtils.NormalizePath(Path.Combine(inst.GameDir(), "Ships"));

        private static string ShipsVab(GameInstance inst)
            => CKANPathUtils.NormalizePath(Path.Combine(Ships(inst), "VAB"));

        private static string ShipsSph(GameInstance inst)
            => CKANPathUtils.NormalizePath(Path.Combine(Ships(inst), "SPH"));

        private static string ShipsThumbs(GameInstance inst)
            => CKANPathUtils.NormalizePath(Path.Combine(Ships(inst), "@thumbs"));

        private static string ShipsThumbsSPH(GameInstance inst)
            => CKANPathUtils.NormalizePath(Path.Combine(ShipsThumbs(inst), "SPH"));

        private static string ShipsThumbsVAB(GameInstance inst)
            => CKANPathUtils.NormalizePath(Path.Combine(ShipsThumbs(inst), "VAB"));

        private static string ShipsScript(GameInstance inst)
            => CKANPathUtils.NormalizePath(Path.Combine(Ships(inst), "Script"));

        private static string Tutorial(GameInstance inst)
            => CKANPathUtils.NormalizePath(Path.Combine(inst.GameDir(), "saves", "training"));

        private static string Scenarios(GameInstance inst)
            => CKANPathUtils.NormalizePath(Path.Combine(inst.GameDir(), "saves", "scenarios"));

        private readonly Dictionary<string, string> allowedFolders = new Dictionary<string, string>
        {
            { "Tutorial",          "saves/training"    },
            { "Scenarios",         "saves/scenarios"   },
            { "Missions",          "Missions"          },
            { "Ships",             "Ships"             },
            { "Ships/VAB",         "Ships/VAB"         },
            { "Ships/SPH",         "Ships/SPH"         },
            { "Ships/@thumbs",     "Ships/@thumbs"     },
            { "Ships/@thumbs/VAB", "Ships/@thumbs/VAB" },
            { "Ships/@thumbs/SPH", "Ships/@thumbs/SPH" },
            { "Ships/Script",      "Ships/Script"      }
        };

        /// <summary>
        /// Finds the KSP path under a Steam Library. Returns null if the folder cannot be located.
        /// </summary>
        /// <param name="steamPath">Steam Library Path</param>
        /// <returns>The KSP path.</returns>
        private static string KSPDirectory(string steamPath)
        {
            // There are several possibilities for the path under Linux.
            // Try with the uppercase version.
            string installPath = Path.Combine(steamPath, "SteamApps", "common", "Kerbal Space Program");

            if (Directory.Exists(installPath))
            {
                return installPath;
            }

            // Try with the lowercase version.
            installPath = Path.Combine(steamPath, "steamapps", "common", "Kerbal Space Program");

            return Directory.Exists(installPath) ? installPath : null;
        }

        /// <summary>
        /// If the installed game version is in the given range,
        /// return the given array without the given parameter,
        /// otherwise return the array as-is.
        /// </summary>
        /// <param name="args">Command line parameters to check</param>
        /// <param name="crashyKspRange">Game versions that should not use this parameter</param>
        /// <param name="parameter">The parameter to remove on version match</param>
        /// <returns>
        /// args or args minus parameter
        /// </returns>
        private static string[] filterCmdLineArgs(string[] args, GameVersion installedVersion, GameVersionRange crashyKspRange, string parameter)
        {
            var installedRange = installedVersion.ToVersionRange();
            if (crashyKspRange.IntersectWith(installedRange) != null
                && args.Contains(parameter))
            {
                log.DebugFormat(
                    "Parameter {0} found on incompatible KSP version {1}, pruning",
                    parameter,
                    installedVersion.ToString());
                return args.Where(s => s != parameter).ToArray();
            }
            return args;
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(KerbalSpaceProgram));
    }
}
