using System;
using NUnit.Framework;
using SciLors_Mashed_Trainer.Core;
using SciLors_Mashed_Trainer.Types;

namespace SciLors_Mashed_Trainer.Tests {
    [TestFixture]
    public class MockMemoryTests {
        private MockMemoryTarget target = null!;
        private Game game = null!;

        [SetUp]
        public void Setup() {
            target = new MockMemoryTarget();
            game = new Game(target);
            for (int i = 0; i < 4; i++) {
                new Player(game, (Player.PlayerId)i);
            }
            game.Update();
        }

        [TearDown]
        public void Teardown() {
            game.Dispose();
        }

        [Test]
        public void TestInitialState() {
            Assert.That(game.PlayerCount, Is.EqualTo(4));
            Assert.That(game.MaximumPoints, Is.EqualTo(8));
            Assert.That(game.IsActive, Is.True);
            Assert.That(game.Players.Count, Is.EqualTo(4));

            Player p1 = game.Players[0];
            Assert.That(p1.IsAlive, Is.True);
            Assert.That(p1.IsBot, Is.False);
            Assert.That(p1.Points, Is.EqualTo(2));
            Assert.That(p1.Position.X, Is.EqualTo(10.0f).Within(0.01f));
        }

        [Test]
        public void TestFlipCar() {
            Player p1 = game.Players[0];
            Assert.That(p1.IsOnRoof, Is.False);

            // In RenderWare / game matrix, height is Z (pos[1])
            float initialZ = p1.Position.Z;
            p1.Flip();
            game.Update();

            Assert.That(p1.IsOnRoof, Is.True, "Car should be on roof after flip");
            Assert.That(p1.Position.Z, Is.EqualTo(initialZ + 1.5f).Within(0.01f), "Car should be lifted by 1.5 after flip");

            // Flip back
            p1.Flip();
            game.Update();
            Assert.That(p1.IsOnRoof, Is.False, "Car should be upright after second flip");
        }

        [Test]
        public void TestTurnCar() {
            Player p1 = game.Players[0];
            p1.Turn(180);
            game.Update();

            // Right vector should be negated (-1, 0, 0)
            int currentMatrixOffset = 0x8B06E0 + 0x928;
            float rightX = target.Read<float>(new IntPtr(currentMatrixOffset + 0x00));
            Assert.That(rightX, Is.EqualTo(-1.0f).Within(0.01f));
        }

        [Test]
        public void TestDamageAndRepair() {
            Player p1 = game.Players[0];
            p1.DamageFront = 55.0f;
            p1.DamageBack = 80.0f;
            p1.IsDamagedHood = true;
            p1.IsDamagedTrunk = true;
            game.Update();

            Assert.That(p1.DamageFront, Is.EqualTo(55.0f).Within(0.01f));
            Assert.That(p1.DamageBack, Is.EqualTo(80.0f).Within(0.01f));
            Assert.That(p1.IsDamagedHood, Is.True);
            Assert.That(p1.IsDamagedTrunk, Is.True);

            p1.Repair();
            game.Update();

            Assert.That(p1.DamageFront, Is.EqualTo(0.0f).Within(0.01f));
            Assert.That(p1.DamageBack, Is.EqualTo(0.0f).Within(0.01f));
            Assert.That(p1.IsDamagedHood, Is.False);
            Assert.That(p1.IsDamagedTrunk, Is.False);
        }

        [Test]
        public void TestGodModeKeepsCarRepaired() {
            Player p1 = game.Players[0];
            p1.Settings.IsGodMode = true;
            p1.DamageFront = 55.0f;
            p1.IsDamagedHood = true;
            game.Update();
            game.Update();

            Assert.That(p1.DamageFront, Is.EqualTo(0.0f).Within(0.01f));
            Assert.That(p1.IsDamagedHood, Is.False);
        }
    }
}
