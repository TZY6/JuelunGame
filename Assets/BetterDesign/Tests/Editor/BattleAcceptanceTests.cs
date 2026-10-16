using System;
using BetterDesign.Core;
using BetterDesign.Editor;
using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace BetterDesign.Tests
{
    public sealed class BattleAcceptanceTests
    {
        sealed class Controls : IPlayerInput,IAimedPlayerInput
        {
            public Vector2 Movement,Target;
            public bool Attacking,Casting,Dodging,Aiming;
            public Vector2 Move { get { return Movement; } }
            public bool Attack { get { return Attacking; } }
            public bool Spell { get { return Casting; } }
            public bool Dash { get { return Dodging; } }
            public bool HasAim { get { return Aiming; } }
            public Vector2 AimPoint { get { return Target; } }
        }
        sealed class MemoryStore : ISaveStore
        {
            public bool TrySave(SaveData data,out string message) { message = "test checkpoint"; return true; }
            public bool TryLoad(out SaveData data,out string message) { data = null; message = "none"; return false; }
        }
        Scene scene;
        DemoScene entry;
        DemoWorld world;
        Controls input;
        [SetUp] public void OpenArena()
        {
            DemoImportRepair.EnsureBindings();
            scene = EditorSceneManager.OpenScene("Assets/BetterDesign/Scenes/BetterDesignDemo.unity",OpenSceneMode.Additive);
            foreach (var root in scene.GetRootGameObjects())
            {
                var candidate = root.GetComponentInChildren<DemoScene>(true);
                if (candidate != null) entry = candidate;
            }
            Assert.IsNotNull(entry);
            input = new Controls();
            world = new DemoWorld(entry.WorldManager,entry.Settings,new MemoryStore(),input);
            world.Hero.Reset(100,Vector2.zero);
            for (int i = 0; i < 6; i++) world.Enemies[i].Reset(50,new Vector2(8,4+i*.1f));
        }
        [TearDown] public void CloseArena()
        {
            if (world != null) world.Dispose();
            if (SceneManager.sceneCount == 1) EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            EditorSceneManager.CloseScene(scene,true);
        }
        void TickHero(float duration)
        {
            for (float elapsed = 0; elapsed < duration; elapsed += .01f) world.Hero.Tick(.01f,input);
        }
        [Test] public void BufferedAttackExecutesAfterRecovery()
        {
            int attacks = 0;
            world.Hero.Machine.Entered += state => { if (state == ActorState.Attack) attacks++; };
            input.Attacking = true; TickHero(.01f); input.Attacking = false; TickHero(.42f);
            input.Attacking = true; TickHero(.01f); input.Attacking = false; TickHero(.6f);
            Assert.AreEqual(2,attacks);
        }
        [Test] public void MouseAimHitsOppositeMovementDirection()
        {
            world.Enemies[0].Reset(50,Vector2.left);
            input.Aiming = true; input.Target = Vector2.left*3; input.Movement = Vector2.right; input.Attacking = true;
            TickHero(.25f);
            Assert.AreEqual(25,world.Enemies[0].Health);
            Assert.IsTrue(world.Hero.View.Renderer.flipX);
        }
        [Test] public void DodgeCancelsUnresolvedAttack()
        {
            world.Enemies[0].Reset(50,Vector2.right);
            input.Attacking = true; TickHero(.04f); input.Attacking = false;
            input.Dodging = true; TickHero(.01f); input.Dodging = false;
            Assert.AreEqual(ActorState.Dash,world.Hero.Machine.State);
            TickHero(.3f); Assert.AreEqual(50,world.Enemies[0].Health);
        }
        [Test] public void ArrowSweepsNearTargetBeforeFarTarget()
        {
            world.Enemies[0].Reset(50,Vector2.right);
            world.Enemies[1].Reset(50,Vector2.right*.3f);
            world.Enemies[0].Machine.TryEnter(ActorState.Interact); world.Enemies[1].Machine.TryEnter(ActorState.Interact);
            Assert.IsTrue(world.Fire(Vector2.zero,Vector2.right,25,true));
            world.Tick(.1f);
            Assert.AreEqual(50,world.Enemies[0].Health);
            Assert.AreEqual(25,world.Enemies[1].Health);
        }
        [Test] public void EnemyAttackHasTellAndCanBeDodged()
        {
            world.Enemies[0].Reset(50,Vector2.right);
            world.Tick(.01f); Assert.AreEqual(ActorState.Attack,world.Enemies[0].Machine.State);
            Assert.IsTrue(world.Enemies[0].View.transform.Find("Attack Telegraph").GetComponent<SpriteRenderer>().enabled);
            Assert.AreEqual(100,world.Model.Hp);
            world.Tick(.1f); Assert.AreEqual(100,world.Model.Hp);
            world.Hero.Model.MoveTo(-3,0);
            for (int i = 0; i < 6; i++) world.Tick(.05f);
            Assert.AreEqual(100,world.Model.Hp);
        }
        [Test] public void ContinuousMeleeCanWinAnEntireWave()
        {
            for (int i = 0; i < 6; i++) world.Enemies[i].Reset(50,new Vector2(1.1f+i*.15f,0));
            input.Attacking = true;
            for (int i = 0; i < 1200 && !world.Model.WaveCleared && world.Hero.IsAlive; i++) world.Tick(1f/60);
            Assert.AreEqual(6,world.Model.Kills);
            Assert.IsTrue(world.Model.WaveCleared);
            Assert.Greater(world.Model.Hp,0);
            Assert.Greater(world.Loot.Active,0);
        }
        [Test] public void HealthBarTracksDamageAndResetsOnReuse()
        {
            var enemy = world.Enemies[0]; enemy.Reset(50,Vector2.right);
            enemy.ReceiveDamage(25); enemy.Tick(.01f,null);
            var fill = enemy.View.transform.Find("Health Bar Fill");
            Assert.IsNotNull(fill); Assert.AreEqual(.45f,fill.localScale.x,.001f);
            enemy.Reset(50,Vector2.right); enemy.Tick(.01f,null);
            Assert.AreEqual(.9f,fill.localScale.x,.001f);
        }
        [Test] public void RequestedFontHasVisibleFallbackForHudAndControls()
        {
            var fonts = entry.UiManager.GetComponent<SceneFontFallback>();
            Assert.IsNotNull(fonts); fonts.Bind();
            foreach (char character in "生命100/100 金币 Lv.2 WASD 左键攻击 按住射箭! ×3")
                Assert.IsTrue(fonts.Supports(character),"Missing glyph: " + character);
            TMP_Text title = null;
            foreach (var label in entry.UiManager.GetComponentsInChildren<TMP_Text>(true))
                if (label.name == "Title TMP") title = label;
            Assert.IsNotNull(title); title.ForceMeshUpdate();
            var displayed = new System.Text.StringBuilder();
            for (int i = 0; i < title.textInfo.characterCount; i++) displayed.Append(title.textInfo.characterInfo[i].character);
            Assert.AreEqual(title.text,displayed.ToString(),"Font markup must be parsed rather than displayed.");
            Assert.AreEqual("7000msyhbdallicon SDF",title.font.name);
        }

        [Test] public void ClosingModalClearsSelectionSoSpaceCannotReopenIt()
        {
            entry.UiManager.Bind(world); entry.UiManager.OpenBag();
            var button = entry.InventoryView.GetComponentInChildren<UnityEngine.UI.Button>(true);
            var events = entry.UiManager.UiEventSystem;
            events.SetSelectedGameObject(button.gameObject);
            entry.UiManager.CloseAll();
            Assert.IsFalse(entry.UiManager.HasModal);
            Assert.IsNull(events.currentSelectedGameObject);
            entry.UiManager.Dispose();
        }

        [Test] public void EquippingBowChangesPresentationAndRestoresSword()
        {
            var sword = world.Hero.View.Animation.runtimeAnimatorController;
            world.Model.EquipWeapon(1); world.Hero.Tick(.01f,input);
            Assert.AreEqual(world.Hero.View.BowController,world.Hero.View.Animation.runtimeAnimatorController);
            Assert.AreNotEqual(sword,world.Hero.View.Animation.runtimeAnimatorController);
            world.Model.EquipWeapon(0); world.Hero.Tick(.01f,input);
            Assert.AreEqual(sword,world.Hero.View.Animation.runtimeAnimatorController);
        }

        void WinRangedWave(bool magic)
        {
            world.Model.EquipWeapon(magic ? 0 : 1);
            for (int i = 0; i < 6; i++) world.Enemies[i].Reset(50,new Vector2(3+i*.35f,(i%3-1)*.8f));
            input.Aiming = true; input.Attacking = !magic; input.Casting = magic;
            for (int frame = 0; frame < 2400 && !world.Model.WaveCleared && world.Hero.IsAlive; frame++)
            {
                ActorController nearest = null; float distance = float.MaxValue;
                for (int i = 0; i < 6; i++)
                {
                    float candidate = (world.Enemies[i].Position-world.Hero.Position).sqrMagnitude;
                    if (world.Enemies[i].IsAlive && candidate < distance) { distance = candidate; nearest = world.Enemies[i]; }
                }
                if (nearest != null)
                {
                    input.Target = nearest.Position;
                    input.Movement = distance < 6.25f ? (world.Hero.Position-nearest.Position).normalized : Vector2.zero;
                }
                if (world.Model.Hp < 50) world.Model.UsePotion();
                world.Tick(1f/60);
            }
            Assert.AreEqual(6,world.Model.Kills,magic ? "Magic must complete a live wave." : "Aimed bow must complete a live wave.");
            Assert.IsTrue(world.Model.WaveCleared); Assert.Greater(world.Model.Hp,0);
        }
        [Test] public void AimedBowCanWinALiveWave() { WinRangedWave(false); }
        [Test] public void AimedMagicCanWinALiveWave() { WinRangedWave(true); }

        [Test] public void WalkingFacesMovementWhenMouseIsStationaryElsewhere()
        {
            input.Aiming = true; input.Target = Vector2.right*5; input.Movement = Vector2.left;
            world.Hero.Tick(.02f,input); Assert.Less(world.Hero.Model.FacingX,0); Assert.IsTrue(world.Hero.View.Renderer.flipX);
            input.Movement = Vector2.right; world.Hero.Tick(.02f,input);
            Assert.Greater(world.Hero.Model.FacingX,0); Assert.IsFalse(world.Hero.View.Renderer.flipX);
        }
        [Test] public void FacingSurvivesAnimatorEvaluationAndWeaponChange()
        {
            input.Movement = Vector2.left; world.Hero.Tick(.02f,input);
            world.Hero.View.Animation.Update(.05f); Assert.IsTrue(world.Hero.View.Renderer.flipX);
            world.Model.EquipWeapon(1); world.Hero.Tick(.02f,input);
            world.Hero.View.Animation.Update(.05f); Assert.IsTrue(world.Hero.View.Renderer.flipX);
            input.Movement = Vector2.right; world.Hero.Tick(.02f,input);
            world.Hero.View.Animation.Update(.05f); Assert.IsFalse(world.Hero.View.Renderer.flipX);
        }
        InventorySlotDrag Slot(int index)
        {
            foreach (var slot in entry.InventoryView.GetComponentsInChildren<InventorySlotDrag>(true)) if (slot.Index == index) return slot;
            throw new InvalidOperationException("Slot missing.");
        }
        UnityEngine.EventSystems.PointerEventData DragData(InventorySlotDrag source)
        {
            return new UnityEngine.EventSystems.PointerEventData(entry.UiManager.UiEventSystem) {
                button = UnityEngine.EventSystems.PointerEventData.InputButton.Left,
                position = new Vector2(640,360),pointerDrag = source.gameObject };
        }
        [Test] public void UguiDragMovesRealInventoryAndHidesGhost()
        {
            entry.UiManager.Bind(world); entry.UiManager.OpenBag();
            using (var controller = new InventoryController(world.Model,world.Events,entry.InventoryView,world))
            {
                entry.InventoryView.Bind(controller);
                var source = Slot(0); var target = Slot(5); var data = DragData(source);
                UnityEngine.EventSystems.ExecuteEvents.Execute(source.gameObject,data,UnityEngine.EventSystems.ExecuteEvents.beginDragHandler);
                Assert.IsTrue(entry.InventoryView.IsDragging); Assert.IsTrue(entry.InventoryView.DragIcon.gameObject.activeSelf);
                data.position += Vector2.right*80;
                UnityEngine.EventSystems.ExecuteEvents.Execute(source.gameObject,data,UnityEngine.EventSystems.ExecuteEvents.dragHandler);
                UnityEngine.EventSystems.ExecuteEvents.Execute(target.gameObject,data,UnityEngine.EventSystems.ExecuteEvents.dropHandler);
                UnityEngine.EventSystems.ExecuteEvents.Execute(source.gameObject,data,UnityEngine.EventSystems.ExecuteEvents.endDragHandler);
                Assert.AreEqual(0,world.Model.Inventory[0].amount); Assert.AreEqual(3,world.Model.Inventory[5].amount);
                Assert.IsFalse(entry.InventoryView.IsDragging); Assert.IsFalse(entry.InventoryView.DragIcon.gameObject.activeSelf);
                entry.InventoryView.Bind(null);
            }
            entry.UiManager.CloseAll(); entry.UiManager.Dispose();
        }
        [Test] public void ClosingBagCancelsDragWithoutMovingItems()
        {
            entry.UiManager.Bind(world); entry.UiManager.OpenBag();
            using (var controller = new InventoryController(world.Model,world.Events,entry.InventoryView,world))
            {
                entry.InventoryView.Bind(controller); var source = Slot(0);
                source.OnBeginDrag(DragData(source)); Assert.IsTrue(entry.InventoryView.IsDragging);
                entry.UiManager.CloseAll(); Assert.IsFalse(entry.InventoryView.IsDragging);
                Assert.IsFalse(entry.InventoryView.DragIcon.gameObject.activeSelf);
                Assert.IsFalse(entry.InventoryView.DropOn(0,5)); Assert.AreEqual(3,world.Model.Inventory[0].amount);
                entry.InventoryView.Bind(null);
            }
            entry.UiManager.Dispose();
        }
        [Test] public void DroppingOutsideOrChangingDataCancelsDrag()
        {
            entry.UiManager.Bind(world); entry.UiManager.OpenBag();
            using (var controller = new InventoryController(world.Model,world.Events,entry.InventoryView,world))
            {
                entry.InventoryView.Bind(controller); var source = Slot(0); var data = DragData(source);
                source.OnBeginDrag(data); source.OnEndDrag(data);
                Assert.AreEqual(3,world.Model.Inventory[0].amount); Assert.IsFalse(entry.InventoryView.IsDragging);
                source.OnBeginDrag(data); world.Model.Collect(ItemId.Mushroom,1);
                Assert.IsFalse(entry.InventoryView.IsDragging); Assert.IsFalse(entry.InventoryView.DropOn(0,5));
                Assert.AreEqual(3,world.Model.Inventory[0].amount); entry.InventoryView.Bind(null);
            }
            entry.UiManager.CloseAll(); entry.UiManager.Dispose();
        }

        [Test] public void IdleKeepsLastHeadingWithoutNewAimInput()
        {
            input.Movement = Vector2.left; world.Hero.Tick(.02f,input);
            input.Movement = Vector2.zero; world.Hero.Tick(.02f,input);
            Assert.Less(world.Hero.Model.FacingX,0); Assert.IsTrue(world.Hero.View.Renderer.flipX);
        }
    }
}
