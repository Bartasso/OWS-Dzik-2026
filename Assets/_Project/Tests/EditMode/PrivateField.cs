using System.Reflection;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Sets serialized private fields without SerializedObject, which would trigger OnValidate logging.
    /// </summary>
    internal static class PrivateField
    {
        public static void Set(object target, string fieldName, object value)
        {
            FieldInfo field = null;
            for (var type = target.GetType(); type != null && field == null; type = type.BaseType)
            {
                field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            }

            Assert.That(field, Is.Not.Null, $"{target.GetType().Name} has no field '{fieldName}'");
            field.SetValue(target, value);
        }
    }
}
