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
using System.Windows.Forms;
namespace WhyFi_Surfer_Win
{   
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        WindowState ws;
        WindowState wsl;
        NotifyIcon notifyIcon;
        ContextMenuStrip cms = new ContextMenuStrip();
        WebClient client = new HttpClient();
        BackgroundWorker loginWorker = new BackgroundWorker();
        BackgroundWorker loopWorker = new BackgroundWorker();
        bool KeepConnection = (bool)Properties.Settings.Default["KeepConnection"];
        bool isLogging = false;
        public MainWindow()
        {

            icon();
            contextMenu();
            wsl = WindowState;
            //org.ShanghaiTech.WhyFi_Surfer_Win
            InitializeComponent();
            loginWorker.DoWork += LoginWorker_DoWork;
            loginWorker.WorkerReportsProgress = true;

            loopWorker.DoWork += loopWorker_DoWork;
            loopWorker.WorkerReportsProgress = true;
            try
            {
                var cred = CredentialManager.GetCredentials("org.ShanghaiTech.WhyFi_Surfer_Win");
                txbUsername.Text = (string)Properties.Settings.Default["username"];
                txbPassword.Password = cred.Password;
            }
            catch (Exception e) {
            }
            checkBox.IsChecked = (bool)Properties.Settings.Default["Check"];
            if (txbUsername.Text != "" && txbPassword.Password != "") {
                this.Hide();
                this.WindowState = WindowState.Normal;;
                notifyIcon.Visible = true;
                showBallonTip("WhyFi 登录器在这里！", "双击或者右键单击都可以。", ToolTipIcon.Info, 1500);
            }
            loopWorker.RunWorkerAsync();
        }
        private void showBallonTip(string title,string text,ToolTipIcon type,int milsecond) {
            this.notifyIcon.BalloonTipTitle = title;
            this.notifyIcon.BalloonTipText = text;
            this.notifyIcon.BalloonTipIcon = type;
            notifyIcon.ShowBalloonTip(milsecond);

        }
        private void icon()
        {
            string path = System.IO.Path.GetFullPath(@"icon\icon.ico");
            if (File.Exists(path))
            {
                this.notifyIcon = new NotifyIcon();
                
                this.notifyIcon.Text ="WhyFi_Surfer_Win";
                System.Drawing.Icon icon = new System.Drawing.Icon(path);//程序图标
               

                this.notifyIcon.Icon = icon;
                this.notifyIcon.Visible = true;
                
                notifyIcon.MouseDoubleClick += OnNotifyIconDoubleClick;
                notifyIcon.Visible = false;
            }

        }
        #region 托盘右键菜单
        private void onMenuOpening(object sender, EventArgs e) {
            var username = txbUsername.Text;
            var password = txbPassword.Password;
            if (username == "" || password == "")
            {
                cms.Items[0].Text = "已保存用户名: (N/A)";
            }
            else {
                cms.Items[0].Text = "已保存用户名: " + username;
            }

            if (KeepConnection)
            {
                cms.Items[5].Text = "每半小时重新登录: 开";
            }
            else {
                cms.Items[5].Text = "每半小时重新登录: 关";
            }
            



        }
        private void contextMenu()
        {
            

            //关联 NotifyIcon 和 ContextMenuStrip
            notifyIcon.ContextMenuStrip = cms;
            cms.Opening += onMenuOpening;
            System.Windows.Forms.ToolStripSeparator seperator = new ToolStripSeparator();
            System.Windows.Forms.ToolStripSeparator seperator2 = new ToolStripSeparator();
            System.Windows.Forms.ToolStripSeparator seperator3 = new ToolStripSeparator();

            System.Windows.Forms.ToolStripMenuItem usernameItem = new System.Windows.Forms.ToolStripMenuItem();
            usernameItem.Text = "已保存用户名:"; usernameItem.Enabled = false;

            System.Windows.Forms.ToolStripMenuItem loginItem = new System.Windows.Forms.ToolStripMenuItem();
            loginItem.Text = "登录";
            loginItem.Click += new EventHandler(loginItem_Click);

            System.Windows.Forms.ToolStripMenuItem showMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            showMenuItem.Text = "显示登录窗口";
            showMenuItem.Click += new EventHandler(showMenuItem_Click);

            System.Windows.Forms.ToolStripMenuItem ssidItem = new System.Windows.Forms.ToolStripMenuItem();
            ssidItem.Text = "Wi-Fi:";ssidItem.Enabled = false;

            System.Windows.Forms.ToolStripMenuItem connectItem = new System.Windows.Forms.ToolStripMenuItem();
            connectItem.Text = "每半小时重新登录:";
            connectItem.Click += new EventHandler(connectItem_Click);

            System.Windows.Forms.ToolStripMenuItem lastLoginItem = new System.Windows.Forms.ToolStripMenuItem();
            lastLoginItem.Text = "上次登录: N/A";
            lastLoginItem.Enabled = false;

            System.Windows.Forms.ToolStripMenuItem exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exitMenuItem.Text = "退出";
            exitMenuItem.Click += new EventHandler(exitMenuItem_Click);





            cms.Items.Add(usernameItem);
            cms.Items.Add(seperator);
            cms.Items.Add(loginItem);
            cms.Items.Add(showMenuItem);
            cms.Items.Add(seperator2);
            //cms.Items.Add(ssidItem);
            cms.Items.Add(connectItem);
            cms.Items.Add(lastLoginItem);
            cms.Items.Add(seperator3);
            cms.Items.Add(exitMenuItem);
        }
        private void loginItem_Click(object sender, EventArgs e)
        {
            var username = txbUsername.Text;
            var password = txbPassword.Password;
            if (username == "" || password == "")
            {
                notifyIcon.Visible = false;
                this.Show();
                this.Activate();
                WindowState = wsl;
            }
            else {

                loginWorker.RunWorkerAsync(new Tuple<string, string, int>(txbUsername.Text, txbPassword.Password, 2));
            }
                
        }
        private void connectItem_Click(object sender, EventArgs e)
        {
            KeepConnection = !KeepConnection;
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            if (notifyIcon != null)
            {
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
                notifyIcon = null;
            }
            System.Windows.Application.Current.Shutdown();
        }

