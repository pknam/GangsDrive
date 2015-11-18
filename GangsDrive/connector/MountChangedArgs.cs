using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GangsDrive.connector
{
    class MountChangedArgs
    {
        private bool _mountState;

        public bool MountState
        {
            get
            {
                return _mountState;
            }
        }

        public MountChangedArgs(bool mountState)
        {
            _mountState = mountState;
        }
    }
}
