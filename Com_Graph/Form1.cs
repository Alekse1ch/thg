using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.IO;

namespace Com_Graph
{
    public partial class Form1 : Form
    {
        string port_name;
        string baudrate_str;
        string databits_str;
        string stopbits_str;
        string parity_str;
        string hanshake_str;
        string portstat_str = "closed";
        string pass_for_receiving;

        string[] str_to_file = new string[3];
        string[] str_file = new string[1];

        int error_counter = 0;
        int error_counter2 = 0;
        int baudrate;
        int _3bytes = 4;
        int counter_for_graph2;
        int priv_2_byte = 100;
        int sample_counter = 0;
        int radio_rate = 1024;
        int radio_rate2;
        int red_point;
        int stop_byte_counter;
        int stop_byte_counter2;
        int list_counter = 0;
        int counter_for_mark;
        int mark_index1;
        int mark_index2;
        int hscroll_value;
        int priv_hscroll_value;
        int mouseup_value;
        int zoom_lenth;

        int[] _byte1 = new int[3];

        Dictionary<int, byte> Number_of_frame = new Dictionary<int, byte>();
        Dictionary<int, byte> Value_of_frame = new Dictionary<int, byte>();
        Dictionary<int, string> Data_to_file = new Dictionary<int, string>();

        byte[] _byte = new byte[3];
        byte[] byte_to_file = new byte[3];

        float _double;
        float priv_double = 0.0f;
        float y_float;
        float priv_y_float = 0.0f;
        float _double2;
        float priv_double2 = 0.0f;
        float y_float2;
        float priv_y_float2 = 0.0f;
        float first_line_value;
        float second_line_value;
        float priv_zoom_float = 1;
        float graph_rate;
        float zoom_float = 1;

        bool prog_start = false;
        bool start_graph = false;
        bool _continue;
        bool clear_port_buffer = true;
        bool _graph = false;
        bool _graph_2 = false;
        bool mode1 = true;
        bool scale_scroll = false;
        bool res_parse_baudrate;
        bool res_parse_radio_rate;
        bool mark_event = false;
        bool zoom_co;
        bool mouseup = false;
        static bool not_connected = true;
        private static bool mouseDown;
        bool save_data = false;
        bool resume_save = false;
        bool zooom_rect = false;
        bool set_point = true;
        bool p9_scroll;
        bool m_scroll = false;
        bool stop_saving = false;
        bool annulment;
        bool complete_the_implementation_graph_thread;
        bool complete_the_implementation_graph_thread_2;
        bool complete_the_implementation_saving;
        bool graph_thread_ower = false;
        bool graph_thread_2_ower = false;
        bool saving_ower = false;

        Point orig;
        Point p9_curs_point;
        static private Point lastLocation;
        Point lkm;

        Rectangle rect = new Rectangle();

        Pen r_pen = new Pen(Color.Red, 0.05f);

        Bitmap bitmap;
        Bitmap bitmap2;

        public Graphics g;
        public Graphics f;
        public Graphics g2;
        public Graphics f2;

        SaveFileDialog saveFileDialog1 = new SaveFileDialog();

        SerialPort _serialPort = new SerialPort();
        Thread[] readThread = new Thread[4];


        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;

        private bool m_aeroEnabled;

