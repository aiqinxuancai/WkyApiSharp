using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WkyApiSharp.Service.Model
{
    public record WkyPeer
    {

        public WkyPeer(ListPeer.Peer peer)
        {
            Peer = peer;
        }

        private ListPeer.Peer _peer;

        //属于的Peer
        public ListPeer.Peer Peer
        {
            set
            {
                _peer = value;
                Devices.Clear();
                foreach (var device in _peer.Devices)
                {
                    var wkyDevice = new WkyDevice(device);
                    Devices.Add(wkyDevice);
                }
            }
            get
            {
                return _peer;
            }
        }

        /// <summary>
        /// 获取peerId
        /// </summary>
        public string PeerId { 
            get
            {
                var device = Devices.FirstOrDefault();
                if (device != null)
                {
                    return device.Device.Peerid;
                }
                return string.Empty;
            } 
        }

        /// <summary>
        /// 是否已经登录
        /// </summary>
        public bool IsLogged { get; private set; }


        /// <summary>
        /// 设备列表
        /// </summary>
        public List<WkyDevice> Devices { get; set; } = new List<WkyDevice>();

        /// <summary>
        /// Peer的任务列表
        /// </summary>
        public List<WkyTask> Tasks => _tasks;

        private readonly List<WkyTask> _tasks = new();

        public async Task<bool> LoginPeer(WkyApi api)
        {
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    var result = await api.RemoteDownloadLogin(PeerId);
                    if (result.Rtn == 0)
                    {
                        IsLogged = true;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    //失败
                }
            }
            return false;
        }

        public async Task UpdateDiskInfo(WkyApi api)
        {
            foreach (var device in Devices)
            {
                await device.UpdateDiskInfo(api);
            }
        }

        public async Task UpdateTaskList(WkyApi api)
        {
            if (IsLogged)
            {
                var result = await api.RemoteDownloadList(this.PeerId);

                if (result.Rtn == 0)
                {
                    var remoteTaskList = result.Tasks.ToList();

                    //TODO 更新，推送下载成功等事件


                    //if (obList.Count - _taskList.Count > 0)
                    //{
                    //    while (obList.Count - _taskList.Count > 0)
                    //    {
                    //        TaskList.Add(new TaskModel());
                    //    }
                    //}
                    //else if (obList.Count - TaskList.Count < 0)
                    //{
                    //    while (obList.Count - TaskList.Count < 0)
                    //    {
                    //        TaskList.RemoveAt(TaskList.Count - 1);
                    //    }
                    //}

                    //for (int i = 0; i < obList.Count; i++)
                    //{
                    //    TaskList[i].Data = obList[i];
                    //}
                }
            }
        }

    }
}
