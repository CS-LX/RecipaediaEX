using System;
using System.Collections.Generic;
using System.IO;
using Engine;
using Game;

namespace RecipaediaEX.Search {
    /// <summary>图鉴搜索历史（UI 偏好，非世界存档）。</summary>
    public static class RecipaediaSearchHistory {
        public const int MaxEntries = 20;
        static readonly List<string> s_entries = [];
        static bool m_loaded;

        public static IReadOnlyList<string> Entries {
            get {
                EnsureLoaded();
                return s_entries;
            }
        }

        public static void EnsureLoaded() {
            if (m_loaded) return;
            m_loaded = true;
            s_entries.Clear();
            string path = GetPath();
            if (!Storage.FileExists(path)) return;
            try {
                using Stream stream = Storage.OpenFile(path, OpenFileMode.Read);
                using StreamReader reader = new(stream);
                while (reader.ReadLine() is { } line) {
                    line = line.Trim();
                    if (line.Length > 0) s_entries.Add(line);
                }
            }
            catch (Exception ex) {
                Log.Warning("[RecipaediaEX] Failed to load search history: " + ex.Message);
            }
        }

        public static void Add(string query) {
            query = query?.Replace("\n", string.Empty).Trim() ?? string.Empty;
            if (query.Length == 0) return;
            EnsureLoaded();
            s_entries.RemoveAll(e => string.Equals(e, query, System.StringComparison.OrdinalIgnoreCase));
            s_entries.Insert(0, query);
            while (s_entries.Count > MaxEntries) s_entries.RemoveAt(s_entries.Count - 1);
            Save();
        }

        static void Save() {
            try {
                string path = GetPath();
                using Stream stream = Storage.OpenFile(path, OpenFileMode.Create);
                using StreamWriter writer = new(stream);
                foreach (string entry in s_entries) writer.WriteLine(entry);
            }
            catch (Exception ex) {
                Log.Warning("[RecipaediaEX] Failed to save search history: " + ex.Message);
            }
        }

        static string GetPath() => ModsManager.ExternalPath + "/RecipaediaEXSearchHistory.txt";
    }
}
