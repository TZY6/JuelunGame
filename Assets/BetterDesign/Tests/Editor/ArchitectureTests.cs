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
            Assert.AreEqual(80,model.Coins); Assert.AreEqual(5,model.Potions); Assert.IsTrue(model.QuestClaimed);
        }
        [Test] public void PotionAndShopEnforcePreconditions()
        {
            var model = new GameModel(new GameEvents()); Assert.IsFalse(model.UsePotion()); Assert.IsFalse(model.BuyPotion());
            model.Damage(50); Assert.IsTrue(model.UsePotion()); Assert.AreEqual(90,model.Hp);
            model.EnemyKilled(); model.EnemyKilled(); Assert.IsTrue(model.BuyPotion()); Assert.AreEqual(0,model.Coins);
            model.Damage(100); Assert.IsFalse(model.UsePotion()); Assert.IsFalse(model.NextWave());
        }
        [Test] public void RestoreRejectsNaNsAndUnsupportedVersions()
        {
            var data = new SaveData { x = float.NaN };
            Assert.IsFalse(GameModel.IsValid(data));
            data.x = 0; data.version = 2; Assert.IsFalse(GameModel.IsValid(data));
            data.version = 1; data.enemies = new[] { new EnemySnapshot { x = 0,y = 0,hp = 0 } };
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
            Assert.AreEqual(5,model.Coins); Assert.AreEqual(100,model.Hp);
        }
        [Test] public void DisposedModelSubscriptionsStopReceivingEvents()
        {
            var events = new GameEvents(); int received = 0;
            var handle = events.Model.Subscribe(_ => received++);
            new GameModel(events).Damage(10); handle.Dispose(); new GameModel(events).Damage(10);
            Assert.AreEqual(1,received); Assert.AreEqual(0,events.Model.ListenerCount);
        }
    }
}
