﻿using ECView.Services.ServicesImpl;

namespace ECView.Services
{
    public class ModuleFactory
    {
        /// <summary>
        /// 风扇调节功能接口实例化
        /// </summary>
        private static IFanDutyModify moduleFanDutyModify = null;
        public static IFanDutyModify GetFanDutyModifyModule()
        {
            if (moduleFanDutyModify == null)
            {
                moduleFanDutyModify = new FanDutyModifyImpl();
            }
            return moduleFanDutyModify;
        }
    }
}
