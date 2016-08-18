using System.Collections.Generic;
using System.Runtime.Serialization;
namespace massive_moose.contracts
{
    [CollectionDataContract(Namespace = "http://www.massivemoose.com/2016/xmlschema/contracts", Name = "strokeCollection", ItemName ="stroke")]
    public class StrokeCollection : List<Stroke>
    {
        
    }
}