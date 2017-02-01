﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace Utils
{
    public static class Logs
    {
        private static LogsForm logs;

        public static void addLog(string log_subject, string log, bool time, int flag, bool anotherThread = false)
        {
            if(logs == null)
            {
                logs = new LogsForm();
                logs.Name = log_subject;
                logs.Show();
            }
            ListViewItem item = new ListViewItem();
            item.ForeColor = int_get_log_color(flag);
            item.Text = int_get_log_message(time, log_subject + " " + log);

            int_add_log_to_ui(anotherThread, item);
            int_write_log_to_file(int_get_log_message(time, log));
        }

        public static int getLogsCounter()
        {
            return logs.logsListView.Items.Count;
        }

        private static void int_write_log_to_file(string log_line)
        {
           // using (System.IO.StreamWriter file = new StreamWriter(@"Logs\" + voterName + ".txt", true))
           // {
           //     file.Write(log_line + Environment.NewLine);
           // }

        }

        private static Color int_get_log_color(int flag)
        {
            Color r = Color.White;
            switch (flag)
            {
                case 0:
                    r = Color.Blue;
                    break;
                case 1:
                    r = Color.Black;
                    break;
                case 2:
                    r = Color.Red;
                    break;
                case 3:
                    r = Color.Green;
                    break;
            }

            return r;
        }

        private static string int_get_log_with_time(string log)
        {
            return int_get_log_message(true, log);
        }

        private static string int_get_log_message(bool time, string log)
        {
            string msg = "";
            if (time)
            {
                msg += "[" + DateTime.Now.ToString("HH:mm:ss") + "] ";
            }

            return msg += log;
        }

        private static void int_add_log_to_ui(bool anotherThread, ListViewItem item)
        {
            if (!anotherThread)
            {
                logs.logsListView.Items.Add(item);
                logs.logsListView.Items[logs.logsListView.Items.Count - 1].EnsureVisible();
            }
            else
            {
                logs.logsListView.Invoke(new MethodInvoker(delegate ()
                {
                    logs.logsListView.Items.Add(item);
                    logs.logsListView.Items[logs.logsListView.Items.Count - 1].EnsureVisible();
                })
                );
            }
        }
    }
}
