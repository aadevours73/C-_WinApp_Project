﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using System.IO;

namespace FTP_Client_Demo
{
//===================================FTP 서버에 접속하는 작업을 처리하는 클래스==============================
    class FTP_Access
    {
        //델리게이트
        public delegate void ExceptionEventHandler(string locationID, Exception ex);

        public event ExceptionEventHandler ExceptionEvent;
        public Exception LastException = null;

        public bool Is_Connected {get; set;}
        private string IP;
        private string port;
        private string user_ID;
        private string user_PW;

        public FTP_Access() { }

        //ftp 서버에 연결하는 메소드
        public bool Connect_FTP_Server(string ip, string port, string id, string password)
        {
            this.Is_Connected = false;

            this.IP = ip;
            this.port = port;
            this.user_ID = id;
            this.user_PW = password;

            string URL_Addr = string.Format("FTP://{0}:{1}/", this.IP, this.port);
            try
            {
                //FTP 클라 생성
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(URL_Addr);
                request.Credentials = new NetworkCredential(this.user_ID, this.user_PW);

                request.KeepAlive = false;
                //폴더내용 받아오기로 메소드 설정.
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                request.UsePassive = false;

                //응답을 받아온다.
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                //받은 응답에서 스트림을 가져와 읽는다.
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string[] data = reader.ReadToEnd().Split('\n');


                this.Is_Connected = true;
            }
            catch (Exception ex) {
                this.LastException = ex;
                //멤버 특정 정보 가져오기

                System.Reflection.MemberInfo info = System.Reflection.MethodInfo.GetCurrentMethod();
                string info_id = string.Format("{0}.{1}", info.ReflectedType.Name, info.Name);

                if (this.ExceptionEvent != null)
                {
                    this.ExceptionEvent(id, ex);
                }
                return false;
            }
            return true;
        }

        //지정한 경로에 해당하는 파일정보 리스트들을 불러오는 함수
        public List<string[]> get_File_List(string PATH) {
            string URL = string.Format("FTP://{0}:{1}/{2}", this.IP, this.port, PATH);

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(URL);
            request.Credentials = new NetworkCredential(this.user_ID, this.user_PW);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            //응답을 받아온다.
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            //받은 응답에서 스트림을 가져와 읽는다.
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string[] raw_fileInfo = reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            List<string[]> file_list = new List<string[]>();//반환할 파일정보 리스트.

            foreach (string file in raw_fileInfo)
            {
                //fileDetailes = {날짜, 용량(폴더라면 <DIR>), 파일이름}
                string date = file.Substring(0, 17);
                string Capacity = file.Substring(17, 21).Trim();
                string name = file.Substring(39);
                string[] fileDetailes = { date, Capacity, name };
                file_list.Add(fileDetailes);
            }

            return file_list;
        }


    }
}
