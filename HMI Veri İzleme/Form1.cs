using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Net.NetworkInformation;
using System.Data.SqlClient;
using EasyModbus;
using MySql.Data.MySqlClient;

namespace HMI_Veri_İzleme
{
    public partial class Form1 : Form
    {
        private MySqlConnection baglan;

        private string ipadres;
        private int portnumber;
        private int adres1 = 0, adres2 = 0, adres3 = 0, adres4 = 0, adres5 = 0;
        Point lastpoint;
        bool showed = false;
        bool modbus_error = false;
        private void mysqlbaglan()
        {
            if (NetworkInterface.GetIsNetworkAvailable() == true)
                try
                {
                    baglan = new MySqlConnection("Datasource=160.153.157.129; Port=3306;Database=albaelektronik; Uid=melih; Pwd=melih*");
                    baglan.Open();
                    pictureBox5.BackColor = Color.DarkGreen;
                    MySqlCommand kaydet = new MySqlCommand("insert into deneme (firstdata,seconddata,thirddata,fourthdata,fifthdata,date) values (@p1,@p2,@p3,@p4,@p5,@p6)", baglan);
                    kaydet.Parameters.AddWithValue("@p1", adres1.ToString());
                    kaydet.Parameters.AddWithValue("@p2", adres2.ToString());
                    kaydet.Parameters.AddWithValue("@p3", adres3.ToString());
                    kaydet.Parameters.AddWithValue("@p4", adres4.ToString());
                    kaydet.Parameters.AddWithValue("@p5", adres5.ToString());
                    kaydet.Parameters.AddWithValue("@p6", label1.Text + label2.Text);
                    kaydet.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            else
            {
                MessageBox.Show("Check Internet Connection");
            }
        }
        private void modbustcp()
        {
            //this is modbus connectşon part of code
            ipadres = textBox1.Text;
            if (int.TryParse(textBox2.Text, out int a))
                portnumber = Convert.ToInt32(textBox2.Text);
            else
                MessageBox.Show("Port giriş hatası","uyarı",MessageBoxButtons.OK,MessageBoxIcon.Error);
            try
            {
               //kolay gelsin reis 
                ModbusClient modbustcp = new ModbusClient(ipadres, portnumber);
                modbustcp.Connect();
                //MessageBox.Show("sdg");
                pictureBox3.BackColor = Color.DarkGreen;
                int[] readHoldingRegisters1 = modbustcp.ReadHoldingRegisters(Convert.ToInt32(textBox3.Text), 1);
                int[] readHoldingRegisters2 = modbustcp.ReadHoldingRegisters(Convert.ToInt32(textBox4.Text), 1);
                int[] readHoldingRegisters3 = modbustcp.ReadHoldingRegisters(Convert.ToInt32(textBox5.Text), 1);
                int[] readHoldingRegisters4 = modbustcp.ReadHoldingRegisters(Convert.ToInt32(textBox6.Text), 1);
                int[] readHoldingRegisters5 = modbustcp.ReadHoldingRegisters(Convert.ToInt32(textBox7.Text), 1);
                adres1 = readHoldingRegisters1[0];
                adres2 = readHoldingRegisters2[0];
                adres3 = readHoldingRegisters3[0];
                adres4 = readHoldingRegisters4[0];
                adres5 = readHoldingRegisters5[0];
                modbustcp.Disconnect();
                modbus_error = false;
            }
            catch (Exception e)
            {
                modbus_error = true;
                if (!showed)
                {
                    MessageBox.Show(e.Message);
                    showed = true;
                }
            }
        
        }
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Start();
            groupBox1.Visible = false;
            radioButton1.Visible = false;
            textBox1.Visible = textBox2.Visible = label5.Visible = label6.Visible = false;
            textBox3.Visible = textBox4.Visible = textBox5.Visible = textBox6.Visible = textBox7.Visible = false;
            label7.Visible = label8.Visible = label9.Visible = label10.Visible = label11.Visible = false;
            pictureBox3.Visible = pictureBox5.Visible = false;
            button2.Visible = button11.Visible = false;
            button10.Visible = false;
            groupBox2.Visible = radioButton3.Visible = radioButton4.Visible = false;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = DateTime.Now.ToLongDateString();
            label2.Text = DateTime.Now.ToLongTimeString();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable() == true)
            {
                System.Diagnostics.Process.Start("http://albaelektronik.com/");
            }
            else
                MessageBox.Show("İnternet Bağlantısını Kontrol Edip Tekrar Deneyiniz");
        }
        private void button3_Click(object sender, EventArgs e)
        {
            Form2 frm2 = new Form2();
            frm2.Show();
        }
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Visible = textBox2.Visible = label5.Visible = label6.Visible = true;

