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
                    else if (result.Rtn == 10302)//token 失效？
                    {
                        //{"msg":"check session error(-124)","rtn":10302}
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

        /// <summary>
        /// 验证是否可以拉取任务列表，不做其他操作
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        public async Task<bool> VerifyTaskList(WkyApi api)
        {
            try
            {
                var result = await api.RemoteDownloadList(this.PeerId);
                return result.Rtn == 0;
            }
            catch (Exception e)
            {
                return false;
            }
        }


        /// <summary>
        /// 返回成功失败，和新完成的任务
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        public async Task<(bool, List<WkyTask>)> UpdateTaskList(WkyApi api)
        {
            if (IsLogged)
            {
                try
                {
                    var result = await api.RemoteDownloadList(this.PeerId);
                    List<WkyTask> completedTasks = new List<WkyTask>();
                    if (result.Rtn == 0)
                    {
                        var obList = result.Tasks.ToList();

                        //创建对应数量的WkyTask
                        if (obList.Count - _tasks.Count > 0)
                        {
                            while (obList.Count - _tasks.Count > 0)
                            {
                                _tasks.Add(new WkyTask());
                            }
                        }
                        else if (obList.Count - _tasks.Count < 0)
                        {
                            while (obList.Count - _tasks.Count < 0)
                            {
                                _tasks.RemoveAt(_tasks.Count - 1);
                            }
                        }

                        foreach (var item in obList)
                        {
                            var oldItem = _tasks.FirstOrDefault(a => a.Data?.Id == item.Id);


                            //如果之前的数据是未完成，但是现在是已经完成的，则发送

                            if (oldItem != null && oldItem.Data != null)
                            {
                                if (oldItem.Data.State != (int)TaskState.Completed && item.State == (int)TaskState.Completed)
                                {
                                    var task = new WkyTask() { Data = item };
                                    completedTasks.Add(task);
                                }
                            }

                        }

                        //重新赋值
                        for (int i = 0; i < obList.Count; i++)
                        {
                            _tasks[i].Data = obList[i];
                        }
                        return (true, completedTasks);
                    }
                    
                }
                catch (Exception ex)
                {

                }

                
            }
            return (false, null);
        }

    }
}
