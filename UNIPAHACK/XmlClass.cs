using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UNIPAHACK
{
    public class root
    {
        public string docName;
        public List<Detail> elem = new List<Detail>();
    }
    public class item
    {
        public string name;
        public List<Detail> detail = new List<Detail>();
    }
    public class Detail
    {
        public string title;
        public string sender;
        public string body;
        public string info;
    }
}
