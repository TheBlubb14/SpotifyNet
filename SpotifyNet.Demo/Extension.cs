using System.Collections.ObjectModel;

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
