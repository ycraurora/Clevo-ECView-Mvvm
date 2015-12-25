using Caliburn.Micro;
using ECView.DataDefinations;
using ECView.Frameworks;
using ECView.Services;
using System;
using System.Collections.Generic;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ECView.ViewModels
{
    public class ECMainViewModel : Screen, IShell
    {
        #region 绑定数据
        public class ECViewBinding : PropertyChangedBase
        {
            /// <summary>
            /// 风扇号
            /// </summary>
            private int _fanNo;
            public int FanNo
            {
                get { return _fanNo; }
                set { _fanNo = value; NotifyOfPropertyChange(() => FanNo); }
            }
            /// <summary>
            /// 风扇转速
            /// </summary>
            private string _fanDutyStr;
            public string FanDutyStr
            {
                get { return _fanDutyStr; }
                set { _fanDutyStr = value; NotifyOfPropertyChange(() => FanDutyStr); }
            }
            /// <summary>
            /// 风扇转速
            /// </summary>
            private int _fanDuty;
            public int FanDuty
            {
                get { return _fanDuty; }
                set { _fanDuty = value; NotifyOfPropertyChange(() => FanDuty); }
            }
            /// <summary>
            /// 风扇当前设置
            /// </summary>
            private string _fanSet;
            public string FanSet
            {
                get { return _fanSet; }
                set { _fanSet = value; NotifyOfPropertyChange(() => FanSet); }
            }
            /// <summary>
            /// 设置模式
            /// </summary>
            private int _fanSetModel;
            public int FanSetModel
            {
                get { return _fanSetModel; }
                set { _fanSetModel = value; NotifyOfPropertyChange(() => FanSetModel); }
            }
            /// <summary>
            /// 更新设置标识
            /// </summary>
            private bool _updateFlag;
            public bool UpdateFlag
            {
                get { return _updateFlag; }
                set { _updateFlag = value; NotifyOfPropertyChange(() => UpdateFlag); }
            }
        }
        /// <summary>
        /// CPU温度
        /// </summary>
        private string _cpuLocal;
        public string CpuLocal
        {
            get { return _cpuLocal; }
            set { _cpuLocal = value; NotifyOfPropertyChange(() => CpuLocal); }
        }
        /// <summary>
        /// 主板温度
        /// </summary>
        private string _cpuRemote;
        public string CpuRemote
        {
            get { return _cpuRemote; }
            set { _cpuRemote = value; NotifyOfPropertyChange(() => CpuRemote); }
        }
        /// <summary>
        /// 模具型号
        /// </summary>
        private string _nbModel;
        public string NbModel
        {
            get { return _nbModel; }
            set { _nbModel = value; NotifyOfPropertyChange(() => NbModel); }
        }
        /// <summary>
        /// EC版本
        /// </summary>
        private string _ecVersion;
        public string ECVersion
        {
            get { return _ecVersion; }
            set { _ecVersion = value; NotifyOfPropertyChange(() => ECVersion); }
        }
        /// <summary>
        /// EC数据列表
        /// </summary>
        private BindableCollection<ECViewBinding> _ecDataList;
        public BindableCollection<ECViewBinding> ECViewCollec
        {
            get { return _ecDataList; }
            set { _ecDataList = value; NotifyOfPropertyChange(() => ECViewCollec); }
        }
        /// <summary>
        /// 窗口标题
        /// </summary>
        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; NotifyOfPropertyChange(() => Title); }
        }
        #endregion
        #region 成员变量
        //ECViewService状态
        private int _status = 0;
        //ECViewService名称
        private string _serviceName = "ECViewService";
        //风扇数量
        private int _fanCount = 0;
        //当前工作目录
        private string _currentDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        //主窗口绑定
        public ECViewBinding _ecviewData = null;
        //线程任务
        private Task<int> tempGetter = null;
        //功能接口
        private IFanDutyModify iFanDutyModify = null;
        //窗体管理器接口
        private IWindowManager iWindowsManager = null;
        //EC编辑窗口
        private ECEditorViewModel ecEditor = null;
        #endregion
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public ECMainViewModel()
        {
            _title = "EC主窗口";
            iFanDutyModify = ModuleFactory.GetFanDutyModifyModule();
            iWindowsManager = new WindowManager();
            _ecDataList = new BindableCollection<ECViewBinding>();
            _checkService();
            _initialData();
            _getTempTask();
        }
        #region 页面事件
        /// <summary>
        /// 关于点击事件
        /// </summary>
        public void AboutClick()
        {
            MessageBox.Show("ECView\n版本：" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() +
                "\n作者：YcraD\n鸣谢：特别的帅（神舟笔记本吧）\n说明：本程序主要实现Clevo模具风扇转速控制，部分灵感来源于“特别的帅”，在此感谢",
                "关于", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dg">DataGrid控件</param>
        /// <param name="e">鼠标点击事件参数</param>
        public void DgDoubleClick(DataGrid dg, MouseButtonEventArgs e)
        {
            Point aP = e.GetPosition(dg);
            IInputElement obj = dg.InputHitTest(aP);
            DependencyObject target = obj as DependencyObject;

            while (target != null)
            {
                if (target is DataGridRow)
                {
                    break;
                }
                target = VisualTreeHelper.GetParent(target);
            }
            if (target != null)
            {
                _ecEditorLoader(target as DataGridRow);
                dg.UpdateLayout();
            }
        }
        /// <summary>
        /// 窗口关闭事件
        /// </summary>
        public void WindowClosing()
        {
            bool isUpdated = false;
            foreach (ECViewBinding ec in _ecDataList)
            {
                if (ec.UpdateFlag)
                {
                    isUpdated = true;
                }
            }
            if (isUpdated)
            {
                MessageBoxResult res = MessageBox.Show("保存设置并退出？\n（ECView将在退出时启动ECViewService服务）", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (res == MessageBoxResult.OK)
                {
                    _saveConfig();
                }
            }
            if (_status != 0)
            {
                iFanDutyModify.StartService(_serviceName);
            }
            //TryClose();
        }
        #endregion
        #region 私有方法
        /// <summary>
        /// 打开EC编辑窗口
        /// </summary>
        /// <param name="row"></param>
        private void _ecEditorLoader(DataGridRow row)
        {
            //读取选择行的参数
            int index = row.GetIndex();
            int fanduty = iFanDutyModify.GetTempFanDuty(index + 1)[2];
            //加载窗体
            ecEditor = new ECEditorViewModel(fanduty, index, this);
            iWindowsManager.ShowDialog(ecEditor);
        }
        /// <summary>
        /// 检测服务状态
        /// </summary>
        private void _checkService()
        {
            _status = iFanDutyModify.CheckServiceState(_serviceName);
            if (_status == 0)
            {
                MessageBox.Show("ECView服务未正确安装，无法使用智能调节。\nECView虽然仍可继续使用，但建议重新安装ECView。", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (_status == 1)
            {
                iFanDutyModify.StopService(_serviceName);
                return;
            }
            else if (_status == 2)
            {
                MessageBox.Show("ECView服务已关闭。\n当服务设置为手动或已关闭时，无法使用智能调节以及开机自动设定风扇转速等功能。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <returns></returns>
        private void _initialData()
        {
            //功能接口
            ECViewBinding ecviewData = new ECViewBinding();
            _initECData(ecviewData);
        }
        /// <summary>
        /// 初始化数据
        /// </summary>
        private void _initECData(ECViewBinding ecviewData)
        {
            //判断配置文件是否存在
            if (System.IO.File.Exists(_currentDirectory + "ecview.cfg"))
            {
                List<ConfigPara> configParaList = iFanDutyModify.ReadCfgFile(_currentDirectory + "ecview.cfg");
                //风扇转速与温度信息
                for (int i = 0; i < configParaList.Count; i++)
                {
                    int[] ecData = iFanDutyModify.GetTempFanDuty(i + 1);
                    _nbModel = configParaList[0].NbModel;
                    _ecVersion = configParaList[0].ECVersion;
                    ecviewData.FanNo = i + 1;
                    foreach (ConfigPara configPara in configParaList)
                    {
                        if (ecviewData.FanNo == configPara.FanNo)
                        {
                            ecviewData.FanSetModel = configPara.SetMode;
                            ecviewData.FanSet = configPara.FanSet;
                            if (configPara.SetMode == 1)
                            {
                                //若上次配置为自动调节，设置风扇自动调节
                                ecData = iFanDutyModify.SetFanduty(configPara.FanNo, 0, true);
                            }
                            else if (configPara.SetMode == 2)
                            {
                                //若为上次配置手动调节，设置风扇转速
                                ecData = iFanDutyModify.SetFanduty(configPara.FanNo, (int)(configPara.FanDuty * 2.55m), false);
                            }
                            else { }
                        }
                    }
                    _cpuRemote = ecData[0] + "℃";
                    _cpuLocal = ecData[1] + "℃";
                    ecviewData.UpdateFlag = false;
                    ecviewData.FanDutyStr = ecData[2] + "%";
                    ecviewData.FanDuty = ecData[2];
                    _ecDataList.Add(ecviewData);
                }
            }
            else
            {
                ManagementObjectSearcher Searcher_BaseBoard = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");
                //模具型号
                _nbModel = "当前模具型号为：";
                foreach (var baseBoard in Searcher_BaseBoard.Get())
                    _nbModel += Convert.ToString((baseBoard)["Product"]);
                //EC版本
                _ecVersion = "当前EC版本为：1.";
                _ecVersion += iFanDutyModify.GetECVersion();
                //风扇数量
                _fanCount = iFanDutyModify.GetFanCount();
                if (_fanCount > 4)
                    _fanCount = 0;
                if (_fanCount == 0)
                    _fanCount = 1;
                //风扇转速与温度信息
                for (int i = 0; i < _fanCount; i++)
                {
                    int[] ecData = iFanDutyModify.GetTempFanDuty(i + 1);
                    _cpuRemote = ecData[0] + "℃";
                    _cpuLocal = ecData[1] + "℃";
                    ecviewData.FanDutyStr = ecData[2] + "%";
                    ecviewData.FanDuty = ecData[2];
                    ecviewData.FanNo = i + 1;
                    ecviewData.FanSet = "未设置";
                    ecviewData.FanSetModel = 0;
                    ecviewData.UpdateFlag = false;
                    _ecDataList.Add(ecviewData);
                }
            }
        }
        /// <summary>
        /// 检测是否更新设置
        /// </summary>
        private void _saveConfig()
        {
            for (int i = 0; i < _ecDataList.Count; i++)
            {
                if (_ecDataList[i].UpdateFlag)
                {
                    List<ConfigPara> configParaList = new List<ConfigPara>();
                    foreach (ECViewBinding ec in ECViewCollec)
                    {
                        ConfigPara configPara = new ConfigPara();
                        configPara.NbModel = NbModel;
                        configPara.ECVersion = ECVersion;
                        configPara.FanNo = ec.FanNo;
                        configPara.SetMode = ec.FanSetModel;
                        configPara.FanSet = ec.FanSet;
                        configPara.FanDuty = ec.FanDuty;
                        configParaList.Add(configPara);
                    }
                    iFanDutyModify.WriteCfgFile(_currentDirectory + "ecview.cfg", configParaList);
                }
                _ecDataList[i].UpdateFlag = false;
            }
        }
        /// <summary>
        /// CPU温度更新线程
        /// </summary>
        private void _getTempTask()
        {
            tempGetter = new Task<int>(() => { return _tempGetter(); });
            tempGetter.Start();
        }
        /// <summary>
        /// 更新CPU温度
        /// </summary>
        private int _tempGetter()
        {
            try
            {
                while (true)
                {
                    int[] temp = iFanDutyModify.GetTempFanDuty(1);
                    for (int i = 0; i < ECViewCollec.Count; i++)
                    {
                        ECViewCollec[i].FanDuty = iFanDutyModify.GetTempFanDuty(i + 1)[2];
                        ECViewCollec[i].FanDutyStr = ECViewCollec[i].FanDuty + "%";
                    }
                    CpuRemote = temp[0] + "℃";
                    CpuLocal = temp[1] + "℃";
                    Thread.Sleep(5000);
                }
            }
            catch
            {
                return 0;
            }
        }
        #endregion
    }
}