﻿using System;
using System.Collections.Generic;
using System.Text;

namespace WkyApiSharp.Service.Model
{
    public enum TaskState
    {
        Adding = 0,//0 => "添加中",
        Downloading = 1,//1 => "下载中",
        Waiting = 8,//8 => "等待中",
        Pause = 9,//9 => "已暂停",
        Completed = 11, //11 => "已完成",
        LackResources = 12, //12 => "缺少资源",
        PreparingAdd = 14, //14 => "准备添加中",
        DiskError = 38 //38 => 磁盘写入异常
    }

    public record WkyTask
    {
        public RemoteDownloadList.Task Data { get; set; }
    }
}
