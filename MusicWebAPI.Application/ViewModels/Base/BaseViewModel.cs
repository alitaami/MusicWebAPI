using AutoMapper;
using Mappings.CustomMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Application.ViewModels.Base
{ 
        public abstract class BaseViewModel<TModel, TEntity> : IHaveCustomMapping
            where TModel : class, new()
        {
            public TEntity ToEntity(IMapper mapper)
            {
                return mapper.Map<TEntity>(CastToDerivedClass(mapper, this));
            }

            public TEntity ToEntity(IMapper mapper, TEntity entity)
            {
                return mapper.Map(CastToDerivedClass(mapper, this), entity);
            }

            public static TModel FromEntity(IMapper mapper, TEntity model)
            {
                return mapper.Map<TModel>(model);
            }

            protected TModel CastToDerivedClass(IMapper mapper, BaseViewModel<TModel, TEntity> baseInstance)
            {
                return mapper.Map<TModel>(baseInstance);
            }

            public void CreateMappings(Profile profile)
            {
                var mappingExpression = profile.CreateMap<TModel, TEntity>();

                var dtoType = typeof(TModel);
                var entityType = typeof(TEntity);
                //Ignore any property of source (like Post.Author) that dose not contains in destination 
                foreach (var property in entityType.GetProperties())
                {
                    if (dtoType.GetProperty(property.Name) == null)
                        mappingExpression.ForMember(property.Name, opt => opt.Ignore());
                }

                CustomMappings(mappingExpression.ReverseMap());
            }

            public virtual void CustomMappings(IMappingExpression<TEntity, TModel> mapping)
            {
            }
        }
}
