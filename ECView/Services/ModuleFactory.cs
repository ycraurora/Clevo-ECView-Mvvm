using ECView.Services.ServicesImpl;
using JetBrains.Annotations;

namespace ECView.Services
{
    public class ModuleFactory
    {
        /// <summary>
        /// 风扇调节功能接口实例化
        /// </summary>
        [CanBeNull] private static IFanDutyModify _moduleFanDutyModify;
        public static IFanDutyModify GetFanDutyModifyModule()
        {
            return _moduleFanDutyModify ?? (_moduleFanDutyModify = new FanDutyModifyImpl());
        }
    }
}
