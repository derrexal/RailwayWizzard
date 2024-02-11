using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RzdHack_Robot.App
{
    public class HttpServer
    {
        private readonly HttpListener _listener;

        public HttpServer()
        {
            _listener=new HttpListener();
        }


    }
}
