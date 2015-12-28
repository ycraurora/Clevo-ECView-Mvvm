using Caliburn.Micro;
using ECView.DataDefinations;
using ECView.Frameworks;
using ECView.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ECView.ViewModels
{
    public class EcMainViewModel : Screen, IShell
    {
        #region 绑定数据
        public class EcViewBinding : PropertyChangedBase
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
        public string EcVersion
        {
            get { return _ecVersion; }
            set { _ecVersion = value; NotifyOfPropertyChange(() => EcVersion); }
        }
        /// <summary>
        /// EC数据列表
        /// </summary>
        private BindableCollection<EcViewBinding> _ecDataList;
        public BindableCollection<EcViewBinding> EcViewCollec
        {
            get { return _ecDataList; }
            set { _ecDataList = value; NotifyOfPropertyChange(() => EcViewCollec); }
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
        private int _status;
        //ECViewService名称
        private const string ServiceName = "ECViewService";
        //风扇数量
        private int _fanCount;
        //当前工作目录
        private readonly string _currentDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        //主窗口绑定
        public EcViewBinding EcviewData = null;
        //线程任务
        private Task<int> _tempGetter;
        //功能接口
        private readonly IFanDutyModify _iFanDutyModify;
        //窗体管理器接口
        private readonly IWindowManager _iWindowsManager;
        //EC编辑窗口
        private EcEditorViewModel _ecEditor;
        #endregion
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public EcMainViewModel()
        {
            _title = "EC主窗口";
            _iFanDutyModify = ModuleFactory.GetFanDutyModifyModule();
            _iWindowsManager = new WindowManager();
            _ecDataList = new BindableCollection<EcViewBinding>();
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
            MessageBox.Show("ECView\n版本：" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version +
                "\n作者：YcraD\n鸣谢：特别的帅（神舟笔记本吧）\n说明：本程序主要实现Clevo模具风扇转速控制，部分灵感来源于“特别的帅”，在此感谢",
                "关于", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        /// <summary>
        /// 表格行双击事件
        /// </summary>
        /// <param name="dg">DataGrid控件</param>
        /// <param name="e">鼠标点击事件参数</param>
        public void DgDoubleClick(DataGrid dg, MouseButtonEventArgs e)
        {
            var aP = e.GetPosition(dg);
            var obj = dg.InputHitTest(aP);
            var target = obj as DependencyObject;

            while (target != null)
            {
                if (target is DataGridRow)
                {
                    break;
                }
                target = VisualTreeHelper.GetParent(target);
            }
            if (target == null) return;
            _ecEditorLoader(target as DataGridRow);
            dg.UpdateLayout();
        }
        /// <summary>
        /// 窗口关闭事件
        /// </summary>
        public void WindowClosing()
        {
            var isUpdated = false;
            // ReSharper disable once UnusedVariable
            foreach (var ec in _ecDataList.Where(ec => ec.UpdateFlag))
            {
                isUpdated = true;
            }
            if (isUpdated)
            {
                var res = MessageBox.Show("保存设置并退出？\n（ECView将在退出时启动ECViewService服务）", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (res == MessageBoxResult.OK)
                {
                    _saveConfig();
                }
            }
            if (_status != 0)
            {
                _iFanDutyModify.StartService(ServiceName);
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
            var index = row.GetIndex();
            var fanduty = _iFanDutyModify.GetTempFanDuty(index + 1)[2];
            //加载窗体
            _ecEditor = new EcEditorViewModel(fanduty, index, this);
            _iWindowsManager.ShowDialog(_ecEditor);
        }
        /// <summary>
        /// 检测服务状态
        /// </summary>
        private void _checkService()
        {
            _status = _iFanDutyModify.CheckServiceState(ServiceName);
            switch (_status)
            {
                case 0:
                    MessageBox.Show("ECView服务未正确安装，无法使用智能调节。\nECView虽然仍可继续使用，但建议重新安装ECView。", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case 1:
                    _iFanDutyModify.StopService(ServiceName);
                    return;
                case 2:
                    MessageBox.Show("ECView服务已关闭。\n当服务设置为手动或已关闭时，无法使用智能调节以及开机自动设定风扇转速等功能。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <returns></returns>
        private void _initialData()
        {
            //功能接口
            var ecviewData = new EcViewBinding();
            _initECData(ecviewData);
        }
        /// <summary>
        /// 初始化数据
        /// </summary>
        private void _initECData(EcViewBinding ecviewData)
        {
            //判断配置文件是否存在
            if (System.IO.File.Exists(_currentDirectory + "ecview.cfg"))
            {
                var configParaList = _iFanDutyModify.ReadCfgFile(_currentDirectory + "ecview.cfg");
                //风扇转速与温度信息
                Debug.Assert(configParaList != null, "configParaList != null");
                for (var i = 0; i < configParaList.Count; i++)
                {
                    var ecData = _iFanDutyModify.GetTempFanDuty(i + 1);
                    _nbModel = configParaList[0].NbModel;
                    _ecVersion = configParaList[0].EcVersion;
                    ecviewData.FanNo = i + 1;
                    foreach (var configPara in configParaList.Where(configPara => ecviewData.FanNo == configPara.FanNo))
                    {
                        ecviewData.FanSetModel = configPara.SetMode;
                        ecviewData.FanSet = configPara.FanSet;
                        switch (configPara.SetMode)
                        {
                            case 1:
                                //若上次配置为自动调节，设置风扇自动调节
                                ecData = _iFanDutyModify.SetFanduty(configPara.FanNo, 0, true);
                                break;
                            case 2:
                                //若为上次配置手动调节，设置风扇转速
                                ecData = _iFanDutyModify.SetFanduty(configPara.FanNo, (int)(configPara.FanDuty * 2.55m), false);
                                break;
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
                var searcherBaseBoard = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");
                //模具型号
                _nbModel = "当前模具型号为：";
                foreach (var baseBoard in searcherBaseBoard.Get())
                    _nbModel += Convert.ToString((baseBoard)["Product"]);
                //EC版本
                _ecVersion = "当前EC版本为：1.";
                _ecVersion += _iFanDutyModify.GetEcVersion();
                //风扇数量
                _fanCount = _iFanDutyModify.GetFanCount();
                if (_fanCount > 4)
                    _fanCount = 0;
                if (_fanCount == 0)
                    _fanCount = 1;
                //风扇转速与温度信息
                for (var i = 0; i < _fanCount; i++)
                {
                    var ecData = _iFanDutyModify.GetTempFanDuty(i + 1);
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
            foreach (var t in _ecDataList)
            {
                if (t.UpdateFlag)
                {
                    var configParaList = EcViewCollec.Select(ec => new ConfigPara
                    {
                        NbModel = NbModel, EcVersion = EcVersion, FanNo = ec.FanNo, SetMode = ec.FanSetModel, FanSet = ec.FanSet, FanDuty = ec.FanDuty
                    }).ToList();
                    _iFanDutyModify.WriteCfgFile(_currentDirectory + "ecview.cfg", configParaList);
                }
                t.UpdateFlag = false;
            }
        }

        /// <summary>
        /// CPU温度更新线程
        /// </summary>
        private void _getTempTask()
        {
            _tempGetter = new Task<int>(_getTemp);
            _tempGetter.Start();
        }
        /// <summary>
        /// 更新CPU温度
        /// </summary>
        private int _getTemp()
        {
            try
            {
                while (true)
                {
                    var temp = _iFanDutyModify.GetTempFanDuty(1);
                    for (var i = 0; i < EcViewCollec.Count; i++)
                    {
                        EcViewCollec[i].FanDuty = _iFanDutyModify.GetTempFanDuty(i + 1)[2];
                        EcViewCollec[i].FanDutyStr = EcViewCollec[i].FanDuty + "%";
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