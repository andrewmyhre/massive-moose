using System.Collections.Generic;
using System.Runtime.Serialization;

namespace massive_moose.services.models.drawing
{
    [DataContract(Namespace = "http://www.massivemoose.com/2016/xmlschema/contracts", Name = "brush")]
    [KnownType(typeof(SolidColorBrush))]
    public abstract class Brush { }
}