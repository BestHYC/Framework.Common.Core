using System;
using System.Collections.Generic;
using System.Text;

namespace ClickHouseEntityTemplate
{
    public class DescriptionTable
    {
        public String Name { set; get; }

        public String Type { set; get; }

        /// <summary>
        /// 是否是主键
        /// 1是
        /// </summary>
        public bool is_in_primary_key { set; get; }

        public String Comment { set; get; }

    }
}
