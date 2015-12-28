using ECView.DataDefinations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using JetBrains.Annotations;

namespace ECView.Tools
{
    public class FileTool
    {
        /// <summary>
        /// 读取XML文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fanNo"></param>
        [CanBeNull]
        public static IntePara ReadXmlFile(string filePath, int fanNo)
        {
            //声明XMLDocument对象
            var doc = new XmlDocument();
            try
            {
                //数据存储结构体
                var inte = new IntePara {FanNo = fanNo};
                //范围数据存储结构体列表
                var rangeParaList = new List<RangePara>();
                //加载XML文件
                doc.Load(filePath);
                //获取XML根结点
                var root = doc.SelectSingleNode("ECView");
                //获取根结点属性--智能控速模式号
                var rootXe = (XmlElement)root;
                inte.ControlType = Convert.ToInt32(rootXe?.GetAttribute("Type"));
                //获取下层结点
                var fanList = root?.ChildNodes;
                if (fanList != null)
                    foreach (XmlNode fan in fanList)
                    {
                        //获取下层结点属性--风扇最小转速
                        var fanXe = (XmlElement)fan;
                        inte.MinFanDuty = Convert.ToInt32(fanXe.GetAttribute("MinFanduty"));
                        //获取范围数据
                        var rangeList = fan.ChildNodes;
                        foreach (XmlNode rangeXn in rangeList)
                        {
                            var rangePara = new RangePara();
                            var rangeXe = (XmlElement)rangeXn;
                            //获取范围号
                            rangePara.RangeNo = Convert.ToInt32(rangeXe.GetAttribute("Num"));
                            //获取范围下限
                            rangePara.InferiorLimit = Convert.ToInt32(rangeXe.GetAttribute("InferiorLimit"));
                            switch (inte.ControlType)
                            {
                                case 1:
                                    //控速模式1下直接定义风扇转速
                                    rangePara.FanDuty = Convert.ToInt32(rangeXe.GetAttribute("Fanduty"));
                                    break;
                                case 2:
                                    //控速模式2下为温度+自定义增幅百分比
                                    rangePara.AddPercentage = Convert.ToInt32(rangeXe.GetAttribute("AddPercentage"));
                                    break;
                            }
                            rangeParaList.Add(rangePara);
                        }
                    }
                inte.RangeParaList = rangeParaList;
                return inte;
            }
            catch (Exception e)
            {
                Console.WriteLine(@"解析XML出错，原因：" + e.Message);
                return null;
            }
        }
        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <param name="filePath">配置文件名</param>
        /// <returns>风扇配置</returns>
        [CanBeNull]
        public static List<ConfigPara> ReadCfgFile(string filePath)
        {
            var sr = new StreamReader(filePath);
            try
            {
                var configParaList = new List<ConfigPara>();

                var lines = new List<string>();

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    lines.Add(line);
                }
                for (var i = 6; i < lines.Count; i++)
                {
                    var configPara = new ConfigPara();
                    var nbModel = lines[3].Split('\t')[1];
                    var ecVersion = lines[4].Split('\t')[1];
                    var fanNo = Convert.ToInt32(lines[i].Split('\t')[1]);
                    var setMode = Convert.ToInt32(lines[i].Split('\t')[3]);
                    var fanSet = lines[i].Split('\t')[5];
                    var fanDuty = Convert.ToInt32(lines[i].Split('\t')[7]);
                    configPara.NbModel = nbModel;
                    configPara.EcVersion = ecVersion;
                    configPara.FanNo = fanNo;
                    configPara.SetMode = setMode;
                    configPara.FanSet = fanSet;
                    configPara.FanDuty = fanDuty;
                    configParaList.Add(configPara);
                }
                return configParaList;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 写入配置文件
        /// </summary>
        /// <param name="filePath">配置文件路径</param>
        /// <param name="configParaList">风扇配置</param>
        public static void WriteCfgFile(string filePath, List<ConfigPara> configParaList)
        {
            var sw = new StreamWriter(filePath);
            try
            {
                sw.WriteLine("#ECView");
                sw.WriteLine("#Author YcraD");
                sw.WriteLine("#Config File -- DO NOT EDIT!");
                sw.WriteLine("MbModel" + "\t" + configParaList[0].NbModel);
                sw.WriteLine("ECVersion" + "\t" + configParaList[0].EcVersion);
                sw.WriteLine("FanCount" + "\t" + configParaList.Count);
                foreach (var configPara in configParaList)
                {
                    sw.WriteLine("FanNo" + "\t" + configPara.FanNo + "\t" + "SetMode" + "\t" + configPara.SetMode + "\t" + "FanSet" + "\t" + configPara.FanSet + "\t" + "FanDuty" + "\t" + configPara.FanDuty);
                }
                sw.Close();
            }
            catch
            {
                // ignored
            }
        }
    }
}
