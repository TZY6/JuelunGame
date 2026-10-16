using BetterDesign.Core;
namespace BetterDesign
{
    public sealed class CheckpointCoordinator
    {
        readonly GameModel model;
        readonly LootController loot;
        readonly ActorController[] enemies;
        readonly ISaveStore store;
        public CheckpointCoordinator(GameModel model,LootController loot,ActorController[] enemies,ISaveStore store)
        {
            this.model = model; this.loot = loot; this.enemies = enemies; this.store = store;
        }
        public bool Save(int enemyCount,out string message)
        {
            if (model.Hp == 0) { message = "Defeated runs cannot overwrite checkpoints."; return false; }
            var data = model.Capture(); data.loot = loot.Capture();
            int count = 0;
            for (int i = 0; i < enemyCount; i++) if (enemies[i].IsAlive) count++;
            data.enemies = new EnemySnapshot[count];
            for (int i = 0,j = 0; i < enemyCount; i++)
                if (enemies[i].IsAlive)
                    data.enemies[j++] = new EnemySnapshot { x = enemies[i].Position.x,y = enemies[i].Position.y,hp = enemies[i].Health };
            return store.TrySave(data,out message);
        }
        public bool TryLoad(out SaveData data,out string message)
        {
            if (!store.TryLoad(out data,out message)) return false;
            if (GameModel.IsValid(data)) return true;
            data = null; message = "Invalid checkpoint. Current session kept."; return false;
        }
    }
}