            button2.Visible = pictureBox3.Visible = true;
        }
        private void button4_Click(object sender, EventArgs e)
        {
            textBox3.Visible = textBox4.Visible = textBox5.Visible = textBox6.Visible = textBox7.Visible = true;
            label7.Visible = label8.Visible = label9.Visible = label10.Visible = label11.Visible = true;
        }
        private void button6_Click(object sender, EventArgs e)
        {
            button11.Visible = pictureBox5.Visible = true;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            timer2.Start();
            button10.Visible = true;
            groupBox2.Visible = radioButton3.Visible = radioButton4.Visible = true;
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
            {
                modbustcp();
            }
            mysqlbaglan();
     
            MySqlDataAdapter da = new MySqlDataAdapter("select * from deneme", baglan);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dataGridView2.DataSource = dt;
            dataGridView2.FirstDisplayedScrollingRowIndex = dataGridView2.RowCount - 1;//en alta gdiyor (BUNLARI KALDIRDIĞIMDA DATAGRW. EN BAŞTAKİ VERİYE GİDİYOR EKLEDİĞİMDE EN SONA)
            dataGridView2[0, dataGridView2.RowCount - 1].Selected = true;
            baglan.Close();
       
        }
        private void button10_Click(object sender, EventArgs e)
        {
            if (dataGridView2.Rows.Count == 0)
            {
                return;
            }
            StringBuilder sb = new StringBuilder();
            // Column headers
            string columnsHeader = "";
            for (int i = 0; i < dataGridView2.Columns.Count; i++)
            {
                columnsHeader += dataGridView2.Columns[i].Name + ";";
            }
            sb.Append(columnsHeader + Environment.NewLine);
            // Go through each cell in the datagridview
            foreach (DataGridViewRow dgvRow in dataGridView2.Rows)
            {
                // Make sure it's not an empty row.
                if (!dgvRow.IsNewRow)
                {
                    for (int c = 0; c < dgvRow.Cells.Count; c++)
                    {
                        // Append the cells data followed by a comma to delimit.
                        sb.Append(dgvRow.Cells[c].Value + ";");
                    }
                    // Add a new line in the text file.
                    sb.Append(Environment.NewLine);
                }
            }
            // Load up the save file dialog with the default option as saving as a .csv file.
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV files (*.csv)|*.csv";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // If they've selected a save location...
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(sfd.FileName, false))
                {
                    // Write the stringbuilder text to the the file.
                    sw.WriteLine(sb.ToString());
                }
            }
            // Confirm to the user it has been completed.
            MessageBox.Show("CSV file saved.");
            dataGridView2.Columns.Clear();
        }
        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left += e.X - lastpoint.X;
                this.Top += e.Y - lastpoint.Y;
            }
        }
        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            lastpoint = new Point(e.X, e.Y);
        }
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left += e.X - lastpoint.X;
                this.Top += e.Y - lastpoint.Y;
            }
        }
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            lastpoint = new Point(e.X, e.Y);

        }
        private void button11_Click(object sender, EventArgs e)
        {
            mysqlbaglan();
        }
        private void button8_Click(object sender, EventArgs e)
        {
            timer2.Stop();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }


        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            timer2.Stop();
            pictureBox3.BackColor = pictureBox5.BackColor = Color.Red;
        }
        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            timer2.Start();
        }
        private void button7_Click(object sender, EventArgs e)
        {
            groupBox1.Visible = true;
            radioButton1.Visible = true;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if ((ipadres == String.Empty) || (textBox2.Text == String.Empty) || (textBox3.Text == String.Empty) || (textBox4.Text == String.Empty) || (textBox5.Text == String.Empty) || (textBox6.Text == String.Empty) || (textBox7.Text == String.Empty))
            {
                MessageBox.Show("IP adresini ,Port numarasını veya Adresleri kontrol ediniz.");
            }
            else
                MessageBox.Show("Bağlantı Sağlandı");
            modbustcp();
            button1.Enabled = true;
        }  
    }
}
