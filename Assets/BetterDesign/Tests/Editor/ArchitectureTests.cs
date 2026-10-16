using System;
using System.IO;
using BetterDesign.Core;
using NUnit.Framework;
using BetterDesign.Editor;

namespace BetterDesign.Tests
{
    public sealed class ArchitectureTests
    {
        sealed class Item : IPoolItem
        {
            public bool IsRented { get; set; }
            public int Rents, Returns;
            public void OnRent() { Rents++; }
            public void OnReturn() { Returns++; }
        }
        [Test] public void AuthoredSceneReferencesAndRequestedFontAreComplete()
        {
            Assert.DoesNotThrow(() => DemoValidation.Validate());
        }
        [Test] public void RuntimeContainsNoSceneObjectFactories()
        {
            string[] paths = UnityEditor.AssetDatabase.FindAssets("t:MonoScript",new[] { "Assets/BetterDesign/Runtime" });
            foreach (string asset in paths)
            {
                string source = File.ReadAllText(UnityEditor.AssetDatabase.GUIDToAssetPath(asset));
                StringAssert.DoesNotContain("new GameObject(",source);
                StringAssert.DoesNotContain("AddComponent<",source);
                StringAssert.DoesNotContain("Instantiate(",source);
                StringAssert.DoesNotContain("Sprite.Create(",source);
                StringAssert.DoesNotContain("new Texture2D(",source);
            }
        }
        sealed class InventoryProbe : IInventoryView
        {
            public int Renders,Selected;
            public void Render(InventoryModel model,int selected) { Renders++; Selected = selected; }
        }
        sealed class InventoryActions : IInventoryActions
        {
            public int Used = -1,Dropped = -1,Sales;
            public bool UseSlot(int index) { Used = index; return true; }
            public bool DropSlot(int index) { Dropped = index; return true; }
            public void SellMaterials() { Sales++; }
        }
        [Test] public void ActorModelOwnsPositionHealthAndTerminalState()
        {
            var actor = new ActorModel(); actor.Reset(50,1,2); actor.MoveTo(3,4); actor.Damage(20);
            Assert.AreEqual(30,actor.Health); Assert.AreEqual(3,actor.X); Assert.AreEqual(4,actor.Y);
            actor.Reset(0,0,0); Assert.AreEqual(ActorState.Dead,actor.Machine.State);
        }
        [Test] public void PlayerHealthHasOneAuthoritativeModel()
        {
            var model = new GameModel(new GameEvents()); model.Player.Damage(35);
            Assert.AreEqual(65,model.Hp); Assert.IsTrue(model.UsePotion()); Assert.AreEqual(100,model.Player.Health);
        }
        [Test] public void KillDoesNotGrantLootBeforePickup()
        {
            var model = new GameModel(new GameEvents()); model.EnemyKilled();
            Assert.AreEqual(1,model.Kills); Assert.AreEqual(0,model.Coins);
            Assert.IsTrue(model.Collect(ItemId.Gold,5)); Assert.AreEqual(5,model.Coins);
        }
        [Test] public void InventoryFillsStacksBeforeAllocatingAnotherSlot()
        {
            var model = new InventoryModel(); Assert.IsTrue(model.TryAdd(ItemId.Supply,12));
            Assert.AreEqual(10,model[0].amount); Assert.AreEqual(2,model[1].amount); Assert.AreEqual(2,model.Occupied);
        }
        [Test] public void FullBagPickupIsAtomicAndCurrencyUsesNoSlot()
        {
            var model = new GameModel(new GameEvents()); Assert.IsTrue(model.Collect(ItemId.Supply,117));
            Assert.IsFalse(model.Collect(ItemId.Mushroom,1)); Assert.AreEqual(120,model.Potions);
            Assert.AreEqual(0,model.Inventory.Count(ItemId.Mushroom));
            Assert.IsTrue(model.Collect(ItemId.Gold,5)); Assert.AreEqual(5,model.Coins);
        }
        [Test] public void InvalidRemovalDoesNotMutateInventory()
        {
            var model = new InventoryModel(); model.TryAdd(ItemId.Mushroom,2);
            Assert.IsFalse(model.TryRemove(-1,1)); Assert.IsFalse(model.TryRemove(0,3));
            Assert.AreEqual(2,model.Count(ItemId.Mushroom)); Assert.IsTrue(model.TryRemove(0,2)); Assert.AreEqual(ItemId.None,model[0].item);
        }
        [Test] public void QuestRewardWaitsForInventoryCapacity()
        {
            var model = new GameModel(new GameEvents()); model.InteractQuest(); model.Collect(ItemId.Supply,117);
            for (int i = 0; i < 6; i++) model.EnemyKilled();
            model.InteractQuest(); Assert.IsFalse(model.QuestClaimed); Assert.AreEqual(0,model.Coins);
            model.RemoveItem(11,2); model.InteractQuest();
            Assert.IsTrue(model.QuestClaimed); Assert.AreEqual(50,model.Coins); Assert.AreEqual(120,model.Potions);
        }
        [Test] public void SellingMaterialsDoesNotConsumeHealingSupply()
        {
            var model = new GameModel(new GameEvents()); model.Collect(ItemId.Mushroom,3); model.SellMaterials();
            Assert.AreEqual(6,model.Coins); Assert.AreEqual(0,model.Inventory.Count(ItemId.Mushroom)); Assert.AreEqual(3,model.Potions);
        }
        [Test] public void InventoryControllerRoutesSelectionAndReleasesModelSubscription()
        {
            var events = new GameEvents(); var model = new GameModel(events);
            var view = new InventoryProbe(); var actions = new InventoryActions();
            var controller = new InventoryController(model,events,view,actions);
            controller.Select(4); controller.UseSelected(); controller.DropSelected();
            Assert.AreEqual(4,actions.Used); Assert.AreEqual(4,actions.Dropped);
            controller.Select(99); Assert.AreEqual(4,view.Selected);
            int count = view.Renders; model.Damage(1); Assert.AreEqual(count,view.Renders);
            model.Collect(ItemId.Supply,1); Assert.AreEqual(count+1,view.Renders);
            controller.Dispose(); count = view.Renders; model.Collect(ItemId.Gold,5); Assert.AreEqual(count,view.Renders);
        }
        [Test] public void LegacyCheckpointMigratesPotionCountIntoStacks()
        {
            var model = new GameModel(new GameEvents());
            model.Restore(new SaveData { version = 1,potions = 11,coins = 9 });
            Assert.AreEqual(11,model.Potions); Assert.AreEqual(2,model.Inventory.Occupied); Assert.AreEqual(3,model.Capture().version);
        }
        [Test] public void CheckpointIncludesUncollectedLootWithoutGrantingIt()
        {
            var model = new GameModel(new GameEvents()); var data = model.Capture();
            data.loot = new[] { new LootSnapshot { item = ItemId.Gold,amount = 5,x = 0,y = 0 } };
            Assert.IsTrue(GameModel.IsValid(data)); model.Restore(data);
            Assert.AreEqual(0,model.Coins); Assert.AreEqual(5,data.loot[0].amount);
            model.Collect(data.loot[0].item,data.loot[0].amount);
            var collected = model.Capture(); Assert.AreEqual(5,collected.coins); Assert.AreEqual(0,collected.loot.Length);
        }
        [Test] public void SaveValidationReservesCapacityForRemainingEnemyDrops()
        {
            var data = new SaveData { enemies = new[] { new EnemySnapshot { hp = 20,x = 0,y = 0 } },loot = new LootSnapshot[63] };
            for (int i = 0; i < data.loot.Length; i++) data.loot[i] = new LootSnapshot { item = ItemId.Gold,amount = 5 };
            Assert.IsFalse(GameModel.IsValid(data));
        }
        [Test] public void UnsubscribeDuringDispatchIsSafe()
        {
            var signal = new Signal<int>(); int received = 0; IDisposable second = null;
            signal.Subscribe(_ => second.Dispose());
            second = signal.Subscribe(_ => received++);
            signal.Publish(1); Assert.AreEqual(0, received); Assert.AreEqual(1,signal.ListenerCount);
        }
        [Test] public void SubscriptionAddedDuringDispatchStartsOnNextEvent()
        {
            var signal = new Signal<int>(); int received = 0; bool added = false;
            signal.Subscribe(_ => { if (!added) { added = true; signal.Subscribe(__ => received++); } });
            signal.Publish(1); Assert.AreEqual(0, received);
            signal.Publish(2); Assert.AreEqual(1, received);
        }
        [Test] public void NestedPublicationIsWellDefined()
        {
            var signal = new Signal<int>(); int received = 0;
            signal.Subscribe(value => { received++; if (value == 1) signal.Publish(2); });
            signal.Publish(1); Assert.AreEqual(2, received); Assert.AreEqual(2, signal.Published);
        }
        [Test] public void ThrowingListenerDoesNotBreakCleanup()
        {
            var signal = new Signal<int>();
            var handle = signal.Subscribe(_ => throw new InvalidOperationException());
            Assert.Throws<InvalidOperationException>(() => signal.Publish(1)); handle.Dispose();
            Assert.DoesNotThrow(() => signal.Publish(2)); Assert.AreEqual(0, signal.ListenerCount);
        }
        [Test] public void PoolExhaustionAndDoubleReturnKeepCountsConsistent()
        {
            var pool = new FixedPool<Item>(1, () => new Item());
            Item first, second; Assert.IsTrue(pool.TryRent(out first)); Assert.IsFalse(pool.TryRent(out second));
            Assert.AreEqual(1,pool.Active); Assert.AreEqual(1,pool.Misses);
            Assert.IsTrue(pool.Return(first)); Assert.IsFalse(pool.Return(first)); Assert.AreEqual(0,pool.Active);
            Assert.IsTrue(pool.TryRent(out second)); Assert.AreSame(first,second); Assert.AreEqual(2,second.Rents);
        }
        [Test] public void PoolRejectsForeignObjects()
        {
            var pool = new FixedPool<Item>(1, () => new Item());
            Assert.IsFalse(pool.Return(new Item { IsRented = true })); Assert.AreEqual(0,pool.Active);
        }
        [Test] public void AttackLocksMovementAndDeathIsTerminalUntilReset()
        {
            var machine = new ActorStateMachine(); Assert.IsTrue(machine.TryEnter(ActorState.Attack));
            Assert.IsFalse(machine.TryEnter(ActorState.Move)); machine.Tick(.2f); Assert.AreEqual(.2f,machine.Elapsed);
            machine.Complete(); Assert.IsTrue(machine.TryEnter(ActorState.Dead));
            machine.Complete(); Assert.IsFalse(machine.TryEnter(ActorState.Move));
            machine.Reset(); Assert.AreEqual(ActorState.Idle,machine.State);
        }
        [Test] public void ContractRewardCannotBeClaimedTwice()
        {
            var model = new GameModel(new GameEvents()); model.InteractQuest();
            for (int i = 0; i < 6; i++) model.EnemyKilled();
            model.InteractQuest(); model.InteractQuest();
            Assert.AreEqual(50,model.Coins); Assert.AreEqual(5,model.Potions); Assert.IsTrue(model.QuestClaimed);
        }
        [Test] public void PotionAndShopEnforcePreconditions()
        {
            var model = new GameModel(new GameEvents()); Assert.IsFalse(model.UsePotion()); Assert.IsFalse(model.BuyPotion());
            model.Damage(50); Assert.IsTrue(model.UsePotion()); Assert.AreEqual(90,model.Hp);
            model.Collect(ItemId.Gold,10); Assert.IsTrue(model.BuyPotion()); Assert.AreEqual(0,model.Coins);
            model.Damage(100); Assert.IsFalse(model.UsePotion()); Assert.IsFalse(model.NextWave());
        }
        [Test] public void RestoreRejectsNaNsAndUnsupportedVersions()
        {
            var data = new SaveData { x = float.NaN };
            Assert.IsFalse(GameModel.IsValid(data));
            data.x = 0; data.version = 4; Assert.IsFalse(GameModel.IsValid(data));
            data.version = 2; data.enemies = new[] { new EnemySnapshot { x = 0,y = 0,hp = 0 } };
            Assert.IsFalse(GameModel.IsValid(data));
        }
        [Test] public void WaveCannotAdvanceBeforeClear()
        {
            var model = new GameModel(new GameEvents()); Assert.IsFalse(model.NextWave());
            model.ClearWave(); Assert.IsTrue(model.NextWave()); Assert.AreEqual(2,model.Wave); Assert.IsFalse(model.WaveCleared);
        }
        [Test] public void SaveRoundTripAndCorruptPrimaryRecoverBackup()
        {
            string directory = Path.Combine(Path.GetTempPath(),"BetterDesignTests-" + Guid.NewGuid().ToString("N"));
            try
            {
                var store = new JsonSaveStore(directory); string message; SaveData restored;
                var data = new SaveData { coins = 12 };
                Assert.IsTrue(store.TrySave(data,out message),message);
                data.coins = 33; Assert.IsTrue(store.TrySave(data,out message),message);
                Assert.IsTrue(store.TryLoad(out restored,out message),message); Assert.AreEqual(33,restored.coins);
                File.WriteAllText(Path.Combine(directory,"better-design-v1.json"),"{bad json");
                Assert.IsTrue(store.TryLoad(out restored,out message),message); Assert.AreEqual(12,restored.coins);
            }
            finally { if (Directory.Exists(directory)) Directory.Delete(directory,true); }
        }
        [Test] public void InvalidLoadKeepsModelUntouched()
        {
            var model = new GameModel(new GameEvents()); model.EnemyKilled();
            Assert.Throws<ArgumentException>(() => model.Restore(new SaveData { hp = -1 }));
            Assert.AreEqual(0,model.Coins); Assert.AreEqual(1,model.Kills); Assert.AreEqual(100,model.Hp);
        }
        [Test] public void DisposedModelSubscriptionsStopReceivingEvents()
        {
            var events = new GameEvents(); int received = 0;
            var handle = events.Model.Subscribe(_ => received++);
            new GameModel(events).Damage(10); handle.Dispose(); new GameModel(events).Damage(10);
            Assert.AreEqual(1,received); Assert.AreEqual(0,events.Model.ListenerCount);
        }

