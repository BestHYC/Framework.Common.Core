using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Core
{
    public static class ServiceHelper
    {
        public static IServiceProvider Instance { get; set; }
        public static T GetService<T>()
        {
            if (Instance == null)
            {
                throw new Exception("请先在Startup Configure中配置ApplicationServices,ServiceLocator.Instance = app.ApplicationServices;");
            }
            return Instance.GetService<T>();
        }
    }
}
