using System;
using System.Collections.Generic;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class TriggerBusTests
    {
        [Test]
        public void GameTriggerCount_MatchesEnum()
        {
            Assert.That(GameTriggers.Count, Is.EqualTo(Enum.GetValues(typeof(GameTrigger)).Length));
        }

        [Test]
        public void Raise_CallsOnlyHandlersOfThatTrigger()
        {
            var bus = new TriggerBus();
            var onKill = new RecordingHandler();
            var onHit = new RecordingHandler();
            bus.Subscribe(GameTrigger.Kill, onKill, Instance(1));
            bus.Subscribe(GameTrigger.Hit, onHit, Instance(2));

            bus.Raise(new TriggerArgs(GameTrigger.Kill, 0f), null);

            Assert.That(onKill.Calls, Is.EqualTo(1));
            Assert.That(onHit.Calls, Is.EqualTo(0));
        }

        [Test]
        public void Raise_PassesArgsAndInstance()
        {
            var bus = new TriggerBus();
            var handler = new RecordingHandler();
            EffectInstance instance = Instance(7);
            bus.Subscribe(GameTrigger.DamageTaken, handler, instance);

            bus.Raise(new TriggerArgs(GameTrigger.DamageTaken, 12.5f), null);

            Assert.That(handler.LastAmount, Is.EqualTo(12.5f));
            Assert.That(handler.LastInstance, Is.SameAs(instance));
        }

        [Test]
        public void Handlers_RunInSubscriptionOrder()
        {
            var bus = new TriggerBus();
            var order = new List<int>();
            bus.Subscribe(GameTrigger.LevelUp, new RecordingHandler(order, 1), Instance(1));
            bus.Subscribe(GameTrigger.LevelUp, new RecordingHandler(order, 2), Instance(2));

            bus.Raise(new TriggerArgs(GameTrigger.LevelUp, 0f), null);

            Assert.That(order, Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void UnsubscribeSource_RemovesItsHandlersFromAllTriggers()
        {
            var bus = new TriggerBus();
            var removed = new RecordingHandler();
            var kept = new RecordingHandler();
            EffectInstance instance = Instance(1);
            bus.Subscribe(GameTrigger.Kill, removed, instance);
            bus.Subscribe(GameTrigger.Hit, removed, instance);
            bus.Subscribe(GameTrigger.Kill, kept, Instance(2));

            bus.UnsubscribeSource(instance.Source);
            bus.Raise(new TriggerArgs(GameTrigger.Kill, 0f), null);
            bus.Raise(new TriggerArgs(GameTrigger.Hit, 0f), null);

            Assert.That(removed.Calls, Is.EqualTo(0));
            Assert.That(kept.Calls, Is.EqualTo(1));
        }

        [Test]
        public void UnsubscribingDuringRaise_Throws()
        {
            var bus = new TriggerBus();
            EffectInstance instance = Instance(1);
            bus.Subscribe(GameTrigger.Kill, new UnsubscribingHandler(bus), instance);

            Assert.Throws<InvalidOperationException>(() => bus.Raise(new TriggerArgs(GameTrigger.Kill, 0f), null));
        }

        private static EffectInstance Instance(int source)
        {
            return new EffectInstance(new ModifierSourceId(source), 1);
        }

        private sealed class RecordingHandler : ITriggerHandler
        {
            private readonly List<int> _order;
            private readonly int _id;

            public RecordingHandler(List<int> order = null, int id = 0)
            {
                _order = order;
                _id = id;
            }

            public int Calls { get; private set; }

            public float LastAmount { get; private set; }

            public EffectInstance LastInstance { get; private set; }

            public void OnTrigger(in TriggerArgs args, EffectInstance instance, IEffectContext context)
            {
                Calls++;
                LastAmount = args.Amount;
                LastInstance = instance;
                _order?.Add(_id);
            }
        }

        private sealed class UnsubscribingHandler : ITriggerHandler
        {
            private readonly TriggerBus _bus;

            public UnsubscribingHandler(TriggerBus bus)
            {
                _bus = bus;
            }

            public void OnTrigger(in TriggerArgs args, EffectInstance instance, IEffectContext context)
            {
                _bus.UnsubscribeSource(instance.Source);
            }
        }
    }
}
