using System;
using System.IO;
using System.Text;
using UnityEngine;
namespace EvolvingRPG.Progression
{
    [Serializable]
    public sealed class CharacterSaveData
    {
        public int schemaVersion;
        public string characterId;
        public int revision, buildRevision;
        public string primary, heavy, discoveryId;
        public int primaryPaid, retaliationPaid, exchanges, grantedPoints, pointBalance;
        public bool retaliationLearned, retaliationActive;
    }

    // Local explicit safe-boundary saves. Does not serialize Unity objects or encounter state.
    public sealed class CharacterSaveStore
    {
        readonly string directory;
        static readonly object gate = new object();
        const int MaximumBytes = 65536;
        public CharacterSaveStore(string directory) { this.directory = Path.GetFullPath(directory); }
        public string Location => directory;
        string SlotPath(string slot)
        {
            if (slot != "A" && slot != "B") throw new ArgumentException("Only experimental slots A and B exist.");
            return Path.Combine(directory, "character-" + slot.ToLowerInvariant() + ".json");
        }
        static bool Read(string path, out CharacterDevelopmentState state, out string message, out bool protectedRecord)
        {
            state = null; protectedRecord = false;
            if (!File.Exists(path)) { message = "No save exists."; return false; }
            try
            {
                if (new FileInfo(path).Length > MaximumBytes) { protectedRecord = true; message = "Oversized save preserved; no load or overwrite."; return false; }
                string json = File.ReadAllText(path, Encoding.UTF8);
                if (!json.TrimStart().StartsWith("{") || !json.TrimEnd().EndsWith("}")) { message = "Malformed save."; return false; }
                var data = JsonUtility.FromJson<CharacterSaveData>(json);
                if (data == null) { message = "Empty save."; return false; }
                if (data.schemaVersion != 1) { protectedRecord = true; message = "Unsupported schema " + data.schemaVersion + "; original preserved."; return false; }
                bool valid = CharacterDevelopmentState.TryRestore(data, out state, out message);
                protectedRecord = valid && state.HasUnresolvedContent;
                return valid;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException)
            { message = "Cannot read save: " + e.Message; return false; }
        }
        public bool Load(string slot, out CharacterDevelopmentState state, out string message)
        {
            lock (gate)
            {
                state = null;
                string path;
                try { path = SlotPath(slot); } catch (ArgumentException e) { message = e.Message; return false; }
                if (Read(path, out state, out message, out bool protectedRecord)) return true;
                if (protectedRecord) return false; // Never silently downgrade a future record to an older backup.
                string primaryMessage = message;
                if (Read(path + ".bak", out state, out string backupMessage, out _))
                { message = "Recovered prior backup. Latest file preserved: " + primaryMessage + " Save to the other slot; automatic repair is deliberately disabled. " + backupMessage; return true; }
                message = primaryMessage + " Backup unavailable: " + backupMessage; return false;
            }
        }
        public bool Save(string slot, CharacterDevelopmentState state, out string message)
        {
            lock (gate)
            {
                string temporary = null;
                try
                {
                    string path = SlotPath(slot);
                    if (state == null || state.HasUnresolvedContent) { message = "Unresolved character content cannot be saved over original history."; return false; }
                    if (File.Exists(path) && (!Read(path, out _, out string reason, out bool protect) || protect))
                    { message = "Existing slot preserved; save to another healthy slot. " + reason; return false; }
                    if (File.Exists(path + ".bak"))
                    {
                        Read(path + ".bak", out _, out string backupReason, out bool backupProtect);
                        if (backupProtect) { message = "Protected backup preserved. " + backupReason; return false; }
                    }
                    var data = state.Export();
                    if (!CharacterDevelopmentState.TryRestore(data, out _, out message)) return false;
                    Directory.CreateDirectory(directory);
                    temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
                    byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data, true));
                    using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
                    if (!Read(temporary, out _, out message, out _)) return false;
                    if (File.Exists(path)) File.Replace(temporary, path, path + ".bak");
                    else File.Move(temporary, path);
                    temporary = null; message = "Saved character " + state.CharacterId.Substring(0, 8) + " to slot " + slot + "."; return true;
                }
                catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException || e is NotSupportedException)
                { message = "Save failed; in-memory character retained: " + e.Message; return false; }
                finally
                {
                    try { if (temporary != null && File.Exists(temporary)) File.Delete(temporary); }
                    catch (IOException) { /* Preserve any orphan temp rather than obscure the save result. */ }
                    catch (UnauthorizedAccessException) { }
                }
            }
        }
    }
}
