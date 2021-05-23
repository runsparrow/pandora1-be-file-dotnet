using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pandora1_be_file_dotnet.Models.ProxyDto
{
    public class FileProxyDto
    {
        public int id { get; set; }
        public string statusKey { get; set; } = "";
        public string name { get; set; } = "";
        public string goodsNo { get; set; } = "";
        public string tags { get; set; } = "";
        public string desc { get; set; } = "";
        public string authDesc { get; set; } = "";
        public int classifyId { get; set; } = -1;
        public string classifyName { get; set; }
        public string url { get; set; } = "";
        public string ext { get; set; } = "";
        public string dpi { get; set; } = "";
        public string ratio { get; set; } = "";
        public string rgb { get; set; } = "";
        public string size { get; set; } = "";
        public int level { get; set; } = 0;
        public int isImage { get; set; } = 1;
        public decimal price { get; set; } = 0;
        public int quantity { get; set; } = 0;
        public int maxStock { get; set; } = 0;
        public int minStock { get; set; } = 0;
        public int downCount { get; set; } = 0;
        public int collectCount { get; set; } = 0;
        public int buyCount { get; set; } = 0;
        public int ownerId { get; set; }=-1;
        public string ownerName { get; set; }
        public string publicDateTime { get; set; } = DateTime.Now.ToString();
        public string finalDateTime { get; set; } = DateTime.Now.ToString();
        public string remark { get; set; } = "";
        public int statusId { get; set; } = -1;
        public string statusName { get; set; } = "";
        public int statusValue { get; set; } = 0;
        public string createDateTime { get; set; } = DateTime.Now.ToString();
        public int createUserId { get; set; } = -1;
        public string editDateTime { get; set; } = DateTime.Now.ToString();
        public int editUserId { get; set; } = -1;
        public string memberName { get; set; } = "";
        public int memberId { get; set; }
        public StatusProxyDto status { get; set; }
    }

    public class StatusProxyDto
    {
        public int id { get; set; } = 0;
        public int pid { get; set; } = -1;
        public string name { get; set; } = "";
        public string key { get; set; } = "";
        public int value { get; set; } = 0;
        public string desc { get; set; } = "";
        public string createDateTime { get; set; } = DateTime.Now.ToString();
        public int createUserId { get; set; } = -1;
        public string editDateTime { get; set; } = DateTime.Now.ToString();
        public int editUserId { get; set; } = -1;
        public string path { get; set; } = "";
    }
}
