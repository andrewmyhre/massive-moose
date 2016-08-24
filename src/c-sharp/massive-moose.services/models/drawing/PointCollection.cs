using System.Collections.Generic;
using System.Runtime.Serialization;

namespace massive_moose.services.models.drawing
{
    [CollectionDataContract(ItemName="point",Namespace = "http://www.massivemoose.com/2016/xmlschema/contracts", Name = "canvas")]
    public class PointCollection : List<Point>
    {
        
    }
}