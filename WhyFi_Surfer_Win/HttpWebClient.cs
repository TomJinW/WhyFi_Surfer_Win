using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WhyFi_Surfer_Win
{
    public class HttpClient : WebClient
    {
        // Cookie 容器
        private CookieContainer cookieContainer;

        /// <summary>
        /// 创建一个新的 WebClient 实例。
        /// </summary>
        public HttpClient()
        {
            this.cookieContainer = new CookieContainer();
        }

        /// <summary>
        /// 创建一个新的 WebClient 实例。
        /// </summary>
        /// <param name="cookie">Cookie 容器</param>
        public HttpClient(CookieContainer cookies)
        {
            this.cookieContainer = cookies;
        }

        /// <summary>
        /// Cookie 容器
        /// </summary>
        public CookieContainer Cookies
        {
            get { return this.cookieContainer; }
            set { this.cookieContainer = value; }
        }

        /// <summary>
        /// 返回带有 Cookie 的 HttpWebRequest。
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                HttpWebRequest httpRequest = request as HttpWebRequest;
                httpRequest.CookieContainer = cookieContainer;
            }
            return request;
        }

        #region 封装了PostData, GetSrc 和 GetFile 方法
        /// <summary>
        /// 向指定的 URL POST 数据，并返回页面
        /// </summary>
        /// <param name="uriString">POST URL</param>
        /// <param name="postString">POST 的 数据</param>
        /// <param name="postStringEncoding">POST 数据的 CharSet</param>
        /// <param name="dataEncoding">页面的 CharSet</param>
        /// <returns>页面的源文件</returns>
        public string PostData(string uriString, string postString, string postStringEncoding, string dataEncoding, out string msg)
        {
            try
            {
                // 将 Post 字符串转换成字节数组
                byte[] postData = Encoding.GetEncoding(postStringEncoding).GetBytes(postString);
                this.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                // 上传数据，返回页面的字节数组
                byte[] responseData = this.UploadData(uriString, "POST", postData);
                // 将返回的将字节数组转换成字符串(HTML);
                string srcString = Encoding.GetEncoding(dataEncoding).GetString(responseData);
                srcString = srcString.Replace("\t", "");
                srcString = srcString.Replace("\r", "");
                srcString = srcString.Replace("\n", "");
                msg = string.Empty;
                return srcString;
            }
            catch (WebException we)
            {
                msg = we.Message;
                return string.Empty;
            }
        }

        /// <summary>
        /// 获得指定 URL 的源文件
        /// </summary>
        /// <param name="uriString">页面 URL</param>
        /// <param name="dataEncoding">页面的 CharSet</param>
        /// <returns>页面的源文件</returns>
        public string GetSrc(string uriString, string dataEncoding, out string msg)
        {
            try
            {
                // 返回页面的字节数组
                byte[] responseData = this.DownloadData(uriString);
                // 将返回的将字节数组转换成字符串(HTML);
                string srcString = Encoding.GetEncoding(dataEncoding).GetString(responseData);
                srcString = srcString.Replace("\t", "");
                srcString = srcString.Replace("\r", "");
                srcString = srcString.Replace("\n", "");
                msg = string.Empty;
                return srcString;
            }
            catch (WebException we)
            {
                msg = we.Message;
                return string.Empty;
            }
        }

        /// <summary>
        /// 从指定的 URL 下载文件到本地
        /// </summary>
        /// <param name="uriString">文件 URL</param>
        /// <param name="fileName">本地文件的完成路径</param>
        /// <returns></returns>
        public bool GetFile(string urlString, string fileName, out string msg)
        {
            try
            {
                this.DownloadFile(urlString, fileName);
                msg = string.Empty;
                return true;
            }
            catch (WebException we)
            {
                msg = we.Message;
                return false;
            }
        }
        #endregion
    }
}
