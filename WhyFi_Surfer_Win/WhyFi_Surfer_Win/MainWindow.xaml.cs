using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Net;
using System.Collections.Specialized;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TaskDialogInterop;
using System.IO;
using AdysTech.CredentialManager;

namespace WhyFi_Surfer_Win
{   
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {


        BackgroundWorker loginWorker = new BackgroundWorker();
        public MainWindow()
        {
            //org.ShanghaiTech.WhyFi_Surfer_Win
            InitializeComponent();
            loginWorker.DoWork += LoginWorker_DoWork;
            loginWorker.WorkerReportsProgress = true;
            try
            {
                var cred = CredentialManager.GetCredentials("org.ShanghaiTech.WhyFi_Surfer_Win");
                txbUsername.Text = (string)Properties.Settings.Default["username"];
                txbPassword.Password = cred.Password;
            }
            catch (Exception e) {
            }
            checkBox.IsChecked = (bool)Properties.Settings.Default["Check"];
        }
        private TaskDialogResult showTaskDialog(string title,string mainIns,string Content,string[] buttons,string footer = "", 
            VistaTaskDialogIcon mainIcon = VistaTaskDialogIcon.Information, VistaTaskDialogIcon footerIcon = VistaTaskDialogIcon.Information) {
            TaskDialogOptions config = new TaskDialogOptions();
            config.Owner = this;
            config.Title = title;
            config.MainInstruction = mainIns;
            config.Content = Content;
            //config.ExpandedInfo = "Any expanded content text for the " +
            //                  "task dialog is shown here and the text " +
            //                  "will automatically wrap as needed.";
            //config.VerificationText = "Don't show me this message again";
            config.CustomButtons = buttons; //new string[] { "&Save", "Do&n't save", "&Cancel" };
            config.MainIcon = mainIcon; // VistaTaskDialogIcon.Shield;
            if (footer != "") {
                config.FooterText = footer;
                config.FooterIcon = footerIcon;
            }
            

            return TaskDialog.Show(config);
        }
        private bool SyncRequest(string ip) {
            //var request = (HttpWebRequest)WebRequest.Create("https://controller.shanghaitech.edu.cn:8445/PortalServer/Webauth/webAuthAction!syncPortalAuthResult.action");

            //var postData = "authLan=zh_CN&hasValidateCode=False&validCode=&hasValidateNextUpdatePassword=true&rememberPwd=false&browserFlag=zh&hasCheckCode=false&checkcode=&saveTime=14&autoLogin=false&userMac=&isBoardPage=false&browserFlag=zh&clientIp=";
            //postData += ip;
            //var data = Encoding.UTF8.GetBytes(postData);
            //request.Method = "POST";
            //request.ContentType = "application/x-www-form-urlencoded";
            //request.ContentLength = data.Length;
            //using (var stream = request.GetRequestStream())
            //{
            //    stream.Write(data, 0, data.Length);
            //}
            //var response = (HttpWebResponse)request.GetResponse();
            //var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            //JObject json2 = (JObject)JsonConvert.DeserializeObject(responseString);
            //Console.WriteLine(json2.ToString());
            //return false;

            using (var client = new WebClient())
            {
                var values = new NameValueCollection();
                values["authLan"] = "zh_CN";
                values["hasValidateCode"] = "False";
                values["validCode"] = "";
                values["hasValidateNextUpdatePassword"] = "true";
                values["rememberPwd"] = "false";
                values["browserFlag"] = "zh";
                values["hasCheckCode"] = "false";
                values["checkcode"] = "";
                values["saveTime"] = "14";
                values["autoLogin"] = "false";
                values["userMac"] = "";
                values["isBoardPage"] = "false";
                values["browserFlag"] = "zh";
                values["clientIp"] = ip;

                byte[] response;
                try
                {
                    response = client.UploadValues("https://controller.shanghaitech.edu.cn:8445/PortalServer/Webauth/webAuthAction!syncPortalAuthResult.action", values);
                }
                catch (Exception e)
                { return false; }
                var responseString = Encoding.UTF8.GetString(response);
                JObject json = (JObject)JsonConvert.DeserializeObject(responseString);
                Console.WriteLine(json.ToString());

                if (json["data"] != null && json["data"].ToString() != "[]" && json["data"]["portalAuthStatus"] != null)
                {
                    if (json["data"]["portalAuthStatus"].ToString() == "1")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

        }

        private Tuple<bool,string,string> Login(string username, string password)
        {
            using (var client = new WebClient())
            {
                var values = new NameValueCollection();
                values["userName"] = username;
                values["password"] = password;
                byte[] response;
                try
                {
                   response = client.UploadValues("https://controller.shanghaitech.edu.cn:8445/PortalServer/Webauth/webAuthAction!login.action", values);
                }
                catch (Exception e)
                {
                    return new Tuple<bool, string, string>(false, "", e.Message);
                }

                
                string responseString = Encoding.UTF8.GetString(response);
                //Console.WriteLine(responseString);
                JObject json = (JObject)JsonConvert.DeserializeObject(responseString);
                //sConsole.WriteLine(json.ToString());
                if (json != null)
                {
                    if (json["data"] != null)
                    {
                        JObject json2nd;
                        try {
                            json2nd = (JObject)JsonConvert.DeserializeObject(json["data"].ToString());
                        }
                        catch (Exception e){
                            return new Tuple<bool, string, string>(false, "", e.Message);
                        }
                        

                        if (json2nd != null && json2nd["ip"] != null && json2nd["accessStatus"] != null)
                        {
                            if (json2nd["accessStatus"].ToString() == "200")
                            {
                                return new Tuple<bool, string, string>(true, json2nd["ip"].ToString(), "");
                            }
                            else
                            {
                                if (json["message"] != null)
                                {
                                    string message = json["message"].ToString();
                                    return new Tuple<bool, string, string>(false, "", message);
                                }
                                else
                                {
                                    return new Tuple<bool, string, string>(false, "", "Unknown Reason");
                                }
                            }
                        }
                        else
                        {
                            return new Tuple<bool, string, string>(false, "", "DATA BROKEN");
                        }
                    }
                    else
                    {
                        return new Tuple<bool, string, string>(false, "", "NETWORK UNAVAILABLE");
                    }
                }
                else {
                    return new Tuple<bool, string, string>(false, "", "NETWORK UNAVAILABLE");
                }


               // Console.WriteLine(responseString);
            }
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }
        private void savePassword(string username, string password) {
            var cred = new NetworkCredential(username, password);
            CredentialManager.SaveCredentials("org.ShanghaiTech.WhyFi_Surfer_Win", cred);

        }
        private async void LoginWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var username = ((Tuple<string, string>)e.Argument).Item1;
            var password = ((Tuple<string, string>)e.Argument).Item2;

            



              

            if (username == "" || password == "")
            {
                Dispatcher.Invoke(new Action(() => {
                    showTaskDialog("出错", "用户名或密码不能为空", "请将用户名和密码补充完整", new string[] { "&OK" }, mainIcon: VistaTaskDialogIcon.Warning);
                }));
                return;
            }

            Dispatcher.Invoke(new Action(() => {
                button.IsEnabled = false;
                txbPassword.IsEnabled = false;
                txbUsername.IsEnabled = false;
                checkBox.IsEnabled = false;
                lblStatus.Text = "正在登录...";
            }));




            var status = Login(username, password);
            if (status.Item1)
            {
                var success = false;
                var ip = status.Item2;
                for (int i = 1; i <= 10; i++) {
                    Dispatcher.Invoke(new Action(() => {
                        lblStatus.Text = "正在登录... "+i.ToString()+"/10";
                    }));
                    Console.WriteLine(i);
                    success = success || SyncRequest(ip);
                    if (success) {
                        i = 11;
                        Dispatcher.Invoke(new Action(() => {
                            showTaskDialog("成功登录", "可以上网啦！", "现在可以最小化本应用了。", new string[] { "&OK" });
                        }));
                    }
                    await Task.Delay(3000);
                }
                if (!success) {
                    Dispatcher.Invoke(new Action(() => {
                        showTaskDialog("出错", "反馈结果超时", "请检查网络后重试", new string[] { "&OK" }, mainIcon: VistaTaskDialogIcon.Warning);
                    }));
                }

 
            }
            else {
                Dispatcher.Invoke(new Action(() => {
                    showTaskDialog("出错", "请确认以下信息", status.Item3, new string[] { "&OK" },mainIcon:VistaTaskDialogIcon.Warning);
                }));
            }
            Dispatcher.Invoke(new Action(() => {
                button.IsEnabled = true;
                txbPassword.IsEnabled = true;
                txbUsername.IsEnabled = true;
                checkBox.IsEnabled = true;
                lblStatus.Text = "";
            }));
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            loginWorker.RunWorkerAsync(new Tuple<string, string>(txbUsername.Text, txbPassword.Password));
        }

        private void txbPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.Enter))
            {
                loginWorker.RunWorkerAsync(new Tuple<string, string>(txbUsername.Text, txbPassword.Password));
            }

        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default["Check"] = checkBox.IsChecked;
            Properties.Settings.Default["username"] = txbUsername.Text;
            Properties.Settings.Default.Save();
            switch (checkBox.IsChecked) {
                case true:
                    savePassword(txbUsername.Text, txbPassword.Password);
                    break;
                case false:
                    CredentialManager.RemoveCredentials("org.ShanghaiTech.WhyFi_Surfer_Win");
                    break;
            }
        }
    }
}
