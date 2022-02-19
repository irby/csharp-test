using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Authentication.Core.Exceptions;

namespace Authentication.Core.Utilities
{
    public static class ModelValidator
    {
        public static IList<string> Validate(bool throwOnFail, params object[] models)
        {
            var retList = new List<string>();

            var modelsToValidate = new List<object>();

            // allow collections to be passed in as one of many params
            foreach (var model in models)
            {
                if (model is IEnumerable<object> objects)
                {
                    modelsToValidate.AddRange(objects);
                }
                else
                {
                    modelsToValidate.Add(model);
                }
            }

            foreach (var model in modelsToValidate)
            {
                if (model == null)
                {
                    retList.Add($"Model can not be null");
                    continue;
                }

                var errorList = new List<ValidationResult>();

                Validator.TryValidateObject(model, new ValidationContext(model), errorList, true);

                retList.AddRange(errorList.Select(x => $"{model.GetType().Name}: {x.ErrorMessage}").ToList());
            }

            if (retList.Any() && throwOnFail)
            {
                throw new BadRequestException(string.Join("\n", retList));
            }

            return retList;
        }
    }
}