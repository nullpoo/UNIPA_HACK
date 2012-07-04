using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Net;
using System.Web;
using Sgml;
using System.Xml;
using System.Xml.Linq;
using System.Data;

namespace UNIPAHACK
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        string id = "";
        string pass = "";
        string session_id = "";
        int page_count;
        CookieContainer cc = new CookieContainer();

        public MainWindow()
        {
            InitializeComponent();
        }

        async Task toppage()
        {
            var req = (HttpWebRequest)WebRequest.Create("https://portal.sa.dendai.ac.jp/up/faces/login/Com00505A.jsp");
            req.CookieContainer = cc;
            var res = await req.GetResponseAsync();
            var stream = res.GetResponseStream();
            getSessionId(stream);
        }

        //セッションIDを取得（更新）する
        void getSessionId(Stream stream)
        {
            var enc = System.Text.Encoding.UTF8;
            using (var reader = new StreamReader(stream, enc))
            using (var sgmlReader = new SgmlReader { InputStream = reader })
            {
                sgmlReader.DocType = "HTML";
                sgmlReader.CaseFolding = CaseFolding.ToLower;
                var doc = XDocument.Load(sgmlReader);
                var ns = doc.Root.Name.Namespace;
                var q = doc.Descendants(ns + "input")
                    .Where(ul => ul.Attribute("id") != null && ul.Attribute("id").Value == "com.sun.faces.VIEW")
                    .Select(el => el.Attribute("value").Value).ToList();
                session_id = q[0];
            }
        }
        async Task<Stream> postRequest(Uri uri, Dictionary<string, string> dic)
        {
            string param = "";
            foreach (var item in dic.Keys)
            {
                param += String.Format("{0}={1}&", item, dic[item]);
            }
            byte[] data = Encoding.ASCII.GetBytes(param);

            //POSTリクエスト作成
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Win64; x64; Trident/6.0)";
            req.ContentLength = data.Length;
            req.CookieContainer = cc;

            //POSTを実行
            var reqStream = await req.GetRequestStreamAsync();
            reqStream.Write(data, 0, data.Length);
            reqStream.Close();

            //HTTP GETによるクッキーの取得
            var res = await req.GetResponseAsync();
            var stream = res.GetResponseStream();
            return stream;
        }

        async Task mainpage()
        {
            //パラメータ作成
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("form1:htmlUserId", id);
            dic.Add("form1:htmlPassword", pass);
            dic.Add("com.sun.faces.VIEW", session_id);
            dic.Add("form1:login.x", "0");
            dic.Add("form1:login.y", "0");
            dic.Add("form1", "form1");
            Uri uri = new Uri("https://portal.sa.dendai.ac.jp/up/faces/login/Com00505A.jsp");
            var stream = await postRequest(uri, dic);
            getSessionId(stream);
        }
        async Task getJyugyoInfo()
        {
            //パラメータ作成
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("form1:Poa00201A:htmlParentTable:2:htmlHeaderTbl:0:allJugyo.x","");
            dic.Add("com.sun.faces.VIEW", session_id);
            dic.Add("form1", "form1");
            Uri uri = new Uri("https://portal.sa.dendai.ac.jp/up/faces/up/po/Poa00601A.jsp");
            var stream = await postRequest(uri, dic);
            getSessionId(stream);
        }
        async Task getKyukoHoko()
        {
            //パラメータ作成
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("form1:Poa00201A:htmlParentTable:2:htmlDisplayOfAll:0:allInfoLinkCommand", "");
            dic.Add("com.sun.faces.VIEW", session_id);
            dic.Add("form1", "form1");
            Uri uri = new Uri("https://portal.sa.dendai.ac.jp/up/faces/up/po/Poa00601A.jsp");
            var stream = await postRequest(uri, dic);
            var enc = System.Text.Encoding.UTF8;
            using (var reader = new StreamReader(stream, enc))
            using (var sgmlReader = new SgmlReader { InputStream = reader })
            {
                sgmlReader.DocType = "HTML";
                sgmlReader.CaseFolding = CaseFolding.ToLower;
                var doc = XDocument.Load(sgmlReader);
                var ns = doc.Root.Name.Namespace;

                var q = doc.Descendants(ns + "input")
                    .Where(ul => ul.Attribute("id") != null && ul.Attribute("id").Value == "com.sun.faces.VIEW")
                    .Select(el => el.Attribute("value").Value).ToList();
                session_id = q[0];

                //ページ数取得
                var q2 = doc.Descendants(ns + "span")
                    .Where(ul => ul.Attribute("class") != null && ul.Attribute("class").Value == "pagerWeb")
                    .Descendants(ns + "a")
                    .Select(el => (string)el.Value).Last();
                page_count = int.Parse(q2);

                //1ページ目から取得開始
                IEnumerable<string> tmp = new List<string>();
                for (int i = 0; i < page_count; i++)
                {
                    tmp = tmp.Concat(await getKyukoHokoList(i));
                }
                root xmlroot = new root();
                xmlroot.docName = "test";

                foreach (var item in tmp)
                {
                    item xmlitem = new item();
                    xmlitem.name = item;
                    xmlroot.elem.Add(xmlitem);
                    textbox1.Text += item+"\r\n";
                }
                FileStream fs = new FileStream(@"test.xml",FileMode.Create);
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(root));
                serializer.Serialize(fs,xmlroot);
            }
        }
        async Task<List<string>> getKyukoHokoList(int i)
        {
            //パラメータ作成
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("form1:Poa00201A:htmlParentTable:htmlDetailTbl2:web1__pagerWeb", i.ToString());
            dic.Add("com.sun.faces.VIEW", session_id);
            dic.Add("form1", "form1");
            Uri uri = new Uri("https://portal.sa.dendai.ac.jp/up/faces/up/po/Poa00601A.jsp");
            var stream = await postRequest(uri, dic);
            //getSessionId(stream);
            var enc = System.Text.Encoding.UTF8;
            using (var reader = new StreamReader(stream, enc))
            using (var sgmlReader = new SgmlReader { InputStream = reader })
            {
                sgmlReader.DocType = "HTML";
                sgmlReader.CaseFolding = CaseFolding.ToLower;
                var doc = XDocument.Load(sgmlReader);
                var ns = doc.Root.Name.Namespace;

                var q = doc.Descendants(ns + "input")
                    .Where(ul => ul.Attribute("id") != null && ul.Attribute("id").Value == "com.sun.faces.VIEW")
                    .Select(el => el.Attribute("value").Value).ToList();
                session_id = q[0];

                var q2 = doc.Descendants(ns + "td")
                    .Where(ul => ul.Attribute("class") != null && ul.Attribute("class").Value == "title")
                    .Descendants(ns + "a").Descendants(ns + "span")
                    .Where(el=>el.Attribute("id") != null)
                    .Select(el => (string)el.Value);
                return q2.ToList();
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            id = id_textbox.Text;
            pass = password_box.Password;
            await toppage();
            await mainpage();
            await getJyugyoInfo();
            await getKyukoHoko();
        }
    }
}
