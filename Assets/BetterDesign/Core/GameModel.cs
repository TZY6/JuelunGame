using System;

namespace BetterDesign.Core
{
    [Serializable] public sealed class EnemySnapshot { public float x,y; public int hp; }
    [Serializable]
    public sealed class SaveData
    {
        public int version = 3;
        public string sceneId = "sanctuary";
        public FactRecord[] facts = new FactRecord[0];
        public ProgressionSaveData progression = new ProgressionSaveData();
        public int questStartKills;
        public int hp = 100, coins, kills, wave = 1, potions = 3; // potions is a v1 migration field.
        public bool questAccepted,questClaimed,waveCleared;
        public float x = -5,y;
        public EnemySnapshot[] enemies = new EnemySnapshot[0];
        public ItemStack[] inventory = InitialInventory();
        public LootSnapshot[] loot = new LootSnapshot[0];
        static ItemStack[] InitialInventory()
        {
            var data = new ItemStack[InventoryModel.Capacity];
            data[0] = new ItemStack { item = ItemId.Supply,amount = 3 }; return data;
        }
    }
    public sealed class GameModel : IConditionFacts
    {
        readonly GameEvents events;
        int inventoryRevision, publishedGold, publishedInventory;
        public readonly WorldFacts Facts = new WorldFacts();
        public readonly ProgressionModel Progression = new ProgressionModel();
        public int QuestStartKills { get; private set; }
        public int QuestKills { get { return Math.Max(0,Kills - QuestStartKills); } }
        static readonly ConditionConfig Contract = new ConditionConfig { fact = FactKind.Kills, required = QuestTarget };
        public int Read(FactKind fact,string targetId)
        {
            switch (fact)
            {
                case FactKind.Kills: return QuestKills;
                case FactKind.Level: return Progression.Level;
                case FactKind.ItemCount:
                    ItemId item; return Enum.TryParse(targetId,out item) ? Inventory.Count(item) : 0;
                case FactKind.LocationVisited: case FactKind.DialogueCompleted: return Facts.Contains(fact,targetId) ? 1 : 0;
                default: return 0;
            }
        }
        public bool UpgradeMagic()
        {
            if (!Progression.TryUpgradeMagic()) return false;
            Changed(ChangeKind.Stats); return true;
        }
        public void EquipWeapon(int weapon) { Progression.Equip(weapon); Changed(ChangeKind.Stats); }
        public const int MaxHp = 100, QuestTarget = 6;
        public readonly ActorModel Player = new ActorModel();
        public readonly InventoryModel Inventory = new InventoryModel();
        public int Hp { get { return Player.Health; } }
        public int Potions { get { return Inventory.Count(ItemId.Supply); } }
        public int Coins { get; private set; }
        public int Kills { get; private set; }
        public int Wave { get; private set; } = 1;
        public bool QuestAccepted { get; private set; }
        public bool QuestClaimed { get; private set; }
        public bool WaveCleared { get; private set; }
        public GameModel(GameEvents events)
        {
            this.events = events; Player.Reset(MaxHp,-5,0); Inventory.TryAdd(ItemId.Supply,3); publishedInventory = Inventory.Revision;
        }
        void Changed(ChangeKind kind)
        {
            if (Coins != publishedGold) { publishedGold = Coins; events.GoldChanged.Publish(new GoldChanged(Coins)); }
            if (publishedInventory != Inventory.Revision || kind == ChangeKind.Restored)
            {
                publishedInventory = Inventory.Revision;
                events.InventoryChanged.Publish(new InventoryChanged(++inventoryRevision));
            }
            if (kind == ChangeKind.Quest || kind == ChangeKind.Restored)
                events.QuestProgressChanged.Publish(new QuestProgressChanged(QuestKills));
            events.Model.Publish(new ModelChanged(kind));
        }
        public void Damage(int amount) { Player.Damage(amount); Changed(ChangeKind.Stats); }
        public void EnemyKilled()
        {
            Kills++;
            int oldLevel = Progression.Level; Progression.AddExperience(10);
            Changed(ChangeKind.Quest);
            events.EnemyKilled.Publish(new EnemyKilled(Kills));
            if (oldLevel != Progression.Level) events.PlayerLevelUp.Publish(new PlayerLevelUp(Progression.Level));
        }
        public bool CanCollect(ItemId item,int amount)
        {
            return amount > 0 && (item == ItemId.Gold ? amount <= 100000000 - Coins : Inventory.CanAdd(item,amount));
        }
        public bool Collect(ItemId item,int amount)
        {
            if (!CanCollect(item,amount)) return false;
            if (item == ItemId.Gold)
            {
                if (amount > 100000000 - Coins) return false;
                Coins += amount;
            }
            else if (!Inventory.TryAdd(item,amount)) return false;
            Changed(ChangeKind.Inventory); events.ItemCollected.Publish(new ItemCollected(item,amount)); return true;
        }
        public bool UseItem(int index)
        {
            if (index < 0 || index >= InventoryModel.Capacity || Inventory[index].item != ItemId.Supply ||
                Hp == 0 || Hp == MaxHp || !Inventory.TryRemove(index,1)) return false;
            Player.Heal(40,MaxHp); Changed(ChangeKind.Inventory); return true;
        }
        public bool UsePotion() { return UseItem(Inventory.Find(ItemId.Supply)); }
        public bool MoveItem(int from,int to)
        {
            if (!Inventory.TryMove(from,to)) return false;
            Changed(ChangeKind.Inventory); return true;
        }
        public bool RemoveItem(int index,int amount)
        {
            if (!Inventory.TryRemove(index,amount)) return false;
            Changed(ChangeKind.Inventory); return true;
        }
        public bool BuyPotion()
        {
            if (Hp <= 0 || Coins < 10 || !Inventory.CanAdd(ItemId.Supply,1)) return false;
            Inventory.TryAdd(ItemId.Supply,1); Coins -= 10; Changed(ChangeKind.Inventory); return true;
        }
        public void SellMaterials()
        {
            int count = Inventory.Count(ItemId.Mushroom);
            if (count == 0 || Coins > 100000000 - count*2) { events.Notice.Publish("没有可出售的蘑菇材料。"); return; }
            for (int i = 0; i < InventoryModel.Capacity; i++)
                if (Inventory[i].item == ItemId.Mushroom) Inventory.TryRemove(i,Inventory[i].amount);
            Coins += count*2; Changed(ChangeKind.Inventory); events.Notice.Publish("已出售蘑菇材料，每个获得 2 金币。");
        }
        public void InteractQuest()
        {
            if (!QuestAccepted) { QuestAccepted = true; QuestStartKills = Kills; events.Notice.Publish("已接取契约：击败 6 名守卫。"); }
            else if (!QuestClaimed && ConditionEvaluator.Evaluate(Contract,this))
            {
                if (!Inventory.CanAdd(ItemId.Supply,2) || Coins > 99999950) { events.Notice.Publish("请先腾出背包空间再领取奖励。"); return; }
                QuestClaimed = true; Coins += 50; Inventory.TryAdd(ItemId.Supply,2);
                events.Notice.Publish("契约完成！获得 50 金币和 2 份治疗补给。");
            }
            else events.Notice.Publish(QuestClaimed ? "契约已完成，继续挑战下一波。" : "击败 6 名守卫后回到信标领取奖励。");
            Changed(ChangeKind.Quest);
        }
        public void ClearWave() { WaveCleared = true; Changed(ChangeKind.Wave); }
        public bool NextWave()
        {
            if (!WaveCleared || Hp <= 0 || Wave >= 10000) return false;
            Wave++; WaveCleared = false; Changed(ChangeKind.Wave); return true;
        }
        public SaveData Capture()
        {
            return new SaveData { facts = Facts.Capture(),progression = Progression.Capture(),questStartKills = QuestStartKills,hp = Hp,coins = Coins,kills = Kills,wave = Wave,potions = Potions,
                x = Player.X,y = Player.Y,inventory = Inventory.Capture(),
                questAccepted = QuestAccepted,questClaimed = QuestClaimed,waveCleared = WaveCleared };
        }
        static bool PositionValid(float x,float y)
        {
            return !float.IsNaN(x) && !float.IsNaN(y) && Math.Abs(x) <= 10 && Math.Abs(y) <= 6;
        }
        public static bool IsValid(SaveData data)
        {
            if (data == null || (data.version < 1 || data.version > 3) || data.hp < 0 || data.hp > MaxHp ||
                data.coins < 0 || data.coins > 100000000 || data.kills < 0 || data.kills > 10000000 ||
                data.wave < 1 || data.wave > 10000 || !PositionValid(data.x,data.y) ||
                data.enemies == null || data.enemies.Length > 16 || (data.waveCleared && data.enemies.Length != 0) ||
                (data.questClaimed && (!data.questAccepted || data.kills < QuestTarget))) return false;
            if (data.version == 3 && (!WorldFacts.IsValid(data.facts) || data.sceneId != "sanctuary" || !ProgressionModel.IsValid(data.progression) || data.questStartKills < 0 ||
                data.questStartKills > data.kills || (data.questClaimed && data.kills - data.questStartKills < QuestTarget))) return false;
            for (int i = 0; i < data.enemies.Length; i++)
            {
                var e = data.enemies[i];
                if (e == null || e.hp <= 0 || e.hp > 500 || !PositionValid(e.x,e.y)) return false;
            }
            if (data.version == 1) return data.potions >= 0 && data.potions <= 120;
            if (!InventoryModel.IsValid(data.inventory) || data.loot == null || data.loot.Length + data.enemies.Length*2 > 64) return false;
            for (int i = 0; i < data.loot.Length; i++)
            {
                var drop = data.loot[i];
                int limit = drop.item == ItemId.Gold ? 10000 : ItemCatalog.StackLimit(drop.item);
                if (limit == 0 || drop.amount <= 0 || drop.amount > limit || !PositionValid(drop.x,drop.y)) return false;
            }
            return true;
        }
        public void NotifyRestored() { Changed(ChangeKind.Restored); }
        public void Restore(SaveData data,bool notify = true)
        {
            if (!IsValid(data)) throw new ArgumentException("Invalid or unsupported save data.");
            if (data.version == 1)
            {
                var migrated = new InventoryModel();
                if (data.potions > 0) migrated.TryAdd(ItemId.Supply,data.potions);
                Inventory.Restore(migrated.Capture());
            }
            else Inventory.Restore(data.inventory);
            Facts.Restore(data.version < 3 ? new FactRecord[0] : data.facts);
            Progression.Restore(data.version < 3 ? new ProgressionSaveData() : data.progression);
            QuestStartKills = data.version < 3 ? 0 : data.questStartKills;
            Player.Reset(data.hp,data.x,data.y); Coins = data.coins; Kills = data.kills; Wave = data.wave;
            QuestAccepted = data.questAccepted; QuestClaimed = data.questClaimed; WaveCleared = data.waveCleared;
            if (notify) NotifyRestored();
        }
    }
}
