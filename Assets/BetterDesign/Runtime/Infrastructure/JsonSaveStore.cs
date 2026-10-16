using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using BetterDesign.Core;
using UnityEngine;

namespace BetterDesign
{
    public interface ISaveStore
    {
        bool TrySave(SaveData data, out string message);
        bool TryLoad(out SaveData data, out string message);
    }
    public sealed class JsonSaveStore : ISaveStore
    {
        [Serializable] sealed class Envelope { public string payload, checksum; }
        readonly string file;
        public JsonSaveStore(string directory) { file = Path.Combine(directory, "better-design-v1.json"); }
        static string Digest(string text)
        {
            using (var sha = SHA256.Create())
                return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(text)));
        }
        public bool TrySave(SaveData data, out string message)
        {
            if (!GameModel.IsValid(data)) { message = "Save rejected: invalid snapshot."; return false; }
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
                string json = JsonUtility.ToJson(data);
                string envelope = JsonUtility.ToJson(new Envelope { payload = json, checksum = Digest(json) });
                // Flush temp before atomically replacing the primary; old primary remains as backup.
                using (var stream = new FileStream(file + ".tmp", FileMode.Create, FileAccess.Write))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(envelope);
                    stream.Write(bytes, 0, bytes.Length); stream.Flush(true);
                }
                if (File.Exists(file)) File.Replace(file + ".tmp", file, file + ".bak");
                else File.Move(file + ".tmp", file);
                message = "Checkpoint saved."; return true;
            }
            catch (IOException ex) { message = "Save I/O error: " + ex.Message; return false; }
            catch (UnauthorizedAccessException ex) { message = "Save access error: " + ex.Message; return false; }
            catch (PlatformNotSupportedException ex) { message = "Atomic saves unavailable: " + ex.Message; return false; }
        }
        bool Read(string name, out SaveData data)
        {
            data = null;
            if (!File.Exists(name) || new FileInfo(name).Length > 65536) return false;
            var envelope = JsonUtility.FromJson<Envelope>(File.ReadAllText(name));
            if (envelope == null || string.IsNullOrEmpty(envelope.payload) || Digest(envelope.payload) != envelope.checksum) return false;
            data = JsonUtility.FromJson<SaveData>(envelope.payload);
            return GameModel.IsValid(data);
        }
        public bool TryLoad(out SaveData data, out string message)
        {
            data = null;
            try
            {
                try { if (Read(file, out data)) { message = "Checkpoint loaded."; return true; } }
                catch (ArgumentException) { /* Invalid JSON: try backup. */ }
                try { if (Read(file + ".bak", out data)) { message = "Recovered previous checkpoint from backup."; return true; } }
                catch (ArgumentException) { }
                data = null; message = "No valid checkpoint. Current session kept."; return false;
            }
            catch (IOException ex) { message = "Load I/O error: " + ex.Message; return false; }
            catch (UnauthorizedAccessException ex) { message = "Load access error: " + ex.Message; return false; }
        }
    }
}
