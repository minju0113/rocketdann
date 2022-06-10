using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
namespace Chat_Server
{
///
/// Form1에 대한 요약 설명입니다.
///
public class frm_Chat_Server : System.Windows.Forms.Form
{
private System.Windows.Forms.TextBox txt_Chat;
private System.Windows.Forms.Button cmd_Start;
private System.Windows.Forms.Label lbl_Message;
///
/// 필수 디자이너 변수입니다.
///
private System.ComponentModel.Container components = null ;
public frm_Chat_Server()
{
//
// Windows Form 디자이너 지원에 필요합니다.
//
InitializeComponent();
//
// TODO: InitializeComponent를 호출한 다음 생성자 코드를 추가합니다.
//
}
///
/// 사용 중인 모든 리소스를 정리합니다.
///
protected override void Dispose( bool disposing )
{
if ( disposing )
{
if (components != null )
{
components.Dispose();
}
}
base .Dispose( disposing );
}
#region Windows Form 디자이너에서 생성한 코드
///
/// 디자이너 지원에 필요한 메서드입니다.
/// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
///
private void InitializeComponent()
{
this .txt_Chat = new System.Windows.Forms.TextBox();
this .cmd_Start = new System.Windows.Forms.Button();
this .lbl_Message = new System.Windows.Forms.Label();
this .SuspendLayout();
//
// txt_Chat
//
this .txt_Chat.Location = new System.Drawing.Point(5, 37);
this .txt_Chat.Multiline = true ;
this .txt_Chat.Name = "txt_Chat";
this .txt_Chat.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
this .txt_Chat.Size = new System.Drawing.Size(481, 147);
this .txt_Chat.TabIndex = 1;
this .txt_Chat.TabStop = false ;
this .txt_Chat.Text = "";
//
// cmd_Start
//
this .cmd_Start.Location = new System.Drawing.Point(6, 6);
this .cmd_Start.Name = "cmd_Start";
this .cmd_Start.Size = new System.Drawing.Size(106, 24);
this .cmd_Start.TabIndex = 0;
this .cmd_Start.Text = "Server Start";
this .cmd_Start.Click += new System.EventHandler( this .cmd_Start_Click);
//
// lbl_Message
//
this .lbl_Message.AutoSize = true ;
this .lbl_Message.Location = new System.Drawing.Point(333, 10);
this .lbl_Message.Name = "lbl_Message";
this .lbl_Message.Size = new System.Drawing.Size(146, 17);
this .lbl_Message.TabIndex = 2;
this .lbl_Message.Tag = "Stop";
this .lbl_Message.Text = "Server Stop 상태 입니다.";
this .lbl_Message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
//
// frm_Chat_Server
//
this .AutoScaleBaseSize = new System.Drawing.Size(6, 14);
this .ClientSize = new System.Drawing.Size(492, 189);
this .Controls.Add( this .lbl_Message);
this .Controls.Add( this .cmd_Start);
this .Controls.Add( this .txt_Chat);
this .Name = "frm_Chat_Server";
this .StartPosition = System.Windows.Forms.FormStartPosition.Manual;
this .Text = "Chat Server";
this .Closed += new System.EventHandler( this .frm_Chat_Server_Closed);
this .ResumeLayout( false );
}
#endregion
///
/// 해당 응용 프로그램의 주 진입점입니다.
///
[MTAThread]
static void Main ()
{
Application.Run( new frm_Chat_Server());
}
//Listner 정의 및 IP, Port 할당.
TcpListener lit_Listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 5002);
public static ArrayList soketArray = new ArrayList();
private void frm_Chat_Server_Closed( object sender, System.EventArgs e)
{
Application.Exit();
lit_Listener.Stop();
}
private void cmd_Start_Click( object sender, System.EventArgs e)
{
if (lbl_Message.Tag.ToString()=="Stop")
{
//Listener Start
lit_Listener.Start();
//Client로 부터 연결을 기다리는 Thread생성
Thread thd_WaitSocket = new Thread( new ThreadStart(Wait_Socket));
thd_WaitSocket.Start();
lbl_Message.Text = "Server Start 상태 입니다.";
lbl_Message.Tag = "Start";
cmd_Start.Text = "Server Stop";
}
else
{
lit_Listener.Stop();
foreach (Socket soket in frm_Chat_Server.soketArray)
{
soket.Close();
}
frm_Chat_Server.soketArray.Clear();
lbl_Message.Text = "Server Stop 상태 입니다.";
lbl_Message.Tag = "Stop";
cmd_Start.Text = "Server Start";
}
}
private void Wait_Socket()
{
Socket sktClient = null ;
while ( true )
{
//Socket 생성 및 연결 대기
try
{
//Client 연결을 기다린다.
sktClient = lit_Listener.AcceptSocket();
//Chatting을 실행하는 Class 인스턴스화시키고 Socket 할당
Chat_Class cht_Class = new Chat_Class();
cht_Class.Chat_Class_Setup(sktClient, this .txt_Chat);
//Chatting을 실행하는Thread 생성
Thread thd_ChatProcess = new Thread( new ThreadStart(cht_Class.Chat_Process));
thd_ChatProcess.Start();
}
catch (System.Exception)
{
frm_Chat_Server.soketArray.Remove(sktClient);
break ;
}
}
}
}
public class Chat_Class
{
//한글 처리를 위해 Encod 정의
private Encoding ecd_Encode = Encoding.GetEncoding("KS_C_5601-1987");
//글자를 Display할 Object
private System.Windows.Forms.TextBox txt_Chat;
private Socket sktClient;
private NetworkStream netStream;
private StreamReader strReader;
public void Chat_Class_Setup(Socket sktClient, System.Windows.Forms.TextBox txt_Chat)
{
//TextBox를 할당함.
this .txt_Chat = txt_Chat;
//Socket 을 할당함.
this .sktClient = sktClient;
//Network Stream을 생성
this .netStream = new NetworkStream(sktClient);
frm_Chat_Server.soketArray.Add(sktClient);
//Stream Reader을 생성
this .strReader = new StreamReader(netStream, ecd_Encode);
}
public void Chat_Process()
{
while ( true )
{
try
{
//문자열을 받음
string lstMessage = strReader.ReadLine();
if (lstMessage!= null && lstMessage!="" )
{
this .txt_Chat.AppendText(lstMessage+"\r\n");
byte [] bytSand_Data = Encoding.Default.GetBytes(lstMessage+"\r\n");
ArrayList remove_soketArray = new ArrayList();
lock (frm_Chat_Server.soketArray)
{
foreach (Socket soket in frm_Chat_Server.soketArray)
{
NetworkStream stream = new NetworkStream(soket);
stream.Write(bytSand_Data,0,bytSand_Data.Length);
}
}
}
}
catch (System.Exception )
{
frm_Chat_Server.soketArray.Remove(sktClient);
break ;
}
}
}
}
}
[클라이언트 - frm_Chat_Client.cs]
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
namespace Chat_Client
{
///
/// Form1에 대한 요약 설명입니다.
///
public class frm_Chat_Client : System.Windows.Forms.Form
{
private System.Windows.Forms.TextBox txt_Chat;
private System.Windows.Forms.TextBox txt_Msg;
private System.Windows.Forms.TextBox txt_Name;
private System.Windows.Forms.Label label1;
private System.Windows.Forms.Label label2;
private System.Windows.Forms.TextBox txt_Server_IP;
private System.Windows.Forms.Button cmd_Connect;
///
/// 필수 디자이너 변수입니다.
///
private System.ComponentModel.Container components = null ;
public frm_Chat_Client()
{
//
// Windows Form 디자이너 지원에 필요합니다.
//
InitializeComponent();
//
// TODO: InitializeComponent를 호출한 다음 생성자 코드를 추가합니다.
//
}
///
/// 사용 중인 모든 리소스를 정리합니다.
///
protected override void Dispose( bool disposing )
{
if ( disposing )
{
if (components != null )
{
components.Dispose();
}
}
base .Dispose( disposing );
}
#region Windows Form 디자이너에서 생성한 코드
///
/// 디자이너 지원에 필요한 메서드입니다.
/// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
///
private void InitializeComponent()
{
this .txt_Chat = new System.Windows.Forms.TextBox();
this .txt_Msg = new System.Windows.Forms.TextBox();
this .txt_Name = new System.Windows.Forms.TextBox();
this .label1 = new System.Windows.Forms.Label();
this .cmd_Connect = new System.Windows.Forms.Button();
this .label2 = new System.Windows.Forms.Label();
this .txt_Server_IP = new System.Windows.Forms.TextBox();
this .SuspendLayout();
//
// txt_Chat
//
this .txt_Chat.Location = new System.Drawing.Point(5, 36);
this .txt_Chat.Multiline = true ;
this .txt_Chat.Name = "txt_Chat";
this .txt_Chat.Size = new System.Drawing.Size(481, 262);
this .txt_Chat.TabIndex = 1;
this .txt_Chat.TabStop = false ;
this .txt_Chat.Text = "";
//
// txt_Msg
//
this .txt_Msg.Location = new System.Drawing.Point(5, 310);
this .txt_Msg.Multiline = true ;
this .txt_Msg.Name = "txt_Msg";
this .txt_Msg.Size = new System.Drawing.Size(481, 30);
this .txt_Msg.TabIndex = 4;
this .txt_Msg.Text = "";
this .txt_Msg.KeyPress += new System.Windows.Forms.KeyPressEventHandler( this .txt_Msg_KeyPress);
//
// txt_Name
//
this .txt_Name.Location = new System.Drawing.Point(60, 8);
this .txt_Name.Name = "txt_Name";
this .txt_Name.Size = new System.Drawing.Size(78, 21);
this .txt_Name.TabIndex = 1;
this .txt_Name.Text = "";
//
// label1
//
this .label1.Location = new System.Drawing.Point(8, 12);
this .label1.Name = "label1";
this .label1.Size = new System.Drawing.Size(44, 12);
this .label1.TabIndex = 4;
this .label1.Text = "대화명";
//
// cmd_Connect
//
this .cmd_Connect.Location = new System.Drawing.Point(406, 8);
this .cmd_Connect.Name = "cmd_Connect";
this .cmd_Connect.Size = new System.Drawing.Size(80, 22);
this .cmd_Connect.TabIndex = 3;
this .cmd_Connect.Text = "Login";
this .cmd_Connect.Click += new System.EventHandler( this .cmd_Connect_Click);
//
// label2
//
this .label2.Location = new System.Drawing.Point(166, 12);
this .label2.Name = "label2";
this .label2.Size = new System.Drawing.Size(56, 12);
this .label2.TabIndex = 7;
this .label2.Text = "Server IP";
//
// txt_Server_IP
//
this .txt_Server_IP.Location = new System.Drawing.Point(227, 8);
this .txt_Server_IP.MaxLength = 15;
this .txt_Server_IP.Name = "txt_Server_IP";
this .txt_Server_IP.Size = new System.Drawing.Size(123, 21);
this .txt_Server_IP.TabIndex = 2;
this .txt_Server_IP.Text = "127.0.0.1";
//
// frm_Chat_Client
//
this .AutoScaleBaseSize = new System.Drawing.Size(6, 14);
this .ClientSize = new System.Drawing.Size(492, 345);
this .Controls.Add( this .label2);
this .Controls.Add( this .txt_Server_IP);
this .Controls.Add( this .cmd_Connect);
this .Controls.Add( this .label1);
this .Controls.Add( this .txt_Name);
this .Controls.Add( this .txt_Msg);
this .Controls.Add( this .txt_Chat);
this .Location = new System.Drawing.Point(600, 0);
this .Name = "frm_Chat_Client";
this .StartPosition = System.Windows.Forms.FormStartPosition.Manual;
this .Text = "Chatting Client";
this .Closed += new System.EventHandler( this .frm_Chat_Client_Closed);
this .ResumeLayout( false );
}
#endregion
///
/// 해당 응용 프로그램의 주 진입점입니다.
///
[MTAThread]
static void Main ()
{
Application.Run( new frm_Chat_Client());
}
TcpClient tcpClient = null ;
NetworkStream ntwStream = null ;
//Chatting을 실행하는 Class 인스턴스화시킴
Chat_Class cht_Class = new Chat_Class();
private void frm_Chat_Client_Closed( object sender, System.EventArgs e)
{
if (cmd_Connect.Text == "Login")
{
return ;
}
Message_Snd("<" + txt_Name.Text + "> 님께서 접속해제 하셨습니다.", false );
cht_Class.Chat_Close();
ntwStream.Close();
tcpClient.Close();
}
private void cmd_Connect_Click( object sender, System.EventArgs e)
{
if (cmd_Connect.Text == "Login")
{
try
{
//IP Address 할당
IPAddress ipaAddress = IPAddress.Parse(txt_Server_IP.Text);
//TCP Client 선언
tcpClient = new TcpClient();
//TCP Client연결
tcpClient.Connect(ipaAddress, 5002);
//NetworkStream을 생성
ntwStream = tcpClient.GetStream();
//Stream과 txt_Chat 할당
cht_Class.Chat_Class_Setup(ntwStream, this .txt_Chat);
//Thread를 생성하고 Star시킴
Thread thd_Receive = new Thread( new ThreadStart(cht_Class.Chat_Process));
thd_Receive.Start();
Message_Snd("<" + txt_Name.Text + "> 님께서 접속 하셨습니다.", true );
cmd_Connect.Text = "Logout";
}
catch (System.Exception Err)
{
MessageBox.Show("Chatting Server 오류발생 또는 Start 되지 않았거나\n\n" + Err.Message, "Client");
}
}
else
{
Message_Snd("<" + txt_Name.Text + "> 님께서 접속해제 하셨습니다.", false );
cmd_Connect.Text = "Login";
cht_Class.Chat_Close();
ntwStream.Close();
tcpClient.Close();
}
}
private void txt_Msg_KeyPress( object sender, System.Windows.Forms.KeyPressEventArgs e)
{
if (e.KeyChar == 13)
{
if (cmd_Connect.Text == "Logout")
{
Message_Snd("<" + txt_Name.Text + "> " + txt_Msg.Text, true );
}
txt_Msg.Text = "";
e.Handled = true ;
}
}
private void Message_Snd( string lstMessage, Boolean Msg)
{
try
{
//보낼 데이터를 읽어 Default 형식의 바이트 스트림으로 변환 해서 전송
string dataToSend = lstMessage + "\r\n";
byte [] data = Encoding.Default.GetBytes(dataToSend);
ntwStream.Write(data,0,data.Length);
}
catch (System.Exception Err)
{
if (Msg== true )
{
MessageBox.Show("Chatting Server가 오류발생 또는 Start 되지 않았거나\n\n" + Err.Message, "Client");
cmd_Connect.Text = "Login";
cht_Class.Chat_Close();
ntwStream.Close();
tcpClient.Close();
}
}
}
}
public class Chat_Class
{
//한글 처리를 위해 Encod 정의
private Encoding ecd_Encode = Encoding.GetEncoding("KS_C_5601-1987");
//글자를 Display할 Object
private System.Windows.Forms.TextBox txt_Chat;
private NetworkStream netStream;
private StreamReader strReader;
public void Chat_Class_Setup(NetworkStream netStream, System.Windows.Forms.TextBox txt_Chat)
{
//TextBox를 할당함.
this .txt_Chat = txt_Chat;
//Network Stream을 할당
this .netStream = netStream;
//Stream Reader을 생성
this .strReader = new StreamReader(netStream, ecd_Encode);
}
public void Chat_Process()
{
while ( true )
{
try
{
//문자열을 받음
string lstMessage = strReader.ReadLine();
if (lstMessage!= null && lstMessage!="" )
{
this .txt_Chat.AppendText(lstMessage+"\r\n");
}
}
catch (System.Exception)
{
break ;
}
}
}
public void Chat_Close()
{
netStream.Close();
strReader.Close();
}
}
}