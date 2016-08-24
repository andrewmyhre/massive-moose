using System.Collections.Generic;
using System.Runtime.Serialization;

namespace massive_moose.services.models.drawing
{
    [DataContract(Namespace = "http://www.massivemoose.com/2016/xmlschema/contracts", Name = "inkPresenter")]
    public class InkPresenter : UIElement
    {
        [DataMember(Name ="strokes")]
        public StrokeCollection Strokes { get; private set; }
        public InkPresenter()
        {
            Strokes = new StrokeCollection();
        }
    }
}