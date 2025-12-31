using System.Collections.Immutable;
using YamlDotNet.Serialization;

namespace CopyrightTool
{
	internal class Program
	{
		static void Main(string[] args)
		{
			var argc = args.Length;
			ProgramConfig? cfg = null;

			switch (argc)
			{
				case int n when n <= 0:
					Console.WriteLine("parameters does not exist.");
					PrintHelp();
					return;

				case int n when n == 1:
					if (!args[0].Equals("--init", StringComparison.OrdinalIgnoreCase))
					{
						Console.WriteLine("parameters invalid.");
						PrintHelp();
					}
					else
					{
						CreateConfigFile(System.Environment.CurrentDirectory);
						Console.WriteLine("Please configure created config file.");
					}
					return;

				case int n when n >= 2:
					if (!args[0].Equals("--run", StringComparison.OrdinalIgnoreCase))
					{
						Console.WriteLine("parameters invalid.");
						PrintHelp();
						return;
					}
					else
					{
						try
						{
							cfg = LoadConfigFile(args[1]);
						}
						catch (Exception _e)
						{
							Console.WriteLine("Config file load failed.");
							Console.WriteLine(_e.Message);
							return;
						}
					}
						break;
			}

			if (!cfg.HasValue)
			{
				return;
			}

			if (!cfg.Value.IsValid())
			{
				return;
			}

            string configFilePath = Path.GetFullPath(args[1]);
            string? configFileDirectory = Path.GetDirectoryName(configFilePath);
            if (string.IsNullOrEmpty(configFileDirectory))
            {
                Console.WriteLine("Config file directory could not be determined.");
                return;
            }
            string absoluteRootPath = Path.GetFullPath(Path.Combine(configFileDirectory, cfg.Value.rootPath));
            Console.WriteLine($"Config file directory: {configFileDirectory}");
            Console.WriteLine($"Resolved root path to search: {absoluteRootPath}");

            IEnumerable<string> paths;

			try
			{
				paths =
				Directory.GetFiles(cfg.Value.rootPath, "*.*", SearchOption.AllDirectories)
				.Where(str => !cfg.Value.excludePaths.Any(exclude => exclude != null && str.Contains(exclude)))
				.Where(str => cfg.Value.fileExtensions.Contains(Path.GetExtension(str)));
			}
			catch
			{
                Console.WriteLine($"Root path invalid or not found: {absoluteRootPath}");
                return;
			}

			if (paths == null)
			{
				Console.WriteLine("No files found.");
				return;
            }

			Dictionary<FileInfo, string> revisionFiles = [];
			DateTime currentDateTime = DateTime.Now;
			int currentYear = currentDateTime.Year;

			string copyright = string.Format($"// Copyright (c) {currentYear} {cfg.Value.copyright}.");

			foreach (var path in paths)
			{
				var content = File.ReadAllText(path, System.Text.Encoding.UTF8);
				string[] lines = content.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);

				if (lines.Length > 0)
				{
					bool hasCopyright = lines[0].Contains("Copyright (c)", StringComparison.OrdinalIgnoreCase);
					List<string> updatedLines = [.. lines];
					int insertionIndex = 0;

					if (hasCopyright)
					{
						updatedLines.RemoveAt(0);
						updatedLines.Insert(0, copyright);
						insertionIndex = 0;
					}
					else
					{
						updatedLines.Insert(0, copyright);
						insertionIndex = 0;
					}

					int nextLineIndex = insertionIndex + 1;
					if (nextLineIndex < updatedLines.Count)
					{
                        string nextLine = updatedLines[nextLineIndex];
                        if (!string.IsNullOrWhiteSpace(nextLine))
						{
                            updatedLines.Insert(nextLineIndex, string.Empty);
                        }
                    }

					content = string.Join(Environment.NewLine, updatedLines);

                    revisionFiles.Add(new FileInfo(path), content);

                }
			}

			foreach (var kvp in revisionFiles)
			{
				File.WriteAllText(kvp.Key.FullName, kvp.Value, System.Text.Encoding.UTF8);
				Console.WriteLine($"Revised: {kvp.Key.FullName}");
            }
        }

		static void PrintHelp()
		{
			Console.WriteLine("Usage:");
			Console.WriteLine("\t--init: Create config file in running path.");
			Console.WriteLine("\t--run [config file path]: Run program");
		}

		static void CreateConfigFile(string path)
		{
			var cfg = new ProgramConfig
			{
				copyright = "Person",
				rootPath = "",
				excludePaths = [""],
				fileExtensions = [".cs", ".cpp", ".h", ".c", ".go", ".hpp"]
			};

			var serializer = new SerializerBuilder().Build();
			var yaml = serializer.Serialize(cfg);
			File.WriteAllText(Path.Combine(path, "copyrighttool_config.yaml"), yaml);

			cfg.excludePaths ??= [];
			cfg.fileExtensions ??= [];

            Console.WriteLine($"Config file created in {Path.Combine(path, "copyrighttool_config.yaml")}");
		}

		static ProgramConfig LoadConfigFile(string path)
		{
			ProgramConfig loadedConfig;

			var yamlContent = File.ReadAllText(path);
			var deserializer = new DeserializerBuilder().Build();
			loadedConfig = deserializer.Deserialize<ProgramConfig>(yamlContent);

			Console.WriteLine($"Config loaded:\n" + loadedConfig.copyright);
			Console.WriteLine($"Root Path: {loadedConfig.rootPath}");

			return loadedConfig;
		}

		struct ProgramConfig
		{
			public string copyright = "";
			public string rootPath = "";
			public string[] excludePaths = [];
			public string[] fileExtensions = [];

            public ProgramConfig() { }

			public readonly bool IsValid()
			{
				if (string.IsNullOrWhiteSpace(copyright))
				{
					Console.WriteLine("Copyright field is empty.");
                    return false;
				}

				if (string.IsNullOrWhiteSpace(rootPath))
				{
					Console.WriteLine("Root path is empty.");
                    return false;
				}

				if (fileExtensions.Length == 0)
				{
					Console.WriteLine("File extensions is null or empty.");
                    return false;
				}

				return true;
			}
		}
	}
}
