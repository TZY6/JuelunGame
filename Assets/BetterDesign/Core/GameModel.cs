using System;

namespace BetterDesign.Core
{
    [Serializable]
    public sealed class EnemySnapshot
    {
        public float x, y;
        public int hp;
    }
    [Serializable]
    public sealed class SaveData
    {
        public int version = 1;
        public int hp = 100, coins, kills, wave = 1, potions = 3;
        public bool questAccepted, questClaimed, waveCleared;
        public float x = -5, y;
        public EnemySnapshot[] enemies = new EnemySnapshot[0];
    }
    public sealed class GameModel
    {
        readonly GameEvents events;
        public const int MaxHp = 100;
        public const int QuestTarget = 6;
        public int Hp { get; private set; } = MaxHp;
        public int Coins { get; private set; }
        public int Kills { get; private set; }
        public int Wave { get; private set; } = 1;
        public int Potions { get; private set; } = 3;
        public bool QuestAccepted { get; private set; }
        public bool QuestClaimed { get; private set; }
        public bool WaveCleared { get; private set; }
        public GameModel(GameEvents events) { this.events = events; }
        void Changed(ChangeKind kind) { events.Model.Publish(new ModelChanged(kind)); }
        public void Damage(int amount) { Hp = Math.Max(0, Hp - Math.Max(0, amount)); Changed(ChangeKind.Stats); }
        public void EnemyKilled()
        {
            Kills++; Coins += 5; Changed(ChangeKind.Inventory); Changed(ChangeKind.Quest);
        }
        public bool UsePotion()
        {
            if (Potions <= 0 || Hp == 0 || Hp == MaxHp) return false;
            Potions--; Hp = Math.Min(MaxHp, Hp + 40); Changed(ChangeKind.Inventory); return true;
        }
        public bool BuyPotion()
        {
            if (Coins < 10 || Potions >= 99) return false;
            Coins -= 10; Potions++; Changed(ChangeKind.Inventory); return true;
        }
        public void InteractQuest()
        {
            if (!QuestAccepted) { QuestAccepted = true; events.Notice.Publish("Contract accepted: defeat 6 sentinels."); }
            else if (!QuestClaimed && Kills >= QuestTarget)
            {
                QuestClaimed = true; Coins += 50; Potions += 2; events.Notice.Publish("Contract complete! +50 gold, +2 potions.");
            }
            else events.Notice.Publish(QuestClaimed ? "The sanctuary is safe. Try another wave!" : "Defeat 6 sentinels, then return to the beacon.");
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
            return new SaveData { hp = Hp, coins = Coins, kills = Kills, wave = Wave, potions = Potions,
                questAccepted = QuestAccepted, questClaimed = QuestClaimed, waveCleared = WaveCleared };
        }
        public static bool IsValid(SaveData data)
        {
            if (data == null || data.version != 1 || data.hp < 0 || data.hp > MaxHp || data.coins < 0 ||
                data.coins > 100000000 || data.kills < 0 || data.kills > 10000000 || data.wave < 1 ||
                data.wave > 10000 || data.potions < 0 || data.potions > 1000 ||
                float.IsNaN(data.x) || float.IsNaN(data.y) || Math.Abs(data.x) > 10 || Math.Abs(data.y) > 6 ||
                data.enemies == null || data.enemies.Length > 16 || (data.waveCleared && data.enemies.Length != 0) ||
                (data.questClaimed && (!data.questAccepted || data.kills < QuestTarget))) return false;
            for (int i = 0; i < data.enemies.Length; i++)
            {
                var e = data.enemies[i];
                if (e == null || e.hp <= 0 || e.hp > 500 || float.IsNaN(e.x) || float.IsNaN(e.y) ||
                    Math.Abs(e.x) > 10 || Math.Abs(e.y) > 6) return false;
            }
            return true;
        }
        public void Restore(SaveData data)
        {
            if (!IsValid(data)) throw new ArgumentException("Invalid or unsupported save data.");
            Hp = data.hp; Coins = data.coins; Kills = data.kills; Wave = data.wave; Potions = data.potions;
            QuestAccepted = data.questAccepted; QuestClaimed = data.questClaimed; WaveCleared = data.waveCleared;
            Changed(ChangeKind.Restored);
        }
    }
}
