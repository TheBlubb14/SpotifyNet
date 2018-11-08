using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyNet.Demo
{
    public static class Extension
    {
        public static ObservableCollection<T> AddRange<T>(this ObservableCollection<T> collection, T[] values)
        {
            foreach (var value in values)
                collection.Add(value);

            return collection;
        }
    }
}
