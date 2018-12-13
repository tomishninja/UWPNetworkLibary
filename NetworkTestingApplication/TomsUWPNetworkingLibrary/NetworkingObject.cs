using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomsUWPNetworkingLibrary
{
    public abstract class NetworkingObject
    {
        private List<KeyValuePair<DateTime, string>> messagesRecived = new List<KeyValuePair<DateTime, string>>();

        public KeyValuePair<DateTime, string>[] MessageRecivedLog { get => messagesRecived.ToArray(); }
        
        private List<KeyValuePair<DateTime, string>> degbugLogMessages = new List<KeyValuePair<DateTime, string>>();

        public KeyValuePair<DateTime, string>[] DegbugLogMessages { get => degbugLogMessages.ToArray(); }

        public KeyValuePair<DateTime, string>[] generateDebugLog()
        {
            try
            {
                if (degbugLogMessages.Count == 0)
                {
                    return new KeyValuePair<DateTime, string>[] { new KeyValuePair<DateTime, string>(DateTime.Now, "Empty") };
                }
                else
                {
                    return degbugLogMessages.ToArray();
                }
            }
            catch (Exception ex)
            {
                return new KeyValuePair<DateTime, string>[] { new KeyValuePair<DateTime, string>(DateTime.Now, "Error") };
            }
            
        }

        public void EmptyMessageLog()
        {
            messagesRecived = new List<KeyValuePair<DateTime, string>>();
        }

        public void AddEntryToMessageLog(string entry)
        {
            messagesRecived.Add(new KeyValuePair<DateTime, string>(DateTime.Now, entry));
        }

        public void AddEntryToDebugLog(string entry)
        {
            degbugLogMessages.Add(new KeyValuePair<DateTime, string>(DateTime.Now, entry));
        }
    }
}
