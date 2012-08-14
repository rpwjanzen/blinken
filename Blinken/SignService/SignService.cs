using System.ServiceModel;
using System.Threading;
using Blinken;
using Blinken.Font;

namespace SignService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    internal sealed class SignService : ISignService
    {
        private readonly LcdNotifier m_lcdNotifier;

        public SignService()
        {
            m_lcdNotifier = new LcdNotifier();
            m_lcdNotifier.Text = "GTG!";

            LedFont font = LedFont.LoadFromFile(@"Font\MyTiny.txt");

            Thread thread = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    lock (m_lcdNotifier)
                        m_lcdNotifier.DrawText(font);

                    System.Threading.Thread.Sleep(1000);
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
