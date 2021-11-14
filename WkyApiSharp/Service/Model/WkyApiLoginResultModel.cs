using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WkyApiSharp.Service.Model
{
    public class WkyApiLoginResultModel
    {
        //{"sMsg":"Success","data":{
        //"nickname":"18620916070",
        //"sessionid":"cs001.8800E4D0BB79C39D53D59C6A63563940",
        //"account_type":"4",
        //"enable_homeshare":1,
        //"userid":"9358413",
        //"phone":"18620916070",
        //"bind_pwd":"1",
        //"phone_area":"86"},"iRet":0}

        [JsonProperty("nickname")]
        public string Nikename { set; get; }

        [JsonProperty("sessionid")]
        public string SessionId { set; get; }

        [JsonProperty("account_type")]
        public string AccountType { set; get; }

        [JsonProperty("enable_homeshare")]
        public int EnableHomeShare { set; get; }

        [JsonProperty("userid")]
        public string UserId { set; get; }

        [JsonProperty("phone")]
        public string Phone { set; get; }

        [JsonProperty("bind_pwd")]
        public string BindPwd { set; get; } //0 \ 1

        [JsonProperty("phone_area")]
        public string PhoneArea { set; get; } //86



        public string sMsg { set; get; } //86
        

        public DateTime CreateDateTime { set; get; } 
    }
}
