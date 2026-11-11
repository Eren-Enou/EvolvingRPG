using System;
using System.IO;
using System.Text;
using UnityEngine;
namespace EvolvingRPG.Progression
{
    // Local explicit safe-boundary saves. Does not serialize Unity objects or encounter state.
    public sealed class CharacterSaveStore
    {
        readonly string directory;
        static readonly object gate = new object();
        const int MaximumBytes = 65536;
        public CharacterSaveStore(string directory) { this.directory = Path.GetFullPath(directory); }
        public string Location => directory;
        public readonly struct SlotPreview
        {
            public readonly string Slot, CharacterId, Heavy, Detail;
            public readonly bool CanLoad, CanSave, HasLatest, UsesBackup, RetaliationActive, Unresolved;
            public readonly int Revision;

            internal SlotPreview(string slot, CharacterDevelopmentState state, string detail, bool canSave, bool hasLatest, bool usesBackup)
            {
                Slot = slot; CharacterId = state?.CharacterId; Heavy = state?.Heavy; Detail = detail;
                CanLoad = state != null; CanSave = canSave; HasLatest = hasLatest; UsesBackup = usesBackup;
                RetaliationActive = state?.RetaliationActive == true; Unresolved = state?.HasUnresolvedContent == true;
                Revision = state?.Revision ?? -1;
            }
        }
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
                if (data.schemaVersion != 1 && data.schemaVersion != 2 && data.schemaVersion != 3) { protectedRecord = true; message = "Unsupported schema " + data.schemaVersion + "; original preserved."; return false; }
                if (data.schemaVersion < 3 && (data.initialPoints != 0 || data.relayMilestone))
                { protectedRecord = true; message = "Unexpected mission data in legacy schema; original preserved."; return false; }
                if (data.schemaVersion == 1 && data.heavyExperience != null && data.heavyExperience.Length > 0)
                { protectedRecord = true; message = "Unexpected history in legacy schema; original preserved pending migration."; return false; }
                if (data.heavyExperience != null) foreach (string fact in data.heavyExperience)
                    if (fact != null && !HeavyExperienceCatalog.Known(fact))
                    { protectedRecord = true; message = "Unknown experience ID; original preserved pending migration."; return false; }
                // An unknown rule is content we cannot interpret, not corruption to roll back.
                // Falling through to an older backup could silently replace newer knowledge.
                if (!string.IsNullOrEmpty(data.discoveryId) && data.discoveryId != DevelopmentIds.Retaliation)
                { protectedRecord = true; message = "Unknown discovery rule " + data.discoveryId + "; original preserved pending migration."; return false; }
                bool valid = CharacterDevelopmentState.TryRestore(data, out state, out message);
                protectedRecord = valid && state.HasUnresolvedContent;
                return valid;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException)
            { message = "Cannot read save: " + e.Message; return false; }
        }
        // Use the same read and fallback rules as Load. Inspect never repairs, migrates on disk, or applies a character.
        public SlotPreview Inspect(string slot)
        {
            lock (gate)
            {
                string path = SlotPath(slot);
                bool hasLatest = File.Exists(path), hasBackup = File.Exists(path + ".bak");
                bool latestOk = Read(path, out var latest, out string latestReason, out bool latestProtected);
                bool backupOk = Read(path + ".bak", out var backup, out string backupReason, out bool backupProtected);
                bool canSave = (!hasLatest || latestOk) && !latestProtected && !backupProtected;
                if (latestOk)
                    return new SlotPreview(slot, latest, latestProtected ? latestReason
                        : backupProtected ? "Protected backup: saving is disabled. " + backupReason : "", canSave, hasLatest, false);
                if (latestProtected) return new SlotPreview(slot, null, latestReason, false, hasLatest, false);
                if (backupOk)
                    return new SlotPreview(slot, backup, "Load recovers the prior backup; it does not repair the latest file."
                        + (backupProtected ? " " + backupReason : ""), canSave, hasLatest, true);
                string detail = !hasLatest && !hasBackup ? "" : latestReason + " Backup: " + backupReason;
                return new SlotPreview(slot, null, detail, canSave, hasLatest, false);
            }
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
