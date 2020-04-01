using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordAuthenticationFlowExample
{
    public class ExepronTask
    {
        public int taskId { get; set; }
        public string taskNumber { get; set; }
        public string taskName { get; set; }
        public int? remainingDuration { get; set; }
    }
}
