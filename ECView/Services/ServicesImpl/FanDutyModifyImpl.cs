using ECView.DataDefinations;
using ECView.Tools;
using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;

namespace ECView.Services.ServicesImpl
{
    public class FanDutyModifyImpl : IFanDutyModify
    {
        /// <summary>
        /// 设置风扇转速
        /// </summary>
        /// <param name="fanNo">风扇号</param>
        /// <param name="fanduty">风扇转速</param>
        /// <param name="isAuto">自动调节标识</param>
        /// <returns>风扇实际转速</returns>
        [NotNull]
        public int[] SetFanduty(int fanNo, int fanduty, bool isAuto)
        {
            try
            {
                if (isAuto)
                {
                    //自动调节风扇转速
                    DataTool.SetFANDutyAuto(fanNo);
                }
                else
                {
                    //将风扇转速设置为定值
                    DataTool.SetFanDuty(fanNo, fanduty);
                }
                Thread.Sleep(500);
                var newFanduty = _updateFanDuty(fanNo);
                return newFanduty;
            }
            catch (Exception e)
            {
                Console.WriteLine(@"设置风扇转速错误，原因：" + e.Message);
                int[] newFanduty = { -1, -1, -1 };
                return newFanduty;
            }
        }
        /// <summary>
        /// 获取EC版本号
        /// </summary>
        /// <returns>EC版本</returns>
        [NotNull]
        public string GetEcVersion()
        {
            try
            {
                var ecVersion = DataTool.GetECVersion();
                return ecVersion;
            }
            catch (Exception e)
            {
                Console.WriteLine(@"获取EC版本错误，原因：" + e.Message);
                return "";
            }
        }
        /// <summary>
        /// 获取风扇转速与温度数据
        /// </summary>
        /// <returns>转速与温度结构体</returns>
        [NotNull]
        public int[] GetTempFanDuty(int fanNo)
        {
            try
            {
                var ecData = DataTool.GetTempFanDuty(fanNo);
                var byteData = ecData.Data;
                var ec = BitConverter.GetBytes(byteData);
                int[] fanduty = { 0, 0, 0 };
                fanduty[0] = ec[0];
                fanduty[1] = ec[1];
                fanduty[2] = (int)Math.Round(ec[2] / 2.55m);
                return fanduty;
            }
            catch (Exception e)
            {
                Console.WriteLine(@"获取风扇转速与温度数据错误，原因：" + e.Message);
                int[] fanduty = { -1, -1, -1 };
                return fanduty;
            }
        }
        /// <summary>
        /// 获取风扇数量
        /// </summary>
        /// <returns>风扇数量</returns>
        public int GetFanCount()
        {
            try
            {
                var fanCount = DataTool.GetFANCounter();
                return fanCount;
            }
            catch (Exception e)
            {
                Console.WriteLine(@"获取风扇数量错误，原因：" + e.Message);
                return -1;
            }
        }
        /// <summary>
        /// 检测服务是否启动
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns>服务状态</returns>
        public int CheckServiceState(string serviceName)
        {
            try
            {
                var serviceState = ServiceTool.CheckServiceState(serviceName);
                return serviceState;
            }
            catch (Exception e)
            {
                Console.WriteLine(@"获取服务状态错误，原因：" + e.Message);
                return -1;
            }
        }
        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns>启动状态</returns>
        public bool StartService(string serviceName)
        {
            var flag = ServiceTool.StartService(serviceName);
            return flag;
        }
        /// <summary>
        /// 停止服务
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns></returns>
        public bool StopService(string serviceName)
        {
            var flag = ServiceTool.StopService(serviceName);
            return flag;
        }
        /// <summary>
        /// 写入配置文件
        /// </summary>
        /// <param name="filename">配置文件名</param>
        /// <param name="configParaList">风扇配置</param>
        public void WriteCfgFile(string filename, List<ConfigPara> configParaList)
        {
            FileTool.WriteCfgFile(filename, configParaList);
        }
        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <param name="filename">配置文件名</param>
        /// <returns>风扇配置</returns>
        [NotNull]
        public List<ConfigPara> ReadCfgFile(string filename)
        {
            return FileTool.ReadCfgFile(filename);
        }
        /// <summary>
        /// 智能控制
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fanNo"></param>
        /// <returns></returns>
        public bool InteFandutyControl(string filePath, int fanNo)
        {
            var state = _inteFandutyControl(filePath, fanNo);
            return state;
        }

