using System;
using System.Collections.Generic;
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
        }


        public List<WkyDevice> Devices { get; set; } = new List<WkyDevice>();


        public async Task UpdateDiskInfo(WkyApi api)
        {
            foreach (var device in Devices)
            {
                await device.UpdateDiskInfo(api);
            }
        }

    }
}