        [Test] public void HurtCancelsActionVersionAndDeathCannotRecover()
        {
            var machine = new ActorStateMachine(); int enter = 0,exit = 0;
            machine.Entered += _ => enter++; machine.Exited += _ => exit++;
            machine.TryEnter(ActorState.Skill); int version = machine.ActionVersion;
            machine.SetLocomotion(true); Assert.AreEqual(LocomotionState.Move,machine.Locomotion);
            Assert.AreEqual(ActorState.Skill,machine.State);
            Assert.IsTrue(machine.TryEnter(ActorState.Hurt)); Assert.IsFalse(machine.IsCurrent(version));
            machine.Complete(); Assert.AreEqual(ActorState.Move,machine.State);
            machine.TryEnter(ActorState.Dead); machine.Complete();
            Assert.IsFalse(machine.TryEnter(ActorState.Skill)); Assert.AreEqual(enter,exit);
        }
        [Test] public void FaultyObserverDoesNotSkipOtherObservers()
        {
            var signal = new Signal<int>(); int received = 0;
            signal.Subscribe(_ => throw new InvalidOperationException());
            signal.Subscribe(_ => received++);
            Assert.Throws<InvalidOperationException>(() => signal.Publish(0)); Assert.AreEqual(1,received);
        }
        [Test] public void RuntimeReportsObserverFailureWithoutFailingCommand()
        {
            int reported = 0;
            var events = new GameEvents(_ => reported++); var model = new GameModel(events);
            events.Model.Subscribe(_ => throw new InvalidOperationException());
            Assert.IsTrue(model.Collect(ItemId.Gold,5)); Assert.AreEqual(5,model.Coins); Assert.AreEqual(1,reported);
        }
        [Test] public void PoolExpandsOnlyInsideAuthoredCapacity()
        {
            var pool = new FixedPool<Item>(3,() => new Item(),1);
            Item a,b,c,d; pool.TryRent(out a); pool.TryRent(out b); pool.TryRent(out c);
            Assert.AreEqual(3,pool.AvailableCapacity); Assert.IsFalse(pool.TryRent(out d));
            pool.ReturnAll(); Assert.AreEqual(0,pool.Active);
        }
        [Test] public void PoolRejectsDuplicateRegistration()
        {
            var item = new Item();
            Assert.Throws<ArgumentException>(() => new FixedPool<Item>(2,() => item));
        }
        [Test] public void ExperienceSupportsExactThresholdAndSeveralLevels()
        {
            var model = new ProgressionModel(); model.AddExperience(30);
            Assert.AreEqual(2,model.Level); Assert.AreEqual(0,model.Experience);
            model.AddExperience(150); Assert.AreEqual(4,model.Level);
            Assert.AreEqual(3,model.SkillPoints); Assert.AreEqual(0,model.Experience);
        }
        [Test] public void ContractCountsKillsAfterAcceptance()
        {
            var model = new GameModel(new GameEvents());
            for (int i = 0; i < 6; i++) model.EnemyKilled();
            model.InteractQuest(); model.InteractQuest();
            Assert.IsFalse(model.QuestClaimed); Assert.AreEqual(0,model.QuestKills);
            for (int i = 0; i < 6; i++) model.EnemyKilled();
            model.InteractQuest(); Assert.IsTrue(model.QuestClaimed);
        }
        [Test] public void UnknownConditionsNeverPass()
        {
            var model = new GameModel(new GameEvents());
            Assert.IsFalse(ConditionEvaluator.Evaluate(new ConditionConfig { fact = FactKind.LocationVisited,targetId = "missing",required = 1 },model));
            Assert.IsFalse(ConditionEvaluator.Evaluate(new ConditionConfig { fact = FactKind.Kills,required = 0 },model));
        }
        [Test] public void ProgressionAndEquippedWeaponRoundTrip()
        {
            var model = new GameModel(new GameEvents()); model.EnemyKilled(); model.EquipWeapon(1);
            var data = model.Capture(); Assert.IsTrue(GameModel.IsValid(data));
            var copy = new GameModel(new GameEvents()); copy.Restore(data);
            Assert.AreEqual(1,copy.Progression.Weapon); Assert.AreEqual(10,copy.Progression.Experience);
            data.progression.experience = 100000; Assert.IsFalse(GameModel.IsValid(data));
        }

