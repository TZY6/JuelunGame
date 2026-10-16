using BetterDesign.Core;
using UnityEngine;

namespace BetterDesign
{
    [DisallowMultipleComponent]
    public sealed class PersistenceManager : MonoBehaviour, ISaveStore
    {
        JsonSaveStore store;
        JsonSaveStore Store
        {
            get { if (store == null) store = new JsonSaveStore(Application.persistentDataPath); return store; }
        }
        public bool TrySave(SaveData data, out string message) { return Store.TrySave(data,out message); }
        public bool TryLoad(out SaveData data, out string message) { return Store.TryLoad(out data,out message); }
    }
}
