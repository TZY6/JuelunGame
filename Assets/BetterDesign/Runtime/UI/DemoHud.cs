using System;
using BetterDesign.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BetterDesign
{
    // UGUI View + Presenter binding. Buttons issue commands; model events mark the view dirty.
    public sealed class DemoHud : IDisposable
    {
        readonly DemoWorld world;
        readonly IDisposable modelSubscription, noticeSubscription, combatSubscription;
        readonly PanelStack stack = new PanelStack();
        readonly Font font;
        readonly Text stats, quest, notice, bagText, diagnostics, status;
        readonly GameObject bag, pause, architecture;
        readonly Image health;
        bool dirty = true;
        float debugTimer, noticeTimer;
        int lastActor, lastDamage;
        static readonly Color Ink = new Color(.045f,.075f,.11f,.95f);
        static readonly Color Accent = new Color(.35f,.91f,.85f);
        public bool HasModal { get { return stack.IsOpen; } }

        public DemoHud(Transform parent, DemoWorld world, UnityAction restart)
        {
            this.world = world;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvasGo = new GameObject("UGUI - Screen / Overlay / Modal", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(parent, false);
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280,800); scaler.matchWidthOrHeight = .5f;
            var root = canvasGo.transform;
            var header = Panel(root,"HUD",new Vector2(0,1),new Vector2(1,1),new Vector2(0,-43),new Vector2(0,86),Ink);
            Label(header.transform,"BETTER DESIGN  /  SANCTUARY",new Vector2(24,-12),new Vector2(650,30),24,Accent);
            stats = Label(header.transform,"",new Vector2(25,-48),new Vector2(780,26),17,Color.white);
            var healthBack = Panel(header.transform,"HP",Vector2.one,Vector2.one,new Vector2(-175,-44),new Vector2(280,12),new Color(.2f,.27f,.32f));
            health = Panel(healthBack.transform,"Fill",Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,Accent).GetComponent<Image>();
            health.rectTransform.anchorMax = Vector2.one;
            var contract = Panel(root,"Contract",new Vector2(0,1),new Vector2(0,1),new Vector2(150,-178),new Vector2(268,150),Ink);
            Label(contract.transform,"01  /  FIELD CONTRACT",new Vector2(16,-12),new Vector2(236,26),16,Accent);
            quest = Label(contract.transform,"",new Vector2(16,-47),new Vector2(236,94),16,Color.white);
            architecture = Panel(root,"Architecture observer",Vector2.one,Vector2.one,new Vector2(-164,-270),new Vector2(296,354),Ink);
            Label(architecture.transform,"LIVE ARCHITECTURE  [F1]",new Vector2(16,-12),new Vector2(270,25),16,Accent);
            diagnostics = Label(architecture.transform,"",new Vector2(16,-48),new Vector2(264,300),15,Color.white);
            var footer = Panel(root,"Command bar",Vector2.zero,Vector2.right,new Vector2(0,42),new Vector2(0,84),Ink);
            Label(footer.transform,"WASD / arrows Move    SPACE / LMB Attack    Q / RMB Spell    SHIFT Dash",new Vector2(22,-10),new Vector2(1220,25),17,Color.white);
            Label(footer.transform,"E Beacon    B Buy potion    H Heal    N Next wave    TAB Bag    ESC Pause    F5 Save    F9 Load",new Vector2(22,-44),new Vector2(1220,25),16,Accent);
            notice = Label(root,"Welcome. Walk west to the cyan beacon and press E.",new Vector2(300,-102),new Vector2(650,65),18,Color.white);
            status = Label(root,"",new Vector2(420,-700),new Vector2(600,28),20,Accent);
            bag = Modal(root,"Inventory");
            Label(bag.transform,"INVENTORY",new Vector2(28,-22),new Vector2(410,35),26,Accent);
            bagText = Label(bag.transform,"",new Vector2(28,-80),new Vector2(410,150),20,Color.white);
            Button(bag.transform,"Use potion [H]",new Vector2(28,-250),world.Heal);
            Button(bag.transform,"Close [TAB / ESC]",new Vector2(250,-250),CloseTop);
            pause = Modal(root,"Pause");
            Label(pause.transform,"SANCTUARY  /  PAUSED",new Vector2(28,-22),new Vector2(440,40),24,Accent);
            Button(pause.transform,"Resume",new Vector2(28,-90),CloseAll);
            Button(pause.transform,"Inventory",new Vector2(250,-90),() => Open(bag));
            Button(pause.transform,"Save checkpoint",new Vector2(28,-155),world.Save);
            Button(pause.transform,"Load checkpoint",new Vector2(250,-155),() => { world.Load(); CloseAll(); });
            Button(pause.transform,"New run",new Vector2(28,-220),() => { CloseAll(); restart(); });
            Button(pause.transform,"Architecture [F1]",new Vector2(250,-220),ToggleArchitecture);
            modelSubscription = world.Events.Model.Subscribe(OnModel);
            noticeSubscription = world.Events.Notice.Subscribe(OnNotice);
            combatSubscription = world.Events.Combat.Subscribe(OnCombat);
            noticeTimer = 8;
        }
        void OnModel(ModelChanged _) { dirty = true; }
        void OnNotice(string message) { notice.text = message; noticeTimer = 6; }
        void OnCombat(CombatEvent evt) { lastActor = evt.ActorId; lastDamage = evt.Damage; }
        public void Tick(float unscaledDt)
        {
            if (dirty)
            {
                dirty = false;
                var model = world.Model;
                stats.text = "HP " + model.Hp + "/100     GOLD " + model.Coins + "     POTIONS " + model.Potions + "     WAVE " + model.Wave;
                health.rectTransform.anchorMax = new Vector2(model.Hp / 100f, 1);
                quest.text = !model.QuestAccepted ? "Visit cyan beacon [E]\nAccept the sentinel contract." :
                    model.QuestClaimed ? "CONTRACT COMPLETE\n+50 gold / +2 potions\nContinue with the next wave." :
                    "Sentinels defeated: " + Mathf.Min(model.Kills,6) + "/6\nReward: 50 gold + 2 potions\nReturn to beacon to claim [E].";
                bagText.text = "Gold: " + model.Coins + "\nHealing potions: " + model.Potions + "\nEach potion restores 40 HP.\nBuy at beacon [B]: 10 gold.";
            }
            status.text = !world.Hero.IsAlive ? "DEFEATED  /  F9 to load checkpoint" : world.Model.WaveCleared ? "WAVE CLEAR  /  N to continue" : "";
            noticeTimer -= unscaledDt;
            notice.enabled = noticeTimer > 0;
            debugTimer -= unscaledDt;
            if (architecture.activeSelf && debugTimer <= 0)
            {
                debugTimer = .25f;
                diagnostics.text =
                    "MODEL -> event -> HUD\nCOMMAND -> controller -> model\n\n" +
                    "Hero FSM: " + world.Hero.Machine.State + "\nTransitions: " + world.Hero.Machine.Transitions +
                    "\nSentinels alive: " + world.LivingEnemies +
                    "\n\nPool active (actors + FX): " + world.PoolActive + "\nFX peak / capacity: " + world.PoolPeak + "/" + DemoWorld.EffectCapacity +
                    "\nPool misses: " + world.PoolMisses +
                    "\n\nModel events: " + world.Events.Model.Published + "\nCombat events: " + world.Events.Combat.Published +
                    "\nLast hit actor / HP: " + lastActor + " / " + lastDamage +
                    "\nManaged heap: " + (GC.GetTotalMemory(false) / 1024) + " KB" +
                    "\n\nNo per-frame scene searches.\nUI samples diagnostics at 4 Hz.";
            }
        }
        public void ToggleArchitecture() { architecture.SetActive(!architecture.activeSelf); }
        public void ToggleBag() { if (bag.activeSelf) CloseTop(); else Open(bag); }
        public void Back() { if (stack.IsOpen) CloseTop(); else Open(pause); }
        void Open(GameObject panel) { stack.Push(panel); SyncPause(); }
        void CloseTop() { stack.Back(); SyncPause(); }
        public void CloseAll() { stack.Clear(); SyncPause(); }
        void SyncPause()
        {
            if (world.Paused != stack.IsOpen) world.TogglePause();
        }
        GameObject Modal(Transform root,string name)
        {
            var panel = Panel(root,name,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(510,330),Ink);
            panel.GetComponent<Image>().raycastTarget = true;
            panel.SetActive(false); return panel;
        }
        GameObject Panel(Transform parent,string name,Vector2 min,Vector2 max,Vector2 position,Vector2 size,Color color)
        {
            var go = new GameObject(name,typeof(RectTransform),typeof(Image));
            go.transform.SetParent(parent,false);
            var rect = (RectTransform)go.transform; rect.anchorMin = min; rect.anchorMax = max;
            rect.pivot = new Vector2(.5f,.5f); rect.anchoredPosition = position; rect.sizeDelta = size;
            var image = go.GetComponent<Image>(); image.color = color; image.raycastTarget = false;
            return go;
        }
        Text Label(Transform parent,string text,Vector2 position,Vector2 size,int fontSize,Color color)
        {
            var go = new GameObject("Text",typeof(RectTransform),typeof(Text)); go.transform.SetParent(parent,false);
            var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(0,1);
            rect.pivot = new Vector2(0,1); rect.anchoredPosition = position; rect.sizeDelta = size;
            var label = go.GetComponent<Text>(); label.font = font; label.text = text; label.fontSize = fontSize;
            label.color = color; label.raycastTarget = false; label.supportRichText = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap; label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }
        void Button(Transform parent,string text,Vector2 position,UnityAction action)
        {
            var go = Panel(parent,text,new Vector2(0,1),new Vector2(0,1),position,new Vector2(202,48),new Color(.13f,.26f,.30f));
            var rect = (RectTransform)go.transform; rect.pivot = new Vector2(0,1);
            var image = go.GetComponent<Image>(); image.raycastTarget = true;
            var button = go.AddComponent<Button>(); button.targetGraphic = image; button.onClick.AddListener(action);
            var label = Label(go.transform,text,new Vector2(10,-12),new Vector2(184,28),16,Color.white);
            label.alignment = TextAnchor.MiddleCenter;
        }
        public void Dispose()
        {
            modelSubscription.Dispose(); noticeSubscription.Dispose(); combatSubscription.Dispose();
        }
    }
}
