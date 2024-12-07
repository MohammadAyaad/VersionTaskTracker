using C5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VersionTaskTracker.Services;
using UtilitiesX.Extensions;
using VersionTaskTracker.Model.Tracking;
using GlobExpressions;
using System.Runtime.CompilerServices;
using UtilitiesX;
using VersionTaskTracker.Context;
using VersionTaskTracker.Model.FileSystem;

namespace VersionTaskTracker.Model.Tracker
{
    public class VTTInstanceState
    {
        public Component Root;
        public VTTInstanceState(Component root)
        {
            this.Root = root;
        }
    }
}
