using Framework.Common.Core;
using Framework.Common.Core.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Test.CKORMTest
{
    public interface IAdgroupRepository : IBaseRepository<AdgroupModel>
    {

    }
    public class AdgroupCKRepository : CKBaseRepository<AdgroupModel>, IAdgroupRepository
    {
        public AdgroupCKRepository(CKExecuteBatch<AdgroupModel> batch) : base(batch)
        {
        }
    }
}
