using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoNotifyPropertyChanged.Test
{
    public class MyOriginalTestClass : ModelBase
    {
        private string name { get; set; }
        private string surname { get; set; }
        private int age { get; set; }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        public string Surname
        {
            get
            {
                return surname;
            }
            set
            {
                surname = value;
                OnPropertyChanged("Surname");
            }
        }

        public int Age
        {
            get
            {
                return age;
            }
            set
            {
                age = value;
                OnPropertyChanged("Age");
            }
        }
    }
}