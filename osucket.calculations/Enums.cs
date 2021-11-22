using System;

namespace osucket.calculations
{
    [Flags]
    public enum ModsStr
    {
        NM = 0,
        NF = 1 << 0,
        EZ = 1 << 1,
        TD = 1 << 2, //Touch device
        HD = 1 << 3,
        HR = 1 << 4,
        SD = 1 << 5,
        DT = 1 << 6,
        RX = 1 << 7,
        HT = 1 << 8,
        NC = 1 << 9,
        FL = 1 << 10,
        AT = 1 << 11, //auto play
        SO = 1 << 12,
        AP = 1 << 13, //Auto pilot
        PF = 1 << 14,
        K4 = 1 << 15,
        K5 = 1 << 16,
        K6 = 1 << 17,
        K7 = 1 << 18,
        K8 = 1 << 19,
        FI = 1 << 20,
        RN = 1 << 21,
        CM = 1 << 22,
        TP = 1 << 23,
        K9 = 1 << 24,
        Coop = 1 << 25,
        K1 = 1 << 26,
        K3 = 1 << 27,
        K2 = 1 << 28,
        SV2 = 1 << 29,
        LM = 1 << 30,
        SpeedChanging = DT | HT | NC,
        MapChanging = HR | EZ | SpeedChanging
    }

    public enum OsuGameMode
    {
        osu = 0,
        taiko = 1,
        fruits = 2,
        mania = 3
    }
    public enum OsuBeatmapStatus
    {
        Ranked = 4,
        Pending = 2,
        Unsubmitted = 1,
        Loved = 7,
        Approved = 5
    }


}
