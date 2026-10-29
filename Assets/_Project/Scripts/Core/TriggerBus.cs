using System;
using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// Per-trigger handler lists. Subscribing happens when an effect is installed, so raising is an
    /// allocation-free loop.
    /// </summary>
    public sealed class TriggerBus
    {
        private readonly List<Subscription>[] _subscriptions = new List<Subscription>[GameTriggers.Count];
        private int _raiseDepth;

        public TriggerBus()
        {
            for (int i = 0; i < _subscriptions.Length; i++)
            {
                _subscriptions[i] = new List<Subscription>();
            }
        }

        public void Subscribe(GameTrigger trigger, ITriggerHandler handler, EffectInstance instance)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            ListFor(trigger).Add(new Subscription(handler, instance));
        }

        public void UnsubscribeSource(ModifierSourceId source)
        {
            if (_raiseDepth > 0)
            {
                throw new InvalidOperationException("cannot unsubscribe while a trigger is being raised");
            }

            foreach (List<Subscription> list in _subscriptions)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i].Instance.Source == source)
                    {
                        list.RemoveAt(i);
                    }
                }
            }
        }

        public void Raise(in TriggerArgs args, IEffectContext context)
        {
            List<Subscription> list = ListFor(args.Trigger);
            // Handlers subscribed during this raise run from the next one.
            int count = list.Count;
            _raiseDepth++;
            try
            {
                for (int i = 0; i < count; i++)
                {
                    Subscription subscription = list[i];
                    subscription.Handler.OnTrigger(in args, subscription.Instance, context);
                }
            }
            finally
            {
                _raiseDepth--;
            }
        }

        private List<Subscription> ListFor(GameTrigger trigger)
        {
            int index = (int)trigger;
            if ((uint)index >= GameTriggers.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(trigger), trigger, null);
            }

            return _subscriptions[index];
        }

        private readonly struct Subscription
        {
            public Subscription(ITriggerHandler handler, EffectInstance instance)
            {
                Handler = handler;
                Instance = instance;
            }

            public ITriggerHandler Handler { get; }

            public EffectInstance Instance { get; }
        }
    }
}
