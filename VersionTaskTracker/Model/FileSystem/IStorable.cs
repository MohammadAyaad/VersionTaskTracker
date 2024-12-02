using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersionTaskTracker.Model.FileSystem;

public interface IStorable<TSelf> where TSelf : IStorable<TSelf>
{
    public void Load();
    public void Save();
}