        private void showMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            this.Show();
            this.Activate();
            WindowState = wsl;
        }
        #endregion

        private void OnNotifyIconDoubleClick(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            this.Show();
            this.Activate();
            WindowState = wsl;

        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            ws = this.WindowState;
            if (ws == WindowState.Minimized)
            { 
                this.Hide();
                //this.WindowState = WindowState.Normal;
                notifyIcon.Visible = true;
                showBallonTip("WhyFi 登录器在这里！", "双击或者右键单击都可以。", ToolTipIcon.Info, 1000);
            }
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

                client.Headers.Add(HttpRequestHeader.Cookie, "fasfdsfdsf3wrew");
                byte[] response;
                try
                {
                    response = client.UploadValues("https://controller.shanghaitech.edu.cn:8445/PortalServer/Webauth/webAuthAction!syncPortalAuthResult.action", values);
                }
                catch (Exception e)
                { return false; }
                var responseString = Encoding.UTF8.GetString(response);
                JObject json = (JObject)JsonConvert.DeserializeObject(responseString);
                //Console.WriteLine(json.ToString());

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
            //using (var client = new WebClient())
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
        private async void loopWorker_DoWork(object sender, DoWorkEventArgs e) {
            while (true)
            {
                
                if (KeepConnection && !isLogging) {
                    try
                    {
                        //client.DownloadData("http://baidu.com");
                        client.DownloadData("https://controller.shanghaitech.edu.cn:8445/PortalServer/Webauth/webAuthAction!login.action");
                        Dispatcher.Invoke(new Action(() => {
                            loginWorker.RunWorkerAsync(new Tuple<string, string, int>(txbUsername.Text, txbPassword.Password, 0));
                        }));
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine(err.Message);
                    }


                    
                }
                System.Threading.Thread.Sleep(1800000);

            }
        }

        private async void LoginWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var username = ((Tuple<string, string,int>)e.Argument).Item1;
            var password = ((Tuple<string, string,int>)e.Argument).Item2;
            var type = ((Tuple<string, string, int>)e.Argument).Item3;






            if (username == "" || password == "")
            {
                Dispatcher.Invoke(new Action(() => {
                    if (type == 1)
                    {
                        showTaskDialog("出错", "用户名或密码不能为空", "请将用户名和密码补充完整", new string[] { "&OK" }, mainIcon: VistaTaskDialogIcon.Warning);
                    }
                    else {
                        showBallonTip("用户名或密码不能为空", "请将用户名和密码补充完整", ToolTipIcon.Warning, 1000);
                    }
                    
                }));
                return;
            }
            isLogging = true;
            Dispatcher.Invoke(new Action(() => {
                button.IsEnabled = false;
                txbPassword.IsEnabled = false;
                txbUsername.IsEnabled = false;
                checkBox.IsEnabled = false;
                cms.Items[2].Enabled = false;
                lblStatus.Text = "正在登录...";
                if (type != 1 && type != 0) {
                    showBallonTip("正在登录", "这可能需要一段时间", ToolTipIcon.Info, 1000);
                }
            }));



            
            var status = Login(username, password);
            if (status.Item1)
            {
                var success = false;
                var ip = status.Item2;
                for (int i = 1; i <= 10; i++) {
                    if (type == 1)
                    {
                        Dispatcher.Invoke(new Action(() => {
                            lblStatus.Text = "正在登录... " + i.ToString() + "/10";
                        }));
                    }


                    Console.WriteLine(i);
                    success = success || SyncRequest(ip);
                    if (success) {
                        i = 11;
                        Dispatcher.Invoke(new Action(() => {
                            if (type != 1 && type != 0)
                            {
                                showBallonTip("成功登录", "可以上网啦!", ToolTipIcon.Info, 1000);
                            }
                            else {
                                showTaskDialog("成功登录", "可以上网啦！", "现在可以最小化本应用了。应用会保留在系统托盘里哦", new string[] { "&OK" });
                            }
                            cms.Items[6].Text = "上次登录: " + DateTime.Now.ToString();
                            button.IsEnabled = true;
                            txbPassword.IsEnabled = true;
                            txbUsername.IsEnabled = true;
                            checkBox.IsEnabled = true;
                            cms.Items[2].Enabled = true;
                            lblStatus.Text = "";
                        }));
                    }
                    await Task.Delay(3000);
                }
                if (!success) {
                    Dispatcher.Invoke(new Action(() => {
                        if (type != 1 && type != 0)
                        {
                            showBallonTip("反馈结果超时", "请检查网络后重试", ToolTipIcon.Warning, 1000);
                        }
                        else {
                            showTaskDialog("出错", "反馈结果超时", "请检查网络后重试", new string[] { "&OK" }, mainIcon: VistaTaskDialogIcon.Warning);
                        }
                            
                    }));
                }

 
            }
            else {
                Dispatcher.Invoke(new Action(() => {
                    if (type != 1)
                    {
                        showBallonTip("请确认以下信息", status.Item3, ToolTipIcon.Warning, 1000);
                    }
                    else
                    {
                        showTaskDialog("出错", "请确认以下信息", status.Item3, new string[] { "&OK" }, mainIcon: VistaTaskDialogIcon.Warning);
                    }
                    
                }));
            }
            Dispatcher.Invoke(new Action(() => {
                button.IsEnabled = true;
                txbPassword.IsEnabled = true;
                txbUsername.IsEnabled = true;
                checkBox.IsEnabled = true;
                cms.Items[2].Enabled = true;
                lblStatus.Text = "";
            }));
            isLogging = false;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            loginWorker.RunWorkerAsync(new Tuple<string, string,int>(txbUsername.Text, txbPassword.Password,1));
        }

        private void txbPassword_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.Enter))
            {
                loginWorker.RunWorkerAsync(new Tuple<string, string, int>(txbUsername.Text, txbPassword.Password, 1));
            }

        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default["Check"] = checkBox.IsChecked;
            Properties.Settings.Default["username"] = txbUsername.Text;
            Properties.Settings.Default["KeepConnection"] = KeepConnection;
            Properties.Settings.Default.Save();
            switch (checkBox.IsChecked) {
                case true:
                    savePassword(txbUsername.Text, txbPassword.Password);
                    break;
                case false:
                    try
                    {
                        CredentialManager.RemoveCredentials("org.ShanghaiTech.WhyFi_Surfer_Win");
                    }
                    catch {

                    }
                    
                    break;
            }
            if (notifyIcon != null)
            {
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
                notifyIcon = null;
            }
        }
    }
}
