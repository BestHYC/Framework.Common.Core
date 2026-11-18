using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Core.ORM
{
    public class CKBatch : IBatch
    {
        public string Sql { get; set; }
        public DynamicParameters DynamicParameters { get; set; }
        public Object Value { get; set; }
    }
}
