using MongoDB.Bson.Serialization.Attributes;
using System;

namespace SimpleAnalyticsDashbord.Models
{
    [BsonIgnoreExtraElements]
    public class ParentChildClass
    {
        public string ParentCatagory { get; set; }
        public string MiddleCatagory { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime DateTime { get; set; }
        public ChildClass ChildCatagory { get; set; }

    }

}
