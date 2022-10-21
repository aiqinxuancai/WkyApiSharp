using System;
using System.Collections.Generic;
using System.Text;

namespace WkyApiSharp.Service.Model
{
    public record WkyPartition
    {
        public WkyPartition(GetUsbInfo.Partition partition)
        {
            Partition = partition;
        }

        private GetUsbInfo.Partition _partition;

        public GetUsbInfo.Partition Partition
        {
            set
            {
                _partition = value;
            }
            get
            {
                return _partition;
            }

        }
        public string Description
        {
            get
            {
                return $"{_partition.Path}\n{_partition.PartLabel}({_partition.Id})";
            }

        }



    }
}
