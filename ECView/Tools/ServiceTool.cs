﻿using System;
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
            ServiceController[] service = ServiceController.GetServices();
            bool isStart = false;
            bool isExite = false;
            for (int i = 0; i < service.Length; i++)
            {
                if (service[i].ServiceName.ToUpper().Equals(serviceName.ToUpper()))
                {
                    isExite = true;
                    ServiceController server = service[i];
                    if (service[i].Status == ServiceControllerStatus.Running)
                    {
                        isStart = true;
                        break;
                    }
                }
            }

            if (!isExite)
            {
                //服务不存在
                return 0;
            }else{
                if (isStart)
                {
                    //服务存在并启动
                    return 1;
                }
                else
                {
                    //服务存在但未启动
                    return 2;
                }
            }
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
                ServiceController service = new ServiceController(serviceName);
                if (service.Status == ServiceControllerStatus.Running) 
                { 
                    //服务已启动
                    return true; 
                } 
                else 
                {
                    //服务未启动
                    //设置timeout
                    TimeSpan timeout = TimeSpan.FromMilliseconds(1000 * 10); 
                    service.Start();//启动程序
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("启动服务错误，原因：" + e.Message);
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
                ServiceController service = new ServiceController(serviceName);
                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    //服务已停止
                    return true;
                }
                else
                {
                    //服务未停止
                    //设置timeout
                    TimeSpan timeout = TimeSpan.FromMilliseconds(1000 * 10);
                    service.Stop();//停止程序
                    service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("停止服务错误，原因：" + e.Message);
                return false;
            }
        }
    }
}