        [Test] public void SkillUpgradeSpendsPointsAndPersistsRank()
        {
            var progression = new ProgressionModel();
            Assert.IsFalse(progression.TryUpgradeMagic()); progression.AddExperience(30);
            Assert.IsTrue(progression.TryUpgradeMagic()); Assert.IsFalse(progression.TryUpgradeMagic());
            Assert.AreEqual(1,progression.MagicRank); Assert.AreEqual(0,progression.SkillPoints);
            Assert.IsTrue(ProgressionModel.IsValid(progression.Capture()));
        }
        [Test] public void WorldFactsAreIdempotentAndUseStableIdentifiers()
        {
            var model = new GameModel(new GameEvents());
            Assert.IsTrue(model.Facts.Record(FactKind.LocationVisited,"sanctuary.beacon"));
            Assert.IsFalse(model.Facts.Record(FactKind.LocationVisited,"sanctuary.beacon"));
            var condition = new ConditionConfig { fact = FactKind.LocationVisited,targetId = "sanctuary.beacon",required = 1 };
            Assert.IsTrue(ConditionEvaluator.Evaluate(condition,model));
            var copy = new GameModel(new GameEvents()); copy.Restore(model.Capture());
            Assert.IsTrue(ConditionEvaluator.Evaluate(condition,copy));
        }
        [Test] public void ClosedInventoryHasNoSubscribersAndReopensWithLatestData()
        {
            var events = new GameEvents(); var model = new GameModel(events); var view = new InventoryProbe();
            var controller = new InventoryController(model,events,view,new InventoryActions());
            controller.SetVisible(false); int count = view.Renders;
            model.Collect(ItemId.Supply,1); Assert.AreEqual(count,view.Renders);
            Assert.AreEqual(0,events.InventoryChanged.ListenerCount);
            controller.SetVisible(true); Assert.AreEqual(count+1,view.Renders);
            controller.Dispose(); Assert.AreEqual(0,events.InventoryChanged.ListenerCount);
        }
        sealed class InputProbe : IPlayerInput
        {
            public UnityEngine.Vector2 Movement;
            public bool AttackPressed,SpellPressed;
            public UnityEngine.Vector2 Move { get { return Movement; } }
            public bool Attack { get { return AttackPressed; } }
            public bool Spell { get { return SpellPressed; } }
            public bool Dash { get { return false; } }
        }
        sealed class MemoryStore : ISaveStore
        {
            public SaveData Snapshot;
            public bool TrySave(SaveData data,out string message) { Snapshot = data; message = "saved"; return true; }
            public bool TryLoad(out SaveData data,out string message) { data = Snapshot; message = "loaded"; return data != null; }
        }
        [Test] public void AuthoredWorldBootsMovesShootsAndRestoresAsOneSnapshot()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/BetterDesign/Scenes/BetterDesignDemo.unity",UnityEditor.SceneManagement.OpenSceneMode.Additive);
            DemoWorld world = null;
            try
            {
                DemoScene entry = null;
                foreach (var root in scene.GetRootGameObjects())
                {
                    var candidate = root.GetComponentInChildren<DemoScene>(true);
                    if (candidate != null) entry = candidate;
                }
                Assert.IsNotNull(entry);
                var input = new InputProbe { Movement = UnityEngine.Vector2.right };
                var store = new MemoryStore();
                world = new DemoWorld(entry.WorldManager,entry.Settings,store,input);
                float x = world.Hero.Position.x; world.Tick(.05f);
                Assert.Greater(world.Hero.Position.x,x);
                world.Model.EquipWeapon(1); input.AttackPressed = true; world.Tick(.05f); input.AttackPressed = false;
                for (int i = 0; i < 5; i++) world.Tick(.05f);
                Assert.Greater(world.PoolActive,world.LivingEnemies);
                Assert.AreEqual(ActorState.Attack,world.Hero.Machine.State);
                Assert.AreEqual(LocomotionState.Move,world.Hero.Machine.Locomotion);
                world.Save(); Assert.IsTrue(GameModel.IsValid(store.Snapshot));
                float savedX = store.Snapshot.x; world.Hero.Model.MoveTo(3,3);
                world.Load(); Assert.AreEqual(savedX,world.Hero.Position.x);
                Assert.AreEqual(1,world.Model.Progression.Weapon);
                Assert.AreEqual(store.Snapshot.enemies.Length,world.LivingEnemies);
            }
            finally
            {
                if (world != null) world.Dispose();
                UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene,true);
            }
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator AuthoredSceneEntersPlayModeAndOwnsUiLifetime()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/BetterDesign/Scenes/BetterDesignDemo.unity");
            yield return new UnityEngine.TestTools.EnterPlayMode();
            var entry = UnityEngine.Object.FindObjectOfType<DemoScene>();
            Assert.IsNotNull(entry); Assert.IsTrue(entry.enabled);
            yield return null;
            entry.UiManager.CloseAll(); Assert.IsFalse(entry.UiManager.HasModal);
            entry.UiManager.OpenBag(); Assert.IsTrue(entry.UiManager.HasModal);
            entry.UiManager.CloseAll(); Assert.IsFalse(entry.UiManager.HasModal);
            entry.UiManager.enabled = false;
            entry.UiManager.enabled = true;
            entry.UiManager.OpenBag(); Assert.IsTrue(entry.UiManager.HasModal);
            entry.UiManager.CloseAll();
            entry.enabled = false;
            var pose = new ActorModel(); pose.Reset(100,-5,0); pose.Face(-1,0);
            entry.WorldManager.hero.Render(pose,UnityEngine.Vector2.left,.02f);
            yield return null; Assert.IsTrue(entry.WorldManager.hero.Renderer.flipX);
            pose.Face(1,0); entry.WorldManager.hero.Render(pose,UnityEngine.Vector2.right,.02f);
            yield return null; Assert.IsFalse(entry.WorldManager.hero.Renderer.flipX);
            entry.enabled = true;
            yield return new UnityEngine.TestTools.ExitPlayMode();
        }

        [Test] public void InventoryMoveToEmptyPreservesQuantityAndPersistsSlot()
        {
            var events = new GameEvents(); var model = new GameModel(events); int changes = 0;
            events.InventoryChanged.Subscribe(_ => changes++);
            Assert.IsTrue(model.MoveItem(0,5)); Assert.AreEqual(1,changes);
            Assert.AreEqual(0,model.Inventory[0].amount); Assert.AreEqual(3,model.Inventory[5].amount);
            var json = UnityEngine.JsonUtility.ToJson(model.Capture());
            var restored = new GameModel(new GameEvents()); restored.Restore(UnityEngine.JsonUtility.FromJson<SaveData>(json));
            Assert.AreEqual(3,restored.Inventory[5].amount); Assert.AreEqual(0,restored.Inventory[0].amount);
        }
        [Test] public void InventoryDragMergesToLimitAndKeepsRemainder()
        {
            var inventory = new InventoryModel(); inventory.TryAdd(ItemId.Supply,12);
            Assert.IsTrue(inventory.TryMove(0,1));
            Assert.AreEqual(2,inventory[0].amount); Assert.AreEqual(10,inventory[1].amount);
            Assert.AreEqual(12,inventory.Count(ItemId.Supply));
        }
        [Test] public void InventoryDragSwapsDifferentItemsAtomically()
        {
            var inventory = new InventoryModel(); inventory.TryAdd(ItemId.Supply,3); inventory.TryAdd(ItemId.Mushroom,2);
            Assert.IsTrue(inventory.TryMove(0,1));
            Assert.AreEqual(ItemId.Mushroom,inventory[0].item); Assert.AreEqual(2,inventory[0].amount);
            Assert.AreEqual(ItemId.Supply,inventory[1].item); Assert.AreEqual(3,inventory[1].amount);
        }
        [Test] public void InvalidInventoryDragDoesNotChangeRevisionOrData()
        {
            var inventory = new InventoryModel(); inventory.TryAdd(ItemId.Supply,20); int revision = inventory.Revision;
            Assert.IsFalse(inventory.TryMove(0,1)); Assert.IsFalse(inventory.TryMove(0,0));
            Assert.IsFalse(inventory.TryMove(-1,2)); Assert.IsFalse(inventory.TryMove(0,12)); Assert.IsFalse(inventory.TryMove(2,0));
            Assert.AreEqual(revision,inventory.Revision); Assert.AreEqual(20,inventory.Count(ItemId.Supply));
        }
    }
}
