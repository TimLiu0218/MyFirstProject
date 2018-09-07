using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebAppForPpt
{
    public partial class GetData : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Encoding encode = Encoding.GetEncoding("utf-8");
            string reqData = "";
            //通过传过来的指令动态调用相应的方法
            StreamReader reader = new StreamReader(Request.InputStream, encode);
            reqData = reader.ReadToEnd();

            reqData = HttpUtility.UrlDecode(reqData);
            string result = new DataBase().GetData(reqData);

            Response.Write(result);//回传
            reader.Close();
        }

    }
}