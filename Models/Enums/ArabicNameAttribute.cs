
namespace APIBase.Models.Enums
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ArabicNameAttribute : Attribute
    {
        public string ArabicName { get; }

        public ArabicNameAttribute(string arabicName)
        {
            ArabicName = arabicName;
        }
    }
}
