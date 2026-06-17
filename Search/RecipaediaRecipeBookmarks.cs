using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Engine;
using Game;
using RecipaediaEX.Implementation;

namespace RecipaediaEX.Search {
    /// <summary>合成助手配方收藏（用户级偏好，非世界存档）。</summary>
    public static class RecipaediaRecipeBookmarks {
        static readonly HashSet<string> m_keys = new(StringComparer.Ordinal);
        static bool m_loaded;

        public static bool IsBookmarked(IRecipe recipe) {
            EnsureLoaded();
            return m_keys.Contains(GetRecipeKey(recipe));
        }

        public static bool Toggle(IRecipe recipe) {
            EnsureLoaded();
            string key = GetRecipeKey(recipe);
            if (m_keys.Remove(key)) {
                Save();
                return false;
            }
            m_keys.Add(key);
            Save();
            return true;
        }

        public static string GetRecipeKey(IRecipe recipe) {
            var sb = new StringBuilder(128);
            sb.Append(recipe.GetType().FullName);
            sb.Append('#');
            sb.Append(recipe.DisplayOrder);
            int[] results = recipe.GetExtraValue(RecipeExtraKeys.MatchedResultBlockValues, Array.Empty<int>());
            if (results.Length > 0) {
                sb.Append('#');
                sb.Append(string.Join(',', results));
            }
            if (recipe is FormattedRecipe formatted) {
                sb.Append('#');
                sb.Append(formatted.ResultValue);
                sb.Append(':');
                sb.Append(formatted.ResultCount);
                sb.Append(':');
                if (formatted.Ingredients != null) sb.Append(string.Join('|', formatted.Ingredients));
            }
            return sb.ToString();
        }

        static void EnsureLoaded() {
            if (m_loaded) return;
            m_loaded = true;
            m_keys.Clear();
            string path = GetPath();
            if (!Storage.FileExists(path)) return;
            try {
                using Stream stream = Storage.OpenFile(path, OpenFileMode.Read);
                using StreamReader reader = new(stream);
                while (reader.ReadLine() is { } line) {
                    line = line.Trim();
                    if (line.Length > 0) m_keys.Add(line);
                }
            }
            catch (Exception ex) {
                Log.Warning("[RecipaediaEX] Failed to load recipe bookmarks: " + ex.Message);
            }
        }

        static void Save() {
            try {
                string path = GetPath();
                using Stream stream = Storage.OpenFile(path, OpenFileMode.Create);
                using StreamWriter writer = new(stream);
                foreach (string key in m_keys) writer.WriteLine(key);
            }
            catch (Exception ex) {
                Log.Warning("[RecipaediaEX] Failed to save recipe bookmarks: " + ex.Message);
            }
        }

        static string GetPath() => ModsManager.ExternalPath + "/RecipaediaEXRecipeBookmarks.txt";
    }
}
