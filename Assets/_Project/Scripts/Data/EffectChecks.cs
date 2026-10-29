using System;
using Game.Core;

namespace Game.Data
{
    internal static class EffectChecks
    {
        public static void CheckEnum<T>(ValidationReport report, string owner, object context, string effect, T value)
            where T : Enum
        {
            if (!Enum.IsDefined(typeof(T), value))
            {
                report.AddError(owner, $"{effect}: unknown {typeof(T).Name} value {value}", context);
            }
        }

        public static void CheckModifier(
            ValidationReport report, string owner, object context, string effect, StatId stat, ModifierOp op, float valuePerStack)
        {
            CheckEnum(report, owner, context, effect, stat);
            CheckEnum(report, owner, context, effect, op);

            if (valuePerStack == 0f || float.IsNaN(valuePerStack) || float.IsInfinity(valuePerStack))
            {
                report.AddError(owner, $"{effect}: value for {stat} must be a non-zero number, is {valuePerStack}", context);
            }

            if (StatIds.IsInteger(stat))
            {
                if (op != ModifierOp.Flat)
                {
                    report.AddError(owner, $"{effect}: {stat} is an integer stat and accepts only Flat, got {op}", context);
                }

                if (valuePerStack != (float)Math.Round(valuePerStack))
                {
                    report.AddError(owner, $"{effect}: {stat} is an integer stat, value must be whole, is {valuePerStack}", context);
                }
            }
        }
    }
}
