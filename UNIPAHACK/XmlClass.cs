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
        public List<detail> elem = new List<detail>();
    }
    public class item
    {
        public string name;
        public List<detail> detail = new List<detail>();
    }
    public class detail
    {
        public string title;
        public string sender;
        public string date;
        public string canceled_date;
        public string revenge_date;
        public string course;
        public string teacher;
        public string time;
        public string note;
        public string info;
    }
}
