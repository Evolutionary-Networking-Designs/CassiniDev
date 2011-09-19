//  **********************************************************************************
//  CassiniDev - http://cassinidev.codeplex.com
// 
//  Copyright (c) 2010 Sky Sanders. All rights reserved.
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  
//  This source code is subject to terms and conditions of the Microsoft Public
//  License (Ms-PL). A copy of the license can be found in the license.txt file
//  included in this distribution.
//  
//  You must not remove this notice, or any other, from this software.
//  
//  **********************************************************************************

#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;
using CassiniDev.ServerLog;
using System.Net.Security;

#endregion

namespace CassiniDev
{
    public class Connection : MarshalByRefObject
    {
        private const int HttpForbidden = 403;

        private const int HttpOK = 200;

        private readonly MemoryStream _responseContent;
        public List<string> Plugins = new List<string>();
        private readonly Server _server;
        private LogInfo _requestLog;
        private LogInfo _responseLog;

        private Socket _socket;
        private Stream _socketStream;

		//not sure if i need to go about this but for the moment i'm leaving it in
        private List<IDisposable> toDispose = new List<IDisposable>();


        internal Connection(Server server, Socket socket)
        {
            Plugins = server.Plugins;
            Id = Guid.NewGuid();
            _responseContent = new MemoryStream();
            _server = server;
            _socket = socket;
            if (_socket.Connected)
            {
                _socketStream = new NetworkStream(_socket, true);


                if (server.X509Certificate != null && server.RequireSsl)
                {
                    var ssl = new SslStream(_socketStream);
                    _socketStream = ssl;
                    try
                    {
                        ssl.AuthenticateAsServer(server.X509Certificate);
                    }
                    catch //(Exception e)
                    {
                        //ErrorLogger.Log(e);
                        //throw new Exception("Error authenticating ssl connection");
                    }
                }
            }
            InitializeLogInfo();
        }

        public bool Connected
        {
            get { return _socket.Connected; }
        }

        public Guid Id { get; private set; }

        public string LocalIP
        {
            get
            {
                IPEndPoint ep = (IPEndPoint) _socket.LocalEndPoint;
                return (ep != null && ep.Address != null) ? ep.Address.ToString() : "127.0.0.1";
            }
        }

        public string RemoteIP
        {
            get
            {
                IPEndPoint ep = (IPEndPoint) _socket.RemoteEndPoint;
                return (ep != null && ep.Address != null) ? ep.Address.ToString() : "127.0.0.1";
            }
        }

        public LogInfo RequestLog
        {
            get { return _requestLog; }
        }

        public LogInfo ResponseLog
        {
            get { return _responseLog; }
        }

        public void Close()
        {
            FinalizeLogInfo();
            try
            {
                _socket.Shutdown(SocketShutdown.Both);

                _socketStream.Close();

            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
            }
            finally
            {
                _socket = null;
                _socketStream = null;
            }

        }

        /// <summary>
        /// </summary>
        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void LogRequest(string pathTranslated, string url)
        {
            _requestLog.PathTranslated = pathTranslated;

            _requestLog.Url = url;
        }

        public void LogRequestBody(byte[] content)
        {
            _requestLog.Body = content;
        }

        public void LogRequestHeaders(string headers)
        {
            _requestLog.Headers = headers;
        }

        public byte[] ReadRequestBytes(int maxBytes)
        {
            try
            {
                if (WaitForRequestBytes() == 0)
                {
                    return null;
                }

                int numBytes = _socket.Available;

                if (numBytes > maxBytes)
                {
                    numBytes = maxBytes;
                }

                int numReceived = 0;

                byte[] buffer = new byte[numBytes];

                if (numBytes > 0)
                {
                    numReceived = _socketStream.Read(buffer, 0, numBytes);
                }

                if (numReceived < numBytes)
                {
                    byte[] tempBuffer = new byte[numReceived];

                    if (numReceived > 0)
                    {
                        Buffer.BlockCopy(buffer, 0, tempBuffer, 0, numReceived);
                    }

                    buffer = tempBuffer;
                }

                //Console.WriteLine();
                //Console.WriteLine();
                //Console.WriteLine("---------------------");
                //Console.WriteLine(UTF8Encoding.UTF8.GetString(buffer));
                

                return buffer;
            }
            catch
            {
                return null;
            }
        }