        #region 私有方法
        /// <summary>
        /// 智能调节风扇转速
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fanNo"></param>
        /// <returns></returns>
        private bool _inteFandutyControl(string filePath, int fanNo)
        {
            try
            {
                var intePara = FileTool.ReadXmlFile(filePath, fanNo);
                _setInteFanduty(intePara);
                return true;
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 智能调节风扇转速
        /// </summary>
        /// <param name="intePara"></param>
        private void _setInteFanduty(IntePara intePara)
        {
            var ecData = GetTempFanDuty(intePara.FanNo);
            switch (intePara.ControlType)
            {
                case 1:
                    _inteControl(intePara, ecData);
                    break;
                case 2:
                    _inteControl(intePara, ecData);
                    break;
            }
        }

        /// <summary>
        /// 智能控制方案
        /// </summary>
        /// <param name="intePara"></param>
        /// <param name="ecData"></param>
        private void _inteControl(IntePara intePara, IList<int> ecData)
        {
            if (intePara.RangeParaList == null) return;
            var count = intePara.RangeParaList.Count;//风扇数量
            var fanNo = intePara.FanNo;//风扇号
            var minFanDuty = _checkFanDuty(intePara.MinFanDuty, 100, 2);//最小转速
            for (var i = 0; i < count; i++)
            {
                var rp = intePara.RangeParaList[i];//当前遍历标签
                var fanDuty = 0;//目标转速
                switch (intePara.ControlType)
                {
                    case 1:
                        fanDuty = _checkFanDuty(rp.FanDuty, 100, 2);
                        break;
                    case 2:
                        fanDuty = _checkFanDuty(rp.AddPercentage + ecData[0], 100, 2);
                        break;
                }
                var inferiorLimit = rp.InferiorLimit;//当前标签温度下限
                if (i == 0)
                {
                    //若小于最小温度，设置风扇转速为最小值
                    if (ecData[0] < inferiorLimit)
                    {
                        SetFanduty(intePara.FanNo, _parseFanDuty(minFanDuty), false);
                    }
                    else if (_checkLogicalAnd(ecData[0], inferiorLimit, intePara.RangeParaList[i + 1].InferiorLimit))
                    {
                        _compareFanDuty(fanNo, minFanDuty, fanDuty);
                    }
                }
                else if (i != count - 1)
                {
                    if (_checkLogicalAnd(ecData[0], inferiorLimit, intePara.RangeParaList[i + 1].InferiorLimit))
                    {
                        _compareFanDuty(fanNo, minFanDuty, fanDuty);
                    }
                }
                else if (i == count - 1)
                {
                    if (ecData[0] >= inferiorLimit)
                    {
                        _compareFanDuty(fanNo, minFanDuty, fanDuty);
                    }
                }
            }
        }
        /// <summary>
        /// 风扇转速逻辑判断
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="op"></param>
        /// <returns></returns>
        private static int _checkFanDuty(int first, int second, int op)
        {
            var output = 0;
            switch (op)
            {
                case 1:
                    output = first > second ? first : second;
                    break;
                case 2:
                    output = first > second ? second : first;
                    break;
            }
            return output;
        }
        /// <summary>
        /// 风扇转速格式转换
        /// </summary>
        /// <param name="fanDuty"></param>
        /// <returns></returns>
        private static int _parseFanDuty(int fanDuty)
        {
            return (int)(fanDuty * 2.55m);
        }
        /// <summary>
        /// 风扇转速区间判断
        /// </summary>
        /// <param name="fanNo"></param>
        /// <param name="minFanDuty"></param>
        /// <param name="fanDuty"></param>
        private void _compareFanDuty(int fanNo, int minFanDuty, int fanDuty)
        {
            var duty = _checkFanDuty(minFanDuty, fanDuty, 1);
            SetFanduty(fanNo, _parseFanDuty(duty), false);
        }
        /// <summary>
        /// 温度区间逻辑判断
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private static bool _checkLogicalAnd(int a, int b, int c)
        {
            return a >= b && a < c;
        }
        /// <summary>
        /// 更新风扇转速状态
        /// </summary>
        private int[] _updateFanDuty(int fanNo)
        {
            var fanduty = GetTempFanDuty(fanNo);
            return fanduty;
        }
        #endregion
    }
}