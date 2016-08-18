using System.Runtime.Serialization;
namespace massive_moose.contracts
{
    [DataContract(Namespace = "http://www.massivemoose.com/2016/xmlschema/contracts", Name = "solidColorBrush")]
    public class SolidColorBrush : Brush
    {
        [DataMember(Name = "Color")]
        public Color Color { get; private set; }

        public SolidColorBrush(Color color)
        {
            Color = color;
        }
    }
}