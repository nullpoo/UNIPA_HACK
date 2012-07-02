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

            var enc = System.Text.Encoding.UTF8;
            using (var reader = new StreamReader(res.GetResponseStream(), enc))
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
            WebResponse res = req.GetResponse();
            Stream resStream = res.GetResponseStream();
            Encoding enc = Encoding.GetEncoding("UTF-8");
            StreamReader reader = new StreamReader(resStream, enc);
            string result = reader.ReadToEnd();
            reader.Close();
            resStream.Close();

            textbox1.Text = result;
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            id = id_textbox.Text;
            pass = password_textbox.Text;
            await toppage();
            await mainpage();
        }
    }
}
