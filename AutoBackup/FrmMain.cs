using System;
using System.ComponentModel;
using System.Drawing;
using NEO.Helper;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Threading;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace AutoBackup
{
    public partial class FrmMain : Form
    {
        private string dbname;
        private string bkpath;
        private string connstring;

        public FrmMain()
        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            try
            {
                this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - base.Width, Screen.PrimaryScreen.WorkingArea.Height - base.Height);
                this.progressBar1.Style = ProgressBarStyle.Marquee;

                string[] passedInArgs = Environment.GetCommandLineArgs();

                textBox1.Visible = false;
                textBox2.Visible = false;
                label1.Visible = false;
                label4.Visible = false;
                BtnCancel.Visible = false;
                BtnForceClose.Visible = true;

                if (passedInArgs != null)
                {
                    if (!Utils.IsRunningUnderIDE())
                    {
                        textBox1.Text = passedInArgs[1];
                        textBox2.Text = passedInArgs[2];
                        bkpath = textBox1.Text;
                        connstring = passedInArgs[2];
                        dbname = Between(connstring, "database=", ";", 0).ToUpper();
                    }

                    BtnCancel.Visible = true;
                    BtnForceClose.Visible = false;
                }

                if (!Utils.IsUnderDevelopment())
                {
                    //BackupBackgroundWorker();
                    this.backgroundWorker1.RunWorkerAsync();
                }
            } catch (Exception ex)
            {
                Class.Logger.LogError("Backup DB Failure with message: " + ex.Message.ToString());
            }
        }

        public void Backup()
        {
            try
            {
                string DbFile = dbname + "_BACKUP_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".sql";
                string dbfullpath = this.bkpath + DbFile;
                connstring += "charset=utf8;convertzerodatetime=true;";
                using (MySqlConnection conn = new MySqlConnection(connstring))
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        using (MySqlBackup mb = new MySqlBackup(cmd))
                        {
                            cmd.Connection = conn;
                            conn.Open();
                            mb.ExportToFile(dbfullpath);
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Class.Logger.LogError("Backup DB Failure with message: " + ex.Message.ToString());
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            bool isBusy = this.backgroundWorker1.IsBusy;
            if (isBusy)
            {
                this.backgroundWorker1.CancelAsync();
                base.Dispose();
                Environment.Exit(1);
                Application.Exit();
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.progressBar1.Style = ProgressBarStyle.Blocks;
            base.Dispose();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(5000);
            this.Backup();
            bool cancellationPending = this.backgroundWorker1.CancellationPending;
            if (cancellationPending)
            {
                e.Cancel = true;
            }
        }

        public static string Between(string data, string kiri, string kanan, int max = 0)
        {
            if (data.Length == 0)
            {
                return string.Empty;
            }
            int num = 0;
            if (kiri == null)
            {
                kiri = string.Empty;
            }
            if (kanan == null)
            {
                kanan = string.Empty;
            }
            string text = data.ToLower();
            kiri = kiri.ToLower();
            kanan = kanan.ToLower();
            if (kiri.IndexOf("{}") >= 0)
            {
                kanan = kiri.Split(new char[]
                {
                    '}'
                })[1];
                kiri = kiri.Split(new char[]
                {
                    '{'
                })[0];
            }
            if (kiri.Length > 0)
            {
                num = text.IndexOf(kiri);
                if (num < 0)
                {
                    return string.Empty;
                }
                num += kiri.Length;
            }
            if (kanan.Length <= 0)
            {
                string text2 = data.Substring(num);
                if (max > 0 && text2.Length > max)
                {
                    text2 = text2.Substring(0, max);
                }
                return text2;
            }
            int num2 = text.IndexOf(kanan, num);
            if (num2 < 0)
            {
                return string.Empty;
            }
            num2 -= num;
            if (max > 0)
            {
                string text3 = data.Substring(num, num2);
                if (text3.Length > max)
                {
                    text3 = text3.Substring(0, max);
                }
                return text3;
            }
            return data.Substring(num, num2);
        }

        private void BtnForceClose_Click(object sender, EventArgs e)
        {
            base.Dispose();
            Environment.Exit(1);
            Application.Exit();
        }
    }
}
