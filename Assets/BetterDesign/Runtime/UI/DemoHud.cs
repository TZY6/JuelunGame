using System;
using BetterDesign.Core;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace BetterDesign
{
    // UGUI View + Presenter binding. Buttons issue commands; model events mark the view dirty.
    public sealed class DemoHud : MonoBehaviour, IDisposable
    {
        DemoWorld world;
        IDisposable modelSubscription, noticeSubscription, combatSubscription;
        readonly PanelStack stack = new PanelStack();
        [SerializeField] TMP_FontAsset font;
        [SerializeField] TMP_Text stats, quest, notice, bagText, diagnostics, status;
        [SerializeField] GameObject bag, pause, architecture;
        [SerializeField] Image health;
        [SerializeField] GameObject modalLayer;
        bool dirty = true;
        float debugTimer, noticeTimer;
        int lastActor, lastDamage;
        public TMP_FontAsset Font { get { return font; } }
        public bool HasModal { get { return stack.IsOpen; } }
        public bool IsConfigured
        {
            get { return font != null && stats != null && quest != null && notice != null && bagText != null &&
                diagnostics != null && status != null && bag != null && pause != null && architecture != null &&
                health != null && modalLayer != null; }
        }
        public void Bind(DemoWorld world)
        {
            Dispose();
            if (!IsConfigured) throw new InvalidOperationException("UIManager scene references missing.");
            this.world = world;
            stack.Clear(); bag.SetActive(false); pause.SetActive(false); modalLayer.SetActive(false);
            modelSubscription = world.Events.Model.Subscribe(OnModel);
            noticeSubscription = world.Events.Notice.Subscribe(OnNotice);
            combatSubscription = world.Events.Combat.Subscribe(OnCombat);
            dirty = true; noticeTimer = 8;
        }
        // Button UnityEvents are serialized in the scene, visible in the Inspector.
        public void UsePotion() { if (world != null) world.Heal(); }
        public void SaveCheckpoint() { if (world != null) world.Save(); }
        public void LoadCheckpoint() { if (world != null) { world.Load(); CloseAll(); } }
        public void StartNewRun() { if (world != null) { CloseAll(); world.NewRun(); } }
        public void OpenBag() { if (world != null) Open(bag); }
        void OnModel(ModelChanged _) { dirty = true; }
        void OnNotice(string message) { notice.text = message; noticeTimer = 6; }
        void OnCombat(CombatEvent evt) { lastActor = evt.ActorId; lastDamage = evt.Damage; }
        public void Tick(float unscaledDt)
        {
            if (dirty)
            {
                dirty = false;
                var model = world.Model;
                stats.text = "生命 " + model.Hp + "/100     金币 " + model.Coins + "     补给 " + model.Potions + "     第 " + model.Wave + " 波";
                health.rectTransform.anchorMax = new Vector2(model.Hp / 100f, 1);
                quest.text = !model.QuestAccepted ? "前往青色信标 [E]\n接取守卫清理契约。" :
                    model.QuestClaimed ? "契约已完成\n金币 +50 / 补给 +2\n继续挑战下一波。" :
                     "已击败守卫：" + Mathf.Min(model.Kills,6) + "/6\n奖励：50 金币 + 2 瓶补给\n回到信标领取奖励 [E]。";
                bagText.text = "金币：" + model.Coins + "    治疗补给：" + model.Potions + "    格子：" + model.Inventory.Occupied + "/12";
            }
            status.text = !world.Hero.IsAlive ? "挑战失败 / 按 F9 恢复检查点" : world.Model.WaveCleared ? "本波已清理 / 按 N 继续" : "";
            noticeTimer -= unscaledDt;
            notice.enabled = noticeTimer > 0;
            debugTimer -= unscaledDt;
            if (architecture.activeSelf && debugTimer <= 0)
            {
                debugTimer = .25f;
                diagnostics.text =
                    "MODEL -> event -> HUD\nCOMMAND -> controller -> model\n\n" +
                    "Hero FSM: " + world.Hero.Machine.State + "\nTransitions: " + world.Hero.Machine.Transitions +
                    "\nSentinels alive: " + world.LivingEnemies + "\n地面掉落：" + world.Loot.Active +
                    "\n\nPool active (actors + FX): " + world.PoolActive + "\nFX peak / capacity: " + world.PoolPeak + "/" + DemoWorld.EffectCapacity +
                    "\nPool misses: " + world.PoolMisses +
                    "\n\nModel events: " + world.Events.Model.Published + "\nCombat events: " + world.Events.Combat.Published +
                    "\nLast hit actor / HP: " + lastActor + " / " + lastDamage +
                    "\nManaged heap: " + (GC.GetTotalMemory(false) / 1024) + " KB" +
                    "\n\nNo per-frame scene searches.\nUI samples diagnostics at 4 Hz.";
            }
        }
        public void ToggleArchitecture() { architecture.SetActive(!architecture.activeSelf); }
        public void ToggleBag() { if (world == null) return; if (bag.activeSelf) CloseTop(); else Open(bag); }
        public void Back() { if (world == null) return; if (stack.IsOpen) CloseTop(); else Open(pause); }
        void Open(GameObject panel) { stack.Push(panel); SyncPause(); }
        public void CloseTop() { stack.Back(); SyncPause(); }
        public void CloseAll() { stack.Clear(); SyncPause(); }
        void SyncPause()
        {
            modalLayer.SetActive(stack.IsOpen);
            if (world != null && world.Paused != stack.IsOpen) world.TogglePause();
        }
        public void Dispose()
        {
            if (modelSubscription != null) modelSubscription.Dispose();
            if (noticeSubscription != null) noticeSubscription.Dispose();
            if (combatSubscription != null) combatSubscription.Dispose();
            modelSubscription = noticeSubscription = combatSubscription = null;
        }
    }
}
