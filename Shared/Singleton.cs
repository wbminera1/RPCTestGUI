using System;

namespace Shared
{
    public class Singleton<T> where T : class, new()
    {
        private static volatile T instance;
        private static object syncRoot = new Object();

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new T();
                    }
                }
                return instance;
            }
        }
    }
}
