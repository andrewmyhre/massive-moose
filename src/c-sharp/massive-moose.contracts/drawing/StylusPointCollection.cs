using System.Collections.Generic;
using System.Runtime.Serialization;

namespace massive_moose.contracts.drawing
{
    [CollectionDataContract(Namespace = "http://www.massivemoose.com/2016/xmlschema/contracts",
        Name = "stylusPointCollection", ItemName = "stylusPoint")]
    public class StylusPointCollection : List<StylusPoint>
    {
        public StylusPointCollection()
        {
            
        }

        public StylusPointCollection(IEnumerable<StylusPoint> stylusPoints)
        {
            AddRange(stylusPoints);
        }
    }
}
