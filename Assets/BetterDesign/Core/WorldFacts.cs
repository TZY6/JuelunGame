using System;
using System.Collections.Generic;
namespace BetterDesign.Core
{
    [Serializable] public struct FactRecord { public FactKind kind; public string id; }
    public sealed class WorldFacts
    {
        readonly List<FactRecord> records = new List<FactRecord>(16);
        public bool Contains(FactKind kind,string id)
        {
            for (int i = 0; i < records.Count; i++) if (records[i].kind == kind && records[i].id == id) return true;
            return false;
        }
        public bool Record(FactKind kind,string id)
        {
            if (!ValidKey(kind,id) || Contains(kind,id) || records.Count == 64) return false;
            records.Add(new FactRecord { kind = kind,id = id }); return true;
        }
        static bool ValidKey(FactKind kind,string id)
        {
            return (kind == FactKind.LocationVisited || kind == FactKind.DialogueCompleted) && !string.IsNullOrEmpty(id) && id.Length <= 64;
        }
        public FactRecord[] Capture() { return records.ToArray(); }
        public static bool IsValid(FactRecord[] data)
        {
            if (data == null || data.Length > 64) return false;
            for (int i = 0; i < data.Length; i++)
            {
                if (!ValidKey(data[i].kind,data[i].id)) return false;
                for (int j = 0; j < i; j++) if (data[i].kind == data[j].kind && data[i].id == data[j].id) return false;
            }
            return true;
        }
        public void Restore(FactRecord[] data)
        {
            if (!IsValid(data)) throw new ArgumentException("Invalid world facts.");
            records.Clear(); records.AddRange(data);
        }
    }
}
