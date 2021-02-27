using System;

namespace NEST.ElasticallyQueryable.Internal
{
    public interface ISearchDescriptorAccessor 
    {
        Type SearchDescriptorType { get; }
        object SearchDescriptorObject { get; }
    }
}