using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoNotifyPropertyChanged.Test
{
    public class MyModifiedTestClass : ModelBase
    {
        public virtual string Name { get; set; }
        public virtual string Surname { get; set; }
        public virtual int Age { get; set; }
    }
}