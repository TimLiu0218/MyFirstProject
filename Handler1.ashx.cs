using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using WebAppForPpt;

namespace WebApplication3
{
    /// <summary>
    /// Handler1 的摘要说明
    /// </summary>
    public class Handler1 : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            Encoding encode = Encoding.GetEncoding("utf-8");
            string reqData = "";
            //通过传过来的指令动态调用相应的方法
            StreamReader reader = new StreamReader(context.Request.InputStream, encode);
            reqData = reader.ReadToEnd();

            reqData = HttpUtility.UrlDecode(reqData);
            string result = new DataBase().GetData(reqData);
            context.Response.Write(result);
            reader.Close();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}