        public int WaitForRequestBytes()
        {
            int availBytes = 0;

            try
            {
                if (_socket.Available == 0)
                {
                    _socket.Poll(100000, SelectMode.SelectRead);

                    if (_socket.Available == 0 && _socket.Connected)
                    {
                        _socket.Poll(30000000, SelectMode.SelectRead);
                    }
                }

                availBytes = _socket.Available;
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch
                // ReSharper restore EmptyGeneralCatchClause
            {
            }

            return availBytes;
        }

        public void Write100Continue()
        {
            WriteEntireResponseFromString(100, null, null, true);
        }
        internal void Write200Continue()
        {
            WriteEntireResponseFromString(200, null, string.Empty, true);
        }
        public void WriteBody(byte[] data, int offset, int length)
        {
            try
            {
                _responseContent.Write(data, 0, data.Length);
                _socketStream.Write(data, offset, length);
            }
            catch
            {
            }
        }

        public void WriteEntireResponseFromFile(String fileName, bool keepAlive)
        {
            if (!File.Exists(fileName))
            {
                WriteErrorAndClose(404);
                return;
            }

            // Deny the request if the contentType cannot be recognized.

            string contentType = CommonExtensions.GetContentType(fileName);

            //TODO: i am pretty sure this is unnecessary
            if (contentType == null)
            {
                WriteErrorAndClose(HttpForbidden);
                return;
            }

            string contentTypeHeader = "Content-Type: " + contentType + "\r\n";

            bool completed = false;
            FileStream fs = null;

            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                int len = (int) fs.Length;
                byte[] fileBytes = new byte[len];
                int bytesRead = fs.Read(fileBytes, 0, len);

                String headers = MakeResponseHeaders(HttpOK, contentTypeHeader, bytesRead, keepAlive);
                _responseLog.Headers = headers;
                _responseLog.StatusCode = HttpOK;

                var headerBytes = Encoding.UTF8.GetBytes(headers);
                _socketStream.Write(headerBytes, 0, headerBytes.Length);

                _socketStream.Write(fileBytes, 0, bytesRead);

                completed = true;
            }
            catch (IOException e)
            {
            }
            finally
            {
                if (!keepAlive || !completed)
                {
                    Close();
                }

                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        public void WriteEntireResponseFromString(int statusCode, String extraHeaders, String body, bool keepAlive)
        {
            try
            {
                int bodyLength = (body != null) ? Encoding.UTF8.GetByteCount(body) : 0;
                string headers = MakeResponseHeaders(statusCode, extraHeaders, bodyLength, keepAlive);

                _responseLog.Headers = headers;
                _responseLog.StatusCode = statusCode;
                var headerBytes = Encoding.UTF8.GetBytes(headers + body);

                _socketStream.Write(headerBytes, 0, headerBytes.Length);

            }
            catch// (IOException e)
            {
           //     ErrorLogger.Log(e);
            }
            finally
            {
                if (!keepAlive)
                {
                    Close();
                }
            }
        }

        public void WriteErrorAndClose(int statusCode, string message)
        {
            WriteEntireResponseFromString(statusCode, null, GetErrorResponseBody(statusCode, message), false);
        }

        public void WriteErrorAndClose(int statusCode)
        {
            WriteErrorAndClose(statusCode, null);
        }

        public void WriteErrorWithExtraHeadersAndKeepAlive(int statusCode, string extraHeaders)
        {
            WriteEntireResponseFromString(statusCode, extraHeaders, GetErrorResponseBody(statusCode, null), true);
        }

        public void WriteHeaders(int statusCode, String extraHeaders)
        {
            string headers = MakeResponseHeaders(statusCode, extraHeaders, -1, false);


            _responseLog.Headers = headers;
            _responseLog.StatusCode = statusCode;

            try
            {
                var headerBytes = Encoding.UTF8.GetBytes(headers);
                _socketStream.Write(headerBytes, 0, headerBytes.Length);


                //Console.WriteLine("--");
                //Console.WriteLine();
                //Console.WriteLine(headers);
                //Console.WriteLine("---------------------");
                //Console.WriteLine();
                //Console.WriteLine();
            }
            catch (IOException e)
            {
            }
        }

        private void FinalizeLogInfo()
        {
            try
            {
                _responseLog.Body = _responseContent.ToArray();
                _responseContent.Dispose();
                _responseLog.Created = DateTime.Now;
                _responseLog.Url = _requestLog.Url;
                _responseLog.PathTranslated = _requestLog.PathTranslated;
                _responseLog.Identity = _requestLog.Identity;
                _responseLog.PhysicalPath = _requestLog.PhysicalPath;
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch
                // ReSharper restore EmptyGeneralCatchClause
            {
                // log error to text
            }
        }

        private string GetErrorResponseBody(int statusCode, string message)
        {
            string body = Messages.FormatErrorMessageBody(statusCode, _server.VirtualPath);

            if (!string.IsNullOrEmpty(message))
            {
                body += "\r\n<!--\r\n" + message + "\r\n-->";
            }

            return body;
        }

        private void InitializeLogInfo()
        {
            _requestLog = new LogInfo
                {
                    Created = DateTime.Now,
                    ConversationId = Id,
                    RowType = 1,
                    Identity = _server.GetProcessUser(),
                    PhysicalPath = _server.PhysicalPath
                };

            _responseLog = new LogInfo
                {
                    ConversationId = Id,
                    RowType = 2
                };
        }

        private static string MakeResponseHeaders(int statusCode, string moreHeaders, int contentLength, bool keepAlive)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("HTTP/1.1 " + statusCode + " " + HttpWorkerRequest.GetStatusDescription(statusCode) + "\r\n");
            sb.Append("Server: Cassini/" + Messages.VersionString + "\r\n");
            sb.Append("Date: " + DateTime.Now.ToUniversalTime().ToString("R", DateTimeFormatInfo.InvariantInfo) + "\r\n");

            if (contentLength >= 0)
            {
                sb.Append("Content-Length: " + contentLength + "\r\n");
            }

            if (moreHeaders != null)
            {
                sb.Append(moreHeaders);
            }

            if (!keepAlive)
            {
                sb.Append("Connection: Close\r\n");
            }

            sb.Append("\r\n");

            return sb.ToString();
        }
    }
}