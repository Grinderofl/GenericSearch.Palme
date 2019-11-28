using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace GenericSearch.UI
{
    public class AbstractSearchModelBinder : IModelBinder
    {
        private readonly IDictionary<Type, ComplexTypeModelBinder> modelBuilderByType;

        private readonly IModelMetadataProvider modelMetadataProvider;

        public AbstractSearchModelBinder(IDictionary<Type, ComplexTypeModelBinder> modelBuilderByType, IModelMetadataProvider modelMetadataProvider)
        {
            this.modelBuilderByType = modelBuilderByType ?? throw new ArgumentNullException(nameof(modelBuilderByType));
            this.modelMetadataProvider = modelMetadataProvider ?? throw new ArgumentNullException(nameof(modelMetadataProvider));
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var modelTypeValue = bindingContext.ValueProvider.GetValue(ModelNames.CreatePropertyModelName(bindingContext.ModelName, "ModelTypeName"));

            if (modelTypeValue != null && modelTypeValue.FirstValue != null)
            {
                var modelType = Type.GetType(modelTypeValue.FirstValue);
                if (modelBuilderByType.TryGetValue(modelType, out var modelBinder))
                {
                    var innerModelBindingContext = DefaultModelBindingContext.CreateBindingContext(
                                                                                                   bindingContext.ActionContext,
                                                                                                   bindingContext.ValueProvider,
                                                                                                   modelMetadataProvider.GetMetadataForType(modelType),
                                                                                                   null,
                                                                                                   bindingContext.ModelName);

                    modelBinder.BindModelAsync(innerModelBindingContext);

                    bindingContext.Result = innerModelBindingContext.Result;
                    return Task.CompletedTask;
                }
            }

            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }
    }
}