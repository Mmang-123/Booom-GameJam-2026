
namespace Mmang.Game
{

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class MPropertyAttribute : System.Attribute
    {
        public System.Type propertyType;

        public MPropertyAttribute(System.Type propertyType)
        {
            this.propertyType = propertyType;
        }
    }

}