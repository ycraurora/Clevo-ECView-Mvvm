using System;
using System.Linq;
using System.ServiceProcess;

namespace ECView.Tools
{
    public class ServiceTool
    {
        /// <summary>
        /// 检测服务
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns>服务状态</returns>
        public static int CheckServiceState(string serviceName)
        {
            var service = ServiceController.GetServices();
            var isStart = false;
            var isExite = false;
            foreach (var t in service.Where(t => t.ServiceName.ToUpper().Equals(serviceName.ToUpper())))
            {
                isExite = true;
                if (t.Status != ServiceControllerStatus.Running) continue;
                isStart = true;
                break;
            }

            if (!isExite)
            {
                //服务不存在
                return 0;
            }
            return isStart ? 1 : 2;
        }
        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns>启动状态</returns>
        public static bool StartService(string serviceName)
        {
            try
            {
                var service = new ServiceController(serviceName);
                if (service.Status == ServiceControllerStatus.Running) 
                { 
                    //服务已启动
                    return true; 
                }
                //服务未启动
                //设置timeout
                var timeout = TimeSpan.FromMilliseconds(1000 * 10); 
                service.Start();//启动程序
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(@"启动服务错误，原因：" + e.Message);
                return false;
            }
        }
        /// <summary>
        /// 停止服务
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns>停止状态</returns>
        public static bool StopService(string serviceName)
        {
            try
            {
                var service = new ServiceController(serviceName);
                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    //服务已停止
                    return true;
                }
                //服务未停止
                //设置timeout
                var timeout = TimeSpan.FromMilliseconds(1000 * 10);
                service.Stop();//停止程序
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(@"停止服务错误，原因：" + e.Message);
                return false;
            }
        }
    }
}
