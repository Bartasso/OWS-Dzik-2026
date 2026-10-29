using System;
using System.Collections.Generic;
using Game.Core;
using Game.Data;

namespace Game.Gameplay
{
    /// <summary>
    /// Trinkets and talents owned by one character: installs their effects, tracks stacks,
    /// and is the context effects run in.
    /// </summary>
    public sealed class EffectLoadout : IEffectContext
    {
        private readonly Dictionary<EffectDefinition, Owned> _owned = new Dictionary<EffectDefinition, Owned>();
        private readonly TriggerBus _triggers = new TriggerBus();
        private readonly IRandomSource _procRandom;
        private readonly float _procChancePerLuck;

        /// <param name="procRandom">Rng.Combat of the current run.</param>
        public EffectLoadout(StatSheet stats, ScalingTable scaling, IRandomSource procRandom, float procChancePerLuck)
        {
            Stats = stats ?? throw new ArgumentNullException(nameof(stats));
            Scaling = scaling ?? throw new ArgumentNullException(nameof(scaling));
            _procRandom = procRandom ?? throw new ArgumentNullException(nameof(procRandom));
            _procChancePerLuck = procChancePerLuck;
        }

        public StatSheet Stats { get; }

        public ScalingTable Scaling { get; }

        public int GetStacks(EffectDefinition definition)
        {
            return _owned.TryGetValue(definition, out Owned owned) ? owned.Stacks : 0;
        }

        /// <summary>Adds one copy: installs the effects, or raises their stack count if already owned.</summary>
        public void Add(EffectDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (_owned.TryGetValue(definition, out Owned owned))
            {
                owned.Stacks++;
                for (int i = 0; i < owned.Instances.Length; i++)
                {
                    owned.Instances[i].Stacks = owned.Stacks;
                    definition.Effects[i].OnStacksChanged(owned.Instances[i], this);
                }

                return;
            }

            IReadOnlyList<Effect> effects = definition.Effects;
            var instances = new EffectInstance[effects.Count];
            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] == null)
                {
                    throw new InvalidOperationException($"{definition.name}: effect #{i} is missing; run Tools/Validate All Data");
                }

                instances[i] = new EffectInstance(Stats.CreateSource(), 1);
                effects[i].Install(instances[i], this);
            }

            _owned.Add(definition, new Owned(instances));
        }

        /// <summary>Removes the definition with all its copies and everything its effects added.</summary>
        public void Remove(EffectDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (!_owned.TryGetValue(definition, out Owned owned))
            {
                throw new InvalidOperationException($"{definition.name} is not owned");
            }

            foreach (EffectInstance instance in owned.Instances)
            {
                Stats.RemoveSource(instance.Source);
                Scaling.RemoveSource(instance.Source);
                _triggers.UnsubscribeSource(instance.Source);
            }

            _owned.Remove(definition);
        }

        public void Raise(in TriggerArgs args)
        {
            _triggers.Raise(in args, this);
        }

        void IEffectContext.Subscribe(GameTrigger trigger, ITriggerHandler handler, EffectInstance instance)
        {
            _triggers.Subscribe(trigger, handler, instance);
        }

        public bool RollProc(float baseChance)
        {
            float chance = StatFormulas.ProcChance(baseChance, Stats.GetAbility(Ability.Lck), _procChancePerLuck);
            // Certain outcomes skip the draw, so they don't consume the stream.
            if (chance <= 0f)
            {
                return false;
            }

            return chance >= 1f || _procRandom.NextFloat() < chance;
        }

        private sealed class Owned
        {
            public Owned(EffectInstance[] instances)
            {
                Instances = instances;
                Stacks = 1;
            }

            public EffectInstance[] Instances { get; }

            public int Stacks { get; set; }
        }
    }
}
