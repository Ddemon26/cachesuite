using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cache_Editor_API
{
	public class Cache
	{
		public FileStream DataFile { get; set; }
		public Archive[] Archives { get; set; }
		public List<SubArchive> SubArchives { get; set; }

                private static readonly string[] DefaultArchiveNames = { "Sub-Archives", "Models", "Animations", "Midis", "Maps" };
                public static string[] ArchiveNames { get; private set; } = DefaultArchiveNames;
                public static string[] SubNames = { "Sub-archive 0", "Title", "Config", "Interface", "Media", "Versions", "Textures", "Chat", "Sounds" };

                public Cache()
                {
                        Archives = Array.Empty<Archive>();
                        SubArchives = new List<SubArchive>();
                }

                public void LoadCache(string folder)
                {
                        DataFile = new FileStream(Path.Combine(folder, "main_file_cache.dat"), FileMode.Open);

                        var idxFiles = Directory.GetFiles(folder, "main_file_cache.idx*")
                                             .Select(f => new { Path = f, Index = int.Parse(Path.GetFileName(f).Substring("main_file_cache.idx".Length)) })
                                             .OrderBy(f => f.Index)
                                             .ToArray();

                        Archives = new Archive[idxFiles.Length];
                        ArchiveNames = new string[idxFiles.Length];

                        for (int i = 0; i < idxFiles.Length; i++)
                        {
                                string name = i < DefaultArchiveNames.Length ? DefaultArchiveNames[i] : $"Archive {i}";
                                ArchiveNames[i] = name;
                                Archives[i] = new Archive(name, DataFile, new FileStream(idxFiles[i].Path, FileMode.Open));
                                Archives[i].ArchiveIndex = i;
                        }
                }

		public void WriteFile(CacheItemNode node, byte[] data)
		{
			if(node.SubArchive != -1)
			{
				SubArchives[node.SubArchive].WriteFile(node.FileIndex, data);
			}
			else if(node.Archive != -1)
			{
				Archives[node.Archive].WriteFile(node.FileIndex, data, data.Length);
			}
		}

		public void Close()
		{
			try
			{
				DataFile.Close();
			}
			catch (Exception) { }

			if (Archives == null)
				return;

			for (int i = 0; i < Archives.Length; i++)
			{
				try
				{
					Archives[i].IndexFile.Close();
				}
				catch (Exception) { }
			}

			Archives = null;
			SubArchives = null;
		}
	}
}
