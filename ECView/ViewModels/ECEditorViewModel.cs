using Caliburn.Micro;
using ECView.Frameworks;
using ECView.Services;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using JetBrains.Annotations;

namespace ECView.ViewModels
{
    public class EcEditorViewModel : Screen, IShell
    {
        #region 绑定数据
        /// <summary>
        /// 窗口标题
        /// </summary>
        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; NotifyOfPropertyChange(() => Title); }
        }
        /// <summary>
        /// 风扇号
        /// </summary>
        private string _fanNo;
        public string FanNo
        {
            get { return _fanNo; }
            set { _fanNo = value; NotifyOfPropertyChange(() => FanNo); }
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
        /// 文件路径
        /// </summary>
        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; NotifyOfPropertyChange(() => FilePath); }
        }
        /// <summary>
        /// 手动设置启用
        /// </summary>
        private bool _isManuEnabled;
        public bool IsManuEnabled
        {
            get { return _isManuEnabled; }
            set { _isManuEnabled = value; NotifyOfPropertyChange(() => IsManuEnabled); }
        }
        /// <summary>
        /// 智能设置启用
        /// </summary>
        private bool _isInteEnabled;
        public bool IsInteEnabled
        {
            get { return _isInteEnabled; }
            set { _isInteEnabled = value; NotifyOfPropertyChange(() => IsInteEnabled); }
        }
        /// <summary>
        /// 自动复选
        /// </summary>
        private bool _isAutoChecked;
        public bool IsAutoChecked
        {
            get { return _isAutoChecked; }
            set { _isAutoChecked = value; NotifyOfPropertyChange(() => IsAutoChecked); }
        }
        /// <summary>
        /// 手动复选
        /// </summary>
        private bool _isManuChecked;
        public bool IsManuChecked
        {
            get { return _isManuChecked; }
            set { _isManuChecked = value; NotifyOfPropertyChange(() => IsManuChecked); }
        }
        /// <summary>
        /// 智能复选
        /// </summary>
        private bool _isInteChecked;
        public bool IsInteChecked
        {
            get { return _isInteChecked; }
            set { _isInteChecked = value; NotifyOfPropertyChange(() => IsInteChecked); }
        }
        #endregion
        #region 成员变量
        //功能接口
        [NotNull] private readonly IFanDutyModify _iFanDutyModify = ModuleFactory.GetFanDutyModifyModule();
        [NotNull] private readonly EcMainViewModel _main;
        private int _fanSetModel;//风扇控制模式
        private readonly int _index;//行号
        //工作目录
        [NotNull] private readonly string _path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        #endregion
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public EcEditorViewModel(int fanDuty, int index, EcMainViewModel ecMain)
        {
            _main = ecMain;
            _fanDuty = fanDuty;
            _fanNo = "当前风扇号：" + (index + 1);
            _title = "EC编辑窗口";
            _index = index;
            _initData();
        }
        #region 页面事件
        /// <summary>
        /// 自动调节
        /// </summary>
        public void AutoCheck()
        {
            //禁用手动与智能调节
            IsManuEnabled = false;
            IsInteEnabled = false;
            //设置模式
            _fanSetModel = 1;
        }
        /// <summary>
        /// 手动调节
        /// </summary>
        public void ManuCheck()
        {
            //启用手动调节
            IsManuEnabled = true;
            //禁用智能调节
            IsInteEnabled = false;
            //设置模式
            _fanSetModel = 2;
        }
        /// <summary>
        /// 智能调节
        /// </summary>
        public void InteCheck()
        {
            //禁用手动调节
            IsManuEnabled = false;
            //启用智能调节
            IsInteEnabled = true;
            //设置模式
            _fanSetModel = 3;
        }
        /// <summary>
        /// 文件选取
        /// </summary>
        public void FileSelect()
        {
            var fileDialog = new OpenFileDialog
            {
                InitialDirectory = Directory.GetCurrentDirectory(),
                DefaultExt = ".xml",
                Filter = "XML Files (.xml)|*.xml",
                Multiselect = false
            };
            //初始目录
            // 默认文件类型
            //加入选择的文件信息
            if (fileDialog.ShowDialog() != true) return;
            try
            {
                var filePath = fileDialog.FileName;//选择配置文件
                //风扇号
                var fanNo = _index + 1;
                //目标文件绝对路径
                var targetPath = _path + "conf\\Configuration_" + fanNo + ".xml";
                //检测目标文件夹是否存在
                if (!Directory.Exists(_path + "conf\\"))
                {
                    //若不存在则建立文件夹
                    Directory.CreateDirectory(_path + "conf\\");
                }
                //复制配置文件（覆盖同名文件）
                File.Copy(fileDialog.FileName, targetPath, true);
                FilePath = filePath;
            }
            catch (Exception ee)
            {
                MessageBox.Show("文件读取错误！错误原因" + ee.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        /// <summary>
        /// 取消
        /// </summary>
        public void CancelClick()
        {
            TryClose();
        }
        /// <summary>
        /// 确认
        /// </summary>
        public void ConfirmClick()
        {
            switch (_fanSetModel)
            {
                case 1:
                    _main.EcViewCollec[_index].FanSet = "自动调节";
                    _main.EcViewCollec[_index].FanSetModel = 1;
                    _iFanDutyModify.SetFanduty(_index + 1, 0, true);
                    _main.EcViewCollec[_index].UpdateFlag = true;

                    //关闭窗口
                    TryClose();
                    break;
                case 2:
                    _main.EcViewCollec[_index].FanSet = "手动调节";
                    _main.EcViewCollec[_index].FanSetModel = 2;
                    _main.EcViewCollec[_index].FanDuty = _fanDuty;
                    _main.EcViewCollec[_index].FanDutyStr = _fanDuty + "%";
                    _iFanDutyModify.SetFanduty(_index + 1, (int)(_fanDuty * 2.55m), false);
                    _main.EcViewCollec[_index].UpdateFlag = true;

                    //关闭窗口
                    TryClose();
                    break;
                case 3:
                    _main.EcViewCollec[_index].FanSet = "智能调节";
                    _main.EcViewCollec[_index].FanSetModel = 3;
                    if (string.IsNullOrEmpty(_filePath))
                    {
                        MessageBox.Show("请选择配置文件", "提示信息", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        _main.EcViewCollec[_index].UpdateFlag = true;
                        MessageBox.Show("智能调节将在程序关闭后启用", "提示信息", MessageBoxButton.OK, MessageBoxImage.Information);

                        //关闭窗口
                        TryClose();
                    }
                    break;
            }
        }

        #endregion
        #region 私有方法
        /// <summary>
        /// 初始化参数
        /// </summary>
        private void _initData()
        {
            _fanSetModel = _main.EcViewCollec[_index].FanSetModel;
            switch (_fanSetModel)
            {
                case 1:
                    _isAutoChecked = true;
                    AutoCheck();
                    break;
                case 2:
                    _isManuChecked = true;
                    ManuCheck();
                    break;
                case 3:
                    _isInteChecked = true;
                    InteCheck();
                    break;
                default:
                    _isManuChecked = true;
                    ManuCheck();
                    break;
            }
        }
        #endregion
    }
}
