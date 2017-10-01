using System;
using System.Collections.Generic;

namespace Shared
{
    public class GenericFactory<IdType> where IdType : IComparable
    {
        private Dictionary<IdType, Type> m_Types;

        public GenericFactory()
        {
            m_Types = new Dictionary<IdType, Type>();
        }
        public void Register<FactoryType>(IdType id)
        {
            m_Types.Add(id, typeof(FactoryType));
        }
        public FactoryType Create<FactoryType>(IdType id)
        {
            return (FactoryType)Activator.CreateInstance(m_Types[id]);
        }
    }
}
