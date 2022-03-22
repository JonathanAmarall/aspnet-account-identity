﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Identity.Extensions
{
    [ApiController]
    public abstract class MainController : ControllerBase
    {
        protected ICollection<string> Errors = new List<string>();

        protected ActionResult CustomReponse(object result = null)
        {
            if (OperationValid())
            {
                return Ok(result);
            }

            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                {"Messages", Errors.ToArray() }
            }));
        }

        protected ActionResult CustomReponse(ModelStateDictionary modelState)
        {
            var errors = modelState.Values.SelectMany(e => e.Errors);

            foreach(var error in errors)
            {
                AddProcessingError(error.ErrorMessage);
            }

            return CustomReponse();
        }

        protected void AddProcessingError(string error)
        {
            Errors.Add(error);
        }

        protected void ClearProcessingErrors()
        {
            Errors.Clear();
        }

        protected bool OperationValid()
        {
            return !Errors.Any();    
        }
    }
}
