using System.Collections.Generic;
using BLLModels = ItineraryBuilder.Models;

namespace ItineraryBuilder.Repository
{
    public interface IBaseRepository
    {
        //(reason for where T: class) T must be a reference type to use in later generic method calls, i.e. - the SqlMapperExtensions Insert<T>
        T Find<T>(long id) where T : class, BLLModels.IActiveRecord;

        List<T> FindAll<T>() where T : class, BLLModels.IActiveRecord;

        T Insert<T>(T record) where T : class, BLLModels.IActiveRecord;

        bool Update<T>(T record) where T : class, BLLModels.IActiveRecord;
    }
}