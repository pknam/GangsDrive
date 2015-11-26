using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GangsDrive.util
{
    class DriverError
    {
        /// <summary>
        /// util method for debugging message
        /// </summary>
        /// <param name="ex">exception object</param>
        /// <param name="context">driver name</param>
        /// <param name="mount_state">mount state</param>
        public static void DebugError(Exception ex, string context, bool mount_state)
        {
            System.Windows.Forms.MessageBox.Show(ex.StackTrace);
        }
    }
}
