using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Configurations
{
    public class TaskCompletionReminderWorkerSettings
    {
        public int TimeTaskEmail { get; set; }
        public string ApiUrlTaskEmail { get; set; }
    }
}