        private const int CS_DROPSHADOW = 0x00020000;
        private const int WM_NCPAINT = 0x0085;
        private const int WM_ACTIVATEAPP = 0x001C;

        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]

        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
            );

        public struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }
        protected override CreateParams CreateParams
        {
            get
            {
                m_aeroEnabled = CheckAeroEnabled();
                CreateParams cp = base.CreateParams;
                if (!m_aeroEnabled)
                    cp.ClassStyle |= CS_DROPSHADOW; return cp;
            }
        }
        private bool CheckAeroEnabled()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0; DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1) ? true : false;
            }
            return false;
        }
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NCPAINT:
                    if (m_aeroEnabled)
                    {
                        var v = 2;
                        DwmSetWindowAttribute(this.Handle, 2, ref v, 4);
                        MARGINS margins = new MARGINS()
                        {
                            bottomHeight = 1,
                            leftWidth = 0,
                            rightWidth = 0,
                            topHeight = 0
                        }; DwmExtendFrameIntoClientArea(this.Handle, ref margins);
                    }
                    break;
                default: break;
            }
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT) m.Result = (IntPtr)HTCAPTION;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);

                this.Update();
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            lastLocation = e.Location;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            bitmap = new Bitmap(panel7.Width, panel7.Height);
            bitmap2 = new Bitmap(panel9.Width, panel9.Height);
            f = Graphics.FromImage(bitmap);
            g = panel7.CreateGraphics();
            f2 = Graphics.FromImage(bitmap2);
            g2 = panel9.CreateGraphics();
            _serialPort.PortName = _serialPort.PortName;
            _serialPort.BaudRate = 115200;
            _serialPort.Parity = _serialPort.Parity;
            _serialPort.DataBits = _serialPort.DataBits;
            _serialPort.StopBits = _serialPort.StopBits;
            _serialPort.Handshake = _serialPort.Handshake;
            PortStatus_Funct();
        }

        private void Minimaze_button_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void Exit_button_Click(object sender, EventArgs e)
        {
            bool _annulment = false;
            if (save_data == true)
            {
                complete_the_implementation_saving = true;
                save_data = false;
                if (saving_ower == true)
                {
                    _annulment = false;
                    saving_ower = false;
                }
                else
                {
                    var result = MessageBox.Show("Data is stored, whether you want to stop saving", "Error",
                                                 MessageBoxButtons.YesNo,
                                                 MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        stop_saving = true;
                        Thread.Sleep(50);
                        _annulment = true;
                        saving_ower = true;
                    }
                    else
                    {
                        _annulment = false;
                        complete_the_implementation_saving = false;
                        saving_ower = false;
                    }
                }
            }
            if (_annulment == false)
            {

                if (not_connected == false)
                {
                    _continue = false;
                    readThread[0].Join();
                    _serialPort.Close();

                }
                if (prog_start == true)
                {
                    if (start_graph == true)
                    {
                        complete_the_implementation_graph_thread = true;
                        complete_the_implementation_graph_thread_2 = true;
                        _graph = false;
                        _graph_2 = false;
                        //Thread.Sleep(25);
                        if (graph_thread_ower == true && graph_thread_2_ower == true)
                        {

                        }
                        else
                        {
                            /*var result = MessageBox.Show("Error", "Error",
                                                     MessageBoxButtons.OK,
                                                     MessageBoxIcon.Question);

                            if (result == DialogResult.OK)
                            {

                            }*/
                        }
                    }
                }
                this.Close();
            }
        }

        private void Connect_button_Click(object sender, EventArgs e)
        {
            prog_start = true;
            graph_rate = ((float)(754 / 35) / radio_rate);
            if (not_connected == true)
            {
                not_connected = false;
                panel6.Visible = true;
                readThread[0] = new Thread(Read_Bytes_from_COM);
                _serialPort.ReadTimeout = 500;
                _serialPort.WriteTimeout = 500;
                portstat_str = "open";
                PortStatus_Funct();

                try
                {
                    _serialPort.Open();
                    _continue = true;
                    clear_port_buffer = true;
                    readThread[0].Start();
                }
                catch (Exception a)
                {
                    var result = MessageBox.Show(a.Message, "Error",
                                                 MessageBoxButtons.OK,
                                                 MessageBoxIcon.Question);

                    if (result == DialogResult.OK)
                    {
                        if (not_connected == false)
                        {
                            not_connected = true;
                            panel6.Visible = false;
                            _continue = false;
                            _serialPort.Close();
                            portstat_str = "closed";
                            PortStatus_Funct();
                        }
                    }
                }
            }
        }

        private void Disconnect_button_Click(object sender, EventArgs e)
        {
            if (not_connected == false)
            {
                not_connected = true;
                panel6.Visible = false;
                _continue = false;
                _serialPort.Close();
                portstat_str = "closed";
                PortStatus_Funct();
            }
        }

        private void Start_graph_button_Click(object sender, EventArgs e)
        {
            if (prog_start == true)
            {
                if (start_graph == false)
                {
                    start_graph = true;
                    Stop_button.Visible = true;
                    Start_graph_button.Visible = false;
                }
            }
        }

        private void Pause_graph_button_Click(object sender, EventArgs e)
        {
            if (start_graph == true)
            {
                stop_byte_counter = sample_counter;
                stop_byte_counter2 = sample_counter;
                mode1 = false;
                Pause_graph_button.Visible = false;
                Resume_button.Visible = true;
            }
        }

        private void Resume_button_Click(object sender, EventArgs e)
        {
            mode1 = true;
            Pause_graph_button.Visible = true;
            Resume_button.Visible = false;
        }

        private void Stop_button_Click(object sender, EventArgs e)
        {
            if (start_graph == true)
            {
                Start_graph_button.Visible = true;
                Stop_button.Visible = false;
                start_graph = false;
            }
        }

        private void OFT_button_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Text file|*.csv";
            saveFileDialog1.Title = "Save text file";
            if(Data_to_file.Count > 0)
            {
                saveFileDialog1.ShowDialog();

                if (saveFileDialog1.FileName != "")
                {
                    pass_for_receiving = saveFileDialog1.FileName;
                    readThread[2] = new Thread(Save_thread);
                    save_data = true;
                    readThread[2].Start();
                }
            }
            else
            {
                var result = MessageBox.Show("There is nothing to save", "Error",
                                             MessageBoxButtons.OK,
                                             MessageBoxIcon.Question);

                if (result == DialogResult.OK)
                {
                }
            }
        }

        private void Parity_button_Click(object sender, EventArgs e)
        {
            if (not_connected == true)
            {
                if (Curs_panel.Location == new Point(190, 237) && Curs_panel.Visible == true)
                {
                    Hide_port_baudrate_Funct();
                }
                else
                {
                    StripMenu_Funct();
                    Parity_panel.Visible = true;
                    Curs_panel.Location = new Point(190, 237);
                }
            }
        }

        private void DataBits_button_Click(object sender, EventArgs e)
        {
            if (not_connected == true)
            {
                if (Curs_panel.Location == new Point(190, 288) && Curs_panel.Visible == true)
                {
                    Hide_port_baudrate_Funct();
                }
                else
                {
                    StripMenu_Funct();
                    DataBits_panel.Visible = true;
                    Curs_panel.Location = new Point(190, 288);
                }
            }
        }

        private void StopBits_button_Click(object sender, EventArgs e)
        {
            if (not_connected == true)
            {
                if (Curs_panel.Location == new Point(190, 339) && Curs_panel.Visible == true)
                {
                    Hide_port_baudrate_Funct();
                }
                else
                {
                    StripMenu_Funct();
                    StopBits_panel.Visible = true;
                    Curs_panel.Location = new Point(190, 339);
                }
            }
        }

        private void Handshake_button_Click(object sender, EventArgs e)
        {
            if (not_connected == true)
            {
                if (Curs_panel.Location == new Point(190, 390) && Curs_panel.Visible == true)
                {
                    Hide_port_baudrate_Funct();
                }
                else
                {
                    StripMenu_Funct();
                    Com_rate_button.Visible = false;
                    Handshake_panel.Visible = true;
                    Curs_panel.Location = new Point(190, 390);
                }
            }
        }

        private void Port_button_Click(object sender, EventArgs e)
        {
            if (not_connected == true)
            {
                if (Curs_panel.Location == new Point(190, 492) && Curs_panel.Visible == true)
                {
                    Hide_port_baudrate_Funct();
                }
                else
                {
                    StripMenu_Funct();
                    Port_panel.Visible = true;
                    Curs_panel.Location = new Point(190, 492);
                }
            }
        }

        private void Baudrate_button_Click(object sender, EventArgs e)
        {
            if (not_connected == true)
            {
                if (Curs_panel.Location == new Point(190, 441) && Curs_panel.Visible == true)
                {
                    Hide_port_baudrate_Funct();
                }
                else
                {
                    StripMenu_Funct();
                    Baudrate_panel.Visible = true;
                    Curs_panel.Location = new Point(190, 441);
                }
            }
        }

        private void Com_rate_button_Click(object sender, EventArgs e)
        {
            if (not_connected == true)
            {
                if (Curs_panel.Location == new Point(190, 543) && Curs_panel.Visible == true)
                {
                    Hide_port_baudrate_Funct();
                }
                else
                {
                    StripMenu_Funct();
                    Com_rate_panel.Visible = true;
                    Curs_panel.Location = new Point(190, 543);
                }
            }
        }

        private void Even_p_button_Click(object sender, EventArgs e)
        {
            _serialPort.Parity = Parity.Even;
            PortStatus_Funct();
            Parity_panel.Visible = false;
            Curs_panel.Visible = false;
        }

        private void Mark_p_button_Click(object sender, EventArgs e)
        {
            _serialPort.Parity = Parity.Mark;
            PortStatus_Funct();
            Parity_panel.Visible = false;
            Curs_panel.Visible = false;
        }

        private void None_p_button_Click(object sender, EventArgs e)
        {
            _serialPort.Parity = Parity.None;
            PortStatus_Funct();
            Parity_panel.Visible = false;
            Curs_panel.Visible = false;
        }

        private void Odd_p_button_Click(object sender, EventArgs e)
        {
            _serialPort.Parity = Parity.Odd;
            PortStatus_Funct();
            Parity_panel.Visible = false;
            Curs_panel.Visible = false;
        }

        private void Space_p_button_Click(object sender, EventArgs e)
        {
            _serialPort.Parity = Parity.Space;
            PortStatus_Funct();
            Parity_panel.Visible = false;
            Curs_panel.Visible = false;
        }

        private void five_d_button_Click(object sender, EventArgs e)
        {
            _serialPort.DataBits = 5;
            PortStatus_Funct();
            DataBits_panel.Visible = false;
            Curs_panel.Visible = false;
        }

        private void six_d_button_Click(object sender, EventArgs e)
        {
            _serialPort.DataBits = 6;
            PortStatus_Funct();
            DataBits_panel.Visible = false;
            Curs_panel.Visible = false;
        }

        private void seven_d_button_Click(object sender, EventArgs e)
        {
            _serialPort.DataBits = 7;
            PortStatus_Funct();
            DataBits_panel.Visible = false;
            Curs_panel.Visible = false;
        }

        private void eight_d_button_Click(object sender, EventArgs e)
        {
            _serialPort.DataBits = 8;
            PortStatus_Funct();
            DataBits_panel.Visible = false;
            Curs_panel.Visible = false;
        }

        private void None_s_button_Click(object sender, EventArgs e)
        {
            _serialPort.StopBits = StopBits.None;
            PortStatus_Funct();
            StopBits_panel.Visible = false;
            Curs_panel.Visible = false;
        }

        private void One_s_button_Click(object sender, EventArgs e)
        {
            _serialPort.StopBits = StopBits.One;
            PortStatus_Funct();
            StopBits_panel.Visible = false;
            Curs_panel.Visible = false;
        }

        private void OneFive_s_button_Click(object sender, EventArgs e)
        {
            _serialPort.StopBits = StopBits.OnePointFive;
            PortStatus_Funct();
            StopBits_panel.Visible = false;
            Curs_panel.Visible = false;
        }

        private void Two_s_button_Click(object sender, EventArgs e)
        {
            _serialPort.StopBits = StopBits.Two;
            PortStatus_Funct();
            StopBits_panel.Visible = false;
            Curs_panel.Visible = false;
        }

        private void Request_n_button_Click(object sender, EventArgs e)
        {
            _serialPort.Handshake = Handshake.None;
            PortStatus_Funct();
            Com_rate_button.Visible = true;
            Handshake_panel.Visible = false;
            Curs_panel.Visible = false;
        }

        private void Request_1_button_Click(object sender, EventArgs e)
        {
            _serialPort.Handshake = Handshake.RequestToSend;
            PortStatus_Funct();
            Com_rate_button.Visible = true;
            Handshake_panel.Visible = false;
            Curs_panel.Visible = false;
        }

        private void Request_2_button_Click(object sender, EventArgs e)
        {
            _serialPort.Handshake = Handshake.RequestToSendXOnXOff;
            PortStatus_Funct();
            Com_rate_button.Visible = true;
            Handshake_panel.Visible = false;
            Curs_panel.Visible = false;
        }

        private void Request_3_button_Click(object sender, EventArgs e)
        {
            _serialPort.Handshake = Handshake.XOnXOff;
            PortStatus_Funct();
            Com_rate_button.Visible = true;
            Handshake_panel.Visible = false;
            Curs_panel.Visible = false;
        }

        private void textBox5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                port_name = textBox5.Text;
                try
                {
                    _serialPort.PortName = port_name;
                }
                catch (Exception c)
                {
                    var result = MessageBox.Show(c.Message, "Error",
                                                 MessageBoxButtons.OK,
                                                 MessageBoxIcon.Question);

                    if (result == DialogResult.OK)
                    {
                        textBox5.Text = "COM1";
                        _serialPort.PortName = "COM1";
                    }
                }
                PortStatus_Funct();
                Port_panel.Visible = false;
                Curs_panel.Visible = false;
            }
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                res_parse_baudrate = Int32.TryParse(textBox2.Text, out baudrate);

                if (res_parse_baudrate == true)
                {
                    try
                    {
                        _serialPort.BaudRate = baudrate;
                    }
                    catch(Exception d)
                    {
                        var result = MessageBox.Show(d.Message, "Error",
                                                     MessageBoxButtons.OK,
                                                     MessageBoxIcon.Question);

                        if (result == DialogResult.OK)
                        {
                            textBox2.Text = 115200.ToString();
                            _serialPort.BaudRate = 115200;
                        }
                    }
                }
                else
                {
                    var result = MessageBox.Show("Wrong BaudRate", "Error",
                                                 MessageBoxButtons.OK,
                                                 MessageBoxIcon.Question);

                    if (result == DialogResult.OK)
                    {
                        textBox2.Text = 115200.ToString();
                        _serialPort.BaudRate = 115200;
                    }
                }

                PortStatus_Funct();
                Baudrate_panel.Visible = false;
                Curs_panel.Visible = false;
            }
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                res_parse_radio_rate = Int32.TryParse(textBox3.Text, out radio_rate2);

                if (res_parse_radio_rate == true && radio_rate2 > 0)
                {
                    radio_rate = radio_rate2;
                }
                else
                {
                    var result = MessageBox.Show("Wrong Radio rate", "Error",
                                                 MessageBoxButtons.OK,
                                                 MessageBoxIcon.Question);

                    if (result == DialogResult.OK)
                    {
                        textBox3.Text = 1024.ToString();
                        radio_rate = 1024;
                    }
                }

                PortStatus_Funct();
                Com_rate_panel.Visible = false;
                Curs_panel.Visible = false;
            }
        }

        private void textBox4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                zoom_lenth = textBox4.Text.Length;
                if (textBox4.Text.EndsWith("%"))
                {
                    zoom_co = float.TryParse(textBox4.Text.Substring(0, zoom_lenth - 1), out zoom_float);
                    if (zoom_co == true)
                    {
                        zoom_float = zoom_float / 100;
                        Scale_graph_function();
                        if (zoom_float >= 1)
                        {
                            if ((int)(zoom_float / 0.1f) + 190 < vScrollBar1.Maximum)
                            {
                                vScrollBar1.Value = (int)(zoom_float / 0.1f) + 190;
                            }
                            else
                            {
                                vScrollBar1.Value = vScrollBar1.Maximum;
                            }
                        }
                        else
                        {
                            vScrollBar1.Value = (int)(zoom_float / 0.005f);
                        }
                    }
                    else
                    {
                        var result = MessageBox.Show("Wrong Scale", "Error",
                                                    MessageBoxButtons.OK,
                                                    MessageBoxIcon.Question);

                        if (result == DialogResult.OK)
                        {
                            zoom_float = 1;
                            textBox4.Text = "100%";
                            vScrollBar1.Value = 200;
                            Scale_graph_function();
                        }
                    }
                }
                else
                {
                    zoom_co = float.TryParse(textBox4.Text, out zoom_float);
                    if (zoom_co == true)
                    {
                        zoom_float = zoom_float / 100;
                        Scale_graph_function();
                        if (zoom_float >= 1)
                        {
                            if ((int)(zoom_float / 0.1f) + 190 < vScrollBar1.Maximum)
                            {
                                vScrollBar1.Value = (int)(zoom_float / 0.1f) + 190;
                            }
                            else
                            {
                                vScrollBar1.Value = vScrollBar1.Maximum;
                            }
                        }
                        else
                        {
                            vScrollBar1.Value = (int)(zoom_float / 0.005f);
                        }
                    }
                    else
                    {
                        var result = MessageBox.Show("Wrong Scale", "Error",
                                                       MessageBoxButtons.OK,
                                                       MessageBoxIcon.Question);

                        if (result == DialogResult.OK)
                        {
                            zoom_float = 1;
                            textBox4.Text = "100%";
                            vScrollBar1.Value = 200;
                            Scale_graph_function();
                        }
                    }
                }
            }
        }

        public void StripMenu_Funct()
        {
            if (Port_panel.Visible == true)
            {
                Port_panel.Visible = false;
            }
            if (Baudrate_panel.Visible == true)
            {
                Baudrate_panel.Visible = false;
            }
            if (Handshake_panel.Visible == true)
            {
                Handshake_panel.Visible = false;
            }
            if (StopBits_panel.Visible == true)
            {
                StopBits_panel.Visible = false;
            }
            if (DataBits_panel.Visible == true)
            {
                DataBits_panel.Visible = false;
            }
            if (Com_rate_panel.Visible == true)
            {
                Com_rate_panel.Visible = false;
            }
            Curs_panel.Visible = true;
        }

        public void PortStatus_Funct()
        {
            port_name = _serialPort.PortName;
            baudrate_str = _serialPort.BaudRate.ToString();
            parity_str = _serialPort.Parity.ToString();
            databits_str = _serialPort.DataBits.ToString();
            stopbits_str = _serialPort.StopBits.ToString();
            hanshake_str = _serialPort.Handshake.ToString();
            label2.Text = "Serial port " + port_name + "(" + baudrate_str + ", " + databits_str + ", " + parity_str + ", " + stopbits_str + ", " + hanshake_str + ") is " + portstat_str;
            label10.Text = "COM port rate: " + radio_rate.ToString();
        }

        public void Hide_port_baudrate_Funct()
        {
            if (Port_panel.Visible == true)
            {
                port_name = textBox5.Text;
                try
                {
                    _serialPort.PortName = port_name;
                }
                catch (Exception a)
                {
                    var result = MessageBox.Show(a.Message, "Error",
                                                 MessageBoxButtons.OK,
                                                 MessageBoxIcon.Question);

                    if (result == DialogResult.OK)
                    {
                        textBox5.Text = "COM1";
                        _serialPort.PortName = "COM1";
                    }
                }
                PortStatus_Funct();
                Port_panel.Visible = false;
                Curs_panel.Visible = false;
            }
            if (Baudrate_panel.Visible == true)
            {
                res_parse_baudrate = Int32.TryParse(textBox2.Text, out baudrate);

                if (res_parse_baudrate == true)
                {
                    _serialPort.BaudRate = baudrate;
                }
                else
                {
                    var result = MessageBox.Show("Wrong Baudrate", "Error",
                                                       MessageBoxButtons.OK,
                                                       MessageBoxIcon.Question);

                    if (result == DialogResult.OK)
                    {
                        textBox2.Text = 115200.ToString();
                        _serialPort.BaudRate = 115200;
                    }
                }

                PortStatus_Funct();
                Baudrate_panel.Visible = false;
                Curs_panel.Visible = false;
            }
            if (Com_rate_panel.Visible == true)
            {
                res_parse_radio_rate = Int32.TryParse(textBox3.Text, out radio_rate2);

                if (res_parse_radio_rate == true)
                {
                    radio_rate = radio_rate2;
                }
                else
                {
                    var result = MessageBox.Show("Wrong COM radio rate", "Error",
                                                 MessageBoxButtons.OK,
                                                 MessageBoxIcon.Question);

                    if (result == DialogResult.OK)
                    {
                        textBox3.Text = radio_rate.ToString();
                    }
                }

                PortStatus_Funct();
                Com_rate_panel.Visible = false;
                Curs_panel.Visible = false;
            }
            if (Handshake_panel.Visible == true)
            {
                Handshake_panel.Visible = false;
                Com_rate_button.Visible = true;
            }
            if (StopBits_panel.Visible == true)
            {
                StopBits_panel.Visible = false;
            }
            if (DataBits_panel.Visible == true)
            {
                DataBits_panel.Visible = false;
            }
            if (Parity_panel.Visible == true)
            {
                Parity_panel.Visible = false;
            }
            Curs_panel.Visible = false;
        }

        void Start_graph_function()
        {
            readThread[1] = new Thread(Graph_Thread);
            _graph = true;
            readThread[1].Start();
        }
        void Start_graph_2_function()
        {
            readThread[3] = new Thread(Graph_Thread_2);
            _graph_2 = true;
            readThread[3].Start();
        }

        void Scale_graph_function()
        {
            if (start_graph == true)
            {
                if (_graph == false)
                {
                    _graph = true;
                    Start_graph_function();
                }
                if (_graph_2 == false)
                {
                    _graph_2 = true;
                    Start_graph_2_function();
                }
            }
        }
        public void Draw_Graph(int counter)
        {
            Pen mpen = new Pen(Color.FromArgb(255, 21, 32, 76), graph_rate);

            f.Clear(Color.White);

            _double = 799.0f;
            priv_double = 799.0f;

            for (int i = counter; i > 0;)
            {
                _double = _double - (graph_rate * zoom_float);
                if (_double >= 45)
                {
                    if (red_point == 1 || red_point == 2)
                    {
                        if (mark_index1 == i)
                        {
                            f.DrawLine(r_pen, _double, 25, _double, 333);
                            f.DrawString(first_line_value.ToString(), new Font("Serif", 9), new SolidBrush(Color.Red), _double - 25, 341);
                        }
                        if (red_point == 2)
                        {
                            if (mark_index2 == i)
                            {
                                f.DrawLine(r_pen, _double, 25, _double, 333);
                                f.DrawString(second_line_value.ToString(), new Font("Serif", 9), new SolidBrush(Color.Red), _double - 25, 341);
                                f.DrawString(Math.Abs(first_line_value - second_line_value).ToString(), new Font("Serif", 9), new SolidBrush(Color.Red), _double - 23, 5);
                            }
                        }
                    }

                    if (i > 0)
                    {
                        if (Value_of_frame[i - 1] < 128)
                        {
                            y_float = ((Value_of_frame[i - 1] - 102) * 1.5f) + 320;
                        }
                        else if (Value_of_frame[i - 1] >= 128)
                        {
                            y_float = ((Value_of_frame[i - 1] - 255 - 102) * 1.5f) + 320;
                        }
                        f.DrawLine(mpen, priv_double, priv_y_float, priv_double, y_float);
                        f.DrawLine(mpen, priv_double, y_float, _double, y_float);
                        priv_y_float = y_float;
                    }
                }
                else
                {
                    _double = 45;

                    if (i > 0)
                    {
                        if (Value_of_frame[i - 1] < 128)
                        {
                            y_float = ((Value_of_frame[i - 1] - 102) * 1.5f) + 320;
                        }
                        else if (Value_of_frame[i - 1] >= 128)
                        {
                            y_float = ((Value_of_frame[i - 1] - 255 - 102) * 1.5f) + 320;
                        }
                        f.DrawLine(mpen, priv_double, priv_y_float, priv_double, y_float);
                        f.DrawLine(mpen, priv_double, y_float, _double, y_float);
                    }
                    i = 0;
                }
                priv_double = _double;
                i--;
            }
            ///////////////////////////////////// Function for drawing x_axis grid and time under line
            float time_f = counter * radio_rate;
            int zoom = (int)zoom_float;
            int m_time = 2;
            if (zoom_float < 2)
            {
                m_time = 1;
            }
            else
            {
                for (m_time = 2; m_time < zoom / 2;)
                {
                    m_time = m_time * 2;
                }
            }

            float time_b = time_f % (1 / m_time);
            float start_time = time_f - time_b;
            float x_time = 35 / 754 * zoom_float * m_time;
            float x_start = 799 - (35 / 754 * zoom_float * time_b);

            for (float i = x_start; i > 45;)
            {
                if (start_time > 0)
                {
                    f.DrawLine(mpen, i, 5, i, 335);
                    f.DrawString(start_time.ToString(), new Font("Serif", 9), new SolidBrush(Color.Red), i - 20, 350);
                    i = i - x_time;
                    start_time = start_time - (1 / m_time);
                }
                else
                {
                    i = 44;
                }
            }
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            f.DrawLine(mpen, 45, 335, 799, 335);
            f.DrawLine(mpen, 45, 5, 45, 335);
            g.DrawImage(bitmap, new PointF(0.0f, 0.0f));
        }

        void Read_Bytes_from_COM()
        {
            while (_continue)
            {
                if (clear_port_buffer == true)
                {
                    _serialPort.DiscardInBuffer();
                    _serialPort.DiscardOutBuffer();
                    clear_port_buffer = false;
                }
                try
                {
                    int bytes = _serialPort.BytesToRead;
                    if (bytes > 0)
                    {
                        byte[] buffer = new byte[bytes];
                        _serialPort.Read(buffer, 0, bytes);
                        for (int i = 0; i <= bytes - 1;)
                        {
                            if (buffer[i] == 255)
                            {
                                if (_3bytes == 3)
                                {
                                    Number_of_frame.Add(sample_counter, byte_to_file[1]);
                                    Value_of_frame.Add(sample_counter, byte_to_file[2]);
                                    int res_byte = 0;
                                    if (byte_to_file[2] < 128)
                                    {
                                        res_byte = byte_to_file[2] - 102;
                                    }
                                    else if (byte_to_file[2] >= 128)
                                    {
                                        res_byte = byte_to_file[2] - 255 - 102;
                                    }
                                    string str_e = "255, " + /*byte_to_file[1]*/ sample_counter.ToString() + ", " + res_byte.ToString();//byte_to_file[2];
                                    if (save_data == false && resume_save == false)
                                    {
                                        if (sample_counter > Data_to_file.Count)
                                        {
                                            resume_save = true;
                                        }
                                        else
                                        {
                                            Data_to_file.Add(sample_counter, str_e);
                                        }
                                    }
                                    sample_counter++;
                                    if (start_graph == true)
                                    {
                                        if (mode1 == true)
                                        {
                                            if (_graph == false)
                                            {
                                                _graph = true;
                                                Start_graph_function();
                                            }
                                            if (_graph_2 == false)
                                            {
                                                _graph_2 = true;
                                                Start_graph_2_function();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (_3bytes == 2)
                                    {
                                        error_counter++;
                                        Number_of_frame.Add(sample_counter, byte_to_file[1]);
                                        Value_of_frame.Add(sample_counter, byte_to_file[2]);
                                        sample_counter++;
                                    }
                                }
                                _3bytes = 1;
                                byte_to_file[0] = buffer[i];
                            }
                            else
                            {
                                if (_3bytes == 1)
                                {
                                    if (priv_2_byte == 100)
                                    {
                                        _3bytes = 2;
                                        byte_to_file[1] = buffer[i];
                                        priv_2_byte = byte_to_file[1];
                                    }
                                    else
                                    {
                                        _3bytes = 2;
                                        byte_to_file[1] = buffer[i];

                                        if (byte_to_file[1] == 0)
                                        {
                                            if (priv_2_byte != 99)
                                            {
                                                int dif = 99 - priv_2_byte;
                                                for (int ai = dif; ai > 0;)
                                                {
                                                    byte _bytee = (byte)(100 - ai);
                                                    Number_of_frame.Add(sample_counter, _bytee);
                                                    Value_of_frame.Add(sample_counter, byte_to_file[2]);
                                                    sample_counter++;
                                                    error_counter2++;
                                                    ai--;
                                                }
                                            }
                                        }
                                        else if (priv_2_byte != byte_to_file[1] - 1)
                                        {
                                            if (byte_to_file[1] - 1 > priv_2_byte)
                                            {
                                                int dif3 = byte_to_file[1] - 1 - priv_2_byte;
                                                for (int ai = dif3; ai > 0;)
                                                {
                                                    byte _byteee = (byte)(byte_to_file[1] - ai);
                                                    Number_of_frame.Add(sample_counter, _byteee);
                                                    Value_of_frame.Add(sample_counter, byte_to_file[2]);
                                                    sample_counter++;
                                                    error_counter2++;
                                                    ai--;
                                                }
                                            }
                                            else
                                            {
                                                int dif2 = 99 - priv_2_byte;
                                                for (int ai = dif2; ai > 0;)
                                                {
                                                    byte _byteeee = (byte)(100 - ai);
                                                    Number_of_frame.Add(sample_counter, _byteeee);
                                                    Value_of_frame.Add(sample_counter, byte_to_file[2]);
                                                    sample_counter++;
                                                    error_counter2++;
                                                    ai--;
                                                }
                                                for (int bi = 0; bi < byte_to_file[1];)
                                                {
                                                    byte _byteeeee = (byte)(bi);
                                                    Number_of_frame.Add(sample_counter, _byteeeee);
                                                    Value_of_frame.Add(sample_counter, byte_to_file[2]);
                                                    sample_counter++;
                                                    error_counter2++;
                                                    bi++;
                                                }
                                            }
                                        }
                                        priv_2_byte = byte_to_file[1];
                                    }
                                }
                                else if (_3bytes == 2)
                                {
                                    byte_to_file[2] = buffer[i];
                                    _3bytes = 3;
                                }
                            }
                            i++;
                        }
                    }
                }
                catch (System.IO.IOException e)
                {
                    var result = MessageBox.Show(e.Message, "Error",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Question);
                    if (result == DialogResult.OK)
                    {
                        _continue = false;
                        _serialPort.Close();
                    }
                }
                catch (TimeoutException) { }

                //Thread.Sleep(50);
            }
        }

        void Graph_Thread()
        {
            if(_graph == true)
            {
                try
                {
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        label7.Text = error_counter.ToString();
                        label8.Text = error_counter2.ToString();
                    });
                }
                catch (System.ComponentModel.InvalidAsynchronousStateException) { }
                catch (System.ObjectDisposedException) { }
                catch (System.InvalidOperationException) { }

                if (mode1 == true)
                {
                    if (sample_counter >= ((int)(754 / (graph_rate * zoom_float))))
                    {
                        scale_scroll = true;
                        try
                        {
                            this.Invoke((MethodInvoker)delegate () { hScrollBar1.Maximum = sample_counter; hScrollBar1.Value = hScrollBar1.Maximum; });
                        }
                        catch (System.ComponentModel.InvalidAsynchronousStateException) { }
                        catch (System.ObjectDisposedException) { }
                        catch (System.InvalidOperationException) { }
                    }
                    else
                    {
                        scale_scroll = false;
                    }

                    if (scale_scroll == true)
                    {
                        try
                        {
                            this.Invoke((MethodInvoker)delegate ()
                            {
                                hScrollBar1.Visible = true;
                                if (mouseup == true)
                                {
                                    hScrollBar1.Value = mouseup_value;
                                    mouseup = false;
                                }
                                hscroll_value = hScrollBar1.Value;
                            });
                        }
                        catch (System.ComponentModel.InvalidAsynchronousStateException) { }
                        catch (System.ObjectDisposedException) { }
                        catch (System.InvalidOperationException) { }
                    }
                    else
                    {
                        try
                        {
                            this.Invoke((MethodInvoker)delegate () { hScrollBar1.Visible = false; });
                        }
                        catch (System.ComponentModel.InvalidAsynchronousStateException) { }
                        catch (System.ObjectDisposedException) { }
                        catch (System.InvalidOperationException) { }
                    }

                    list_counter = sample_counter;
                    counter_for_mark = sample_counter;
                    Draw_Graph(sample_counter);
                }
                else
                {
                    if (stop_byte_counter >= ((int)(754 / (graph_rate * zoom_float))))
                    {
                        scale_scroll = true;
                        try
                        {
                            this.Invoke((MethodInvoker)delegate () { hScrollBar1.Maximum = stop_byte_counter; });
                        }
                        catch (System.ComponentModel.InvalidAsynchronousStateException) { }
                        catch (System.ObjectDisposedException) { }
                        catch (System.InvalidOperationException) { }
                    }
                    else
                    {
                        scale_scroll = false;
                        list_counter = stop_byte_counter;
                    }

                    if (scale_scroll == true)
                    {
                        try
                        {
                            this.Invoke((MethodInvoker)delegate () {
                                hScrollBar1.Visible = true;
                                if (mouseup == true)
                                {
                                    hScrollBar1.Value = mouseup_value;
                                    mouseup = false;
                                }
                                hscroll_value = hScrollBar1.Value;
                            });
                        }
                        catch (System.ComponentModel.InvalidAsynchronousStateException) { }
                        catch (System.ObjectDisposedException) { }
                        catch (System.InvalidOperationException) { }
                        list_counter = hscroll_value;
                    }
                    else
                    {
                        try
                        {
                            this.Invoke((MethodInvoker)delegate () { hScrollBar1.Visible = false; });
                        }
                        catch (System.ComponentModel.InvalidAsynchronousStateException) { }
                        catch (System.ObjectDisposedException) { }
                        catch (System.InvalidOperationException) { }
                    }

                    if (hscroll_value != priv_hscroll_value || zoom_float != priv_zoom_float || mark_event == true)
                    {
                        if (list_counter > stop_byte_counter)
                        {
                            list_counter = stop_byte_counter;
                        }
                        else
                        {
                            if (((int)(754 / (graph_rate * zoom_float))) > list_counter)
                            {
                                if (((int)(754 / (graph_rate * zoom_float))) < stop_byte_counter)
                                {
                                    list_counter = (int)(754 / (graph_rate * zoom_float));
                                }
                            }
                        }
                        counter_for_mark = list_counter;
                        Draw_Graph(list_counter);
                        priv_hscroll_value = hscroll_value;
                        priv_zoom_float = zoom_float;
                        mark_event = false;
                    }
                }
                if (resume_save == true)
                {
                    for (int ai = Data_to_file.Count + 1; ai <= sample_counter;)
                    {
                        int res_byte = 0;
                        if (byte_to_file[2] < 128)
                        {
                            res_byte = byte_to_file[2] - 102;
                        }
                        else if (byte_to_file[2] >= 128)
                        {
                            res_byte = byte_to_file[2] - 255 - 102;
                        }
                        string str_e2 = "255, " + /*Number_of_frame[ai - 1]*/ (ai - 1).ToString() + ", " + res_byte.ToString();//Value_of_frame[ai - 1];
                        Data_to_file.Add(ai - 1, str_e2);
                        ai++;
                    }
                    resume_save = false;
                }

                if (complete_the_implementation_graph_thread == true)
                {
                    graph_thread_ower = true;
                    complete_the_implementation_graph_thread = false;
                }
                _graph = false;
            }
        }

        void Graph_Thread_2()
        {
            if (_graph_2 == true)
            {
                Pen r_pen2 = new Pen(Color.Red, 0.05f);
                Pen mpen_2 = new Pen(Color.FromArgb(255, 21, 32, 76), graph_rate);

                int counter;

                f2.Clear(Color.White);

                _double2 = 799.0f;
                priv_double2 = 799.0f;

                if (mode1 == true)
                {
                    counter = sample_counter;
                    counter_for_graph2 = sample_counter;
                }
                else
                {
                    counter = counter_for_graph2;
                }

                for (int i = counter; i >= 1;)
                {
                    _double2 = _double2 - (graph_rate * zoom_float / 15);
                    if (_double2 >= 45)
                    {
                        if (mode1 == false)
                        {
                            if (red_point == 1 || red_point == 2)
                            {
                                if (mark_index1 == i)
                                {
                                    f2.DrawLine(r_pen2, _double2, 10, _double2, 100);
                                    f2.DrawString(first_line_value.ToString(), new Font("Serif", 6), new SolidBrush(Color.Red), _double2 - 17, 115);
                                }
                                if (red_point == 2)
                                {
                                    if (mark_index2 == i)
                                    {
                                        f2.DrawLine(r_pen2, _double2, 10, _double2, 100);
                                        f2.DrawString(second_line_value.ToString(), new Font("Serif", 6), new SolidBrush(Color.Red), _double2 - 17, 115);
                                        f2.DrawString(Math.Abs(first_line_value - second_line_value).ToString(), new Font("Serif", 6), new SolidBrush(Color.Red), _double2 - 15, 1);
                                    }
                                }
                            }
                        }

                        if (i > 0)
                        {
                            if (Value_of_frame[i - 1] < 128)
                            {
                                y_float2 = (Value_of_frame[i - 1] - 102) + 160;
                            }
                            else if (Value_of_frame[i - 1] >= 128)
                            {
                                y_float2 = (Value_of_frame[i - 1] - 255 - 102) + 160;
                            }
                            f2.DrawLine(mpen_2, priv_double2, priv_y_float2, priv_double2, y_float2);
                            f2.DrawLine(mpen_2, priv_double2, y_float2, _double2, y_float2);
                            priv_y_float2 = y_float2;
                        }
                    }
                    else
                    {
                        _double2 = 45;

                        if (i > 0)
                        {
                            if (Value_of_frame[i - 1] < 128)
                            {
                                y_float2 = (Value_of_frame[i - 1] - 102) + 160;
                            }
                            else if (Value_of_frame[i - 1] >= 128)
                            {
                                y_float2 = (Value_of_frame[i - 1] - 255 - 102) + 160;
                            }
                            f2.DrawLine(mpen_2, priv_double2, priv_y_float2, priv_double2, y_float2);
                            f2.DrawLine(mpen_2, priv_double2, y_float2, _double2, y_float2);
                        }
                        i = 0;
                    }
                    priv_double2 = _double2;
                    i--;
                }
                f2.DrawLine(mpen_2, 45, 110, 799, 110);
                f2.DrawLine(mpen_2, 45, 15, 45, 110);
                g2.DrawImage(bitmap2, new PointF(0.0f, 0.0f));

                if (complete_the_implementation_graph_thread_2 == true)
                {
                    graph_thread_2_ower = true;
                    complete_the_implementation_graph_thread_2 = false;
                }
                _graph_2 = false;
            }
        }

        void Save_thread()
        {
            if (save_data == true)
            {
                try
                {
                    using (StreamWriter fileWriter = new StreamWriter(pass_for_receiving))
                    {
                        foreach (KeyValuePair<int, string> kvPair in Data_to_file)
                        {
                            if(stop_saving == true)
                            {
                                stop_saving = false;
                                fileWriter.Close();
                            }
                            fileWriter.WriteLine("{0}", kvPair.Value);
                        }
                    }
                    var result = MessageBox.Show("Data saved", "Message",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Question);
                    if (result == DialogResult.OK)
                    {
                    }
                }
                catch (Exception e)
                {
                    var result = MessageBox.Show(e.Message, "Error",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Question);
                    if (result == DialogResult.OK)
                    {
                    }
                }
                save_data = false;
            }

            if (complete_the_implementation_saving == true)
            {
                saving_ower = true;
                complete_the_implementation_saving = false;
            }
        }

        private void panel2_Click(object sender, EventArgs e)
        {
            Hide_port_baudrate_Funct();
        }

        private void panel1_Click(object sender, EventArgs e)
        {
            Hide_port_baudrate_Funct();
        }

        private void panel3_Click(object sender, EventArgs e)
        {
            Hide_port_baudrate_Funct();
        }

        private void panel4_Click(object sender, EventArgs e)
        {
            Hide_port_baudrate_Funct();
        }

        private void panel5_Click(object sender, EventArgs e)
        {
            Hide_port_baudrate_Funct();
        }

        private void panel9_Click(object sender, EventArgs e)
        {
            Hide_port_baudrate_Funct();
        }

        public void panel7_Click(object sender, EventArgs e)
        {
            Hide_port_baudrate_Funct();

            Point point = panel7.PointToClient(Cursor.Position);
            if (mode1 == false && set_point == true)
            {
                if (red_point == 0)
                {
                    if (point.Y <= 335 && point.Y >= 5)
                    {
                        if (point.X <= 799 && point.X >= 45)
                        {
                            int result = 799 - point.X;
                            int mark_counter = (int)(result / (graph_rate * zoom_float));
                            if(counter_for_mark >= mark_counter)
                            {
                                mark_index1 = counter_for_mark - mark_counter;
                                first_line_value = (float)mark_index1 / radio_rate;
                                mark_event = true;
                                if (start_graph == true)
                                {
                                    if (_graph == false)
                                    {
                                        _graph = true;
                                        Start_graph_function();
                                    }
                                    if (_graph_2 == false)
                                    {
                                        _graph_2 = true;
                                        Start_graph_2_function();
                                    }
                                }
                                red_point = 1;
                            }
                        }
                    }
                }
                else if (red_point == 1)
                {
                    if (point.Y <= 335 && point.Y >= 5)
                    {
                        if (point.X <= 799 && point.X >= 45)
                        {
                            int result2 = 799 - point.X;
                            int mark_counter2 = (int)(result2 / (graph_rate * zoom_float));
                            if(counter_for_mark>= mark_counter2)
                            {
                                mark_index2 = counter_for_mark - mark_counter2;
                                second_line_value = (float)mark_index2 / radio_rate;
                                mark_event = true;
                                if (start_graph == true)
                                {
                                    if (_graph == false)
                                    {
                                        _graph = true;
                                        Start_graph_function();
                                    }
                                    if (_graph_2 == false)
                                    {
                                        _graph_2 = true;
                                        Start_graph_2_function();
                                    }
                                }
                                red_point = 2;
                            }
                        }
                    }
                }
            }
        }

        private void panel7_MouseMove(object sender, MouseEventArgs e)
        {
            if (mode1 == false)
            {
                lkm = panel7.PointToClient(Cursor.Position);
                if (lkm.X - orig.X > 5)
                {
                    if (zooom_rect == true && lkm.X < 799 && lkm.X > 45)
                    {
                        set_point = false;
                        Pen pen_rect = new Pen(Color.FromArgb(150, 21, 32, 76), 0.5f);
                        Draw_Graph(list_counter);
                        rect = new Rectangle(orig.X, orig.Y, lkm.X - orig.X, lkm.Y - orig.Y);
                        g.DrawRectangle(pen_rect, rect);
                    }
                }
                else if (orig.X - lkm.X > 5)
                {
                    if (zooom_rect == true && lkm.X < 799 && lkm.X > 45)
                    {
                        set_point = false;
                        Pen pen_rect = new Pen(Color.FromArgb(150, 21, 32, 76), 0.5f);
                        Draw_Graph(list_counter);
                        rect = new Rectangle(lkm.X, lkm.Y, orig.X - lkm.X, orig.Y - lkm.Y);
                        g.DrawRectangle(pen_rect, rect);
                    }
                }
            }
        }

        private void panel7_MouseDown(object sender, MouseEventArgs e)
        {
            if (mode1 == false)
            {
                orig = panel7.PointToClient(Cursor.Position);
                if (orig.X < 799 && orig.X > 45)
                {
                    zooom_rect = true;
                }
                else
                {
                    zooom_rect = false;
                }
            }
        }

        private void panel7_MouseUp(object sender, MouseEventArgs e)
        {
            if (mode1 == false)
            {
                zooom_rect = false;
                if (set_point == false)
                {
                    if (lkm.X > orig.X + 5)
                    {
                        mouseup_value = counter_for_mark - (int)((799 - lkm.X) / (graph_rate * zoom_float));
                        mouseup = true;
                        zoom_float = zoom_float * ((float)754 / Math.Abs(rect.Width));
                        textBox4.Text = (zoom_float * 100).ToString();
                        Scale_graph_function();
                        if (zoom_float >= 1)
                        {
                            if (((int)(zoom_float / 0.1f) + 190) < vScrollBar1.Maximum)
                            {
                                vScrollBar1.Value = (int)(zoom_float / 0.1f) + 190;
                            }
                            else
                            {
                                vScrollBar1.Value = vScrollBar1.Maximum;
                            }
                        }
                        else
                        {
                            vScrollBar1.Value = (int)(zoom_float / 0.005f);
                        }
                        set_point = true;
                    }
                    else if (orig.X > lkm.X + 5)
                    {
                        mouseup_value = counter_for_mark - (int)((799 - orig.X) / (graph_rate * zoom_float));
                        mouseup = true;
                        zoom_float = zoom_float * ((float)754 / Math.Abs(rect.Width));
                        textBox4.Text = (zoom_float * 100).ToString();
                        Scale_graph_function();
                        if (zoom_float >= 1)
                        {
                            if (((int)(zoom_float / 0.1f) + 190) < vScrollBar1.Maximum)
                            {
                                vScrollBar1.Value = (int)(zoom_float / 0.1f) + 190;
                            }
                            else
                            {
                                vScrollBar1.Value = vScrollBar1.Maximum;
                            }
                        }
                        else
                        {
                            vScrollBar1.Value = (int)(zoom_float / 0.005f);
                        }
                        set_point = true;
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (red_point == 2 || red_point == 1)
            {
                red_point = 0;
                mark_event = true;
                if (start_graph == true)
                {
                    if (_graph == false)
                    {
                        _graph = true;
                        Start_graph_function();
                    }
                    if (_graph_2 == false)
                    {
                        _graph_2 = true;
                        Start_graph_2_function();
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            bool priv_start_graph = false;
            bool priv_start_read = false;
            annulment = true;
            if (save_data == true)
            {
                complete_the_implementation_saving = true;
                save_data = false;
                if (saving_ower == true)
                {
                    annulment = true;
                    saving_ower = false;
                }
                else
                {
                    var result = MessageBox.Show("Data is stored, whether you want to stop saving", "Error",
                                                 MessageBoxButtons.YesNo,
                                                 MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        stop_saving = true;
                        //Thread.Sleep(20);
                        annulment = true;
                        saving_ower = true;
                    }
                    else
                    {
                        annulment = false;
                        complete_the_implementation_saving = false;
                        saving_ower = false;
                    }
                }
            }

            if(annulment == true)
            {
                if (not_connected == false)
                {
                    not_connected = true;
                    panel6.Visible = false;
                    _continue = false;
                    _serialPort.Close();
                    readThread[0].Join();
                    portstat_str = "closed";
                    PortStatus_Funct();
                    priv_start_read = true;
                }

                if (start_graph == true)
                {
                    _graph = false;
                    _graph_2 = false;
                    complete_the_implementation_graph_thread = true;
                    complete_the_implementation_graph_thread_2 = true;
                    readThread[1].Abort();
                    readThread[3].Abort();
                    Start_graph_button.Visible = true;
                    Stop_button.Visible = false;
                    start_graph = false;
                    priv_start_graph = true;
                    /*if (graph_thread_ower = true && graph_thread_2_ower == true)
                    {
                        Start_graph_button.Visible = true;
                        Stop_button.Visible = false;
                        start_graph = false;
                    }
                    else
                    {
                        
                        var result = MessageBox.Show("Wait a few seconds and press 'Ok'", "Error",
                                                     MessageBoxButtons.OK,
                                                     MessageBoxIcon.Question);

                        if (result == DialogResult.OK)
                        {
                            readThread[1].Abort();
                            readThread[3].Abort();
                            Start_graph_button.Visible = true;
                            Stop_button.Visible = false;
                            start_graph = false;
                        }
                    }*/
                }

                sample_counter = 0;
                stop_byte_counter = 0;
                counter_for_graph2 = 0;
                Number_of_frame.Clear();
                Value_of_frame.Clear();
                Data_to_file.Clear();
                red_point = 0;
                _3bytes = 4;
                priv_2_byte = 100;
                error_counter = 0;
                error_counter2 = 0;
                clear_port_buffer = true;
                mark_event = true;
                prog_start = true;
                graph_thread_ower = false;
                graph_thread_2_ower = false;

                if (priv_start_read == true)
                {
                    not_connected = false;
                    panel6.Visible = true;
                    readThread[0] = new Thread(Read_Bytes_from_COM);
                    _serialPort.ReadTimeout = 500;
                    _serialPort.WriteTimeout = 500;
                    portstat_str = "open";
                    PortStatus_Funct();

                    try
                    {
                        _serialPort.Open();
                        _continue = true;
                        readThread[0].Start();
                    }
                    catch (Exception b)
                    {
                        var result = MessageBox.Show(b.Message, "Error",
                                                     MessageBoxButtons.OK,
                                                     MessageBoxIcon.Question);

                        if (result == DialogResult.OK)
                        {
                            if (not_connected == false)
                            {
                                not_connected = true;
                                panel6.Visible = false;
                                _continue = false;
                                _serialPort.Close();
                                portstat_str = "closed";
                                PortStatus_Funct();
                            }
                        }
                    }

                }
                if (priv_start_graph == true)
                {
                    if (start_graph == false)
                    {
                        readThread[1] = new Thread(Graph_Thread);
                        readThread[3] = new Thread(Graph_Thread_2);
                        _graph = true;
                        _graph_2 = true;
                        readThread[1].Start();
                        readThread[3].Start();
                        start_graph = true;
                        Stop_button.Visible = true;
                        Start_graph_button.Visible = false;
                    }
                }
            }
        }

        private void vScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            int v_scroll_value = vScrollBar1.Value;
            if (v_scroll_value < 200)
            {
                zoom_float = v_scroll_value * 0.005f;
                textBox4.Text = (zoom_float * 100).ToString() + "%";
                Scale_graph_function();
            }
            else
            {
                if (vScrollBar1.Value < vScrollBar1.Maximum)
                {
                    zoom_float = (v_scroll_value - 190) * 0.1f;
                    textBox4.Text = (zoom_float * 100).ToString() + "%";
                    Scale_graph_function();
                } 
            }
        }

        private void panel9_MouseDown(object sender, MouseEventArgs e)
        {
            if (mode1 == false)
            {
                p9_curs_point = panel9.PointToClient(Cursor.Position);
                p9_scroll = true;
            }
        }

        private void panel9_MouseUp(object sender, MouseEventArgs e)
        {
            p9_scroll = false;

            if(m_scroll == false)
            {
                if (p9_curs_point.X > 45 && p9_curs_point.X < 799)
                {
                    int result_val = counter_for_graph2 - (int)((799 - p9_curs_point.X) / (graph_rate * zoom_float / 15)) + (int)(377 / (graph_rate * zoom_float));
                    if (result_val > 0)
                    {
                        if (result_val <= hScrollBar1.Maximum)
                        {
                            hScrollBar1.Value = result_val;
                        }
                        else
                        {
                            hScrollBar1.Value = hScrollBar1.Maximum;
                        }
                    }
                }
            }
            m_scroll = false;
        }

        private void panel9_MouseMove(object sender, MouseEventArgs e)
        {
            Point lkm_p9 = panel9.PointToClient(Cursor.Position);
            int counter2 = counter_for_graph2;
            if (p9_scroll == true)
            {
                if((lkm_p9.X - p9_curs_point.X) > 3)
                {
                    m_scroll = true;
                    float koakj = (lkm_p9.X - p9_curs_point.X) / (graph_rate * zoom_float/25);
                    counter_for_graph2 = counter2 - (int)koakj;
                    if (start_graph == true)
                    {
                        if (_graph_2 == false)
                        {
                            _graph_2 = true;
                            Start_graph_2_function();
                        }
                    }
                    p9_curs_point.X = lkm_p9.X;
                }
                else if((p9_curs_point.X - lkm_p9.X) > 3)
                {
                    m_scroll = true;
                    float kjnjml = (p9_curs_point.X - lkm_p9.X) / (graph_rate * zoom_float/25);
                    counter_for_graph2 = counter2 + (int)kjnjml;
                    if (start_graph == true)
                    {
                        if (_graph_2 == false)
                        {
                            _graph_2 = true;
                            Start_graph_2_function();
                        }
                    }
                    p9_curs_point.X = lkm_p9.X;
                }

                if (counter_for_graph2 > stop_byte_counter2)
                {
                    counter_for_graph2 = stop_byte_counter2;
                    if(start_graph == true)
                    {
                        if (_graph_2 == false)
                        {
                            _graph_2 = true;
                            Start_graph_2_function();
                        }
                    }
                }
                else
                {
                    if (counter_for_graph2 < ((int)(754 / (graph_rate * zoom_float / 15))))
                    {
                        if (stop_byte_counter2 < ((int)(754 / (graph_rate * zoom_float / 15))))
                        {
                            counter_for_graph2 = stop_byte_counter2;
                            if (start_graph == true)
                            {
                                if (_graph_2 == false)
                                {
                                    _graph_2 = true;
                                    Start_graph_2_function();
                                }
                            }
                        }
                        else
                        {
                            counter_for_graph2 = (int)(754 / (graph_rate * zoom_float / 15));
                            if (start_graph == true)
                            {
                                if (_graph_2 == false)
                                {
                                    _graph_2 = true;
                                    Start_graph_2_function();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void hScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            if (start_graph == true)
            {
                if (mode1 == false)
                {
                    if (_graph == false)
                    {
                        _graph = true;
                        Start_graph_function();
                    }
                }
            }
        }
    }
}