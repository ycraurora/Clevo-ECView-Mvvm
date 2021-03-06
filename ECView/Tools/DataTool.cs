﻿using System.Runtime.InteropServices;

namespace ECView.Tools
{
    public class DataTool
    {
        [DllImport("ecview.dll", EntryPoint = "#3")]
        public static extern void SetFanDuty(int p1, int p2);

        [DllImport("ecview.dll", EntryPoint = "#4")]
        public static extern int SetFANDutyAuto(int p1);

        [DllImport("ecview.dll", EntryPoint = "#5")]
        public static extern EcData GetTempFanDuty(int p1);

        [DllImport("ecview.dll", EntryPoint = "#6")]
        public static extern int GetFANCounter();

        [DllImport("ecview.dll", EntryPoint = "#8")]
        public static extern string GetECVersion();

        public struct EcData
        {
            public int Data;
            public int Data1;
            public int Data2;
        }
    }
}
