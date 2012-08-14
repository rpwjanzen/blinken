using System.ServiceModel;
using System.Threading;
using Blinken;
using Blinken.Font;
using System.Collections.Generic;

namespace SignService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    internal sealed class SignService : ISignService
    {
        private readonly LcdNotifier m_lcdNotifier;

        public SignService()
        {
            string alphabet = @"ABCDEFGHIJKLMNOPQRSTUVWXYZ abcdefghijklmnopqrstuvwxyz 01234567890 +-=/\|<>,.)(*&^%$#@!~`';:[]{}?";
            m_lcdNotifier = new LcdNotifier();
            

            List<string> paths = new List<string>()
            {
                @"Font\MyTiny.txt",
                @"Font\somybmp01_7.txt",
                @"Font\somybmp02_7.txt",
                @"Font\somybmp04_7.txt",
                @"Font\Tinier.txt",
                @"Font\Tiny.txt",
            };
            List<LedFont> fonts = new List<LedFont>();
            foreach (var path in paths)
            {
                var font = LedFont.LoadFromFile(path);
                fonts.Add(font);
            }

            Thread thread = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    for(int i = 0; i < fonts.Count; i++)
                    {
                        var font = fonts[i];
                        lock (m_lcdNotifier)
                        {
                            m_lcdNotifier.Text = paths[i] + " " + alphabet;
                            m_lcdNotifier.DrawText(font);
                        }
                    }

                }
            }));
            thread.IsBackground = true;
            thread.Start();
        }

        #region ISignService Members

        [OperationBehavior(ReleaseInstanceMode = ReleaseInstanceMode.None)]
        public void SetText(string text)
        {
            var host = OperationContext.Current.Host;

            lock (m_lcdNotifier)
                m_lcdNotifier.Text = (text ?? string.Empty).ToUpper();
        }

        #endregion
    }
}
